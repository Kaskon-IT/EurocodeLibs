namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class J3ConsoleResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];

        public double H { get; set; }
        public double B { get; set; }
        public double Ac { get; set; }

        // Geometrie-invoer die nodig is voor weergave en validaties.
        public double L { get; set; }
        public double Dekking { get; set; }
        public double LoadPlateLength { get; set; }
        public double LoadPlateWidth { get; set; }

        public double FactorHorizontaal { get; set; } = 0.4;
        public double FhEd => FactorHorizontaal * FvEd;

        public double DikteOplegmateriaal { get; set; } = 20;
        public double DeltaAc => (H - D + DikteOplegmateriaal) * FactorHorizontaal;

        public double FvEd { get; set; }
        public double FEd => Math.Sqrt(FvEd * FvEd + FhEd * FhEd);

        public double Fcd { get; set; }
        public double Fyd { get; set; }

        public double Sigma1RdMax { get; set; }
        public double Sigma2RdMax { get; set; }

        public double X1 { get; set; }
        public double A { get; set; }
        public double D { get; set; }
        public double D1 => H - D;
        public double DiameterMain { get; set; }
        public double DiameterBgl { get; set; }
        
        public double Z { get; set; }

        public double Z0 { get; set; }
        public double Y1 { get; set; }

        
        public double Fc { get; set; }

        public double F1x { get; set; }
        public double F1y { get; set; }

        public double Ft { get; set; }
        public double Fwd { get; set; }

        public double AsMain { get; set; }
        public double Asw { get; set; }

        public double SigmaNode1Ed { get; set; }
        public double SigmaNode2Ed { get; set; }

        public bool Node1Ok => SigmaNode1Ed <= Sigma1RdMax;
        public bool Node2Ok => SigmaNode2Ed <= Sigma2RdMax;

        public bool HorizontaleBeugelsNodig => Ac <= 0.5 * H;
        public bool VerticaleBeugelsNodig => Ac > 0.5 * H;

        public int AantalStaven
        {
            get
            {
                var asMainPerStaaf = WapeningHelper.GetDsnOpp(1, DiameterMain);
                return (int)Math.Ceiling(AsMain / asMainPerStaaf);
            }
        } 

        public int AantalBeugels
        {
            get
            {
                var AswPerBeugel = WapeningHelper.GetDsnOpp(2, DiameterBgl);
                var benodigd = (int)Math.Ceiling(Asw / AswPerBeugel);
                int minimum = 3;

                return Math.Max(benodigd, minimum);
            }
        }


        public double Nu { get; set; } // (6.57N)

        public double Sigma3RdMax { get; set; }    // CTT



        public double TanTheta { get; set; }
        public double ThetaDeg { get; set; }
        public bool IsThetaOk => TanTheta >= 1.0 && TanTheta <= 2.5;
        public bool IsZ0Ok => Z0 > Ac;

        public J3ConsoleLinkType LinkType { get; set; }



        public List<J3ConsoleNodeResult> Nodes { get; set; } = [];

        public J3ConsoleNodeResult? Node1 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 1");

        public J3ConsoleNodeResult? Node2 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 2");

        /// <summary>
        /// Validatiefouten op de consoleberekening. Gedeeld door alle weergaven
        /// (SVG/HTML/Razor) zodat ze dezelfde lijst tonen.
        /// </summary>
        public List<string> Validaties
        {
            get
            {
                static string N(double v) => v.ToString("0.##");

                var fouten = new List<string>();

                // Breedte oplegplaat mag niet groter zijn dan de breedte van de console.
                if (LoadPlateWidth > B)
                    fouten.Add($"Breedte oplegplaat ({N(LoadPlateWidth)} mm) > breedte console ({N(B)} mm).");

                // Lengte oplegplaat mag niet groter zijn dan de lengte van de console - dekking.
                double maxPlaatLengte = L - Dekking;
                if (LoadPlateLength > maxPlaatLengte)
                    fouten.Add($"Lengte oplegplaat ({N(LoadPlateLength)} mm) > lengte console - dekking ({N(maxPlaatLengte)} mm).");

                // De rand van de oplegplaat mag niet voorbij de dekking vallen.
                double randX = L - Dekking;
                double maxPlaatX = Ac + LoadPlateLength / 2.0;
                if (maxPlaatX > randX)
                    fouten.Add("Oplegplaat steekt voorbij de console");
                double minPlaatX = Ac - LoadPlateLength / 2.0;
                if (minPlaatX < 0)
                    fouten.Add("Oplegplaat steekt voorbij de oplegging");

                // tan(theta) moet tussen 1 en 2,5 liggen.
                if (TanTheta < 1.0 || TanTheta > 2.5)
                    fouten.Add($"tan(theta) = {N(TanTheta)} valt buiten [1 ; 2,5].");

                // Z0 mag niet kleiner zijn dan ac.
                if (Z0 < Ac)
                    fouten.Add($"z0 ({N(Z0)} mm) > ac ({N(Ac)} mm).");

                return fouten;
            }
        }



    }
}
