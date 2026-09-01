namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using CommonLibrary.Models;
    using ExportFactory.Shared;

    /// <summary>
    /// EC2 §8      Detaillering van wapening
    /// EC2 §8.1    Algemeen
    /// EC2 §8.2    Staafafstanden 
    /// EC2 §8.3    Buigdoorndiameter 
    /// EC2 §8.4    Verankering langswapening
    /// EC2 §8.5    Verankering beugels en dwarskrachtwapening
    /// EC2 §8.6    Verankering aangelaste staven 
    /// EC2 §8.7    Overlapping en mechanische koppelling
    /// EC2 §8.8    Aanvullende regels staven met grote diameter
    /// EC2 §8.9    Gebundelde staven
    /// EC2 §8.10   Voorspanelementen (NIET OPGENOMEN) 
    /// Resultaat van de verankeringslengte-berekening volgens EC2 §8.3 en §8.4.
    /// Bevat zowel de losse rekenwaarden als opgemaakte <see cref="ResultRow"/>'s voor UI/rapportage.
    /// </summary>
    public class VerankeringResult : IRowResult
    {
        public string Id { get; set; } = "";
        public string Naam { get; set; } = "";


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


        //gedeelte aangelaste staven
        public double Fbtd { get; set; }
        public static Formula FbtdFormula => new("(8.8N)", 
            $@"F_{{btd}}=l_{{td}} /phi_t /sigma_{{td}} \text{{, maar niet grotere dan }}F_{{wd}}", 
            @$"=");
        
        /// <summary>
        /// Is de rekenwaarde van de lengte van de dwarsstaaf
        /// </summary>
        public double Ltd { get; set; }
        /// <summary>
        /// Is de lengte van de dwarsstaaf, maar niet groter dan de hoh-afstand tussen de te verankeren staven
        /// </summary>
        public double Lt { get; set; }


        /// <summary>
        /// Diameter van de dwarsstaaf
        /// </summary>
        public double DiameterT { get; set; }

        /// <summary>
        /// Betonspanning
        /// </summary>
        public double SigmaTD { get; set; }
        public static Formula SigmaTDFormula => new("",
            $@"/sigma_{{td}} = (f_{{ctd}} + /sigma_{{cm}} / y /leq 3 f_{{cd}}",
            $@"");


        /// <summary>
        /// Drukspanning in het beton loodrecht op beide staven (gemiddelde waarde, positief voor druk)
        /// </summary>
        public double SigmaCM { get; set; }

        public double Y { get; set; }
        public double X { get; set; }



        public double AfstandTotFbt { get; set; }
        public double Fbt { get; set; }
        public double FactorResterendeLengte => Math.Max(0, 1 - (AfstandTotFbt / Verankeringslengte));
        public Formula FactorResterendeLengteFormula => new()
        {
            StaticValue = @$"\eta_{{lb}} = 1 - (a_{{recht}} / l_{{bd}}) \geq 0",
            DynamicValue = @$"= 1 - ({AfstandTotFbt} / {Verankeringslengte})"
        };



        public Formula FbtFormula => new() { 
            StaticValue = @$"F_{{bt}} = \eta_{{lb}} \cdot A_s \cdot f_y", 
            DynamicValue = @$"= \eta_{{lb}} \cdot {WapeningHelper.GetDsnOpp(1,Diameter):0} \cdot {RekenwaardeStaafspanning:0} = {Fbt:0} \text{{ N}}" };
        public double Ab { get; set; }
        
        /// <summary>
        /// c_d voor alpha2
        /// </summary>
        public double Cd { get; set; }
        public Formula CdFormula => new() { StaticValue = @$"c_d = {Cd:0} mm" };

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
        public Formula Alpha2Formula
        {
            get
            {
                if (StaafType == VerankeringStaafType.Drukstaaf)
                    return new() { StaticValue = @$"\alpha_2={Alpha2} \text{{ (drukstaaf)}}" };
                else if (StaafVorm == VerankeringStaafVorm.Recht)
                {
                    return new() { StaticValue = @"\alpha_2=1-0.15(c_d-\phi) / \phi \geq 0.7 \leq 1.0 \text{ (recht)}" };
                }
                else if (StaafVorm == VerankeringStaafVorm.Gebogen)
                {
                    return new() { StaticValue = @"\alpha_2=1-0.15(c_d-3\phi) / \phi \geq 0.7 \leq 1.0 \text{ (gebogen)}" };
                }
                else return new() { StaticValue = "?" };
            }
        }
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
            $@"\phi_{{m,min,s}} \geq {(Diameter > 16? "7\\phi":"4\\phi")}",
            $@"\geq {MinimaleBuigdoornDiamStaal.ToTeX()} \text{{ mm}}");

        

        /// <summary>
        /// <i>Ø</i><sub>m,min</sub> op basis van betondrukbezwijken bij de buiging (8.1) [mm].
        /// 0 als er geen <see cref="VerankeringslengteInput.Fbt"/> is opgegeven.
        /// </summary>
        public double MinimaleBuigdoornDiameterBeton { get; set; }
        public Formula MinimaleBuigdoornDiameterBetonFormula  => new("(8.1)",
            @"\phi_{m,min,b} \geq \frac{F_{bt}}{f_{cd}} \left( \frac{1}{a_b} + \frac{1}{2\phi} \right)",
            $@"\geq \frac{{{Fbt.ToTeX()}}}{{{Fcd.ToTeX()}}} \left( \frac{{1}}{{{Ab.ToTeX()}}} + \frac{{1}}{{2\cdot{Diameter.ToTeX()}}} \right) \geq {MinimaleBuigdoornDiameterBeton.ToTeX()} \text{{ mm}}");

        public bool IsBuigrolControleUitgevoerd => MinimaleBuigdoornDiameterBeton > 0;

        /// <summary> Maatgevende minimale buigroldiameter (max. van Tabel 8.1N en (8.1)) [mm]. </summary>
        public double MinimaleBuigdoornDiameter =>
            Math.Max(MinimaleBuigdoornDiamStaal, MinimaleBuigdoornDiameterBeton);


        public Formula MinimaleBuigdoornDiameterFormula => new("(8.1)",
            $@"\phi_{{m,min}} \geq max \left[ \phi_{{min,s}} ;  \phi_{{min,b}} \right]",
            $@"\geq max \left[ {MinimaleBuigdoornDiamStaal:0} ; {MinimaleBuigdoornDiameterBeton:0} \right] \geq {MinimaleBuigdoornDiameter.ToTeX()} \text{{ mm}}");


        // Unity check (optioneel)
        public double ToegepasteVerankeringslengte { get; set; }
        public bool IsUnityCheckVerankeringslengteUitgevoerd => ToegepasteVerankeringslengte > 0;
        public double UnityCheckVerankeringslengte =>
            ToegepasteVerankeringslengte > 0 ? Verankeringslengte / ToegepasteVerankeringslengte : double.PositiveInfinity;
        public bool IsVoldoende => IsUnityCheckVerankeringslengteUitgevoerd && UnityCheckVerankeringslengte <= 1.001;


        public double ToegepasteBuigdoornDiameter { get; set; }
        public bool IsUnityCheckBuigdoornUitgevoerd => ToegepasteBuigdoornDiameter > 0;
        public double UnityCheckBuigdoorn =>
            ToegepasteBuigdoornDiameter > 0 ? MinimaleBuigdoornDiameter / ToegepasteBuigdoornDiameter : double.PositiveInfinity;
        public bool BuigdoornOk => IsUnityCheckBuigdoornUitgevoerd && UnityCheckBuigdoorn <= 1.001;

        private double ToegepasteBuigstraalInner => ToegepasteBuigdoornDiameter / 2.0;
        private double ToepgepasteBuigstraalHart => (ToegepasteBuigdoornDiameter + Diameter) / 2.0;


        public Formula ToegepasteBuigdoornFormula{
            get
            {
                return new() { StaticValue = @$"\phi_{{m}} = {ToegepasteBuigdoornDiameter},\quad r_i={ToegepasteBuigstraalInner:0.#},\quad r_{{hart}}={ToepgepasteBuigstraalHart:0.#} " };

            }
        }


    }
}
