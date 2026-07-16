using System.Globalization;
using CommonLibrary.Models;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Genereert een markdown-rekenvoorbeeld (met LaTeX-formules) voor de J3-console
    /// volgens het strut-and-tie model. De markdown kan met Markdig + KaTeX op het
    /// scherm of in een rapport worden getoond.
    /// <para>
    /// Gebouwd met de generieke <see cref="RekenvoorbeeldBuilder"/>: stapnummers
    /// lopen automatisch op en stappen kunnen per sleutel worden overgeslagen,
    /// vervangen of ingevoegd, bijv. <c>MaakBuilder(i, r).Skip("fwd").Build()</c>.
    /// </para>
    /// </summary>
    public static class J3ConsoleRekenvoorbeeld
    {
        private static string N(double v, string format = "0.0") =>
            v.ToString(format, CultureInfo.InvariantCulture);

        public static string Genereer(J3ConsoleInput i, J3ConsoleResult r) =>
            MaakBuilder(i, r).Build();

        /// <summary>
        /// Exposeert de builder zodat aanroepers stappen kunnen overslaan of
        /// aanpassen vóór <see cref="RekenvoorbeeldBuilder.Build"/>.
        /// </summary>
        public static RekenvoorbeeldBuilder MaakBuilder(J3ConsoleInput i, J3ConsoleResult r)
        {
            double sigmaN = r.Sigma1RdMax;
            double sigmaCCT = r.Sigma2RdMax;
            double deltaA = i.FactorHEd * (i.Hc - r.D);
            double w = r.Fc * 1000.0 / (i.Bc * sigmaN);
            string aswFactorTex = r.LinkType == J3ConsoleLinkType.Verticaal ? "0{,}5" : "0{,}25";
            double aswMin = (r.LinkType == J3ConsoleLinkType.Verticaal ? 0.5 : 0.25) * r.AsMain;

            return new RekenvoorbeeldBuilder("Rekenvoorbeeld console (Strut-and-Tie)")
                .Intro($@"## Uitgangspunten

| Grootheid | Waarde |
| --- | --- |
| Verticale belasting           | $F_{{v,Ed}}={N(r.FEd, "0")}\ \mathrm{{kN}}$ |
| Horizontale belasting         | $F_{{h,Ed}}={N(r.HEd, "0")}\ \mathrm{{kN}}$ |
| Afstand belasting tot kolom   | $a_c={N(i.Ac, "0")}\ \mathrm{{mm}}$ |
| Breedte console               | $b_c={N(i.Bc, "0")}\ \mathrm{{mm}}$ |
| Hoogte console                | $h_c={N(i.Hc, "0")}\ \mathrm{{mm}}$ |
| Dekking                       | $h_c={N(i.Dekking, "0")}\ \mathrm{{mm}}$ |
| Diameter                      | $Ø_{{main}}={N(i.HoofdstaafDiameter, "0")}\ \mathrm{{mm}}$ |
| Effectieve hoogte             | $d={N(r.D)}\ \mathrm{{mm}}$ |
| Hefboomarm trekband           | $z={N(r.Z)}\ \mathrm{{mm}}$ |
| Toelaatbare CCC-spanning      | $\sigma_n=\sigma_{{Rd,\max}}={N(sigmaN, "0.00")}\ \mathrm{{N/mm^2}}$ |
| Toelaatbare CCT-spanning      | $\sigma_n=\sigma_{{Rd,\max}}={N(sigmaCCT, "0.00")}\ \mathrm{{N/mm^2}}$ |

### Schema

<div style=""max-width:480px"">
{r.SchemaSvg}
</div>")

                .Stap_("schema", "schema", r.SchemaSvg)

                .Stap_("x1", "Verticaal knoopvlak", $@"De bekende verticale belasting bepaalt direct de hoogte van het eerste knoopvlak:

$$
x_1=\frac{{F_{{v,Ed}}}}{{b\,\sigma_n}}
=\frac{{{N(r.FEd * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(r.X1)}\ \mathrm{{mm}}
$$")

                .Stap_("deltaA", "Correctie voor horizontale belasting", $@"Een horizontale belasting op de bovenzijde van de console veroorzaakt een extra moment.
De vergroting van de arm is:

$$
\Delta a=\frac{{F_{{h,Ed}}}}{{F_{{v,Ed}}}}\,(h_c-d)
={N(i.FactorHEd, "0.00")}\cdot{N(i.Hc - r.D)}
={N(deltaA)}\ \mathrm{{mm}}
$$")

                .Stap_("a", "Horizontale arm", $@"Het knooppunt ligt in het midden van het knoopvlak:

$$
a=a_c+\frac{{x_1}}{{2}}+\Delta a
={N(i.Ac, "0")}+\frac{{{N(r.X1)}}}{{2}}+{N(deltaA)}
={N(r.A)}\ \mathrm{{mm}}
$$")

                .Stap_("z", "Hefboomarm trekband", $@"De hefboomarm van de trekband is:

$$
z={N(i.FactorZ, "0.0")}\cdot d
= {N(i.FactorZ, "0.0")}\cdot{N(r.D, "0")}
= {N(r.Z)}\ \mathrm{{mm}}
$$")

                .Stap_("ft", 
                "Trekbandkracht",
                $@"Momentenevenwicht (de horizontale belasting zit al in $a$ via $\Delta a$):

$$
F_{{t}}=H+\frac{{F_{{v,Ed}}\,a}}{{z}}
={N(r.HEd)}+ \frac{{{N(r.FEd, "0")}\cdot{N(r.A)}}}{{{N(r.Z)}}}
={N(r.Ft)}\ \mathrm{{kN}}
$$")

                .Stap_("fc", "Drukstaaf", $@"Uit krachtenevenwicht:

$$
F_c=\sqrt{{F_{{v,Ed}}^2+F_{{1x}}^2}}
=\sqrt{{{N(r.FEd, "0")}^2+{N(r.F1x)}^2}}
={N(r.Fc)}\ \mathrm{{kN}}
$$")

                .Stap_("theta", "Hoek drukstaaf", $@"$$
\theta=\arctan\left(\frac{{z}}{{a}}\right)
=\arctan\left(\frac{{{N(r.Z)}}}{{{N(r.A)}}}\right)
={N(r.ThetaDeg)}^\circ
$$")

                .Stap_("y1", "Horizontaal knoopvlak", $@"$$
y_1=\frac{{F_{{t,Ed}}}}{{b\,\sigma_n}}
=\frac{{{N(r.Ft * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(r.Y1)}\ \mathrm{{mm}}
$$")

                .Stap_("w1", "Breedte loodrecht op de drukstaaf", $@"$$
w_1=\frac{{F_c}}{{b\,\sigma_n}}
=\frac{{{N(r.Fc * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(w)}\ \mathrm{{mm}}
$$")

                .Stap_("fwd", "Kracht in aanvullende beugels", $@"De drukdiagonaal spreidt tussen knoop 1 en knoop 2; de aanvullende beugels
nemen de spreidkracht $F_{{wd}}$ op (vgl. NEN-EN 1992-1-1 J.3):

$$
F_{{wd}}=\frac{{\dfrac{{2z}}{{a}}-1}}{{3+\dfrac{{F_{{v,Ed}}}}{{F_{{1x}}}}}}\,F_{{1x}}
=\frac{{\dfrac{{2\cdot{N(r.Z)}}}{{{N(r.A)}}}-1}}{{3+\dfrac{{{N(r.FEd, "0")}}}{{{N(r.F1x)}}}}}\cdot{N(r.F1x)}
={N(r.Fwd)}\ \mathrm{{kN}}
$$")

                .Stap_("wapening", "Benodigde wapening", $@"Hoofdtrekwapening uit de trekbandkracht:

$$
A_{{s,req}}=\frac{{F_{{t,Ed}}}}{{f_{{yd}}}}
=\frac{{{N(r.Ft * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}}
={N(r.AsMain, "0")}\ \mathrm{{mm^2}}
$$

Aanvullende {(r.VerticaleBeugelsNodig? "vertikale" : "horizontale")} beugels uit $F_{{wd}}$, met als minimum ${aswFactorTex}\,A_{{s,req}}$:

$$
A_{{sw,req}}=\max\left(\frac{{ F_{{wd}} }}{{ f_{{yd}} }};\ {aswFactorTex}\,A_{{s,req}}\right)
=\max\left(\frac{{{N(r.Fwd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}};\ {N(aswMin, "0")}\right)
={N(r.Asw, "0")}\ \mathrm{{mm^2}}
$$")

                .Stap_("moment-rand", "Moment rand", $@"Moment aan de rand :
$$
M_{{rand,Ed}}=F*a_c+H \cdot a_h
={N(r.FEd, "0.#")} \cdot {N(r.Ac/1000, "0.000")} + {N(r.HEd, "0.#")} \cdot {N(r.Ah/1000, "0.000")}
={N(r.Mrand, "0.0")}\ \mathrm{{kNm}}
$$

Moment tot aan hart kolom/wand:

$$
M_{{hart,Ed}}=M_{{rand}} +  \frac{{d_{{kolom}}}}{{2}} \cdot F_{{Ed}}
= {N(r.Mrand, "0.0")} + \frac{{{(N((i.KolomDikte/1000.0), "0.000"))}}}{{2}}  \cdot {r.FEd}
= {N(r.Mhart, "0.0")}\ \mathrm{{kNm}} 
$$


"


                )


                .Slot($@"# Samenvatting

| Grootheid | Resultaat |
| --- | --- |
| $x_1$ | {N(r.X1)} mm |
| $\Delta a$ | {N(deltaA)} mm |
| $a$ | {N(r.A)} mm |
| $z$ | {N(r.Z)} mm |
| $\theta$ | {N(r.ThetaDeg)}° |
| $F_{{t}}$ | {N(r.Ft)} kN |
| $F_{{c}}$ | {N(r.Fc)} kN |
| $F_{{c,h}}$ | {N(r.F1x)} kN |
| $F_{{c,v}}c$ | {N(r.F1y)} kN |
| $y_1$ | {N(r.Y1)} mm |
| $w_1$ | {N(w)} mm |
| $F_{{wd}}$ | {N(r.Fwd)} kN |
| $A_{{s,req}}$ | {N(r.AsMain, "0")} mm² |
| $A_{{sw,req}}$ | {N(r.Asw, "0")} mm² |");
        }
    }
}
