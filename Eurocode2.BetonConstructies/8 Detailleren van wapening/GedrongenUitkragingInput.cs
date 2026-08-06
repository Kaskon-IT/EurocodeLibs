namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class GedrongenUitkragingInput : BaseInput
    {

        private double _ac = 150;
        public double Ac { 
            get => _ac; 
            set => SetProperty(ref _ac, value);
        }

        private double _h = 500;
        public double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        private double _l = 300;
        public double L
        {
            get => _l;
            set => SetProperty(ref _l, value);
        }

        private double ab = 150;
        public double Ab
        {
            get => ab;
            set => SetProperty(ref ab, value);
        }


    }
}
