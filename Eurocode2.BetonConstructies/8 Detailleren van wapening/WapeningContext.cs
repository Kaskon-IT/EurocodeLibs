using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public class WapeningContext : INotifyPropertyChanged
    {
        public WapeningContext()
        {

        }
        public WapeningContext(string tekst)
        {
            Tekst = tekst;
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
                    _wapgroepen = WapeningHelper.GetWapGroepen(_tekst);
                    OnPropertyChanged(nameof(Tekst));
                    OnPropertyChanged(nameof(As));
                    OnPropertyChanged(nameof(HohMaat));
                }
            }
        }

        public double As { get { return WapeningHelper.GetDsnOpp(Tekst); } }

        private List<string>? _wapgroepen;

        public double HohMaat { get { return WapeningHelper.GetKleinsteHohMaat(_wapgroepen); } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
