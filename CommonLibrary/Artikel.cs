using System.ComponentModel;

namespace CommonLibrary
{
    public enum EurocodeNorm
    {
        [Description("Grondslagen van de berekening")]
        Grondslagen = 0,
        Belastingen = 1,
        Betonconstructies = 2,
        Staalconstructies = 3,
        StaalBetonConstructies = 4,
        Houtconstructies = 5,
        Metselwerk = 6,
        Geotechniek = 7,
        Aardbevingen = 8,
    }

    public class Artikel
    {
        public EurocodeNorm Norm { get; set; }
        public string Nummer { get; set; }
        public string Omschrijving { get; set; }



    }



}
