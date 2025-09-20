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
        public override string Heading { get; set; } = "Studie doorbuiging";


        /// <summary>
        /// Parameterloze constructor voor serialisatie doeleinden.
        /// Tijdelijke voorbeeld/test class.
        /// </summary>
        public DoorbuigingStudie()
        {
            KruipKrimpBerekening = new BetonContextKruipEnKrimpCalculator(Beton);
            DoorbuigingBerekening = new BetonDoorbuigingCalculator(Beton, Wapening, KruipKrimpBerekening, Krachten, Profiel, this);
            BuigingBerekening = new BendingResults(Beton, Profiel, Wapening, Krachten);

        }

        public BetonContext Beton { get; set; } = new("C30/37");
        public Snedekrachten Krachten { get; set; } = new();
        public BetonContextKruipEnKrimpCalculator KruipKrimpBerekening { get; set; }
        public BetonDoorbuigingCalculator DoorbuigingBerekening { get; set; }
        public ParametrischeProfielen.ParametrischProfielContext Profiel = new() { Breedte = 500, Hoogte = 700 };
        public WapeningContext Wapening { get; set; } = new() { Tekst = "6Ø25" };
        public BendingResults BuigingBerekening { get; set; }

        public DoorbuigingStudie(double l = 10, double q = 40)
        {
            Heading = "Studie doorbuiging";
            L = l;
            Lijnlast = q;
            Profiel = new(500, 700);
            Beton = new(BetonsterkteklasseEnum.C30_37) { Profiel = this.Profiel };

            KruipKrimpBerekening = new BetonContextKruipEnKrimpCalculator(Beton);
            DoorbuigingBerekening = new BetonDoorbuigingCalculator(Beton, Wapening, KruipKrimpBerekening, Krachten, Profiel, this);
            BuigingBerekening = new BendingResults(Beton, Profiel, Wapening, Krachten);
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
            Description = "is de nuttige hoogte en wordt berekend door middel van de hoogte van profiel en de referentieafstand.",
            Symbol = "d",
            Unit = "mm")]
        public double NuttigeHoogte
        {
            get
            {
                return Hoogte - ReferentieAfstand;
            }
        }
        public Formula NuttigeHoogteFormula => new() { StaticValue = "d = h - d_{ref}", DynamicValue = $"d = {Hoogte} - {ReferentieAfstand} = {NuttigeHoogte.ToTeX(unit: "mm")}" };



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


        //[TableColumn(Label = "deler toelaatbare doorbuiging", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "1/")]
        public int DelerVoorToelaatbareDoorbuiging { get; set; } = 250;



        [TableColumn(Label = "toelaatbare doorbuiging", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "")]

        public double ToelaatbareDoorbuigingFractieOverspanning => 1 / (double)DelerVoorToelaatbareDoorbuiging;

        [TableColumn(Label = "toelaatbare doorbuiging", Symbol = "<i>u</i><sub>toel.</sub>", Unit = "mm")]
        public double ToelaatbareDoorbuiging
        {
            get
            {
                return L * 1000 * ToelaatbareDoorbuigingFractieOverspanning;
            }
        }




        [TableColumn(Label = "doorbuiging", Symbol = "<i>u</i><sup>*</sup>", Unit = "mm")]
        public double DoorbuigingEenvoudig
        {
            get
            {
                BerekenEnValideer();
                return DoorbuigingBerekening.DoorbuigingBenadering;
            }
        }

        //WapeningContext Wapening = new() { Tekst = "6Ø25" }; // 2945 mm2








        //public BetonContextKruipEnKrimpCalculator KruipEnKrimp = new(_beton);


        // nieuwe waarden











        protected override void Bereken()
        {
            // gaat automatisch
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            if (DoorbuigingBerekening.DoorbuigingBenadering > ToelaatbareDoorbuiging)
            {


                Meldingen.Add(new Melding(MeldingType.Waarschuwing, $"Doorbuiging is groter dan toelaatbaar."));
                return false;
            }
            return true;
        }
    }
}
