namespace CommonLibrary.Interfaces
{
    public interface IProfiel
    {
        string Id { get; set; }
        string Naam { get; set; }

        double Iy { get; }
        double Iz { get; }

        //double KgPerM { get; }
        double A { get; }

        string SvgPath { get; set; }


        //[Obsolete("IProfiel moet geen materiaal hebben => gebruik Staalprofiel, Betonprofiel etcetera voor profiel MET materiaal")]
        //IMateriaal Materiaal { get; set; }

    }

    /// <summary>
    /// Interface voor de groep staalprofielen
    /// </summary>
    public interface IStaalProfiel : IProfiel
    {


        //double Iw { get; }
        //double J { get; }
        //double KrasMaat { get; }
        //IMateriaal Materiaal { get; set; }

        double WelY { get; }
        double WelZ { get; }
        double WplY { get; }
        double WplZ { get; }

    }

    /// <summary>
    /// Interface voor de groep betonprofielen
    /// </summary>
    public interface IBetonProfiel : IProfiel
    {
        //IMateriaal Materiaal { get; set; }
    }

    /// <summary>
    /// Interface voor de groep houtprofielen
    /// </summary>
    public interface IHoutProfiel : IProfiel
    {
        //IMateriaal Materiaal { get; set; }
    }


}
