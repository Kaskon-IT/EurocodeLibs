using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Een voorbeeld voor gecombineerde eurocode toepassingen.
    /// 
    /// </summary>
    public class DoorbuigingStudie : BaseEurocodeContext
    {
        private readonly BetonContext _beton;
        public Snedekrachten Krachten { get; set; } = new();
        public BetonContextKruipEnKrimpCalculator KruipKrimpBerekening { get; set; }
        public BetonDoorbuigingCalculator DoorbuigingBerekening { get; set; }
        public ParametrischeProfielen.ParametrischProfielContext Profiel = new() { Breedte = 500, Hoogte = 700 };
        public WapeningContext Wapening { get; set; } = new() { Tekst = "6Ø25" };
        public BendingResults BuigingBerekening { get; set; }

        public DoorbuigingStudie(double l = 10, double q = 40)
        {
            L = l;
            Lijnlast = q;
            Profiel = new(500, 700);
            _beton = new(BetonsterkteklasseEnum.C30_37) { Profiel = this.Profiel };

            KruipKrimpBerekening = new BetonContextKruipEnKrimpCalculator(_beton);
            DoorbuigingBerekening = new BetonDoorbuigingCalculator(_beton, Wapening, KruipKrimpBerekening, Krachten, Profiel, this);
            BuigingBerekening = new BendingResults(_beton, Profiel, Wapening, Krachten);
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


        [TableColumn(Label = "hoogte profiel", Symbol = "h", Unit = "mm")]
        public double Hoogte
        {
            get { return Profiel.Hoogte; }
            set { Profiel.Hoogte = value; }
        }

        [TableColumn(Label = "breedte profiel", Symbol = "b", Unit = "mm")]

        public double Breedte
        {
            get { return Profiel.Breedte; }
            set { Profiel.Breedte = value; }
        }

        [TableColumn(Label = "wapening (tekst)",
            Description = "Voor opgave wapening gebruik bijvoorbeeld: " +
            "<br />6r25 (voor 6 staven Ø25)" +
            "<br />r8-200 (voor staven Ø8 hoh 200mm)" +
            "<br />4r20+2r25 (voor meerdere groepen)")]
        public string WapeningTekst
        {
            get { return Wapening.Tekst; }
            set { Wapening.Tekst = value; }
        }

        [TableColumn(Label = "wapening (mm²)", Symbol = "<i>A<i><sub>s,toe</sub>", Unit = "mm²")]
        public double WapeningAsApplied
        {
            get { return Wapening.As; }
        }

        //public double Hoogte = 700;
        //public double Breedte = 500;

        [TableColumn(Label = "referentie afstand",
            Description = "is een referentie afstand om automatisch de nuttige hoogte te kunnen bepalen. " +
            "<br />= Øk/2 + c" +
            "<br />waarbij:" +
            "<br />Øk is de staafdiameter" +
            "<br />c is de dekking op de hoofdwapening",
            Symbol = "<i>d</i><sub>ref</sub>", Unit = "mm")]
        public double ReferentieAfstand { get; set; } = 50;



        [TableColumn(Label = "nuttige hoogte",
            Description = "is de nuttige hoogte van de profiel en wordt berekend door middel van de hoogte en de referentieafstand.",
            Symbol = "d",
            Unit = "mm")]
        public double NuttigeHoogte
        {
            get
            {
                return Hoogte - ReferentieAfstand;
            }
        }
        public Formula NuttigeHoogteFormula => new() { StaticValue = "d = h - d_{ref}", DynamicValue = $"d = {Hoogte} - {ReferentieAfstand}" };



        [TableColumn(Label = "lijnlast", Description = "lijnlast q in kN/m¹", Symbol = "q", Unit = "kN/m¹")]
        public double Lijnlast { get; set; } = 10; // kN/m



        [TableColumn(Label = "Moment (BGT)", Description = "Moment", Symbol = "<i>M</i><sub>Eqp</sub>", Unit = "kNm")]
        public double MomentEqp
        {
            get
            {

                this.Krachten.My.Kar = Lijnlast * Math.Pow(L, 2) / 8.0;
                return this.Krachten.My.Kar;

            }
        }
        public Formula MomentEqpFormula => new() { StaticValue = @$"M_{{Eqp}} = \frac{{1}}{{8}} q L^2 = {MomentEqp.ToTeX()}" };

        [TableColumn(Symbol = "<i>M</i><sub>Ed</sub>", Unit = "kNm", Label = "Moment rekenwaarde")]
        public double MomentEd
        {
            get
            {
                this.Krachten.My.Ed = 1.3 * MomentEqp;
                return this.Krachten.My.Ed;
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
