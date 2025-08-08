using CommonLibrary;

namespace Eurocode.BetonConstructies
{
    public class UitkragingContext
    {
        internal double AsBen;

        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } = new(1000, 100);

        public BetonContext Beton { get; set; } = new();



        public double TandBreedte { get; set; } = 1000;

        public double TandLengte { get; set; } = 100;
        public double LengteOverspanning { get { return 2 * ArmReactieVerticaal; } }

        public bool IsGedrongenLigger { get { return Schematisering.IsGedrongenLigger(LengteOverspanning, TandHoogte); } }



        public double TandHoogte { get; set; } = 100;
        public double TandNuttigeHoogte { get; set; } = 100 - 20 - 4;



        public double Reactiekracht { get; set; } = 8;



        public double VerhoudingHorizontaal { get; set; } = 0.3;
        public double ReactieKrachtHorizontaal { get { return VerhoudingHorizontaal * Reactiekracht; } }


        public double HoogteHals { get; set; } = 100;

        //public TandMetHals Tand { get; set; } = new();
        public double ArmReactieVerticaal { get { return TandLengte / 2 + HoogteHals / 2; } }
        public double ArmReactieHorizontaal { get { return TandHoogte / 2; } }

        public double Moment
        {
            get
            {
                return this.ArmReactieVerticaal * 1e-3 * this.Reactiekracht + this.ArmReactieHorizontaal * 1e-3 * this.ReactieKrachtHorizontaal;
            }
        }


        public BetonConstructies.BuigingBasic BuigingTand { get; set; }

        public BendingResults Buiging1 { get; set; }

        public DwarskrachtWapContext TandShear { get; set; }
        public Snedekrachten Snedekrachten { get; set; } = new Snedekrachten();






        public void Bereken()
        {
            //Tand.BerekeningNeus(this);
            BuigingTand = new() { Beton = this.Beton, D = this.TandNuttigeHoogte, M = this.Moment };

            //Buiging1 = new(Beton, TandBreedte, TandLengte, 24, Moment);

            ParametrischeProfielen.ParametrischProfielContext profiel = new(TandBreedte, TandHoogte);

            TandShear = new(Beton, profiel, Snedekrachten) { NutHoogte = this.TandNuttigeHoogte };


        }

    }









}
