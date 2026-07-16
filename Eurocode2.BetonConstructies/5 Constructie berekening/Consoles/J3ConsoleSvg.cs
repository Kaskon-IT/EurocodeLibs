using System.Text;

namespace Eurocode.BetonConstructies
{
    using System.Globalization;

    public static class J3ConsoleSvg
    {

        /// <summary>
        /// Eenvoudig strut-and-tie schema: contour, knopen, drukdiagonalen,
        /// trekband en de belastingen/reacties. Bedoeld voor het markdown-
        /// rekenvoorbeeld (schaalbaar via viewBox, geen vaste afmetingen).
        /// Het modelassenstelsel (y naar beneden) valt samen met dat van SVG.
        /// </summary>
        public static string CreateSchemaSvg(J3ConsoleResult r, J3ConsoleInput i)
        {
            var ci = CultureInfo.InvariantCulture;
            string N(double v) => v.ToString("0.#", ci);

            double H = i.Hc, L = i.Lc, Bw = i.KolomDikte, ac = i.Ac;
            double y2 = i.Hc - r.D;
            double y1 = y2 + r.Z;
            double y3 = (y1 + y2) / 2.0;
            double dX = i.FactorHEd * (r.Hc - r.D);
            double hPl = i.DikteOplegmateriaal;
            double lPl = i.LoadPlateLength;

            // Knopen (zelfde afleiding als J3ConsoleViewModel)
            (double x, double y) n0 = (ac, 0);
            (double x, double y) n1 = (-r.X1 / 2.0, y1);
            (double x, double y) n2 = (ac + dX, y2);
            (double x, double y) n3 = (-r.X1 / 2.0, y3);
            (double x, double y) n4 = (ac + dX, y3);

            double kolomBoven = -0.35 * H, kolomOnder = H + 0.5 * H;
            double pijl = 0.5 * H; // pijllengte belasting
            double pijlH = 0.5 * pijl;

            var sb = new StringBuilder();

            string Rect(double x, double y, double w, double h, string stroke, string fill, string extra) =>
                $"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(w)}\" height=\"{N(h)}\" stroke=\"{stroke}\" stroke-width=\"1\" fill='{fill}' {extra}/>";


            string Line(double xa, double ya, double xb, double yb, string stroke, string extra = "") =>
                $"<line x1=\"{N(xa)}\" y1=\"{N(ya)}\" x2=\"{N(xb)}\" y2=\"{N(yb)}\" stroke=\"{stroke}\" stroke-width=\"6\" {extra}/>";

            void Strut(( double x, double y) a, (double x, double y) b) =>
                sb.AppendLine(Line(a.x, a.y, b.x, b.y, "#d32f2f", "stroke-dasharray=\"18 12\""));

            void Knoop((double x, double y) p, string naam, double dxLbl, double dyLbl)
            {
                sb.AppendLine($"<circle cx=\"{N(p.x)}\" cy=\"{N(p.y)}\" r=\"14\" stroke='#111' fill=\"#eee\"/>");
                sb.AppendLine($"<text x=\"{N(p.x + dxLbl)}\" y=\"{N(p.y + dyLbl)}\" font-size=\"52\" fill=\"#000\">{naam}</text>");
            }

            void Label(double x, double y, string text, string extra)
            {
                sb.AppendLine($"<text x=\"{N(x)}\" y=\"{N(y)}\" font-size=\"52\" fill=\"#000\" {extra}>{text}</text>");
            }

            void KrachtPijl(double x, double y, double dx, double dy, string kleur, string label)
            {
                double xe = x + dx, ye = y + dy;
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len < 1e-9) return;

                // Pijlpunt meeschalend met de pijllengte (met onder-/bovengrens),
                // in dezelfde kleur als de lijn; lijn stopt bij de basis van de punt.
                double kop = Math.Clamp(0.22 * len, 20, 60);
                double ux = dx / len, uy = dy / len;   // richting
                double bx = xe - kop * ux, by = ye - kop * uy; // basis van de punt
                double px = -uy, py = ux;              // loodrecht
                double halfB = 0.35 * kop;

                sb.AppendLine(Line(x, y, bx, by, kleur));
                sb.AppendLine($"<polygon points=\"{N(xe)},{N(ye)} {N(bx + halfB * px)},{N(by + halfB * py)} {N(bx - halfB * px)},{N(by - halfB * py)}\" fill=\"{kleur}\"/>");
                sb.AppendLine($"<text x=\"{N(xe + 15)}\" y=\"{N(ye - 15)}\" font-size=\"52\" fill=\"{kleur}\">{label}</text>");
            }

            // Contour kolom + console (licht)
            sb.AppendLine($"<path d=\"M {N(-Bw)} {N(kolomBoven)} L {N(-Bw)} {N(kolomOnder)} M 0 {N(kolomOnder)} L 0 {N(H)} L {N(L)} {N(H)} L {N(L)} 0 L 0 0 L 0 {N(kolomBoven)}\" fill=\"none\" stroke=\"#999\" stroke-width=\"4\"/>");

            // Trekband (blauw) en drukdiagonalen (rood, gestreept)
            sb.AppendLine(Line(n3.x, n3.y, n4.x, n4.y, "#1565c0"));
            Strut(n0, n2);
            Strut(n1, n2);
            Strut(n1, n3);
            Strut(n1, n4);
            Strut(n3, n2);
            Strut(n4, n2);

            // Knopen
            Knoop(n1, "1", -70, 0);
            Knoop(n2, "2", 30, 0);
            Knoop(n3, "3", -70, 0);
            Knoop(n4, "4", 30, 0);

            // plaat
            sb.AppendLine(Rect(n0.x - lPl / 2.0, -hPl, lPl, hPl, "red", fill: "red", ""));

            // Labels
            Label(n1.x -pijl - 10, n1.y, "F1h", "text-anchor='end' dominant-baseline='middle'");
            Label(n1.x, n1.y + pijl + 10, "F1v", "text-anchor='middle' dominant-baseline='hanging'");
            Label(n2.x - pijl - pijlH - 10, n2.y, "Ft", "text-anchor='end' dominant-baseline='middle'");
            Label(n0.x, n0.y - pijl - hPl -10, "F", "text-anchor='middle' dominant-baseline='above'");
            Label(n0.x + pijlH + 10, n0.y - hPl, "H", "text-anchor='start' dominant-baseline='middle'");



            // Krachten schalen: de grootste kracht krijgt de volle pijllengte,
            // alle andere pijlen worden relatief daaraan getekend.
            // nieuw inzicht
            // alle pijleen gelijke hoogte


            double fRes = Math.Sqrt(r.FEd * r.FEd + r.HEd * r.HEd);
            double fMax = Math.Max(Math.Max(fRes, Math.Max(r.FEd, r.HEd)),
                          Math.Max(Math.Max(r.F1x, r.FEd), r.Ft));
            double schaal = pijl / Math.Max(fMax, 1e-9);
            double LenF(double f) => f * schaal;

            // Belasting op n0: één resulterende pijl F = √(Fv² + Fh²),
            // in de werkelijke richting (Fh naar rechts, Fv omlaag).
            double fdx = LenF(r.HEd);
            double fdy = LenF(r.FEd);

           

            // HEd
            KrachtPijl(n0.x, n0.y - hPl, pijlH, 0, "#111", $"");
            // FEd
            KrachtPijl(n0.x, n0.y - hPl - pijl, 0, pijl, "#111", $"");

            // Fi
            //KrachtPijl(n0.x - fdx, n0.y - fdy, fdx, fdy, "#111", $"F");

            // n1: componenten + resultante (F1x naar rechts, FvEd omhoog)
            KrachtPijl(n1.x - pijl, n1.y, dx: pijl, 0, kleur: "#111", label: "");
            KrachtPijl(n1.x, n1.y + pijl, dx: 0, dy: -pijl, kleur: "#111", label: "");
            //KrachtPijl(n1.x - LenF(r.F1x), n1.y + LenF(r.FvEd), dx: LenF(r.F1x), dy: -LenF(r.FvEd), kleur: "#111", label: "Fc");

            // n2
            KrachtPijl(n2.x, n2.y, -(pijl + pijlH), 0, "#111", label: ""); // Ft = Fc + H dus 
            
            
            //sb.AppendLine($"<text x=\"{N(n2.x - 0.9 * pijl)}\" y=\"{N(n2.y - 30)}\" font-size=\"52\" fill=\"#1565c0\">F<tspan font-size=\"36\" dy=\"10\">t</tspan> = {N(r.Ft)} kN</text>");

            // ViewBox met marge rondom
            double minX = -Bw - pijl - 100, maxX = L + 3.2 * pijl;
            double minY = kolomBoven - pijl, maxY = kolomOnder + 60;

            return $@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""{N(minX)} {N(minY)} {N(maxX - minX)} {N(maxY - minY)}"" width=""100%"">
<defs><marker id=""pk"" markerWidth=""8"" markerHeight=""8"" refX=""6"" refY=""3"" orient=""auto""><path d=""M0,0 L7,3 L0,6 z""/></marker></defs>
{sb}</svg>";
        }

        public static string CreateSvg(J3ConsoleResult r, J3ConsoleInput i)
        {
            var ci = CultureInfo.InvariantCulture;

            double B = i.Bc;
            double Bw = i.KolomDikte;
            double H = i.Hc;
            double L = i.Lc;
            double x1 = r.X1;
            double ac = i.Ac;
            double c = i.Dekking;
            double phi = i.HoofdstaafDiameter;
            double y2 = H - r.D;
            double z = r.Z;
            double z0 = r.Z0;
            double z0y = z0 + y2;
            double d = r.D;
            double y1 = y2 + z;
            double y3 = (y1 + y2) / 2.0;

            double cccHoogte = r.Sigma1RdMax * 4;
            double cccBreedte = r.SigmaNode1Ed * 4;

            double dikteOpleg = r.DikteOplegmateriaal;



            // Coördinaten volgens jouw afspraak:
            // (0,0) = rand kolom + bovenkant console
            // x positief naar rechts
            // y positief naar beneden in het rekenmodel

            double n0x = ac;
            double n0y = 0;

            double dX = i.FactorHEd * (r.Hc - r.D);



            double n1x = -x1 / 2.0;
            double n1y = y1;

            double n2x = ac + dX;
            double n2y = y2;

            int strutWidth = 5;

            double n3x = n1x;
            double n3y = y3;

            double n4x = n2x;
            double n4y = n3y;

            double fEdX = ac;
            double fEdY = -dikteOpleg;

            // kolom met breedte x1: x=-x1 tot x=0
            // y=-H tot y=2H
            double colX = -x1;
            double colY = -100;
            double colW = x1;
            double colH = H + 200;
            double colBot = colY + colH;

            // console/nok: x=0 tot B, y=0 tot H
            double nokX = 0;
            double nokY = 0;
            double nokW = L;
            double nokH = H;

            // lastrect
            double plaatB = i.LoadPlateLength;
            double plaatX = ac - plaatB / 2.0;
            double plaatY = -dikteOpleg;
            double plaatH = dikteOpleg;



            // viewbox rondom model
            double margin = 80;
            double minX = -Bw - margin;
            double maxX = L + 200 + margin;
            double minY = -H - margin;
            double maxY = 2 * H + margin;

            // driehoek knoop 1
            double dhy = (d - z) * 2.0;
            double dhBot = H;
            double dhTop = H - dhy;
            double dhLeft = -x1;
            double dhRight = 0;


            string N(double v) => v.ToString("0.###", ci);

           

            string Line(
                double x1_, 
                double y1_, 
                double x2_, 
                double y2_,
                string stroke = "black",
                double sw = 2,
                double opacity = 1,
                string dash = "",
                string markerStart = "",
                string markerEnd = "")
            {
                var sb = new StringBuilder();
                sb.Append($"""<line x1="{N(x1_)}" y1="{N(y1_)}" x2="{N(x2_)}" y2="{N(y2_)}" stroke="{stroke}" opacity="{N(opacity)}" stroke-width="{N(sw)}" """);

                if (!string.IsNullOrWhiteSpace(dash))
                    sb.Append($"""stroke-dasharray="{dash}" """);

                if (!string.IsNullOrWhiteSpace(markerStart))
                    sb.Append($"""marker-start="{markerStart}" """);

                if (!string.IsNullOrWhiteSpace(markerEnd))
                    sb.Append($"""marker-end="{markerEnd}" """);

                sb.Append("/>");
                return sb.ToString();
            }

            string Text(double x, double y, string text, int size = 16, string anchor = "start", double rotate = 0, string baseline = "")
            {
                string transform = rotate != 0
                    ? $"""transform="rotate({N(rotate)} {N(x)} {N(y)})" """
                    : "";
                string baselineAttr = !string.IsNullOrWhiteSpace(baseline)
                    ? $"""dominant-baseline="{baseline}" """
                    : "";
                return $"""<text x="{N(x)}" y="{N(y)}" font-size="{size}" text-anchor="{anchor}" font-family="Arial, sans-serif" {baselineAttr}{transform}>{text}</text>""";
            }

            string Pijl(double x, double y, double hoek)
            {
                // Lengte ≈100, schacht 20 breed, pijlpunt 40 lang
                return $"""
                <path d="
                    M 0 -5
                    L 60 -5
                    L 60 -20
                    L 100 0
                    L 60 20
                    L 60 5
                    L 0 5
                    Z"
                    fill="black"
                    transform="translate({x:0.###},{y:0.###}) rotate({hoek:0.###})" />
                """;
            }

           

            string Console(double l, double h, double vk)
            {

                return $"""
                <path d="
                    M 0 0
                    L {(l-vk):0} 0
                    L {l:0} {vk:0}
                    L {l:0} {(h/2.0):0}
                    L 0 {h:0}
                    "
                    fill="gray"
                    stroke="black"
                    opacity="0.4"
                    />
                """;
            }

            string Circle(double x, double y, double r = 6)
                => $"""<circle cx="{N(x)}" cy="{N(y)}" r="{N(r)}" fill="black" />""";

            // Boog tussen twee hoeken (radialen) rond een middelpunt; y wijst naar beneden in SVG.
            string Arc(double cx, double cy, double radius, double a1, double a2,
                string stroke = "black", double sw = 1.5)
            {
                double sx = cx + radius * System.Math.Cos(a1);
                double sy = cy + radius * System.Math.Sin(a1);
                double ex = cx + radius * System.Math.Cos(a2);
                double ey = cy + radius * System.Math.Sin(a2);

                double delta = a2 - a1;
                int largeArc = System.Math.Abs(delta) > System.Math.PI ? 1 : 0;
                int sweep = delta > 0 ? 1 : 0;

                return $"""<path d="M {N(sx)} {N(sy)} A {N(radius)} {N(radius)} 0 {largeArc} {sweep} {N(ex)} {N(ey)}" fill="none" stroke="{stroke}" stroke-width="{N(sw)}" />""";
            }

            // Maatlijn tussen twee gemeten punten, verschoven met (offX, offY).
            // Tekent hulplijnen, een maatlijn met pijlen aan beide zijden en een label.
            string DimLine(double px1, double py1, double px2, double py2,
                double offX, double offY, string label,
                string color = "black", double ext = 8)
            {
                double d1x = px1 + offX, d1y = py1 + offY;
                double d2x = px2 + offX, d2y = py2 + offY;

                double offLen = System.Math.Sqrt(offX * offX + offY * offY);
                double ux = offLen > 0 ? offX / offLen : 0;
                double uy = offLen > 0 ? offY / offLen : 0;

                var sb = new StringBuilder();

                // hulplijnen (iets doorlopend voorbij de maatlijn)
                sb.Append(Line(px1, py1, d1x + ux * ext, d1y + uy * ext, color, 1));
                sb.Append(Line(px2, py2, d2x + ux * ext, d2y + uy * ext, color, 1));

                // maatlijn met pijlen aan beide zijden
                sb.Append(Line(d1x, d1y, d2x, d2y, color, 1,1, "", "url(#dim-arrow-j3)", "url(#dim-arrow-j3)"));

                // label net buiten de maatlijn, in het midden
                // verticale maatlijn: tekst 90° meedraaien (leesrichting onder->boven)
                bool isVertical = System.Math.Abs(d2y - d1y) > System.Math.Abs(d2x - d1x);
                double rotate = isVertical ? -90 : 0;
                double gap = 16;
                double mx = (d1x + d2x) / 2.0 + ux * gap;
                double my = (d1y + d2y) / 2.0 + uy * gap;
                sb.Append(Text(mx, my, label, 16, "middle", rotate, "central"));

                return sb.ToString();
            }




            // pijllengtes: Fc en Ft relatief tot FEd
            double fBase = 200.0;
            double fBaseWidth = 2.0;
            double fcLen = fBase * (r.Fc / i.FEd);
            double ftLen = fBase * (r.Ft / i.FEd);
            double fLen = fBase;
            double fhLen = fBase * (r.FactorHorizontaal);
            
            double fcWidth = fcLen / fLen * fBaseWidth;
            double ftWidth = fcWidth;

            // theta-boog in knoop 2: tussen horizontale trekstaaf en drukdiagonaal naar knoop 1
            double thStrutAng = System.Math.Atan2(n1y - n2y, n1x - n2x);
            double thTieAng = System.Math.Atan2(0, n1x - n2x);
            double thStrutLen = System.Math.Sqrt((n2x - n1x) * (n2x - n1x) + (n2y - n1y) * (n2y - n1y));
            double thR = thStrutLen * 0.28;
            double thMid = (thStrutAng + thTieAng) / 2.0;
            double thLabelR = thR + 22;
            double thLabelX = n2x + thLabelR * System.Math.Cos(thMid);
            double thLabelY = n2y + thLabelR * System.Math.Sin(thMid);


           


            return $"""
<svg xmlns="http://www.w3.org/2000/svg"
     viewBox="{N(minX)} {N(minY)} {N(maxX - minX)} {N(maxY - minY)}"
     width="100%"
     height="1200">

  <defs>
    <marker id="arrow-j3" markerWidth="10" markerHeight="10" refX="10" refY="5"
            orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L10,5 L0,10 Z" fill="black" opacity="0.9" />
    </marker>

    <marker id="dim-arrow-j3" markerWidth="16" markerHeight="16" refX="13" refY="8"
            orient="auto-start-reverse" markerUnits="userSpaceOnUse">
      <path d="M0,2 L13,8 L0,14 Z" fill="black" />
    </marker>
  </defs>

  <!-- kolom / drukvlak x1 -->
  <rect x="{N(colX)}" y="{N(H)}" width="{N(x1)}" height="{N(cccHoogte)}"
        fill="red" stroke="none" stroke-width="2" opacity="0.4" />

<rect x="{N(colX - cccBreedte)}" y="{N(H - dhy)}" width="{N(cccBreedte)}" height="{N(dhy)}"
        fill="red" stroke="none" stroke-width="2" opacity="0.4" />

  <!-- console / nok -->
   <rect x="{N(-Bw)}" y="{N(-100)}" width="{N(Bw)}" height="{N(H+200)}"
        fill="gray" stroke="black" stroke-width="1" opacity="0.4" />
 

        <!-- oplegplaat-->
 <rect x="{N(plaatX)}" y="{N(plaatY)}" width="{N(plaatB)}" height="{N(plaatH)}"
        fill="purple" stroke="black" stroke-width="1" opacity="0.4" />



  <!-- rand kolom x=0 -->
    {Line(-Bw, colY, -Bw, colBot, "black", 1.5)}
  
  
   {Console(L,H,i.Dekking)}




 {Line(-Bw, colY, 0, colY, "black", 1.5, 1,"8 6")}
 {Line(-Bw, colBot, 0, colBot, "black", 1.5, 1,"8 6")}





 <!-- driehoek -->
  {Line(dhLeft, dhBot, dhRight, dhBot, "red", 1)}
  {Line(dhLeft, dhBot, dhLeft, dhTop, "red", 1)}
  {Line(dhLeft, dhTop, dhRight, dhBot, "red", 1)}



  <!-- knopen -->
 {Circle(n0x, n0y, 7)}

  {Circle(n1x, n1y, 7)}
  {Text(n1x - 22, n1y + 5, "1", 18)}

  {Circle(n2x, n2y, 7)}
  {Text(n2x + 10, n2y + 5, "2", 18)}

 {Circle(n3x, n3y, 7)}
  {Text(n3x - 22, n3y + 5, "3", 18)}

  {Circle(n4x, n4y, 7)}
  {Text(n4x + 10, n4y + 5, "4", 18)}


 



  <!-- drukdiagonaal -->
  {Line(n0x, n0y, n2x, n2y, "black", strutWidth, 0.8, "10 8")}
  {Line(n1x, n1y, n2x, n2y, "black", strutWidth, 0.8, "10 8")}
  {Line(n1x, n1y, n3x, n3y, "black", strutWidth, 0.4,  "10 8")}
  {Line(n1x, n1y, n4x, n4y, "black", strutWidth, 0.4, "10 8")}
  {Line(n3x, n3y, n4x, n4y, "black", strutWidth, opacity:0.4, "")}
  {Line(n3x, n3y, n2x, n2y, "black", strutWidth, 0.4, "10 8")}
  {Line(n4x, n4y, n2x, n2y, "black", strutWidth, 0.4, "10 8")}
  {Line(n1x, n2y, n2x, n2y, "black", strutWidth)}


  <!-- theta -->
  {Arc(n2x, n2y, thR, thTieAng, thStrutAng, "black", 1.5)}
  {Text(thLabelX, thLabelY, "θ", 18, "middle")}

  <!-- maatlijnen -->
  <!-- a -->
  {DimLine(n1x, 0, n4x, 0, 0, -250, $"a")}
  
  <!-- ac -->
  {DimLine(0, 0, n0x, 0, 0, -200, $"ac")}
 {DimLine(-x1/2.0, 0, 0, 0, 0, -200, $"x1/2")}


  <!-- z0 -->
  {DimLine(L, n2y, L, z0y, 50, 0, $"z0")}
  <!-- z -->
  {DimLine(L, n2y, L, n1y, 100, 0, $"z")}
 
 <!-- d -->
  {DimLine(L, H, L, n2y, 150, 0, $"d")}
{DimLine(L, n2y, L, 0, 150, 0, $"d1")}
 
  <!-- H -->
  {DimLine(L, H, L, 0, 200, 0, $"H")}




  <!-- Ft op hoogte van knoop 2, naar links -->
  {Pijl(-x1, n2y, 180)}
  {Text(-x1 - 105, n2y, "Ft", 18, "end", 0, "central")}

  <!-- Fc vanaf knoop 1 - lengte naar rechts (op knoop1) -->
  {Pijl(-x1 -100, n1y, 0)}
  {Text(-x1 - 105, n1y, "Fc", 18, "end", 0, "central")}

  <!-- FEd op (ac, 0), naar beneden -->
  {Pijl(fEdX, fEdY - 100, 90)}
  {Text(fEdX, fEdY - 105, "FEd", 18, "middle", 0, "above")}

  <!-- HEd -->
  {Pijl(fEdX, fEdY , 0)}
  {Text(fEdX + 105, fEdY, "HEd", 18, "start", 0, "central")}



  <!-- F reactie bij knoop 1, omhoog -->
  {Pijl(n1x, dhBot +100, -90)}
  {Text(n1x, dhBot + 105, "F", 18, "middle", 0, "hanging")}


</svg>
""";
        }
    }
}
