namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class J3ConsoleInput : BaseInput
    {
        private double _l = 250;
        public double L
        {
            get => _l;
            set => SetProperty(ref _l, value);
        }

        private double _factorHEd = 0.4;
        public double FactorHEd
        {
            get => _factorHEd;
            set => SetProperty(ref _factorHEd, value);
        }

        public double HEd => FactorHEd * FEd;

        private double _dikteOplegmateriaal = 20;
        public double DikteOplegmateriaal
        {
            get => _dikteOplegmateriaal;
            set => SetProperty(ref _dikteOplegmateriaal, value);
        }


        private double _b = 350;
        public double B
        {
            get => _b;
            set => SetProperty(ref _b, value);
        }

        private double _bw = 400;
        public double Bw
        {
            get => _bw;
            set => SetProperty(ref _bw, value);
        }


        private double _h = 450;
        public double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        private double _ac = 125;
        public double Ac
        {
            get => _ac;
            set => SetProperty(ref _ac, value);
        }

        private double _nEd = 600;
        public double NEd
        {
            get => _nEd;
            set => SetProperty(ref _nEd, value);
        }


        private double _fEd = 600;
        public double FEd
        {
            get => _fEd;
            set => SetProperty(ref _fEd, value);
        }

        private double _loadPlateLength = 200;
        public double LoadPlateLength
        {
            get => _loadPlateLength;
            set => SetProperty(ref _loadPlateLength, value);
        }

        private double _loadPlateWidth = 300;
        public double LoadPlateWidth
        {
            get => _loadPlateWidth;
            set => SetProperty(ref _loadPlateWidth, value);
        }

        private double _dekking = 35;
        public double Dekking
        {
            get => _dekking;
            set => SetProperty(ref _dekking, value);
        }

        private double _beugelDiameter = 8;
        public double BeugelDiameter
        {
            get => _beugelDiameter;
            set => SetProperty(ref _beugelDiameter, value);
        }

        private double _hoofdstaafDiameter = 16;
        public double HoofdstaafDiameter
        {
            get => _hoofdstaafDiameter;
            set => SetProperty(ref _hoofdstaafDiameter, value);
        }

        private double _fck = 30;
        public double Fck
        {
            get => _fck;
            set => SetProperty(ref _fck, value);
        }

        private double _alphaCc = 0.85;
        public double AlphaCc
        {
            get => _alphaCc;
            set => SetProperty(ref _alphaCc, value);
        }

        private double _gammaC = 1.5;
        public double GammaC
        {
            get => _gammaC;
            set => SetProperty(ref _gammaC, value);
        }

        private double _fyk = 500;
        public double Fyk
        {
            get => _fyk;
            set => SetProperty(ref _fyk, value);
        }

        private double _gammaS = 1.15;
        public double GammaS
        {
            get => _gammaS;
            set => SetProperty(ref _gammaS, value);
        }

        private double _vRdc;
        public double VRdc
        {
            get => _vRdc;
            set => SetProperty(ref _vRdc, value);
        }
    }
}
