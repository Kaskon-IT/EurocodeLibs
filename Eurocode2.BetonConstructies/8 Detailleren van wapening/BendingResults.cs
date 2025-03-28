using CommonLibrary;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    public class BuigingGedrongen
    {
        public double M { get; set; }
        //public double 
    }



    public class BendingResults : BaseEurocodeContext
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

        [TableColumn("Positie", order: 0)]
        public string Name { get; set; } = "";


        [TableColumn("M~Ed~", StringFormat = "0.0 kNm", Order = 1)]
        public double M { get; set; } = 50;


        [TableColumn("Breedte", order: 2, StringFormat = "0 mm")]
        public double B { get; set; } = 300;

        [TableColumn("Hoogte", order: 3, StringFormat = "0 mm")]
        public double H { get; set; } = 400;

        public double ZRef { get; set; } = 50;

        [TableColumn("d", order: 4, StringFormat = "0 mm")]
        public double D { get { return H - ZRef; } }


        [TableColumn("x~u~", order: 5, StringFormat = "0.## mm")]
        public double Xu
        {
            get
            {
                return (D - Math.Pow(D * D - 4.0 * Beton.GetBeta() * Math.Abs(M) * 1000000.0 / (Beton.GetAlpha() * B * Beton.Fcd), 0.5)) / (2.0 * Beton.GetBeta());
            }
        }

        public double XuMax
        {
            get
            {
                double betonDsnOpp = B * H;
                return BuigingContext.GetMaximaleHoogteDrukzoneZonderVoorspanning(Beton, D, betonDsnOpp);
            }
        }

        public double XuDMax
        {
            get
            {
                return XuMax / D;
            }
        }


        public double XuD
        {
            get { return Xu / D; }
        }

        [TableColumn("z", order: 21, StringFormat = "0.# mm")]
        public double Z
        {
            get
            {
                return D - Beton.GetBeta() * Xu;
            }
        }

        [TableColumn("A~s,ben~", Order = 40, StringFormat = "0 mm²")]
        public double AsRequired
        {
            get
            {
                return Math.Max(AsMin, AsBerekend);
            }
        }

        [TableColumn("A~s,toe~", Order = 41, StringFormat = "0 mm²")]
        public double AsApplied
        {
            get { return Math.Ceiling(AsRequired); }
        }


        public WapeningContext Wapening { get; set; }


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
                return Math.Abs(M) / (Z / 1000);
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





        public bool MinimaleWapeningToegepast
        {
            get
            {
                return AsMin > AsBerekend;
            }
        }

        public override bool IsAkkoord()
        {
            if (Xu > XuMax)
            {
                AddMeldingWaarschuwing("hoogte drukzone niet akkoord");
                Meldingen.Add(new(MeldingType.Waarschuwing, "overschrijding maximale hoogte drukzone"));
                return false;
            }


            return true;
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
