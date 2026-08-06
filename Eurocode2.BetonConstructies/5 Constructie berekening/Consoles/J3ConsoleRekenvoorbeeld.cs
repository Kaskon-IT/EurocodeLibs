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
            i.RekenMethode switch
            {
                J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie => MaakBuilderGedrongenLigger(i, r).Build(),
                _ => MaakBuilder(i, r).Build(),
            };

        /// <summary>
        /// Rekenvoorbeeld voor de gedrongen-liggertheorie (6.1 (10)).
        /// Opzet conform stappenplan; controles 3.2 t/m 3.4 volgen nog.
        /// </summary>
        public static RekenvoorbeeldBuilder MaakBuilderGedrongenLigger(J3ConsoleInput i, J3ConsoleResult r)
        {
            var builder = new RekenvoorbeeldBuilder("Rekenvoorbeeld console (gedrongen-liggertheorie 6.1 (10))")
                .Intro($@"
| Invoer | Waarde | |
|---|---|---:|
| *F*~Ed~  | {N(i.FEd, "0 kN")} | Verticale belasting |
| *H*~Ed~  | {N(i.HEd, "0 kN")} | Horizontale belasting |
| *f*~BGT~ | {N(i.FactorBgt, "0.00")} | Factor BGT/UGT |
| *a*~c~   | {N(i.Ac, "0 mm")} | Afstand belasting tot betonrand |
| *B*~c~   | {N(i.Bc, "0 mm")} | Breedte console |
| *H*~c~   | {N(i.Hc, "0 mm")} | Hoogte console |
| *c*      | {N(i.Dekking, "0 mm")} | Dekking |
| *f*~ck~  | {N(i.Fck, "0 N/mm²")} | Druksterkte beton |
| *f*~yk~  | {N(i.Fyk, "0 N/mm²")} | Vloeigrens betonstaal |

")

                .Stap_("nutHo", "Nuttige hoogte", $@"
$$
d = H_c - d_1 = {N(i.Hc)} - {N(r.D1, "0")} = {N(r.D, "0")} \text{{ mm}}
$$
")

                .Stap_("z", "Hefboomsarm gedrongen constructie 6.1 (10)", $@"
De inwendige hefboomsarm *z* volgt uit de formule voor gedrongen constructies (NEN-EN 1992-1-1 6.1 (10)):

$$
{r.GedrongenUitkraging.ZFormula.FullValue}
$$

Voor consoles is er een begrenzing gegeven aan de hellingshoek van de drukdiagonaal (EC2 J.3):
$$
1.0 \leq \tan\theta \leq 2.5
$$

waarbij 
$$
\tan\theta = \frac{{z}}{{a}} = \frac{{{N(r.Z,"0")}}}{{{N(r.A,"0")}}} = {N(r.Z / r.A, "0.0")}
$$

")

                .Stap_("med", "Buigend moment", $@"
$$
M_{{Ed}} = a \cdot F_{{Ed}} + (z + H_c - d) \cdot H_{{Ed}}
= {N(r.A, "0")} \cdot {N(r.FEd*1000, "0")} + ({N(r.Z, "0")} + {N(i.Hc, "0")} - {N(r.D, "0")}) \cdot {N(r.HEd*1000, "0")}
= {N(r.MEd, "0")} \text{{ Nmm}}
$$
")

                .Stap_("wapening", "Benodigde hoofdwapening", $@"
$$
A_{{s,req}} = \frac{{M_{{Ed}}}}{{f_{{yd}} \cdot z}}
= \frac{{{N(r.MEd, "0")}}}{{{N(r.Fyd, "0")} \cdot {N(r.Z, "0")}}}
= {N(r.AsMain, "0")} \text{{ mm²}}
$$

$$
A_{{s,prov}} = {r.AantalMain} \phi {r.DiameterMain}{(r.AantalMain2 > 0 ? $@" + {r.AantalMain2} \phi {r.DiameterMain2}" : "")} = {N(r.AsMainProv, "0")} \text{{ mm²}}
$$

{(r.AsMainProv >= r.AsMain ? "✔️ Toegepaste wapening voldoet." : "⚠️ Toegepaste wapening onvoldoende.")}
");

            // 3.1 Minimale wapening EC2 7.3.2 (7.1)
            if (r.MinimumWapening is not null)
            {
                var mw = r.MinimumWapening;
                builder.Stap_("asmin", "Minimale wapening 7.3.2 (7.1)", $@"
Normaalkracht (trek) in BGT: $N = {N(i.HBgt, "0")}$ kN, dus
$\sigma_N = N/(b \cdot h) = {N(mw.SigmaN, "0.00")}$ N/mm².

Spanningen net voor scheurvorming (lineair verloop):
$$
\sigma_{{boven}} = f_{{ct,eff}} = {N(mw.SigmaBoven, "0.0")} \text{{ N/mm²}} \qquad
\sigma_{{onder}} = 2 \cdot \sigma_N - f_{{ct,eff}} = {N(mw.SigmaOnder, "0.0")} \text{{ N/mm²}}
$$

$$
{mw.HcrFormula.FullValue}
$$

$$
{mw.ActFormula.FullValue}
$$

$$
{mw.AsMinFormula.FullValue}
$$

{(r.AsMinOk ? "✔️ As,prov ≥ As,min." : "⚠️ As,prov < As,min.")}
");
            }

            // 3.2 Scheurwijdtetoetsing 7.3.4 (7.8)
            if (r.Scheurwijdte is not null)
            {
                var sw = r.Scheurwijdte;
                builder.Stap_("scheurwijdte", "Scheurwijdtetoetsing 7.3.4 (7.8)", $@"
BGT-belastingen: $F = {N(i.FBgt, "0")}$ kN en $H = {N(i.HBgt, "0")}$ kN
(factor {N(i.FactorBgt, "0.00")}), dus $M_{{BGT}} = {N(sw.MBgt, "0")}$ kNmm.

$$
{sw.SigmaSFormula.FullValue}
$$

$$
{sw.DiameterEqFormula.FullValue}
$$

$$
{sw.HcEffFormula.FullValue}
$$

met $\rho_{{p,eff}} = A_s / A_{{c,eff}} = {N(sw.AsProv, "0")} / {N(sw.AcEff, "0")} = {N(sw.RhoPEff, "0.0000")}$.

$$
{sw.SrMaxFormula.FullValue}
$$

$$
{sw.EpsSmMinusEpsCmFormula.FullValue}
$$

$$
{sw.WkFormula.FullValue}
$$

{(sw.IsVoldoende ? $"✔️ w~k~ = {N(sw.Wk, "0.00")} mm ≤ w~max~ = {N(sw.WMax, "0.00")} mm." : $"⚠️ w~k~ = {N(sw.Wk, "0.00")} mm > w~max~ = {N(sw.WMax, "0.00")} mm.")}
");
            }

            // 3.3 Controle dwarskracht 6.2.2
            builder.Stap_("dwarskracht", "Controle dwarskracht en wringing", $@"
De dwarskrachtweerstand van de console met dwarskrachtwapnening wordt getoetst op basis van de sterkte van de betondrukdiagonaal. Bij dwarskracht mag volgens EC2 (6.7N) deze hoek niet groter dan 45 graden zijn.
We stellen dus de hoek in op 45 graden en gebruiken ook de bijbehorende hoogte z. De maximale spanning in de beugels houdeen we op 80% van de f~yk~.
$$
\theta = 45^\circ
$$

$$
z = a = {N(r.A,"0")} \text{{ mm}}
$$

$$
f_{{ywd}}= 0.8 \cdot f_{{yk}} = 0.8 \cdot {i.Fyk:0} = {r.Fywd:0} \text{{ N/mm²}}
$$


{(r.Dwarskracht is null ? "" : $@"
Eerst de dwarskrachtweerstand zonder dwarskrachtwapening volgens 6.2.2 vgl. (6.2a),
met $N_{{Ed}} = -H_{{Ed}} = {N(r.Dwarskracht.NEd, "0")}$ kN (trek):

$$
{r.Dwarskracht.CRdcFormula.FullValue}
$$

$$
{r.Dwarskracht.KFormula.FullValue}
$$

$$
{r.Dwarskracht.RhoLFormula.FullValue}
$$

$$
k_1 = {N(r.Dwarskracht.K1, "0.00")}
$$

$$
{r.Dwarskracht.SigmaCpFormula.FullValue}
$$

$$
{r.Dwarskracht.VRdcFormula.FullValue}
$$

Reductie van de belastingsbijdrage bij een last dicht bij de oplegging, 6.2.2 (6):

$$
{r.Dwarskracht.BetaFormula.FullValue}
$$

$$
{r.Dwarskracht.VEdRedFormula.FullValue}
$$

{(r.Dwarskracht.IsVoldoende
    ? $"✔️ V~Ed,red~ = {N(r.Dwarskracht.VEdRed, "0")} kN ≤ V~Rd,c~ = {N(r.Dwarskracht.VRdc, "0")} kN, geen dwarskrachtwapening vereist."
    : $"⚠️ V~Ed,red~ = {N(r.Dwarskracht.VEdRed, "0")} kN > V~Rd,c~ = {N(r.Dwarskracht.VRdc, "0")} kN, dwarskrachtwapening vereist.")}

Controle drukdiagonaal (θ = 45°), met V~Ed~ zonder reductie:

$$
{r.Dwarskracht.VRdMaxFormula.FullValue}
$$

{(r.Dwarskracht.VRdMaxOk
    ? $"✔️ V~Ed~ = {N(r.Dwarskracht.VEd, "0")} kN ≤ V~Rd,max~ = {N(r.Dwarskracht.VRdMax, "0")} kN."
    : $"⚠️ V~Ed~ = {N(r.Dwarskracht.VEd, "0")} kN > V~Rd,max~ = {N(r.Dwarskracht.VRdMax, "0")} kN, drukdiagonaal niet toereikend!")}

Beugelwapening t.b.v. dwarskracht volgens vgl. (6.19), aan te brengen in het middelste ¾ deel van $a_v$:

$$
{r.Dwarskracht.AswVFormula.FullValue}
$$
")}

{(r.Torsie is null ? "" : r.Torsie.TEd <= 0 ? @"
### Wringing 6.3.2

Torsie niet van toepassing (geen excentriciteit van de belasting).
" : $@"
### Wringing 6.3.2

Wringmoment door excentriciteit $e = {N(r.Torsie.Excentriciteit, "0")}$ mm in de breedterichting,
$T_{{Ed}} = F_{{Ed}} \cdot e = {N(r.Torsie.TEd, "0.00")}$ kNm.

$$
{r.Torsie.TEfFormula.FullValue}
$$

{(r.Torsie.TEfMinMaatgevend ? "De ondergrens $2c + 2Ø_{bgl} + Ø_{langs}$ is maatgevend." : "")}

$$
{r.Torsie.AkFormula.FullValue}
$$

$$
{r.Torsie.TRdcFormula.FullValue}
$$

{(r.Torsie.IsVoldoende
    ? $"✔️ T~Ed~ = {N(r.Torsie.TEd, "0.00")} kNm ≤ T~Rd,c~ = {N(r.Torsie.TRdc, "0.00")} kNm."
    : $"⚠️ T~Ed~ = {N(r.Torsie.TEd, "0.00")} kNm > T~Rd,c~ = {N(r.Torsie.TRdc, "0.00")} kNm.")}

$$
{r.Torsie.TRdMaxFormula.FullValue}
$$

Beugelwapening t.b.v. wringing (per zijde):

$$
{r.Torsie.AswTFormula.FullValue}
$$
")}

{(r.TorsieDwarskrachtCombinatie is null || r.Torsie is null || r.Torsie.TEd <= 0 ? "" : $@"
### Combinatie dwarskracht en wringing 6.3.2 (5)

Controle drukdiagonaal, met V~Ed~ zonder reductie:

$$
{r.TorsieDwarskrachtCombinatie.UnityCheckMaxFormula.FullValue}
$$

{(r.TorsieDwarskrachtCombinatie.IsDrukdiagonaalVoldoende
    ? $"✔️ Unity check (6.29) = {N(r.TorsieDwarskrachtCombinatie.UnityCheckMax, "0.00")} ≤ 1.0, drukdiagonaal toereikend."
    : $"⚠️ Unity check (6.29) = {N(r.TorsieDwarskrachtCombinatie.UnityCheckMax, "0.00")} > 1.0, drukdiagonaal niet toereikend!")}

Totale beugelwapening (dwarskracht + wringing) in zone $a_v = {N(r.TorsieDwarskrachtCombinatie.AvZone, "0")}$ mm:

Bij 2-snedige beugels:

Bij 3-snedige beugels:

Bij 4-snedige beugels:

Bij n-snedige beugels (n>=2):
Buitenste benen (links/recht):
Asw,req = {r.TorsieDwarskrachtCombinatie.AswT:0} + {r.TorsieDwarskrachtCombinatie.AswV:0} / n = 
Binnenste benen:
Asw,req = {r.TorsieDwarskrachtCombinatie.AswV:0} / n =




$$
{r.TorsieDwarskrachtCombinatie.AswTZoneFormula.FullValue}
$$

$$
{r.TorsieDwarskrachtCombinatie.AswTotaalFormula.FullValue}
$$

$$
{r.TorsieDwarskrachtCombinatie.UnityCheckFormula.FullValue}
$$

{(r.TorsieDwarskrachtCombinatie.IsVoldoende
    ? $"✔️ Unity check = {N(r.TorsieDwarskrachtCombinatie.UnityCheck, "0.00")} ≤ 1.0, geen aanvullende wapening voor dwarskracht + wringing vereist."
    : $"⚠️ Unity check = {N(r.TorsieDwarskrachtCombinatie.UnityCheck, "0.00")} > 1.0, aanvullende wapening voor dwarskracht + wringing vereist.")}
")}
")

                .Stap_("wap-oplegging", "Wapening t.p.v. oplegging", $@"
Onder de oplegging moet de wapening in de dwarsrichting van de console gecontroleerd worden:
todo: Dit alleen als flexibel oplegmateriaal, anders conform art 6.5
$$
A_s = 0.25 \cdot (t/h) F_{{Ed}} / f_{{yd}} = 0.25 \cdot ({N(i.DikteOplegmateriaal, "0")} / {N(r.LoadPlateWidth, "0")}) \cdot ({N(r.FEd * 1000.0, "0")} / {N(r.Fyd, "0")}) = {N(r.AsOplegging, "0")} \text{{ mm²}}
$$

$$
A_{{s,oplegging}} 
= 0.25 \cdot \frac{{t}}{{h}} \cdot \frac{{F_{{Ed}}}}{{f_{{yd}}}} 
= \frac{{{N(r.FEd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}} = {N(r.AsOplegging, "0")} \text{{ mm²}}
$$
")
                .Stap_("wap-lnk", "Wapening lnk", $@"
$$
{r.AsLnkFormula.FullValue}
$$
")

                .Stap_("controle", "Controle wapening", $@"
*Nog uit te werken:* verankering, buigdoorndiameter en detailleringseisen.
")

                .Slot(@"
## Samenvatting


");

            return builder;
        }


        /// <summary>
        /// Exposeert de builder zodat aanroepers stappen kunnen overslaan of
        /// aanpassen vóór <see cref="RekenvoorbeeldBuilder.Build"/>.
        /// </summary>
        public static RekenvoorbeeldBuilder MaakBuilder(J3ConsoleInput i, J3ConsoleResult r)
        {
            double sigmaN = r.Sigma1RdMax;
            double sigmaCCT = r.Sigma2RdMax;
            double sigmaNode1VerticalPlane = r.F1x * 1000.0 / (r.Y1 * r.Bc);
            double deltaA = i.FactorHEd * (i.Hc - r.D);
            double w = r.Fc * 1000.0 / (i.Bc * sigmaN);
            string aswMinTex = r.LinkType == J3ConsoleLinkType.Verticaal ?
                @"0.5 \cdot \frac{{ F_{{Ed}} }}{{ f_{{yd}} }}" :
                @"0.25 \cdot A_{s,main}";
            //string aswFactorTex = r.LinkType == J3ConsoleLinkType.Verticaal ? "0.5" : "0.25";
            double aswMin = (r.LinkType == J3ConsoleLinkType.Verticaal ? 
                0.5 * r.FEd * 1000 / r.Fyd : 
                0.25 * r.AsMain);

            return new RekenvoorbeeldBuilder("Rekenvoorbeeld console (Strut-and-Tie)")
                .Intro($@"
### Schema
<div style=""max-width:480px"">
{r.SchemaSvg}
</div>
Schematisering met CCC-knoop (1) in de kolom aan de onderzijde van de console en een CCT-knoop (2) 
boven in de console ter hoogte van de trekband.

| Invoer | Waarde | |
|---|---|---:|
| *F*~Ed~  | {N(i.FEd, "0 kN")} | Verticale belasting |
| *H*~Ed~  | {N(i.HEd, "0 kN")} | Horizontale belasting |
| *a*~c~   | {N(i.Ac, "0 mm")} | Afstand belasting tot betonrand |
| *B*~c~   | {N(i.Bc, "0 mm")} | Breedte console |
| *H*~c~   | {N(i.Hc, "0 mm")} | Hoogte console |
| *c*      | {N(i.Dekking, "0 mm")} | Dekking |
| *Ø*~main~| {N(i.HoofdstaafDiameter, "0 mm")} | Diameter trekbandwapening |
| *f*~ck~  | {N(i.Fck, "0 N/mm²")} | Druksterkte beton |
| *f*~yk~  | {N(i.Fyk, "0 N/mm²")} | Trekstrekte betonstaal |

")

                .Stap_("nutHo", "Nuttige hoogte", $@"
$$
d = H_c - d_1 = {N(i.Hc)} - {N(r.D1, "0")} = {N(r.D, "0")} \text{{ mm}}
$$
$$
d_1 = c + \frac{{Ø_{{main}}}}{{2}} {(r.HorizontaleBeugelsNodig ? "" : "+ Ø_{{bgl}}")} = {r.D1:0} \text{{ mm}}
$$
")

                .Stap_("x1", "Afmeting knoopvlak", $@"
De bekende verticale belasting in knoop 1 ($F_{{1,v}}=F_{{Ed}}$) bepaalt direct de horizontale afmeting van dit knoopvlak:

$$
x_1=\frac{{F_{{1,v}}}}{{b\,\sigma_n}}
=\frac{{{N(r.FEd * 1000.0, "0")}}}{{{N(i.Bc, "0")}\cdot{N(sigmaN, "0.00")}}}
={N(r.X1)}\ \mathrm{{mm}}
$$

$$
\sigma_n = \sigma_{{Rd,max}} = k_1  \nu' f_{{cd}} = {N(r.Sigma1RdMax)} \text{{ N/mm²}} 
$$


")

                .Stap_("arm", "Horizontale arm ↔", $@"
De totale horizontale arm:
$$
a=a_c+\frac{{x_1}}{{2}}+\Delta a
={N(i.Ac, "0")}+\frac{{{N(r.X1)}}}{{2}}+{N(deltaA)}
={N(r.A)}\ \text{{ mm}}
$$

met vergroting uit horizontale belasting:
$$
\Delta a=\frac{{F_{{h,Ed}}}}{{F_{{v,Ed}}}} \cdot d_1
={N(i.FactorHEd, "0.00")}\cdot{N(r.D1, "0")}
={N(deltaA)}\ \text{{ mm}}
$$



")

                


                .Stap_("z", "Hefboomarm ↕", $@"

Voor de hefboomsarm houden we een ondergrens aan van:

$$
z \leq {N(i.FactorZ, "0.###")}\cdot d
\leq {N(i.FactorZ, "0.###")}\cdot{N(r.D, "0")}
\leq {N(i.FactorZ * r.D)} \text{{ mm}}
$$

Tevens berekenen we *z* (situatie met maximale spanning in vertikaal knoopvlak) zodat we zeker weten dat toelaatbare spanning niet wordt overschreden:

$$
z =\frac{{d+\sqrt{{d^{{2}}-\frac{{2F_{{Ed}}\,a}}{{b\,\sigma_{{Rd}}}}}}}}{{2}} = {N(r.ZBer)} \text{{ mm}}
$$

We houden de kleinste waarde aan:
$$
z = \min({N(i.FactorZ * r.D)};{N(r.ZBer)}) = {N(r.Z)} \text{{ mm}}
$$
")
                 .Stap_("y1", "Afmeting knoopvlak", $@"

$$ 
y_1 = 2 \cdot (d-z) = {N(r.Y1)} \text{{ mm}}
$$

$$
\sigma_{{c}} = \frac{{F_{{}} }}{{y_1\cdot b}} = {N(sigmaNode1VerticalPlane, "0.00")} \text{{ N/mm²}}
$$


")




                .Stap_("ft",
                "Trekbandkracht",
                $@"Momentenevenwicht:

$$
F_{{t}} = F_2 = H+\frac{{F_{{v,Ed}}\,a}}{{z}}
={N(r.HEd)}+ \frac{{{N(r.FEd, "0")}\cdot{N(r.A)}}}{{{N(r.Z)}}}
={N(r.Ft)}\ \mathrm{{kN}}
$$


")

                .Stap_("fc", "Drukstaaf", $@"Uit krachtenevenwicht:

$$
F_{{1,h}}=\frac{{F_{{v,Ed}}\,a}}{{z}}
={N(r.F1x)}\ \mathrm{{kN}}
$$

$$
F_c = F_1 =\sqrt{{F_{{1,h}}^2+F_{{1,v}}^2}}
=\sqrt{{{N(r.F1x, "0")}^2+{N(r.F1y)}^2}}
={N(r.Fc)}\ \mathrm{{kN}}
$$")

                .Stap_("theta", "Hoek drukstaaf", $@"$$
\theta=\arctan\left(\frac{{z}}{{a}}\right)
=\arctan\left(\frac{{{N(r.Z)}}}{{{N(r.A)}}}\right)
={N(r.ThetaDeg)}^\circ
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
A_{{s,main}}=\frac{{F_{{t,Ed}}}}{{f_{{yd}}}}
=\frac{{{N(r.Ft * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}}
={N(r.AsMain, "0")} \text{{ mm²}}
$$



Aanvullende {(r.VerticaleBeugelsNodig ?
"vertikale" : "horizontale")} beugels met 
$\Sigma A_{{s,lnk}} \geq {aswMinTex}$

$$
\Sigma A_{{s,lnk,req}}
=\max\left(\frac{{ F_{{wd}} }}{{ f_{{yd}} }};\ {aswMinTex} \right)
=\max\left(\frac{{{N(r.Fwd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}};\ {N(aswMin, "0")}\right)
={N(r.Asw, "0")} \text{{ mm²}}
$$")



                .Stap_("wapening-trekband", "Wapening trekband", $@"
$$
{r.AsMainFormula.FullValue}
$$

$$
A_{{s,main,prov}} = {r.AantalMain} \phi {r.DiameterMain} = {N(r.AsMainProv, "0")} \text{{ mm²}}
$$

Staalspanning:
$$
f_y 
= \frac{{F_t}}{{A_{{s,prov}}}} 
= \frac{{{r.Ft * 1000:0}}}{{{r.AsMainProv:0}}}
= {r.MainFy:0} \text{{ N/mm²}}
$$

{(r.MainFy > r.Fyd ? "⚠️ Overschrijding trekspanning." : "")}  <br />

Verankeringslengte :
$$
{r.VerankeringMain.VerankeringslengteFormula.FullValue}
$$

$$
{r.VerankeringMain.BasisVerankeringslengteFormula.FullValue}
$$

$$
{r.VerankeringMain.FbdFormula.FullValue}
$$





Minimale buigdoorn diameter (zonder recht staafdeel):
$$
{r.VerankeringContext.MinimaleBuigdoornDiameterFormula.FullValue}
$$

$$
F_{{bt(0)}} = f_y \cdot A_{{s}} 
= {N(r.MainFy)} \cdot {N(WapeningHelper.GetDsnOpp(1, r.DiameterMain))}
= {N(r.MainFbt(0), "0")} \text{{ N}} 
$$ 


Toegepast is buigdoorndiameter van {r.BuigdoorMain:0} mm. <br/> 
Aan de **kolomzijde** begint de verankering op de vertikale lijn die door knoop 1 gaat en is er 
een rechte lengte van {r.RechtDeelMain:0} mm aanwezig tot aan de ombuiging.
<br />
Aan de **consolezijde** begint de verankering aan het begin van de oplegplaat en is er
een rechte lengte van {r.RechtDeelMain2:0} mm aanwezig tot aan de ombuiging.




Het kleinst rechte staafdeel tot aan de ombuiging is {N(Math.Min(r.RechtDeelMain, r.RechtDeelMain2), "0")} mm en het
resterende deel is:
$$
\eta = 1 - (\frac{{{N(Math.Min(r.RechtDeelMain, r.RechtDeelMain2), "0")}}}{{{r.VerankeringMain.Verankeringslengte:0}}}) = {N(r.ResterendPercentageVerankeringsLengte(Math.Min(r.RechtDeelMain, r.RechtDeelMain2)), "0.00")}
$$

$$
F_{{bt({N(Math.Min(r.RechtDeelMain, r.RechtDeelMain2), "0")})}} = \eta \cdot F_{{bt(0)}} = {N(r.MainFbt(Math.Min(r.RechtDeelMain, r.RechtDeelMain2) ), "0")} \text{{ N}}
$$



$$
\phi_{{m,min}} \geq {Math.Min(r.VerankeringMain.MinimaleBuigdoornDiameter, r.VerankeringMainConsoleZijde.MinimaleBuigdoornDiameter):0} \text{{ mm}}
$$

Toegepast:
$$
\phi_{{m}} = {N(r.BuigdoorMain, "0")} \text{{ mm}} 
$$
$$
r_{{i}} = {N((r.BuigdoorMain + r.DiameterMain) * 0.5, "0")} \text{{ mm}} 
$$

{(r.BuigdoornMainReq(r.RechtDeelMain) > r.BuigdoorMain ?
"⚠️ Let op, ombuiging niet akkoord. Zorg voor alternatieve verankering zonder omgebogen staven"
: "")}

"
)

                .Stap_("beugels", "Beugels", $@"
$$
\Sigma A_{{s,lnk}}
= \frac{{ F_{{wd}} }}{{ f_{{yd}} }}
= \frac{{{N(r.Fwd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}}
= {N(r.Asw, "0")} \text{{ mm²}}
$$

$$
{r.AsLnkFormula.FullValue}
$$

$$
\Sigma A_{{s,lnk, prov}} 
= {(r.AantalBeugels)} \text{{bg}} \phi{r.DiameterBgl} = {r.AswProv:0} \text{{ mm²}}
$$

")

                .Stap_("momenten", "Momenten extern", $@"
Moment aan de rand:
$$
M_{{rand,Ed}}=F \cdot a_c + H \cdot a_h
={N(r.FEd, "0.#")} \cdot {N(r.Ac / 1000, "0.000")} + {N(r.HEd, "0.#")} \cdot {N(r.Ah / 1000, "0.000")}
={N(r.Mrand, "0.0")}\ \mathrm{{kNm}}
$$
Moment tot aan hart kolom/wand:
$$
M_{{hart,Ed}}=M_{{rand}} +  \frac{{d_{{kolom}}}}{{2}} \cdot F_{{Ed}}
= {N(r.Mrand, "0.0")} + \frac{{{(N((i.KolomDikte / 1000.0), "0.000"))}}}{{2}}  \cdot {r.FEd}
= {N(r.Mhart, "0.0")}\ \mathrm{{kNm}} 
$$
")
                .Slot(@"
## Samenvatting


")
                
                
                
                ; 
        }
    }
}
