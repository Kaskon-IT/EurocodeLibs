using CommonLibrary;
using Eurocode.Belastingen;
using ParametrischeProfielen;

namespace Eurocode.BetonConstructies
{
    public class BetonDoorbuigingContext : BaseEurocodeContext
    {
        public BetonDoorbuigingContext ShallowClone()
        {
            var clone = (BetonDoorbuigingContext)this.MemberwiseClone();
            clone.Id = Guid.NewGuid(); // of whatever je Id-generator is
            return clone;
        }



        // context (data) model voor doorbuiging (eenvoudig)
        // LET OP!!! deze doorbuiging alleen voor VRIJ OPGELEGDE LIGGER met UNIFORME LIJNLAST!!

        // nodig voor berekening:
        // d
        // L        lengte in [mm]
        // Beton, Profiel, Kruip

        // lijnlastBlijvend     [kN/m¹]   -> w1 
        // lijnlastCombinatie   [kN/m¹]   -> w_tot
        //                                w_bijk = w_tot - w1                                  
        public BetonDoorbuigingContext() { }

        public BetonDoorbuigingContext(BetonContext beton, ParametrischProfielContext profiel, WapeningContext wapening)
        {
            this.Beton = beton;
            this.Profiel = profiel;
            this.Wapening = wapening;
            this.Kruipkrimp = new(Beton, Profiel);
            Heading = "Gegevens doorbuiging";



        }




        private double _d, _lengteMM, _lijnlast, _lijnlastG;

        [TableColumn(Label = "nuttige hoogte", Symbol = "d", Unit = "mm")]
        public double D { get => _d; set => SetProperty(ref _d, value); }

        [TableColumn(Label = "lengte", Symbol = "L", Unit = "mm")]
        public double LengteMM { get => _lengteMM; set => SetProperty(ref _lengteMM, value); }

        //[TableColumn(Label = "lijnlast voor w1", Symbol = "q<sub>p</sub>")]
        //public double LijnlastBijvend { get => _qBlijvend; set => SetProperty(ref _qBlijvend, value); }

        [TableColumn(Label = "lijnlast (bijkomende doorbuiging)", Symbol = "q<sub>...</sub>")]
        public double Lijnlast { get => _lijnlast; set => SetProperty(ref _lijnlast, value); }
        public string LijnlastSymbol => $"<i>q</i><sub>{BelastingenHelpers.GetSubscript(CombinatieType)}</sub>";





        [TableColumn(Label = "lijnlast voor <i>w</i><sub>on,G</sub>", Symbol = "q<sub>G,k</sub>")]
        public double LijnlastG { get => _lijnlastG; set => SetProperty(ref _lijnlastG, value); }


        public double Moment => 1.0 / 8.0 * _lijnlast * Math.Pow(LengteMM * 0.001, 2); // kNm
        public double MomengtG => 1.0 / 8.0 * _lijnlastG * Math.Pow(LengteMM * 0.001, 2); // kNm


        [TableColumn(Label = "Combinatie")]
        public BelastingCombinatieTypeEnum CombinatieType { get; set; } = BelastingCombinatieTypeEnum.Frequent;


        /// <summary>
        /// De doorbuiging als gevolg van blijvende combinatie, korteduur   
        /// </summary>
        public double W1
        {
            get; set;

        }


        /// <summary>
        /// De totale doorbuiging (voor de toetsing van de bijkomende doorbuiging)
        /// </summary>
        public double Wtotaal { get; set; }

        /// <summary>
        /// De totale doorbuiging (voor de toetsing van de doorbuiging in de eindfase)
        /// </summary>



        /// <summary>
        /// De bijkomende doorbuiging
        /// </summary>
        public double Wbijk => Wtotaal - W1;

        public double Weind => Wtotaal - Wzeeg;

        public double Wzeeg => 0;


        private BetonContext _beton = new();
        public BetonContext Beton
        {
            get => _beton;
            set => SetNestedProperty(ref _beton!, value);
        }

        private ParametrischProfielContext _profiel = new();
        public ParametrischProfielContext Profiel
        {
            get => _profiel;
            set => SetNestedProperty(ref _profiel!, value);
        }


        private BetonContextKruipEnKrimpCalculator _kruipkrimp = new();
        public BetonContextKruipEnKrimpCalculator Kruipkrimp
        {
            get => _kruipkrimp;
            set => SetNestedProperty(ref _kruipkrimp!, value);
        }


        private WapeningContext _wapening = new();
        public WapeningContext Wapening
        {
            get => _wapening;
            set => SetNestedProperty(ref _wapening!, value);
        }

        protected override void Bereken()
        {
            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}
