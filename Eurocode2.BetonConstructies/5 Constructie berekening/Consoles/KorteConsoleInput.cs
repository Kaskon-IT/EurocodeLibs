using CommonLibrary.Models;
using ExportFactory.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.BetonConstructies
{

    public class KorteConsoleInput : BaseInput
    {

        private double _vEd = 100;
        public double VEd
        {
            get => _vEd;
            set => SetProperty(ref _vEd, value);
        }

        private double _a = 200;
        public double A
        {
            get => _a;
            set => SetProperty(ref _a, value);
        }

        private double _h = 400;
        public double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
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
    }

    public class KorteConsoleResult
    {
        public double D { get; set; }
        public double Z { get; set; }
        public double ThetaDeg { get; set; }

        public double TEd { get; set; }      // kN
        public double CEd { get; set; }      // kN
        public double AsReq { get; set; }    // mm²

        public bool IsKorteConsole { get; set; }
    }

    public class KorteConsoleBerekening
    : BaseBerekening<KorteConsoleInput, KorteConsoleResult>
    {
        public override string Naam => "Korte console";

        public KorteConsoleBerekening(KorteConsoleInput input) : base(input)
        {
        }

        protected override KorteConsoleResult BerekenInternal()
        {
            var i = Input;

            double d = i.H - i.Dekking - i.BeugelDiameter - 0.5 * i.HoofdstaafDiameter;
            double z = 0.8 * d;

            double theta = Math.Atan(z / i.A);
            double thetaDeg = theta * 180.0 / Math.PI;

            double tEd = i.VEd * i.A / z;
            double cEd = i.VEd / Math.Sin(theta);

            double fyd = i.Fyk / i.GammaS;
            double asReq = tEd * 1000.0 / fyd;

            Formules.Add(new(
                "Effectieve hoogte",
                @"d = h - c - \varnothing_{beugel} - \frac{1}{2}\varnothing_{hoofd}",
                $"{i.H:0} - {i.Dekking} - {i.BeugelDiameter} - \\frac{{1}}{{2}}\\cdot{i.HoofdstaafDiameter:0} = {d:0}"));

            Formules.Add(new(
                "Inwendige hefboomsarm",
                @"z = 0{,}8d",
                $"{z:0}"));

            Formules.Add(new(
                "Hoek drukstang",
                @"\theta = \arctan\left(\frac{z}{a}\right)",
                $"{thetaDeg:0.0}"));

            Formules.Add(new(
                "Trekkracht hoofdwapening",
                @"T_{Ed} = \frac{V_{Ed} \cdot a}{z}",
                $"{tEd:0.0}"));

            Formules.Add(new(
                "Benodigde hoofdwapening",
                @"A_{s,req} = \frac{T_{Ed}}{f_{yd}}",
                $"{asReq:0}"));

            return new KorteConsoleResult
            {
                D = d,
                Z = z,
                ThetaDeg = thetaDeg,
                TEd = tEd,
                CEd = cEd,
                AsReq = asReq,
                IsKorteConsole = i.A <= d
            };
        }
    }

    public static class KorteConsoleSvg
    {
        public static string CreateTruss1(KorteConsoleInput i)
        {
            double a = i.A;
            double h = i.H;

            const double s = 0.75;
            double xKolom = 120;
            double yTop = 70;

            double w = Math.Max(260, a * s + 80);
            double hh = h * s;

            double x0 = xKolom;
            double y0 = yTop;
            double x1 = x0 + w;
            double y1 = y0 + hh;

            double loadX = x0 + a * s;
            double topTieY = y0 + 45;
            double node1X = x0 + 25;
            double node1Y = y1 - 45;
            double node2X = loadX;
            double node2Y = topTieY;

            return $"""
<svg xmlns="http://www.w3.org/2000/svg"
     viewBox="0 0 {x1 + 120:0} {y1 + 70:0}"
     width="100%"
     height="260">

  <defs>
    <marker id="arrow" markerWidth="8" markerHeight="8" refX="4" refY="4"
            orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L8,4 L0,8 Z" fill="black" />
    </marker>
  </defs>

  <!-- kolomvlak -->
  <line x1="{x0:0}" y1="20" x2="{x0:0}" y2="{y1 + 40:0}"
        stroke="black" stroke-width="2" stroke-dasharray="12 8" />

  <!-- console -->
  <rect x="{x0:0}" y="{y0:0}" width="{w:0}" height="{hh:0}"
        fill="none" stroke="black" stroke-width="2" />

  <!-- trekband boven -->
  <line x1="{x0 - 70:0}" y1="{topTieY:0}" x2="{node2X + 45:0}" y2="{topTieY:0}"
        stroke="black" stroke-width="4" marker-start="url(#arrow)" />
  <text x="{x0 - 105:0}" y="{topTieY - 8:0}" font-size="18">F<tspan baseline-shift="super" font-size="12">t</tspan></text>

  <!-- drukstaaf -->
  <line x1="{node1X:0}" y1="{node1Y:0}" x2="{node2X:0}" y2="{node2Y:0}"
        stroke="black" stroke-width="4" stroke-dasharray="12 8" />
  <text x="{(node1X + node2X) / 2 + 8:0}" y="{(node1Y + node2Y) / 2:0}" font-size="18">Fc2</text>

  <!-- belasting boven -->
  <line x1="{loadX:0}" y1="{y0 - 40:0}" x2="{loadX:0}" y2="{topTieY - 8:0}"
        stroke="black" stroke-width="3" marker-end="url(#arrow)" />
  <text x="{loadX + 12:0}" y="{y0 - 18:0}" font-size="18">F<tspan baseline-shift="super" font-size="12">′</tspan><tspan baseline-shift="sub" font-size="12">Ed</tspan></text>

  <!-- oplegreactie / belasting onder -->
  <line x1="{node1X:0}" y1="{y1 + 35:0}" x2="{node1X:0}" y2="{node1Y + 12:0}"
        stroke="black" stroke-width="3" marker-end="url(#arrow)" />
  <text x="{node1X + 12:0}" y="{y1 + 15:0}" font-size="18">F<tspan baseline-shift="sub" font-size="12">Ed</tspan></text>

  <!-- oplegvlak -->
  <rect x="{node1X - 35:0}" y="{node1Y - 8:0}" width="55" height="24"
        fill="none" stroke="black" stroke-width="2" />
  <path d="M {node1X - 35:0},{node1Y + 16:0} l 10,-24 m 0,24 l 10,-24 m 0,24 l 10,-24 m 0,24 l 10,-24"
        stroke="black" stroke-width="1.5" />

  <!-- knopen -->
  <text x="{node1X - 48:0}" y="{node1Y - 12:0}" font-size="18">1</text>
  <text x="{node2X + 12:0}" y="{node2Y + 5:0}" font-size="18">2</text>

  <!-- maat a -->
  <line x1="{x0:0}" y1="{y0 - 20:0}" x2="{loadX:0}" y2="{y0 - 20:0}"
        stroke="black" stroke-width="1.5"
        marker-start="url(#arrow)" marker-end="url(#arrow)" />
  <text x="{(x0 + loadX) / 2:0}" y="{y0 - 28:0}" font-size="18">a</text>

  <!-- maat z -->
  <line x1="{x1 + 35:0}" y1="{topTieY:0}" x2="{x1 + 35:0}" y2="{node1Y:0}"
        stroke="black" stroke-width="1.5"
        marker-start="url(#arrow)" marker-end="url(#arrow)" />
  <text x="{x1 + 48:0}" y="{(topTieY + node1Y) / 2:0}" font-size="18">z</text>

  <!-- maat d -->
  <line x1="{x1 + 65:0}" y1="{y0:0}" x2="{x1 + 65:0}" y2="{y1:0}"
        stroke="black" stroke-width="1.5"
        marker-start="url(#arrow)" marker-end="url(#arrow)" />
  <text x="{x1 + 78:0}" y="{(y0 + y1) / 2:0}" font-size="18">d</text>

  <!-- titel -->
  <text x="{x0 + w / 2 - 35:0}" y="{y1 + 45:0}" font-size="20">TRUSS 1</text>
</svg>
""";
        }


        public static string Create(KorteConsoleInput input)
        {
            var a = input.A;
            var h = input.H;

            // vaste schaal / layout
            const double xKolom = 80;
            const double yTop = 60;
            const double kolomB = 80;

            const double schaal = 0.6;
            double consoleL = Math.Max(180, a * schaal + 100);
            double consoleH = h * schaal;

            double x0 = xKolom + kolomB;
            double y0 = yTop + 80;
            double y1 = y0 + consoleH;

            double loadX = x0 + a * schaal;
            double loadY = y0;

            double wapY = y1 - 35;
            double nodeX = x0 - 25;
            double nodeY = wapY;

            double width = x0 + consoleL + 120;
            double height = y1 + 90;

            return $"""
<svg xmlns="http://www.w3.org/2000/svg"
     viewBox="0 0 {width:0} {height:0}"
     width="100%"
     height="260">

  <defs>
    <marker id="arrow" markerWidth="8" markerHeight="8" refX="4" refY="4"
            orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L8,4 L0,8 Z" fill="black" />
    </marker>
  </defs>

  <!-- kolom -->
  <rect x="{xKolom}" y="20" width="{kolomB}" height="{height - 40:0}"
        fill="#ddd" stroke="black" stroke-width="2" />

  <!-- kolomvlak -->
  <line x1="{x0}" y1="20" x2="{x0}" y2="{height - 20:0}"
        stroke="black" stroke-width="1.5" stroke-dasharray="6 5" />

  <!-- console -->
  <rect x="{x0}" y="{y0}" width="{consoleL:0}" height="{consoleH:0}"
        fill="#eee" stroke="black" stroke-width="2" />

  <!-- oplegplaat -->
  <rect x="{loadX - 25:0}" y="{loadY - 12:0}" width="50" height="12"
        fill="#555" stroke="black" />

  <!-- belasting VEd -->
  <line x1="{loadX:0}" y1="{loadY - 65:0}" x2="{loadX:0}" y2="{loadY - 15:0}"
        stroke="black" stroke-width="5" marker-end="url(#arrow)" />
  <text x="{loadX + 15:0}" y="{loadY - 42:0}" font-size="18">V<tspan baseline-shift="sub" font-size="12">Ed</tspan></text>

  <!-- maat a -->
  <line x1="{x0}" y1="{y0 - 45:0}" x2="{loadX}" y2="{y0 - 45:0}"
        stroke="black" stroke-width="1.5"
        marker-start="url(#arrow)" marker-end="url(#arrow)" />
  <text x="{(x0 + loadX) / 2 - 5:0}" y="{y0 - 55:0}" font-size="20">a</text>

  <!-- maat h -->
  <line x1="{x0 + consoleL + 35:0}" y1="{y0}" x2="{x0 + consoleL + 35:0}" y2="{y1}"
        stroke="black" stroke-width="1.5"
        marker-start="url(#arrow)" marker-end="url(#arrow)" />
  <text x="{x0 + consoleL + 48:0}" y="{(y0 + y1) / 2:0}" font-size="20">h</text>

  <!-- drukstang -->
  <line x1="{nodeX:0}" y1="{nodeY:0}" x2="{loadX:0}" y2="{loadY:0}"
        stroke="#0047bb" stroke-width="4" stroke-dasharray="10 8" />
  <text x="{(nodeX + loadX) / 2 - 20:0}" y="{(nodeY + loadY) / 2 - 10:0}"
        font-size="16" fill="#0047bb">
    drukstang C<tspan baseline-shift="sub" font-size="11">Ed</tspan>
  </text>

  <!-- hoofdwapening / trekband -->
  <line x1="{x0 + 5:0}" y1="{wapY:0}" x2="{x0 + consoleL - 20:0}" y2="{wapY:0}"
        stroke="red" stroke-width="7" />
  <text x="{x0 + 90:0}" y="{wapY + 28:0}" font-size="16" fill="red">
    trekband / hoofdwapening T<tspan baseline-shift="sub" font-size="11">Ed</tspan>
  </text>

  <!-- verankering -->
  <path d="M {x0 + 5:0},{wapY:0}
           C {x0 - 40:0},{wapY:0} {x0 - 55:0},{wapY - 60:0} {x0 - 25:0},{wapY - 85:0}"
        fill="none" stroke="black" stroke-width="3" stroke-dasharray="8 6" />

  <!-- knoop -->
  <circle cx="{nodeX:0}" cy="{nodeY:0}" r="8" fill="black" />
  <circle cx="{loadX:0}" cy="{loadY:0}" r="6" fill="black" />

  <!-- label -->
  <text x="40" y="{height - 25:0}" font-size="16">
    Korte console — drukstang-trekbandmodel
  </text>
</svg>
""";
        }
    }



}
