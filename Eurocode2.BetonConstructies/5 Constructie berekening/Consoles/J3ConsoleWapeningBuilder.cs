using Eurocode.BetonConstructies.StrutAndTie;
using Eurocode2.BetonConstructies;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Maakt de wapeninggroepen uit <see cref="J3ConsoleInput"/> geometrisch af
    /// (shapes + verdeellijn) op basis van het berekende resultaat.
    /// De geometrie is een 1-op-1 port van de bestaande Three.js-bouwers
    /// (<c>rebar()</c>, <c>beugel()</c>, <c>beugelVer()</c> en de haarspeld in
    /// <c>J3ConsoleThreeView.razor</c>), in modelcoördinaten (y omlaag).
    /// </summary>
    public class J3ConsoleWapeningBuilder(
        J3ConsoleInput input,
        J3ConsoleResult result)
    {
        private readonly J3ConsoleInput _input = input;
        private readonly J3ConsoleResult _result = result;

        public Model Build()
        {
            //var model = new Model();

            BouwBeton(_result.Model);
            BouwWapening(_result.Model);

            return _result.Model;
        }

        // TODO: Hernoem naar J3ConsoleBuilder en maak ALLE modelobjecten in de nieuwe Model class.
        // TODO: Daarna stop alleen nog dit Model in de THREE render classes waarmee alle onderdelen opgebouwd worden!
        // eerst nog ZONDER knopen en strut-and-tie in het Model maar later deze ook als modelobject toevoegen.



        private void BouwBeton(Model model)
        {
            // ...
            Beam test = new()
            {
                StartPoint = new Punt3D(0, 0, 0),
                EndPoint = new Punt3D(1000,1000,1000)
            };
            model.Add(test);
        }

        private void BouwWapening(Model model)
        {
            // ...
            VulWapening(model, _input, _result);


        }


        public static void VulWapening(Model model, J3ConsoleInput i, J3ConsoleResult r)
        {
            Console.WriteLine("[VulWapening] 🔥" + model.Objects.Count);
            model.Add(BouwVerticaleHaarspelden(i, r));
            Console.WriteLine("[VulWapening] 🍂" + model.Objects.Count);
            foreach (var rbg in model.GetObjects<WapeningGroep>())
            {
                Console.WriteLine($"rbg:{rbg.DisplayText}");
            }

        }

        /// <summary>
        /// Vult de vier groepen op het resultaat. Aanroepen nadat alle scalars
        /// van het resultaat (D, Z, AantalBeugels, ...) bepaald zijn.
        /// </summary>
        public static void VulGroepen(J3ConsoleInput i, J3ConsoleResult r)
        {
            r.WapVerticaleHaarspelden = BouwVerticaleHaarspelden(i, r);
            // uitgecomment als test, kijken of we via r.Model het THREE scene kunnen opbouwen.
            r.WapHorizontaleHaarspelden = BouwHorizontaleHaarspelden(i, r);
            r.WapBglsHor = BouwBeugelsHorizontaal(i, r);
            r.WapBglsVer = BouwBeugelsVerticaal(i, r);
        }

        /// <summary> Gebruikers-buigstralen van de groep, of anders de standaardwaarde. </summary>
        private static double[] Buigstralen(WapeningGroep groep, double standaard) =>
            groep.Buigstralen is { Length: > 0 } ? groep.Buigstralen : [standaard];

        /// <summary>
        /// Buitenrand (x) van de betonvorm op hoogte ym, loodrecht 'off' naar binnen;
        /// port van <c>betonXBuiten()</c> in <c>j3console-build.js</c>.
        /// </summary>
        private static double BetonXBuiten(double ym, double off, double L, double H, bool taper)
        {
            var xVert = L - off;
            if (!taper) return xVert;
            var hyp = Math.Sqrt(L * L + H / 2.0 * (H / 2.0));
            var xTaper = 2.0 / H * (L * H - off * hyp - L * ym);
            return Math.Min(xVert, xTaper);
        }

        /// <summary>
        /// Onderrand (ym) van de betonvorm op positie x, loodrecht 'off' naar binnen;
        /// port van <c>betonYOnder()</c> in <c>j3console-build.js</c>.
        /// </summary>
        private static double BetonYOnder(double x, double off, double L, double H)
        {
            var hyp = Math.Sqrt(L * L + H / 2.0 * (H / 2.0));
            var yVlak = H - off;
            var yTaper = H - H * x / (2.0 * L) - off * hyp / L;
            return Math.Min(yVlak, yTaper);
        }

        

        /// <summary>
        /// Hoofd-trekwapening (AsMain) als verticale haarspeld; geometrie identiek
        /// aan <c>rebar()</c> in <c>j3console-build.js</c>. Verdeeld over de
        /// beschikbare breedte (z-richting).
        /// </summary>
        public static WapeningGroep BouwVerticaleHaarspelden(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapVerticaleHaarspelden;
            groep.DisplayName = "testDisplayName";
            
            //var hspPlat = i.WapHorizontaleHaarspelden;
            var toonTaper = i.AfschuiningOnderzijde && !r.VerticaleBeugelsNodig;

            var c = i.Dekking;
            var hart = groep.Tussenruimte() + groep.Diameter;
            var ab = Math.Min(c + groep.Diameter / 2.0, hart / 2.0);
            var benutting = r.AsMain / r.AsMainProv;
            var fbt = benutting * WapeningHelper.GetDsnOpp(1, groep.Diameter) * r.Fyd;



            var xAnchorLeft = -r.X1 / 2.0;
            var xAnchorRight = r.Ac - r.LoadPlateLength / 2.0;


            var H = i.Hc;
            var L = i.Lc;
            var Bw = i.KolomDikte;
            var dBgl = i.WapBglsVer.Diameter;
            var phi = groep.Diameter;
            var bendR = i.HoofdstaafBuigdoornDiameterFactor > 0
                ? (i.HoofdstaafBuigdoornDiameterFactor * phi + phi) / 2.0
                : (r.BuigdoorMain + phi) / 2.0;

            var cc = c + dBgl + phi / 2.0;   // hart hoofdstaaf
            var yTop = cc;
            var xTip = L - cc;




            var left = -Bw + cc;

            var a = xAnchorLeft - left - bendR;
            var b = 90.0 / 360.0 * 2 * bendR * Math.PI;

            var afstandTotFbt = a;

            
            
            //var ctxPhi = r.WapKolomMain.GrootsteDiameter;

            var inputVerankering = new VerankeringslengteInput()
            {
                Ab = ab,
                AfstandTotFbt = afstandTotFbt,
                Benuttingsgraad = benutting,
                StaafType = VerankeringStaafType.Trekstaaf,
                StaafVorm = VerankeringStaafVorm.Gebogen,
                Diameter = groep.Diameter,
                GoedeAanhechting = false,
                Beton = new BetonContext(r.Fck),
                ToegepasteBuigdoornDiameter = groep.BuigdoornDiameter()
            };

            var resultVerankering = VerankeringslengteCalculator.Bereken(inputVerankering);
            resultVerankering.Id = "verankering1";
            resultVerankering.Naam = "Verankering trekband (sg1)";

            // wat is de benodigde lbd?
            if (groep.Buigstralen != null)
            {
                bendR = groep.Buigstralen.FirstOrDefault();
            }
            
            var lbd = resultVerankering.Verankeringslengte;


            var upReq = lbd - a - b + bendR;
            var up = Math.Max(5.0 * phi + bendR, upReq);

            bool ombuigen = upReq > 0;
            if (!ombuigen)
            {
                inputVerankering.StaafVorm = VerankeringStaafVorm.Recht;
                resultVerankering = VerankeringslengteCalculator.Bereken(inputVerankering);

                lbd = resultVerankering.Verankeringslengte;
                left = xAnchorLeft - lbd;
            }

            // geef de verankering terug aan het resultaat (builder → toets)
            var verankeringKolomzijde = WapeningHelper.GetVerankeringResult(
                ab,
                fbt,
                groep.Diameter,
                (int)i.Fck,
                benutting
                );

            verankeringKolomzijde.Id = $"verankering-{groep.Prefix}-kolom";
            verankeringKolomzijde.Naam = "Verankering kolomzijde";

            r.SetVerankering(groep, resultVerankering);
            


            
            //r.VerankeringenPerGroep[groep] = resultVerankering;

            var c1 = new Punt3D(left, yTop + up, 0);   // vrij uiteinde onderaan de opgaande tak
            var c2 = new Punt3D(left, yTop, 0);        // bovenhoek links (90°)
            var c3 = new Punt3D(xTip, yTop, 0);        // bovenhoek rechts (90°)




            List<Punt3D> punten;
            if (r.UseAnchorageBar)
            {
                punten = [c1, c2, c3]; // met verankeringsstaaf: alleen de eerste 3 knikpunten
            }
            else if (toonTaper)
            {
                var hyp = Math.Sqrt(L * L + H / 2.0 * (H / 2.0));
                var diag = 10 * phi + hyp;
                var taperX = -L / hyp;
                var taperY = H / 2.0 / hyp;
                var ymC4 = H - H * xTip / (2 * L) - cc * hyp / L;
                var c4 = new Punt3D(xTip, ymC4, 0);
                var c5 = new Punt3D(c4.X + taperX * diag, c4.Y + taperY * diag, 0);
                punten = [c1, c2, c3, c4, c5];
            }
            else
            {
                var yBot = yTop + (H - 2 * cc);
                var c4 = new Punt3D(xTip, yBot, 0);
                var c5 = new Punt3D(left, yBot, 0);
                punten = [c1, c2, c3, c4, c5];
            }

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = Buigstralen(groep, bendR) }];

            var offStartEnd = c + i.WapBglsHor.Diameter + i.WapBglsVer.Diameter + i.WapHorizontaleHaarspelden.Diameter + i.WapVerticaleHaarspelden.Diameter / 2.0 ;
            //var halfClear = Math.Max(i.Bc / 2.0 - c - dBgl - ctxPhi -hspPlat.Diameter - phi / 2.0, 0);
            var halfDepth = i.Bc / 2.0;

            groep.VerdeelStart = new Punt3D(0, 0, halfDepth);
            groep.VerdeelEind = new Punt3D(0, 0, -halfDepth);
            groep.Verdeling.OffsetStart = offStartEnd;
            groep.Verdeling.OffsetEind = offStartEnd;


            // verankering console zijde
            inputVerankering.StaafVorm = VerankeringStaafVorm.Gebogen;
            inputVerankering.AfstandTotFbt = 100;
            inputVerankering.Alpha5 = 0.7;

            var vResConsole = VerankeringslengteCalculator.Bereken(inputVerankering);
            vResConsole.Naam = "Verankering console zijde";
            vResConsole.Id = Guid.NewGuid().ToString();
            r.SetVerankering(groep, vResConsole);



            return groep;
        }

        /// <summary>
        /// Horizontale haarspeld bij de trekband (hoort bij de rechte
        /// verankeringsstaaf, zichtbaar bij <see cref="J3ConsoleInput.UseAnchorageBar"/>).
        /// V1: polyline van 6 punten — punt 1–3 op het voorvlak (z = B/2 − c − ØbglHor − ØbglVer),
        /// punt 4–6 op het achtervlak (z = −B/2 + c + ØbglHor + ØbglVer).
        /// Punt 1/6: x = linkerzijde kolom + c + ØbglHor, 10Ø onder de trekband.
        /// Punt 2/5: zelfde x, op de trekband. Punt 3/4: consoletip op de trekband.
        /// De definitieve shape wordt later uitgewerkt.
        /// </summary>
        public static WapeningGroep BouwHorizontaleHaarspelden(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapHorizontaleHaarspelden;
            var lbd = r.VerankeringMain.Verankeringslengte;

            

            var xLinksBenodigd = -r.X1 / 2.0 - lbd;
            Console.WriteLine($"hor. haarspeld xL,req = {xLinksBenodigd:0}");


            var c = i.Dekking;
            var dBglHor = i.WapBglsHor.Diameter;
            var dBglVer = i.WapBglsVer.Diameter;
            var phi = groep.Diameter;

            var L = i.Lc;
            var Bw = i.KolomDikte;
            var B = i.Bc;

            var zVoor = B / 2.0 - c - dBglHor - dBglVer - phi/2.0;
            var zAchter = -B / 2.0 + c + dBglHor + dBglVer + phi/2.0;

            var xLinksUiterste = -Bw + c + dBglHor + groep.Diameter / 2.0;
            var xLinks = Math.Max(xLinksUiterste, xLinksBenodigd);

            bool ombuigingLinks = xLinksBenodigd < xLinksUiterste;

            var xTip = L - c - groep.Diameter / 2.0;

            var yTrek = i.Hc - r.D;        // positie trekband (d1) [mm, y omlaag]
            var yHaak = yTrek + Math.Min(20 * phi, 200);  // 10Ø onder de trekband

            
            List<Punt3D> punten =
            [
                //new(xLinks, yHaak, zVoor),   // 1: haak, voorvlak
                new(xLinks, yTrek, zVoor),   // 2: trekband, voorvlak
                new(xTip, yTrek, zVoor),     // 3: consoletip, voorvlak
                new(xTip, yTrek, zAchter),   // 4: consoletip, achtervlak
                new(xLinks, yTrek, zAchter), // 5: trekband, achtervlak
                //new(xLinks, yHaak, zAchter), // 6: haak, achtervlak
            ];

            if (ombuigingLinks)
            {
                punten = [
                new(xLinks, yTrek, zAchter),   // 1: haak!
                new(xLinks, yTrek, zVoor),   // 2: trekband, voorvlak
                new(xTip, yTrek, zVoor),     // 3: consoletip, voorvlak
                new(xTip, yTrek, zAchter),   // 4: consoletip, achtervlak
                new(xLinks, yTrek, zAchter), // 5: trekband, achtervlak
                new(xLinks, yTrek, zVoor), // 6: haak!
                    ];

            }


            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = Buigstralen(groep, phi * 2.5) }];

            // Eén positie: verdeellijn is richting-only.
            groep.VerdeelStart = new Punt3D(0, 0, 0);
            groep.VerdeelEind = new Punt3D(0, 100, 0);
            groep.Verdeling.Type = VerdelingType.ExacteHartOpHart;
            groep.Verdeling.HartOpHartAfstanden = [20];
            //groep.Buigstralen = [55]; // fixed
            //groep.Verdeling = new WapeningVerdeling { Type = VerdelingType.Gelijkmatig };

            // test
            if (r.AantalMain2 == 2)
            {
                groep.Verdeling.HartOpHartAfstanden = [groep.Diameter];
                // dit zou moeten triggeren dat de diam_eq wijzigt omdat we nu een bundel hebben

            }


            // voordat we de groep teruggeven, de verankering in het resultaat zetten (builder → toets)
            VerankeringslengteInput vi1 = new VerankeringslengteInput()
            {
                AfstandTotFbt = 100,
                Benuttingsgraad = 1.0,
                Beton = new(30),
                Diameter = groep.Diameter,
                DiameterT = groep.Diameter,
                DekkingC = i.Dekking,

            };

            r.SetVerankering(groep, VerankeringslengteCalculator.Bereken(vi1));

            VerankeringslengteInput vi2 = new()
            {
                AfstandTotFbt = 100,
                Benuttingsgraad = 1.0,
                Beton = new((int)i.Fck),
                
            };

            r.SetVerankering(groep, VerankeringslengteCalculator.Bereken(vi2));


            return groep;
        }

        /// <summary>
        /// Horizontale beugels; geometrie identiek aan <c>beugel()</c> in
        /// <c>j3console-build.js</c> (7-punts curve in het x-z-vlak), verdeeld
        /// langs y van yFirst tot yLast. Aantal = <see cref="J3ConsoleResult.AantalBeugels"/>.
        /// V1: geen alternerende spiegeling (flip) — alle haken aan dezelfde zijde.
        /// </summary>
        public static WapeningGroep BouwBeugelsHorizontaal(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapBglsHor;
            var dBgl = groep.Diameter;
            var dMain = i.WapVerticaleHaarspelden.Diameter;
            var c = i.Dekking;
            var H = i.Hc;
            var L = i.Lc;
            var Bw = i.KolomDikte;
            var B = i.Bc;

            var y2 = i.Hc - r.D;
            var yFirst = y2 + 4 * dMain;
            var yLast = Math.Min(2.0 / 3.0 * r.D, H - c - dBgl / 2.0);

            var taper = i.AfschuiningOnderzijde && !r.VerticaleBeugelsNodig;
            var xIn = -Bw + c + dBgl / 2.0;
            var zPos = B / 2.0 - c - dBgl / 2.0;
            var zNeg = -zPos;
            var hook = 10 * dBgl;
            var dd = dBgl;

            // 7-punts curve op hoogte y; de buitenrand xOut volgt de betonvorm
            // (bij afschuining wordt de beugel dus smaller richting de tip).
            List<Punt3D> Punten(double y, double x1, double x2)
            {
                //var xOut = BetonXBuiten(y, c + dBgl / 2.0, L, H, taper);
                return
                [
                    new(x1, 0, zPos - hook),   // 1: beginhaak
                    new(x1, 0, zPos),          // 2: hoek A
                    new(x2, dd / 2.0, zPos),  // 3: hoek B
                    new(x2, dd / 2.0, zNeg),  // 4: hoek C
                    new(x1, dd, zNeg),         // 5: hoek D
                    new(x1, dd, zPos),         // 6: terug op hoek A
                    new(x1 + hook, dd, zPos),  // 7: eindhaak
                ];
            }

            var buigstralen = Buigstralen(groep, 2.5 * dBgl);
            StaafShape Shape(double y, double x1, double x2) => new() { Punten = Punten(y, x1, x2), Buigstralen = buigstralen };
            var off = c + dBgl / 2.0;
            if (taper)
            {
                // Knikpunt: hoogte waarop de schuine rand de verticale rand snijdt
                // (BetonXBuiten schakelt daar van xVert naar xTaper). Tot dat punt
                // blijven de beugels gelijk; daarna verloopt de vorm mee met de rand.
                
                var hyp = Math.Sqrt(L * L + H / 2.0 * (H / 2.0));
                var yKnik = H / 2.0 + off * (H / 2.0 - hyp) / L;
                yKnik = Math.Clamp(yKnik, yFirst, yLast);

                var bereik = yLast - yFirst;
                var tKnik = bereik > 0 ? (yKnik - yFirst) / bereik : 0;

                // 3 shapes: [0] en [1] gelijk (constant traject), [2] de eindvorm.
                groep.Shapes = [Shape(0,xIn, L-off), Shape(H/2, xIn, L-off), Shape(H, xIn, 0-off)];
                // totdat we de offset op de shape hebben moet
                // voorlopig de hoogte snijpunt bepaald worden.
                var hoek = Math.Atan2(H / 2.0, L);
                var deltaSp = Math.Cos(Math.Atan2(H / 2.0, L)) * off;
                var percSp = deltaSp / H;
               

                 /// <summary>
                 /// Berekent de verschuiving van het snijpunt (dx, dy) na een loodrechte offset.
                 /// </summary>
                (double dx, double dy) BerekenSnijpuntVerschuivingBAK(double B, double H, double a)
        {
            // 1. Bereken de lengte van de richtingsvector (schuine zijde)
            double lengte = Math.Sqrt(B * B + H * H);

            // 2. De verticale lijn schuift puur horizontaal op met afstand a
            double dx = a;

            // 3. Bereken dy op basis van de geometrische wetten van de offset
            double dy = a * (B + lengte) / H;



            return (dx, dy);
        }

                


            Point2D BerekenLoodrechtSnijpunt(Point2D a, Point2D b, Point2D c, Point2D d, double offset)
                {
                    // 1. Richtingsvectoren van beide lijnen bepalen
                    double v1x = b.X - a.X;
                    double v1y = b.Y - a.Y;
                    double v2x = d.X - c.X;
                    double v2y = d.Y - c.Y;

                    // 2. Lengte berekenen voor normalisatie
                    double len1 = Math.Sqrt(v1x * v1x + v1y * v1y);
                    double len2 = Math.Sqrt(v2x * v2x + v2y * v2y);

                    if (len1 < 1e-9 || len2 < 1e-9)
                        throw new ArgumentException("Punten liggen te dicht bij elkaar om een lijn te vormen.");

                    // 3. Loodrechte eenheidsvector (normaalvector naar links gedraaid)
                    double n1x = -v1y / len1;
                    double n1y = v1x / len1;

                    double n2x = -v2y / len2;
                    double n2y = v2x / len2;

                    // 4. Verschuif de oorspronkelijke punten loodrecht met de offset
                    Point2D a_offset = new Point2D(a.X + n1x * offset, a.Y + n1y * offset);
                    Point2D b_offset = new Point2D(b.X + n1x * offset, b.Y + n1y * offset);

                    Point2D c_offset = new Point2D(c.X + n2x * offset, c.Y + n2y * offset);
                    Point2D d_offset = new Point2D(d.X + n2x * offset, d.Y + n2y * offset);

                    // 5. Bereken het snijpunt tussen de twee nieuwe, verschoven lijnen (Lijn-Lijn intersectie)
                    double determinant = v1x * v2y - v1y * v2x;


                    // 2. BEREKEN OORSPRONKELIJK SNIJPUNT
                    double t_orig = ((c.X - a.X) * v2y - (c.Y - a.Y) * v2x) / determinant;
                    Point2D sp1 = new Point2D(a.X + t_orig * v1x, a.Y + t_orig * v1y);


                    if (Math.Abs(determinant) < 1e-9)
                        throw new InvalidOperationException("De lijnen lopen parallel; er is geen snijpunt.");

                    // Lineair stelsel oplossen op basis van de verschoven punten
                    double t = ((c_offset.X - a_offset.X) * v2y - (c_offset.Y - a_offset.Y) * v2x) / determinant;
                    Point2D sp2 = new Point2D(a_offset.X + t * v1x, a_offset.Y + t * v1y);




                    // Het verschil
                    Point2D delta = new(
                        Math.Max(sp1.X, sp2.X) - Math.Min(sp1.X,sp2.X),
                        Math.Max(sp1.Y, sp2.Y) - Math.Min(sp1.Y, sp2.Y));

                    Console.Write($"sp1 x{sp1.X} y{sp1.Y}");
                    Console.Write($"sp2 x{sp2.X} y{sp2.Y}");


                    return delta;
                }




                var translatie = BerekenLoodrechtSnijpunt(
                    new(0,0),
                    new(L, H/2.0),
                    new(L,0),
                    new(L,1),
                    off);

                Console.WriteLine($"offst = {off:0.000} mm");

                Console.WriteLine($"verschuiving = x={translatie.X:0.000} y={translatie.Y:0.000} mm");

                Console.WriteLine($"percentage = {percSp}");

                var translatiePruts = dBgl; // totdat we dekking correct hebben even prutsen    
                var posSp1 = H / 2.0 - translatie.Y -translatiePruts;
                var posSp2 = H - translatie.Y - translatiePruts;

                groep.ShapePosities = [0, posSp1 / H, posSp2 / H];
                Console.WriteLine($"$dy = {deltaSp:0.000} mm");
                Console.WriteLine($"hoek = {hoek:0.000} rad");
                Console.WriteLine($"p1 ={posSp1 / H} %");
                Console.WriteLine($"p2 ={posSp2 / H} %");


            }
            else
            {
                groep.Shapes = [Shape(0,xIn, L-off)];
                groep.ShapePosities = null;
            }
            groep.VerdeelStart = new Punt3D(0, 0, 0);
            groep.VerdeelEind = new Punt3D(0, H, 0);
            groep.Verdeling.OffsetStart = yFirst - 0;
            groep.Verdeling.OffsetEind = H - yLast;

            // Alleen bij Gelijkmatig het aantal uit de berekening overnemen;
            // een expliciete gebruikersverdeling (Exacte/BeoogdeHartOpHart) blijft staan.
            if (groep.Verdeling.Type == VerdelingType.Gelijkmatig)
            {
                groep.Verdeling.Aantal = Math.Max(r.AantalBeugels, 1);
            }

            return groep;
        }

        /// <summary>
        /// Verticale beugels; geometrie identiek aan <c>beugelVer()</c> in
        /// <c>j3console-build.js</c> (7-punts curve in het y-z-vlak), verdeeld
        /// langs x van xFirst tot xLast. Aantal = <see cref="J3ConsoleResult.AantalBeugels"/>.
        /// V1: geen alternerende spiegeling (flip).
        /// </summary>
        public static WapeningGroep BouwBeugelsVerticaal(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapBglsVer;
            var dBgl = groep.Diameter;
            var c = i.Dekking;
            var H = i.Hc;
            var B = i.Bc;

            var v1 = new Punt3D(0, 0, 0);
            var v2 = new Punt3D(i.Lc, 0, 0);

            var xFirst = dBgl / 2.0;
            var dx = i.FactorHEd * (i.Hc - r.D);
            var xLast = i.Ac + dx; // = N2x in de view

            var taper = i.AfschuiningOnderzijde && !r.VerticaleBeugelsNodig;
            var L = i.Lc;
            var yTopB = c + dBgl / 2.0;
            var zPos = B / 2.0 - c - 1.5 * dBgl; // binnen de horizontale beugel
            var zNeg = -zPos;
            var hook = 10 * dBgl;
            
            var dd = dBgl;

            // 7-punts curve op positie x; bij afschuining volgt de onderste
            // beugeltak de schuine rand (dekking op het diepste punt: x + dBgl).
            List<Punt3D> Punten(double x)
            {
                var yBotB = taper
                    ? BetonYOnder(x + dd/2.0, c + dBgl / 2.0, L, H)
                    : H - c - dBgl / 2.0;
                return
                [
                    new(0, yTopB, zPos - hook),        // 1: beginhaak
                    new(0, yTopB, zPos),               // 2: hoek A
                    new(dd / 2.0, yBotB, zPos),        // 3: hoek B
                    new(dd / 2.0, yBotB, zNeg),        // 4: hoek C
                    new(dd, yTopB, zNeg),              // 5: hoek D
                    new(dd, yTopB, zPos),              // 6: terug op hoek A
                    new(dd, yTopB + hook, zPos),       // 7: eindhaak
                ];
            }

            var buigstralen = Buigstralen(groep, 2.5 * dBgl);
            var startShape = new StaafShape { Punten = Punten(v1.X), Buigstralen = buigstralen };
            groep.Shapes = taper
                ? [startShape, new StaafShape { Punten = Punten(v2.X), Buigstralen = buigstralen }]
                : [startShape];
            groep.VerdeelStart = v1;
            groep.VerdeelEind = v2;

            if (groep.Verdeling.Type == VerdelingType.Gelijkmatig)
            {
                groep.Verdeling.Aantal = Math.Max(r.AantalBeugels, 1);
            }

            return groep;
        }
    }
}
