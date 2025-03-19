using CommonLibrary;
using ExportFactory.MigraDocContentModels;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    public enum BerekeningTypeEnum
    {
        BepaalBenodigeWapening,
        ControleerWapening,
    }

    public class DwarskrachtWapContext
    {
        // context voor de dwarskrachtwapening volgens art. 6.2
        // Uitgangspunten voor niet-voorgespannen constructies

        //private List<int> _meldingen { get; set; } = [];
        //private Dictionary<int, Melding> _betonMeldingen = new MeldingenBeton().Meldingen;

        public List<Melding> Meldingen { get; set; } = [];
        public List<string> Artikelen { get; set; } = [];

        public BerekeningTypeEnum BerekeningType { get; set; } = BerekeningTypeEnum.BepaalBenodigeWapening;


        [TableColumn("Opm.", "Opmerkingen")]
        public string MeldingenUserFriendlyName { get { return string.Join(",", Meldingen); } }

        //[TableColumn("Art.", "Artikelen")]
        public string GebruikteArtikelenUserFriendlyName { get { return string.Join(",", Artikelen); } }

        //public WringingWapContext WringWap { get; set; } // voor berekeningen met dwarskracht EN wringing (als onderdeel van de dwarskrachtWap)
        //public MeldingenBeton MeldingenBeton = new MeldingenBeton(); // voor meldingen en foutmeldingen uit de betonModule

        // Vanuit het beton
        //public double Fck { get; set; }

        public BetonContext Beton { get; set; }

        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; }


        /// <summary>
        /// Rekenwaarde van de dwarskracht in kN
        /// </summary>
        [TableColumn("V~Ed~", StringFormat = "0.##\tkN")]
        public double Ved { get; set; }

        /// <summary>
        /// is de hoek in graden tussen de drukdiagonaal van beton en de as van de ligger loodrecht op de dwarskracht;
        /// </summary>
        [TableColumn("|theta|", StringFormat = "0.##\t°")]

        public double Theta { get; set; }

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
        [TableColumn("cot |theta|", StringFormat = "0.##")]

        public double CotTheta { get { return 1 / TanTheta; } }

        /// <summary>
        /// is de hoek tussen de dwarskrachtwapening en de as van de ligger loodrecht op de dwarskracht (positief gemeten zoals getoond in figuur 6.5) in graden;
        /// </summary>
        [TableColumn("|alpha|", StringFormat = "0.##\t°")]

        public double Alpha { get; set; } = 90;  // hoek van de dwarskrachtwapning standaard 90 graden
        [TableColumn("tan |alpha|", StringFormat = "0.##")]

        private double TanAlpha { get { return Math.Tan(Alpha * Math.PI / 180); } }

        //[TableColumn("cot |alpha|", StringFormat = "0.##")]

        public double CotAlpha { get { return 1 / TanAlpha; } }


        /// <summary>
        /// Breedte van de doorsnede voor de dwarskracht in mm
        /// </summary>
        [TableColumn("b")]

        public double Breedte
        {
            get { return Profiel.BreedteDwarskracht; }
        }

        /// <summary>
        /// is de minimale breedte tussen de trek- en drukrand in mm²
        /// </summary>
        [TableColumn("A~sl~")]
        public double AsLangs { get; set; }

        /// <summary>
        /// Nuttige hooge (d) van de dwarskrachtdoorsnede in mm
        /// </summary>
        [TableColumn("d")]

        public double NutHoogte { get; set; }



        // --- Toegepaste Wapening
        public double BeugelDiameter;
        public double BeugelHartOpHartAfstand;
        public double BeugelSnedeAantal;
        public double BeugelAfstandDwarsToegepast;
        public double DekkingZijkantToegepast;

        public DwarskrachtWapContext(BetonContext beton, ParametrischeProfielen.ParametrischProfielContext profiel, double ved)
        {
            Beton = beton; // materiaal, staal, dekking, etcetera
            Profiel = profiel; // geometrie 
            Ved = ved; // krachten
        }



        // vanuit het betonstaal
        public double Fywk { get; set; } // 
        public double Fywd { get; set; } // OPMERKING Indien vergelijking (6.10) gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot 0,8fywk;

        //public double AswToegepast { get; set; }

        private double? _aswToegepast; // backing-field om gebruikersinvoer te bewaren

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
            set
            {
                // Sta gebruikersinvoer toe
                _aswToegepast = value;
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



        [TableColumn("|alpha|~cw~", Weergave = WeergaveEnum.AlleTabellen)]
        public double AlphaCw { get; } = 1; // art. 6.2.3 (3) NB cw = 1 voor niet-voorgespannen constructies

        [TableColumn("k1", Weergave = WeergaveEnum.AlleTabellen)]
        public double FactorK1DwarskrachtWeerstandBeton { get; } = 0.15;   // 6.2.2(1) De waarde van k1 moet gelijk aan 0,15 zijn genomen.



        [TableColumn("k", StringFormat = "0.##")]
        public double FactorKDwarskrachtWeerstandBeton { get { return this.SetFactorK(); } }
        [TableColumn("|rho|~1~")]
        public double Rho1 { get { return this.SetRho1(); } }


        [TableColumn("|rho|~min~", StringFormat = "0.##")]
        public double RhoWMin { get { return this.SetRhoWMin(); } }



        [TableColumn("C~rdc~", StringFormat = "0.##")]
        public double Crdc { get { return this.SetCrdc(); } }               // conform art. 6.4.4 (1) PONS

        [TableColumn("|nu|", StringFormat = "0.##\t-")]
        public double SterkteReductieFactorBetonGescheurdDoorDwarskracht { get { return this.SetSterkteReductieFactorBetonGescheurdDoorDwarskracht(); } }


        [TableColumn("|nu|~1~", StringFormat = "0.##\t-")]
        public double SterkteReductieFactorBetonGescheurdDoorDwarskracht1 { get { return this.SetSterkteReductieFactorBetonGescheurdDoorDwarskracht1(); } }


        [TableColumn("|nu|~Rd,max~", StringFormat = "0.##\tN/mm²")]
        public double SchuifspanningWeerstandMax { get { return DwarskrachtWeerstandMax * 1000 / Breedte / NutHoogte; } }

        [TableColumn("|nu|~Rd,c~", StringFormat = "0.##\tN/mm²")]
        public double SchuifspanningWeerstandBeton { get { return this.SetSchuifspanningWeerstandZonderDwarskrachtWapening(); } } // (6.2.a)

        [TableColumn("|nu|~Rd,s~", StringFormat = "0.##\tN/mm²")]
        public double SchuifspanningWeerstandStaal { get { return this.SetSchuifspanningWeerstandZonderWapeningMin(); } } // (6.2.b)


        /// <summary>
        /// vEd is de rekenwaarde van de schuifspanning in N/mm²
        /// </summary>
        [TableColumn("|nu|~Ed~", StringFormat = "0.##\tN/mm²")]
        public double SchuifspanningD { get { return Ved * 1000 / Breedte / NutHoogte; } }

        [TableColumn("V~Rd,max~", StringFormat = "0.##\tkN")]
        public double DwarskrachtWeerstandMax { get { return this.SetVrdMax().value; } }

        [TableColumn("V~Rd,c~", StringFormat = "0.##\tkN")]
        public double DwarskrachtWeerstandBeton { get { return Math.Max(SchuifspanningWeerstandBeton, SchuifspanningWeerstandStaal) * Breedte * NutHoogte / 1000; } } // kN

        [TableColumn("V~Rd,s~", StringFormat = "0.##\tkN")]
        public double DwarskrachtWeerstandStaal { get { return this.SetVrds().value; } }

        [TableColumn("V~Rd~", StringFormat = "0.##\tkN")]
        public double DwarskrachtWeerstand { get { return Math.Min(DwarskrachtWeerstandStaal, DwarskrachtWeerstandMax); } }



        public bool BerekeningVrd { get; set; } = false;    // bool om aan te geven of we beugels berekenen, of de Vrd bepalen. 

        /// <summary>
        /// is de inwendige hefboomsarm voor een element met constante hoogte, overeenkomend met het
        /// buigend moment in het beschouwde element. In de dwarskrachtberekening van de gewapend beton
        /// zonder normaalkracht mag in het algemeen de benaderende waarde z = 0,9d zijn gebruikt.
        /// </summary>
        [TableColumn("z", StringFormat = "0.##\tmm")]
        public double Z { get; set; }

        [TableColumn("A~sw,min~", StringFormat = "0.##\tmm²/m")]
        public double AswMin { get { return this.SetAswMin().value; } }

        [TableColumn("A~sw,ber~", StringFormat = "0.##\tmm²/m")]
        public double AswBerekend { get { return this.SetAswBerekend().value; } }

        [TableColumn("A~sw,ben~", StringFormat = "0\tmm²/m")]
        public double AswBenPerMeter
        {
            get { return Math.Max(AswMin, AswBerekend); }


        }
    }
}
