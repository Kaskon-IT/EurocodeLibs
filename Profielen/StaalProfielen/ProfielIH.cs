namespace StaalProfielen
{
    /// <summary>
    /// Voor I-profielen en H-profielen.
    /// Bron Arcelor Mittel.
    /// 
    /// </summary>
    public class ProfielIH
    {
        public string Naam { get; set; } = "";
        
        /// <summary>
        /// Hoogte van het profiel in mm
        /// </summary>
        public double H { get; set; }

        /// <summary>
        /// Breedte van het profiel in mm
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Dikte van het lijf in mm
        /// </summary>
        public double Tw { get; set; }

        /// <summary>
        /// Dikte van de flens in mm
        /// </summary>
        public double Tf { get; set; }

        /// <summary>
        /// Straal tussen flens en lijf in mm.
        /// </summary>
        public double R { get; set; }

        /// <summary>
        /// Gewicht in kilogram per strekkende meter. (kg/m¹)
        /// </summary>
        public double G { get { return A / 1000000 * Materiaal.VolumeGewicht; } }
        /// <summary>
        /// Doorsnede Oppervlakte voor I of H profielen, (bron ArcelorMittal)
        /// /// </summary>
        public double A { get { return 2 * Tf * B + (H - 2 * Tf) * Tw + (4 - Math.PI) * R * R; ; } }
        
        
        /// <summary>
        /// Interne hoogte (overbodig?)
        /// </summary>
        public double HoogteIntern { get { return H - 2 * Tf; } }
        
        
        private double D { get { return H - 2 * (Tf + R); } }
        
        public double Ø { get; set; }

        /// <summary>
        /// Traaheidsmoment in de y-as (sterke richting)
        /// </summary>
        public double Iy { get { return 1.0 / 12.0 * (B * H * H * H - (B - Tw) * Math.Pow((H - 2 * Tf), 3)) + 0.03 * Math.Pow(R, 4) + 0.2146 * Math.Pow(R, 2) * Math.Pow((H - 2 * Tf - 0.4468 * R), 2); } }
        
        /// <summary>
        /// Traagheidsmoment in de z-as (zwakke richting)
        /// </summary>
        public double Iz { get { return 1.0 / 12.0 * (2 * Tf * B * B * B + (H - 2 * Tf) * Math.Pow(Tw, 3)) + 0.03 * Math.Pow(R, 4) + 0.2146 * Math.Pow(R, 2) * Math.Pow((Tw + 0.4468 * R), 2); } }

        /// <summary>
        /// Torsie traagheid (Bron ArcelorMittal) 
        /// </summary>
        public double It
        {
            get
            {
                var deel1 = -0.042 + 0.2204 * Tw / Tf + 0.1355 * R / Tf - 0.0865 * (R * Tw / Math.Pow(Tf, 2)) - 0.0725 * Math.Pow(Tw / Tf, 2);
                var deel2 = (Math.Pow(R + Tw / 2.0, 2) + Math.Pow(R + Tf, 2) - Math.Pow(R, 2)) / (2 * R + Tf);
                return 2.0 / 3.0 * (B - 0.64 * Tf) * Math.Pow(Tf, 3) + 1.0 / 3.0 * (H - 2 * Tf) * Math.Pow(Tw, 3) + 2 * (deel1) * Math.Pow(deel2, 4);
            }
        }



        public double Iw { get { return (Tf * B * B * B) / 24.00 * Math.Pow((H - Tf), 2); } }
        
        /// <summary>
        /// Elastisch Weerstandsmoment in de sterke-as
        /// </summary>
        public double WelY { get { return (2 * Iy) / H; } }
        
        /// <summary>
        /// Elastisch Weerstandsmoment in de zwakke-as
        /// </summary>
        public double WelZ { get { return (2 * Iz) / B; } }

        /// <summary>
        /// Plastishc WeerstandsMoment in sterke-as
        /// </summary>
        public double WplY { get { return (Tw * H * H) / 4.0 + (B - Tw) * (H - Tf) * Tf + (4 - Math.PI) / 2.0 * R * R * (H - 2 * Tf) + (3 * Math.PI - 10) / 3.0 * R * R * R; } }

        /// <summary>
        /// Plastisch Weerstandsmoment in de zwakke-as
        /// </summary>
        public double WplZ { get { return (B * B * Tf) / 2.0 + (H - 2 * Tf) / 4.0 * Tw * Tw + R * R * R * (10.0 / 3.0 - Math.PI) + (2.0 - Math.PI / 2.0) * Tw * R * R; } }

        /// <summary>
        /// Traagheidsstraal in sterke richting.
        /// </summary>
        public double TraagheidsStraaliY { get { return Math.Sqrt((Iy / A)); } }

        /// <summary>
        /// Traagheidsstraal in de zwakke richting.
        /// </summary>
        public double TraagheidsStraaliZ { get { return Math.Sqrt((Iz / A)); } }
        
        /// <summary>
        /// Doorsnede Opp. voor dwarskracht voor I of H profielen, born ArcelorMittal
        /// </summary>
        public double Avz { get { return A - 2 * B * Tf + (Tw + 2 * R) * Tf; } }

        /// <summary>
        /// Classificatie van doorsneden
        /// klasse 1 (plastische doorsneden)
        /// klasse 2 (gedrongen doorsneden)
        /// klasse 3 (semigedrongen doorsneden)
        /// klasse 4 (slanke doorsneden)
        /// </summary>
        public int KlasseBuiging { get { return GetKlasseBuigingInwendigeOpDrukBelasteOnderdelen(D, Tw, EpsilonVoorKlasse); } }
       
        private double FyVoorKlasse { get; set; } = 235; // default
        private double EpsilonVoorKlasse { get { return Math.Sqrt((235.0 / FyVoorKlasse)); } }


        public static int GetKlasseBuigingInwendigeOpDrukBelasteOnderdelen(double c, double t, double epsilon)
        {
            if (c / t <= 72 * epsilon) return 1;
            else if (c / t <= 83 * epsilon) return 2;
            else if (c / t < 124 * epsilon) return 3;
            else return 4;
        }

                

        /// <summary>
        /// Length of stiff bearing
        /// The length of stiff bearing on the flange is the distance over which an applied force is effectively distributed.It influences the resistance of the unstiffened web of an adjacent section to transverse forces. 
        /// </summary>
        public double Ss { get { return Tw + 2 * Tf + (4 - 2 * Math.Sqrt(2)) * R; } }
        public double Lt { get; set; }
        public double Lw { get; set; }


    }



}
