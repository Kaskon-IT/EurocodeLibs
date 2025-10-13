using CommonLibrary;
using Eurocode.Belastingen;
using ExportFactory.Shared;
using ParametrischeProfielen;

namespace Eurocode.BetonConstructies
{
    public class DoorbuigingValidatieContext : BaseEurocodeContext
    {
        public DoorbuigingValidatieContext()
        {

        }

        public DoorbuigingValidatieContext(BetonContext beton, ParametrischProfielContext profiel, WapeningContext wapening)
        {
            Beton = beton;
            Profiel = profiel;
            Wapening = wapening;

            Kruipkrimp.Beton = beton;
            Kruipkrimp.Profiel = profiel;

            BerekenEnValideer();
        }


        public override string Heading { get; set; } = "Validatie doorbuiging";

        // lengte
        private double _lengteMM = 7200;
        [TableColumn(Label = "lengte", Symbol = "<i>L</i><sub>t</sub>", Unit = "mm")]
        public double LengteMM { get => _lengteMM; }


        private bool _gebruikFctmlFl = true;
        [TableColumn(Label = "gebruik <i>f</i><sub>ctm,fl</sub> in berekening")
            ]
        public bool GebruikFctmFl { get => _gebruikFctmlFl; set => SetProperty(ref _gebruikFctmlFl, value); }

        private double _factorZeeg = 0.0;
        [TableColumn(Label = "zeeg", Symbol = "<i>w</i><sub>c</sub>", Unit = "×L")]
        public double FactorZeeg { get => _factorZeeg; set => SetProperty(ref _factorZeeg, value); }

        [TableColumn(Label = "bijkomend (combinatie)")]
        public BelastingCombinatieTypeEnum CombinatieTypeBijkomend { get; set; } = BelastingCombinatieTypeEnum.Frequent;

        [TableColumn(Label = "bijkomend grenswaarde 1", Unit = "×L")]
        public double FactorBijkomend { get; set; } = 0.002;

        public double GrenswaardeBijk1 => FactorBijkomend * LengteMM;

        private double _bijkomendGrenswaarde2 = 15;

        [TableColumn(Label = "bijkomend grenswaarde 2", Unit = "mm")]
        public double GrenswaardeBijk2
        {
            get => _bijkomendGrenswaarde2;
            set => SetProperty(ref _bijkomendGrenswaarde2, value);
        }

        [TableColumn(Label = "bijkomend grenswaarde", Unit = "mm")]
        public double GrenswaardeBijkomend => Math.Min(GrenswaardeBijk1, GrenswaardeBijk2);
        public Formula GrenswaardeBijkomendFormula => new() { StaticValue = @$" = \max \left\{{ {FactorBijkomend}\cdot{LengteMM:0} ,\;{GrenswaardeBijk2:0.##} \right\}}" };

        [TableColumn(Label = "eindfase (combinatie)")]
        public BelastingCombinatieTypeEnum CombinatieTypeEind { get; set; } = BelastingCombinatieTypeEnum.QuasiBlijvend;

        [TableColumn(Label = "eindfase grenswaarde 1", Unit = "×L")]
        public double FactorEind { get; set; } = 0.004;

        public double GrenswaardeEind1 => FactorEind * LengteMM;

        private double _eindGrenswaarde2 = 25;

        [TableColumn(Label = "eindfase grenswaarde 2", Unit = "mm")]
        public double GrenswaardeEind2
        {
            get => _eindGrenswaarde2;
            set => SetProperty(ref _eindGrenswaarde2, value);
        }

        [TableColumn(Label = "eindfase grenswaarde", Unit = "mm")]
        public double GrenswaardeEind => Math.Min(GrenswaardeEind1, GrenswaardeEind2);
        public Formula GrenswaardeEindFormula => new() { StaticValue = @$" = \max \left\{{ {FactorEind}\cdot{LengteMM:0} ,\;{GrenswaardeEind2:0.##} \right\}}" };



        // backing fields
        private ParametrischProfielContext _profiel = new();
        private BetonContext _beton = new();
        private WapeningContext _wapening = new();
        private BetonContextKruipEnKrimpCalculator _kruipkrimp = new();

        // reference properties
        public ParametrischeProfielen.ParametrischProfielContext Profiel { get => _profiel; set => SetNestedProperty(ref _profiel!, value); }
        public BetonContext Beton { get => _beton; set => SetNestedProperty(ref _beton!, value); }
        public WapeningContext Wapening { get => _wapening; set => SetNestedProperty(ref _wapening!, value); }
        public BetonContextKruipEnKrimpCalculator Kruipkrimp { get => _kruipkrimp; set => SetNestedProperty(ref _kruipkrimp!, value); }


        // results
        [TableColumn(Label = "zeeg", Symbol = "<i>w</i><sub>c</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wc { get; private set; }

        [TableColumn(Label = "doorbuiging onmiddelijk (zonder kruip)", Symbol = "<i>w</i><sub>1</sub>", StringFormat = "0.#", Unit = "mm")]
        public double W1 { get; private set; }

        [TableColumn(Label = "maximale doorbuiging", Symbol = "<i>w</i><sub>max</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wmax { get; private set; }

        [TableColumn(Label = "bijkomende doorbuiging", Symbol = "<i>w</i><sub>bijk</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wbijk { get; private set; }

        [TableColumn(Label = "totale doorbuiging", Symbol = "<i>w</i><sub>tot</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wtot { get; private set; }





        protected override void Bereken()
        {
            BetonDoorbuigingContext ctx = new()
            {
                LengteMM = LengteMM,
                Profiel = Profiel,
                Beton = Beton,
                Wapening = Wapening,
                Kruipkrimp = Kruipkrimp,
                CombinatieType = CombinatieTypeBijkomend,
                Lijnlast = 6.5, // kN/m
                LijnlastG = 5.0, // kN/m
                D = Profiel.Hoogte - 50
            };

            BetonDoorbuigingCalculator calculator = new(ctx);
            calculator.GebruikFctmFl = GebruikFctmFl;
            calculator.DoorbuigingZeeg = LengteMM * FactorZeeg;
            calculator.BerekenEnValideer();

            Console.WriteLine($"BIJKOMEND -> combinatie : {ctx.CombinatieType}");
            Console.WriteLine($"W zeeg: {calculator.DoorbuigingZeeg} mm");
            Wc = calculator.DoorbuigingZeeg;
            Console.WriteLine($"W1: {calculator.DoorbuigingW1} mm");
            W1 = calculator.DoorbuigingW1;
            Console.WriteLine($"W bijk: {calculator.DoorbuigingBijk} mm");
            Wbijk = calculator.DoorbuigingBijk;
            Console.WriteLine($"W tot: {calculator.DoorbuigingLangeduur} mm");


            // pas de context aan
            ctx.Lijnlast = 5.9;
            ctx.CombinatieType = CombinatieTypeEind;
            // pas de calculator aan
            calculator.Ctx = ctx;
            calculator.BerekenEnValideer();

            Console.WriteLine($"EINDFASE -> combinatie : {ctx.CombinatieType}");
            Console.WriteLine($"W eind: {calculator.DoorbuigingEind} mm");
            Wmax = calculator.DoorbuigingEind;

            Console.WriteLine($"W tot: {calculator.DoorbuigingLangeduur} mm");
            Wtot = calculator.DoorbuigingLangeduur;

        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            bool returnVal = true;

            if (this.Wbijk > this.GrenswaardeBijkomend)
            {
                AddMeldingWaarschuwing($"overschrijding bijkomende doorbuiging");
                returnVal = false;
            }

            if (this.Wmax > this.GrenswaardeEind)
            {
                AddMeldingWaarschuwing("overschrijding maximale doorbuiging");
                returnVal = false;
            }

            if (this.FactorZeeg > 0.004)
            {
                AddMeldingOpmerking("een in de bekisting aangebrachte opbuiging (zeeg) behoort in het algemeen niet groter te zijn dan de overspanning/250");
            }

            return returnVal;
        }
    }
}
