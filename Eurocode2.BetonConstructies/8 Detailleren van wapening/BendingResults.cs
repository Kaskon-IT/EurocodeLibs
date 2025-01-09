namespace Eurocode.BetonConstructies
{
    public class BendingResults
    {
        public BendingResults()
        {
            Beton = new();
        }

        public BendingResults(BetonContext beton, double b, double h, double zRef, double m)
        {
            Beton = beton;
            B = b;
            H = h;
            ZRef = zRef;
            M = m;
        }

        public BetonContext Beton { get; set; }
        public double B { get; set; } = 300;
        public double H { get; set; } = 400;
        public double ZRef { get; set; } = 50;
        public double D { get { return H - ZRef; } }
        public double M { get; set; } = 50;

        public double Xu
        {
            get
            {
                return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * Math.Abs(M) * 1000000.0 / (Beton.GetAlpha() * B * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta());
            }
        }

        public double XuD
        {
            get { return Xu / D; }
        }

        public double Z
        {
            get
            {
                return D - Beton.GetBeta() * Xu;
            }
        }

        public double AsApplied
        {
            get
            {
                return AsRequired;
            }
        }


        public double SigmaS
        {
            get
            {
                return Ns * 1000 / AsApplied;
            }
        }

        public double Ns
        {
            get
            {
                return M / (Z / 1000);
            }
        }



        public double AsMin1
        {
            get { return Beton.GetAlpha() * B * XeMin * Beton.Fcd / Beton.BetonStaal.Fyd; }
        }
        public double AsMin2
        {
            get
            {
                return 1.25 * AsBerekend;
            }
        }
        public double AsMin
        {
            get
            {
                return Math.Min(AsMin1, AsMin2);
            }
        }

        public double AsBerekend
        {
            get
            {
                return Beton.GetAlpha() * B * Xu * Beton.Fcd / Beton.BetonStaal.Fyd;
            }
        }

        public double Iy
        {
            get
            {
                return B * H * H / 6.0;
            }
        }

        public double MeMin { get { return Beton.Fctm * Iy / 1000 / 1000; } }
        public double XeMin { get { return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * MeMin * 1000000.0 / (Beton.GetAlpha() * B * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta()); } }


        public double AsRequired
        {
            get
            {
                return Math.Max(AsMin, AsBerekend);
            }
        }

        public bool MinimaleWapeningToegepast
        {
            get
            {
                return AsMin > AsBerekend;
            }
        }



    }




}
