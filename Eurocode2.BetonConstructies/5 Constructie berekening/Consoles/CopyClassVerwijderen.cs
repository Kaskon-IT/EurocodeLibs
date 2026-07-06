using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Models;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public enum J3ConsoleLinkType
    {
        Geen,
        HorizontaalOfSchuin,
        Verticaal
    }

    public enum J3NodeType
    {
        CCC,
        CCT,
        CTT
    }



    public class J3ConsoleInput : BaseInput
    {
        private double _l = 250;
        public double L
        {
            get => _l;
            set => SetProperty(ref _l, value);
        }

        private double _b = 350;
        public double B
        {
            get => _b;
            set => SetProperty(ref _b, value);
        }

        private double _bw = 400;
        public double Bw
        {
            get => _bw;
            set => SetProperty(ref _bw, value);
        }


        private double _h = 450;
        public double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        private double _ac = 125;
        public double Ac
        {
            get => _ac;
            set => SetProperty(ref _ac, value);
        }

        private double _fEd = 600;
        public double FEd
        {
            get => _fEd;
            set => SetProperty(ref _fEd, value);
        }

        private double _loadPlateLength = 200;
        public double LoadPlateLength
        {
            get => _loadPlateLength;
            set => SetProperty(ref _loadPlateLength, value);
        }

        private double _loadPlateWidth = 300;
        public double LoadPlateWidth
        {
            get => _loadPlateWidth;
            set => SetProperty(ref _loadPlateWidth, value);
        }

        private double _dekking = 35;
        public double Dekking
        {
            get => _dekking;
            set => SetProperty(ref _dekking, value);
        }

        private double _beugelDiameter = 8;
        public double BeugelDiameter
        {
            get => _beugelDiameter;
            set => SetProperty(ref _beugelDiameter, value);
        }

        private double _hoofdstaafDiameter = 16;
        public double HoofdstaafDiameter
        {
            get => _hoofdstaafDiameter;
            set => SetProperty(ref _hoofdstaafDiameter, value);
        }

        private double _fck = 30;
        public double Fck
        {
            get => _fck;
            set => SetProperty(ref _fck, value);
        }

        private double _alphaCc = 0.85;
        public double AlphaCc
        {
            get => _alphaCc;
            set => SetProperty(ref _alphaCc, value);
        }

        private double _gammaC = 1.5;
        public double GammaC
        {
            get => _gammaC;
            set => SetProperty(ref _gammaC, value);
        }

        private double _fyk = 500;
        public double Fyk
        {
            get => _fyk;
            set => SetProperty(ref _fyk, value);
        }

        private double _gammaS = 1.15;
        public double GammaS
        {
            get => _gammaS;
            set => SetProperty(ref _gammaS, value);
        }

        private double _vRdc;
        public double VRdc
        {
            get => _vRdc;
            set => SetProperty(ref _vRdc, value);
        }
    }

    public class J3ConsoleNodeResult
    {
        public string Naam { get; set; } = "";
        public J3NodeType Type { get; set; }

        public double X { get; set; }              // mm
        public double Y { get; set; }              // mm

        public double SigmaEd { get; set; }        // N/mm²
        public double SigmaRdMax { get; set; }     // N/mm²

        public double UnityCheck =>
            SigmaRdMax > 0 ? SigmaEd / SigmaRdMax : double.PositiveInfinity;

        public bool IsOk => UnityCheck <= 1.0;
    }

    public class J3ConsoleResult : IRowResult
    {
        public List<ResultRow> ResultRows { get; } = [];

        public double H { get; set; }
        public double B { get; set; }
        public double Ac { get; set; }
        public double FEd { get; set; }

        public double Fcd { get; set; }
        public double Fyd { get; set; }

        public double Sigma1RdMax { get; set; }
        public double Sigma2RdMax { get; set; }

        public double X1 { get; set; }
        public double A { get; set; }
        public double D { get; set; }
        
        public double Z { get; set; }

        public double Z0 { get; set; }
        public double Y1 { get; set; }

        public double Fc { get; set; }
        public double Ft { get; set; }
        public double Fwd { get; set; }

        public double AsMain { get; set; }
        public double Asw { get; set; }

        public double SigmaNode1Ed { get; set; }
        public double SigmaNode2Ed { get; set; }

        public bool Node1Ok => SigmaNode1Ed <= Sigma1RdMax;
        public bool Node2Ok => SigmaNode2Ed <= Sigma2RdMax;

        public bool HorizontaleBeugelsNodig => Ac < 0.5 * H;
        public bool VerticaleBeugelsNodig => Ac > 0.5 * H;


        public double Nu { get; set; } // (6.57N)

        public double Sigma3RdMax { get; set; }    // CTT



        public double TanTheta { get; set; }
        public double ThetaDeg { get; set; }
        public bool IsThetaOk => TanTheta >= 1.0 && TanTheta <= 2.5;
        public bool IsZ0Ok => Z0 > Ac;

        public J3ConsoleLinkType LinkType { get; set; }



        public List<J3ConsoleNodeResult> Nodes { get; set; } = [];

        public J3ConsoleNodeResult? Node1 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 1");

        public J3ConsoleNodeResult? Node2 =>
            Nodes.FirstOrDefault(x => x.Naam == "Knoop 2");



    }

    

public static class J3ConsoleSvg
    {
        public static string CreateSvg(J3ConsoleResult r, J3ConsoleInput i)
        {
            var ci = CultureInfo.InvariantCulture;

            double B = i.B;
            double Bw = i.Bw;
            double H = i.H;
            double L = i.L;
            double x1 = r.X1;
            double ac = i.Ac;
            double c = i.Dekking;
            double phi = i.HoofdstaafDiameter;
            double y2 = H - r.D;
            double z = r.Z;
            double d = r.D;
            double y1 = y2 + z;
            double y3 = (y1 + y2) / 2.0;

            double cccHoogte = r.Sigma1RdMax * 4;
            double cccBreedte = r.SigmaNode1Ed * 4;


            // Coördinaten volgens jouw afspraak:
            // (0,0) = rand kolom + bovenkant console
            // x positief naar rechts
            // y positief naar beneden in het rekenmodel
            double n1x = -x1 / 2.0;
            double n1y = y1;

            double n2x = ac;
            double n2y = y2;

            double n3x = n1x;
            double n3y = y3;

            double n4x = n2x;
            double n4y = n3y;

            double fEdX = ac;
            double fEdY = -20.0;

            // kolom met breedte x1: x=-x1 tot x=0
            // y=-H tot y=2H
            double colX = -x1;
            double colY = -H/2;
            double colW = x1;
            double colH = 2 * H;
            double colBot = colY + colH;

            // console/nok: x=0 tot B, y=0 tot H
            double nokX = 0;
            double nokY = 0;
            double nokW = L;
            double nokH = H;

            // lastrect
            double plaatB = i.LoadPlateLength;
            double plaatX = ac - plaatB / 2.0;
            double plaatY = -20.0;
            double plaatH = 20.0;



            // viewbox rondom model
            double margin = 80;
            double minX = -x1 - margin;
            double maxX = B + margin;
            double minY = -H - margin;
            double maxY = 2 * H + margin;

            // driehoek knoop 1
            double dhy = (d - z) * 2.0;
            double dhBot = H;
            double dhTop = H - dhy;
            double dhLeft = -x1;
            double dhRight = 0;


            string N(double v) => v.ToString("0.###", ci);

           

            string Line(double x1_, double y1_, double x2_, double y2_,
                string stroke = "black",
                double sw = 2,
                string dash = "",
                string markerStart = "",
                string markerEnd = "")
            {
                var sb = new StringBuilder();
                sb.Append($"""<line x1="{N(x1_)}" y1="{N(y1_)}" x2="{N(x2_)}" y2="{N(y2_)}" stroke="{stroke}" stroke-width="{N(sw)}" """);

                if (!string.IsNullOrWhiteSpace(dash))
                    sb.Append($"""stroke-dasharray="{dash}" """);

                if (!string.IsNullOrWhiteSpace(markerStart))
                    sb.Append($"""marker-start="{markerStart}" """);

                if (!string.IsNullOrWhiteSpace(markerEnd))
                    sb.Append($"""marker-end="{markerEnd}" """);

                sb.Append("/>");
                return sb.ToString();
            }

            string Text(double x, double y, string text, int size = 16, string anchor = "start")
                => $"""<text x="{N(x)}" y="{N(y)}" font-size="{size}" text-anchor="{anchor}" font-family="Arial, sans-serif">{text}</text>""";

            string Circle(double x, double y, double r = 6)
                => $"""<circle cx="{N(x)}" cy="{N(y)}" r="{N(r)}" fill="black" />""";




            // pijllengtes: Fc en Ft relatief tot FEd
            double fBase = 200.0;
            double fBaseWidth = 2.0;
            double fcLen = fBase * (r.Fc / i.FEd);
            double ftLen = fBase * (r.Ft / i.FEd);
            double fLen = fBase;
            double fcWidth = fcLen / fLen * fBaseWidth;
            double ftWidth = fcWidth;


           


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
  </defs>

  <!-- kolom / drukvlak x1 -->
  <rect x="{N(colX)}" y="{N(H)}" width="{N(x1)}" height="{N(cccHoogte)}"
        fill="black" stroke="none" stroke-width="2" opacity="0.4" />

<rect x="{N(colX - cccBreedte)}" y="{N(H - dhy)}" width="{N(cccBreedte)}" height="{N(dhy)}"
        fill="black" stroke="none" stroke-width="2" opacity="0.4" />

  <!-- console / nok -->
  <rect x="{N(nokX)}" y="{N(nokY)}" width="{N(nokW)}" height="{N(nokH)}"
        fill="white" stroke="black" stroke-width="2" />


 <rect x="{N(plaatX)}" y="{N(plaatY)}" width="{N(plaatB)}" height="{N(plaatH)}"
        fill="black" stroke="black" stroke-width="1" />



  <!-- rand kolom x=0 -->
  {Line(0, colY, 0, colBot, "black", 1.5)}
{Line(-Bw, colY, -Bw, colBot, "black", 1.5)}
  


 {Line(-Bw, colY, 0, colY, "black", 1.5, "8 6")}
 {Line(-Bw, colBot, 0, colBot, "black", 1.5, "8 6")}





 <!-- driehoek -->
  {Line(dhLeft, dhBot, dhRight, dhBot, "red", 1)}
  {Line(dhLeft, dhBot, dhLeft, dhTop, "red", 1)}
  {Line(dhLeft, dhTop, dhRight, dhBot, "red", 1)}



  <!-- knoop 1 en 2 -->
  {Circle(n1x, n1y, 7)}
  {Text(n1x - 22, n1y + 5, "1", 18)}

  {Circle(n2x, n2y, 7)}
  {Text(n2x + 10, n2y + 5, "2", 18)}

 {Circle(n3x, n3y, 7)}
  {Text(n3x - 22, n3y + 5, "3", 18)}

  {Circle(n4x, n4y, 7)}
  {Text(n4x + 10, n4y + 5, "4", 18)}


 



  <!-- drukdiagonaal -->
  {Line(n1x, n1y, n2x, n2y, "black", 5, "10 8")}
  {Line(n1x, n1y, n3x, n3y, "black", 4, "10 8")}
  {Line(n1x, n1y, n4x, n4y, "black", 4, "10 8")}
  {Line(n3x, n3y, n4x, n4y, "black", 1)}
  {Line(n3x, n3y, n2x, n2y, "black", 4, "10 8")}
  {Line(n4x, n4y, n2x, n2y, "black", 4, "10 8")}
  {Line(n1x, n2y, n2x, n2y, "black", 1)}





  <!-- Ft op hoogte van knoop 2, naar links -->
  {Line(-x1, n2y, -x1 - ftLen, n2y, "black", ftWidth, "", "", "url(#arrow-j3)")}
  {Text(-x1 - ftLen - 28, n2y, "Ft", 18)}

  <!-- Fc vanaf knoop 1 - lengte naar rechts (op knoop1) -->
  {Line(-x1 - fcLen, n1y, -x1, n1y, "black", fcWidth, "", "", "url(#arrow-j3)")}
  {Text(-x1 - fcLen - 28, n1y, "Fc", 18)}

  <!-- FEd op (ac, 0), naar beneden -->
  {Line(fEdX, fEdY - fLen, fEdX, fEdY, "black", fBaseWidth, "", "", "url(#arrow-j3)")}
  {Text(fEdX + 10, fEdY - fLen + 18, "FEd", 18)}

  <!-- F reactie bij knoop 1, omhoog -->
  {Line(n1x, dhBot + fLen, n1x, dhBot, "black", fBaseWidth, "", "", "url(#arrow-j3)")}
  {Text(n1x + 10, dhBot + fLen - 8, "F", 18)}


</svg>
""";
        }
    }


    public static class J3ConsoleCalculator
    {
        public static J3ConsoleResult Bereken(J3ConsoleInput i)
        {
            double fcd = i.AlphaCc * i.Fck / i.GammaC;
            
            double fyd = i.Fyk / i.GammaS;
            double nu = 1.0 - i.Fck / 250.0;

            double sigma1RdMax = 1.00 * nu * fcd; // CCC
            double sigma2RdMax = 0.85 * nu * fcd; // CCT
            double sigma3RdMax = 0.75 * nu * fcd; // CTT

            // x1 uit druksterkte node 1
            double x1 = i.FEd * 1000.0 / (sigma1RdMax * i.B);

            // a = ac + x1/2
            double a = i.Ac + x1 / 2.0;

            // J3: bepaal type aanvullende beugels
            J3ConsoleLinkType linkType = BepaalLinkType(i);

            // effectieve hoogte
            double d = i.H
                - i.Dekking
                - i.BeugelDiameter
                - 0.5 * i.HoofdstaafDiameter;

            // volgens jouw voorbeeld
            double z = 0.9 * d;
           
            double z0 = z * i.Ac / a;


            double y1 = (d - z) * 2; // volgens mij


            double tanTheta = z / a;
            double thetaDeg = Math.Atan(tanTheta) * 180.0 / Math.PI;




            // Rotational equilibrium: FEd * a = Fc * z
            double fc = i.FEd * a / z; // kN
            double ft = fc;

            // Main reinforcement
            double asMain = ft * 1000.0 / fyd;

            // Aanvullende wapening
            double fwd = ((2.0 * z / a - 1.0) / (3.0 + i.FEd / fc)) * fc;
            double asw = fwd * 1000.0 / fyd;

            // Node 1 verification
            double sigmaNode1Ed = fc * 1000.0 / (i.B * 2.0 * y1);

            // Node 2 verification below load plate
            double sigmaNode2Ed = i.FEd * 1000.0 / (i.LoadPlateLength * i.LoadPlateWidth);

            var result = new J3ConsoleResult
            {
                H = i.H, // wacht even dit is input?? maar die wil ik ook in resultaat... hmmm.. even nadenken
                B = i.B,
                FEd = i.FEd,
                Ac = i.Ac,

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

                TanTheta = tanTheta,
                ThetaDeg = thetaDeg,
                

                LinkType = linkType,

                Fc = fc,
                Ft = ft,
                Fwd = fwd,

                AsMain = asMain,
                Asw = asw
            };

            

            result.Nodes.Add(new J3ConsoleNodeResult
            {
                Naam = "Knoop 1",
                Type = J3NodeType.CCC,
                X = x1 / 2.0,
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

            return result;
        }

        private static J3ConsoleLinkType BepaalLinkType(J3ConsoleInput i)
        {
            if (i.Ac < 0.5 * i.H) 
                return J3ConsoleLinkType.HorizontaalOfSchuin;

            if (i.Ac > 0.5 * i.H && i.FEd > i.VRdc)
                return J3ConsoleLinkType.Verticaal; // 'slanke consoles' krijgen vertikale beugels!

            if (i.Ac == 0.5 * i.H)
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
                Waarde = i.L.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Hoogte console",
                SymboolHtml = "<i>H</i><sub>c</sub>",
                SymboolTex = @"H_c",
                Waarde = r.H.ToString("0"),
                Eenheid = "mm",
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Breedte console",
                SymboolHtml = "<i>B</i><sub>c</sub>",
                SymboolTex = @"B_c",

                Waarde = r.B.ToString("0"),
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
                Toelichting = "afstand belasting tot kolomrand",
                SymboolHtml = "<i>a</i><sub>c</sub>",
                SymboolTex = @"a_c",
                Waarde = i.Ac.ToString("0"),
                Eenheid = "mm"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Staafdiameter",
                SymboolHtml = "<i>Ø</i><sub>bgl</sub>",
                SymboolTex = @"Ø_{bgl}",
                Waarde = i.BeugelDiameter.ToString("0"),
                Eenheid = "mm"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Staafdiameter",
                SymboolHtml = "<i>Ø</i><sub>hoofd</sub>",
                SymboolTex = @"Ø_{hoofd}",
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
                    SymboolTex = @"a_c < 0.5 h_c",
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
                Toelichting = "Bepaal de capaciteit van de CCC-knoop",
                SymboolHtml = "σ<sub>1Rd,max</sub>",
                SymboolTex = @"\sigma_{1Rd,max}",
                Waarde = r.Sigma1RdMax.ToString("0.00"),
                Eenheid = "N/mm²",
                FormuleTex = @"\sigma_{1Rd,max}=1.0 ν'\cdot f_{cd}", Artikel = "(6.60)"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de capaciteit van de CCT-knoop",
                SymboolHtml = "σ<sub>2Rd,max</sub>",
                SymboolTex = @"\sigma_{2Rd,max}",
                Waarde = r.Sigma2RdMax.ToString("0.00"),
                Eenheid = "N/mm²",
                FormuleTex = @"\sigma_{2Rd,max}=0.85 ν'\cdot f_{cd}",
                Artikel = "(6.61)"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal vervolgens de lengte drukvlak knoop 1",
                SymboolHtml = "<i>x</i><sub>1</sub>",
                SymboolTex = @"x_1",
                Waarde = r.X1.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @"x_1=\frac{F_{Ed}}{\sigma_{1Rd,max}b}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal vervolgens de hefboomsarm",
                SymboolHtml = "<i>a</i>",
                SymboolTex = @"a",
                Waarde = r.A.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @"a=a_c+\frac{x_1}{2}"
            });





            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de nuttige hoogte",
                SymboolHtml = "<i>d</i>",
                SymboolTex = @"d",
                Waarde = r.D.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = r.VerticaleBeugelsNodig ? @"d=h-c-Ø_{bgl}-Ø_{hoofd}/2" : @"d=h-c-Ø_{hoofd}/2"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de inwendige hefboomsarm",
                SymboolHtml = "<i>z</i>",
                SymboolTex = @"z",
                Waarde = r.Z.ToString("0.0"),
                Eenheid = "mm",
                FormuleTex = @"z=0.9d"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Bepaal de hoek drukdiagonaal",
                SymboolHtml = "<i>θ</i>",
                SymboolTex = @"\theta",
                Waarde = r.ThetaDeg.ToString("0.#"),
                Eenheid = "º",
                FormuleTex = @"\theta = \arctan\left(\frac{a_c}{z}\right)"
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
                Waarde = r.Z0.ToString("0.#"),
                Eenheid = "mm",
                FormuleTex = @"z_0 = z \cdot \frac{a_c}{a}"

            });

            r.ResultRows.Add(new()
            {
                Toelichting = "controleer of voldoet aan eis",
                Waarde = r.IsZ0Ok ? "ok" : "niet ok",
                FormuleTex = @"a_c < z_0"
            });


            r.ResultRows.Add(new()
            {
                Toelichting = "Drukkracht volgt uit evenwicht",
                SymboolHtml = "<i>F</i><sub>c</sub>",
                Waarde = r.Fc.ToString("0.00"),
                Eenheid = "kN",
                FormuleTex = @"F_{Ed}a=F_cz"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Beugelkracht",
                SymboolHtml = "<i>F</i><sub>wd</sub>",
                Waarde = r.Fwd.ToString("0.00"),
                Eenheid = "kN",
                FormuleTex = @"F_{wd}=\frac{2z/a-1}{3+F_{Ed}/F_c}F_c"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Benodigde beugelwapening",
                SymboolHtml = "<i>A</i><sub>sl</sub>",
                Waarde = r.Asw.ToString("0"),
                Eenheid = "mm²",
                FormuleTex = @"A_{sw}=\frac{F_{wd}}{f_{yd}}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Benodigde hoofdwapening",
                SymboolHtml = "<i>A</i><sub>sw</sub>",
                Waarde = r.AsMain.ToString("0"),
                Eenheid = "mm²",
                FormuleTex = @"A_{sl}=\frac{F_{t}}{f_{yd}}"
            });

            r.ResultRows.Add(new()
            {
                Toelichting = "Controle knoop 1",
                SymboolHtml = "σ<sub>Ed,1</sub>",
                Waarde = $"{r.SigmaNode1Ed:0.00} ≤ {r.Sigma1RdMax:0.00}",
                Eenheid = "N/mm²",
                IsOk = r.Node1Ok,
                FormuleTex = @"\sigma_{Ed,1}=\frac{F_c}{b\cdot2y_1}"
            });




            r.ResultRows.Add(new()
            {
                Toelichting = "Controle knoop 2 onder oplegplaat",
                SymboolHtml = "σ<sub>Ed,2</sub>",
                Waarde = $"{r.SigmaNode2Ed:0.00} ≤ {r.Sigma2RdMax:0.00}",
                Eenheid = "N/mm²",
                IsOk = r.Node2Ok,
                FormuleTex = @"\sigma_{Ed,2}=\frac{F_{Ed}}{l_{plate}b_{plate}}"
            });
        }

    }
}
