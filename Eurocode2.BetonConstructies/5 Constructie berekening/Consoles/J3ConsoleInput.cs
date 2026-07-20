namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class J3ConsoleInput : BaseInput
    {
        private double _lc = 280;
        public double Lc
        {
            get => _lc;
            set => SetProperty(ref _lc, value);
        }



        public double FactorHEd => HEd / FEd;
        

        private double _factorZ = 0.9;
        public double FactorZ
        {
            get => _factorZ;
            set => SetProperty(ref _factorZ, value);
        }

        private bool _toonKolom = true;
        public bool ToonKolom
        {
            get => _toonKolom;
            set => SetProperty(ref _toonKolom, value);
        }

        private bool _toonKolomWap = false;
        public bool ToonKolomWap
        {
            get => _toonKolomWap;
            set => SetProperty(ref _toonKolomWap, value);
        }

        private double _hEd = 100;
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

        private double _kolomDikte = 350;
        public double KolomDikte
        {
            get => _kolomDikte;
            set => SetProperty(ref _kolomDikte, value);
        }

        private double _kolomBreedte = 350;
        public double KolomBreedte
        {
            get => _kolomBreedte;
            set => SetProperty(ref _kolomBreedte, value);
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

        private double _nEd = 500;
        public double NEd
        {
            get => _nEd;
            set => SetProperty(ref _nEd, value);
        }


        private double _fEd = 500;
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

        private double _hoofdstaafDiameter = 20;
        public double HoofdstaafDiameter
        {
            get => _hoofdstaafDiameter;
            set => SetProperty(ref _hoofdstaafDiameter, value);
        }

        private double _hoofdstaafAantal = 3;
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
        

        private double _fck = 40;
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

        // ---- noodzakelijk? of verplaatsen
        private double _diamKolomWap = 12;
        public double DiamKolomWap
        {
            get => _diamKolomWap;
            set => SetProperty(ref _diamKolomWap, value);
        }


    }
}
