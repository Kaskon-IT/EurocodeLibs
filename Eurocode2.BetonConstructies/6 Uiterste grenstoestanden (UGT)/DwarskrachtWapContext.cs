using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    public class DwarskrachtWapContext
    {
        // context voor de dwarskrachtwapening volgens art. 6.2
        // Uitgangspunten voor niet-voorgespannen constructies




        //public WringingWapContext WringWap { get; set; } // voor berekeningen met dwarskracht EN wringing (als onderdeel van de dwarskrachtWap)
        //public MeldingenBeton MeldingenBeton = new MeldingenBeton(); // voor meldingen en foutmeldingen uit de betonModule

        // Vanuit het beton
        public double Fck { get; set; }

        public required BetonContext Beton { get; set; }




        /// <summary>
        /// Rekenwaarde van de dwarskracht in kN
        /// </summary>
        public double Ved { get; set; }

        /// <summary>
        /// is de hoek in graden tussen de drukdiagonaal van beton en de as van de ligger loodrecht op de dwarskracht;
        /// </summary>
		public double Theta { get; set; }

        /// <summary>
        /// hoek drukdiagonaal in radialen
        /// </summary>
        public double GetThetaRadialen { get { return Theta * Math.PI / 180; } }

        /// <summary>
        /// tangens van de hoek drukdiagonaal
        /// </summary>
        public double TanTheta { get { return Math.Tan(Theta * Math.PI / 180); } }

        /// <summary>
        /// CoTangens van de hoek drukdiagonaal
        /// </summary>
        public double CotTheta { get { return 1 / TanTheta; } }

        /// <summary>
        /// is de hoek tussen de dwarskrachtwapening en de as van de ligger loodrecht op de dwarskracht (positief gemeten zoals getoond in figuur 6.5) in graden;
        /// </summary>
        public double Alpha { get; set; } = 90;  // hoek van de dwarskrachtwapning standaard 90 graden

        /// <summary>
        /// Breedte van de doorsnede voor de dwarskracht in mm
        /// </summary>
        public double Breedte { get; set; }

        /// <summary>
        /// is de minimale breedte tussen de trek- en drukrand in mm²
        /// </summary>
        public double AsLangs { get; set; }

        /// <summary>
        /// Nuttige hooge (d) van de dwarskrachtdoorsnede in mm
        /// </summary>
        public double NutHoogte { get; set; }



        // --- Toegepaste Wapening
        public double BeugelDiameter;
        public double BeugelHartOpHartAfstand;
        public double BeugelSnedeAantal;
        public double BeugelAfstandDwarsToegepast;
        public double DekkingZijkantToegepast;



        // vanuit het betonstaal
        public double Fywk { get; set; } // 
        public double Fywd { get; set; } // OPMERKING Indien vergelijking (6.10) gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot 0,8fywk;

        public double AswToegepast { get; set; }
        public double BeugelAfstandMaxLangs { get; set; }
        public double BeugelAfstandMaxDwars { get; set; }
        public double SpanningDwarskrachtWapening
        {
            get
            {
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




        //public List<BeugelWap> LijstBeugelWap { get; set; }



        [TableColumn("|alpha~cw~|", Weergave = WeergaveEnum.Geen)]
        public double AlphaCw { get; } = 1; // art. 6.2.3 (3) NB cw = 1 voor niet-voorgespannen constructies

        [TableColumn("k1", Weergave = WeergaveEnum.Geen)]
        public double K1 { get; } = 0.15;   // 6.2.2(1) De waarde van k1 moet gelijk aan 0,15 zijn genomen.




        public double FactorK { get; set; }
        public double Rho1 { get; set; }
        public double RhoMin { get; set; }




        public double Crdc { get; set; }               // conform art. 6.4.4 (1) PONS

        public double SterkteReductieV { get; set; }



        public double SterkteReductieV1
        {
            get
            {
                return 1.00; // todo op juiste plek zetten
                //return DwarskrachtWap.GetSterkteReductieV1(Beton.Fck, SpanningDwarskrachtWapening, Beton.BetonStaal.Fyk);
            }
        }



        public double SchuifspanningMax { get { return VrdMax * 1000 / Breedte / NutHoogte; } }
        public double SchuifspanningWeerstandZonderDwarskrachtWapening { get; set; } // (6.2.a)
        public double SchuifspanningWeerstandZonderDwarskrachtWapeningMin { get; set; } // (6.2.b)


        /// <summary>
        /// vEd is de rekenwaarde van de schuifspanning in N/mm²
        /// </summary>
        public double SchuifspanningD { get { return Ved * 1000 / Breedte / NutHoogte; } }

        public double VrdMax { get; set; }
        public double VrdC { get { return Math.Max(SchuifspanningWeerstandZonderDwarskrachtWapening, SchuifspanningWeerstandZonderDwarskrachtWapeningMin) * Breedte * NutHoogte / 1000; } } // kN
        public double VrdS { get; set; }
        public double Vrd { get; set; }
        public bool BerekeningVrd { get; set; } = false;    // bool om aan te geven of we beugels berekenen, of de Vrd bepalen. 

        /// <summary>
        /// is de inwendige hefboomsarm voor een element met constante hoogte, overeenkomend met het
        /// buigend moment in het beschouwde element. In de dwarskrachtberekening van de gewapend beton
        /// zonder normaalkracht mag in het algemeen de benaderende waarde z = 0,9d zijn gebruikt.
        /// </summary>
        public double Z { get; set; }

        public double AswMin { get; set; }

        public double AswBerekend { get; set; }

        public double AswBenPerMeter { get; set; }


    }
}
