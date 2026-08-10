namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Maakt de wapeninggroepen uit <see cref="J3ConsoleInput"/> geometrisch af
    /// (shapes + verdeellijn) op basis van het berekende resultaat.
    /// De geometrie is een 1-op-1 port van de bestaande Three.js-bouwers
    /// (<c>rebar()</c>, <c>beugel()</c>, <c>beugelVer()</c> en de haarspeld in
    /// <c>J3ConsoleThreeView.razor</c>), in modelcoördinaten (y omlaag).
    /// </summary>
    public static class J3ConsoleWapeningBuilder
    {
        /// <summary>
        /// Vult de vier groepen op het resultaat. Aanroepen nadat alle scalars
        /// van het resultaat (D, Z, AantalBeugels, ...) bepaald zijn.
        /// </summary>
        public static void VulGroepen(J3ConsoleInput i, J3ConsoleResult r)
        {
            r.WapVerticaleHaarspelden = BouwVerticaleHaarspelden(i, r);
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
            var hspPlat = i.WapHorizontaleHaarspelden;
            var toonTaper = i.AfschuiningOnderzijde && !r.VerticaleBeugelsNodig;

            var c = i.Dekking;
            var dBgl = i.WapBglsVer.Diameter;
            var phi = groep.Diameter;
            var bendR = i.HoofdstaafBuigdoornDiameterFactor > 0
                ? (i.HoofdstaafBuigdoornDiameterFactor * phi + phi) / 2.0
                : (r.BuigdoorMain + phi) / 2.0;
            var ctxPhi = r.WapKolomMain.GrootsteDiameter;

            var H = i.Hc;
            var L = i.Lc;
            var Bw = i.KolomDikte;

            var cc = c + dBgl + phi / 2.0;   // hart hoofdstaaf
            var yTop = cc;
            var xTip = L - cc;
            var up = 10 * phi + bendR;
            var left = -Bw + cc;

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

            var c = i.Dekking;
            var dBglHor = i.WapBglsHor.Diameter;
            var dBglVer = i.WapBglsVer.Diameter;
            var phi = groep.Diameter;

            var L = i.Lc;
            var Bw = i.KolomDikte;
            var B = i.Bc;

            var zVoor = B / 2.0 - c - dBglHor - dBglVer - phi/2.0;
            var zAchter = -B / 2.0 + c + dBglHor + dBglVer + phi/2.0;

            var xLinks = -Bw + c + dBglHor + groep.Diameter / 2.0;
            var xTip = L - c - groep.Diameter / 2.0;

            var yTrek = i.Hc - r.D;        // positie trekband (d1) [mm, y omlaag]
            var yHaak = yTrek + Math.Min(20 * phi, 200);  // 10Ø onder de trekband

            List<Punt3D> punten =
            [
                new(xLinks, yHaak, zVoor),   // 1: haak, voorvlak
                new(xLinks, yTrek, zVoor),   // 2: trekband, voorvlak
                new(xTip, yTrek, zVoor),     // 3: consoletip, voorvlak
                new(xTip, yTrek, zAchter),   // 4: consoletip, achtervlak
                new(xLinks, yTrek, zAchter), // 5: trekband, achtervlak
                new(xLinks, yHaak, zAchter), // 6: haak, achtervlak
            ];

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = Buigstralen(groep, phi * 2.5) }];

            // Eén positie: verdeellijn is richting-only.
            groep.VerdeelStart = new Punt3D(0, 0, 0);
            groep.VerdeelEind = new Punt3D(0, 100, 0);
            groep.Verdeling.Type = VerdelingType.ExacteHartOpHart;
            groep.Verdeling.HartOpHartAfstanden = [20];
            groep.Buigstralen = [55]; // fixed
            //groep.Verdeling = new WapeningVerdeling { Type = VerdelingType.Gelijkmatig };

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
                var deltaSp = Math.Cos(Math.Atan2(H / 2.0, L)) * off;
                var percSp = deltaSp / H;
                Console.WriteLine($"percentage = {percSp}");
                groep.ShapePosities = [0, 0.5 - percSp, 1.0 - percSp/2.0];
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
                    ? BetonYOnder(x + dBgl, c + dBgl / 2.0, L, H)
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
            var startShape = new StaafShape { Punten = Punten(xFirst), Buigstralen = buigstralen };
            groep.Shapes = taper
                ? [startShape, new StaafShape { Punten = Punten(xLast), Buigstralen = buigstralen }]
                : [startShape];
            groep.VerdeelStart = new Punt3D(xFirst, 0, 0);
            groep.VerdeelEind = new Punt3D(xLast, 0, 0);

            if (groep.Verdeling.Type == VerdelingType.Gelijkmatig)
            {
                groep.Verdeling.Aantal = Math.Max(r.AantalBeugels, 1);
            }

            return groep;
        }
    }
}
