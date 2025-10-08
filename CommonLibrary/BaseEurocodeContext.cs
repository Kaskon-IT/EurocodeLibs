using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CommonLibrary
{
    public abstract class BaseEurocodeContext : IEurocodeContext, IContext, IMarkupConvertible, INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public virtual string Heading { get; set; } = "Onbekend";

        public virtual void Init()
        {
            SubscribeAllNestedProperties(this);
        }

        public DateTime AangemaaktOp { get; private set; } = DateTime.UtcNow;
        public DateTime GewijzigdOp { get; set; } = DateTime.UtcNow;

        public ObservableCollection<Melding> Meldingen { get; private set; } = [];
        public ObservableCollection<int> MeldingCodes { get; private set; } = [];

        public event Action? OnUpdated;
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly HashSet<object> _visited = new();

        protected bool SetAndRecalculate<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                BerekenEnValideer();
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public void UpdateGewijzigdOp() => GewijzigdOp = DateTime.UtcNow;

        protected void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnUpdated?.Invoke();
#if DEBUG
            Console.WriteLine($"{GetType().Name}: {propertyName} changed");
#endif
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Voor nested INotifyPropertyChanged properties
        protected bool SetNestedProperty<T>(ref T? field, T? value, [CallerMemberName] string? propertyName = null)
            where T : class, INotifyPropertyChanged
        {
            if (ReferenceEquals(field, value)) return false;

            // oude handler loskoppelen
            if (field != null)
                field.PropertyChanged -= NestedPropertyChanged;

            field = value;

            // nieuwe handler koppelen
            if (field != null)
                field.PropertyChanged += NestedPropertyChanged;

            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _isCalculating;

        private void NestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Bubbel property change omhoog
            OnPropertyChanged(e.PropertyName);

            // voorkomen dat BerekenEnValideer zichzelf triggert
            if (_isCalculating)
                return;

            try
            {
                _isCalculating = true;
                BerekenEnValideer();
            }
            finally
            {
                _isCalculating = false;
            }
        }










        // 🔥 Nieuwe generieke deep-subscribe
        protected void SubscribeAllNestedProperties(object? obj = null)
        {
            obj ??= this;

            if (obj == null || !_visited.Add(obj))
                return;

            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);
                if (value is null) continue;

                // 1. Enkelvoudige child
                if (value is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= NestedPropertyChanged;
                    inpc.PropertyChanged += NestedPropertyChanged;

                    SubscribeAllNestedProperties(value);
                }
                // 2. Collecties met children
                else if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is INotifyPropertyChanged itemInpc)
                        {
                            itemInpc.PropertyChanged -= NestedPropertyChanged;
                            itemInpc.PropertyChanged += NestedPropertyChanged;

                            SubscribeAllNestedProperties(itemInpc);
                        }
                    }

                    if (value is INotifyCollectionChanged collChanged)
                    {
                        collChanged.CollectionChanged -= NestedCollectionChanged;
                        collChanged.CollectionChanged += NestedCollectionChanged;
                    }
                }
            }
        }






        private void NestedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += NestedPropertyChanged;
                    SubscribeAllNestedProperties(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= NestedPropertyChanged;
                }
            }

            BerekenEnValideer();
            OnPropertyChanged(null);
        }

        public bool IsValidated { get; private set; }

        public bool HeeftWaarschuwing() => Meldingen.Any(m => m.Type == MeldingType.Waarschuwing);

        public bool BerekenEnValideer()
        {
            ClearMeldingen();
            Bereken();
            GewijzigdOp = DateTime.UtcNow;
            IsValidated = Valideer();
            OnUpdated?.Invoke();
            return IsValidated;
        }

        protected abstract void Bereken();
        protected abstract bool Valideer();

        public void AddMelding(Melding melding)
        {
            if (!Meldingen.Any(m => m.Bericht == melding.Bericht))
                Meldingen.Add(melding);
        }

        public void AddMelding(int code)
        {
            if (MeldingCodes.Contains(code))
                return;

            MeldingCodes.Add(code);
            var melding = CommonLibrary.Helpers.MeldingenBetonHelper.GetMelding(code);
            AddMelding(melding);
#if DEBUG
            Console.WriteLine($"{melding}");
#endif
        }

        public void ClearMeldingen()
        {
            Meldingen.Clear();
            MeldingCodes.Clear();
        }

        public void AddMeldingWaarschuwing(string tekst) => Meldingen.Add(new(MeldingType.Waarschuwing, tekst));
        public void AddMeldingOpmerking(string tekst) => Meldingen.Add(new(MeldingType.Opmerking, tekst));

        public MarkupString ToMarkupString() => Helpers.MarkupHelper.ToMarkupString(ToString());
        public MarkupString ToMarkupString(bool withUnityCheck) => Helpers.MarkupHelper.ToMarkupString(ToString(), withUnityCheck);
    }
}
