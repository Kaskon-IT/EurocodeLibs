namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;

    public class J3ConsoleResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];

        /// <summary>
        /// Hoogte van de console
        /// </summary>
        public double Hc { get; set; }

        /// <summary>
        /// Breedte van de console
        /// </summary>
        public double Bc { get; set; }

        /// <summary>
        /// Afstand van belasting tot rand kolom/wand
        /// </summary>
        public double Ac { get; set; }

        /// <summary>
        /// Afstand van horizontale belasting tot hart trekband
        /// </summary>
        public double Ah { get; set; }

        // Geometrie-invoer die nodig is voor weergave en validaties.

        /// <summary>
        /// Lengte van de console (in de richting van de belasting)
        /// </summary>
        public double Lc { get; set; }
        public double KolomDikte { get; set; }
        
        
        public double Dekking { get; set; }

        /// <summary>
        /// Lengte van de oplegplaat (in de richting van de belasting)
        /// </summary>
        public double LoadPlateLength { get; set; }

        /// <summary>
        /// Breedte van de oplegplaat (loodrecht op de richting van de belasting)
        /// </summary>
        public double LoadPlateWidth { get; set; }

        /// <summary>
        /// Factor voor de horizontale kracht. Wordt gebruikt om FhEd te berekenen uit FvEd.
        /// </summary>
        public double FactorHorizontaal { get; set; } = 0.4;
        public double HEd { get; set; }

        //public double FhEd => FactorHorizontaal * FvEd;

        public double DikteOplegmateriaal { get; set; } = 20;
        public double DeltaAc => (Hc - D + DikteOplegmateriaal) * FactorHorizontaal;

        public double FEd { get; set; }
        
        public double FEdTotaal => Math.Sqrt(FEd * FEd + HEd * HEd);

        public int Fck { get; set; }
        public double Fcd { get; set; }
        public double Fyd { get; set; }

        public double Sigma1RdMax { get; set; }
        public double Sigma2RdMax { get; set; }

        public double X1 { get; set; }
        public double A { get; set; }
        public double D { get; set; }
        public double D1 => Hc - D;
        
        
        
        public double DiameterMain { get; set; }
        public double DiameterMainAnchorage { get; set; }
        public double DiameterBeugelAnchorage { get; set; }

        public double DiameterBgl { get; set; }
        public WapeningContext WapKolomMain { get; set; } = new() { Tekst = "8r20"};
        public WapeningContext WapKolomBeugels { get; set; } = new() { Tekst = "8-150"};
        public WapeningContext WapConsoleMain { get; set; } = new() { Tekst = "3r16" };


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
        public double Mrand { get; set; }
        public double Mhart => Mrand + FEd * (KolomDikte / 1000.0); 

        public double SigmaNode1Ed { get; set; }
        public double SigmaNode2Ed { get; set; }

        public bool Node1Ok => SigmaNode1Ed <= Sigma1RdMax;
        public bool Node2Ok => SigmaNode2Ed <= Sigma2RdMax;

        public bool HorizontaleBeugelsNodig => Ac <= 0.5 * Hc;
        public bool VerticaleBeugelsNodig => Ac > 0.5 * Hc;

        public int AantalStaven
        {
            get
            {
                var asMainPerStaaf = WapeningHelper.GetDsnOpp(1, DiameterMain);
                return (int)Math.Ceiling(AsMain / asMainPerStaaf);
            }
        } 

        
        public int AantalMain { get; set; }
        public double BuigdoorMain { get; set; }

        public double BuigstraalMainReq
        {
            get
            {
                var buigdoornMin1 = WapeningHelper.GetBuigdoorMin(DiameterMain);
                var buigdoornMin2 = WapeningHelper.GetBuigdoornMin(Fcd, D1, MainFbt, DiameterMain);
                return Math.Max(buigdoornMin1, buigdoornMin2) / 2;
            }
        }


        


        public double MainFy
        {
            get
            {
                var applied = new WapeningContext($"{AantalMain}r{DiameterMain}", Dekking);
                return AsMain / applied.As * Fyd;
            }
        }



        public double MainFbt
        {
            get
            {
                return MainFy * WapeningHelper.GetDsnOpp(1, DiameterMain);
            }
        }


        public double VerankeringsLengteReq
        {
            get
            {
                return WapeningHelper.GetLbReq(D1, MainFbt, DiameterMain, Fck);
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


        public List<StrutAndTie.StrutAndTieNode> StrutAndTieNodes { get; set; } = [];
        public StrutAndTie.StrutAndTieNode? STN1 =>
            StrutAndTieNodes.FirstOrDefault(n => n.Id == "node-1");
        public StrutAndTie.StrutAndTieNode? STN2 =>
            StrutAndTieNodes.FirstOrDefault(n => n.Id == "node-2");


        /// <summary>
        /// Maakt knoop 1 (CCC) en knoop 2 (CCT) voor het
        /// strut-and-tie-model van de console.
        /// </summary>
        /// <param name="node1Center">
        /// Hart van de onderste CCC-knoop, in mm.
        /// </param>
        /// <param name="node2Center">
        /// Hart van de bovenste CCT-knoop, in mm.
        /// </param>
        /// <param name="fvEd">
        /// Verticale kracht in N.
        /// </param>
        /// <param name="ftEd">
        /// Horizontale trekbandkracht in N.
        /// </param>
        /// <param name="width">
        /// Breedte van de console loodrecht op het STM-vlak, in mm.
        /// </param>
        /// <param name="sigmaCccRdMax">
        /// Toelaatbare knoopspanning van knoop 1, in N/mm².
        /// </param>
        /// <param name="sigmaCctRdMax">
        /// Toelaatbare knoopspanning van knoop 2, in N/mm².
        /// </param>
        public void CreateNodes(
            StrutAndTie.Point2D node1Center,
            StrutAndTie.Point2D node2Center,
            StrutAndTie.Vector2D f1,
            StrutAndTie.Vector2D f2,
            double width,
            double sigmaCccRdMax,
            double sigmaCctRdMax)
        {

            var node1 = new StrutAndTie.StrutAndTieNode()
            {
                Name = "Knoop 1",
                Id = "node-1",
                Type = StrutAndTie.StrutAndTieNodeType.CCC,
                Geometry = new StrutAndTie.StrutAndTieNodeGeometry()
                {
                    Center = node1Center
                }
                // waar width?
                // waar sigma
                // waar vector voor F
            };

            var node2 = new StrutAndTie.StrutAndTieNode()
            {
                Name = "Knoop 2",
                Id = "node-2",
                Type = StrutAndTie.StrutAndTieNodeType.CCT,
                Geometry = new StrutAndTie.StrutAndTieNodeGeometry()
                {
                    Center = node2Center
                }
            };

            StrutAndTieNodes.Add(node1);
            StrutAndTieNodes.Add(node2);
        }



        public List<J3ConsoleNodeResult> Nodes { get; set; } = [];

        public J3ConsoleNodeResult? Node1 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 1");

        public J3ConsoleNodeResult? Node2 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 2");

        /// <summary>
        /// Uitgebreid rekenvoorbeeld als markdown (met LaTeX-formules),
        /// gegenereerd door <see cref="J3ConsoleRekenvoorbeeld"/>.
        /// Kan met Markdig + KaTeX op scherm of in een rapport worden getoond.
        /// </summary>
        public string RekenvoorbeeldMarkdown { get; set; } = string.Empty;

        /// <summary>
        /// Eenvoudig strut-and-tie schema (knopen, drukdiagonalen, trekband en
        /// krachten) als schaalbare SVG. Gegenereerd door
        /// <see cref="J3ConsoleSvg.CreateSchemaSvg"/> en o.a. gebruikt in het
        /// markdown-rekenvoorbeeld.
        /// </summary>
        public string SchemaSvg { get; set; } = string.Empty;

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

                // FEd mag niet negatief! 
                if (FEd <= 0)
                {
                    fouten.Add($"F<sub>Ed</sub> moet groter dan 0 zijn.");
                }

                if (FEd < Math.Abs(HEd))
                {
                    fouten.Add("F moet groter of gelijk zijn aan H");
                }

                if (HEd < 0)
                {
                    fouten.Add("H mag niet kleiner dan 0 zijn.");
                }


                // Breedte oplegplaat mag niet groter zijn dan de breedte van de console.
                if (LoadPlateWidth > Bc)
                    fouten.Add($"Breedte oplegplaat ({N(LoadPlateWidth)} mm) > breedte console ({N(Bc)} mm).");

                // Lengte oplegplaat mag niet groter zijn dan de lengte van de console - dekking.
                double maxPlaatLengte = Lc - Dekking;
                if (LoadPlateLength > maxPlaatLengte)
                    fouten.Add($"Lengte oplegplaat ({N(LoadPlateLength)} mm) > lengte console - dekking ({N(maxPlaatLengte)} mm).");

                // De rand van de oplegplaat mag niet voorbij de dekking vallen.
                double randX = Lc - Dekking;
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

                if (MainFy > Fyd)
                    fouten.Add("Hoofdwapening niet akkoord");

                if (!Node1Ok)
                    fouten.Add("Drukknoop niet akkoord");
                if (!Node2Ok)
                    fouten.Add("Knoop tpv oplegging niet akkoord");

                

                return fouten;
            }
        }



    }
}
