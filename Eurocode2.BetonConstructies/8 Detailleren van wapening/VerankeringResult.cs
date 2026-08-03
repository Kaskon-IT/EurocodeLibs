namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using CommonLibrary.Models;
    using ExportFactory.Shared;

    /// <summary>
    /// Resultaat van de verankeringslengte-berekening volgens EC2 §8.3 en §8.4.
    /// Bevat zowel de losse rekenwaarden als opgemaakte <see cref="ResultRow"/>'s voor UI/rapportage.
    /// </summary>
    public class VerankeringResult : IRowResult
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

        public double Fbt { get; set; }
        public double Ab { get; set; }
        public double Fcd { get; set; }

        /// <summary> TeX-formule voor <i>f</i><sub>bd</sub> (8.2). </summary>
        public Formula FbdFormula => new("(8.2)",
            @"f_{bd} = 2.25 \cdot \eta_1 \cdot \eta_2 \cdot f_{ctd}",
            $@"= 2.25 \cdot {Eta1.ToTeX()} \cdot {Eta2.ToTeX()} \cdot {Fctd.ToTeX()} = {Fbd.ToTeX()} \text{{ N/mm}}^2");

        /// <summary> <i>l</i><sub>b,rqd</sub> – basisverankeringslengte (8.3) [mm]. </summary>
        public double BasisVerankeringslengte { get; set; }

        /// <summary> TeX-formule voor <i>l</i><sub>b,rqd</sub> (8.3). </summary>
        public Formula BasisVerankeringslengteFormula => new("(8.3)",
            @"l_{b,rqd} = \frac{\text{Ø}}{4} \cdot \frac{\sigma_{sd}}{f_{bd}}",
            $@"= \frac{{{Diameter.ToTeX()}}}{{4}} \cdot \frac{{{RekenwaardeStaafspanning.ToTeX()}}}{{{Fbd.ToTeX()}}} = {BasisVerankeringslengte.ToTeX()} \text{{ mm}}");

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

        /// <summary> TeX-formule voor <i>l</i><sub>bd</sub> (8.4). </summary>
        public Formula VerankeringslengteFormula => new("(8.4)",
            @"l_{bd} = \alpha_1 \cdot \alpha_2 \cdot \alpha_3 \cdot \alpha_4 \cdot \alpha_5 \cdot l_{b,rqd} \geq l_{b,min}",
            $@"= {Alpha1.ToTeX()} \cdot {Alpha2.ToTeX()} \cdot {Alpha3.ToTeX()} \cdot {Alpha4.ToTeX()} \cdot {Alpha5.ToTeX()} \cdot {BasisVerankeringslengte.ToTeX()} \geq {MinimumVerankeringslengte.ToTeX()} = {Verankeringslengte.ToTeX()} \text{{ mm}}");

        // Buiging / buigstraal (§8.3)
        /// <summary> <i>Ø</i><sub>m,min</sub> – minimale buigroldiameter (Tabel 8.1N) [mm]. </summary>
        public double MinimaleBuigdoornDiamStaal { get; set; }
        public Formula MinimaleBuigdoornDiamStaalFormula => new("Tabel 8.1N",
            $@"\phi_{{m,min}} \geq {(Diameter > 16? "7\\phi":"4\\phi")}",
            $@"\geq {MinimaleBuigdoornDiamStaal.ToTeX()} \text{{ mm}}");

        /// <summary> <i>r</i><sub>min</sub> – minimale buigstraal (hart staaf) [mm]. </summary>
        public double MinimaleBuigstraal { get; set; }

        /// <summary>
        /// <i>Ø</i><sub>m,min</sub> op basis van betondrukbezwijken bij de buiging (8.1) [mm].
        /// 0 als er geen <see cref="VerankeringslengteInput.Fbt"/> is opgegeven.
        /// </summary>
        public double MinimaleBuigdoornDiameterBeton { get; set; }
        public Formula MinimaleBuigdoornDiameterBetonFormula => new("(8.1)",
            @"\phi_{m,\min} \geq \frac{F_{bt}}{f_{cd}} \left( \frac{1}{a_b} + \frac{1}{2\phi} \right)",
            $@"\geq \frac{{{Fbt.ToTeX()}}}{{{Fcd.ToTeX()}}} \left( \frac{{1}}{{{Ab.ToTeX()}}} + \frac{{1}}{{2\cdot{Diameter.ToTeX()}}} \right) \geq {MinimaleBuigdoornDiameterBeton.ToTeX()} \text{{ mm}}");

        public bool IsBuigrolControleUitgevoerd => MinimaleBuigdoornDiameterBeton > 0;

        /// <summary> Maatgevende minimale buigroldiameter (max. van Tabel 8.1N en (8.1)) [mm]. </summary>
        public double MinimaleBuigdoornDiameter =>
            Math.Max(MinimaleBuigdoornDiamStaal, MinimaleBuigdoornDiameterBeton);


        public Formula MinimaleBuigdoornDiameterFormula => new("(8.1)",
            $@"\phi_{{m,\min}} \geq max \left[ {(Diameter > 16? "7\\phi":"4\\phi")} ; \frac{{ F_{{bt}} }}{{f_{{cd}}}} \left( \frac{{1}}{{a_b}} + \frac{{1}}{{2\phi}} \right) \right]",
            $@"\geq max \left[ {MinimaleBuigdoornDiamStaal:0} ; \frac{{{Fbt.ToTeX()}}}{{{Fcd.ToTeX()}}} \left( \frac{{1}}{{{Ab.ToTeX()}}} + \frac{{1}}{{2\cdot{Diameter.ToTeX()}}} \right) \right] \geq {MinimaleBuigdoornDiameter.ToTeX()} \text{{ mm}}");


        // Unity check (optioneel)
        public double ToegepasteVerankeringslengte { get; set; }
        public bool IsUnityCheckUitgevoerd => ToegepasteVerankeringslengte > 0;
        public double UnityCheck =>
            ToegepasteVerankeringslengte > 0 ? Verankeringslengte / ToegepasteVerankeringslengte : double.PositiveInfinity;
        public bool IsVoldoende => IsUnityCheckUitgevoerd && UnityCheck <= 1.0;
    }


    /// <summary>
    /// 6.1 (10) Voor gedrongen constructies, zoals de in (1)P beschreven discontinue gebieden,
    /// mag de grootte van de inwendige hefboomsarm (z) zijn afgeleid uit:
    /// Voor consoles, tanden en andere uitkragingen:
    /// z = 0.4 a + 0.4 <= 1.6 a  /// 
    /// </summary>
    public class GedrongenUitkragingResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];
        public double Z { get; set; }
        public Formula ZFormula => new Formula()
        {
            Name = "6.1 (10)",  
            StaticValue = @"z= 0.4 a + 0.4 h \leq 1.6 a",
            DynamicValue = @$"= 0.4 \cdot {A.ToTeX()} + 0.4 \cdot {H.ToTeX()} \leq 1.6 \cdot {A.ToTeX()} = {Z.ToTeX()}"
        };


        public double A { get; internal set; }
        public double H { get; internal set; }

    }
    public class GedrongenUitkragingInput : BaseInput
    {

        private double _ac = 150;
        public double Ac { 
            get => _ac; 
            set => SetProperty(ref _ac, value);
        }

        private double _h = 500;
        public double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        private double _l = 300;
        public double L
        {
            get => _l;
            set => SetProperty(ref _l, value);
        }

        private double ab = 150;
        public double Ab
        {
            get => ab;
            set => SetProperty(ref ab, value);
        }


    }

    public static class GedrongenUitkragingCalculator
    {
        public static GedrongenUitkragingResult Calculate(GedrongenUitkragingInput input)
        {
            var result = new GedrongenUitkragingResult();
            // Bereken z volgens de formule: z = 0.4 * a + 0.4 h <= 1.6 * a
            List<double> helpers = [input.Ab / 2.0, input.L / 4.0, input.H / 4.0];
            double a = input.Ac + helpers.Min();
            double h = input.H;
            double z = Math.Min(0.4 * a + 0.4 * h, 1.6 * a);
            result.Z = z;
            result.A = a;
            result.H = h;
            return result;
        }
    }
}
