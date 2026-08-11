namespace Eurocode.BetonConstructies
{
    public static class J3ConsoleCalculator
    {
        public static J3ConsoleResult Bereken(J3ConsoleInput i)
        {
            double fcd = i.AlphaCc * i.Fck / i.GammaC;
            double fyd = i.Fyk / i.GammaS;
            double fywd = 0.8 * i.Fyk; // UITGANGSPUNT: f_ywd = 80% * f_yk
            double nu = 0.6 * (1.0 - i.Fck / 250.0); // 6.2.2 (6)
            double nu1 = 0.6; // UITGANGSPUNT: f_ywd = 80% * f_yk 
            double factorZ = i.FactorZ;
            double sigma1RdMax = 1.00 * nu * fcd; // CCC
            double sigma2RdMax = 0.85 * nu * fcd; // CCT
            double sigma3RdMax = 0.75 * nu * fcd; // CTT

            // J3: bepaal type aanvullende beugels
            J3ConsoleLinkType linkType = BepaalLinkType(i);

            // d is afhanklijk van beugelvorm (horizontaal of vertikaal)
            double d = 0.0;

            // effectieve hoogte
            switch (linkType)
            {
                case J3ConsoleLinkType.Geen: 
                case J3ConsoleLinkType.HorizontaalOfSchuin: 
                    d = i.Hc - i.Dekking - i.BeugelDiameter - 0.5 * i.HoofdstaafDiameter;
                    // nu altijd vertikale beugels toepassen of iig verwerken in de nuttige hoogte
                    break;
                case J3ConsoleLinkType.Verticaal:
                    d = i.Hc - i.Dekking - i.BeugelDiameter - 0.5 * i.HoofdstaafDiameter; break;

            }

            // Optionele opgave d1: d = Hc - d1 (bijv. papieren voorbeeld: d1 = 50 -> d = 450)
            if (i.D1Opgave > 0)
            {
                d = i.Hc - i.D1Opgave;
            }



            // x1 uit druksterkte node 1 
            double sigmaToelaatbaar = sigma1RdMax;
            double x1Min = i.Dekking;
            double x1 = Math.Max(i.FEd * 1000.0 / (sigmaToelaatbaar * i.Bc),x1Min);
            double deltaA = i.FactorHEd * (i.Hc - d);

            // a = ac + x1/2 + deltaA
            double a = i.Ac + x1 / 2.0 + deltaA;
            // volgens jouw voorbeeld
            double z = factorZ * d;

            double zBer = BerekenHoogteZ(i.FEd * 1000, a, d, i.Bc, sigma1RdMax);

            z = Math.Min(z, zBer);

            if (i.RekenMethode == J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie)
            {
                // 6.1 (10): z volgens gedrongen-liggertheorie, z = min(0.4a + 0.4h ; 1.6a)
                var gedrongen = GedrongenUitkragingCalculator.Calculate(new GedrongenUitkragingInput
                {
                    Ac = i.Ac,
                    H = i.Hc,
                    L = i.Lc,
                    Ab = i.LoadPlateLength
                });
                a = gedrongen.A;
                z = gedrongen.Z;
            }

            double z0 = z * (i.Ac + deltaA) / a;
            double tanTheta = z / a;
            double thetaDeg = Math.Atan(tanTheta) * 180.0 / Math.PI;

            double d1 = i.Hc - d;
            double ah = i.DikteOplegmateriaal + d1;
            double ft = i.HEd + i.FEd * a / z;
            double f1x = i.FEd * a / z; // 
            double f1y = i.FEd;
            double f1c = Math.Sqrt(f1x * f1x + f1y * f1y);


            // Logica: gebruik
            double y1 = 2 * (d - z);

            // Main reinforcement
            double asMain = ft * 1000.0 / fyd;

            double mEd = 0;
            double asMin = 0;
            double asOplegging = 0;
            bool flexibelOplegmateriaal = i.FlexibelOplegmateriaal;

            if (flexibelOplegmateriaal)
            {
                asOplegging = i.FEd * 1000.0 / fyd * 0.25 * (i.DikteOplegmateriaal / i.LoadPlateWidth);
            }

            double w1 = 0;

            // aavnulling voor verjonging
            double hoogteTpvAc = i.AfschuiningOnderzijde ? i.Hc / 2.0 + ((1 - (i.Ac / i.Lc)) * i.Hc / 2.0) : i.Hc;
            double nutHoogteTpvAc = hoogteTpvAc - d1;


            Scheurbeheersing.ScheurwijdteMinimumWapening? minWap = null;
            J3ConsoleScheurwijdteResult? scheurwijdte = null;
            J3ConsoleDwarskrachtResult? dwarskracht = null;
            J3ConsoleTorsieResult? torsie = null;
            J3ConsoleTorsieDwarskrachtCombinatie? torsieCombinatie = null;
            
            if (i.RekenMethode == J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie)
            {
                // Knoop 1 (onderin): lengte x1 = min(ab ; L/2 ; H/2), consistent met ar in 6.1 (10)
                x1 = new[] { i.LoadPlateLength, i.Lc / 2.0, i.Hc / 2.0 }.Min();

                // y1 zó dat de spanning op het verticale knoopvlak gelijk is aan de
                // spanning onderin: sigma = F/(b*x1) = F1x/(b*y1)  =>  y1 = x1 * F1x / F
                y1 = x1 * f1x / i.FEd;

                // Breedte loodrecht op de drukstaaf; hierop is de spanning dan eveneens gelijk:
                // sigma = Fc/(b*w1) met w1 = sqrt(x1^2 + y1^2)
                w1 = Math.Sqrt(x1 * x1 + y1 * y1);

                // 6.1 (10): As = MEd / (fyd * z) met MEd = a*F + (z + Hc - d) * H  [N*mm]
                mEd = a * i.FEd * 1000 + (z + i.Hc - d) * i.HEd * 1000;
                asMain = mEd / (fyd * z);
                ft = asMain * fyd / 1000.0; // bijbehorende trekbandkracht [kN]

                // 7.3.2 (7.1): As,min = kc * k * fct,eff * Act / sigma_s
                // met normaalkracht N = H,BGT (trek) en trekzonehoogte hcr uit de
                // lineaire spanningsverdeling (sigma_boven = fct,eff bij scheurvorming).
                var beton = new BetonContext((int)i.Fck);
                double fctEff = beton.Fctm;

                minWap = new Scheurbeheersing.ScheurwijdteMinimumWapening
                {
                    Beton = beton,
                    B = i.Bc,
                    H = i.Hc,
                    N = i.HBgt, // trekkracht [kN]
                    FctEff = fctEff,
                    FactorK = 1.0, // doorsnedebreedte <= 300 mm (conform papieren voorbeeld)
                    SigmaS = i.Fyk
                };

                // kc volgens vgl. (7.2); sigma_c = NEd/(b*h), trek negatief; k1 = 2h*/(3h) bij trek
                double hStar = Math.Min(i.Hc, 1000.0);
                double k1 = 2.0 * hStar / (3.0 * i.Hc);
                minWap.FactorKc = Math.Min(0.4 * (1.0 - (-minWap.SigmaN) / (k1 * (i.Hc / hStar) * fctEff)), 1.0);

                minWap.Act = i.Bc * minWap.Hcr;
                asMin = minWap.AsMin;

                // 7.3.4: scheurwijdtetoetsing in BGT
                double asProv = i.WapVerticaleHaarspelden.TotaalAs
                              + i.WapHorizontaleHaarspelden.TotaalAs;
                scheurwijdte = J3ConsoleScheurwijdteCalculator.Bereken(i, a, z, d, asProv, beton);

                // 6.2.2: dwarskrachtweerstand zonder wapening (VRd,c) met NEd = -HEd
                // en VRd,max met theta = 45 graden en z = a
                // OPMERKING: f_ywd = 0.8 * fyd (zie 6.2.2 (6) en 6.3.2 (6)) zodat we nu_1 kunnen gebruiken!
                dwarskracht = J3ConsoleDwarskrachtCalculator.Bereken(i, nutHoogteTpvAc, hoogteTpvAc, asProv, a, fcd, nu1);

                // 6.3.2: torsie (TEd = FEd * e), TRd,max en combinatietoetsen (6.31)/(6.29)
                // OPMERKING hier de normale 'nu' gebruiken.
                torsie = J3ConsoleTorsieCalculator.Bereken(i, beton, fcd, nu, hoogteTpvAc);
                torsieCombinatie = J3ConsoleTorsieCalculator.Combineer(torsie, dwarskracht);
            }


            // J.3(.)
            double asLnk = 0.25 * asMain;
            if (linkType == J3ConsoleLinkType.Verticaal)
            {
                asLnk = 0.5 * i.FEd * 1000 / fyd;
            }
                

           // Aanvullende wapening
           double fwd = ((2.0 * z / a - 1.0) / (3.0 + i.FEd / f1x)) * f1x;
           double asw = Math.Max(fwd * 1000.0 / fyd, asLnk);

            // Node 1 verification
            double sigmaNode1Edx = f1x * 1000.0 / (i.Bc * y1);
            double sigmaNode1Edy = f1y * 1000.0 / (i.Bc * x1);
            double sigmaNode1Ed = Math.Max(sigmaNode1Edx, sigmaNode1Edy);

            // Node 2 verification below load plate
            double sigmaNode2Ed = i.FEd * 1000.0 / (i.LoadPlateLength * i.LoadPlateWidth);

            // M_rand voor opgave kolomberekening
            double mRand = i.FEd * i.Ac / 1000.0 + i.HEd * ah / 1000.0;

           


            var result = new J3ConsoleResult
            {
                // Input overnemen
                
                // geometrie console
                Hc = i.Hc, 
                Bc = i.Bc,
                Lc = i.Lc,
                KolomDikte = i.KolomDikte,
                KolomBreedte = i.KolomBreedte,

                // geometrie plaat

                // krachten
                FEd = i.FEd,
                HEd = i.HEd,

                //FactorHorizontaal = i.FactorHEd,
                DikteOplegmateriaal = i.DikteOplegmateriaal,
                
                Ac = i.Ac,
                Ah = ah,

                Dekking = i.Dekking,
                LoadPlateLength = i.LoadPlateLength,
                LoadPlateWidth = i.LoadPlateWidth,

                Fck = (int)i.Fck,
                Fcd = fcd,
                Fyd = fyd,
                Fywd = fywd,
                Nu = nu,


                Sigma1RdMax = sigma1RdMax,
                Sigma2RdMax = sigma2RdMax,
                Sigma3RdMax = sigma3RdMax,

                SigmaNode1Ed = sigmaNode1Ed,
                SigmaNode2Ed = sigmaNode2Ed,

                X1 = x1,
                W1 = w1,

                A = a,

                D = d,
                Z = z,
                ZBer = zBer,
                Z0 = z0,
                Y1 = y1,

                DiameterBgl = i.BeugelDiameter,
                
                DiameterMain = i.HoofdstaafDiameter,
                BuigdoorMain = i.HoofdstaafBuigdoornDiameterFactor * i.HoofdstaafDiameter,
                AantalMain = (int)i.HoofdstaafAantal,

                DiameterMain2 = i.HoofdstaafDiameter2,
                AantalMain2 = (int)i.HoofdstaafAantal2,

                DiameterMainAnchorage = i.HoofdstaafDiameter, // gelijk aan hoofdstaafdiameter
                DiameterBeugelAnchorage = i.BeugelDiameter, // gelijk aan diameter

                TanTheta = tanTheta,
                ThetaDeg = thetaDeg,
                LinkType = linkType,

                Fc = f1c,
                F1x = f1x,
                F1y = f1y,

                Ft = ft,
                Fwd = fwd,

                AsMain = asMain,
                AsMin = asMin,
                AsOplegging = asOplegging,
                MinimumWapening = minWap,
                Scheurwijdte = scheurwijdte,
                Dwarskracht = dwarskracht,
                Torsie = torsie,
                TorsieDwarskrachtCombinatie = torsieCombinatie,
                MEd = mEd,
                Asw = asw,
                Mrand = mRand,


                HoogteTpvAc = hoogteTpvAc,
                NutHoogteTpvAc = nutHoogteTpvAc


            };

            // Gebruikerskeuze (voorheen: BuigdoorMain < minimale buigdoorndiameter)
            result.UseAnchorageBar = i.UseAnchorageBar;

            // Wapeninggroepen geometrisch afmaken (shapes + verdeellijn) nu alle
            // benodigde resultaten (D, AantalBeugels, UseAnchorageBar) bekend zijn.
            J3ConsoleWapeningBuilder.VulGroepen(i, result);

            result.StrutAndTieNodes.Add(new StrutAndTie.StrutAndTieNode
            {
                Name = "Knoop 1",
                Id = "node-1",
                Type = StrutAndTie.StrutAndTieNodeType.CCC,
                Geometry = new StrutAndTie.StrutAndTieNodeGeometry()
                {
                    Center = new StrutAndTie.Point2D(-x1 / 2.0, y1)
                }
            });
            result.Nodes.Add(new J3ConsoleNodeResult
            {
                Naam = "Knoop 1",
                Type = J3NodeType.CCC,
                X = - x1 / 2.0,
                Y = d,
                SigmaEd = sigmaNode1Ed,
                SigmaRdMax = sigma1RdMax
            });
            result.Nodes.Add(new J3ConsoleNodeResult
            {
                Naam = "Knoop 2",
                Type = J3NodeType.CCT,
                X = a,
                Y = d - z,
                SigmaEd = sigmaNode2Ed,
                SigmaRdMax = sigma2RdMax
            });

            VulRegels(i, result);

            // Eenvoudig strut-and-tie schema (knopen, diagonalen, krachten) als SVG.
            result.SchemaSvg = J3ConsoleSvg.CreateSchemaSvg(result, i);

            // Uitgebreid rekenvoorbeeld (markdown + LaTeX) voor de detail-popup / rapport.
            result.RekenvoorbeeldMarkdown = J3ConsoleRekenvoorbeeld.Genereer(i, result);

            return result;
        }

        private static J3ConsoleLinkType BepaalLinkType(J3ConsoleInput i)
        {
            // todo: input de optie om horizontaal + vertikaal beugels toe te passen.


            if (i.Ac <= 0.5 * i.Hc) 
                return J3ConsoleLinkType.HorizontaalOfSchuin;

            if (i.Ac > 0.5 * i.Hc && i.FEd > i.VRdc)
                return J3ConsoleLinkType.Verticaal; // 'slanke consoles' krijgen vertikale beugels!

            

            return J3ConsoleLinkType.Geen;
        }


        private static void VulRegels(J3ConsoleInput i, J3ConsoleResult r)
        {
            r.ResultRows.Add(new()
            {
                Toelichting = "Invoer",
                Artikel = "J.3"

            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Lengte console",
                SymboolHtml = "<i>L</i><sub>c</sub>",
                SymboolTex = @"L_c",
                Waarde = i.Lc.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Hoogte console",
                SymboolHtml = "<i>H</i><sub>c</sub>",
                SymboolTex = @"H_c",
                Waarde = r.Hc.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Breedte console",
                SymboolHtml = "<i>B</i><sub>c</sub>",
                SymboolTex = @"B_c",
                Waarde = r.Bc.ToString("0"),
                Eenheid = "mm"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Ontwerpbelasting",
                SymboolHtml = "<i>F</i><sub>Ed</sub>",
                SymboolTex = @"F_{Ed}",
                Waarde = r.FEd.ToString("0"),
                Eenheid = "kN"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Ontwerpbelasting",
                SymboolTex = @"H_{Ed}",
                Waarde = r.HEd.ToString("0"),
                Eenheid = "kN"
            });

           


            r.ResultRows.Add(new()
            {
                Toelichting = "afstand belasting tot kolomrand",
                SymboolTex = @"a_c",
                Waarde = i.Ac.ToString("0"),
                Eenheid = "mm"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Staafdiameter",
                SymboolTex = @"\phi_{bgl}",
                Waarde = i.BeugelDiameter.ToString("0"),
                Eenheid = "mm"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Staafdiameter",
                SymboolTex = @"\phi_{hoofd}",
                Waarde = i.HoofdstaafDiameter.ToString("0"),
                Eenheid = "mm"
            });











            r.ResultRows.Add(new()
            {
                Toelichting = "Berekening",
                Artikel = "J.3"

            });

          







            if (r.HorizontaleBeugelsNodig)
            {
                r.ResultRows.Add(new()
                {
                    Toelichting = "In aanvulling op de hoofdtrekwapening gesloten horizontale of schuine beugels aanbrengen",
                    Waarde = r.HorizontaleBeugelsNodig ? "ja" : "nee",
                    SymboolTex = @"a_c \leq 0.5 h_c",
                    Artikel = "(2)"
                });



            }

            if (r.VerticaleBeugelsNodig)
            {
                r.ResultRows.Add(new()
                {
                    Toelichting = "In aanvulling op de hoofdtrekwapening gesloten verticale beugels aanbrengen",
                    Waarde = r.VerticaleBeugelsNodig ? "ja" : "nee",
                    SymboolTex = @"a_c > 0.5 h_c",
                    Artikel = "(3)"
                });
            }


            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal factor",
                SymboolHtml = "<i>ν</i>'",
                SymboolTex = @"\nu'",
                Waarde = r.Nu.ToString("0.00"),
                FormuleTex = @"ν' = 1 - f_{ck}/250",
                Artikel = "(6.57N)"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "CCC-knoop",
                SymboolHtml = "σ<sub>1Rd,max</sub>",
                SymboolTex = @"\sigma_{1Rd,max}",
                Waarde = r.Sigma1RdMax.ToString("0.00"),
                Eenheid = "N/mm²",
                FormuleTex = @"\sigma_{1Rd,max}=1.0 ν'\cdot f_{cd}", Artikel = "(6.60)"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "CCT-knoop",
                SymboolHtml = "σ<sub>2Rd,max</sub>",
                SymboolTex = @"\sigma_{2Rd,max}",
                Waarde = r.Sigma2RdMax.ToString("0.00"),
                Eenheid = "N/mm²",
                FormuleTex = @"\sigma_{2Rd,max}=0.85 ν'\cdot f_{cd}",
                Artikel = "(6.61)"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Knoop 1 (x)",
                SymboolTex = @"x_1",
                Waarde = r.X1.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @"x_1=\frac{F_{Ed}}{\sigma_{1Rd,max}b}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Knoop 1 (y)",
                SymboolTex = @"y_1",
                Waarde = r.Y1.ToString("0.0"),
                Eenheid = "mm",
            });




            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de nuttige hoogte",
                SymboolTex = @"d",
                Waarde = r.D.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = r.VerticaleBeugelsNodig ? @"d=h-c-\phi_{bgl}-\phi_{hoofd}/2" : @"d=h-c-\phi_{hoofd}/2"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Randafstand",
                SymboolTex = @"d_1",
                Waarde = r.D1.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @"d_1 = h-d"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de inwendige hefboomsarm",
                SymboolHtml = "<i>z</i>",
                SymboolTex = @"z",
                Waarde = r.Z.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @$"z={i.FactorZ:0} \cdot d"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal vervolgens de hefboomsarm",
                SymboolHtml = "<i>a</i>",
                SymboolTex = @"a",
                Waarde = r.A.ToString("0.0"),
                Eenheid = "mm",
            });



            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de hoek drukdiagonaal",
                SymboolHtml = "<i>θ</i>",
                SymboolTex = @"\theta",
                Waarde = r.ThetaDeg.ToString("0.#"),
                Eenheid = "º",
                FormuleTex = @"\theta = \arctan\left(\frac{a}{z}\right)"
            });

           

            r.ResultRows.Add(new()
            {
                Toelichting = "Controleer de drukdiagonaal",
                SymboolTex = @"tan\theta",
                Waarde = r.TanTheta.ToString("0.0"),
                IsOk = r.IsThetaOk,
                FormuleTex = @"1.0 \leq \tan\theta \leq 2.5"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de z0",
                SymboolHtml = "<i>z</i><sub>0</sub>",
                SymboolTex = @"z_0",
                Waarde = r.Z0.ToString("0.#"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Controleer of voldoet aan eis",
                SymboolTex = @"a_c < z_0 ?",
                Waarde = r.IsZ0Ok ? "✅" : "❌",
                IsOk = r.IsZ0Ok,
                FormuleTex = @"a_c < z_0"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Trekkracht",
                SymboolHtml = "<i>F</i><sub>t</sub>",
                SymboolTex = @"F_{t}",
                Waarde = r.Ft.ToString("0.00"),
                Eenheid = "kN",
                FormuleTex = @"F_{t} = F_{Ed} \cdot \frac{a}{z} + H_{Ed}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Drukkracht",
                SymboolTex = @"F_{c}",
                Waarde = r.Fc.ToString("0.00"),
                Eenheid = "kN",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Drukkracht verticaal",
                SymboolTex = @"F_{c,y}",
                Waarde = r.F1y.ToString("0.00"),
                Eenheid = "kN",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Drukkracht horizontaal",
                SymboolTex = @"F_{c,x}",
                Waarde = r.F1x.ToString("0.00"),
                Eenheid = "kN",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Drukkracht verticaal",
                SymboolTex = @"F_{c,y}",
                Waarde = r.F1y.ToString("0.00"),
                Eenheid = "kN",
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Beugelkracht",
                SymboolHtml = "<i>F</i><sub>wd</sub>",
                SymboolTex = @"F_{wd}",
                Waarde = r.Fwd.ToString("0.00"),
                Eenheid = "kN",
                FormuleTex = @"F_{wd}=\frac{2z/a-1}{3+F_{Ed}/F_c}F_c"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = r.HorizontaleBeugelsNodig ? 
                "Benodigde beugelwapening (horizontaal)" :
                "Benodigde beugelwapening (vertikaal)",
                SymboolHtml = "<i>A</i><sub>sl</sub>",
                SymboolTex = @"\Sigma A_{s,lnk}",
                Waarde = r.Asw.ToString("0"),
                Eenheid = "mm²",
                

            });

            r.ResultRows.Add(new()
            {
                Toelichting = $"Toegepaste wapening",
                SymboolTex = @"A_{s,lnk,prov}",
                Waarde = $"{r.AantalBeugels}bgØ{i.BeugelDiameter}(2-snedig)",
                FormuleTex = r.HorizontaleBeugelsNodig ?
                @"\Sigma A_{s,lnk} = \max(\frac{F_{wd}}{f_{yd}},0.25 \cdot A_{s,main})" :
                @"\Sigma A_{s,lnk} = \max(\frac{F_{wd}}{f_{yd}},0.50 \cdot A_{s,main})"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Benodigde hoofdwapening",
                SymboolHtml = "<i>A</i><sub>s,main</sub>",
                SymboolTex = @"A_{s,main}",
                Waarde = r.AsMain.ToString("0"),
                Eenheid = "mm²",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = $"Toegepaste wapening",
                SymboolTex = @"A_{s,main,prov}",
                Waarde = $"{r.AantalMain}Ø{i.HoofdstaafDiameter}",
                FormuleTex = @"A_{s,main}=\frac{F_{t}}{f_{yd}}"

            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Controle knoop 1",
                SymboolHtml = "σ<sub>Ed,1</sub>",
                SymboolTex = @"\sigma_{Ed,1}",
                Waarde = $"{r.SigmaNode1Ed:0.00} ≤ {r.Sigma1RdMax:0.00}",
                Eenheid = "N/mm²",
                IsOk = r.Node1Ok,
                FormuleTex = @"\sigma_{Ed,1}=\frac{F_c}{b\cdot y_1}"
            });




            r.ResultRows.Add(new()
            {
                Toelichting = "Controle knoop 2 onder oplegplaat",
                SymboolHtml = "σ<sub>Ed,2</sub>",
                SymboolTex = @"\sigma_{Ed,2}",
                Waarde = $"{r.SigmaNode2Ed:0.00} ≤ {r.Sigma2RdMax:0.00}",
                Eenheid = "N/mm²",
                IsOk = r.Node2Ok,
                FormuleTex = @"\sigma_{Ed,2}=\frac{F_{Ed}}{l_{plate}b_{plate}}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Staafspanning (main)",
                SymboolTex = @"f_{y,main}",
                Waarde = $"{r.MainFy:0}",
                Eenheid = "N/mm²",
                IsOk = r.MainFy < r.Fyd
            });

            if (r.MainFy > r.Fyd)
            {
                r.ResultRows.Add(new()
                {
                    Toelichting = "Overschrijding",
                    SymboolTex = @"f_{y,main}>f_{yd}",
                    IsOk = r.MainFy < r.Fyd
                });
            }

            r.ResultRows.Add(new()
            {
                Toelichting = "Controle buigdoorn",
                SymboolTex = @"\phi_{m,min}",
                Waarde = $"{(r.BuigdoornMainReq):0.00}",
                Eenheid = "mm",
                Artikel = "8.3"
            });

          

            r.ResultRows.Add(new()
            {
                Toelichting = "Buigdoorn", SymboolTex = @"\phi_{m,prov}",
                Waarde = $"{r.BuigdoorMain}",
                Eenheid = "mm",
                IsOk = r.BuigdoorMain >= r.BuigdoornMainReq(r.RechtDeelMain)
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Verankeringslengte",
                SymboolTex = @"l_{b,req}",
                Waarde = $"{r.VerankeringsLengteReq:0}",
                Eenheid = "mm",
                Artikel = "8.4"
            });




        }


        /// <summary>
        /// Berekent de effectieve hefboomsarm z uit:
        ///
        /// Fh = FEd * a / z
        /// y1 = Fh / (b * sigma)
        /// z + y1 / 2 = d
        ///
        /// Eenheden:
        /// - fEd: N
        /// - a, d, b: mm
        /// - sigma: N/mm²
        /// - resultaat: mm
        /// </summary>
        public static double BerekenHoogteZ(
            double fEd,
            double a,
            double d,
            double b,
            double sigma)
        {
            if (fEd < 0)
                throw new ArgumentOutOfRangeException(nameof(fEd));

            if (a < 0)
                throw new ArgumentOutOfRangeException(nameof(a));

            if (d <= 0)
                throw new ArgumentOutOfRangeException(nameof(d));

            if (b <= 0)
                throw new ArgumentOutOfRangeException(nameof(b));

            if (sigma <= 0)
                throw new ArgumentOutOfRangeException(nameof(sigma));

            if (fEd == 0 || a == 0)
                return d;

            var discriminant =
                d * d -
                2.0 * fEd * a / (b * sigma);

            if (discriminant < 0)
            {
                return d;
                throw new InvalidOperationException(
                    "Er bestaat geen reële oplossing voor z. " +
                    "De belasting is te groot voor de beschikbare geometrie " +
                    "en toegestane spanning.");
            }

            // Grootste en fysisch bruikbare oplossing.
            return (d + Math.Sqrt(discriminant)) / 2.0;
        }

      

    }
}
