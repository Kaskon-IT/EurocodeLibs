using Eurocode2.BetonConstructies;

namespace Eurocode.BetonConstructies
{
    public static class J3ConsoleCalculator
    {
        public static J3ConsoleResult Bereken(J3ConsoleInput i)
        {
            bool gdl = i.RekenMethode == J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie;
            var meldingen = new List<string>();

            double fcd = i.AlphaCc * i.Fck / i.GammaC;
            double fyd = i.Fyk / i.GammaS;
            double nu = 0.6 * (1.0 - i.Fck / 250.0); // 6.2.2 (6)
            double nu1 = 0.6; // drukdiagonaal VRd,max (6.9) bij beugelspanning ≤ 0.8 fyk, 6.2.3 (3) opm. 2
            double sigma1RdMax = 1.00 * nu * fcd; // CCC
            double sigma2RdMax = 0.85 * nu * fcd; // CCT
            double sigma3RdMax = 0.75 * nu * fcd; // CTT

            // ---- 1. Nuttige hoogte: d' opgegeven, of d' = c + Ø_bgl + Ø_main/2
            bool dOpgegeven = i.D1Opgave > 0;
            double d = dOpgegeven
                ? i.Hc - i.D1Opgave
                : i.Hc - i.Dekking - i.BeugelDiameter - 0.5 * i.HoofdstaafDiameter;

            // ---- 2. Vakwerk en hefboomsarm
            // STM (J.3): knoop 1 uit de toelaatbare knoopspanning, z uit het knoopevenwicht
            double x1 = Math.Max(i.FEd * 1000.0 / (sigma1RdMax * i.Bc), i.Dekking);
            double deltaA = i.FactorHEd * (i.Hc - d);
            double a = i.Ac + x1 / 2.0 + deltaA;
            bool zBerZonderOplossing = !gdl && HeeftGeenOplossingVoorZ(i.FEd * 1000, a, d, i.Bc, sigma1RdMax);
            double zBer = BerekenHoogteZ(i.FEd * 1000, a, d, i.Bc, sigma1RdMax);
            double z = zBer;

            if (gdl)
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

            if (zBerZonderOplossing)
                meldingen.Add("Geen reële oplossing voor z uit het evenwicht van knoop 1; z = d aangehouden. Controleer knoop 1.");

            // ---- 3. tanθ = z/a, J.3: 1,0 ≤ tanθ ≤ 2,5. Is z te groot, dan z terugzetten tot tanθ = 2,5.
            double zOnbegrensd = z;
            bool zBegrensd = z > 2.5 * a;
            if (zBegrensd)
            {
                z = 2.5 * a;
                meldingen.Add($"tanθ = {zOnbegrensd / a:0.00} > 2,5: z begrensd van {zOnbegrensd:0} mm tot 2,5·a = {z:0} mm.");
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


            // Hoogte verticaal knoopvlak knoop 1: F1x bij σ1Rd,max. Zonder begrenzing van z is dit
            // gelijk aan 2(d - z) (zBer volgt uit z + y1/2 = d); bij een op tanθ = 2,5 begrensde z
            // zou 2(d - z) een veel te hoge knoop geven. (GDL overschrijft y1 hieronder.)
            double y1 = f1x * 1000.0 / (i.Bc * sigma1RdMax);

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

            if (gdl)
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
            }

            // ---- BGT, dwarskracht en wringing: voor beide rekenmethoden
            {
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
                // en VRd,max met theta = 45 graden en z = a. VRd,c bepaalt ook het beugeltype (J.3 (3)).
                dwarskracht = J3ConsoleDwarskrachtCalculator.Bereken(i, nutHoogteTpvAc, hoogteTpvAc, asProv, a, fcd, nu1);

                // 6.3.2: torsie (TEd = FEd * e), TRd,max en combinatietoetsen (6.31)/(6.29)
                // OPMERKING hier de normale 'nu' gebruiken.
                torsie = J3ConsoleTorsieCalculator.Bereken(i, beton, fcd, nu, hoogteTpvAc);
                torsieCombinatie = J3ConsoleTorsieCalculator.Combineer(torsie, dwarskracht);
            }


            // ---- Beugeltype J.3 (2)/(3), met de berekende VRd,c
            J3ConsoleLinkType linkType = BepaalLinkType(i, dwarskracht.VRdc);

            // Dwarskrachtcontrole: verplicht bij GDL en bij verticale beugels volgens J.3 (3)
            bool controleDwarskracht = i.ControleDwarskracht || i.DwarskrachtControleVerplicht
                                       || linkType == J3ConsoleLinkType.Verticaal;

            // ---- As,lnk volgens J.3: k1 = 0,25 (horizontaal), k2 = 0,5 (verticaal)
            double asLnkMin = linkType switch
            {
                J3ConsoleLinkType.Verticaal => 0.5 * i.FEd * 1000 / fyd,
                J3ConsoleLinkType.HorizontaalOfSchuin => 0.25 * asMain,
                _ => 0.0
            };

            // Aanvullende wapening uit de spreiding van de drukdiagonaal
            double fwd = ((2.0 * z / a - 1.0) / (3.0 + i.FEd / f1x)) * f1x;
            double aswJ3 = Math.Max(fwd * 1000.0 / fyd, asLnkMin);
            double asw = aswJ3;

            // ---- Dwarskracht: beugels mogen niet minder zijn dan nodig voor dwarskracht (6.19)
            bool dwarskrachtWapeningNodig = controleDwarskracht && dwarskracht.WapeningNodig;
            bool verticaleBeugelsVoorDwarskracht = dwarskrachtWapeningNodig && linkType != J3ConsoleLinkType.Verticaal;
            bool dwarskrachtMaatgevend = false;
            double aswVerticaalDwarskracht = 0;

            if (dwarskrachtWapeningNodig)
            {
                if (linkType == J3ConsoleLinkType.Verticaal)
                {
                    dwarskrachtMaatgevend = dwarskracht.AswV > aswJ3;
                    asw = Math.Max(aswJ3, dwarskracht.AswV);
                    if (dwarskrachtMaatgevend)
                        meldingen.Add($"Dwarskracht maatgevend voor de verticale beugels: A_sw = {dwarskracht.AswV:0} mm² > ΣA_s,lnk (J.3) = {aswJ3:0} mm².");
                }
                else
                {
                    aswVerticaalDwarskracht = dwarskracht.AswV;
                    meldingen.Add($"V_Ed,red = {dwarskracht.VEdRed:0} kN > V_Rd,c = {dwarskracht.VRdc:0} kN: verticale beugels A_sw ≥ {dwarskracht.AswV:0} mm² in het middelste ¾ deel van a_v.");
                }
            }

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
                RekenMethode = i.RekenMethode,
                Rapportage = i.Rapportage,
                ControleDwarskracht = controleDwarskracht,
                DOpgegeven = dOpgegeven,
                ZOnbegrensd = zOnbegrensd,
                ZBegrensd = zBegrensd,
                ZBerZonderOplossing = zBerZonderOplossing,
                AsLnkMin = asLnkMin,
                AswJ3 = aswJ3,
                DwarskrachtMaatgevend = dwarskrachtMaatgevend,
                VerticaleBeugelsVoorDwarskracht = verticaleBeugelsVoorDwarskracht,
                AswVerticaalDwarskracht = aswVerticaalDwarskracht,
                Meldingen = meldingen,

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
                Av = dwarskracht.Av,
                Ah = ah,

                Dekking = i.Dekking,
                LoadPlateLength = i.LoadPlateLength,
                LoadPlateWidth = i.LoadPlateWidth,

                Fck = (int)i.Fck,
                Fcd = fcd,
                Fyd = fyd,
                Fywd = dwarskracht.Fywd,
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
            var builder = new J3ConsoleWapeningBuilder(i, result);
            var model = builder.Build();

            J3ConsoleWapeningBuilder.VulGroepen(i, result); // <-- vul wapening
            J3ConsoleStaafgroepToetsen.Toets(i, result);   // <-- toets
            J3ConsoleWapeningBuilder.VulGroepen(i, result); // <-- itteratie 1: nu zijn details bekend
            //J3ConsoleStaafgroepToetsen.Toets(i, result);   // <-- toets nogmaals 


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

            // Rapportage: uitgebreid (formules) en compact (tabel); RapportMarkdown volgt de keuze.
            result.RekenvoorbeeldMarkdown = J3ConsoleRekenvoorbeeld.MaakBuilder(i, result).Build();
            result.RapportCompactMarkdown = J3ConsoleRekenvoorbeeld.GenereerCompact(result);

            //var builder = new J3ConsoleWapeningBuilder(i, result);

            return result;
        }

        /// <summary>
        /// J.3 (2): a<sub>c</sub> ≤ 0,5h<sub>c</sub> → horizontale of schuine beugels;
        /// J.3 (3): a<sub>c</sub> &gt; 0,5h<sub>c</sub> en F<sub>Ed</sub> &gt; V<sub>Rd,c</sub> → verticale beugels.
        /// </summary>
        private static J3ConsoleLinkType BepaalLinkType(J3ConsoleInput i, double vRdc)
        {
            if (i.Ac <= 0.5 * i.Hc)
                return J3ConsoleLinkType.HorizontaalOfSchuin;

            if (i.FEd > vRdc)
                return J3ConsoleLinkType.Verticaal;

            return J3ConsoleLinkType.Geen;
        }

        private static bool HeeftGeenOplossingVoorZ(double fEd, double a, double d, double b, double sigma) =>
            fEd > 0 && a > 0 && d * d - 2.0 * fEd * a / (b * sigma) < 0;


        /// <summary>
        /// Vult de compacte (tabel)rapportage. Volgorde gelijk aan het uitgebreide
        /// rekenvoorbeeld (<see cref="J3ConsoleRekenvoorbeeld"/>), zodat beide hetzelfde vertellen.
        /// </summary>
        private static void VulRegels(J3ConsoleInput i, J3ConsoleResult r)
        {
            bool gdl = r.RekenMethode == J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie;

            void Kop(string titel, string? artikel = null) =>
                r.ResultRows.Add(new() { Toelichting = titel, Artikel = artikel });

            void Rij(string toelichting, string symboolTex, string waarde, string eenheid = "",
                string? formuleTex = null, bool? isOk = null, string? artikel = null) =>
                r.ResultRows.Add(new()
                {
                    Toelichting = toelichting,
                    SymboolTex = symboolTex,
                    Waarde = waarde,
                    Eenheid = eenheid,
                    FormuleTex = formuleTex,
                    IsOk = isOk,
                    Artikel = artikel
                });

            // ---- Invoer
            Kop("Invoer", "J.3");
            Rij("Rekenmethode", "", gdl ? "Gedrongen-liggertheorie 6.1 (10)" : "Strut-and-tie bijlage J.3");
            Rij("Lengte console", @"L_c", i.Lc.ToString("0"), "mm");
            Rij("Hoogte console", @"h_c", r.Hc.ToString("0"), "mm");
            Rij("Breedte console", @"b_c", r.Bc.ToString("0"), "mm");
            Rij("Verticale belasting", @"F_{Ed}", r.FEd.ToString("0"), "kN");
            Rij("Horizontale belasting", @"H_{Ed}", r.HEd.ToString("0"), "kN");
            Rij("Afstand belasting tot kolomrand", @"a_c", i.Ac.ToString("0"), "mm");
            Rij("Diameter beugels", @"\phi_{bgl}", i.BeugelDiameter.ToString("0"), "mm");
            Rij("Diameter hoofdwapening", @"\phi_{main}", i.HoofdstaafDiameter.ToString("0"), "mm");

            // ---- Nuttige hoogte
            Kop("Nuttige hoogte");
            Rij(r.DOpgegeven ? "Randafstand trekband (opgegeven)" : "Randafstand trekband (berekend)",
                @"d'", r.D1.ToString("0.0"), "mm",
                r.DOpgegeven ? null : @"d' = c + \phi_{bgl} + \phi_{main}/2");
            Rij("Nuttige hoogte", @"d", r.D.ToString("0.0"), "mm", @"d = h_c - d'");

            // ---- Vakwerk en hefboomsarm
            if (gdl)
            {
                Kop("Hefboomsarm gedrongen ligger", "6.1 (10)");
                Rij("Arm", @"a", r.A.ToString("0.0"), "mm", @"a = a_c + \min(a_b/2;\ L_c/4;\ h_c/4)");
                Rij("Hefboomsarm", @"z", r.ZOnbegrensd.ToString("0.0"), "mm", @"z = \min(0.4a + 0.4h_c;\ 1.6a)");
            }
            else
            {
                Kop("Vakwerk (strut-and-tie)", "J.3");
                Rij("Factor", @"\nu", r.Nu.ToString("0.000"), "", @"\nu = 0.6(1 - f_{ck}/250)");
                Rij("CCC-knoop", @"\sigma_{1Rd,max}", r.Sigma1RdMax.ToString("0.00"), "N/mm²", @"\sigma_{1Rd,max} = \nu f_{cd}", artikel: "(6.60)");
                Rij("CCT-knoop", @"\sigma_{2Rd,max}", r.Sigma2RdMax.ToString("0.00"), "N/mm²", @"\sigma_{2Rd,max} = 0.85\nu f_{cd}", artikel: "(6.61)");
                Rij("Breedte knoop 1", @"x_1", r.X1.ToString("0.0"), "mm", @"x_1 = \frac{F_{Ed}}{\sigma_{1Rd,max}\, b_c}");
                Rij("Arm (positie drukknoop)", @"a", r.A.ToString("0.0"), "mm", @"a = a_c + x_1/2 + \frac{H_{Ed}}{F_{Ed}} d'");
                Rij("Hefboomsarm", @"z", r.ZOnbegrensd.ToString("0.0"), "mm",
                    @"z = \frac{d + \sqrt{d^2 - 2F_{Ed}a/(b_c\sigma_{1Rd,max})}}{2}", isOk: r.ZBerZonderOplossing ? false : null);
            }

            if (r.ZBegrensd)
                Rij("Hefboomsarm begrensd op tanθ = 2,5", @"z", r.Z.ToString("0.0"), "mm", @"z = 2.5 \cdot a");

            // ---- Drukdiagonaal
            Kop("Drukdiagonaal", "J.3");
            Rij("Helling drukdiagonaal", @"\tan\theta", r.TanTheta.ToString("0.00"), "", @"\tan\theta = z/a;\ 1.0 \leq \tan\theta \leq 2.5", r.IsThetaOk);
            Rij("Hoek drukdiagonaal", @"\theta", r.ThetaDeg.ToString("0.#"), "°", @"\theta = \arctan(z/a)");
            Rij("Controle z₀", @"z_0", r.Z0.ToString("0.#"), "mm", @"a_c < z_0", r.IsZ0Ok);

            // ---- Trekband
            Kop("Trekband");
            if (gdl)
            {
                Rij("Moment kolomrand", @"M_{Ed}", (r.MEd / 1e6).ToString("0.0"), "kNm", @"M_{Ed} = a F_{Ed} + (z + d') H_{Ed}");
                Rij("Benodigde hoofdwapening", @"A_{s,main}", r.AsMain.ToString("0"), "mm²", @"A_{s,main} = \frac{M_{Ed}}{f_{yd} z}");
            }
            else
            {
                Rij("Trekbandkracht", @"F_t", r.Ft.ToString("0.0"), "kN", @"F_t = F_{Ed}\frac{a}{z} + H_{Ed}");
                Rij("Benodigde hoofdwapening", @"A_{s,main}", r.AsMain.ToString("0"), "mm²", @"A_{s,main} = \frac{F_t}{f_{yd}}");
            }
            Rij("Toegepaste hoofdwapening", @"A_{s,main,prov}", $"{r.AsMainProv:0} ({r.AantalMain}Ø{i.HoofdstaafDiameter})", "mm²", isOk: r.AsMainProvOk);
            Rij("Staalspanning", @"\sigma_s", r.MainFy.ToString("0"), "N/mm²", @"\sigma_s \leq f_{yd}", r.MainFy <= r.Fyd);

            // ---- Beugels J.3
            Kop("Beugels", "J.3");
            string beugelType = r.LinkType switch
            {
                J3ConsoleLinkType.HorizontaalOfSchuin => "horizontaal of schuin (J.3 (2))",
                J3ConsoleLinkType.Verticaal => "verticaal (J.3 (3))",
                _ => "geen (F_Ed ≤ V_Rd,c)"
            };
            Rij("Type aanvullende beugels", "", beugelType, "",
                r.LinkType == J3ConsoleLinkType.HorizontaalOfSchuin ? @"a_c \leq 0.5h_c" : @"a_c > 0.5h_c;\ F_{Ed} > V_{Rd,c}");
            Rij("Beugelkracht", @"F_{wd}", r.Fwd.ToString("0.0"), "kN", @"F_{wd} = \frac{2z/a - 1}{3 + F_{Ed}/F_{1x}} F_{1x}");
            if (r.LinkType != J3ConsoleLinkType.Geen)
                Rij("Minimum beugels", @"A_{s,lnk,min}", r.AsLnkMin.ToString("0"), "mm²",
                    r.LinkType == J3ConsoleLinkType.Verticaal ? @"0.5 F_{Ed}/f_{yd}" : @"0.25 A_{s,main}");
            Rij("Benodigd volgens J.3", @"\Sigma A_{s,lnk}", r.AswJ3.ToString("0"), "mm²", @"\max(F_{wd}/f_{yd};\ A_{s,lnk,min})");

            // ---- Dwarskracht
            var v = r.Dwarskracht;
            if (r.ControleDwarskracht && v is not null)
            {
                Kop("Dwarskracht", "6.2.2");
                Rij("Afstand last", @"a_v", v.Av.ToString("0"), "mm", v.AvFormula.StaticValue);
                Rij("Reductie last bij oplegging", @"\beta", v.Beta.ToString("0.00"), "", @"\beta = a_v/2d", artikel: "6.2.2 (6)");
                Rij("Gereduceerde dwarskracht", @"V_{Ed,red}", v.VEdRed.ToString("0"), "kN", @"V_{Ed,red} = \beta V_{Ed}");
                Rij("Weerstand zonder wapening", @"V_{Rd,c}", v.VRdc.ToString("0"), "kN", @"V_{Ed,red} \leq V_{Rd,c}", v.IsVoldoende, "(6.2a)");
                Rij("Drukdiagonaal", @"V_{Rd,max}", v.VRdMax.ToString("0"), "kN", @"V_{Ed} \leq V_{Rd,max}", v.VRdMaxOk, "(6.9)");
                if (v.WapeningNodig)
                {
                    Rij("Beugelspanning", @"f_{ywd}", v.Fywd.ToString("0"), "N/mm²", v.FywdFormula.StaticValue);
                    Rij("Beugels t.b.v. dwarskracht", @"A_{sw}", v.AswV.ToString("0"), "mm²", @"A_{sw} = \beta V_{Ed}/f_{ywd}", artikel: "(6.19)");
                }
            }

            // ---- Toegepaste beugels
            Kop("Beugelwapening");
            if (r.DwarskrachtMaatgevend)
                Rij("Maatgevend: dwarskracht", @"\Sigma A_{s,lnk}", r.Asw.ToString("0"), "mm²", @"\max(\Sigma A_{s,lnk,J.3};\ A_{sw})");
            else
                Rij("Benodigde beugelwapening", @"\Sigma A_{s,lnk}", r.Asw.ToString("0"), "mm²");
            Rij("Toegepaste beugels", @"\Sigma A_{s,lnk,prov}", $"{r.AswProv:0} ({r.AantalBeugels}bgØ{i.BeugelDiameter}, 2-snedig)", "mm²", isOk: r.AswProvOk);
            if (r.VerticaleBeugelsVoorDwarskracht)
                Rij("Extra verticale beugels (dwarskracht)", @"A_{sw}", r.AswVerticaalDwarskracht.ToString("0"), "mm²", @"\text{middelste } 3/4 \text{ van } a_v");

            // ---- Knopen
            Kop("Knopen", "6.5.4");
            Rij("Knoop 1 horizontaal", @"x_1", r.X1.ToString("0.0"), "mm");
            Rij("Knoop 1 verticaal", @"y_1", r.Y1.ToString("0.0"), "mm",
                gdl ? @"y_1 = x_1 F_{1x}/F_{Ed}" : @"y_1 = \frac{F_{1x}}{b_c\,\sigma_{1Rd,max}}");
            Rij("Knoop 1 (CCC)", @"\sigma_{Ed,1}", $"{r.SigmaNode1Ed:0.00} ≤ {r.Sigma1RdMax:0.00}", "N/mm²", @"\sigma_{Ed,1} \leq \sigma_{1Rd,max}", r.Node1Ok);
            Rij("Knoop 2 onder oplegplaat (CCT)", @"\sigma_{Ed,2}", $"{r.SigmaNode2Ed:0.00} ≤ {r.Sigma2RdMax:0.00}", "N/mm²", @"\sigma_{Ed,2} = \frac{F_{Ed}}{a_b b_b}", r.Node2Ok);

            // ---- Wringing
            if (r.Torsie is { TEd: > 0 } t)
            {
                Kop("Wringing", "6.3.2");
                Rij("Wringmoment", @"T_{Ed}", t.TEd.ToString("0.00"), "kNm", @"T_{Ed} \leq T_{Rd,c}", t.IsVoldoende);
            }

            // ---- BGT
            Kop("Bruikbaarheidsgrenstoestand", "7.3");
            if (r.MinimumWapening is not null)
                Rij("Minimumwapening", @"A_{s,min}", r.AsMin.ToString("0"), "mm²", @"A_{s,prov} \geq A_{s,min}", r.AsMinOk, "(7.1)");
            if (r.Scheurwijdte is { } sw)
                Rij("Scheurwijdte", @"w_k", sw.Wk.ToString("0.00"), "mm", $@"w_k \leq {sw.WMax:0.0}", sw.IsVoldoende, "(7.8)");

            // ---- Verankering
            Kop("Verankering", "8.3 / 8.4");
            Rij("Minimale buigdoorn", @"\phi_{m,min}", r.BuigdoornMainReq(r.RechtDeelMain).ToString("0"), "mm");
            Rij("Toegepaste buigdoorn", @"\phi_{m,prov}", r.BuigdoorMain.ToString("0"), "mm", isOk: r.BuigdoorMain >= r.BuigdoornMainReq(r.RechtDeelMain));
            Rij("Verankeringslengte", @"l_{b,req}", r.VerankeringsLengteReq.ToString("0"), "mm");
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
