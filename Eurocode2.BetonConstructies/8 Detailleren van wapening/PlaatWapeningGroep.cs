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
            get 
            {
                if (_verdeelWapening == null)
                    return _diameterVerdeel;
                else
                    return _verdeelWapening.GrootsteDiameter;
            }
            set => SetProperty(ref _diameterVerdeel, value);
        }
        
        public int LaagHoofdwapening
        {
            get => _laagHoofdwapening;
            set
            {
                if (SetProperty(ref _laagHoofdwapening, value))
                {
                    UpdateReferentieDekkingen();
                }
            }
        }

        /// <summary>
        /// Maakt een diepe kopie van deze PlaatWapeningGroep met alle nested properties.
        /// </summary>
        public PlaatWapeningGroep Clone()
        {
            return new PlaatWapeningGroep()
            {
                Heading = this.Heading,
                DekkingBuitensteLaag = new BetonDekkingContext(this.DekkingBuitensteLaag),
                BasisWapening = this.BasisWapening?.Clone() ?? new(),
                VerdeelWapening = this.VerdeelWapening?.Clone(),
                BijlegWapening = this.BijlegWapening?.Clone(),
                LaagHoofdwapening = this.LaagHoofdwapening,
                DiameterVerdeel = this.DiameterVerdeel
            };
        }



        
        
        


        protected override void Bereken()
        {
            
        }
        protected override bool Valideer()
        {
            Meldingen.Clear();
            return true;
        }

        private void UpdateReferentieDekkingen()
        {
            // basis dekking from outer layer
            double? baseDekking = null;
            try
            {
                baseDekking = _dekking?.DekkingToe;
            }
            catch
            {
                baseDekking = null;
            }

            if (baseDekking == null) return;

            // If hoofdwapening is laag 1 -> Basis = dekking, Verdeel = dekking + verdeel diameter
            // If hoofdwapening is laag 2 -> Verdeel = dekking, Basis = dekking + basis diameter
            if (LaagHoofdwapening == 1)
            {
                // set Basis
                if (_basisWapening != null)
                    TrySetReferentieDekking(_basisWapening, baseDekking.Value, 1);

                if (_verdeelWapening != null)
                {
                    double add = _verdeelWapening.GemiddeldeDiameter;
                    TrySetReferentieDekking(_verdeelWapening, baseDekking.Value + add, 2);
                }
            }
            else
            {
                // laag 2
                if (_verdeelWapening != null)
                    TrySetReferentieDekking(_verdeelWapening, baseDekking.Value, 1);

                if (_basisWapening != null)
                {
                    double add = _basisWapening.GemiddeldeDiameter;
                    TrySetReferentieDekking(_basisWapening, baseDekking.Value + add, 2);
                }
            }
        }

        private static void TrySetReferentieDekking(object wapeningObj, double waarde, int? laagNummer = null)
        {
            if (wapeningObj == null) return;

            var t = wapeningObj.GetType();

            // Try direct property 'ReferentieDekking'
            var p = t.GetProperty("ReferentieDekking");
            if (p != null && p.CanWrite)
            {
                try { p.SetValue(wapeningObj, waarde); return; } catch { /* swallow */ }
            }

            // Try DekkingToegepast
            p = t.GetProperty("DekkingToegepast");
            if (p != null && p.CanWrite)
            {
                try { p.SetValue(wapeningObj, waarde); return; } catch { /* swallow */ }
            }

            // Try nested Dekking object with DekkingToe
            p = t.GetProperty("Dekking");
            if (p != null && p.CanWrite)
            {
                try
                {
                    var dekObj = p.GetValue(wapeningObj);
                    if (dekObj != null)
                    {
                        var pd = dekObj.GetType().GetProperty("DekkingToe");
                        if (pd != null && pd.CanWrite)
                        {
                            pd.SetValue(dekObj, waarde);
                            return;
                        }
                    }
                }
                catch { }
            }
            
            // Probeer LaagNummer property op WapeningContext te zetten (indien aanwezig)
            if (laagNummer.HasValue)
            {
                try
                {
                    var propLaag = t.GetProperty("LaagNummer");
                    if (propLaag != null && propLaag.CanWrite)
                    {
                        propLaag.SetValue(wapeningObj, laagNummer.Value);
                    }
                }
                catch { }
            }
        }

    }
}

