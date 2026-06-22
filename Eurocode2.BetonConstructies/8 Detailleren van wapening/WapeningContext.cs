using CommonLibrary;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Geeft aan ten opzichte van welk vlak de wapening is gedefinieerd
    /// </summary>
    public enum ReferentieVlakEnum
    {
        /// <summary>Bovenzijde van het element (boven plaat, boven balk)</summary>
        Boven,
        
        /// <summary>Onderzijde van het element (onder plaat, onder balk)</summary>
        Onder,
        
        /// <summary>Linkerzijde van het element</summary>
        Links,
        
        /// <summary>Rechterzijde van het element</summary>
        Rechts
    }


    public abstract class BaseWapeningContext : BaseEurocodeContext
    {
        // een basis voor wapening contexten, zoals plaatwapening, balkwapening, etc.
        

    }

    public class PlaatWapening : BaseWapeningContext
    {
        public override string Heading { get; set; } = "plaatwapening";
        
        private PlaatWapeningGroep? _boven;
        private PlaatWapeningGroep? _onder;

        /// <summary>
        /// Bovenwapening groep. Wanneer deze wordt toegewezen, wordt automatisch ReferentieVlak = Boven ingesteld.
        /// </summary>
        public PlaatWapeningGroep? Boven
        {
            get => _boven;
            set
            {
                _boven = value;
                if (_boven != null)
                {
                    _boven.ReferentieVlak = ReferentieVlakEnum.Boven;
                }
            }
        }

        /// <summary>
        /// Onderwapening groep. Wanneer deze wordt toegewezen, wordt automatisch ReferentieVlak = Onder ingesteld.
        /// </summary>
        public PlaatWapeningGroep? Onder
        {
            get => _onder;
            set
            {
                _onder = value;
                if (_onder != null)
                {
                    _onder.ReferentieVlak = ReferentieVlakEnum.Onder;
                }
            }
        }

        /// <summary>
        /// Maakt een diepe kopie van deze PlaatWapening met alle nested properties.
        /// </summary>
        public PlaatWapening Clone()
        {
            return new PlaatWapening
            {
                Heading = this.Heading,
                Boven = this.Boven?.Clone(),
                Onder = this.Onder?.Clone()
            };
        }

        protected override void Bereken()
        {
            //throw new NotImplementedException();
        }
        protected override bool Valideer()
        {
            Meldingen.Clear();
            if (Boven == null && Onder == null)
            {
                AddMeldingError("Minstens één van de plaatwapening groepen (boven of onder) moet worden opgegeven.");
                return false;
            }
            return true;
        }
    }


    public class WapeningContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Wapening";

        public WapeningContext()
        {
            LaagNummer = 2; 
        }

        public WapeningContext(string tekst, double referentieDekking)
        {
            Tekst = tekst;
            ReferentieDekking = referentieDekking;
        }

        public string SanitizedTekst()
        {
            string tekstCompleet = $"{Tekst}";
            
            if (!String.IsNullOrWhiteSpace(BijlegWapening))
            {
                tekstCompleet += $"+{BijlegWapening}";
            }


            return tekstCompleet
                .Replace("x", "Ø")
                .Replace("*", "Ø")
                .Replace("r", "Ø")
                .Replace("R", "Ø")
                .Replace("d", "Ø")
                .Replace("D", "Ø")
                .Replace("+", " + ");
        }

        // OBSOLETE constructor voor backward compatibility
        [Obsolete("Gebruik WapeningContext(string tekst, double referentieDekking) i.p.v. BetonDekkingContext")]
        public WapeningContext(string tekst, BetonDekkingContext dekking)
        {
            Tekst = tekst;
            ReferentieDekking = dekking.DekkingToe;
        }

        private double _gemiddeldeDiameter;
        public double GemiddeldeDiameter
        {
            get
            {
                if (_gemiddeldeDiameter == 0 && !string.IsNullOrEmpty(Tekst))
                {
                    _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
                }
                return _gemiddeldeDiameter;
            }
        }

        private double _grootsteDiameter;
        public double GrootsteDiameter => _grootsteDiameter;

        private double _referentieAfstand;
        public double ReferentieAfstand => _referentieAfstand;

        public double ZRef => _referentieAfstand;

        private BetonDekkingContext _dekking = new();
        
        // Nieuwe properties voor referentiesysteem
        private ReferentieVlakEnum _referentieVlak = ReferentieVlakEnum.Onder;
        private double _referentieDekking = 30.0; // Opgegeven dekking in mm
        private double _referentieLengte = 1000.0; // Standaard 1000mm (1m)

        
        /// <summary>
        /// Het referentievlak waarop de wapening is gedefinieerd
        /// </summary>
        public ReferentieVlakEnum ReferentieVlak
        {
            get => _referentieVlak;
            set => SetProperty(ref _referentieVlak, value);
        }

        /// <summary>
        /// Opgegeven betondekking tot buitenzijde staaf in mm 
        /// Bijvoorbeeld: 30mm dekking volgens Eurocode
        /// </summary>
        public double ReferentieDekking
        {
            get => _referentieDekking;
            set
            {
                if (SetProperty(ref _referentieDekking, value))
                {
                    // ✅ Update backing field STIL (geen event voor ReferentieAfstand)
                    _referentieAfstand = _referentieDekking + (_gemiddeldeDiameter / 2.0);
                    // NIET: OnPropertyChanged(nameof(ReferentieAfstand))
                }
            }
        }

        /// <summary>
        /// Berekende afstand van hart staaf tot referentievlak in mm
        /// Berekend als: ReferentieDekking + (GemiddeldeDiameter / 2)
        /// Bijvoorbeeld: bij dekking 30mm en Ø8: ReferentieAfstand = 30 + 8/2 = 34mm
        /// </summary>
        //public double ReferentieAfstand => ReferentieDekking + (_gemiddeldeDiameter / 2.0);

        /// <summary>
        /// Lengte over welke de wapening is aangebracht in mm
        /// Bijvoorbeeld: bij een balk van 500mm breed moet ReferentieLengte = 500mm zijn
        /// Standaard 1000mm voor rekenkundige wapening per meter
        /// </summary>
        public double ReferentieLengte
        {
            get => _referentieLengte;
            set
            {
                if (SetProperty(ref _referentieLengte, value))
                {
                    OnPropertyChanged(nameof(As));
                    OnPropertyChanged(nameof(AsBasis));
                }
            }
        }

        /// <summary>
        /// Update de gemiddelde en grootste diameter op basis van de Tekst.
        /// Interne methode - triggert GEEN events (om loops te voorkomen).
        /// </summary>
        public void SetZRef()
        {
            _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
            _grootsteDiameter = WapeningHelper.GetGrootsteDiameter(Tekst);
            _referentieAfstand = _referentieDekking + (_gemiddeldeDiameter / 2.0);
            // GEEN OnPropertyChanged calls - dit voorkomt loops!
        }


        

        /// <summary>
        /// Toegepaste betondekking in mm (voor backward compatibility)
        /// Gebruik bij voorkeur ReferentieDekking
        /// </summary>
        public double DekkingToegepast
        {
            get => ReferentieDekking;
            set => ReferentieDekking = value;
        }

        /// <summary>
        /// OBSOLETE: Dekking property voor backward compatibility met oude EurocodeRazorClassLibrary
        /// Gebruik in plaats daarvan ReferentieDekking of DekkingToegepast
        /// </summary>
        [Obsolete("Gebruik ReferentieDekking in plaats van Dekking")]
        public BetonDekkingContext Dekking
        {
            get
            {
                // Lazy initialize en synchroniseer met ReferentieDekking
                if (_dekking == null)
                {
                    _dekking = new BetonDekkingContext();
                }
                _dekking.DekkingToe = ReferentieDekking;
                return _dekking;
            }
            set
            {
                _dekking = value;
                if (_dekking != null)
                {
                    ReferentieDekking = _dekking.DekkingToe;
                }
            }
        }


        private string _tekst = "r8-150";

        [TableColumn(Label = "opgave wapening")]
        public string Tekst
        {
            get => _tekst;
            set
            {
                if (_tekst != value)
                {
                    _tekst = value;
                    _wapgroepen = WapeningHelper.GetWapGroepen(_tekst);
                    
                    // ✅ Update backing fields STIL (geen events)
                    _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(_tekst);
                    _grootsteDiameter = WapeningHelper.GetGrootsteDiameter(_tekst);
                    _referentieAfstand = _referentieDekking + (_gemiddeldeDiameter / 2.0);
                    
                    // ✅ Trigger alleen events voor properties met setters
                    OnPropertyChanged(nameof(Tekst));
                    OnPropertyChanged(nameof(As));
                    OnPropertyChanged(nameof(AsBasis));
                    OnPropertyChanged(nameof(HohMaat));
                    // NIET: GemiddeldeDiameter, GrootsteDiameter, ReferentieAfstand (zijn read-only!)
                }
            }
        }

        private bool _isOpgave;

        /// <summary>
        /// Geeft aan of de wapening een vaste opgave van de gebruiker is.
        /// - <c>true</c>: de wapening wordt NIET automatisch bijgewerkt door de applicatie; er wordt
        ///   gerekend met de door de gebruiker opgegeven <see cref="Tekst"/>. De invoer is aanpasbaar.
        /// - <c>false</c>: de wapening wordt automatisch berekend/bijgewerkt door de applicatie; de invoer is disabled.
        /// </summary>
        [TableColumn(Label = "opgave gebruiker")]
        public bool IsOpgave
        {
            get => _isOpgave;
            set => SetProperty(ref _isOpgave, value);
        }

        private string? _tekstOndergrens;

        /// <summary>
        /// Minimale wapening die altijd aangehouden moet worden (bijv. "r8-100").
        /// Bij (her)berekening wordt de Tekst bijgewerkt met inachtneming van deze ondergrens.
        /// - Diameter: neem maximum van (berekend vs ondergrens)
        /// - Hart-op-hart: neem minimum van (berekend vs ondergrens)
        /// </summary>
        [TableColumn(Label = "minimale wapening")]
        public string? TekstOndergrens
        {
            get => _tekstOndergrens;
            set
            {
                if (_tekstOndergrens != value)
                {
                    _tekstOndergrens = value;
                    OnPropertyChanged(nameof(TekstOndergrens));
                }
            }
        }


        public List<WapeningContext> GetSupGroepen()
        {
            List<WapeningContext> subgroepen = [];
            if (_wapgroepen != null)
            {
                foreach (var groep in _wapgroepen)
                {
                    subgroepen.Add(new() { Tekst = groep });
                }
            }

            return subgroepen;
        }

        public override string ToString()
        {
            return $"{Tekst
                .Replace("r", "Ø")
                .Replace("R", "Ø")
                .Replace("d", "Ø")
                .Replace("D", "Ø")}";
        }

        

        public string ToStringWithLaag()
        {
            var tekst = "";
            switch (LaagNummer, ReferentieVlak)
            {
                case (null, ReferentieVlakEnum.Boven):
                case (1, ReferentieVlakEnum.Boven): 
                    tekst += "▼";
                    break;
                case (2, ReferentieVlakEnum.Boven):
                    tekst += "▼▼";
                    break;

                case (null, ReferentieVlakEnum.Onder):
                case (1, ReferentieVlakEnum.Onder):
                    tekst += "▲";
                    break;
                case (2, ReferentieVlakEnum.Onder):
                    tekst += "▲▲";
                    break;
            }

            return tekst; 
        }

        public string GetUserFriendlyText(string eenheid = "mm²", bool includeGroups = true)
        {
            if (string.IsNullOrEmpty(Tekst))
            {
                return "Onbekend";
            }

            string returnString = $"{As:0} {eenheid}";

            _wapgroepen ??= WapeningHelper.GetWapGroepen(Tekst);


            if (_wapgroepen != null && includeGroups)
            {
                returnString += " (";
                List<string> userFriendlys = [];

                foreach (var groep in _wapgroepen)
                {
                    if (groep.Contains("-"))
                    {
                        var parts = groep.Split('-');

                        // Haal alleen de cijfers en eventueel wat erna uit parts[0], door alles vóór het eerste cijfer te verwijderen
                        string firstPart = parts[0];
                        int firstDigitIndex = -1;

                        for (int i = 0; i < firstPart.Length; i++)
                        {
                            if (char.IsDigit(firstPart[i]))
                            {
                                firstDigitIndex = i;
                                break;
                            }
                        }

                        if (firstDigitIndex != -1)
                            firstPart = firstPart.Substring(firstDigitIndex);
                        else
                            firstPart = ""; // Geen cijfers gevonden, dan leeg

                        userFriendlys.Add($"Ø{firstPart}-{parts[1]}");
                    }
                    else
                    {
                        // Vervang alle 'r', 'R', 'd', 'D' door 'Ø'
                        string replaced = groep.Replace('r', 'Ø')
                                              .Replace('R', 'Ø')
                                              .Replace('d', 'Ø')
                                              .Replace('D', 'Ø');

                        userFriendlys.Add(replaced);
                    }
                }

                returnString += string.Join(" + ", userFriendlys);
                returnString += ")";

            }

            return returnString;
        }

        /// <summary>
        /// Laagnummer toewijzing (1 of 2) — wordt door PlaatWapeningGroep gezet voor sorting/overzicht.
        /// Nullable: kan null zijn wanneer niet ingesteld.
        /// </summary>
        public int? LaagNummer { get; set; } 

        /// <summary>
        /// Maakt een diepe kopie van deze WapeningContext.
        /// </summary>
        public WapeningContext Clone()
        {
            return new WapeningContext()
            {
                Tekst = this.Tekst,
                ReferentieVlak = this.ReferentieVlak,
                ReferentieLengte = this.ReferentieLengte,
                ReferentieDekking = this.ReferentieDekking,
                LaagNummer = this.LaagNummer,
                IsOpgave = this.IsOpgave,
                AantalBijlegStaven = this.AantalBijlegStaven,
                DiameterBijlegStaven = this.DiameterBijlegStaven
            };
        }


        /// <summary>
        /// OBSOLETE: Gebruik ReferentieDekking i.p.v. BetonDekkingContext
        /// Deze property wordt in toekomstige versie verwijderd
        /// </summary>
        //[Obsolete("Gebruik ReferentieDekking i.p.v. Dekking. Deze property wordt verwijderd in toekomstige versie.")]
        //public BetonDekkingContext Dekking
        //{
        //    get => _dekking;
        //    set
        //    {
        //        if (_dekking != value)
        //        {
        //            _dekking = value;
        //            // Update ReferentieDekking voor backward compatibility
        //            if (_dekking != null)
        //            {
        //                ReferentieDekking = _dekking.DekkingToe;
        //            }
        //        }
        //    }
        //}

        




        private double _diameterBijlegStaven = 8;

        private double _aantalBijlegStaven = 0;

        /// <summary>
        /// OBSOLETE: Gebruik ReferentieLengte i.p.v. Breedte
        /// Werkende breedte van de wapening in mm. 
        /// </summary>
        [Obsolete("Gebruik ReferentieLengte i.p.v. Breedte. Deze property wordt verwijderd in toekomstige versie.")]
        public double Breedte
        {
            get => _referentieLengte;
            set => ReferentieLengte = value;
        }

        public double DiameterBijlegStaven { get => _diameterBijlegStaven; set => SetProperty(ref _diameterBijlegStaven, value); }
        public double AantalBijlegStaven { get => _aantalBijlegStaven; set => SetProperty(ref _aantalBijlegStaven, value); }



        [TableColumn(Label = "Doorsnedeeoppervlakte basis", Symbol = "<i>A</i><sub>s</sub>", Unit = "mm²", StringFormat = "0")]
        public double AsBasis
        {
            get 
            {
                // Gebruik ReferentieLengte i.p.v. Breedte
                return WapeningHelper.GetDsnOpp(Tekst, ReferentieLengte); 
            }   
        }


        [TableColumn(Label = "Bijlegwapening", Symbol = "")]
        public string BijlegWapening
        {
            get
            {
                if (_aantalBijlegStaven > 0 && _diameterBijlegStaven > 0)
                {
                    return $"{_aantalBijlegStaven}Ø{_diameterBijlegStaven:0.#}";
                }
                else return "";
            }
        }

        public double AsBijleg
        {
            get
            {
                if (_aantalBijlegStaven > 0 && _diameterBijlegStaven > 0)
                {
                    return WapeningHelper.GetDsnOpp(_aantalBijlegStaven, _diameterBijlegStaven);
                }
                else return 0;
            }
        }


        [TableColumn(Label = "Doorsnedeoppervlakte wapening", Symbol = "<i>A</i><sub>s</sub>", Unit = "mm²", StringFormat = "0")]
        public double As 
        { 
            get 
            {
                return AsBasis + AsBijleg;
            } 
        }

        /// <summary>
        /// Aantal staven met bijbehorende diameter per staafgroep, afgeleid uit <see cref="Tekst"/>.
        /// <para>
        /// Voorbeelden (bij <see cref="ReferentieLengte"/> = 1000 mm):
        /// <list type="bullet">
        /// <item>"4r12" → [(4, 12)]</item>
        /// <item>"r8-150" → [(1000/150 ≈ 6.67, 8)]</item>
        /// <item>"r8-150+3r12" → [(6.67, 8), (3, 12)]</item>
        /// </list>
        /// Bij hart-op-hart-formaat schaalt het aantal mee met <see cref="ReferentieLengte"/>.
        /// </para>
        /// </summary>
        public List<(double Aantal, double Diameter)> Staven
        {
            get
            {
                List<(double Aantal, double Diameter)> staven = [];
                if (string.IsNullOrWhiteSpace(Tekst))
                    return staven;

                var wapgroepen = WapeningHelper.GetWapGroepen(Tekst);
                if (wapgroepen == null)
                    return staven;

                foreach (var wapgroep in wapgroepen)
                {
                    var trimmed = wapgroep.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    staven.Add(WapeningHelper.GetAantalEnDiameter(trimmed, ReferentieLengte));
                }

                return staven;
            }
        }

        /// <summary>
        /// Totaal aantal staven over alle staafgroepen, afgeleid uit <see cref="Tekst"/>.
        /// Bij hart-op-hart-formaat is dit afhankelijk van <see cref="ReferentieLengte"/>.
        /// </summary>
        public double AantalStaven => Staven.Sum(s => s.Aantal);

        private List<string>? _wapgroepen;
        public List<WapeningContext> _subgroepen { get; set; } = [];


        public double HohMaat
        {
            get
            {
                return OpgaveGrootsteHohMaatTenBehoeveVanControleScheurwijdte ?? WapeningHelper.GetKleinsteHohMaat(_wapgroepen);
            }
        }


        public double? OpgaveGrootsteHohMaatTenBehoeveVanControleScheurwijdte { get; set; } = 80;







        protected override void Bereken()
        {
            if (!string.IsNullOrEmpty(Tekst))
            {
                _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
                _grootsteDiameter = WapeningHelper.GetGrootsteDiameter(Tekst);
                _referentieAfstand = _referentieDekking + (_gemiddeldeDiameter / 2.0);
            }
        }



        private List<double> _diameters = [5, 6, 8, 10, 12, 16, 20, 25, 32, 40];
        protected override bool Valideer()
        {
            Meldingen.Clear();



            //AddMeldingOpmerking("bijgewerkt");

            return true;

        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtml(isDraaiTabel);

        //    throw new NotImplementedException();
        //}
    }
}

