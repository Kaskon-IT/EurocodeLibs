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
        
        private ReferentieVlakEnum _referentieVlak = ReferentieVlakEnum.Onder;
        private BetonDekkingContext _dekking = new BetonDekkingContext() 
        { 
            DekkingToe = 30.0  // ✅ Default waarde
        };
        private WapeningContext _basisWapening = new();
        private WapeningContext? _verdeelWapening;
        private WapeningContext? _bijlegWapening;
        private double _diameterVerdeel = 6;
        private int _laagHoofdwapening = 1;

        /// <summary>
        /// Het referentievlak van de wapening (Boven, Onder, Links, Rechts).
        /// Wanneer dit wordt gewijzigd, wordt het ReferentieVlak van alle wapeningcontexts ook bijgewerkt.
        /// </summary>
        public ReferentieVlakEnum ReferentieVlak
        {
            get => _referentieVlak;
            set
            {
                if (SetProperty(ref _referentieVlak, value))
                {
                    // Update alle nested wapening contexten
                    UpdateReferentieVlakkenVanWapening();
                }
            }
        }




        /// <summary>
        /// Dekking van de eerste laag. (buitenste laag)
        /// </summary>
        public BetonDekkingContext DekkingBuitensteLaag
        {
            get => _dekking;
            set => SetNestedProperty(ref _dekking!, value);
        }

        public double DekkingLaag1 => DekkingBuitensteLaag.DekkingToe;

        public double DekkingLaag2
        {
            get => DekkingLaag1 + DiameterLaag1;
        }

        public double DiameterLaag1 
        {
            get
            {
                if (LaagHoofdwapening == 1) return BasisWapening.GrootsteDiameter;
                else return VerdeelWapening?.GrootsteDiameter ?? 0;
            }
        }

        /// <summary>
        /// De hoofdwapening van de groep.
        /// </summary>
        public WapeningContext BasisWapening
        {
            get => _basisWapening;
            set
            {
                if (SetNestedProperty(ref _basisWapening!, value))
                {
                    // Zorg dat nieuwe wapening hetzelfde ReferentieVlak krijgt
                    if (_basisWapening != null)
                    {
                        _basisWapening.ReferentieVlak = this.ReferentieVlak;
                    }
                    // Update dekkingen wanneer BasisWapening verandert
                    UpdateReferentieDekkingen();
                }
            }
        }

        /// <summary>
        /// De verdeelwapening van de groep.
        /// </summary>
        public WapeningContext? VerdeelWapening 
        {
            get => _verdeelWapening;
            set
            {
                if (SetNestedProperty(ref _verdeelWapening, value))
                {
                    // Zorg dat nieuwe wapening hetzelfde ReferentieVlak krijgt
                    if (_verdeelWapening != null)
                    {
                        _verdeelWapening.ReferentieVlak = this.ReferentieVlak;
                    }
                    // Update dekkingen wanneer VerdeelWapening verandert
                    UpdateReferentieDekkingen();
                }
            }
        }

        /// <summary>
        /// Bijlegwapening (optionele aanvullende wapening). Toegevoegd voor backward compatibility.
        /// </summary>
        public WapeningContext? BijlegWapening
        {
            get => _bijlegWapening;
            set
            {
                if (SetNestedProperty(ref _bijlegWapening, value))
                {
                    // Zorg dat nieuwe wapening hetzelfde ReferentieVlak krijgt
                    if (_bijlegWapening != null)
                    {
                        _bijlegWapening.ReferentieVlak = this.ReferentieVlak;
                    }
                }
            }
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
                ReferentieVlak = this.ReferentieVlak,
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
            // Herbereken dekkingen bij elke update (via nested property changes)
            UpdateReferentieDekkingen();
        }
        protected override bool Valideer()
        {
            Meldingen.Clear();
            return true;
        }

        /// <summary>
        /// Update het ReferentieVlak van alle wapeningcontexten naar het ReferentieVlak van deze groep.
        /// </summary>
        private void UpdateReferentieVlakkenVanWapening()
        {
            if (_basisWapening != null)
            {
                _basisWapening.ReferentieVlak = this.ReferentieVlak;
            }
            
            if (_verdeelWapening != null)
            {
                _verdeelWapening.ReferentieVlak = this.ReferentieVlak;
            }
            
            if (_bijlegWapening != null)
            {
                _bijlegWapening.ReferentieVlak = this.ReferentieVlak;
            }
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

            // ✅ Als geen dekking, gebruik default 30mm
            if (baseDekking == null || baseDekking == 0)
            {
                Console.WriteLine($"⚠️ PlaatWapeningGroep: Geen dekking ingesteld, gebruik default 30mm");
                baseDekking = 30.0;
            }

            // If hoofdwapening is laag 1 -> Basis = dekking en laag 1, Verdeel = dekking + verdeel diameter en laag 2
            // If hoofdwapening is laag 2 -> Verdeel = dekking en laag 1, Basis = dekking + basis diameter en laag 2
            if (LaagHoofdwapening == 1)
            {
                // BasisWapening in laag 1
                double add = 0;
                if (_basisWapening != null)
                {
                    TrySetReferentieDekking(_basisWapening, baseDekking.Value, 1);
                    _basisWapening.LaagNummer = 1;
                    add = _basisWapening.GrootsteDiameter;
                }

                if (_verdeelWapening != null)
                {
                    TrySetReferentieDekking(_verdeelWapening, baseDekking.Value + add, 2);
                    _verdeelWapening.LaagNummer = 2;
                }
            }
            else
            {
                // BasisWapening in laag 2
                double add = 0;
                if (_verdeelWapening != null)
                {
                    TrySetReferentieDekking(_verdeelWapening, baseDekking.Value, 1);
                    _verdeelWapening.LaagNummer = 1;
                    add = _verdeelWapening.GrootsteDiameter;
                }

                if (_basisWapening != null)
                {
                    TrySetReferentieDekking(_basisWapening, baseDekking.Value + add, 2);
                    _basisWapening.LaagNummer = 2;
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

