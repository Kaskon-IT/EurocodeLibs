namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class J3ConsoleInput : BaseInput
    {
        private double _lc = 250;
        public double Lc
        {
            get => _lc;
            set => SetProperty(ref _lc, value);
        }



        public double FactorHEd => HEd / FEd;
        

        private double _factorZ = 0.8;
        public double FactorZ
        {
            get => _factorZ;
            set => SetProperty(ref _factorZ, value);
        }

        private double _hEd = 0;
        public double HEd
        {
            get => _hEd;
            set => SetProperty(ref _hEd, value);
        }

        private double _dikteOplegmateriaal = 20;
        public double DikteOplegmateriaal
        {
            get => _dikteOplegmateriaal;
            set => SetProperty(ref _dikteOplegmateriaal, value);
        }

        

        private double _bc = 350;
        public double Bc
        {
            get => _bc;
            set => SetProperty(ref _bc, value);
        }

        private double _kolomDikte = 300;
        public double KolomDikte
        {
            get => _kolomDikte;
            set => SetProperty(ref _kolomDikte, value);
        }


        private double _hc = 450;
        public double Hc
        {
            get => _hc;
            set => SetProperty(ref _hc, value);
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

        private double _hoofdstaafAantal = 4;
        public double HoofdstaafAantal
        {
            get => _hoofdstaafAantal;
            set => SetProperty(ref _hoofdstaafAantal, value);
        }

        private double _hoofdstaafBuigdoornDiameterFactor = 10;
        public double HoofdstaafBuigdoornDiameterFactor
        {
            get => _hoofdstaafBuigdoornDiameterFactor;
            set => SetProperty(ref _hoofdstaafBuigdoornDiameterFactor, value);
        }
        

        private double _fck = 30;
        public double Fck
        {
            get => _fck;
            set => SetProperty(ref _fck, value);
        }

        private double _alphaCc = 1.0;
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
