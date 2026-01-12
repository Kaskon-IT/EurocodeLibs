using CommonLibrary;
using Eurocode.Belastingen;
using Profielen.Parametrisch;
using Profielen.Beton;

namespace Eurocode.BetonConstructies
{
    public class ZuivereBuiging : BaseEurocodeContext
    {

        public ZuivereBuiging()
        {

        }

        public override string Heading { get; set; } = "Zuivere buiging";
        public override void Init()
        {
            base.Init();
            //SubscribeToContext(Beton);
            //SubscribeToContext(Profiel);
            //SubscribeToContext(Wapening);
            //SubscribeToContext(Forces);

            // geef de contexten mee aan de berekening
            // let op, zonder dekking dus opgave z-ref noodzakelijk
            BerekeningBuiging = new BendingResults(Beton, Profiel, Wapening, Forces);
            ShearCalculation = new DwarskrachtWapContext(Beton, Profiel, Forces);
        }

        // Input parameters
        // Input Contexten
        public BetonContext Beton { get; set; } = new(BetonsterkteklasseEnum.C30_37);
        public BetonProfiel Profiel { get; set; } = new() { Breedte = 400, Hoogte = 500 };
        public WapeningContext Wapening { get; set; } = new() { Tekst = "4x16" };
        public SectionForces Forces { get; set; } = new(my: 188);


        // Input properties

        // backing fields voor alle properties met setters (anders geen recalculatie)
        private double _dekkingToegepast = 25;
        private double _staafDiameter = 12;


        [TableColumn("Dekking op langswapening", Symbol = "<i>c</i>", Unit = "mm")]
        public double DekkingToegepast
        {
            get => _dekkingToegepast;
            set
            {
                if (SetAndRecalculate(ref _dekkingToegepast, value))
                {
                    // pas de nuttige hoogte aan
                    OnPropertyChanged(nameof(NuttigeHoogte));
                }
            }
        }


        [TableColumn(Label = "Staafdiameter", Symbol = "<i>Ø</i><sub>k</sub>", Unit = "mm")]
        public double StaafDiameter
        {
            get => _staafDiameter;
            set
            {
                if (SetAndRecalculate(ref _staafDiameter, value))
                {
                    // pas alle relevantie zaken aan
                    OnPropertyChanged(nameof(NuttigeHoogte));
                }
            }
        }


        // Output derived properties
        [TableColumn(Label = "Nuttige hoogte", Symbol = "<i>d</i>", Unit = "mm")]
        public double NuttigeHoogte => Profiel.Hoogte - Zref;

        [TableColumn(Label = "benodigde wapening", Symbol = "<i>A</i><sub>s,req</sub>", Unit = "mm²")]
        public double AsRequired => BerekeningBuiging.AsRequired;

        private double Zref => DekkingToegepast + StaafDiameter / 2;





        // Berekeningen
        public BendingResults BerekeningBuiging { get; set; } = new();
        public DwarskrachtWapContext ShearCalculation { get; set; } = new();



        protected override void Bereken()
        {
            // we hebben geen dekking Context, dus dan moeten we de z-ref doorgeven
            BerekeningBuiging.ZRefZonderDekking = Zref;
            ShearCalculation.NutHoogte = NuttigeHoogte;

            //BerekeningBuiging.Moment = Forces.My;

            BerekeningBuiging.BerekenEnValideer();
            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {
            return BerekeningBuiging.BerekenEnValideer();
            //w NotImplementedException();
        }
    }
}
