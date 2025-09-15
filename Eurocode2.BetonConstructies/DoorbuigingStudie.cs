using CommonLibrary;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    public class DoorbuigingStudie : BaseEurocodeContext
    {
        private readonly BetonContext _beton;
        public Snedekrachten Krachten { get; set; } = new();
        public BetonContextKruipEnKrimpCalculator KruipKrimp { get; set; }
        public BetonDoorbuigingCalculator Doorbuiging { get; set; }
        public ParametrischeProfielen.ParametrischProfielContext Profiel = new() { Breedte = 500, Hoogte = 700 };


        public DoorbuigingStudie(double l = 10, double q = 40)
        {
            L = l;
            Lijnlast = q;
            Profiel = new(500, 700);





            _beton = new(BetonsterkteklasseEnum.C30_37)
            {
                Profiel = new(500, 700),
            };
            KruipKrimp = new BetonContextKruipEnKrimpCalculator(_beton);

            Doorbuiging = new BetonDoorbuigingCalculator(_beton, KruipKrimp, Krachten)
            {

            };
        }







        // rekenvoorbeeld
        // uitgangspunten
        //overspanning: l = 10 m
        //totale hoogte doorsnede: h = 700 mm
        //nuttige hoogte doorsnede: d = 650 mm
        //quasi-blijvend aanwezige belasting(de blijvende belasting plus
        //het quasi-blijvende deel van de veranderlijke belasting) :
        //qEqp = qG,k + ψ2
        //qQ, k = 40 kN/m
        //betonsterkteklasse: C30/37
        //betonstaal: As = 2945 mm2
        // kruipcoëfficiënt φ(∞, t 0) = 2,0
        // vrije krimpvervorming εcs = 415 · 10-6

        [TableColumn(Label = "lengte overspanning", Symbol = "L", Unit = "m")]
        public double L { get; set; } = 10; //m


        [TableColumn(Label = "hoogte", Description = "Hoogte van het profiel", Symbol = "h", Unit = "mm")]
        public double Hoogte
        {
            get { return Profiel.Hoogte; }
            set { Profiel.Hoogte = value; }
        }

        [TableColumn(Label = "breedte", Description = "Breedte van het profiel", Symbol = "b", Unit = "mm")]

        public double Breedte
        {
            get { return Profiel.Breedte; }
            set { Profiel.Breedte = value; }
        }

        //public double Hoogte = 700;
        //public double Breedte = 500;



        [TableColumn(Label = "nuttige hoogte", Description = "Nuttige hoogte, positie wapening", Symbol = "d", Unit = "mm")]
        public double NuttigeHoogte { get; set; } = 650;


        [TableColumn(Label = "lijnlast", Description = "lijnlast q in kN/m¹", Symbol = "q", Unit = "kN/m¹")]
        public double Lijnlast { get; set; } = 10; // kN/m


        //double qEqp = 40; // kN/m
        //double NuttigeHoogte = 650;

        [TableColumn(Label = "MomentEqp", Description = "Moment", Symbol = "M<sub>Eqp</sub>", Unit = "Nmm")]
        public double MomentEqp
        {
            get
            {

                this.Krachten.My.Kar = Lijnlast * Math.Pow(L, 2) / 8.0;
                return this.Krachten.My.Kar;

            }
        }




        //WapeningContext Wapening = new() { Tekst = "6Ø25" }; // 2945 mm2








        //public BetonContextKruipEnKrimpCalculator KruipEnKrimp = new(_beton);


        // nieuwe waarden










        public override bool IsAkkoord()
        {
            return true;
        }

        protected override void Bereken()
        {

            Console.WriteLine();

        }

        protected override bool Valideer()
        {
            return true;
        }
    }
}
