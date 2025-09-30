using CommonLibrary;
using CommonLibrary.Extensions;
using Eurocode.Belastingen;
using ExportFactory.Services;
using ExportFactory.Shared;
using ParametrischeProfielen;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{
    public class BetonDoorbuigingContext : BaseEurocodeContext
    {
        public BetonDoorbuigingContext ShallowClone()
        {
            var clone = (BetonDoorbuigingContext)this.MemberwiseClone();
            clone.Id = Guid.NewGuid(); // of whatever je Id-generator is
            return clone;
        }



        // context (data) model voor doorbuiging (eenvoudig)
        // LET OP!!! deze doorbuiging alleen voor VRIJ OPGELEGDE LIGGER met UNIFORME LIJNLAST!!

        // nodig voor berekening:
        // d
        // L        lengte in [mm]
        // Beton, Profiel, Kruip

        // lijnlastBlijvend     [kN/m¹]   -> w1 
        // lijnlastCombinatie   [kN/m¹]   -> w_tot
        //                                w_bijk = w_tot - w1                                  

        public BetonDoorbuigingContext(BetonContext beton, ParametrischProfielContext profiel, WapeningContext wapening)
        {
            this.Beton = beton;
            this.Profiel = profiel;
            this.Wapening = wapening;
            this.Kruipkrimp = new(Beton, Profiel);



        }



        private double _d, _lengteMM, _lijnlast;

        [TableColumn(Label = "nuttige hoogte", Symbol = "d", Unit = "mm")]
        public double D { get => _d; set => SetProperty(ref _d, value); }

        [TableColumn(Label = "lengte", Symbol = "L", Unit = "mm")]
        public double LengteMM { get => _lengteMM; set => SetProperty(ref _lengteMM, value); }

        //[TableColumn(Label = "lijnlast voor w1", Symbol = "q<sub>p</sub>")]
        //public double LijnlastBijvend { get => _qBlijvend; set => SetProperty(ref _qBlijvend, value); }

        [TableColumn(Label = "lijnlast voor w_tot", Symbol = "q<sub>...</sub>")]
        public double Lijnlast { get => _lijnlast; set => SetProperty(ref _lijnlast, value); }

        public double Moment => 1.0 / 8.0 * _lijnlast * Math.Pow(LengteMM * 0.001, 2); // kNm

        public double W1 { get; set; }
        public double Wtot { get; set; }
        public double Wbijk() => Wtot - W1;


        private BetonContext _beton = new();
        public BetonContext Beton
        {
            get => _beton;
            set => SetNestedProperty(ref _beton!, value);
        }

        private ParametrischProfielContext _profiel = new();
        public ParametrischProfielContext Profiel
        {
            get => _profiel;
            set => SetNestedProperty(ref _profiel!, value);
        }


        private BetonContextKruipEnKrimpCalculator _kruipkrimp = new();
        public BetonContextKruipEnKrimpCalculator Kruipkrimp
        {
            get => _kruipkrimp;
            set => SetNestedProperty(ref _kruipkrimp!, value);
        }


        private WapeningContext _wapening = new();
        public WapeningContext Wapening
        {
            get => _wapening;
            set => SetNestedProperty(ref _wapening!, value);
        }

        protected override void Bereken()
        {
            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {
            return true;
            //throw new NotImplementedException();
        }
    }



    /// <summary>
    /// 7.4.3 Controleren van doorbuigingen door berekening
    /// 
    /// formules
    /// (7.18)
    /// (7.19)
    /// (7.20)
    /// (7.21)
    /// 
    /// </summary>
    public class BetonDoorbuigingCalculator : BaseEurocodeContext
    {
        private BetonDoorbuigingContext _ctx;

        public BetonDoorbuigingContext Ctx
        {
            get => _ctx;
            set
            {
                if (_ctx != null)
                    _ctx.PropertyChanged -= OnContextChanged;

                _ctx = value;

                if (_ctx != null)
                    _ctx.PropertyChanged += OnContextChanged;
            }
        }

        private void OnContextChanged(object? sender, PropertyChangedEventArgs e)
        {
            BerekenEnValideer();
        }





        public override string Heading { get; set; } = "Beton doorbuiging berekening";

        //public BetonDoorbuigingContext Ctx { get; set; } = new BetonDoorbuigingContext();

        //private readonly Eurocode.Grondslagen.GrondslagenContext _grondslagen;

        //private readonly Snedekrachten _krachten;
        private readonly List<SectionForces> _combinationForces = [];
        //private readonly DoorbuigingStudie _doorbuigingStudie;
        public double Overspanning { get; internal set; }

        /// <summary>
        /// Parameterloze constructor niet gebruiken.
        /// Voor Unit tests alleen.
        /// </summary>
        public BetonDoorbuigingCalculator()
        {
            Ctx = new BetonDoorbuigingContext(new(), new(), new());
        }

        public BetonDoorbuigingCalculator(BetonDoorbuigingContext ctx)
        {
            Ctx = ctx;
        }





        private double _kruipfactorOverride = -1;


        [TableColumn(Label = "kruipcoëfficiënt", Symbol = $"<i>{GreekLetters.phi}</i><sub>(t,t<sub>0</sub>)</sub>",
            Description = "is de kruipcoëfficiënt <br />" +
            "Leeg laten of '-1' invullen om te automatisch te laten berekenen<br />" +
            "Er is ook de optie om een eigen waarde op te geven")]
        public double Kruipfactor
        {
            get => _kruipfactorOverride < 0 ? Ctx.Kruipkrimp.KruipCoefficient : _kruipfactorOverride;
            set
            {
                SetProperty(ref _kruipfactorOverride, value); // override opslaan



                //BerekenEnValideer();
            }
        }


        [TableColumn(Label = "krimpverkorting", Symbol = "<i>ε</i><sub>cs</sub>", Description = "is de totale krimpverkorting")]
        public double Krimpverkorting
        {
            get => Ctx.Kruipkrimp.TotaleKrimpverkorting;
        }


        private bool _verwaarloosKrimp = true;

        [TableColumn(Label = "verwaarloos krimp",
          Article = "7.4.3 (6)",
          Description = "Indien aangevinkt wordt de krimp niet meegenomen in de berekening<br />" +
          "OPMERKING Bij de berekening van de doorbuiging van vloeren en balken wordt de invloed van de krimp " +
          "op de grootte van de doorbuiging verwaarloosd.")]
        public bool VerwaarloosKrimp
        {
            get => _verwaarloosKrimp;
            set
            {
                SetProperty(ref _verwaarloosKrimp, value);
                //if (_verwaarloosKrimp != value)
                //{
                //    _verwaarloosKrimp = value;
                //    BerekenEnValideer();
                //}
            }
        }



        //private double LengteMM => _doorbuigingStudie.LengteLocX * 1000;
        //private double _lengteMM = 4000;
        //public double LengteMM
        //{
        //     get => _lengteMM;
        //     set => SetProperty(ref _lengteMM, value);
        //}






        [TableColumn(Label = "doorbuiging benadering", Symbol = "<i>u</i><sup>*</sup>", Unit = "mm")]
        public double DoorbuigingBenadering
        {
            get
            {
                // Bij een gelijkmatig verdeelde belasting is de doorbuiging in het
                // midden van de overspanning van de gescheurde ligger bij
                // benadering gelijk aan:
                // u * = 5/48 * k_max * l^2


                var kmax = TotaleKromming;
                var l = Ctx.LengteMM;

                var wTot = ((5.0 / 48.0) * kmax * Math.Pow(l, 2));
                Ctx.Wtot = wTot;

                // geef ook w1
                // test
                var ei = this.StijfheidMetInterpolatie;
                var w1 = (5 * Ctx.Lijnlast * Math.Pow(l, 4)) / (384 * ei);
                Ctx.W1 = w1;

                return wTot;
            }
        }
        public Formula DoorbuigingBenaderingFormula => new()
        {
            StaticValue = "u^{*} = \\frac{5}{48} \\cdot \\kappa_{max} \\cdot l^2 ",
            DynamicValue = $"= \\frac{{5}}{{48}} \\cdot {TotaleKromming.ToTeX()} \\cdot {Ctx.LengteMM.ToEng()}^2 = {DoorbuigingBenadering.ToTeX()}"
        };

        [TableColumn(Label = "context.q")]
        public string Debug
        {
            get
            {
                return $"q={this.Ctx.Lijnlast}";
            }
        }




        [TableColumn("moment", Symbol = "M<sub>Eqp</sub>")]
        public double M
        {
            get
            {
                return Ctx.Moment;


                var mEqp = _combinationForces
                    .FirstOrDefault(sf => sf.CombinatieType == BelastingCombinatieTypeEnum.QuasiBlijvend);

                return mEqp?.My ?? 500;

            }
        }

        public double MomentNmm
        {
            get
            {
                return M * 1e6;
            }
        }
        //public double ScheurMoment { get; set; } = 150e6;

        /// <summary>
        /// d
        /// </summary>
        public double NuttigeHoogte
        {
            get
            {
                //
                return this.Ctx.D;
            }
        }


        /// <summary>
        /// Mcr in kNm
        /// </summary>
        public double McrNmm
        {
            get
            {
                return Ctx.Profiel.Wy * Ctx.Beton.Fctm;
            }
        }


        [TableColumn(Label = "Scheurmoment", Symbol = "<i>M</i><sub>cr</sub>", Unit = "kNm")]
        public double Mcr
        {
            get
            {
                return McrNmm * 1e-6;
            }
        }
        public Formula McrFormula => new()
        {
            StaticValue = "M_{cr} = W\\cdot f_{ctm}",
            DynamicValue = $"= {Ctx.Profiel.Wy.ToEng()} \\cdot {Ctx.Beton.Fctm.ToEng()} = {McrNmm.ToTeX(forcedExponent: 6, unit: "Nmm")}"
        };




        //Als het scheurmoment is overschreden, wordt het moment
        //opgenomen door de combinatie van een staaltrekkracht en een
        //betondrukzone.De drukzonehoogte volgt uit:

        /// <summary>
        /// x
        /// </summary>
        [TableColumn(Label = "hoogte drukzone", Symbol = "<i>x</i>", Unit = "mm", Description = "is de hoogte van de drukzone")]
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
            DynamicValue = $"= \\left(- {(AlphaeRho).ToTeX()}  + \\sqrt{{ ({(AlphaeRho).ToTeX()} )^2 + 2 \\cdot {AlphaeRho.ToTeX()} }} \\right) {NuttigeHoogte.ToTeX()} = {Drukzonehoogte.ToTeX()}"
        };


        [TableColumn(Label = "verhouding", Symbol = "<i>ρ</i>", Description = "= <i>A</i><sub>s</sub> / (<i>bd</i>)")]
        public double Rho { get { return Ctx.Wapening.As / (Ctx.Profiel.Breedte * NuttigeHoogte); } }
        public Formula RhoFormula => new() { StaticValue = "\\rho = A_s/(b\\cdot d)", DynamicValue = $"= {Ctx.Wapening.As.ToEng()} / ({Ctx.Profiel.Breedte} \\cdot {NuttigeHoogte.ToTeX()}) = {Rho.ToEng()}" };



        [TableColumn(Label = "", Symbol = "<i>α</i><sub>e</sub><i>ρ</i>", Description = "wordt gebruikt bij het bepalen van de drukzonehoogte")]
        public double AlphaeRho { get { return this.Alphae * Rho; } }
        public Formula AlphaeRhoFormula => new() { StaticValue = $@"\alpha_e\rho={Alphae.ToTeX()}\cdot{Rho.ToTeX()}={AlphaeRho.ToTeX()}" };


        [TableColumn(Label = "stijfheid (ongescheurd)", Symbol = "(<i>EI</i>)<sub>I</sub>", Unit = "Nmm²")]
        public double StijfheidI { get { return this.EcEff * Ctx.Profiel.Iy; } }
        public Formula StijfheidIFormula => new()
        {
            StaticValue = "(EI)_I = E_{c,eff} \\cdot I_{I}",
            DynamicValue = $"(EI)_I = {StijfheidI.ToTeX()}"
        };


        [TableColumn(Label = "stijfheid (gescheurd)", Symbol = "(<i>EI</i>)<sub>II</sub>", Unit = "Nmm²")]

        public double StijfheidII
        {
            get
            {
                double b = Ctx.Profiel.Breedte;
                double x = Drukzonehoogte;

                return this.EcEff * (1.0 / 12.0 * b * Math.Pow(x, 3) + (b * x) * Math.Pow(x / 2, 2) + this.Alphae * Ctx.Wapening.As * Math.Pow((NuttigeHoogte - x), 2));
            }
        }
        public Formula StijfheidIIFormula => new()
        {
            StaticValue = @"(EI)_{II} = E_{c,eff} \cdot I_{II}",
            DynamicValue = @$"(EI)_{{II}} = {EcEff.ToTeX()} \cdot {I_II.ToTeX()}"
        };

        private double StijfheidMetInterpolatie
        {
            get
            {
                return Zeta * StijfheidII + (1 - Zeta) * StijfheidI;
            }
        }




        [TableColumn(Label = "kromming (ongescheurd)", Symbol = "<i>κ</i><sub>I</sub>", Description = "is de kromming in het ongescheurde stadium", Unit = "mm<sup>-1</sup>")]
        public double KappaI { get { return MomentNmm / StijfheidI; } }
        public Formula KappaIFormula => new()
        {
            StaticValue = "\\kappa_I = \\frac{M_{Eqp}}{(EI)_I}",
            DynamicValue = $"= \\frac{{{MomentNmm.ToTeX(forcedExponent: 6)}}}{{{StijfheidI.ToTeX()}}} = {KappaI.ToTeX()}"
        };






        [TableColumn(Label = "kromming (gescheurd)", Symbol = "<i>κ</i><sub>II</sub>", Description = "is de kromming in het gescheurde stadium", Unit = "mm<sup>-1</sup>")]
        public double KappaII { get { return MomentNmm / StijfheidII; } }
        public Formula KappaIIFormula => new()
        {
            StaticValue = "\\kappa_{II} = \\frac{M_{Eqp}}{(EI)_{II}}",
            DynamicValue = $"= \\frac{{{MomentNmm.ToTeX()}}}{{{StijfheidII.ToTeX()}}} = {KappaII.ToTeX()}"
        };







        [TableColumn(Label = "verdelingsfactor", Symbol = "ζ", Article = "7.4.3 (3)", Description = "is een verdelingsfactor rekening houdend met 'tension stiffening' in een doorsnede")]
        public double Zeta
        {
            get
            {
                // aanvulling met Abs

                if (Mcr > Math.Abs(M)) return 0;

                return 1 - BetaTensionStiffening * Math.Pow(Mcr / Math.Abs(M), 2);

            }
        }

        public Formula VerdelingsfactorTensionStiffeningFormula
        {
            get
            {
                if (Mcr > Math.Abs(M)) return new() { Name = "(7.19)", StaticValue = @"\zeta = 0\; voor\; ongescheurde \;doorsnede" };

                return new("(7.19)",
            @"\zeta = 1 - \beta \left( \frac{M_{cr}}{M} \right)^2",
            $@"\zeta = 1 - {BetaTensionStiffening.ToTeX()} \left( \frac{{{Mcr.ToTeX()}}}{{{Math.Abs(M).ToTeX()}}} \right)^2 = {Zeta.ToTeX()}");
            }
        }


        [TableColumn(Label = "factor belastingduur",
            Symbol = "<i>β</i>",
            Article = "7.4.3",
            Description = "is een coëfficiënt die rekening houdt met de invloed van de belastingsduur of herhaalde belasting op de gemiddelde rek; <br />= 1,0 voor een enkele kortdurende belasting; <br />= 0,5 voor aanhoudende belastingen of meervoudige cycli van zich herhalende belastingen")]
        public double BetaTensionStiffening
        {
            get => _betaTensionStiffening;
            set => SetProperty(ref _betaTensionStiffening, value);
        }
        private double _betaTensionStiffening = 0.5;




        /// <summary>
        /// 7.4.3 
        /// (5) Voor belastingen met een duur die kruip veroorzaakt, mag de totale vervorming, inclusief kruip, zijn 
        /// berekend door gebruik te maken van een effectieve elasticiteitsmodulus voor beton volgens
        /// vergelijking(7.20) 
        /// De effectieve elasticiteitsmodulus van beton. (inclusief kruip)
        /// Ec,eff = Ecm / (1 + KruipCoefficient)
        /// </summary>
        [TableColumn(Label = "effectieve elasticiteitsmodulus",
            Symbol = $"<i>E</i><sub>c,eff</sub>", Article = "7.4.3 (5)", Unit = "N/mm²",
            Description = "(5) Voor belastingen met een duur die kruip veroorzaakt, mag de totale vervorming, inclusief kruip, zijn berekend door gebruik te maken van een effectieve elasticiteitsmodulus voor beton volgens vergelijking (7.20).")]
        public double EcEff
        {
            get
            {
                return Ctx.Beton.Ecm / (1 + this.Kruipfactor);
            }
        }
        public Formula EcEffFormula => new("(7.20)",
            @"E_{c,eff} = \frac{E_{cm}}{1 + \varphi(\infty,t_0)}",
            $@"E_{{c,eff}} = \frac{{{Ctx.Beton.Ecm.ToTeX()}}}{{1 + {this.Kruipfactor.ToTeX()}}} = {EcEff.ToTeX()}");

        [TableColumn(Label = "verhouding staalrek/betonrek", Symbol = $"<i>α</i><sub>e</sub>", StringFormat = "{0:0.0000}", Article = "7.4.3 (6)")]
        public double Alphae
        {
            get
            {
                return BetonStaalContext.Es / (EcEff * 1);
            }
        }
        public Formula AlphaeFormula => new("(7.21)",
            @"\alpha_{e} = \frac{E_{s}}{E_{c,eff}}",
            $@"\alpha_{{e}} = \frac{{{BetonStaalContext.Es.ToTeX()}}}{{{(EcEff * 1).ToTeX()}}} = {Alphae.ToTeX()}");


        [TableColumn(Label = "kromming uitwendig",
            Symbol = "<i>κ</i><sub>qp</sub>",
            Unit = "mm<sup>-1</sup>",
            Article = "7.4.3 (3)",
            Description = "is de kromming ten gevolge van de uitwendige belasting (<i>M</i><sub>Eqp</sub>). Deze wordt gevonden door interpolatie tussen de uiterste toestanden ‘ongescheurd’ en ‘geheel gescheurd’ (EC2; vgl. (7.18)):")]
        public double KrommingUitwendig
        {
            get
            {
                return Zeta * KappaII + (1 - Zeta) * KappaI;
            }
        }
        public Formula KrommingUitwendigFormula => new()
        {
            Name = "(7.18)",
            StaticValue = @"\kappa_{qp} = \zeta \kappa_{II} + (1-\zeta) \kappa_{I}",
            DynamicValue = $@"\kappa_{{qp}} = {Zeta.ToTeX()} \cdot {KappaII.ToTeX()} + (1 - {Zeta.ToTeX()}) \cdot {KappaI.ToTeX()} = {KrommingUitwendig.ToTeX()}"
        };




        [TableColumn(Label = "kromming door krimp (ongescheurd)", Symbol = "<i>κ</i><sub>cs,I</sub>",
            Article = "7.4.3 (6)", Unit = "mm<sup>-1</sup>",
            Description = "is de bijdrage aan de kromming door krimp (ongescheurd)")]
        public double KrommingKrimpI
        {
            get
            {
                return Ctx.Kruipkrimp.TotaleKrimpverkorting * this.Alphae * (this.S_I / I_I);
            }
        }
        public Formula KrommingKrimpIFormula => new()
        {
            Name = "(7.21)",
            StaticValue = @"\kappa_{cs,I} = \epsilon_{cs} \alpha_e \frac{S_{I}}{I_{I}}",
            DynamicValue = $@"\kappa_{{cs,I}} = {Ctx.Kruipkrimp.TotaleKrimpverkorting.ToTeX()} \cdot {this.Alphae.ToTeX()} \cdot \frac{{{this.S_I.ToTeX()}}}{{{I_I.ToTeX()}}} = {KrommingKrimpI.ToTeX()}"
        };

        [TableColumn(Label = "kromming door krimp (gescheurd)", Symbol = "<i>κ</i><sub>cs,II</sub>",
            Article = "7.4.3 (6)", Unit = "mm<sup>-1</sup>",
            Description = "is de bijdrage aan de kromming door krimp wordt (gescheurd)")]
        public double KrommingKrimpII
        {
            get
            {
                return Ctx.Kruipkrimp.TotaleKrimpverkorting * this.Alphae * (this.S_II / I_II);
            }
        }
        public Formula KrommingKrimpIIFormula => new()
        {
            Name = "(7.21)",
            StaticValue = @"\kappa_{cs,II} = \epsilon_{cs} \alpha_e \frac{S_{II}}{I_{II}}",
            DynamicValue = $@"\kappa_{{cs,II}} = {Ctx.Kruipkrimp.TotaleKrimpverkorting.ToTeX()} \cdot {this.Alphae.ToTeX()} \cdot \frac{{{this.S_II.ToTeX()}}}{{{I_II.ToTeX()}}} = {KrommingKrimpII.ToTeX()}"
        };

        [TableColumn(Label = "kromming door krimp (interpolatie)", Symbol = "<i>κ</i><sub>cs</sub>", Article = "7.4.3 (3)", Unit = "mm<sup>-1</sup>",
            Description = "is de kromming ten gevolge van de krimp. Deze wordt gevonden door interpolatie tussen de uiterste toestanden ‘ongescheurd’ en ‘geheel gescheurd’ (EC2; vgl. (7.18)):")]
        public double KrommingKrimp
        {
            get
            {
                return Zeta * KrommingKrimpII + (1 - Zeta) * KrommingKrimpI;
            }
        }
        public Formula KrommingKrimpFormula => new()
        {
            Name = "(7.18)",
            StaticValue = @"\kappa_{cs} = \zeta \kappa_{cs,II} + (1-\zeta) \kappa_{cs,I}",
            DynamicValue = $@"\kappa_{{cs}} = {Zeta.ToTeX()} \cdot {KrommingKrimpII.ToTeX()} + (1 - {Zeta.ToTeX()}) \cdot {KrommingKrimpI.ToTeX()} = {KrommingKrimp.ToTeX()}"
        };

        [TableColumn(Label = "totale kromming",
            Symbol = "<i>κ</i><sub>tot</sub>",
            Unit = "mm<sup>-1</sup>",
            Description = "is de totale kromming. Dit is de som van de kromming uit uitwendige belasting (<i>κ</i><sub>qp</sub>) " +
            "en de kromming uit krimp (<i>κ</i><sub>cs</sub>)<br >" +
            "OPMERKING de kromming uit krimp mag verwaarloosd zijn volgens de Nationale Bijlage ")]
        public double TotaleKromming
        {
            get
            {
                if (VerwaarloosKrimp)
                {
                    return KrommingUitwendig;
                }
                else
                {
                    return KrommingUitwendig + KrommingKrimp;
                }
            }
        }
        public Formula TotaleKrommingFormula
        {
            get
            {
                if (VerwaarloosKrimp)
                {
                    return new()
                    {
                        StaticValue = @"\kappa_{tot} = \kappa_{qp}",
                        DynamicValue = $@"\kappa_{{tot}} = {KrommingUitwendig.ToTeX()} = {TotaleKromming.ToTeX()}"
                    };
                }
                else
                {
                    return new()
                    {
                        StaticValue = @"\kappa_{tot} = \kappa_{qp} + \kappa_{cs}",
                        DynamicValue = $@"\kappa_{{tot}} = {KrommingUitwendig.ToTeX()} + {KrommingKrimp.ToTeX()} = {TotaleKromming.ToTeX()}"
                    };
                }
            }
        }







        [TableColumn(Label = "lineaire oppervlaktemoment (ongescheurd)",
            Symbol = "<i>S</i><sub>I</sub>",
            Unit = "mm³",
            Description = "is het lineaire oppervlaktemoment van de wapening ten opzichte van de zwaartelijn van de doorsnede (ongescheurd)<br />" +
            "NB. Omdat we (in het ongescheurde stadium) de invloed van het betonstaal niet in rekening brengen, bevindt de zwaartelijn zich in het zwaartepunt van de betondoorsnede")]
        public double S_I
        {
            get
            {
                return Ctx.Wapening.As * (NuttigeHoogte - 1.0 / 2.0 * Ctx.Profiel.Hoogte);
            }
        }
        public Formula S_IFormula => new()
        {
            StaticValue = @"S_{I} = A_s \left( d- \frac{1}{2} h \right)",
            DynamicValue = $"= {Ctx.Wapening.As.ToTeX()} \\cdot \\left( {NuttigeHoogte.ToTeX()} - \\frac{{1}}{{2}} \\cdot{Ctx.Profiel.Hoogte} \\right)"
        };


        [TableColumn(Label = "lineaire oppervlaktemoment (gescheurd)",
           Symbol = "<i>S</i><sub>II</sub>",
           Unit = "mm³",
           Description = "is het lineaire oppervlaktemoment van de wapening ten opzichte van de zwaartelijn van de doorsnede (gescheurd)")]
        public double S_II
        {
            get
            {
                return Ctx.Wapening.As * (NuttigeHoogte - Drukzonehoogte);
            }
        }
        public Formula S_IIFormula => new()
        {
            StaticValue = @"S_{II} = A_s (d-x)",
            DynamicValue = $"= {Ctx.Wapening.As.ToTeX()} \\cdot ({NuttigeHoogte.ToTeX()} - {Drukzonehoogte.ToTeX()}) = {S_II.ToTeX()}"
        };



        [TableColumn(Label = "kwadratische oppervlaktemoment (ongescheurd)",
            Symbol = "<i>I</i><sub>I</sub>",
            Unit = "mm<sup>4</sup>",
            Description = "is het kwadratische oppervlaktemoment van de gehele doorsnede (ongescheurd). <br />" +
            "NB. De invloed van het betonstaal is hier buiten beschouwing gelaten.")]
        public double I_I
        {
            get
            {
                return Ctx.Profiel.Iy;
            }
        }
        public Formula I_IFormula => new()
        {
            StaticValue = @"I_{I} = I_y"
        };




        [TableColumn(Label = "kwadratische oppervlaktemoment (gescheurd)",
            Symbol = "<i>I</i><sub>II</sub>",
            Unit = "mm<sup>4</sup>",
            Description = "is het kwadratische oppervlaktemoment van de gehele doorsnede (gescheurd)")]
        public double I_II
        {
            get
            {
                double b = Ctx.Profiel.Breedte;
                double x = Drukzonehoogte;

                return (1.0 / 12.0 * b * Math.Pow(x, 3) + (b * x) * Math.Pow(x / 2, 2) + this.Alphae * Ctx.Wapening.As * Math.Pow((NuttigeHoogte - x), 2));
            }
        }
        public Formula I_IIFormula
        {
            get
            {
                var b = Ctx.Profiel.Breedte.ToTeX();
                var x = Drukzonehoogte.ToTeX();
                return new()
                {

                    StaticValue = @"I_{II} = \frac{1}{12}bx^3 + (bx) \left( \frac{1}{2}x \right)^2 + \alpha_e A_s (d-x)^2",
                    DynamicValue = @$"I_{{II}} = \frac{{1}}{{12}}\cdot{b}\cdot{x}^3 + ({b}\cdot{x}) \left( \frac{{1}}{{2}}{x} \right)^2 + {Alphae.ToTeX()} \cdot {Ctx.Wapening.As.ToTeX()} \cdot ({NuttigeHoogte.ToTeX()}-{x})^2"
                };
            }
        }
















        protected override void Bereken()
        {
            // optie om berekeningen toe te voegen
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            if (_kruipfactorOverride > -1)
            {
                AddMeldingOpmerking($"De kruipcoëfficiënt is door gebruiker zelf opgegeven");


            }

            if (BetaTensionStiffening < 0)
            {
                AddMeldingWaarschuwing("factor belastingduur mag niet kleiner dan 0 zijn");
            }

            if (BetaTensionStiffening > 1)
            {
                AddMeldingWaarschuwing("factor belastingduur mag niet gorter dan 1 zijn");
            }


            if (!VerwaarloosKrimp)
            {
                AddMeldingOpmerking("Invloed krimp op de grootte van de doorbuiging NIET verwaarloosd. ");
            }



            //if (DoorbuigingBenadering > Toe)
            //{
            //    Meldingen.Add(new Melding(MeldingType.Waarschuwing, $"De berekende doorbuiging {DoorbuigingBenadering:N0} mm is groter dan de maximaal toelaatbare doorbuiging {_doorbuigingStudie.ToelaatbareDoorbuiging:N0} mm (L/250)"));
            //    return false;
            //}
            return true;
        }
    }
}
