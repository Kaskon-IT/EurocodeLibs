using CommonLibrary;

namespace Eurocode.BetonConstructies
{

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
            this._zRef = DekkingToegepast + _gemiddeldeDiameter / 2;
        }

        private string _tekst = "8-150";
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
            return $"{Tekst} | ({As:0} mm²)";
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



        //public double ZRef
        //{
        //    get
        //    {
        //        return DekkingToegepast + 0.5 * GemiddeldeDiameter;
        //    }
        //}

        //public double GemiddeldeDiameter
        //{
        //    get
        //    {
        //        return WapeningHelper.GetGemiddeldeDiameter(Tekst);
        //    }
        //}




        public double As { get { return WapeningHelper.GetDsnOpp(Tekst); } }

        private List<string>? _wapgroepen;
        public List<WapeningContext> _subgroepen { get; set; } = new();


        public double HohMaat { get { return WapeningHelper.GetKleinsteHohMaat(_wapgroepen); } }









        protected override void Bereken()
        {
            _gemiddeldeDiameter = WapeningHelper.GetGemiddeldeDiameter(Tekst);
            _zRef = DekkingToegepast + 0.5 * _gemiddeldeDiameter;
        }

        public override bool IsAkkoord()
        {

            return true;
        }



        protected override bool Valideer()
        {
            Meldingen.Clear();

            AddMeldingOpmerking("bijgewerkt");

            return true;

        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtml(isDraaiTabel);

        //    throw new NotImplementedException();
        //}
    }
}
