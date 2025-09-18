using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;



namespace CommonLibrary
{
    /// <summary>
    /// Basis voor alle eurocode context, waarbij validatie en melding noodzakelijk is.
    /// Een lijst met meldingen is beschikbaar. 
    /// (bijvoorbeeld "Waarschuwing - Overschrijding hoogte drukzone" of
    /// neutrale melding "Minimale wapening toegepast conform artikel 80.80")
    /// </summary>
    public abstract class BaseEurocodeContext : IEurocodeContext, IContext, IMarkupConvertible, INotifyPropertyChanged
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public virtual string Heading { get; set; } = "Onbekend";

        public virtual void Init() { } // Init methode om de context te initialiseren, kan overschreven worden in child-classes

        public DateTime AangemaaktOp { get; private set; } = DateTime.UtcNow;

        public DateTime GewijzigdOp { get; set; } = DateTime.UtcNow;


        public ObservableCollection<Melding> Meldingen { get; private set; } = [];
        public ObservableCollection<int> MeldingCodes { get; private set; } = [];


        public event Action? OnUpdated; // 🔥 Event voor automatische UI-updates
        public event PropertyChangedEventHandler? PropertyChanged; // Welke moeten we nu gebruiken?.. 




        public void UpdateGewijzigdOp() => GewijzigdOp = DateTime.UtcNow; // Bijwerken van de wijzigingsdatum

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnUpdated?.Invoke();
        }

        // gebruikt voor binding van Nested properties die INotifyPropertyChanged implementeren
        protected bool SetNestedProperty<T>(
            ref T? field,
            T? value,
            PropertyChangedEventHandler handler,
            string propertyName) where T : class, INotifyPropertyChanged
        {
            if (ReferenceEquals(field, value))
                return false;

            if (field != null)
                field.PropertyChanged -= handler;

            field = value;

            if (field != null)
                field.PropertyChanged += handler;

            OnPropertyChanged(propertyName);
            return true;
        }





        /// <summary>
        /// Abonneer op nested properties die INotifyPropertyChanged implementeren
        /// </summary>
        protected void SubscribeNestedProperty<T>(T? field) where T : class, INotifyPropertyChanged
        {
            if (field != null)
            {
                // eerst detach oude handler als deze al bestond
                field.PropertyChanged -= NestedPropertyChanged;
                // subscribe nieuwe handler
                field.PropertyChanged += NestedPropertyChanged;
            }
        }





        /// <summary>
        /// Abonneer automatisch op PropertyChanged van alle nested properties die INotifyPropertyChanged implementeren
        /// </summary>
        protected void SubscribeNestedProperties()
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType));

            foreach (var prop in props)
            {
                var nested = prop.GetValue(this) as INotifyPropertyChanged;
                if (nested != null)
                {
                    nested.PropertyChanged -= NestedPropertyChanged;
                    nested.PropertyChanged += NestedPropertyChanged;
                }
            }
        }

        private void NestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Trigger BerekenEnValideer en geef door dat iets is veranderd
            BerekenEnValideer();
            OnPropertyChanged(null); // null = alle properties “updaten”
        }

        /// <summary>
        /// Handig om te gebruiken in constructor of setter van nested objects
        /// </summary>
        protected void ReplaceNestedProperty<T>(ref T? field, T? value) where T : class, INotifyPropertyChanged
        {
            if (field != null)
                field.PropertyChanged -= NestedPropertyChanged;

            field = value;

            if (field != null)
                field.PropertyChanged += NestedPropertyChanged;

            OnPropertyChanged(nameof(field));
        }




        //protected BaseEurocodeContext()
        //{
        //    OnUpdated += () => Console.WriteLine($"{GetType().Name} updated");
        //}

        public bool IsValidated { get; private set; }

        //public abstract bool IsAkkoord();

        // ⚠️ Centrale methode om door te geven of de context waarschuwingen bevat
        public bool HeeftWaarschuwing() => Meldingen.Any(m => m.Type == MeldingType.Waarschuwing);

        // 🔥 Centrale methode die elke context opnieuw berekent en valideert
        public bool BerekenEnValideer()
        {
            ClearMeldingen(); // 🧹 Oude meldingen wissen
            Bereken(); // 🚀 Context-specifieke berekeningen uitvoeren
            GewijzigdOp = DateTime.UtcNow;
            IsValidated = Valideer(); // ✅ Validaties uitvoeren
            OnUpdated?.Invoke(); // 🔥 UI wordt automatisch geüpdatet
            return IsValidated;
        }


        protected abstract void Bereken();

        protected abstract bool Valideer();

        public void AddMelding(Melding melding)
        {
            if (!Meldingen.Any(m => m.Bericht == melding.Bericht))
            {
                Meldingen.Add(melding);
            }
        }  // Voeg een melding toe aan de lijst

        public void AddMelding(int code)
        {
            if (MeldingCodes.Contains(code))
                return; // Voorkom dubbele meldingen
            MeldingCodes.Add(code); // Voeg de code toe aan de lijst van codes

            var melding = CommonLibrary.Helpers.MeldingenBetonHelper.GetMelding(code); // Haal de melding op uit de helper

            AddMelding(melding); // Voeg de melding toe aan de lijst van meldingen

#if DEBUG
            Console.WriteLine($"{melding}");
#endif

        }


        public void ClearMeldingen()
        {
            Meldingen.Clear(); // 🧹 Oude meldingen wissen
            MeldingCodes.Clear(); // en ook de codes
        }

        public void AddMeldingWaarschuwing(string tekst) => Meldingen.Add(new(MeldingType.Waarschuwing, tekst));

        public void AddMeldingOpmerking(string tekst) => Meldingen.Add(new(MeldingType.Opmerking, tekst));


        //public void ControleerMeldingen()
        //{
        //    ClearMeldingen();
        //    IsAkkoord(); // Voer validatie uit in de child-class
        //}

        public MarkupString ToMarkupString() => Helpers.MarkupHelper.ToMarkupString(this.ToString());
        public MarkupString ToMarkupString(bool withUnityCheck) => Helpers.MarkupHelper.ToMarkupString(this.ToString(), withUnityCheck);


    }
}
