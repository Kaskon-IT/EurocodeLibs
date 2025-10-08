using CommonLibrary;
using CommonLibrary.Extensions;
using Eurocode.Belastingen;
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


        private BetonDoorbuigingContext _ctx = new();
        public BetonDoorbuigingContext Ctx
        {
            get => _ctx;
            set => SetNestedProperty(ref _ctx, value);
        }
        //public BetonDoorbuigingContext CtxFrequent { get; set; } = new();


        private void SetContextCollection(DoorbuigingTrap trap)
        {
            // vul de context
            Ctx.Beton = trap.Beton;
            Ctx.Profiel = trap.Profiel;
            Ctx.Wapening = trap.Wapening;
            Ctx.Kruipkrimp = trap.KruipKrimpBerekening;

            Ctx.D = trap.NuttigeHoogte;
            //CtxQuasiBlijvend.LengteMM = trap.LengteMM;
            Ctx.Lijnlast = trap.Lijnlast;


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
            CalculatorDoorbuigingQuasiBlijvend = new BetonDoorbuigingCalculator(Ctx);
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

            Ctx = context;
            CalculatorDoorbuigingQuasiBlijvend = new(context);
            //
            //context.Kruipkrimp = new() { Beton = context.Beton, Profiel = context.Profiel };


            Init();
            BerekenEnValideer();
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
            CalculatorDoorbuigingQuasiBlijvend = new BetonDoorbuigingCalculator(Ctx);
            //CalculatorDoorbuigingFrequent = new BetonDoorbuigingCalculator(CtxFrequent);

            //CtxQuasiBlijvend.LengteMM
            //BuigingBerekening = new BendingResults(Beton, Profiel, Wapening, SectionForces);
        }



        [TableColumn(Label = "lijnlast (BGT)", Symbol = "<i>q</i><sub>E,BGT</sub>", Unit = "kN/m")]
        public double Lijnlast
        {
            get => Ctx.Lijnlast;
        }
        public string LijnlastSymbol => $"<i>q</i><sub>{SuffixCombinatieType}</sub>";

        [TableColumn(Label = "lijnlast (permanent)", Symbol = "<i>q</i><sub>G</sub>", Unit = "kN/m")]
        public double LijnlastPermanent
        {
            get => Ctx.LijnlastG;
        }



        //[TableColumn(Label = "lengte overspanning", Symbol = "<i>L</i><sub>t,proj.z</sub>", Unit = "m")]
        public double LengteM => Overspanning / 1000.0;


        [TableColumn(Label = "overspanning", Symbol = "<i>L</i><sub>t</sub>", Unit = "mm")]
        public double Overspanning
        {
            get => Ctx.LengteMM;
            //internal set { CtxQuasiBlijvend.LengteMM = value; }
        }


        //[TableColumn(Label = "hoogte profiel", Symbol = "h", Unit = "mm")]
        public double Hoogte
        {
            get { return Profiel.Hoogte; }
            internal set { Profiel.Hoogte = value; }
        }

        //[TableColumn(Label = "breedte profiel", Symbol = "b", Unit = "mm")]

        public double Breedte
        {
            get { return Profiel.Breedte; }
            internal set { Profiel.Breedte = value; }
        }

        //[TableColumn(Label = "wapening (tekst)",
        //    Description = "Voor opgave wapening gebruik bijvoorbeeld: " +
        //    "<br />6r25 (voor 6 staven Ø25)" +
        //    "<br />r8-200 (voor staven Ø8 hoh 200mm)" +
        //    "<br />4r20+2r25 (voor meerdere groepen)")]
        public string WapeningTekst
        {
            get { return Wapening.Tekst; }
            internal set { Wapening.Tekst = value; }
        }

        //[TableColumn(Label = "wapening (mm²)", Symbol = "<i>A<i><sub>s,toe</sub>", Unit = "mm²")]
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





        //[TableColumn(Label = "nuttige hoogte",
        //    Description = "is de nuttige hoogte en wordt berekend door middel van de hoogte van profiel en de referentieafstand.",
        //    Symbol = "d",
        //    Unit = "mm")]
        public double NuttigeHoogte
        {
            get
            {
                return Ctx.D;
            }
        }



        //[TableColumn(Label = "lijnlast", Description = "lijnlast q in kN/m¹", Symbol = "q", Unit = "kN/m¹")]
        //public double Lijnlast => CtxBlijvend.Q;
        private string SuffixCombinatieType
        {
            get
            {
                switch (Ctx.CombinatieType)
                {
                    case Belastingen.BelastingCombinatieTypeEnum.Fundamenteel_A:
                    case Belastingen.BelastingCombinatieTypeEnum.Fundamenteel_B:
                        return "Ed";
                    case Belastingen.BelastingCombinatieTypeEnum.Brand:
                        return "Ebr";
                    case Belastingen.BelastingCombinatieTypeEnum.Aardbeving:
                        return "Eab";
                    case Belastingen.BelastingCombinatieTypeEnum.Karakteristiek:
                        return "Ek";
                    case Belastingen.BelastingCombinatieTypeEnum.Frequent:
                        return "Efr";
                    case Belastingen.BelastingCombinatieTypeEnum.QuasiBlijvend:
                        return "Eqp";
                    case Belastingen.BelastingCombinatieTypeEnum.Blijvend:
                        return "Ep";
                    default:
                        return "Ed";
                }
            }
        }

        //public double LijnlastLocZ => Lijnlast * Math.Pow(Aantrede / Schuin, 2);

        [TableColumn(Label = "Moment (BGT)", Description = "Moment", Symbol = "<i>M</i><sub>E,BGT</sub>", Unit = "kNm")]
        public double MomentEqp
        {
            get
            {
                return Ctx.Lijnlast * Math.Pow(LengteM, 2) / 8.0;
            }
        }
        public string MomentEqpSymbol => $"<i>M</i><sub>{SuffixCombinatieType}</sub>";

        public Formula MomentEqpFormula => new() { StaticValue = @$"M_{{{SuffixCombinatieType}}} = \frac{{1}}{{8}} q L^2 = {1 / 8.0} \cdot {Ctx.Lijnlast.ToTeX()} \cdot {LengteM.ToTeX()}^2 = {MomentEqp.ToTeX()}" };


        [TableColumn(Label = "Moment (G)", Description = "Moment", Symbol = "<i>M</i><sub>G</sub>", Unit = "kNm")]
        public double MomentG
        {
            get
            {
                return Ctx.LijnlastG * Math.Pow(LengteM, 2) / 8.0;
            }
        }





        //[TableColumn(Label = "deler toelaatbare doorbuiging", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "1/")]
        public int DelerVoorToelaatbareDoorbuigingEind { get; set; } = 250;
        public int DelerVoorToelaatbareDoorbuigingBijk { get; set; } = 500;

        //[TableColumn(Label = "Combinatietype")]
        public BelastingCombinatieTypeEnum CombinatieType => Ctx.CombinatieType;


        //[TableColumn(Label = "fractie toelaatbare doorbuiging (eind)", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "")]
        public double ToelaatbareDoorbuigingEindFractieOverspanning => 1 / (double)DelerVoorToelaatbareDoorbuigingEind;

        //[TableColumn(Label = "toelaatbare doorbuiging (eind)", Symbol = "<i>u</i><sub>eind,toel.</sub>", Unit = "mm")]
        public double ToelaatbareDoorbuigingEind
        {
            get
            {
                return Overspanning * ToelaatbareDoorbuigingEindFractieOverspanning;
            }
        }

        [TableColumn(Label = "unity check eind", Symbol = "U.C.", StringFormat = "0.00")]
        public double UnityCheckEind => DoorbuigingEind / ToelaatbareDoorbuigingEind;
        public Formula UnityCheckEindFormula => new() { StaticValue = $@"= \frac{{{DoorbuigingEind:0.00}}} {{{ToelaatbareDoorbuigingEind:0.00}}} = {UnityCheckEind:0.00}" };

        [TableColumn(Label = "unity check bijk", Symbol = "U.C.", StringFormat = "0.00")]
        public double UnityCheckBijk => DoorbuigingBijk / ToelaatbareDoorbuigingBijk;
        public Formula UnityCheckBijkFormula => new() { StaticValue = $@"= \frac{{{DoorbuigingBijk:0.00}}} {{{ToelaatbareDoorbuigingBijk:0.00}}} = {UnityCheckBijk:0.00}" };






        //[TableColumn(Label = "fractie toelaatbare doorbuiging (bijkomend)", Unit = "×L", Description = "De toelaatbare doorbuiging wordt bepaalt als fractie van de overspanning, bijvoorbeeld 1/250 of 1/300", Symbol = "")]
        public double ToelaatbareDoorbuigingBijkFractieOverspanning => 1 / (double)DelerVoorToelaatbareDoorbuigingBijk;

        //[TableColumn(Label = "toelaatbare doorbuiging (bijkomend)", Symbol = "<i>w</i><sub>bijk.toel.</sub>", Unit = "mm")]
        public double ToelaatbareDoorbuigingBijk
        {
            get
            {
                return Overspanning * ToelaatbareDoorbuigingBijkFractieOverspanning;
            }
        }




        public double UnityCheckDoorbuigingEind
        {
            get
            {
                return DoorbuigingEind / ToelaatbareDoorbuigingEind;
            }
        }

        public double UnityCheckDoorbuigingBijkomend
        {
            get
            {
                return DoorbuigingBijk / ToelaatbareDoorbuigingBijk;
            }
        }

        //[TableColumn(Label = "doorbuiging", Symbol = "<i>w</i><sub>tot</sub>", Unit = "mm")]
        public double Wtot
        {
            get
            {
                //CalculatorDoorbuigingQuasiBlijvend.BerekenEnValideer();
                return Ctx.Wtot;
            }
        }



        [TableColumn(Label = "doorbuiging (eind)", Symbol = "<i>w</i><sub>eind</sub>", Unit = "mm")]
        public double DoorbuigingEind
        {
            get
            {
                //BerekenEnValideer();

                // dit is niet goed, maar als we de krachten willen vullen moeten we even M opvragen.
                var moment = this.MomentEqp;

                //CalculatorDoorbuigingQuasiBlijvend.BerekenEnValideer();
                return CalculatorDoorbuigingQuasiBlijvend.DoorbuigingEind;

            }
        }

        [TableColumn(Label = "doorbuiging (bijkomstig)", Symbol = "<i>w</i><sub>bijk</sub>", Unit = "mm")]
        public double DoorbuigingBijk
        {
            get
            {
                return Ctx.Wbijk;

            }
        }


        [TableColumn(Label = "doorbuiging (onmiddelijk)", Symbol = "<i>w</i><sub>on,G</sub>", Unit = "mm")]
        public double DoorbuigingW1
        {
            get
            {
                return Ctx.W1;

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
            bool returnVal = true;
            if (UnityCheckEind > 1)
            {
                AddMeldingWaarschuwing("doorbuiging eindfase te groot");
                returnVal = false;
            }

            if (UnityCheckBijk > 1)
            {
                AddMeldingWaarschuwing("doorbuiging bijkomend te groot");
                returnVal = false;
            }



            return returnVal;
        }
    }
}
