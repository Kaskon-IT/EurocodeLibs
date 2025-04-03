using System.Collections.ObjectModel;

namespace CommonLibrary.Interfaces
{

    public interface IEurocodeContext
    {
        // Lijst met meldingen
        ObservableCollection<Melding> Meldingen { get; }

        // Methode om te controleren of het object akkoord is
        bool IsAkkoord();

        // Eventueel: een methode om meldingen toe te voegen
        void AddMelding(Melding melding);
    }


}
