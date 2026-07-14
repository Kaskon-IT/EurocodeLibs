using System.Globalization;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Genereert een markdown-rekenvoorbeeld (met LaTeX-formules) voor de J3-console
    /// volgens het strut-and-tie model. De markdown kan met Markdig + KaTeX op het
    /// scherm of in een rapport worden getoond.
    /// </summary>
    public static class J3ConsoleRekenvoorbeeld
    {
        private static string N(double v, string format = "0.0") =>
            v.ToString(format, CultureInfo.InvariantCulture);

        public static string Genereer(J3ConsoleInput i, J3ConsoleResult r)
        {
            double sigmaN = r.Sigma1RdMax;
            double deltaA = i.FactorHEd * (i.Hc - r.D);
            double w = r.Fc * 1000.0 / (i.Bc * sigmaN);

            return $@"# Rekenvoorbeeld console (Strut-and-Tie)

## Uitgangspunten

| Grootheid | Waarde |
| --- | --- |
| Verticale belasting | $F_{{v,Ed}}={N(r.FvEd, "0")}\ \mathrm{{kN}}$ |
| Horizontale belasting | $F_{{h,Ed}}={N(r.FhEd, "0")}\ \mathrm{{kN}}$ |
| Breedte console | $b={N(i.Bc, "0")}\ \mathrm{{mm}}$ |
| Afstand belasting tot kolom | $a_c={N(i.Ac, "0")}\ \mathrm{{mm}}$ |
| Effectieve hoogte | $d={N(r.D)}\ \mathrm{{mm}}$ |
| Hefboomarm trekband | $z={N(r.Z)}\ \mathrm{{mm}}$ |
| Toelaatbare CCC-spanning | $\sigma_n=\sigma_{{Rd,\max}}={N(sigmaN, "0.00")}\ \mathrm{{N/mm^2}}$ |

---

## Stap 1 -- Verticaal knoopvlak

De bekende verticale belasting bepaalt direct de hoogte van het eerste knoopvlak:

$$
x_1=\frac{{F_{{v,Ed}}}}{{b\,\sigma_n}}
=\frac{{{N(r.FvEd * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(r.X1)}\ \mathrm{{mm}}
$$

---

## Stap 2 -- Correctie voor horizontale belasting

Een horizontale belasting op de bovenzijde van de console veroorzaakt een extra moment.
De vergroting van de arm is:

$$
\Delta a=\frac{{F_{{h,Ed}}}}{{F_{{v,Ed}}}}\,(h_c-d)
={N(i.FactorHEd, "0.00")}\cdot{N(i.Hc - r.D)}
={N(deltaA)}\ \mathrm{{mm}}
$$

---

## Stap 3 -- Horizontale arm

Het knooppunt ligt in het midden van het knoopvlak:

$$
a=a_c+\frac{{x_1}}{{2}}+\Delta a
={N(i.Ac, "0")}+\frac{{{N(r.X1)}}}{{2}}+{N(deltaA)}
={N(r.A)}\ \mathrm{{mm}}
$$

---

## Stap 4 -- Trekbandkracht

Momentenevenwicht (de horizontale belasting zit al in $a$ via $\Delta a$):

$$
F_{{t,Ed}}=\frac{{F_{{v,Ed}}\,a}}{{z}}
=\frac{{{N(r.FvEd, "0")}\cdot{N(r.A)}}}{{{N(r.Z)}}}
={N(r.Ft)}\ \mathrm{{kN}}
$$

---

## Stap 5 -- Drukstaaf

Uit krachtenevenwicht:

$$
F_c=\sqrt{{F_{{v,Ed}}^2+F_{{1x}}^2}}
=\sqrt{{{N(r.FvEd, "0")}^2+{N(r.F1x)}^2}}
={N(r.Fc)}\ \mathrm{{kN}}
$$

---

## Stap 6 -- Hoek drukstaaf

$$
\theta=\arctan\left(\frac{{z}}{{a}}\right)
=\arctan\left(\frac{{{N(r.Z)}}}{{{N(r.A)}}}\right)
={N(r.ThetaDeg)}^\circ
$$

---

## Stap 7 -- Horizontaal knoopvlak

$$
y_1=\frac{{F_{{t,Ed}}}}{{b\,\sigma_n}}
=\frac{{{N(r.Ft * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(r.Y1)}\ \mathrm{{mm}}
$$

---

## Stap 8 -- Breedte loodrecht op de drukstaaf

$$
w=\frac{{F_c}}{{b\,\sigma_n}}
=\frac{{{N(r.Fc * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(w)}\ \mathrm{{mm}}
$$

---

## Stap 9 -- Kracht in aanvullende beugels

De drukdiagonaal spreidt tussen knoop 1 en knoop 2; de aanvullende beugels
nemen de spreidkracht $F_{{wd}}$ op (vgl. NEN-EN 1992-1-1 J.3):

$$
F_{{wd}}=\frac{{\dfrac{{2z}}{{a}}-1}}{{3+\dfrac{{F_{{v,Ed}}}}{{F_{{1x}}}}}}\,F_{{1x}}
=\frac{{\dfrac{{2\cdot{N(r.Z)}}}{{{N(r.A)}}}-1}}{{3+\dfrac{{{N(r.FvEd, "0")}}}{{{N(r.F1x)}}}}}\cdot{N(r.F1x)}
={N(r.Fwd)}\ \mathrm{{kN}}
$$

---

## Stap 10 -- Benodigde wapening

Hoofdtrekwapening uit de trekbandkracht:

$$
A_{{s,req}}=\frac{{F_{{t,Ed}}}}{{f_{{yd}}}}
=\frac{{{N(r.Ft * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}}
={N(r.AsMain, "0")}\ \mathrm{{mm^2}}
$$

Aanvullende beugels uit $F_{{wd}}$, met als minimum {(r.LinkType == J3ConsoleLinkType.Verticaal ? "$0{,}5$" : "$0{,}25$")}$\,A_{{s,req}}$:

$$
A_{{sw,req}}=\max\left(\frac{{F_{{wd}}}}{{f_{{yd}}}};\ {(r.LinkType == J3ConsoleLinkType.Verticaal ? "0{,}5" : "0{,}25")}\,A_{{s,req}}\right)
=\max\left(\frac{{{N(r.Fwd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}};\ {N((r.LinkType == J3ConsoleLinkType.Verticaal ? 0.5 : 0.25) * r.AsMain, "0")}\right)
={N(r.Asw, "0")}\ \mathrm{{mm^2}}
$$

---

# Samenvatting

| Grootheid | Resultaat |
| --- | --- |
| $x_1$ | {N(r.X1)} mm |
| $\Delta a$ | {N(deltaA)} mm |
| $a$ | {N(r.A)} mm |
| $F_{{t,Ed}}$ | {N(r.Ft)} kN |
| $F_c$ | {N(r.Fc)} kN |
| $\theta$ | {N(r.ThetaDeg)}° |
| $y_1$ | {N(r.Y1)} mm |
| $w_1$ | {N(w)} mm |
| $F_{{wd}}$ | {N(r.Fwd)} kN |
| $A_{{s,req}}$ | {N(r.AsMain, "0")} mm² |
| $A_{{sw,req}}$ | {N(r.Asw, "0")} mm² |
";
        }
    }
}
