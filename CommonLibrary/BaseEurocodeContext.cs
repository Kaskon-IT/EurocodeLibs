using CommonLibrary.Interfaces;
using System.Collections.ObjectModel;


namespace CommonLibrary
{
    /// <summary>
    /// Basis voor alle eurocode context, waarbij validatie en melding noodzakelijk is.
    /// Een lijst met meldingen is beschikbaar. 
    /// (bijvoorbeeld "Waarschuwing - Overschrijding hoogte drukzone" of
    /// neutrale melding "Minimale wapening toegepast conform artikel 80.80")
    /// </summary>
    public abstract class BaseEurocodeContext : IEurocodeContext
    {
        public ObservableCollection<Melding> Meldingen { get; private set; } = [];

        public event Action? OnUpdated; // 🔥 Event voor automatische UI-updates

        protected BaseEurocodeContext()
        {
            OnUpdated += () => Console.WriteLine($"{GetType().Name} bijgewerkt!");
        }

        public bool IsValidated { get; private set; }

        public abstract bool IsAkkoord();

        // ⚠️ Centrale methode om door te geven of de context waarschuwingen bevat
        public bool HeeftWaarschuwing()
        {
            return Meldingen.Any(m => m.Type == MeldingType.Waarschuwing);
        }


        // 🔥 Centrale methode die elke context opnieuw berekent en valideert
        public bool BerekenEnValideer()
        {
            ClearMeldingen(); // 🧹 Oude meldingen wissen
            Bereken(); // 🚀 Context-specifieke berekeningen uitvoeren
            IsValidated = Valideer(); // ✅ Validaties uitvoeren

            OnUpdated?.Invoke(); // 🔥 UI wordt automatisch geüpdatet
            return IsValidated;
        }

        protected abstract void Bereken(); // Implementatie per context
        protected abstract bool Valideer(); // Implementatie per context

        public void AddMelding(Melding melding)
        {
            Meldingen.Add(melding);
            //OnMeldingenChanged?.Invoke();
        }


        public void ClearMeldingen()
        {
            Meldingen.Clear();
            //OnMeldingenChanged?.Invoke();
        }

        public void AddMeldingWaarschuwing(string tekst)
        {
            Meldingen.Add(new(MeldingType.Waarschuwing, tekst));
            //OnMeldingenChanged?.Invoke();
        }

        public void AddMeldingOpmerking(string tekst) => Meldingen.Add(new(MeldingType.Opmerking, tekst));




        public void ControleerMeldingen()
        {
            Meldingen.Clear(); // Reset meldingen
            IsAkkoord(); // Voer validatie uit in de child-class
        }

        // Hulpmethode om bij wijziging automatisch te valideren
        protected T SetAndValidate<T>(ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                ControleerMeldingen(); // Automatisch valideren bij wijziging
            }
            return value;
        }

    }
}
