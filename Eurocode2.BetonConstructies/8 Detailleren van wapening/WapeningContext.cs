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




    public class PlaatWapening : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "plaatwapening";
        public PlaatWapeningGroep? Boven { get; set; }
        public PlaatWapeningGroep? Onder { get; set; }

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

        }

        public WapeningContext(string tekst, double referentieDekking)
        {
            Tekst = tekst;
            ReferentieDekking = referentieDekking;
        }

        // OBSOLETE constructor voor backward compatibility
        [Obsolete("Gebruik WapeningContext(string tekst, double referentieDekking) i.p.v. BetonDekkingContext")]
        public WapeningContext(string tekst, BetonDekkingContext dekking)
        {
            Tekst = tekst;
            ReferentieDekking = dekking.DekkingToe;
        }

        private double _gemiddeldeDiameter;
        private double _zRef;
        private BetonDekkingContext _dekking = new();
        
        // Nieuwe properties voor referentiesysteem
        private ReferentieVlakEnum _referentieVlak = ReferentieVlakEnum.Onder;
        private double _referentieDekking = 30.0; // Opgegeven dekking in mm
        private double _referentieLengte = 1000.0; // Standaard 1000mm (1m)

        public double GemiddeldeDiameter => _gemiddeldeDiameter;
        public double ZRef => _zRef;

        /// <summary>
        /// Het referentievlak waarop de wapening is gedefinieerd
        /// </summary>
        public ReferentieVlakEnum ReferentieVlak
        {
            get => _referentieVlak;
            set => SetProperty(ref _referentieVlak, value);
        }

        /// <summary>
        /// Opgegeven betondekking tot buitenzijde staaf in mm (c_nom of c_prov)
        /// Bijvoorbeeld: 30mm dekking volgens Eurocode
        /// </summary>
        public double ReferentieDekking
        {
            get => _referentieDekking;
            set
            {
                if (SetProperty(ref _referentieDekking, value))
                {
                    OnPropertyChanged(nameof(ReferentieAfstand));
                    OnPropertyChanged(nameof(ZRef));
                }
            }
        }

        /// <summary>
        /// Berekende afstand van hart staaf tot referentievlak in mm
        /// Berekend als: ReferentieDekking + (GemiddeldeDiameter / 2)
        /// Bijvoorbeeld: bij dekking 30mm en Ø8: ReferentieAfstand = 30 + 8/2 = 34mm
        /// </summary>
        public double ReferentieAfstand
        {
            get => ReferentieDekking + (_gemiddeldeDiameter / 2.0);
        }

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

        public void SetZRef()
        {
            this._gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
            this._zRef = ReferentieAfstand; // Berekende afstand tot hart staaf
        }


        /// <summary>
        /// OBSOLETE: Gebruik ReferentieAfstand i.p.v. BetonDekkingContext
        /// Deze property wordt in toekomstige versie verwijderd
        /// </summary>
        [Obsolete("Gebruik ReferentieAfstand i.p.v. Dekking. Deze property wordt verwijderd in toekomstige versie.")]
        public BetonDekkingContext Dekking
        {
            get => _dekking;
            set
            {
                if (_dekking != value)
                {
                    _dekking = value;
                    // Update ReferentieAfstand voor backward compatibility
                    if (_dekking != null)
                    {
                        _referentieAfstand = _dekking.DekkingToe + (_gemiddeldeDiameter / 2.0);
                    }
                }
            }
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


        private string _tekst = "8-150";
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
                    
                    OnPropertyChanged(nameof(Tekst));
                    OnPropertyChanged(nameof(As));
                    OnPropertyChanged(nameof(AsBasis));
                    OnPropertyChanged(nameof(HohMaat));
                    
                    // Update diameter en ReferentieAfstand bij wijziging van Tekst
                    _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(_tekst);
                    OnPropertyChanged(nameof(GemiddeldeDiameter));
                    OnPropertyChanged(nameof(ReferentieAfstand));
                    OnPropertyChanged(nameof(ZRef));
                    
                    Bereken();
                }
            }
        }


        public List<WapeningContext> GetSupGroepen()
        {
            List<WapeningContext> subgroepen = new();
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
                .Replace("D", "Ø")}  ({As:0} mm²)";
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

        private List<string>? _wapgroepen;
        public List<WapeningContext> _subgroepen { get; set; } = new();


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
            _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
            _zRef = ReferentieAfstand; // Direct de referentieafstand gebruiken
        }



        private List<double> _diameters = [5, 6, 8, 10, 12, 16, 20, 25, 32, 40];
        private double _referentieAfstand;

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

