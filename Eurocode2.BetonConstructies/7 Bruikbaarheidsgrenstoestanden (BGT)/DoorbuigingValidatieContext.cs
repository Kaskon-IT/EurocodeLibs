using CommonLibrary;
using CommonLibrary.Extensions;
using Eurocode.Belastingen;
using ExportFactory.Shared;
using ParametrischeProfielen;

namespace Eurocode.BetonConstructies
{
    public class DoorbuigingCombinatieContext
    {
        [TableColumn(Label = "combinatie")]
        public BelastingCombinatieTypeEnum CombinatieType { get; set; } = BelastingCombinatieTypeEnum.Frequent;

        public double Lijnlast { get; set; } = -5.0; // kN/m

        [TableColumn(Label = "moment", Unit = "kNm")]
        public double M { get; internal set; }
        public string MSymbol => $"<i>M</i><sub>{BelastingenHelpers.GetSubscript(CombinatieType)}</sub>";


        [TableColumn(Label = "doorbuiging bijkomend", Symbol = "<i>w</i><sub>bijk</sub>", StringFormat = "0.00", Unit = "mm")]
        public double Wbijk { get; internal set; } // mm
        public string WbijkSymbol => $"<i>w</i><sub>bijk,{BelastingenHelpers.GetSubscript(CombinatieType)}</sub>";

        [TableColumn(Label = "doorbuiging totaal", Symbol = "<i>w</i><sub>tot</sub>", StringFormat = "0.00", Unit = "mm")]
        public double Wtot { get; internal set; } // mm
        public string WtotSymbol => $"<i>w</i><sub>tot,{BelastingenHelpers.GetSubscript(CombinatieType)}</sub>";



        [TableColumn(Label = "doorbuiging maximaal (uiteindelijk)", Symbol = "<i>w</i><sub>max</sub>", StringFormat = "0.00", Unit = "mm")]
        public double Wmax { get; internal set; } // mm
        public string WmaxSymbol => $"<i>w</i><sub>max,{BelastingenHelpers.GetSubscript(CombinatieType)}</sub>";

        //protected override void Bereken()
        //{
        //    // nothing to do, wordt in de hoofdcontext berekend
        //}

        //protected override bool Valideer()
        //{
        //    return true;
        //}





    }


    public class DoorbuigingValidatieContext : BaseEurocodeContext
    {
        public DoorbuigingValidatieContext()
        {

        }

        public DoorbuigingValidatieContext(BetonContext beton, ParametrischProfielContext profiel, WapeningContext wapening, double lengteMM, List<DoorbuigingCombinatieContext> combinaties)
        {
            Beton = beton;
            Profiel = profiel;
            Wapening = wapening;

            _lengteMM = lengteMM;

            //this.QG = qG;
            //this.QEind = qEind;
            //this.QBijk = qBijk;
            //this.L = l;
            this.CombinatieContexts = combinaties;

            Kruipkrimp.Beton = beton;
            Kruipkrimp.Profiel = profiel;

            // Automatisch herberekenen bij wijziging
            //L.ValueChanged += _ => BerekenEnValideer();



            BerekenEnValideer();
        }

        //public Ref<double> L { get; }
        public List<DoorbuigingCombinatieContext> CombinatieContexts { get; } = [];




        public override string Heading { get; set; } = "Validatie doorbuiging";

        // lengte
        private double _lengteMM = 7200;
        [TableColumn(Label = "lengte", Symbol = "<i>L</i><sub>t</sub>", Unit = "mm")]
        public double LengteMM { get => _lengteMM; set => SetProperty(ref _lengteMM, value); }


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

        private double _grenswaardeBijk2 = 15;

        [TableColumn(Label = "bijkomend grenswaarde 2", Unit = "mm")]
        public double GrenswaardeBijk2
        {
            get => _grenswaardeBijk2;
            set => SetProperty(ref _grenswaardeBijk2, value);
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


        public double UnityCheckBijkomend => Wbijk / GrenswaardeBijkomend;
        public double UnityCheckEind => Wmax / GrenswaardeEind;



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
        public double Wc => LengteMM * FactorZeeg; // doorbuiging door zeeg in mm

        [TableColumn(Label = "doorbuiging blijvend (zonder kruip)", Symbol = "<i>w</i><sub>1</sub>", StringFormat = "0.00", Unit = "mm")]
        public double W1 { get; private set; } // onmiddelijke doorbuiging bij blijvende belasting

        [TableColumn(Label = "doorbuiging blijvend (incl. kruip)", Symbol = "<i>w</i><sub>2</sub>", StringFormat = "0.00", Unit = "mm")]
        public double W2 { get; internal set; } // bijkomende doorbuiging bij blijvende belasting

        [TableColumn(Label = "maximale doorbuiging", Symbol = "<i>w</i><sub>max</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wmax { get; private set; }

        [TableColumn(Label = "toets maximale doorbuiging", Symbol = "<i>UC</i><sub>max</sub>", StringFormat = "0.00")]
        public double UnityCheckMax => Math.Abs(Wmax / GrenswaardeEind);
        public string UnityCheckMaxUnit => UnityCheckMax < 1.01 ? "✔️" : "❌";
        public Formula UnityCheckMaxFormula => new() { StaticValue = $"UC = \\left| \\frac{{{Wmax.ToTeX()}}}{{{GrenswaardeEind.ToTeX()}}} \\right|= {UnityCheckMax.ToTeX()}" };


        [TableColumn(Label = "bijkomende doorbuiging", Symbol = "<i>w</i><sub>bijk</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wbijk { get; private set; }

        [TableColumn(Label = "toets bijkomende doorbuiging", Symbol = "<i>UC</i><sub>bijk</sub>", StringFormat = "0.00")]
        public double UcBijk => Math.Abs(Wbijk / GrenswaardeBijkomend);
        public string UcBijkUnit => UcBijk < 1.01 ? "✔️" : "❌";
        public Formula UcBijkFormula => new() { StaticValue = $"UC =\\left| \\frac{{{Wbijk.ToTeX()}}}{{{GrenswaardeBijkomend.ToTeX()}}} \\right| = {UcBijk.ToTeX()}" };



        [TableColumn(Label = "totale doorbuiging", Symbol = "<i>w</i><sub>tot</sub>", StringFormat = "0.#", Unit = "mm")]
        public double Wtot { get; private set; }




        private double D => Profiel.Hoogte - Wapening.ZRef;


        // tijdelijk voor debug
        private List<BetonDoorbuigingCalculator> Calculators { get; set; } = [];


        private double QG => CombinatieContexts.FirstOrDefault(c => c.CombinatieType == BelastingCombinatieTypeEnum.Blijvend)?.Lijnlast ?? 0.0;

        //[TableColumn(Label = "combinaties")]
        //public string CombiTest => this.CombinatieContexts == null || CombinatieContexts.Count == 0
        //   ? "n.v.t."
        //   : string.Join(" \r\n",
        //   CombinatieContexts
        //       .Where(p => p.CombinatieType != BelastingCombinatieTypeEnum.Blijvend)
        //       .Select(p => $"{p.CombinatieType}:Wbij{p.Wbijk:0.0} Wtot{p.Wtot:0.0}"));





        private void BerekenDoorbuigingCombinatie(DoorbuigingCombinatieContext combinatie)
        {
            BetonDoorbuigingContext ctx = new()
            {
                LengteMM = this.LengteMM,
                Profiel = this.Profiel,
                Beton = this.Beton,
                Wapening = this.Wapening,
                Kruipkrimp = this.Kruipkrimp,
                CombinatieType = combinatie.CombinatieType,
                Lijnlast = combinatie.Lijnlast,
                LijnlastG = this.QG,
                D = this.D
            };


            BetonDoorbuigingCalculator calculator = new(ctx);
            calculator.GebruikFctmFl = GebruikFctmFl;
            calculator.Wc = this.LengteMM * FactorZeeg;

            calculator.BerekenEnValideer();

            // geef de resultaten terug
            combinatie.M = calculator.Moment;
            combinatie.Wbijk = calculator.Wbijk;
            combinatie.Wtot = calculator.Wtot;
            combinatie.Wmax = calculator.Wmax;

            // alleen de blijvende combinatie vult W1 en W2 in de hoofdcontext in
            if (combinatie.CombinatieType == BelastingCombinatieTypeEnum.Blijvend)
            {
                this.W1 = calculator.W1;
                this.W2 = calculator.Wtot;
            }

            if (combinatie.CombinatieType == this.CombinatieTypeBijkomend)
            {
                this.Wbijk = combinatie.Wbijk;
            }

            if (combinatie.CombinatieType == this.CombinatieTypeEind)
            {
                this.Wmax = combinatie.Wmax;
            }


            Calculators.Add(calculator); // voor debug

            Console.WriteLine($"--");
            Console.WriteLine($"| Berekening doorbuiging : combinatie -> {ctx.CombinatieType}");
            Console.WriteLine($"| Wc    : {calculator.Wc:0.00} mm");
            Console.WriteLine($"| W1    : {calculator.W1:0.00} mm");
            Console.WriteLine($"| Wbij  : {calculator.Wbijk:0.00} mm");
            Console.WriteLine($"| Wmax  : {calculator.Wmax:0.00} mm");
            Console.WriteLine($"| Wtot  : {calculator.Wtot:0.00} mm");
            Console.WriteLine($"| Mcr   : {calculator.Mcr:0.00} kNm");
            Console.WriteLine($"| M     : {calculator.Moment:0.00} kNm");

            Console.WriteLine($"| Zeta0 : {calculator.Zeta0:0.00}");
            Console.WriteLine($"| Zeta∞ : {calculator.Zeta:0.00}");
            Console.WriteLine($"| As    : {calculator.Ctx.Wapening.As:0} mm²");
            Console.WriteLine($"--");


        }

        protected override void Bereken()
        {
            if (Profiel == null || Beton == null || Wapening == null || Kruipkrimp == null)
            {
                Console.WriteLine("Profiel, beton, wapening en kruip/krimp moeten zijn ingevuld");
                return;
            }



            Wapening.SetZRef(); // waarschijnlijk niet nodig, maar voor de zekerheid

            Kruipkrimp.BerekenEnValideer(); // waarschijnlijk niet nodig, maar voor de zekerheid

            Calculators.Clear();


            foreach (var combinatie in CombinatieContexts)
            {
                BerekenDoorbuigingCombinatie(combinatie);
            }









            //this.Calculator = calculator; // tijdelijk voor debug

        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            bool returnVal = true;

            if (this.Wbijk > this.GrenswaardeBijkomend)
            {
                AddMeldingError($"overschrijding bijkomende doorbuiging");
                returnVal = false;
                AddMeldingHint($"pas de hoogte van het element aan, of gebruik meer wapening");
            }

            if (this.Wmax > this.GrenswaardeEind)
            {
                AddMeldingError("overschrijding maximale doorbuiging");
                returnVal = false;
                if (this.FactorZeeg < 0.004)
                {
                    if (this.FactorZeeg == 0)
                        AddMeldingHint("pas eventueel een zeeg toe");
                    else
                        AddMeldingHint("vergroot eventueel de zeeg");
                }

            }




            if (this.FactorZeeg > 0.004)
            {
                AddMeldingWaarschuwing("een in de bekisting aangebrachte opbuiging (zeeg) behoort in het algemeen niet groter te zijn dan de overspanning/250");
            }

            return returnVal;
        }
    }
}
