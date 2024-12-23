namespace Eurocode.BetonConstructies
{
    public class BendingResults
    {
        public BendingResults()
        {
            Beton = new();
        }

        public BendingResults(BetonContext beton, double b, double h, double d, double m)
        {
            Beton = beton;
            B = b;
            H = h;
            D = d;
            M = m;
        }

        public BetonContext Beton { get; set; }
        public double B { get; set; } = 300;
        public double H { get; set; } = 400;
        public double D { get; set; } = 350;
        public double M { get; set; } = 50;

        public double Xu
        {
            get
            {
                return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * Math.Abs(M) * 1000000.0 / (Beton.GetAlpha() * B * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta());
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
