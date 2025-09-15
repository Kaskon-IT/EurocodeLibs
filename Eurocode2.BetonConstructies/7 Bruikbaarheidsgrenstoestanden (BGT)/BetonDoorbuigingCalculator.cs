using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// 7.4.3 Controleren van doorbuigingen door berekening
    /// 
    /// formules
    /// (7.18)
    /// (7.19)
    /// (7.20)
    /// (7.21)
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public class BetonDoorbuigingCalculator : BaseEurocodeContext
    {
        private readonly BetonContext _beton;
        private readonly BetonContextKruipEnKrimpCalculator _kruipkrimp;
        private readonly WapeningContext _wapening = new() { Tekst = "6Ø25" }; // 2945 mm2
        private readonly Snedekrachten _krachten;

        public BetonDoorbuigingCalculator(BetonContext beton, BetonContextKruipEnKrimpCalculator kruipkrimp, Snedekrachten krachten)
        {
            _beton = beton;
            _kruipkrimp = kruipkrimp;
            _krachten = krachten;
        }


        // TODO DIT GEDEELTE WEER NAAR EEN ANDERE CLASS


        public double Moment
        {
            get
            {
                return _krachten.My.Kar;
            }
        }
        //public double ScheurMoment { get; set; } = 150e6;

        /// <summary>
        /// d
        /// </summary>
        public double NuttigeHoogte { get; set; } = 650; // mm
        public double As { get; set; } = 2945; // mm2


        /// <summary>
        /// Mcr in kNm
        /// </summary>
        [TableColumn(Label = "Scheurmoment", Symbol = "<i>M</i><sub>cr</sub>", Unit = "kNm")]
        public double Scheurmoment
        {
            get
            {
                return _beton.Profiel.Wy * _beton.Fctm * 1e-6;
            }
        }




        public Formula ScheurmomentFormula => new()
        {
            StaticValue = "M_{cr} = Wf_{ctm}",
            DynamicValue = $"= {_beton.Profiel.Wy.ToEng()} \\cdot {_beton.Fctm.ToEng()} = {Scheurmoment.ToEng()}"
        };



        //Als het scheurmoment is overschreden, wordt het moment
        //opgenomen door de combinatie van een staaltrekkracht en een
        //betondrukzone.De drukzonehoogte volgt uit:

        /// <summary>
        /// x
        /// </summary>
        [TableColumn(Label = "Drukzonehoogte", Symbol = "<i>x</i>", Unit = "mm")]
        public double Drukzonehoogte
        {
            get
            {
                //double verhoudingEmod = Beton.Alphae;
                //double verhoudingWap = Wapening.As / Beton.Profiel.Area;
                //double product = verhoudingEmod * verhoudingWap;
                double d = NuttigeHoogte;

                return (-AlphaeRho + Math.Sqrt(Math.Pow(AlphaeRho, 2) + 2 * AlphaeRho)) * d;


            }
        }

        public Formula DrukzonehoogteFormula => new()
        {
            StaticValue = "x = \\left(- \\alpha_{e}\\rho + \\sqrt{(\\alpha_{e}\\rho)^2 + 2 \\alpha_e\\rho} \\right) d",
            DynamicValue = $"= \\left(- {(AlphaeRho).ToTeX()}  + \\sqrt{{ ({(AlphaeRho).ToTeX()} )^2 + 2 \\cdot {AlphaeRho.ToTeX()} }} \\right) {650} = {Drukzonehoogte.ToTeX()}"
        };


        [TableColumn(Label = "verhouding wapening / beton", Symbol = "<i>ρ</i>")]
        public double Rho { get { return _wapening.As / (_beton.Profiel.Breedte * NuttigeHoogte); } }
        public Formula RhoFormula => new() { StaticValue = "\\rho = A_s/(b\\cdot d)", DynamicValue = $"= {_wapening.As.ToEng()} / ({_beton.Profiel.Breedte} \\cdot {NuttigeHoogte}) = {Rho.ToEng()}" };



        [TableColumn(Label = "", Symbol = "<i>α</i><sub>e</sub><i>ρ</i>")]
        public double AlphaeRho { get { return this.Alphae * Rho; } }



        [TableColumn(Label = "stijfheid ongescheurd", Symbol = "(<i>EI</i>)<sub>I</sub>", Unit = "Nmm²")]
        public double StijfheidI { get { return this.EcEff * _beton.Profiel.Iy; } }
        public Formula StijfheidIFormula => new() { StaticValue = "EI_I = E_{c,eff}I_{y}", DynamicValue = $"= {StijfheidI.ToTeX()}" };

        [TableColumn(Label = "kromming (ongescheurd)", Symbol = "<i>κ</i><sub>I</sub>", Description = "In het ongescheurde stadium is de kromming:", Unit = "mm<sup>-1</sup>")]
        public double KappaI { get { return Moment / StijfheidI; } }
        public Formula KappaIFormula => new() { StaticValue = "\\Kappa_I = \\frac{M_{Eqp}}{(EI)_I}", DynamicValue = $"= \\frac{{{Moment.ToTeX()}}}{{{StijfheidI.ToTeX()}}} = {KappaI.ToTeX()}" };



        [TableColumn(Label = "stijfheid gescheurd", Symbol = "(<i>EI</i>)<sub>II</sub>", Unit = "Nmm²")]

        public double StijfheidII
        {
            get
            {
                double b = _beton.Profiel.Breedte;
                double x = Drukzonehoogte;

                return this.EcEff * (1.0 / 12.0 * b * Math.Pow(x, 3) + (b * x) * Math.Pow(x / 2, 2) + this.Alphae * _wapening.As * Math.Pow((NuttigeHoogte - x), 2));
            }
        }
        public Formula StijfheidIIFormula => new()
        {
            StaticValue = @"(EI)_{II} = E_{c,eff} \left( \frac{1}{12}bx^3 + (bx) \left( \frac{1}{2}x \right)^2 + \alpha_e A_s (d-x)^2  \right)"
        };


        [TableColumn(Label = "kromming (gescheurd)", Symbol = "<i>κ</i><sub>II</sub>", Description = "In het gescheurde stadium is de kromming:", Unit = "mm<sup>-1</sup>")]
        public double KappaII { get { return Moment / StijfheidII; } }
        public Formula KappaIIFormula => new()
        {
            StaticValue = "\\Kappa_{II} = \\frac{M_{Eqp}}{(EI)_II}",
            DynamicValue = $"= \\frac{{{Moment.ToTeX()}}}{{{StijfheidII.ToTeX()}}} = {KappaII.ToTeX()}"
        };







        [TableColumn(Label = "verdelingsfactor", Symbol = "ζ", Article = "7.4.3", Description = "is een verdelingsfactor (rekening houdend met 'tension stiffening' in een doorsnede)")]
        public double VerdelingsfactorTensionStiffening
        {
            get
            {

                return 1 - BetaTensionStiffening * Math.Pow(Scheurmoment / Moment, 2);

            }
        }

        public Formula VerdelingsfactorTensionStiffeningFormula => new("(7.19)",
            @"\zeta = 1 - \beta \left( \frac{M_{cr}}{M} \right)^2",
            $@"\zeta = 1 - {BetaTensionStiffening.ToTeX()} \left( \frac{{{Scheurmoment.ToTeX()}}}{{{Moment.ToTeX()}}} \right)^2 = {VerdelingsfactorTensionStiffening.ToTeX()}");


        [TableColumn(Label = "factor belastingduur", Symbol = "<i>β</i>", Description = "is een coëfficiënt die rekening houdt met de invloed van de belastingsduur of herhaalde belasting op de gemiddelde rek; <br />= 1,0 voor een enkele kortdurende belasting; <br />= 0,5 voor aanhoudende belastingen of meervoudige cycli van zich herhalende belastingen")]
        public double BetaTensionStiffening { get; set; } = 0.5;





        /// <summary>
        /// 7.4.3 
        /// (5) Voor belastingen met een duur die kruip veroorzaakt, mag de totale vervorming, inclusief kruip, zijn 
        /// berekend door gebruik te maken van een effectieve elasticiteitsmodulus voor beton volgens
        /// vergelijking(7.20) 
        /// De effectieve elasticiteitsmodulus van beton. (inclusief kruip)
        /// Ec,eff = Ecm / (1 + KruipCoefficient)
        /// </summary>
        [TableColumn(Label = "effectieve elasticiteitsmodulus", Symbol = $"<i>E</i><sub>c,eff</sub>", Article = "7.4.3 (5)", Unit = "N/mm²",
            Description = "(5) Voor belastingen met een duur die kruip veroorzaakt, mag de totale vervorming, inclusief kruip, zijn berekend door gebruik te maken van een effectieve elasticiteitsmodulus voor beton volgens vergelijking (7.20).")]
        public double EcEff
        {
            get
            {
                return _beton.Ecm / (1 + _kruipkrimp.KruipCoefficient);
            }
        }
        public Formula EcEffFormula => new("(7.20)",
            @"E_{c,eff} = \frac{E_{cm}}{1 + \varphi(\infty,t_0)}",
            $@"E_{{c,eff}} = \frac{{{_beton.Ecm.ToTeX()}}}{{1 + {_kruipkrimp.KruipCoefficient.ToTeX()}}} = {EcEff.ToTeX()}");

        [TableColumn(Label = "verhouding staal/beton", Symbol = $"<i>α</i><sub>e</sub>", StringFormat = "{0:0.0000}", Article = "7.4.3 (6)")]
        public double Alphae
        {
            get
            {
                return BetonStaalContext.Es / (EcEff * 1); // todo controle duizend
            }
        }
        public Formula AlphaeFormula => new("(7.21)",
            @"\alpha_{e} = \frac{E_{s}}{E_{c,eff}}",
            $@"\alpha_{{e}} = \frac{{{BetonStaalContext.Es.ToTeX()}}}{{{(EcEff * 1).ToTeX()}}} = {Alphae.ToTeX()}");


        [TableColumn(Symbol = "<i>κ</i><sub>cs,I</sub>")]
        public double KrommingKrimpI
        {
            get
            {

                return _kruipkrimp.TotaleKrimpverkorting * this.Alphae * (this.S_I / I_I);
            }
        }

        [TableColumn(Symbol = "<i>κ</i><sub>cs,II</sub>")]
        public double KrommingKrimpII
        {
            get
            {

                return _kruipkrimp.TotaleKrimpverkorting * this.Alphae * (this.S_II / I_II);
            }
        }




        [TableColumn(Symbol = "<i>S</i><sub>I</sub>", Description = "lineaire oppervlaktemoment van de wapening ten opzichte van de zwaartelijn van de doorsnede (ongescheurd)")]
        public double S_I
        {
            get
            {
                return _wapening.As * (NuttigeHoogte - 1.0 / 2.0 * _beton.Profiel.Hoogte);
            }
        }


        [TableColumn(Symbol = "<i>I</i><sub>I</sub>", Description = "kwadratische oppervlaktemoment gehele doorsnede (ongescheurd)")]
        public double I_I
        {
            get
            {
                return _beton.Profiel.Iy;
            }
        }
        public Formula I_IFormula => new()
        {
            StaticValue = @"I_{I} = I_y"
        };

        [TableColumn(Symbol = "<i>S</i><sub>II</sub>", Description = "lineaire oppervlaktemoment van de wapening ten opzichte van de zwaartelijn van de doorsnede (gescheurd)")]
        public double S_II
        {
            get
            {
                return _wapening.As * (NuttigeHoogte - Drukzonehoogte);
            }
        }

        [TableColumn(Symbol = "<i>I</i><sub>II</sub>", Description = "kwadratische oppervlaktemoment gehele doorsnede (gescheurd)")]
        public double I_II
        {
            get
            {
                double b = _beton.Profiel.Breedte;
                double x = Drukzonehoogte;

                return (1.0 / 12.0 * b * Math.Pow(x, 3) + (b * x) * Math.Pow(x / 2, 2) + this.Alphae * _wapening.As * Math.Pow((NuttigeHoogte - x), 2));
            }
        }
        public Formula I_IIFormula => new()
        {
            StaticValue = @"I_{II} = \frac{1}{12}bx^3 + (bx) \left( \frac{1}{2}x \right)^2 + \alpha_e A_s (d-x)^2"
        };









        public override bool IsAkkoord()
        {
            throw new NotImplementedException();
        }

        protected override void Bereken()
        {
            throw new NotImplementedException();
        }

        protected override bool Valideer()
        {
            throw new NotImplementedException();
        }
    }
}
