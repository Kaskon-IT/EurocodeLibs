namespace Eurocode.BetonConstructies
{
    public static class J3ConsoleCalculator
    {
        public static J3ConsoleResult Bereken(J3ConsoleInput i)
        {
            double fcd = i.AlphaCc * i.Fck / i.GammaC;
            double fyd = i.Fyk / i.GammaS;
            double nu = 1.0 - i.Fck / 250.0;
            double nEd = i.NEd; // normaalkracht in kolom/wand
            double dsnOppKolom = i.KolomDikte * i.Bc;
            double sigmaBasis = 0; // er is geen spanning, // check dit met Martijn, waarom niet nEd * 1000 / dsnOppKolom;

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
                    d = i.Hc - i.Dekking - 0.5 * i.HoofdstaafDiameter; break;
                case J3ConsoleLinkType.Verticaal:
                    d = i.Hc - i.Dekking - i.BeugelDiameter - 0.5 * i.HoofdstaafDiameter; break;

            }

            // x1 uit druksterkte node 1 (met zelfbedachte ondergrens)
            double sigmaToelaatbaar = Math.Max(sigma1RdMax - sigmaBasis, 0);
            double x1Min = i.Dekking;
            double x1 = Math.Max(i.FEd * 1000.0 / (sigmaToelaatbaar * i.Bc),x1Min);
            double deltaA = i.FactorHEd * (i.Hc - d);

            // a = ac + x1/2 + deltaA
            double a = i.Ac + x1 / 2.0 + deltaA;
            // volgens jouw voorbeeld
            double z = factorZ * d;
            double z0 = z * (i.Ac + deltaA) / a;
            double tanTheta = z / a;
            double thetaDeg = Math.Atan(tanTheta) * 180.0 / Math.PI;

            double d1 = i.Hc - d;
            double ah = i.DikteOplegmateriaal + d1;



            // Trekbandkracht volgens rekenvoorbeeld: Ft = FvEd * a_eff / z,
            // waarbij a_eff (= a) de correctie voor de horizontale belasting al bevat (deltaA).
            // Oftewel: Ft = (FvEd*a + FhEd*d1) / z.
            // Oude logica (vervangen): double ft = i.FEd * a / z + i.HEd; // met aandeel horizontaal
            double ft = i.HEd + i.FEd * a / z;
            double f1x = i.FEd * a / z; // 
            double f1y = i.FEd;
            double f1c = Math.Sqrt(f1x * f1x + f1y * f1y);

            // Horizontaal knoopvlak volgens rekenvoorbeeld: y1 = Fc,x / (b * sigma_n).
            // Oude logica (vervangen): double y1 = (d - z) * 2; // volgens mij
            double y1 = f1x * 1000.0 / (i.Bc * sigmaToelaatbaar);



           
         

            // Main reinforcement
            double asMain = ft * 1000.0 / fyd;

            double aswMin = 0.25 * asMain;
            if (linkType == J3ConsoleLinkType.Verticaal)
            {
                aswMin = 0.5 * asMain;
            }

           // Aanvullende wapening
           double fwd = ((2.0 * z / a - 1.0) / (3.0 + i.FEd / f1x)) * f1x;
           double asw = Math.Max(fwd * 1000.0 / fyd, aswMin);

            


            // Node 1 verification
            double sigmaNode1Ed = f1x * 1000.0 / (i.Bc * y1);

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

                // geometrie plaat

                // krachten
                FEd = i.FEd,
                HEd = i.HEd,

                FactorHorizontaal = i.FactorHEd,
                DikteOplegmateriaal = i.DikteOplegmateriaal,
                
                Ac = i.Ac,
                Ah = ah,

                Dekking = i.Dekking,
                LoadPlateLength = i.LoadPlateLength,
                LoadPlateWidth = i.LoadPlateWidth,

                Fck = (int)i.Fck,
                Fcd = fcd,
                Fyd = fyd,
                Nu = nu,


                Sigma1RdMax = sigma1RdMax,
                Sigma2RdMax = sigma2RdMax,
                Sigma3RdMax = sigma3RdMax,

                SigmaNode1Ed = sigmaNode1Ed,
                SigmaNode2Ed = sigmaNode2Ed,

                X1 = x1,
                
                A = a,

                D = d,
                Z = z,
                Z0 = z0,
                Y1 = y1,

                DiameterBgl = i.BeugelDiameter,
                
                DiameterMain = i.HoofdstaafDiameter,
                BuigdoorMain = i.HoofdstaafBuigdoornDiameterFactor * i.HoofdstaafDiameter,
                AantalMain = (int)i.HoofdstaafAantal,

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
                Asw = asw,
                Mrand = mRand,
            };


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
            if (i.Ac < 0.5 * i.Hc) 
                return J3ConsoleLinkType.HorizontaalOfSchuin;

            if (i.Ac > 0.5 * i.Hc && i.FEd > i.VRdc)
                return J3ConsoleLinkType.Verticaal; // 'slanke consoles' krijgen vertikale beugels!

            if (i.Ac == 0.5 * i.Hc)
            {
                return J3ConsoleLinkType.HorizontaalOfSchuin; // aanvulling altijd beugels toepassen voor Fwd. 
            }

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
                Waarde = $"{(2 * r.BuigstraalMainReq):0.00}",
                Eenheid = "mm",
                Artikel = "8.3"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Controle buigradius",
                SymboolTex = @"r_{m,min}",
                Waarde = $"{r.BuigstraalMainReq:0.00}",
                Eenheid = "mm",
                Artikel = "8.3"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Buigdoorn", SymboolTex = @"\phi_{m,prov}",
                Waarde = $"{r.BuigdoorMain}",
                Eenheid = "mm",
                IsOk = r.BuigdoorMain >= 2 * r.BuigstraalMainReq
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

    }
}
