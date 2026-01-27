using CommonLibrary;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Een plaatwapening groep bestaat uit een collectie van waplagen op één zijde van de plaat.
    /// Bijvoorbeeld bovenwapening of onderwapening.
    /// 
    /// Elke laag heeft zijn eigen betondekking en functie (langswapening, dwarswapening, etc).
    /// Bijvoorbeeld: onderwapening r8-150+r10-500 (2e laag) en verdeelwapening r6-200 (1e laag).
    /// Bijvoorbeeld: onderwapening r8-150 (1e laag) en geen verdeelwapening.
    /// 
    /// </summary>
    public class PlaatWapeningGroep : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "plaatwapening";
        
        private BetonDekkingContext _dekking = new BetonDekkingContext();
        private WapeningContext _basisWapening = new();
        private WapeningContext? _verdeelWapening;
        private WapeningContext? _bijlegWapening;
        private double _diameterVerdeel = 6;
        private int _laagHoofdwapening = 1;




        /// <summary>
        /// Dekking van de eerste laag. (buitenste laag)
        /// </summary>
        public BetonDekkingContext DekkingBuitensteLaag
        {
            get => _dekking;
            set => SetNestedProperty(ref _dekking!, value);
        }

        /// <summary>
        /// OBSOLETE: oude naam 'Dekking' behouden voor backward compatibility.
        /// Verwijst naar de huidige 'DekkingBuitensteLaag'.
        /// </summary>
        [Obsolete("Gebruik DekkingBuitensteLaag i.p.v. Dekking. Deze property blijft voor backward compatibility.")]
        public BetonDekkingContext Dekking
        {
            get => _dekking;
            set => SetNestedProperty(ref _dekking!, value);
        }


        /// <summary>
        /// De hoofdwapening van de groep.
        /// </summary>
        public WapeningContext BasisWapening
        {
            get => _basisWapening;
            set => SetNestedProperty(ref _basisWapening!, value);
        }

        /// <summary>
        /// De verdeelwapening van de groep.
        /// </summary>
        public WapeningContext? VerdeelWapening 
        {
            get => _verdeelWapening;
            set => SetNestedProperty(ref _verdeelWapening, value);
        }

        /// <summary>
        /// Bijlegwapening (optionele aanvullende wapening). Toegevoegd voor backward compatibility.
        /// </summary>
        public WapeningContext? BijlegWapening
        {
            get => _bijlegWapening;
            set => SetNestedProperty(ref _bijlegWapening, value);
        }

        public double DiameterVerdeel
        {
            get => _diameterVerdeel;
            set => SetProperty(ref _diameterVerdeel, value);
        }
        
        public int LaagHoofdwapening
        {
            get => _laagHoofdwapening;
            set => SetProperty(ref _laagHoofdwapening, value);
        }





        protected override void Bereken()
        {
            
        }
        protected override bool Valideer()
        {
            Meldingen.Clear();
            return true;
        }

    }
}

