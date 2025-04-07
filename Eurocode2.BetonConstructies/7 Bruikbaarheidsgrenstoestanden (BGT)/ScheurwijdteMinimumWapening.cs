namespace Eurocode.BetonConstructies
{
    public partial class Scheurbeheersing
    {
        /// <summary>
        /// 7.3.2 Oppervlakte van de minimumwapening
        /// </summary>
        public class ScheurwijdteMinimumWapening
        {
            public double AsMin { get
                {
                    return this.FactorKc * this.FactorK * this.FctEff * this.Act / this.SigmaS;
                } }
            public double Act { get; set; }
            public double SigmaS { get { return Beton.BetonStaal.SigmaSd; } } // todo: check
            public double FctEff { get; set; }
            public double FactorK { get; set; }
            public double FactorKc { get; set; }
            public required BetonContext Beton { get; set; }
            

            public void Update()
            {
                
            }
            


            public double GetFactorK(double lijfHoogteOfFlensBreedte)

            {
                // stap1: als h kleiner of gelijk is aan 300, dan geldt k = 1,0 
                // stap2: als h groter of gelijk is dan 800 dan geldt k = 0,65
                // stap3: hiertussen kunnen we lekker interpoleren. (rechtlijnige grafiek dus!!)
                // Methode met if, if else, else (ondergrens, bovengrens, interpolatie)
                if (lijfHoogteOfFlensBreedte <= 300)
                {
                    return 1.0;
                }
                else if (lijfHoogteOfFlensBreedte >= 800)
                {
                    return 0.65;
                }
                else
                {
                    return (lijfHoogteOfFlensBreedte - 300) / 500 * 0.35 + 0.65;
                }
            }   // factor k in berekening scheurwijdte

            public double GetFctEff(BetonContext beton)
            {
                if (beton.Tijdstip < 28)
                {
                    throw new NotImplementedException("tijdstip kleiner dan 28 dagen niet geimplementeerd.");
                }
                else
                {
                    return beton.Fctm;
                }
            }



        }
    }
}
