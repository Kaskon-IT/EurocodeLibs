using System.Text;

namespace Eurocode.BetonConstructies
{
    using System.Globalization;

    public static class J3ConsoleSvg
    {
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
