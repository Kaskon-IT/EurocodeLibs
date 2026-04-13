using CommonLibrary;
using Profielen.Parametrisch;
using System.ComponentModel;

namespace BetonProfielen
{
    


    public class BetonProfiel : BaseEurocodeContext, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ParametrischProfielContext _profiel = new();
        public ParametrischProfielContext Profiel
        {
            get => _profiel;
            set
            {
                if (_profiel != value)
                {
                    if (_profiel != null)
                        _profiel.PropertyChanged -= Profiel_PropertyChanged;

                    _profiel = value;
                    OnPropertyChanged(nameof(Profiel));

                    if (_profiel != null)
                        _profiel.PropertyChanged += Profiel_PropertyChanged;
                }
            }
        }

        public BetonProfiel()
        {
            _profiel.PropertyChanged += Profiel_PropertyChanged;
        }

        public BetonProfiel(ParametrischProfielContext profiel)
        {
            Profiel = profiel;
        }

        public BetonProfiel(double b, double h)
        {
            Profiel = new(b, h);
        }

        private void Profiel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward event, optionally with prefix:
            OnPropertyChanged($"Profiel.{e.PropertyName}");
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string Heading { get; set; } = "Doorsnede (profiel)";

        [TableColumn(Label = "naam")]
        public string Naam => Profiel?.UserFriendlyName ?? "";

        [TableColumn(Label = "breedte", Symbol = "<i>b</i>", Unit = "mm")]
        public double Breedte => Profiel?.Breedte ?? 0;

        [TableColumn(Label = "hoogte", Symbol = "<i>h</i>", Unit = "mm")]
        public double Hoogte => Profiel?.Hoogte ?? 0;

        [TableColumn(Label = "oppervlak", Symbol = "<i>A</i>", Unit = "mm²")]
        public double Area => Profiel?.Area ?? 0;

        [TableColumn(Label = "traagheidsmoment", Symbol = "<i>I</i><sub>y</sub>", Unit = "mm⁴")]
        public double Iy => Profiel?.Iy ?? 0;

        [TableColumn(Label = "weerstandsmoment", Symbol = "<i>W</i><sub>y</sub>", Unit = "mm³")]
        public double Wy => Profiel?.Wy ?? 0;

        protected override void Bereken()
        {
            // aanvullende berekeningen hier
        }

        protected override bool Valideer()
        {
            // validaties hier

            return true;
        }
    }
}
