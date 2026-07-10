namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    /// <summary>
    /// Resultaat van de verankeringslengte-berekening volgens EC2 §8.3 en §8.4.
    /// Bevat zowel de losse rekenwaarden als opgemaakte <see cref="ResultRow"/>'s voor UI/rapportage.
    /// </summary>
    public class VerankeringslengteResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];

        // Invoer die ook in het resultaat handig is
        public double Diameter { get; set; }
        public double RekenwaardeStaafspanning { get; set; }
        public VerankeringStaafType StaafType { get; set; }
        public VerankeringStaafVorm StaafVorm { get; set; }

        // Aanhechting
        public double Fctd { get; set; }
        public double Eta1 { get; set; }
        public double Eta2 { get; set; }

        /// <summary> <i>f</i><sub>bd</sub> – rekenwaarde opneembare aanhechtspanning (8.2) [N/mm²]. </summary>
        public double Fbd { get; set; }

        /// <summary> <i>l</i><sub>b,rqd</sub> – basisverankeringslengte (8.3) [mm]. </summary>
        public double BasisVerankeringslengte { get; set; }

        // α-factoren (Tabel 8.2)
        public double Alpha1 { get; set; }
        public double Alpha2 { get; set; }
        public double Alpha3 { get; set; }
        public double Alpha4 { get; set; }
        public double Alpha5 { get; set; }

        /// <summary> Product <i>α</i><sub>2</sub>·<i>α</i><sub>3</sub>·<i>α</i><sub>5</sub> ≥ 0,7 (8.5). </summary>
        public double ProductAlpha235 { get; set; }

        /// <summary> <i>l</i><sub>b,min</sub> – minimum verankeringslengte (8.6)/(8.7) [mm]. </summary>
        public double MinimumVerankeringslengte { get; set; }

        /// <summary> <i>l</i><sub>bd</sub> – rekenwaarde van de verankeringslengte (8.4) [mm]. </summary>
        public double Verankeringslengte { get; set; }

        // Buiging / buigstraal (§8.3)
        /// <summary> <i>Ø</i><sub>m,min</sub> – minimale buigroldiameter (Tabel 8.1N) [mm]. </summary>
        public double MinimaleBuigrolDiameter { get; set; }
        /// <summary> <i>r</i><sub>min</sub> – minimale buigstraal (hart staaf) [mm]. </summary>
        public double MinimaleBuigstraal { get; set; }

        /// <summary>
        /// <i>Ø</i><sub>m,min</sub> op basis van betondrukbezwijken bij de buiging (8.1) [mm].
        /// 0 als er geen <see cref="VerankeringslengteInput.Fbt"/> is opgegeven.
        /// </summary>
        public double MinimaleBuigrolDiameterBeton { get; set; }

        public bool IsBuigrolControleUitgevoerd => MinimaleBuigrolDiameterBeton > 0;

        /// <summary> Maatgevende minimale buigroldiameter (max. van Tabel 8.1N en (8.1)) [mm]. </summary>
        public double MaatgevendeBuigrolDiameter =>
            Math.Max(MinimaleBuigrolDiameter, MinimaleBuigrolDiameterBeton);

        // Unity check (optioneel)
        public double ToegepasteVerankeringslengte { get; set; }
        public bool IsUnityCheckUitgevoerd => ToegepasteVerankeringslengte > 0;
        public double UnityCheck =>
            ToegepasteVerankeringslengte > 0 ? Verankeringslengte / ToegepasteVerankeringslengte : double.PositiveInfinity;
        public bool IsVoldoende => IsUnityCheckUitgevoerd && UnityCheck <= 1.0;
    }
}
