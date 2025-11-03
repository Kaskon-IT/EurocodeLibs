using CommonLibrary;
using CommonLibrary.Extensions;
using Eurocode.Belastingen;
using ExportFactory.MigraDocContentModels;
using ExportFactory.Services;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{
    public enum BerekeningTypeEnum
    {
        [Description("Ontwerpberekening")]
        BepaalBenodigeWapening,
        [Description("Controleberekening")]
        ControleerWapening,
    }

    [Obsolete("classes met korte weergave niet meer nodig, we kunnen de properties filteren, orderen etcetera.")]
    public class DwarskrachtWapContextKort : DwarskrachtWapContext
    {

        // deze class heb ik aangemaakt om een korte weergave te hebben in de UI
        // dit is een tijdelijke oplossing, want uiteindelijk wil ik dat we de properties kunnen filteren
        // en ordenen op basis van de TableColumn attributes.
        // Dit is een tussenstap, want ik wil de UI niet teveel aanpassen in één keer.
        // in de vakantie van Martijn is dit toch gedaan, dus deze class is nu overbodig geworden.
        // even samen doornemen met Martijn.

        public DwarskrachtWapContextKort()
        {
            // lege constructor
        }

        public DwarskrachtWapContextKort(BetonContext beton, ParametrischeProfielen.ParametrischProfielContext profiel, SectionForces snedekrachten)
            : base(beton, profiel, snedekrachten)
        {
            BerekeningType = BerekeningTypeEnum.BepaalBenodigeWapening;
            // Bereken(); // niet nodig, want we hebben geen meldingen
        }

        [TableColumn(Symbol = "<i>V</i><sub>Ed</sub>", Unit = "kN")]
        public string Test
        {
            get
            {
                return this.Ved.ToString();
            }
        }

        [TableColumn("V<sub>Rd,c</sub>")]
        public string Test2
        {
            get
            {
                return this.DwarskrachtWeerstandBeton.ToString();
            }
        }

    }


    public class DwarskrachtWapContext : BaseEurocodeContext
    {
        public DwarskrachtWapContext()
        {
            Beton = new();
            Profiel = new();
            Snedekrachten = new();
            LijstBeugelWap = new List<BeugelWap>();
        }

        // context voor de dwarskrachtwapening volgens art. 6.2
        // Uitgangspunten voor niet-voorgespannen constructies

        //private List<int> _meldingen { get; set; } = [];
        //private Dictionary<int, Melding> _betonMeldingen = new MeldingenBeton().Meldingen;

        //public List<Melding> Meldingen { get; set; } = [];
        public override string Heading { get; set; } = "Dwarskracht";

        public override string ToString()
        {
            if (Ved <= DwarskrachtWeerstandBeton)
            {
                return $"V<sub>Ed</sub> = {Ved:0.# kN}, V<sub>Rd,c</sub> = {DwarskrachtWeerstandBeton:0.# kN}";
            }
            else
            {
                return $"V<sub>Ed</sub> = {Ved:0.# kN}, " +
                    $"V<sub>Rd,c</sub> = {DwarskrachtWeerstandBeton:0.# kN}, " +
                    $"V~Rd~ = {DwarskrachtWeerstand:0.# kN}, " +
                    $"V<sub>Rd,max</sub> = {DwarskrachtWeerstandMax:0 kN}, " +
                    $"A~sw,ben~ = {AswBenPerMeter: 0 mm²/m¹}, " +
                    $"A~sw,toe~ = {AswToegepast: 0 mm²/m¹}, " +
                    $"(UC = {Math.Max(Ved / DwarskrachtWeerstand, Ved / DwarskrachtWeerstandMax):0.00})";
            }


        }


        public bool IsDwarskrachtWapeningBenodigd()
        {
            return Ved > DwarskrachtWeerstandBeton;
        }

        public List<string> Artikelen { get; set; } = [];


        public BerekeningTypeEnum _berekeningType = BerekeningTypeEnum.ControleerWapening;
        public BerekeningTypeEnum BerekeningType
        {
            get => _berekeningType;
            set => SetProperty(ref _berekeningType, value);
        }


        //[TableColumn("Opm.", "Opmerkingen")]
        public string MeldingenUserFriendlyName { get { return string.Join(",", Meldingen); } }

        //[TableColumn("Art.", "Artikelen")]
        public string GebruikteArtikelenUserFriendlyName { get { return string.Join(",", Artikelen); } }

        //public WringingWapContext WringWap { get; set; } // voor berekeningen met dwarskracht EN wringing (als onderdeel van de dwarskrachtWap)
        //public MeldingenBeton MeldingenBeton = new MeldingenBeton(); // voor meldingen en foutmeldingen uit de betonModule

        // Vanuit het beton
        //public double Fck { get; set; }
        private BetonContext _beton = new();
        public BetonContext Beton
        {

            get => _beton;
            set => SetNestedProperty(ref _beton!, value);
        }

        private ParametrischeProfielen.ParametrischProfielContext _profiel = new();
        public ParametrischeProfielen.ParametrischProfielContext Profiel
        {
            get => _profiel;
            set => SetNestedProperty(ref _profiel!, value);
        }

        //public Snedekrachten Snedekrachten { get; set; } = new();
        private SectionForces _snedekrachten = new();
        public SectionForces Snedekrachten
        {
            get => _snedekrachten;
            set => SetNestedProperty(ref _snedekrachten!, value);
        }





        /// <summary>
        /// Rekenwaarde van de dwarskracht in kN
        /// </summary>
        [TableColumn(
            Label = "dwarskracht (rekenwaarde)",
            Symbol = "<i>V</i><sub>Ed</sub>",
            Unit = "kN")]
        public double Ved { get { return Snedekrachten.Vz; } }

        /// <summary>
        /// is de hoek in graden tussen de drukdiagonaal van beton en de as van de ligger loodrecht op de dwarskracht;
        /// </summary>
        [TableColumn(
            Label = "hoek drukdiagonaal",
            Symbol = $"<i>{GreekLetters.theta}</i>",
            Unit = "°")]

        public double Theta
        {
            get => _theta;
            set => SetProperty(ref _theta, value);
        }
        public bool ThetaEditable { get; set; } = false;

        private double _theta = 21.8;


        /// <summary>
        /// hoek drukdiagonaal in radialen
        /// </summary>
        public double GetThetaRadialen { get { return Theta * Math.PI / 180; } }

        /// <summary>
        /// tangens van de hoek drukdiagonaal
        /// </summary>
        //[TableColumn("tan |theta|", StringFormat = "0.##")]

        public double TanTheta { get { return Math.Tan(Theta * Math.PI / 180); } }

        /// <summary>
        /// CoTangens van de hoek drukdiagonaal
        /// </summary>
        /// 
        [TableColumn(
            Symbol = "cot<i>θ</i>",
            Label = "cotangens hoek drukdiagonaal",
            StringFormat = "0.##")]

        public double CotTheta
        {
            get
            {
                if (TanTheta != 0)
                    return 1 / TanTheta;
                else
                    return 0;
            }
        }

        /// <summary>
        /// is de hoek tussen de dwarskrachtwapening en de as van de ligger loodrecht op de dwarskracht (positief gemeten zoals getoond in figuur 6.5) in graden;
        /// </summary>
        [TableColumn(Label = "hoek dwarskrachtwapening",
            Symbol = $"<i>{GreekLetters.alpha}</i>",
            Unit = "°")]
        public double Alpha
        {
            get => _alpha;
            set => SetProperty(ref _alpha, value);
        }
        private double _alpha = 90;
        public bool AlphaEditable { get; set; } = false;


        private double TanAlpha { get { return Math.Tan(Alpha * Math.PI / 180); } }
        public double CotAlpha { get { return 1 / TanAlpha; } }


        /// <summary>
        /// Breedte van de doorsnede voor de dwarskracht in mm
        /// </summary>
        [TableColumn(Label = "breedte dwarskracht", Symbol = "<i>b</i><sub>w</sub>", Unit = "mm")]
        public double Breedte
        {
            get { return Profiel.BreedteDwarskracht; }
        }

        /// <summary>
        /// is de minimale breedte tussen de trek- en drukrand in mm²
        /// </summary>
        [TableColumn(
            Label = "langswapening",
            Symbol = "<i>A</i><sub>sl</sub>", Unit = "mm²", StringFormat = "0")]
        public double AsLangs
        {
            get => _asLangs;
            set => SetProperty(ref _asLangs, value);
        }
        public bool AsLangsEditable { get; set; } = false;

        private double _asLangs;

        /// <summary>
        /// Nuttige hooge (d) van de dwarskrachtdoorsnede in mm
        /// </summary>
        [TableColumn(Label = "nuttige hoogte", Symbol = "d", Unit = "mm")]
        public double NutHoogte
        {
            get => _nutHoogte;
            set => SetProperty(ref _nutHoogte, value);
        }
        public bool NutHoogteEditable { get; set; } = false;
        private double _nutHoogte = 90;


        // --- Toegepaste Wapening
        public double BeugelDiameter;
        public double BeugelHartOpHartAfstand;
        public double BeugelSnedeAantal;
        public double BeugelAfstandDwarsToegepast;
        public double DekkingZijkantToegepast;

        public DwarskrachtWapContext(BetonContext beton, ParametrischeProfielen.ParametrischProfielContext profiel, SectionForces snedekrachten)
        {
            Beton = beton; // materiaal, staal, dekking, etcetera
            Profiel = profiel; // geometrie 
            Snedekrachten = snedekrachten; // krachten
            //LijstBeugelWap = new List<BeugelWap>();

        }



        // vanuit het betonstaal
        public double Fywk { get; set; } // 
        public double Fywd { get; set; } // OPMERKING Indien vergelijking (6.10) gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot 0,8fywk;


        private double? _aswToegepast; // backing-field om gebruikersinvoer te bewaren

        [TableColumn(
            Label = "toegepaste dwarskrachtwapening",
            Symbol = "<i>A</i><sub>sw,prov</sub>",
            Unit = "mm²/m")]
        public double AswToegepast
        {
            get
            {
                // Automatische berekening bij een specifiek BerekeningType
                if (this.BerekeningType == BerekeningTypeEnum.BepaalBenodigeWapening)
                {
                    return Math.Ceiling(this.AswBenPerMeter);
                }
                // Gebruik de handmatige invoer als die er is
                return _aswToegepast ?? 0.0; // Standaardwaarde indien null
            }
            set => SetProperty(ref _aswToegepast, value);

        }

        public string ToelichtingVRdc
        {
            get
            {
                return $"V<sub>Rd,c</sub> = {SchuifspanningWeerstandBeton:0.##} N/mm² * {Breedte:0.##} mm * {NutHoogte:0.##} mm / 1000 = {DwarskrachtWeerstandBeton:0.##} kN";
            }
        }

        public string ToelichtingSchuifspanningWeerstandBeton
        {
            get
            {
                return $"<i>ν</i><sub>Rd,c</sub> = {SchuifspanningWeerstandStaal:0.##} N/mm² * {Breedte:0.##} mm * {NutHoogte:0.##} mm / 1000 = {DwarskrachtWeerstandStaal:0.##} kN";
            }
        }


        public double BeugelAfstandMaxLangs { get; set; }
        public double BeugelAfstandMaxDwars { get; set; }
        public double SpanningDwarskrachtWapening
        {
            get
            {
                if (AswToegepast == 0)
                    return Fywd;
                else
                    return (AswBenPerMeter / AswToegepast) * Fywd;
            }
        }
        public bool SpanningWapeningKleinerDan80ProcentKarakteristiekeVloeigrens
        {
            get
            {
                if (SpanningDwarskrachtWapening < (Fywk * 0.8)) return true;
                else return false;
            }
        }



        public List<BeugelWap> LijstBeugelWap { get; set; }



        public double AlphaCw { get; } = 1; // art. 6.2.3 (3) NB cw = 1 voor niet-voorgespannen constructies

        public double FactorK1DwarskrachtWeerstandBeton { get; } = 0.15;   // 6.2.2(1) De waarde van k1 moet gelijk aan 0,15 zijn genomen.

        [TableColumn(Label = "factor", Symbol = "<i>k</i>", StringFormat = "0.##")]
        public double FactorK { get { return this.SetFactorK(); } }
        public Formula FactorKFormula => new()
        {
            StaticValue = @"k = 1 + \sqrt{ \frac{200}{d} } \leq 2.0",
            DynamicValue = @$"= 1 + \sqrt{{ \frac{{200}} {{{NutHoogte.ToTeX()}}} }} \leq 2.0"
        };



        /// <summary>
        /// ρ~l~ verhouding aanwezige langswapening 
        /// </summary>
        [TableColumn(Symbol = $"<i>ρ</i><sub>l</sub>", Label = "verhouding langswapening")]
        public double RhoLangs { get { return this.SetRho1(); } }
        public Formula RhoLangsFormula => new()
        {
            StaticValue = @"\rho_{l} = \frac {A_{sl}} {b_wd} \leq 0.02",
            DynamicValue = @$"= \frac {{ {AsLangs.ToTeX()} }} {{ {Breedte.ToTeX()} \cdot {NutHoogte.ToTeX()}  }} \leq 0.02"
        };



        public double NEd { get { return Snedekrachten.Nx; } } // N<sub>Ed</sub>

        [TableColumn(Label = "spanning uit normaalkracht", Symbol = "<i>σ</i><sub>cp</sub>", Unit = "N/mm²", Article = "6.2.2 (1)")]
        public double SigmaCp { get { return Math.Min(NEd * 1000 / Profiel.Area, 0.2 * this.Beton.Fcd); } }
        public Formula SigmaCpFormula => new() { StaticValue = @"\sigma_{cp} = N_{Ed} / A_c < 0.2\;f_{cd}" };
        // sigma~cp~ = N<sub>Ed</sub> / Ac < 0,2 fcd   volgens art. 6.2.2 (1) 



        /// <summary>
        /// Minimale verhouding dwarskrachtwapening, ρ<sub>min</sub>
        /// NB. Hieruit volgt A~sw,min~
        /// </summary>
        [TableColumn(
            Symbol = "<i>ρ</i><sub>w,min</sub>",
            Label = "ondergrens dwarskrachtwapeningsverhouding")]
        public double RhoWMin { get { return this.SetRhoWMin(); } }

        [TableColumn(
            Label = "factor",
            Symbol = "C<sub>rdc</sub>",
            StringFormat = "0.##")]
        public double Crdc { get { return this.SetCrdc(); } }               // conform art. 6.4.4 (1) PONS

        [TableColumn(
            Label = "sterktereductiefactor",
            Symbol = "<i>ν</i>")]
        public double Nu { get { return this.SetNu(); } }

        [TableColumn(
            Label = "sterktereductiefactor",
            Symbol = "<i>ν</i><sub>1</sub>")]
        public double Nu1 { get { return this.SetNu1(); } }

        [TableColumn(
            Symbol = "<i>ν</i><sub>Rd,max</sub>",
            Label = "schuifspanningweerstand",
            Description = "is de maximale schuifspanningweerstand",
            Unit = "N/mm²"
            )]
        public double SchuifspanningWeerstandMax { get { return DwarskrachtWeerstandMax * 1000 / Breedte / NutHoogte; } }

        [TableColumn(
            Symbol = "<i>ν</i><sub>Rd,c</sub>",
            Label = "schuifspanningweerstand",
            Description = "is de schuifspanningweerstand zonder dwarskrachtwapening",
            Unit = "N/mm²")]
        public double SchuifspanningWeerstandBeton { get { return this.SetSchuifspanningWeerstandZonderDwarskrachtWapening(); } } // (6.2)


        [TableColumn(
            Symbol = "<i>v</i><sub>min</sub>",
            Label = "schuifspanningweerstand",
            Description = "is de minimale schuifspanningweerstand",
            Unit = "N/mm²")] // (6.2b)
        public double SchuifspanningMin { get { return this.SetSchuifspanningWeerstandZonderWapeningMin(); } } // (6.2b) minimale schuifspanning

        [TableColumn(
            Symbol = "<i>ν</i><sub>Rd,s</sub>",
            Label = "schuifspanningweerstand",
            Description = "is de schuifspanningweerstand door dwarskrachtwapening",
            Unit = "N/mm²")]
        public double SchuifspanningWeerstandStaal
        {
            get { return this.DwarskrachtWeerstandStaal * 1000 / Breedte / NutHoogte; }
        }

        //public double SchuifspanningWeerstandStaal { get { return  this.SetSchuifspanningWeerstandZonderWapeningMin(); } } // (6.2.b)


        /// <summary>
        /// vEd is de rekenwaarde van de schuifspanning in N/mm²
        /// </summary>
        [TableColumn(
            Symbol = "<i>ν</i><sub>Ed</sub>",
            Label = "schuifspanning (rekenwaarde)",
            Description = "is de rekenwaarde van de schuifspanning",
            Unit = "N/mm²")]
        public double SchuifspanningD { get { return Ved * 1000 / Breedte / NutHoogte; } }

        [TableColumn(Label = "bovengrens dwarskrachtweerstand",
            Symbol = "<i>V</i><sub>Rd,max</sub>", Unit = "kN")]
        public double DwarskrachtWeerstandMax { get { return this.SetVrdMax().value; } }
        public Formula DwarskrachtWeerstandMaxFormula
        {
            get
            {
                if (Alpha == 90)
                {
                    //VRd,max = αcw bw z ν1 fcd/ (cot θ + tan θ )
                    return new()
                    {
                        Name = "(6.9)",
                        StaticValue = "V_{Rd,max} = α_{cw} b_w z v_1 f_{cd} / (cot θ + tan θ )",
                        DynamicValue = $@"= {AlphaCw.ToTeX()} \cdot {Breedte.ToTeX()} \cdot {Z.ToTeX()} \cdot {Nu1.ToTeX()} \cdot {Beton.Fcd.ToTeX()} / ({CotTheta.ToTeX()}+{TanTheta.ToTeX()}) = {(DwarskrachtWeerstandMax * 1000).ToTeX(unit: "N", forcedExponent: 3)}"
                    };
                }
                else
                {
                    return new()
                    {
                        Name = "(6.14)",
                        StaticValue = "todo",
                        DynamicValue = "todo"
                    };
                }
            }
        }



        [TableColumn(Label = "dwarskrachtweerstand (rekenwaarde)",
            Symbol = "<i>V</i><sub>Rd,c</sub>",
            Unit = "kN",
            Description = "is de rekenwaarde van de dwarskrachtweerstand")]
        public double DwarskrachtWeerstandBeton { get { return SchuifspanningWeerstandBeton * Breedte * NutHoogte / 1000; } } // kN



        public Formula DwarskrachtWeerstandBetonFormula
        {
            get
            {
                if (SchuifspanningMin < SchuifspanningWeerstandBeton)
                {
                    // (6.2a) is gebruikt
                    return new()
                    {
                        Name = "(6.2a)",
                        StaticValue = @"V_{Rd,c} = \left[C_{Rd,c}k(100 ρ_l f_{ck})^{1/3} + k_1 σ_{cp} \right] b_wd ",
                        DynamicValue = $@"= \left[ {Crdc.ToTeX()} {FactorK.ToTeX()} (100 \cdot {RhoLangs.ToTeX()} \cdot {Beton.Fck.ToTeX()})^{{1/3}} + {FactorK1DwarskrachtWeerstandBeton.ToTeX()} \cdot {SigmaCp.ToTeX()} \right] \cdot {Breedte.ToTeX()} \cdot{NutHoogte.ToTeX()} = {DwarskrachtWeerstandBeton.ToTeX()}"
                    };
                }
                else
                {
                    // (6.2b) is gebruikt
                    return new()
                    {
                        Name = "(6.2b)",
                        StaticValue = @"V_{Rd,c} = (\nu_{min}+k_1\sigma_{cp})b_wd",
                        DynamicValue = $"= ({SchuifspanningMin.ToTeX()} + {FactorK1DwarskrachtWeerstandBeton.ToTeX()}\\cdot {SigmaCp.ToTeX()}) {Breedte.ToTeX()}\\cdot{NutHoogte.ToTeX()} = {DwarskrachtWeerstandBeton.ToTeX()}"
                    };
                }
            }
        }

        private double? _dwarskrachtWeerstandStaal; // backing-field om gebruikersinvoer te bewaren


        [TableColumn(
            Label = "dwarskrachtweerstand door dwarskrachtwapening",
            Symbol = "V<sub>Rd,s</sub>",
            Unit = "kN")]
        public double DwarskrachtWeerstandStaal
        {

            get => _dwarskrachtWeerstandStaal ?? this.SetVrds().vrds;
            internal set
            {
                if (_dwarskrachtWeerstandStaal != value)
                {
                    _dwarskrachtWeerstandStaal = this.SetVrds().vrds; // Get the current value from the SetVrds method
                }
            }
        }

        [TableColumn(Label = "dwarskrachtweerstand", Symbol = "<i>V</i><sub>Rd</sub>", Unit = "kN")]
        public double DwarskrachtWeerstand { get { return Math.Min(Math.Max(DwarskrachtWeerstandStaal, DwarskrachtWeerstandBeton), DwarskrachtWeerstandMax); } }
        public Formula DwarskrachtWeerstandFormula
        {
            get
            {
                if (DwarskrachtWeerstandMax < DwarskrachtWeerstand)
                {
                    return new()
                    {
                        StaticValue = "V_{Rd} = min(V_{Rd} ; V_{Rd,max})",
                        DynamicValue = $"= min({Math.Max(DwarskrachtWeerstandStaal, DwarskrachtWeerstandBeton).ToTeX()};{DwarskrachtWeerstandMax.ToTeX()}) = {DwarskrachtWeerstand.ToTeX()}"
                    };
                }
                else
                {
                    return new()
                    {
                        Name = "(6.1)",
                        StaticValue = "V_{Rd} = V_{Rd,s} + V_{ccd} + V_{td}",
                        DynamicValue = $"= {DwarskrachtWeerstandStaal.ToTeX()} + {Vccd} + {Vtd} = {DwarskrachtWeerstand.ToTeX()} "
                    };
                }
            }
        }

        private readonly double Vccd = 0;
        private readonly double Vtd = 0;

        public bool BerekeningVrd { get; set; } = false;    // bool om aan te geven of we beugels berekenen, of de Vrd bepalen. 

        /// <summary>
        /// is de inwendige hefboomsarm voor een element met constante hoogte, overeenkomend met het
        /// buigend moment in het beschouwde element. In de dwarskrachtberekening van de gewapend beton
        /// zonder normaalkracht mag in het algemeen de benaderende waarde z = 0,9d zijn gebruikt.
        /// </summary>
        [TableColumn(
            Label = "inwendige hefboomsarm",
            Symbol = "<i>z</i>",
            Unit = "mm")]
        public double Z
        {
            get
            {
                switch (MethodeVoorBerekenenZ)
                {
                    default:
                    case MethodeVoorBerekenenInwendigeHefboomsArmEnum.ViaNuttigeHoogte:
                        return 0.9 * NutHoogte;
                    case MethodeVoorBerekenenInwendigeHefboomsArmEnum.ViaMomentRekenwaarde:
                    case MethodeVoorBerekenenInwendigeHefboomsArmEnum.ViaMomentOpneembaar:
                        throw new NotImplementedException("Deze methode is niet ondersteund");




                }
            }
        }

        public MethodeVoorBerekenenInwendigeHefboomsArmEnum MethodeVoorBerekenenZ { get; set; }


        [TableColumn(Label = "minimale dwarskrachtwapening",
            Symbol = "<i>A</i><sub>sw,min</sub>",
            Unit = "mm²/m")]
        public double AswMin { get { return this.SetAswMin().value; } }

        [TableColumn(Label = "berekende dwarskrachtwapening",
            Symbol = "<i>A</i><sub>sw,ber</sub>",
            Unit = "mm²/m")]
        public double AswBerekend { get { return this.SetAswBerekend().value; } }

        public enum MethodeVoorBerekenenInwendigeHefboomsArmEnum
        {
            [Description("0,9 * d")]
            ViaNuttigeHoogte,
            [Description("M<sub>Ed</sub>")]
            ViaMomentRekenwaarde,
            [Description("M~Rd~")]
            ViaMomentOpneembaar,

        }



        [TableColumn(Label = "benodigde dwarskrachtwapening", Symbol = "<i>A</i><sub>sw,ben</sub>", Unit = "mm²/m")]
        public double AswBenPerMeter
        {
            get { return Math.Max(AswMin, AswBerekend); }
        }



        protected override void Bereken()
        {
            // nalopen

        }

        protected override bool Valideer()
        {
            if (Ved < DwarskrachtWeerstandBeton)
            {
                // AddMeldingOpmerking("dwarskracht kleiner dan ");
            }

            if (Ved > DwarskrachtWeerstand)
            {
                AddMeldingError($"dwarskracht niet akkoord (V<sub>Ed</sub> > V<sub>Rd</sub>) {(Ved / DwarskrachtWeerstand):0.##}");
                return false;
            }

            if (Ved > DwarskrachtWeerstandMax)
            {
                AddMeldingError($"dwarskracht niet akkoord (V<sub>Ed</sub> > V<sub>Rd,max</sub>) {(Ved / DwarskrachtWeerstandMax):0.##}");
                return false;
            }
            return true;
        }
    }
}
