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

        /// <summary>
        /// Hoofd-trekwapening (AsMain) als verticale haarspeld; geometrie identiek
        /// aan <c>rebar()</c> in <c>j3console-build.js</c>. Verdeeld over de
        /// beschikbare breedte (z-richting).
        /// </summary>
        public static WapeningGroep BouwVerticaleHaarspelden(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapVerticaleHaarspelden;
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

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = [bendR] }];

            var halfClear = Math.Max(i.Bc / 2.0 - c - dBgl - ctxPhi - phi / 2.0, 0);
            groep.VerdeelStart = new Punt3D(0, 0, -halfClear);
            groep.VerdeelEind = new Punt3D(0, 0, halfClear);

            return groep;
        }

        /// <summary>
        /// Tweede laag hoofdwapening als horizontale haarspeld; geometrie identiek
        /// aan de haarspeld in <c>J3ConsoleThreeView.razor</c>. Meerdere posities
        /// worden (v1) gestapeld in y met h.o.h. 2Ø, omhoog vanaf de onderste laag.
        /// </summary>
        public static WapeningGroep BouwHorizontaleHaarspelden(J3ConsoleInput i, J3ConsoleResult r)
        {
            var groep = i.WapHorizontaleHaarspelden;
            var toonTaper = i.AfschuiningOnderzijde && !r.VerticaleBeugelsNodig;

            var c = i.Dekking;
            var dBgl = i.WapBglsVer.Diameter;
            var phi = groep.Diameter;
            var ctxPhi = r.WapKolomMain.GrootsteDiameter;

            var H = i.Hc;
            var L = i.Lc;
            var Bw = i.KolomDikte;
            var B = i.Bc;

            var hoek = toonTaper ? Math.Atan2(H / 2.0, L) : 0;
            var hsC = c + dBgl + phi / 2.0;
            var hsDy = hsC / Math.Cos(hoek);
            var z1 = -B / 2.0 + c + dBgl + ctxPhi + phi / 2.0;
            var z2 = B / 2.0 - c - dBgl - ctxPhi - phi / 2.0;
            var x0 = Math.Max(-30 * phi, -Bw + c);

            List<Punt3D> punten =
            [
                new(x0, H - hsDy, z1),
                new(0, H - hsDy, z1),
                new(L - hsC, H - (L - hsC) * Math.Tan(hoek) - hsDy, z1),
                new(L - hsC, hsC, z1),
                new(L - hsC, hsC, z2),
                new(L - hsC, H - (L - hsC) * Math.Tan(hoek) - hsDy, z2),
                new(0, H - hsDy, z2),
                new(x0, H - hsDy, z2),
            ];

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = [phi * 2.5] }];

            // v1: lagen gestapeld in y (omhoog = -y in modelcoördinaten? nee: y omlaag,
            // dus tweede laag ligt hoger = kleinere y → richting -y).
            var n = Math.Max(groep.AantalPosities, 1);
            var hoh = 2 * phi;
            groep.VerdeelStart = new Punt3D(0, 0, 0);
            groep.VerdeelEind = new Punt3D(0, -(n - 1) * hoh, 0);
            if (n == 1)
            {
                groep.VerdeelEind = new Punt3D(0, -1, 0); // richting-only, 1 positie
            }

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
            var yFirst = y2 + 2 * dMain;
            var yLast = Math.Min(2.0 / 3.0 * r.D, H - c - dBgl / 2.0);

            var xOut = L - (c + dBgl / 2.0); // v1: rechte rand (geen afschuining-correctie per beugel)
            var xIn = -Bw + c + dBgl / 2.0;
            var zPos = B / 2.0 - c - dBgl / 2.0;
            var zNeg = -zPos;
            var hook = 10 * dBgl;
            var dd = dBgl;

            List<Punt3D> punten =
            [
                new(xIn, 0, zPos - hook),   // 1: beginhaak
                new(xIn, 0, zPos),          // 2: hoek A
                new(xOut, dd / 2.0, zPos),  // 3: hoek B
                new(xOut, dd / 2.0, zNeg),  // 4: hoek C
                new(xIn, dd, zNeg),         // 5: hoek D
                new(xIn, dd, zPos),         // 6: terug op hoek A
                new(xIn + hook, dd, zPos),  // 7: eindhaak
            ];

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = [2.5 * dBgl] }];
            groep.VerdeelStart = new Punt3D(0, yFirst, 0);
            groep.VerdeelEind = new Punt3D(0, yLast, 0);
            groep.Verdeling = new WapeningVerdeling
            {
                Type = VerdelingType.Gelijkmatig,
                Aantal = Math.Max(r.AantalBeugels, 1),
            };

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

            var yTopB = c + dBgl / 2.0;
            var yBotB = H - c - dBgl / 2.0; // v1: rechte onderzijde (geen afschuining-correctie)
            var zPos = B / 2.0 - c - 1.5 * dBgl; // binnen de horizontale beugel
            var zNeg = -zPos;
            var hook = 10 * dBgl;
            var dd = dBgl;

            List<Punt3D> punten =
            [
                new(0, yTopB, zPos - hook),        // 1: beginhaak
                new(0, yTopB, zPos),               // 2: hoek A
                new(dd / 2.0, yBotB, zPos),        // 3: hoek B
                new(dd / 2.0, yBotB, zNeg),        // 4: hoek C
                new(dd, yTopB, zNeg),              // 5: hoek D
                new(dd, yTopB, zPos),              // 6: terug op hoek A
                new(dd, yTopB + hook, zPos),       // 7: eindhaak
            ];

            groep.Shapes = [new StaafShape { Punten = punten, Buigstralen = [2.5 * dBgl] }];
            groep.VerdeelStart = new Punt3D(xFirst, 0, 0);
            groep.VerdeelEind = new Punt3D(xLast, 0, 0);
            groep.Verdeling = new WapeningVerdeling
            {
                Type = VerdelingType.Gelijkmatig,
                Aantal = Math.Max(r.AantalBeugels, 1),
            };

            return groep;
        }
    }
}
