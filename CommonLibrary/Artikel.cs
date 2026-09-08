using System.ComponentModel;

namespace CommonLibrary
{

    public enum EurocodeNormEnum
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

    public class Norm
    {
        public Norm()
        {
        }

        public Norm(string naam, string titel)
        {
            Naam = naam;
            Titel = titel;
        }

        public string Naam { get; set; } = "";
        public string Titel { get; set; } = "";
    }



    public class EurocodeParagraaf
    {
        public EurocodeParagraaf(Norm normVerwijzing, string nummer, string naam)
        {
            NormVerwijzing = normVerwijzing;
            Nummer = nummer;
            Naam = naam;
        }

        public Norm NormVerwijzing { get; set; }
        public string Nummer { get; set; } = "";
        public string Naam { get; set; } = "";





    }

    public class EurocodeFormule
    {
        public required EurocodeParagraaf Paragraaf;
        public string Nummer { get; set; } = "";
        public string Formule { get; set; } = "";

    }




    public class Artikel
    {
        public EurocodeNormEnum Norm { get; set; }
        public string Nummer { get; set; } = "";
        public string Omschrijving { get; set; } = "";



    }





}
