using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Een voorbeeld voor gecombineerde eurocode toepassingen.
    /// 
    /// </summary>
    public class DoorbuigingTrap : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "doorbuiging trap";

        //public BetonDoorbuigingContext CtxBlijvend { get; set; } = new();
        //public BetonDoorbuigingContext CtxBlijvend { get; set; } = new();
        public BetonDoorbuigingContext CtxQuasiBlijvend { get; set; } = new(new(), new(), new());
        //public BetonDoorbuigingContext CtxFrequent { get; set; } = new();


        private void SetContextCollection(DoorbuigingTrap trap)
        {
            // vul de context
            CtxQuasiBlijvend.Beton = trap.Beton;
            CtxQuasiBlijvend.Profiel = trap.Profiel;
            CtxQuasiBlijvend.Wapening = trap.Wapening;
            CtxQuasiBlijvend.Kruipkrimp = trap.KruipKrimpBerekening;

            CtxQuasiBlijvend.D = trap.NuttigeHoogte;
            //CtxQuasiBlijvend.LengteMM = trap.LengteMM;
            CtxQuasiBlijvend.Lijnlast = trap.Lijnlast;


            // clone
            //CtxFrequent = CtxQuasiBlijvend.ShallowClone();

            //            CtxFrequent.Lijnlast = trap.Lijnlast;


        }


        /// <summary>
        /// Parameterloze constructor voor serialisatie doeleinden.
        /// Tijdelijke voorbeeld/test class.
        /// </summary>
        public DoorbuigingTrap()
        {
            KruipKrimpBerekening = new BetonContextKruipEnKrimpCalculator(Beton, Profiel);
            //SetContextCollection(this);
            CalculatorDoorbuigingQuasiBlijvend = new BetonDoorbuigingCalculator(CtxQuasiBlijvend);
        }

        public BetonContext Beton { get; set; } = new("C30/37");
        public BetonContextKruipEnKrimpCalculator KruipKrimpBerekening { get; set; }

        //public BetonDoorbuigingCalculator CalculatorDoorbuigingBlijvend { get; set; }
        public BetonDoorbuigingCalculator CalculatorDoorbuigingQuasiBlijvend { get; set; }
        //public BetonDoorbuigingCalculator CalculatorDoorbuigingFrequent { get; set; }




        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } =
            new() { Breedte = 1000, Hoogte = 150 };

        public WapeningContext Wapening { get; set; } = new() { Tekst = "8-150" };

        public DoorbuigingTrap(BetonDoorbuigingContext context)
        {
            Heading = "trap doorbuiging";
            Profiel = context.Profiel;
            Beton = context.Beton;
            KruipKrimpBerekening = context.Kruipkrimp;
            Wapening = context.Wapening;

            CtxQuasiBlijvend = context;
            CalculatorDoorbuigingQuasiBlijvend = new(context);
            //
            //context.Kruipkrimp = new() { Beton = context.Beton, Profiel = context.Profiel };


            Init();
        }


        public DoorbuigingTrap(double lengteMM = 10, double qQp = 30, double qFr = 31)
        {
            Heading = "TRAP DOORBUIGING - VERWIJDER!!";
            //LengteMM = lengteMM;
            //NuttigeHoogte
            //Lijnlast = q;
            Profiel = new(1000, 120);
            Beton = new(BetonsterkteklasseEnum.C45_55);
            KruipKrimpBerekening = new BetonContextKruipEnKrimpCalculator(Beton, Profiel);

            // stel de verschillende contexten in
            //SetContextCollection(this);

            //DoorbuigingBerekening = new BetonDoorbuigingCalculator(CtxBlijvend);
            //CalculatorDoorbuigingBlijvend = new BetonDoorbuigingCalculator(CtxBlijvend);
            CalculatorDoorbuigingQuasiBlijvend = new BetonDoorbuigingCalculator(CtxQuasiBlijvend);
            //CalculatorDoorbuigingFrequent = new BetonDoorbuigingCalculator(CtxFrequent);

            //CtxQuasiBlijvend.LengteMM
            //BuigingBerekening = new BendingResults(Beton, Profiel, Wapening, SectionForces);
        }



        [TableColumn(Label = "lijnlast", Symbol = "<i>q</i><sub>qp</sub>", Unit = "kN/m")]
        public double Lijnlast
        {
            get => CtxQuasiBlijvend.Lijnlast;

        }


        //[TableColumn(Label = "lengte overspanning", Symbol = "<i>L</i><sub>t,proj.z</sub>", Unit = "m")]
        public double LengteM => Overspanning / 1000.0;


        [TableColumn(Label = "overspanning", Symbol = "Lt", Unit = "mm")]
        public double Overspanning
        {
            get => CtxQuasiBlijvend.LengteMM;
            //internal set { CtxQuasiBlijvend.LengteMM = value; }
        }


        [TableColumn(Label = "hoogte profiel", Symbol = "h", Unit = "mm")]
        public double Hoogte
        {
            get { return Profiel.Hoogte; }
            internal set { Profiel.Hoogte = value; }
        }

        [TableColumn(Label = "breedte profiel", Symbol = "b", Unit = "mm")]

        public double Breedte
        {
            get { return Profiel.Breedte; }
            internal set { Profiel.Breedte = value; }
        }

        [TableColumn(Label = "wapening (tekst)",
            Description = "Voor opgave wapening gebruik bijvoorbeeld: " +
            "<br />6r25 (voor 6 staven Ø25)" +
            "<br />r8-200 (voor staven Ø8 hoh 200mm)" +
            "<br />4r20+2r25 (voor meerdere groepen)")]
        public string WapeningTekst
        {
            get { return Wapening.Tekst; }
            internal set { Wapening.Tekst = value; }
        }

        [TableColumn(Label = "wapening (mm²)", Symbol = "<i>A<i><sub>s,toe</sub>", Unit = "mm²")]
        public double WapeningAsApplied
        {
            get { return Wapening.As; }
        }








        //private double Schuin => Math.Sqrt(Math.Pow(Aantrede, 2) + Math.Pow(Optrede, 2));

        //[TableColumn(Symbol = "<i>L</i><sub>t,loc,x</sub>", Label = "lengte (schuin)")]
        //public double LengteLocX
        //{
        //    get { return LengteM * Schuin / Aantrede; }
        //}





        [TableColumn(Label = "nuttige hoogte",
            Description = "is de nuttige hoogte en wordt berekend door middel van de hoogte van profiel en de referentieafstand.",
            Symbol = "d",
            Unit = "mm")]
        public double NuttigeHoogte
        {
            get
            {
                return CtxQuasiBlijvend.D;
            }
        }



        //[TableColumn(Label = "lijnlast", Description = "lijnlast q in kN/m¹", Symbol = "q", Unit = "kN/m¹")]
        //public double Lijnlast => CtxBlijvend.Q;


        //public double LijnlastLocZ => Lijnlast * Math.Pow(Aantrede / Schuin, 2);

        [TableColumn(Label = "Moment (BGT)", Description = "Moment", Symbol = "<i>M</i><sub>Eqp</sub>", Unit = "kNm")]
        public double MomentEqp
        {
            get
            {

                //this.Krachten.My.Kar = Lijnlast * Math.Pow(L, 2) / 8.0;
                //this.SectionForces.My = Lijnlast * Math.Pow(L, 2) / 8.0;
                return CtxQuasiBlijvend.Lijnlast * Math.Pow(LengteM, 2) / 8.0;
                //return this.SectionForces.My;

            }
        }
        public Formula MomentEqpFormula => new() { StaticValue = @$"M_{{Eqp}} = \frac{{1}}{{8}} q L^2 = {1 / 8.0} \cdot {CtxQuasiBlijvend.Lijnlast.ToTeX()} \cdot {LengteM.ToTeX()}^2 = {MomentEqp.ToTeX()}" };


        //[TableColumn(Label = "Moment (BGT)", Description = "Moment (locZ)", Symbol = "<i>M</i><sub>Eqp</sub>", Unit = "kNm")]
        //public double MomentEqpLocX
        //{
        //    get
        //    {
        //        return LijnlastLocZ * Math.Pow(LengteLocX, 2) / 8.0;

        //    }
        //}
        //public Formula MomentEqpLocXFormula => new() { StaticValue = @$"M_{{Eqp}} = \frac{{1}}{{8}} q L^2 = {1 / 8.0} \cdot {LijnlastLocZ.ToTeX()} \cdot {LengteLocX.ToTeX()}^2 = {MomentEqpLocX.ToTeX()}" };




        //[TableColumn(Symbol = "<i>M</i><sub>Ed</sub>", Unit = "kNm", Label = "Moment rekenwaarde")]
        //public double MomentEd
        //{
        //   get
        //   {
        //        this.Krachten.My.Ed = 1.3 * MomentEqp;
        //        return this.Krachten.My.Ed;
        //    }
        //}


        //[TableColumn(Label = "deler toelaatbare doorbuiging", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "1/")]
        public int DelerVoorToelaatbareDoorbuiging { get; set; } = 250;



        [TableColumn(Label = "fractie toelaatbare doorbuiging (eind)", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "")]

        public double ToelaatbareDoorbuigingFractieOverspanning => 1 / (double)DelerVoorToelaatbareDoorbuiging;

        [TableColumn(Label = "toelaatbare doorbuiging (eind)", Symbol = "<i>u</i><sub>toel.</sub>", Unit = "mm")]
        public double ToelaatbareDoorbuigingEind
        {
            get
            {
                return Overspanning * ToelaatbareDoorbuigingFractieOverspanning;
            }
        }




        [TableColumn(Label = "doorbuiging", Symbol = "<i>w</i><sub>tot</sub>", Unit = "mm")]
        public double Wtot
        {
            get
            {
                //CalculatorDoorbuigingQuasiBlijvend.BerekenEnValideer();
                return CtxQuasiBlijvend.Wtot;
            }
        }



        [TableColumn(Label = "doorbuiging", Symbol = "<i>u</i><sup>*</sup>", Unit = "mm")]
        public double DoorbuigingEenvoudig
        {
            get
            {
                //BerekenEnValideer();

                // dit is niet goed, maar als we de krachten willen vullen moeten we even M opvragen.
                var moment = this.MomentEqp;

                //CalculatorDoorbuigingQuasiBlijvend.BerekenEnValideer();
                return CalculatorDoorbuigingQuasiBlijvend.DoorbuigingBenadering;

            }
        }

        //WapeningContext Wapening = new() { Tekst = "6Ø25" }; // 2945 mm2








        //public BetonContextKruipEnKrimpCalculator KruipEnKrimp = new(Ctx.Beton);


        // nieuwe waarden











        protected override void Bereken()
        {
            // gaat automatisch
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            if (Wtot > ToelaatbareDoorbuigingEind)
            {
                Meldingen.Add(new Melding(MeldingType.Waarschuwing, $"Doorbuiging is groter dan toelaatbaar."));
                return false;
            }
            return true;
        }
    }
}
