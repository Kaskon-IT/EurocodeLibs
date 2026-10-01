namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;
    using Eurocode.BetonConstructies.StrutAndTie;
    using Eurocode2.BetonConstructies;
    using ExportFactory.Shared;

    public class J3ConsoleResult : IRowResult
    {
        // Het model
        public Model Model { get; internal set; } = new();





        // Toevoegen aan J3ConsoleResult:
        public List<J3ConsoleStaafgroepToets> StaafgroepToetsen { get; } = [];
        public J3ConsoleTrekbandToets? TrekbandToets { get; set; }

        /// <summary>
        /// Door de builder berekende verankering per wapeninggroep (start-zijde).
        /// Wordt gevuld in <see cref="J3ConsoleWapeningBuilder.VulGroepen"/> en
        /// daarna overgenomen in <see cref="J3ConsoleStaafgroepToetsen.Toets"/>.
        /// </summary>
        public Dictionary<WapeningGroep, List<VerankeringResult>> VerankeringenPerGroep { get; } = [];
        public J3ConsoleInput.RekenMethodeOptie RekenMethode { get; set; } = J3ConsoleInput.RekenMethodeOptie.StrutAndTieModel;

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
        public double Av { get; set; }

        /// <summary>
        /// Afstand van horizontale belasting tot hart trekband
        /// </summary>
        public double Ah { get; set; }

        // Geometrie-invoer die nodig is voor weergave en validaties.

        /// <summary>
        /// Lengte van de console (in de richting van de belasting)
        /// </summary>
        public double Lc { get; set; }

        // kolom
        public double KolomDikte { get; set; }
        public double KolomBreedte { get; set; }

        public double P5x => -KolomDikte + Dekking + DiameterBgl + DiameterMain / 2.0;
        public double BeenlengteMainNaVerankering => -X1/2.0 - P5x;

        

        // Autovalues
        public double RechtDeelMain => BeenlengteMainNaVerankering - DiameterMain / 2.0 - BuigdoorMain / 2.0;

        public double RechtDeelMain2 => Lc - Dekking - DiameterBgl - (BuigdoorMain + DiameterMain) / 2.0 - (Ac - LoadPlateLength / 2.0); 
        
        public double FactorHorizontaal => HEd / FEd;

        /// <summary>
        /// Dekking op de beugel
        /// </summary>
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
        /// Horizontale kracht (rekenwaarde in kN)
        /// </summary>
        public double HEd { get; set; }


        public double DikteOplegmateriaal { get; set; } = 20;
        public bool FlexibelOplegmateriaal { get; set; } = true;
        
        
        //public double DeltaAc => (Hc - D + DikteOplegmateriaal) * FactorHorizontaal;

        public double FEd { get; set; }
        
        public double FEdTotaal => Math.Sqrt(FEd * FEd + HEd * HEd);

        public int Fck { get; set; }
        public double Fcd { get; set; }
        public double Fyd { get; set; }
        public double Fywd { get; set; }

        public double Sigma1RdMax { get; set; }
        public double Sigma2RdMax { get; set; }

        public double X1 { get; set; }

        /// <summary> w1 – breedte loodrecht op de drukstaaf: √(x1²+y1²) [mm] (gedrongen-liggertheorie). </summary>
        public double W1 { get; set; }
        public double A { get; set; }
        public double D { get; set; }
        public double D1 => Hc - D;
        
        
        
        public double DiameterMain { get; set; }
        public double DiameterMainAnchorage { get; set; }
        public double DiameterBeugelAnchorage { get; set; }

        public double DiameterBgl { get; set; }
        public WapeningContext WapKolomMain { get; set; } = new() { Tekst = "8r20"};

        // ---- Afgemaakte wapeninggroepen (gevuld door J3ConsoleWapeningBuilder) ----
        public WapeningGroep? WapBglsHor { get; set; }
        public WapeningGroep? WapBglsVer { get; set; }
        public WapeningGroep? WapVerticaleHaarspelden { get; set; }
        public WapeningGroep? WapHorizontaleHaarspelden { get; set; }


        

        public double Z { get; set; }
        public double ZBer { get; set; }

        public double Z0 { get; set; }
        public double Y1 { get; set; }

        
        public double Fc { get; set; }

        public double F1x { get; set; }
        public double F1y { get; set; }

        public double Ft { get; set; }
        public double Fwd { get; set; }

        public double AsMain { get; set; }
        

        /// <summary> As,min – minimumwapening volgens EC2 7.3.2 vgl. (7.1) [mm²] (gedrongen-liggertheorie). </summary>
        public double AsMin { get; set; }
        public bool AsMinOk => AsMainProv >= AsMin;

        public double AsOplegging { get; set; }

        /// <summary>
        /// Volledige 7.3.2-berekening (incl. HcrFormula, ActFormula, AsMinFormula);
        /// null bij de strut-and-tie methode.
        /// </summary>
        public Scheurbeheersing.ScheurwijdteMinimumWapening? MinimumWapening { get; set; }

        /// <summary>
        /// Scheurwijdtetoetsing 7.3.4 (BGT); null bij de strut-and-tie methode.
        /// </summary>
        public J3ConsoleScheurwijdteResult? Scheurwijdte { get; set; }

        /// <summary>
        /// Dwarskrachtweerstand zonder wapening 6.2.2 (VRd,c); null bij de strut-and-tie methode.
        /// </summary>
        public J3ConsoleDwarskrachtResult? Dwarskracht { get; set; }

        /// <summary> Torsietoets 6.3.2 (TRd,c); null bij de strut-and-tie methode. </summary>
        public J3ConsoleTorsieResult? Torsie { get; set; }

        /// <summary> Combinatietoets dwarskracht + torsie vgl. (6.31); null bij de strut-and-tie methode. </summary>
        public J3ConsoleTorsieDwarskrachtCombinatie? TorsieDwarskrachtCombinatie { get; set; }

        /// <summary> MEd – buigend moment t.p.v. de kolomrand [kNm] (gedrongen-liggertheorie). </summary>
        public double MEd { get; set; }
        public double Asw { get; set; }
        public double Mrand { get; set; }
        public double Mhart => Mrand + FEd * (KolomDikte / 1000.0); 

        public double SigmaNode1Ed { get; set; }
        public double SigmaNode2Ed { get; set; }

        public bool Node1Ok => SigmaNode1Ed <= Sigma1RdMax + 1e-5;
        public bool Node2Ok => SigmaNode2Ed <= Sigma2RdMax + 1e-5;

        public bool HorizontaleBeugelsNodig => Ac <= 0.5 * Hc;
        public bool VerticaleBeugelsNodig => Ac > 0.5 * Hc;


        public double HoogteTpvAc { get; set; }
        public double NutHoogteTpvAc { get; set; }


        public int AantalMain { get; set; }

        // NB1: tweede laag hoofdwapening (0 = niet gebruikt)
        public double DiameterMain2 { get; set; }
        public int AantalMain2 { get; set; }
        public double AsMainProv => AantalMain * WapeningHelper.GetDsnOpp(1, DiameterMain)
                                    + AantalMain2 * WapeningHelper.GetDsnOpp(2, DiameterMain2);
        
        
        public Formula AsMainProvFormula
        {
            get
            {
                var sgPlat = this.WapHorizontaleHaarspelden;
                var sgNorm = this.WapVerticaleHaarspelden;

                //var aantallen = new List<int>() { AantalMain, AantalMain2 };
                //var diameters = new List<double>() { DiameterMain, DiameterMain2 };
                var strings = new List<string>();

                //for (int i = 0; i< 2; i++)
                //{
                //    if (aantallen[i] > 0)
                //    {
                //        strings.Add($"{aantallen[i]}Ø{diameters[i]}");
                //    }
                //}

                if (sgPlat is not null &&
                    sgPlat.AantalPosities > 0 &&
                    sgPlat.Diameter > 0)
                {
                    strings.Add($"{sgPlat.AantalPosities}{sgPlat.Prefix}Ø{sgPlat.Diameter}");
                }

                if (sgNorm is not null &&
                    sgNorm.AantalPosities > 0 &&
                    sgNorm.Diameter > 0)
                {
                    strings.Add($"{sgNorm.AantalPosities}{sgNorm.Prefix}Ø{sgNorm.Diameter}");
                }


                return new()
                {
                    StaticValue = @$"A_{{s,main,prov}} = {string.Join(" + ", strings)}",
                    DynamicValue = @$"= {AsMainProv:0} \text{{ mm}}^2"
                    
                };

            }
        }
        
        public bool AsMainProvOk => AsMainProv >= AsMain;
        public double AswProv => AantalBeugels * WapeningHelper.GetDsnOpp(2, DiameterBgl);
        public bool AswProvOk => AswProv >= Asw;



        public double BuigdoorMain { get; set; }
        public bool UseAnchorageBar { get; set; }
        

        // helpers (overzicht)
        public string OverzichtBoven
        {
            get
            {
                List<string> strValues = [AsMain.ToString("0")];
                if ( Torsie is not null)
                {
                    strValues.Add(Torsie.AslBovenOnder.ToString("0"));
                } 
                return string.Join(", ", strValues);
            }
        }

        public string OverzichtOnder
        {
            get
            {
                if (Torsie is not null)
                {
                    return Torsie.AslBovenOnder.ToString("0");
                }
                else
                {
                    return "n.v.t.";
                }
            }
        }

        public string OverzichtZijkant
        {
            get
            {
                List<string> strValues = [];
                if (HorizontaleBeugelsNodig)
                {
                    strValues.Add(AsLnkFormula.FullValue);
                }

                if (Torsie is not null)
                {
                    strValues.Add(Torsie.AslLinksRechts.ToString("0"));
                }
                return string.Join(", ", strValues);
            }
        }

        public string OverzichtBeugels
        {
            get
            {
                List<string> strValues = [];
                if (VerticaleBeugelsNodig)
                {
                    strValues.Add(AsLnkFormula.FullValue);
                }
                if (Dwarskracht is not null)
                {
                    strValues.Add(Dwarskracht.AswV.ToString("0"));
                }


                if (Torsie is not null)
                {
                    strValues.Add(Torsie.AswTFormula.FullValue);
                }
                return string.Join(", ", strValues);
            }
        }

       
        public double BuigdoornMainReq(double afstandTotAanOmbuiging = 0)
        {
            var factor = ResterendPercentageVerankeringsLengte(afstandTotAanOmbuiging);
            var buigdoornMin1 = WapeningHelper.GetBuigdoorMin(DiameterMain);
            var buigdoornMin2 = WapeningHelper.GetBuigdoornMin(Fcd, D1, MainFbt(afstandTotAanOmbuiging), DiameterMain);
            return Math.Max(buigdoornMin1, buigdoornMin2);
            
        }
        
                


        public double MainFy
        {
            get
            {
                return AsMain / AsMainProv * Fyd;
            }
        }



        public double MainFbt(double afstandTotOmbuiging)
        {
            var restant = ResterendPercentageVerankeringsLengte(afstandTotOmbuiging);
           
            return restant * MainFy * WapeningHelper.GetDsnOpp(1, DiameterMain);
            
        }

        
        /// <summary>
        /// Geeft het resterende percentage (van 100% tot 0%) bij ontwikkelde verankeringslengte.
        /// Bij een afstand van 0 -> 100%
        /// Bij een afstand van 30% -> 70%
        /// Bij de volledige lengte 0% 
        /// Lineair 
        /// </summary>
        /// <param name="ontwikkeldeLengte">De ontwikkelde lengte van de verankeringslengte is </param>
        /// <returns>Resterend percentage van de verankering</returns>
        public double ResterendPercentageVerankeringsLengte(double ontwikkeldeLengte)
        {
            // Bij afstand 0 is per definitie 100% over; dit voorkomt bovendien
            // oneindige recursie via VerankeringsLengteReq -> MainFbt(0) -> hier.
            if (ontwikkeldeLengte <= 0)
                return 1.0;

            // Referentie is de verankeringslengte bij volledige staafkracht (afstand 0),
            // zodat er geen cyclische afhankelijkheid met VerankeringsLengteReq ontstaat.
            double referentie = VerankeringsLengteBasis;
            if (referentie <= 0)
                return 0;

            return Math.Clamp(1.0 - ontwikkeldeLengte / referentie, 0.0, 1.0);
        }


        /// <summary>
        /// Verankeringslengte bij volledige staafkracht (fbt op afstand 0).
        /// Dient als vaste referentie voor <see cref="ResterendPercentageVerankeringsLengte"/>.
        /// Gecachet: de onderliggende berekening is relatief duur en wordt vaak aangeroepen.
        /// </summary>
        private double? _verankeringsLengteBasis;
        public double VerankeringsLengteBasis
        {
            get
            {
                if (_verankeringsLengteBasis is null)
                {
                    double ab = D1;
                    double fbt = MainFbt(0);
                    double benutting = MainFy / Fyd;

                    _verankeringsLengteBasis = WapeningHelper.GetVerankeringResult(ab, fbt, this.DiameterMain, this.Fck, benutting).Verankeringslengte;
                }
                return _verankeringsLengteBasis.Value;
            }
        }


        public double VerankeringsLengteReq => VerankeringMain.Verankeringslengte;


        private GedrongenUitkragingResult? _gedrongenUitkraging;
        public GedrongenUitkragingResult GedrongenUitkraging
        {
            get
            {
                if (_gedrongenUitkraging is null)
                {
                    var ab = LoadPlateLength;
                    var ac = Ac;
                    var h = Hc;
                    var l = Lc;

                    var input = new GedrongenUitkragingInput() {
                        Ac = ac,
                        H = h,
                        L = l,
                        Ab = ab
                    };

                    _gedrongenUitkraging = GedrongenUitkragingCalculator.Calculate(input);
                }
                return _gedrongenUitkraging;
            }
        }



        private VerankeringResult? _verankeringContext;
        public VerankeringResult VerankeringContext
        {
            get
            {
                if (_verankeringContext is null)
                {
                    double ab = D1;
                    double fbt = MainFbt(0);
                    double benutting = 1.0;
                    _verankeringContext = WapeningHelper.GetVerankeringResult(ab, fbt, this.DiameterMain, this.Fck, benutting);
                }
                return _verankeringContext;
            }
        }


        private VerankeringResult? _verankeringMain;
        public VerankeringResult VerankeringMain
        {
            get
            {
                if (_verankeringMain is null)
                {
                    double ab = D1;
                    double fbt = MainFbt(RechtDeelMain);
                    double benutting = MainFy / Fyd;

                    _verankeringMain = WapeningHelper.GetVerankeringResult(ab, fbt, this.DiameterMain, this.Fck, benutting);
                }
                return _verankeringMain;
            }
        }


        private VerankeringResult? _verankeringMainConsoleZijde;
        public VerankeringResult VerankeringMainConsoleZijde
        {
            get
            {
                if (_verankeringMainConsoleZijde is null)
                {
                    double ab = D1;
                    double fbt = MainFbt(RechtDeelMain2);
                    double benutting = MainFy / Fyd;

                    _verankeringMainConsoleZijde = WapeningHelper.GetVerankeringResult(ab, fbt, this.DiameterMain, this.Fck, benutting);
                }
                return _verankeringMainConsoleZijde;
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


        public Formula AsMainFormula
        {
            get
            {
                switch (RekenMethode)
                {
                    case J3ConsoleInput.RekenMethodeOptie.StrutAndTieModel:
                        return new()
                        {
                            StaticValue = @"A_{s,main,req} = \frac{F_{t}}{f_{yd}}",
                            DynamicValue = @$"= \frac{{{(Ft * 1000):0}}}{{{Fyd:0}}} = {AsMain:0} \text{{ mm}}^2"
                        };
                    case J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie:
                        return new()
                        {
                            StaticValue = @"A_{s,main,req} = \frac{M_{Ed}}{f_{yd} \cdot z}",
                            DynamicValue = @$"= \frac{{{this.MEd:0}}}{{{this.Fyd:0}}} \cdot {this.Z} = {AsMain:0} \text{{ mm}}^2"
                        };
                    default:
                        return new()
                        {
                            StaticValue = @"A_{s,main,req} = ....",
                            DynamicValue = @$"= {AsMain:0} \text{{ mm}}^2"
                        };
                } 
            }
        } 
        

        public Formula AsLnkFormula
        {
            get
            {
                if (HorizontaleBeugelsNodig)
                {
                    return new()
                    {
                        StaticValue = @"\Sigma A_{s,lnk} \geq k1 \cdot A_{s,main}",
                        DynamicValue = $@"\geq 0.25 \cdot {AsMain:0} \geq {(0.25*AsMain):0} \text{{ mm}}^2",
                    };
                }
                else return new()
                {
                    StaticValue = @"\Sigma A_{s,lnk} \geq k2 \cdot \frac{F_{Ed}}{f_{yd}}",
                    DynamicValue = $@"\geq 0.50 \cdot \frac{{{FEd:0}}}{{{Fyd:0}}} \geq {(0.5*FEd/Fyd):0} \text{{ mm}}^2",
                };
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

                // buigstaal
                if (RechtDeelMain < 0)
                {
                    fouten.Add("Buigdoorn te groot");
                }

                //if (RechtDeelMain < BenodigdeRechteDeel)
                //{
                //    fouten.Add("Buigdoorn niet akkoord");
                //    fouten.Add($"recht deel benodigd = {BenodigdeRechteDeel:0} mm, beschikbaar = {RechtDeelMain:0} mm");

                //}


                // tan(theta) moet tussen 1 en 2,5 liggen.
                if (TanTheta < 1.0 || TanTheta > 2.5)
                    fouten.Add($"tan(theta) = {N(TanTheta)} valt buiten [1 ; 2,5].");

                // Z0 mag niet kleiner zijn dan ac.
                if (Z0 < Ac)
                    fouten.Add($"z0 ({N(Z0)} mm) > ac ({N(Ac)} mm).");

                if (MainFy > Fyd)
                {
                    fouten.Add($"Hoofdwapening niet akkoord (fy = {MainFy:0} N/mm²)");

                }

                if (BuigdoorMain < VerankeringMain.MinimaleBuigdoornDiameter)
                {
                    fouten.Add($"Buigdoorn in kolom niet akkoord (minimaal {VerankeringMain.MinimaleBuigdoornDiameter:0} mm)");
                }
                if (BuigdoorMain < VerankeringMainConsoleZijde.MinimaleBuigdoornDiameter)
                {
                    fouten.Add($"Buigdoorn in consol niet akkoord");
                }


                if (!Node1Ok)
                {
                    fouten.Add("Drukknoop niet akkoord");
                    fouten.Add($"sigma_Ed = {Node1?.SigmaEd}");
                    fouten.Add($"sigma_Rd = {Node1?.SigmaRdMax}");

                }
                if (!Node2Ok)
                    fouten.Add("Knoop tpv oplegging niet akkoord");

                

                return fouten;
            }
        }

        /// <summary>
        /// Helper om de verankering in te stellen
        /// </summary>
        /// <param name="groep"></param>
        /// <param name="verankering"></param>
        public void SetVerankering(
    WapeningGroep groep,
    VerankeringResult verankering)
        {
            if (!VerankeringenPerGroep.TryGetValue(groep, out var verankeringen))
            {
                verankeringen = [];
                VerankeringenPerGroep[groep] = verankeringen;
            }

            var index = verankeringen.FindIndex(x => x.Id == verankering.Id);

            if (index >= 0)
                verankeringen[index] = verankering;
            else
                verankeringen.Add(verankering);
        }




    }
}
