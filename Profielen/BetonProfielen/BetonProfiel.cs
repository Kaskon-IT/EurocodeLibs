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
