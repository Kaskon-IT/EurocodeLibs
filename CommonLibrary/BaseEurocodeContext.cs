using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CommonLibrary
{
    public abstract class BaseEurocodeContext : IEurocodeContext, IContext, IMarkupConvertible, INotifyPropertyChanged
    {
        protected BaseEurocodeContext()
        {
            Debug.WriteLine($"Nieuw object aangemaakt: {GetType().Name}");
        }


        public Guid Id { get; set; } = Guid.NewGuid();
        public virtual string Heading { get; set; } = "Onbekend";
        public virtual bool ReadOnly { get; set; } = false; // mogelijkheid om de gebruiker alleen te laten lezen.

        public virtual void Init()
        {
            SubscribeAllNestedProperties(this);
            // Trigger een eerste OnUpdated, tenzij een afgeleide dit overschrijft
            OnInitialized();
        }

        /// <summary>
        /// Wordt standaard opgeroepen aan het einde van Init.
        /// Afgeleiden kunnen overrideen als ze initieel andere logica willen.
        /// </summary>
        protected virtual void OnInitialized()
        {
            OnUpdated?.Invoke();
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
            //Console.WriteLine($"{GetType().Name}: {propertyName} changed ({DateTime.Now})");
#endif
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            var oldValue = field;
            field = value;
            OnPropertyChanged(propertyName);

#if DEBUG
            //var ust = value?.GetType().UnderlyingSystemType.ToString() ?? "onbekend";
            //Console.WriteLine($"SetProperty<{ust}> uit {GetType().Name}: {propertyName} changed from {oldValue} to {value}");
#endif

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
            // Filter ruis van ObservableCollection
            if (e.PropertyName is "Count" or "Item[]")
                return;


            // Bubbel property change omhoog
            OnPropertyChanged(e.PropertyName);

            // voorkomen dat BerekenEnValideer zichzelf triggert
            if (_isCalculating)
                return;

            DebounceBerekening(() =>
            {
                try
                {
                    _isCalculating = true;
                    BerekenEnValideer();
                }
                finally
                {
                    _isCalculating = false;
                }
            }, delayMs: 200);
        }

        private CancellationTokenSource? _debounceToken;

        private void DebounceBerekening(Action action, int delayMs = 100)
        {
            _debounceToken?.Cancel();
            _debounceToken = new CancellationTokenSource();
            var token = _debounceToken.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token);
                    if (!token.IsCancellationRequested)
                        action();
                }
                catch (TaskCanceledException)
                {
                    // genegeerd, nieuwe update kwam sneller binnen
                }
            });
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

        public void AddMelding(Melding? melding)
        {
            if (melding is null)
                return;

            // Zorg dat de collectie bestaat
            Meldingen ??= new ObservableCollection<Melding>();

            // Verwijder per ongeluk toegevoegde null-items
            for (int i = Meldingen.Count - 1; i >= 0; i--)
            {
                if (Meldingen[i] is null)
                    Meldingen.RemoveAt(i);
            }

            // Controleer of er al een melding met dezelfde code of bericht bestaat
            bool bestaatAl = Meldingen.Any(m =>
                m is not null &&
                ((m.Code.HasValue && melding.Code.HasValue && m.Code == melding.Code) ||
                 (!string.IsNullOrEmpty(m.Bericht) &&
                  m.Bericht == melding.Bericht)));

            if (!bestaatAl)
            {
                Meldingen.Add(melding);
            }
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
