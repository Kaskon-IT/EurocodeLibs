using CommonLibrary;

namespace Eurocode.BetonConstructies
{



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

    public class PlaatWapeningGroep : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "plaatwapening";
        
        private BetonDekkingContext _dekking = new BetonDekkingContext();
        private WapeningContext _basisWapening = new();
        private WapeningContext? _bijlegWapening;
        private double _diameterVerdeel = 6;
        private int _laagHoofdwapening = 1;


        public BetonDekkingContext Dekking
        {
            get => _dekking;
            set => SetNestedProperty(ref _dekking!, value);
        }

        public WapeningContext BasisWapening
        {
            get => _basisWapening;
            set => SetNestedProperty(ref _basisWapening!, value);
        }

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


    public class WapeningContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Wapening";

        public WapeningContext()
        {

        }

        public WapeningContext(string tekst, BetonDekkingContext dekking)
        {
            Tekst = tekst;
            Dekking = dekking;
        }

        private double _gemiddeldeDiameter;
        private double _zRef;
        private BetonDekkingContext _dekking;

        public double GemiddeldeDiameter => _gemiddeldeDiameter;
        public double ZRef => _zRef;


        public void SetZRef()
        {
            this._gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
            this._zRef = DekkingToegepast + _gemiddeldeDiameter / 2.0;
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
                    //OnPropertyChanged();

                    _wapgroepen = WapeningHelper.GetWapGroepen(_tekst);
                    //_subgroepen.Clear();
                    //foreach (var groep in _wapgroepen)
                    //{
                    //   _subgroepen.Add(new() { Tekst = groep });
                    //}
                    OnPropertyChanged(nameof(Tekst));
                    OnPropertyChanged(nameof(As));
                    OnPropertyChanged(nameof(HohMaat));
                    OnPropertyChanged(nameof(Dekking.DekkingToe));
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



        public BetonDekkingContext Dekking
        {
            get => _dekking;
            set
            {
                if (_dekking != value)
                {
                    _dekking = value;
                }
            }
        }

        public double DekkingToegepast
        {
            get
            {
                if (Dekking != null)
                {
                    //OnPropertyChanged(nameof(Dekking.DekkingToe));
                    //Bereken(); // System.Overflow

                    return Dekking.DekkingToe;
                }

                else
                {
                    return 20;
                }
            }
        }



        private double _breedte = 1000;
        private double _diameterBijlegStaven = 8;
        private double _aantalBijlegStaven = 0;

        /// <summary>
        /// Werkende breedte van de wapening in mm. 
        /// Wanneer niet opgegeven, wordt standaard 1000 mm aangehouden.
        /// Bij hoh-maat berekeningen wordt deze waarde gebruikt om het aantal staven en As,prov te bepalen.
        /// Bijvoorbeeld bij een plaat met een breedte van 500 mm en r8-150, resulteert dit in 500/150 = 3.33 staven x de doorsnede van een r8 staaf = 50.3 mm², dus As,prov = 3.33 x 50.3 = 167.7 mm².
        /// </summary>
        public double Breedte
        {
            get => _breedte;
            set => SetProperty(ref _breedte, value);
        }

        public double DiameterBijlegStaven { get => _diameterBijlegStaven; set => SetProperty(ref _diameterBijlegStaven, value); }
        public double AantalBijlegStaven { get => _aantalBijlegStaven; set => SetProperty(ref _aantalBijlegStaven, value); }



        [TableColumn(Label = "Doorsnedeeoppervlakte basis", Symbol = "<i>A</i><sub>s</sub>", Unit = "mm", StringFormat = "0")]
        public double AsBasis
        {
            get 
            {
                return WapeningHelper.GetDsnOpp(Tekst, Breedte); 
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
            _zRef = DekkingToegepast + 0.5 * _gemiddeldeDiameter;
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
