using System.Globalization;
using System.Text;
using CommonLibrary.Models;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Rapportage van de J3-console: één opzet voor beide rekenmethoden
    /// (gedrongen-liggertheorie 6.1 (10) en strut-and-tie bijlage J.3).
    /// <list type="bullet">
    /// <item>Uitgebreid: markdown met uitgeschreven formules (Markdig + KaTeX).</item>
    /// <item>Compact: markdown-tabel uit <see cref="J3ConsoleResult.ResultRows"/>.</item>
    /// </list>
    /// Gebouwd met de generieke <see cref="RekenvoorbeeldBuilder"/>: stapnummers lopen
    /// automatisch op en stappen kunnen per sleutel worden overgeslagen of vervangen,
    /// bijv. <c>MaakBuilder(i, r).Skip("momenten").Build()</c>.
    /// </summary>
    public static class J3ConsoleRekenvoorbeeld
    {
        private static string N(double v, string format = "0.0") =>
            v.ToString(format, CultureInfo.InvariantCulture);

        /// <summary>
        /// Voegt de stap alleen toe als aan de voorwaarde is voldaan; de inhoud
        /// wordt lazy opgebouwd zodat null-waarden geen exceptie geven.
        /// </summary>
        private static RekenvoorbeeldBuilder Stap_Als(this RekenvoorbeeldBuilder b,
            bool voorwaarde, string key, string titel, Action<StapSchrijver> inhoud)
            => voorwaarde ? b.Stap(key, titel, inhoud) : b;

        /// <summary> Rapportage volgens de keuze in <see cref="J3ConsoleInput.Rapportage"/>. </summary>
        public static string Genereer(J3ConsoleInput i, J3ConsoleResult r) =>
            i.Rapportage == J3ConsoleInput.RapportageOptie.Compact
                ? GenereerCompact(r)
                : MaakBuilder(i, r).Build();

        [Obsolete("Gebruik MaakBuilder; die behandelt beide rekenmethoden.")]
        public static RekenvoorbeeldBuilder MaakBuilderGedrongenLigger(J3ConsoleInput i, J3ConsoleResult r) => MaakBuilder(i, r);

        // ------------------------------------------------------------------ compact

        /// <summary> Compacte rapportage: de resultaatregels als markdown-tabel. </summary>
        public static string GenereerCompact(J3ConsoleResult r)
        {
            static string Cel(string? s) => (s ?? "").Replace("|", "\\|");
            static string Tex(string? s) => string.IsNullOrWhiteSpace(s) ? "" : $"${s}$";

            var sb = new StringBuilder();
            sb.AppendLine($"# Console {(IsGdl(r) ? "(gedrongen-liggertheorie 6.1 (10))" : "(strut-and-tie bijlage J.3)")}");
            sb.AppendLine();
            sb.AppendLine("| Omschrijving | Symbool | Waarde | Eenheid | Formule / eis | |");
            sb.AppendLine("|---|---|--:|---|---|:-:|");

            foreach (var rij in r.ResultRows)
            {
                bool isKop = string.IsNullOrEmpty(rij.Waarde) && string.IsNullOrEmpty(rij.SymboolTex);
                if (isKop)
                {
                    sb.AppendLine($"| **{Cel(rij.Toelichting)}** | | | | {Cel(rij.Artikel)} | |");
                    continue;
                }

                string ok = rij.IsOk switch { true => "✔️", false => "⚠️", _ => "" };
                string formule = Tex(rij.FormuleTex);
                if (!string.IsNullOrEmpty(rij.Artikel))
                    formule = string.IsNullOrEmpty(formule) ? rij.Artikel! : $"{formule} {rij.Artikel}";

                string verwijzing = r.RijVerwijzingen.TryGetValue(rij, out var key) && r.OpmerkingNummer(key) is int nr
                    ? $" <sup>({nr})</sup>"
                    : "";

                sb.AppendLine($"| {Cel(rij.Toelichting)}{verwijzing} | {Tex(rij.SymboolTex)} | {Cel(rij.Waarde)} | {Cel(rij.Eenheid)} | {Cel(formule)} | {ok} |");
            }

            AppendMeldingen(sb, r);
            return sb.ToString();
        }

        // ---------------------------------------------------------------- uitgebreid

        /// <summary>
        /// Uitgebreid rekenvoorbeeld. Exposeert de builder zodat aanroepers stappen kunnen
        /// overslaan of aanpassen vóór <see cref="RekenvoorbeeldBuilder.Build"/>.
        /// </summary>
        public static RekenvoorbeeldBuilder MaakBuilder(J3ConsoleInput i, J3ConsoleResult r)
        {
            bool gdl = IsGdl(r);
            var b = new RekenvoorbeeldBuilder(gdl
                ? "Rekenvoorbeeld console (gedrongen-liggertheorie 6.1 (10))"
                : "Rekenvoorbeeld console (strut-and-tie bijlage J.3)");

            b.Intro(Intro(i, r));

            // 1. Nuttige hoogte
            b.Stap("nuttige-hoogte", "Nuttige hoogte", s =>
            {
                if (r.DOpgegeven)
                    s.Regel($"De randafstand van de trekband is opgegeven: $d' = {N(r.D1, "0")}$ mm.");
                else
                    s.Regel("De randafstand van de trekband volgt uit dekking, beugel en hoofdwapening:")
                     .Formule($@"d' = c + \phi_{{bgl}} + \frac{{\phi_{{main}}}}{{2}} = {N(i.Dekking, "0")} + {N(i.BeugelDiameter, "0")} + \frac{{{N(i.HoofdstaafDiameter, "0")}}}{{2}} = {N(r.D1, "0")} \text{{ mm}}");
                s.Formule($@"d = h_c - d' = {N(i.Hc, "0")} - {N(r.D1, "0")} = {N(r.D, "0")} \text{{ mm}}");
            });

            // 2. Vakwerk en hefboomsarm
            if (gdl)
            {
                double ar = r.A - i.Ac;
                b.Stap("hefboomsarm", "Hefboomsarm gedrongen ligger 6.1 (10)", s => s
                    .Regel("De arm van de belasting tot het steunpunt van de drukdiagonaal:")
                    .Formule($@"a_r = \min\left(\frac{{a_b}}{{2}};\ \frac{{L_c}}{{4}};\ \frac{{h_c}}{{4}}\right) = \min\left({N(i.LoadPlateLength / 2, "0")};\ {N(i.Lc / 4, "0")};\ {N(i.Hc / 4, "0")}\right) = {N(ar, "0")} \text{{ mm}}")
                    .Formule($@"a = a_c + a_r = {N(i.Ac, "0")} + {N(ar, "0")} = {N(r.A, "0")} \text{{ mm}}")
                    .Regel("De inwendige hefboomsarm *z* volgt uit de formule voor gedrongen constructies (NEN-EN 1992-1-1 6.1 (10)):")
                    .Formule(r.GedrongenUitkraging.ZFormula.FullValue));
            }
            else
            {
                double deltaA = i.FactorHEd * r.D1;
                b.Stap("vakwerk", "Vakwerk: positie drukknoop", s => s
                    .Regel("Schematisering met een CCC-knoop (1) in de kolom aan de onderzijde van de console en een CCT-knoop (2) boven in de console ter hoogte van de trekband.")
                    .Formule($@"\nu = 0.6\left(1 - \frac{{f_{{ck}}}}{{250}}\right) = {N(r.Nu, "0.000")} \qquad \sigma_{{1Rd,max}} = \nu f_{{cd}} = {N(r.Sigma1RdMax, "0.00")} \text{{ N/mm²}}")
                    .Regel("De verticale belasting in knoop 1 bepaalt de horizontale afmeting van dit knoopvlak:")
                    .Formule($@"x_1 = \frac{{F_{{Ed}}}}{{b_c\,\sigma_{{1Rd,max}}}} = \frac{{{N(r.FEd * 1000.0, "0")}}}{{{N(i.Bc, "0")} \cdot {N(r.Sigma1RdMax, "0.00")}}} = {N(r.X1)} \text{{ mm}}")
                    .Regel("De drukknoop ligt op $x_1/2$ van de kolomrand; de horizontale belasting vergroot de arm met $\\Delta a$:")
                    .Formule($@"\Delta a = \frac{{H_{{Ed}}}}{{F_{{Ed}}}} \cdot d' = {N(i.FactorHEd, "0.00")} \cdot {N(r.D1, "0")} = {N(deltaA)} \text{{ mm}}")
                    .Formule($@"a = a_c + \frac{{x_1}}{{2}} + \Delta a = {N(i.Ac, "0")} + \frac{{{N(r.X1)}}}{{2}} + {N(deltaA)} = {N(r.A)} \text{{ mm}}"));

                b.Stap("hefboomsarm", "Hefboomsarm", s =>
                {
                    s.Regel("*z* volgt uit het evenwicht van knoop 1 bij de maximale spanning in het verticale knoopvlak ($z + y_1/2 = d$):")
                     .Formule($@"z = \frac{{d + \sqrt{{d^2 - \frac{{2F_{{Ed}}\,a}}{{b_c\,\sigma_{{1Rd,max}}}}}}}}{{2}} = {N(r.ZOnbegrensd)} \text{{ mm}}");
                    if (r.ZBerZonderOplossing)
                        s.Regel("⚠️ Er is geen reële oplossing (de wortel is negatief); $z = d$ aangehouden. Controleer knoop 1.");
                });
            }

            // 3. Drukdiagonaal
            b.Stap("theta", "Helling drukdiagonaal", s =>
            {
                s.Regel("Voor consoles geldt een begrenzing van de hellingshoek van de drukdiagonaal (EC2 J.3):")
                 .Formule(@"1.0 \leq \tan\theta \leq 2.5")
                 .Formule($@"\tan\theta = \frac{{z}}{{a}} = \frac{{{N(r.ZOnbegrensd, "0")}}}{{{N(r.A, "0")}}} = {N(r.ZOnbegrensd / r.A, "0.00")}");
                if (r.ZBegrensd)
                    s.Regel("$\\tan\\theta > 2.5$: de hefboomsarm wordt teruggezet zodat $\\tan\\theta = 2.5$:")
                     .Formule($@"z = 2.5 \cdot a = 2.5 \cdot {N(r.A, "0")} = {N(r.Z)} \text{{ mm}}");
                if (r.TanTheta >= 1.0)
                    s.Toets(true, $"$\\tan\\theta = {N(r.TanTheta, "0.00")}$ voldoet.", "");
                else if (gdl)
                    s.Regel($"$\\tan\\theta = {N(r.TanTheta, "0.00")} < 1.0$: vlakke drukdiagonaal. Bij de gedrongen-liggertheorie toegestaan; de wapening wordt ook op zuivere buiging getoetst.");
                else
                    s.Toets(false, "",
                        $"$\\tan\\theta = {N(r.TanTheta, "0.00")} < 1.0$: **bijlage J.3 (strut-and-tie) is niet van toepassing.** Reken de console als gedrongen ligger (6.1 (10)).");
                s.Formule($@"\theta = \arctan\left(\frac{{z}}{{a}}\right) = {N(r.ThetaDeg)}^\circ")
                 .Formule($@"z_0 = z \cdot \frac{{a_c + \Delta a}}{{a}} = {N(r.Z0)} \text{{ mm}}")
                 .Toets(r.IsZ0Ok, "$a_c < z_0$.", "$a_c \\geq z_0$.");
            });

            // 4. Trekband
            b.Stap("trekband", "Trekband", s =>
            {
                if (gdl)
                    s.Formule($@"M_{{Ed}} = a \cdot F_{{Ed}} + (z + d') \cdot H_{{Ed}} = {N(r.A, "0")} \cdot {N(r.FEd * 1000, "0")} + ({N(r.Z, "0")} + {N(r.D1, "0")}) \cdot {N(r.HEd * 1000, "0")} = {N(r.MEd / 1e6, "0.0")} \text{{ kNm}}");
                else
                    s.Regel("Momentenevenwicht om knoop 1:")
                     .Formule($@"F_t = F_{{Ed}} \frac{{a}}{{z}} + H_{{Ed}} = {N(r.FEd, "0")} \cdot \frac{{{N(r.A)}}}{{{N(r.Z)}}} + {N(r.HEd, "0")} = {N(r.Ft)} \text{{ kN}}");
                s.Formule(r.AsMainFormula.FullValue);

                if (gdl)
                {
                    s.Regel("Bij gedrongen constructies kan buiging volgens 6.1 (1)P een lagere weerstand geven (6.1 (10) opmerking). " +
                            "Toets daarom ook zuivere buiging t.p.v. de kolomrand; de grootste wapening is maatgevend:")
                     .Formule($@"M_{{Ed,s}} = F_{{Ed}} \cdot a_c + H_{{Ed}} \cdot d' = {N(r.FEd * 1000, "0")} \cdot {N(r.Ac, "0")} + {N(r.HEd * 1000, "0")} \cdot {N(r.D1, "0")} = {N(r.MEdBuiging / 1e6, "0.0")} \text{{ kNm}}");

                    if (double.IsNaN(r.XuBuiging))
                        s.Toets(false, "", "Geen oplossing voor $x_u$: de betondrukzone is ontoereikend.");
                    else
                        s.Formule($@"x_u = \frac{{d - \sqrt{{d^2 - 4\beta M_{{Ed,s}}/(\alpha\, b_c f_{{cd}})}}}}{{2\beta}} = \frac{{{N(r.D, "0")} - \sqrt{{{N(r.D, "0")}^2 - 4 \cdot {N(r.BetaBuiging, "0.###")} \cdot {N(r.MEdBuiging, "0")}/({N(r.AlphaBuiging, "0.###")} \cdot {N(r.Bc, "0")} \cdot {N(r.Fcd, "0.0")})}}}}{{2 \cdot {N(r.BetaBuiging, "0.###")}}} = {N(r.XuBuiging)} \text{{ mm}}")
                         .Toets(r.XuBuigingOk,
                            $"$x_u = {N(r.XuBuiging, "0")} \\leq x_{{u,max}} = {N(r.XuMaxBuiging, "0")}$ mm.",
                            $"$x_u = {N(r.XuBuiging, "0")} > x_{{u,max}} = {N(r.XuMaxBuiging, "0")}$ mm: onvoldoende vervormingscapaciteit.")
                         .Formule($@"z = d - \beta x_u = {N(r.D, "0")} - {N(r.BetaBuiging, "0.###")} \cdot {N(r.XuBuiging)} = {N(r.ZBuiging)} \text{{ mm}}")
                         .Formule($@"A_{{s,buiging}} = \frac{{\alpha\, b_c\, x_u\, f_{{cd}}}}{{f_{{yd}}}} + \frac{{H_{{Ed}}}}{{f_{{yd}}}} = {N(r.AsBuiging, "0")} \text{{ mm}}^2")
                         .Formule($@"A_{{s,main,req}} = \max({N(r.AsGedrongen, "0")};\ {N(r.AsBuiging, "0")}) = {N(r.AsMain, "0")} \text{{ mm}}^2")
                         .Regel(r.BuigingMaatgevend
                            ? "⚠️ Zuivere buiging is maatgevend en aangehouden."
                            : "✔️ De gedrongen-liggertheorie is maatgevend.");
                }

                s.Formule(r.AsMainProvFormula.FullValue)
                 .Formule($@"\sigma_s = \frac{{A_{{s,main}}}}{{A_{{s,main,prov}}}} f_{{yd}} = {N(r.MainFy, "0")} \text{{ N/mm²}}")
                 .Toets(r.MainFy <= r.Fyd, "Hoofdwapening voldoet.", "Overschrijding trekspanning in de hoofdwapening.");
            });

            // 5. Drukdiagonaal (krachten)
            b.Stap("drukstaaf", "Drukdiagonaal", s => s
                .Formule($@"F_{{1x}} = F_{{Ed}} \frac{{a}}{{z}} = {N(r.F1x)} \text{{ kN}}")
                .Formule($@"F_c = \sqrt{{F_{{1x}}^2 + F_{{Ed}}^2}} = \sqrt{{{N(r.F1x, "0")}^2 + {N(r.F1y, "0")}^2}} = {N(r.Fc)} \text{{ kN}}"));

            // 6. Beugels volgens J.3
            b.Stap("beugels-j3", "Aanvullende beugels volgens J.3", s =>
            {
                switch (r.LinkType)
                {
                    case J3ConsoleLinkType.HorizontaalOfSchuin:
                        s.Regel($"$a_c = {N(i.Ac, "0")} \\leq 0.5 h_c = {N(0.5 * i.Hc, "0")}$ mm: gesloten horizontale of schuine beugels (J.3 (2)).")
                         .Formule($@"A_{{s,lnk,min}} = k_1 A_{{s,main}} = 0.25 \cdot {N(r.AsMain, "0")} = {N(r.AsLnkMin, "0")} \text{{ mm}}^2");
                        break;
                    case J3ConsoleLinkType.Verticaal:
                        s.Regel($"$a_c = {N(i.Ac, "0")} > 0.5 h_c = {N(0.5 * i.Hc, "0")}$ mm en $F_{{Ed}} = {N(r.FEd, "0")} > V_{{Rd,c}} = {N(r.Dwarskracht?.VRdc ?? 0, "0")}$ kN: gesloten verticale beugels (J.3 (3)).")
                         .Formule($@"A_{{s,lnk,min}} = k_2 \frac{{F_{{Ed}}}}{{f_{{yd}}}} = 0.5 \cdot \frac{{{N(r.FEd * 1000, "0")}}}{{{N(r.Fyd, "0")}}} = {N(r.AsLnkMin, "0")} \text{{ mm}}^2");
                        break;
                    default:
                        s.Regel($"$a_c > 0.5 h_c$ en $F_{{Ed}} = {N(r.FEd, "0")} \\leq V_{{Rd,c}} = {N(r.Dwarskracht?.VRdc ?? 0, "0")}$ kN: geen aanvullende beugels volgens J.3 (3) vereist.");
                        break;
                }
                s.Regel("De drukdiagonaal spreidt tussen knoop 1 en knoop 2; de beugels nemen de spreidkracht op:")
                 .Formule($@"F_{{wd}} = \frac{{2z/a - 1}}{{3 + F_{{Ed}}/F_{{1x}}}} F_{{1x}} = \frac{{2 \cdot {N(r.Z)}/{N(r.A)} - 1}}{{3 + {N(r.FEd, "0")}/{N(r.F1x)}}} \cdot {N(r.F1x)} = {N(r.Fwd)} \text{{ kN}}")
                 .Formule($@"\Sigma A_{{s,lnk,J.3}} = \max\left(\frac{{F_{{wd}}}}{{f_{{yd}}}};\ A_{{s,lnk,min}}\right) = \max\left({N(r.Fwd * 1000.0 / r.Fyd, "0")};\ {N(r.AsLnkMin, "0")}\right) = {N(r.AswJ3, "0")} \text{{ mm}}^2");
            });

            // 7. Dwarskracht
            var v = r.Dwarskracht;
            b.Stap_Als(r.ControleDwarskracht && v is not null, "dwarskracht", "Controle dwarskracht 6.2.2", s =>
            {
                s.RegelAls(gdl, "Bij de gedrongen-liggertheorie is de controle op dwarskracht verplicht.")
                 .RegelAls(!gdl && r.LinkType == J3ConsoleLinkType.Verticaal && !i.ControleDwarskracht,
                    "Er zijn verticale beugels nodig; de dwarskracht wordt daarom altijd beschouwd.");

                if (i.AfschuiningOnderzijde)
                    s.Regel("Er is een afschuining toegepast; de hoogte ter plaatse van $a_c$ is maatgevend:")
                     .Formule($@"h = \frac{{h_c}}{{2}} + \left(1 - \frac{{a_c}}{{L_c}}\right)\frac{{h_c}}{{2}} = {N(r.HoogteTpvAc, "0")} \text{{ mm}} \qquad d = h - d' = {N(r.NutHoogteTpvAc, "0")} \text{{ mm}}");

                s.Regel($"Dwarskrachtweerstand zonder dwarskrachtwapening (6.2a), met $N_{{Ed}} = -H_{{Ed}} = {N(v!.NEd, "0")}$ kN (trek):")
                 .Formule(v.CRdcFormula.FullValue)
                 .Formule(v.KFormula.FullValue)
                 .Formule(v.RhoLFormula.FullValue)
                 .Formule(v.SigmaCpFormula.FullValue)
                 .Formule(v.VRdcFormula.StaticValue)
                 .Formule(v.VRdcFormula.DynamicValue)
                 .Regel("Reductie van de belastingsbijdrage bij een last dicht bij de oplegging, 6.2.2 (6):")
                 .Formule(v.AvFormula.FullValue)
                 .Formule(v.BetaFormula.FullValue)
                 .Formule(v.VEdRedFormula.FullValue)
                 .Toets(v.IsVoldoende,
                    $"$V_{{Ed,red}} = {N(v.VEdRed, "0")} \\leq V_{{Rd,c}} = {N(v.VRdc, "0")}$ kN: geen dwarskrachtwapening vereist.",
                    $"$V_{{Ed,red}} = {N(v.VEdRed, "0")} > V_{{Rd,c}} = {N(v.VRdc, "0")}$ kN: dwarskrachtwapening vereist.")
                 .Regel("Controle drukdiagonaal met $\\theta = 45^\\circ$ en $z = a$, met $V_{Ed}$ zonder reductie:")
                 .Formule(v.VRdMaxFormula.FullValue)
                 .Toets(v.VRdMaxOk,
                    $"$V_{{Ed}} = {N(v.VEd, "0")} \\leq V_{{Rd,max}} = {N(v.VRdMax, "0")}$ kN.",
                    $"$V_{{Ed}} = {N(v.VEd, "0")} > V_{{Rd,max}} = {N(v.VRdMax, "0")}$ kN: drukdiagonaal niet toereikend!");

                if (v.WapeningNodig)
                {
                    s.Regel("Beugelwapening t.b.v. dwarskracht volgens (6.19), aan te brengen in het middelste ¾ deel van $a_v$:")
                     .Formule(v.FywdFormula.FullValue)
                     .Formule(v.AswVFormula.FullValue);

                    if (r.LinkType == J3ConsoleLinkType.Verticaal)
                        s.Regel("De verticale beugels volgens J.3 mogen niet minder zijn dan nodig voor de dwarskracht:")
                         .Formule($@"\Sigma A_{{s,lnk}} = \max(\Sigma A_{{s,lnk,J.3}};\ A_{{sw}}) = \max({N(r.AswJ3, "0")};\ {N(v.AswV, "0")}) = {N(r.Asw, "0")} \text{{ mm}}^2")
                         .RegelAls(r.DwarskrachtMaatgevend, "De dwarskracht is maatgevend voor de verticale beugels.");
                    else
                        s.Regel($"J.3 vraagt geen verticale beugels, maar de dwarskracht wel: $A_{{sw}} \\geq {N(v.AswV, "0")}$ mm² aan verticale beugels.");
                }
            });

            // 8. Toegepaste beugels
            b.Stap("beugels", "Beugelwapening", s =>
            {
                s.Formule($@"\Sigma A_{{s,lnk,req}} = {N(r.Asw, "0")} \text{{ mm}}^2")
                 .Formule($@"\Sigma A_{{s,lnk,prov}} = {r.AantalBeugels} \text{{bg}}\phi{N(r.DiameterBgl, "0")} \text{{ (2-snedig)}} = {N(r.AswProv, "0")} \text{{ mm}}^2")
                 .Toets(r.AswProvOk, "Beugelwapening voldoet.", "Beugelwapening onvoldoende.");
                if (r.VerticaleBeugelsVoorDwarskracht)
                    s.Formule($@"A_{{sw,verticaal}} \geq {N(r.AswVerticaalDwarskracht, "0")} \text{{ mm}}^2 \quad \text{{(dwarskracht, middelste ¾ van }} a_v)");
            });

            // 9. Wringing
            var t = r.Torsie;
            var vt = r.TorsieDwarskrachtCombinatie;
            b.Stap_Als(t is { TEd: > 0 }, "wringing", "Wringing 6.3.2", s =>
            {
                s.Regel($"Wringmoment door excentriciteit $e = {N(t!.Excentriciteit, "0")}$ mm in de breedterichting, $T_{{Ed}} = F_{{Ed}} \\cdot e = {N(t.TEd, "0.00")}$ kNm.")
                 .Formule(t.TEfFormula.FullValue)
                 .RegelAls(t.TEfMinMaatgevend, "De ondergrens $2c + 2Ø_{bgl} + Ø_{langs}$ is maatgevend.")
                 .Formule(t.AkFormula.FullValue)
                 .Formule(t.TRdcFormula.FullValue)
                 .Toets(t.IsVoldoende,
                    $"$T_{{Ed}} = {N(t.TEd, "0.00")} \\leq T_{{Rd,c}} = {N(t.TRdc, "0.00")}$ kNm.",
                    $"$T_{{Ed}} = {N(t.TEd, "0.00")} > T_{{Rd,c}} = {N(t.TRdc, "0.00")}$ kNm: aanvullende wapening vereist.")
                 .Formule(t.TRdMaxFormula.FullValue)
                 .Regel("Beugelwapening t.b.v. wringing (per zijde):")
                 .Formule(t.AswTFormula.FullValue);

                if (vt is not null)
                    s.Regel("Combinatie dwarskracht en wringing 6.3.2 (5), met $V_{Ed}$ zonder reductie:")
                     .Formule(vt.UnityCheckMaxFormula.FullValue)
                     .Toets(vt.IsDrukdiagonaalVoldoende,
                        $"Unity check (6.29) = {N(vt.UnityCheckMax, "0.00")} ≤ 1,0.",
                        $"Unity check (6.29) = {N(vt.UnityCheckMax, "0.00")} > 1,0: drukdiagonaal niet toereikend!")
                     .Formule(vt.AswTZoneFormula.FullValue)
                     .Formule(vt.AswTotaalFormula.FullValue)
                     .Formule(vt.UnityCheckFormula.FullValue)
                     .Toets(vt.IsVoldoende,
                        $"Unity check = {N(vt.UnityCheck, "0.00")} ≤ 1,0: geen aanvullende wapening vereist.",
                        $"Unity check = {N(vt.UnityCheck, "0.00")} > 1,0: aanvullende wapening vereist.");

                s.Regel($"Langswapening t.b.v. wringing (6.28), te verdelen over de omtrek $u_k = {N(t.Uk, "0")}$ mm:")
                 .Formule(t.AslFormula.FullValue)
                 .Formule(t.AslBovenOnderFormula.FullValue)
                 .Formule(t.AslLinksRechtsFormula.FullValue);
            });

            // 10. Knopen
            b.Stap("knopen", "Controle knopen", s => s
                .Regel("Afmetingen van knoop 1 (CCC):")
                .Formule(gdl
                    ? $@"x_1 = {N(r.X1)} \text{{ mm}} \qquad y_1 = x_1 \frac{{F_{{1x}}}}{{F_{{Ed}}}} = {N(r.X1)} \cdot \frac{{{N(r.F1x)}}}{{{N(r.FEd, "0")}}} = {N(r.Y1)} \text{{ mm}}"
                    : $@"x_1 = {N(r.X1)} \text{{ mm}} \qquad y_1 = \frac{{F_{{1x}}}}{{b_c\,\sigma_{{1Rd,max}}}} = \frac{{{N(r.F1x * 1000, "0")}}}{{{N(r.Bc, "0")} \cdot {N(r.Sigma1RdMax, "0.00")}}} = {N(r.Y1)} \text{{ mm}}")
                .Formule($@"\sigma_{{Ed,1}} = \max\left(\frac{{F_{{Ed}}}}{{b_c\,x_1}};\ \frac{{F_{{1x}}}}{{b_c\,y_1}}\right) = {N(r.SigmaNode1Ed, "0.00")} \leq \sigma_{{1Rd,max}} = {N(r.Sigma1RdMax, "0.00")} \text{{ N/mm²}}")
                .Toets(r.Node1Ok, "Knoop 1 (CCC) voldoet.", "Knoop 1 (CCC) voldoet niet.")
                .Formule($@"\sigma_{{Ed,2}} = \frac{{F_{{Ed}}}}{{a_b\,b_b}} = \frac{{{N(r.FEd * 1000, "0")}}}{{{N(r.LoadPlateLength, "0")} \cdot {N(r.LoadPlateWidth, "0")}}} = {N(r.SigmaNode2Ed, "0.00")} \leq \sigma_{{2Rd,max}} = {N(r.Sigma2RdMax, "0.00")} \text{{ N/mm²}}")
                .Toets(r.Node2Ok, "Knoop 2 onder de oplegplaat (CCT) voldoet.", "Knoop 2 onder de oplegplaat (CCT) voldoet niet."));

            // 11. Bruikbaarheidsgrenstoestand
            var mw = r.MinimumWapening;
            b.Stap_Als(mw is not null, "minimumwapening", "Minimale wapening 7.3.2 (7.1)", s => s
                .Regel($"Normaalkracht (trek) in BGT: $N = {N(i.HBgt, "0")}$ kN, dus $\\sigma_N = N/(b \\cdot h) = {N(mw!.SigmaN, "0.00")}$ N/mm².")
                .Regel("Spanningen net voor scheurvorming (lineair verloop):")
                .Formule(mw.SigmaBovenFormula.FullValue)
                .Formule(mw.SigmaOnderFormula.FullValue)
                .Formule(mw.HcrFormula.FullValue)
                .Formule(mw.ActFormula.FullValue)
                .Formule(mw.AsMinFormula.FullValue)
                .Toets(r.AsMinOk, "As,prov ≥ As,min.", "As,prov < As,min."));

            var sw = r.Scheurwijdte;
            b.Stap_Als(sw is not null, "scheurwijdte", "Scheurwijdte 7.3.4 (7.8)", s => s
                .Formule(sw!.SigmaSFormula.FullValue)
                .Formule(sw.DiameterEqFormula.FullValue)
                .Formule(sw.HcEffFormula.FullValue)
                .Formule(sw.SrMaxFormula.FullValue)
                .Formule(sw.EpsSmMinusEpsCmFormula.FullValue)
                .Formule(sw.WkFormula.FullValue)
                .Toets(sw.IsVoldoende,
                    $"$w_k = {N(sw.Wk, "0.00")} \\leq w_{{max}} = {N(sw.WMax, "0.00")}$ mm.",
                    $"$w_k = {N(sw.Wk, "0.00")} > w_{{max}} = {N(sw.WMax, "0.00")}$ mm."));

            // 12. Wapening t.p.v. oplegging
            b.Stap_Als(i.FlexibelOplegmateriaal, "wap-oplegging", "Wapening t.p.v. oplegging", s => s
                .Regel("Bij flexibel oplegmateriaal is wapening in de dwarsrichting onder de oplegging nodig:")
                .Formule($@"A_{{s,oplegging}} = 0.25 \cdot \frac{{t}}{{b_b}} \cdot \frac{{F_{{Ed}}}}{{f_{{yd}}}} = 0.25 \cdot \frac{{{N(i.DikteOplegmateriaal, "0")}}}{{{N(r.LoadPlateWidth, "0")}}} \cdot \frac{{{N(r.FEd * 1000.0, "0")}}}{{{N(r.Fyd, "0")}}} = {N(r.AsOplegging, "0")} \text{{ mm}}^2"));

            // 13. Staafgroepen en verankering
            foreach (var tg in r.StaafgroepToetsen)
            {
                var rbg = tg.Groep;
                b.Stap($"toets-{tg.Naam}", $"Toets {tg.Naam}", s => s
                   .Formule($@"{rbg.AantalPosities}{rbg.Prefix}\phi{rbg.Diameter} \qquad \phi_{{eq}} = {rbg.EquivalenteDiameter():0} \qquad \phi_{{m,prov}} = {rbg.BuigdoornDiameter():0} \qquad s = {rbg.Tussenruimte():0} \text{{ mm}}")
                   .Regel($"Aantal doorsneden in snede/zone: $n = {tg.AantalDoorsneden}$")
                   .Formule($@"A_{{s,prov}} = {N(tg.AsAanwezig, "0")} \text{{ mm²}} \qquad A_{{s,req}} = {N(tg.AsBenodigd, "0")} \text{{ mm}}^2")
                   .Formule($@"\sigma_s = \frac{{A_{{s,req}}}}{{A_{{s,prov}}}} \cdot f_{{yd}} = {N(tg.SigmaS, "0")} \text{{ N/mm²}} \quad (UC = {N(tg.UcDoorsneden, "0.00")})")
                   .Toets(tg.VoldoetDoorsneden, "Doorsneden voldoen.", "Doorsneden onvoldoende."));

                foreach (var vk in tg.Verankeringen)
                {
                    b.Stap(vk.Id, vk.Naam, s => s
                        .Formule(@$"\boxed{{ l_{{bd}} = {vk.Verankeringslengte:0} \text{{ mm}} }}")
                        .Formule(vk.VerankeringslengteFormula.FullValue)
                        .Formule(vk.BasisVerankeringslengteFormula.FullValue)
                        .Formule(vk.FbdFormula.FullValue)
                        .Regel($"Staafvorm: {vk.StaafVorm}, staaftype: {vk.StaafType}. Het rechte staafdeel tot aan de ombuiging is {vk.AfstandTotFbt:0} mm.")
                        .Formule(@$"\boxed{{ \phi_{{m,min}} \geq {vk.MinimaleBuigdoornDiameter:0} \text{{ mm}} }}")
                        .Formule(vk.MinimaleBuigdoornDiameterFormula.FullValue)
                        .Formule(vk.MinimaleBuigdoornDiameterBetonFormula.FullValue)
                        .Formule(vk.MinimaleBuigdoornDiamStaalFormula.FullValue)
                        .Formule(vk.FbtFormula.FullValue)
                        .Formule(vk.FactorResterendeLengteFormula.FullValue));
                }
            }

            if (r.TrekbandToets is { } tb)
            {
                b.Stap("toets-trekband", "Toets trekband bovenin (snede x = 0)", s => s
                    .Formule($@"A_{{s,ben}} = A_{{s,main}} + \Delta A_{{sl,T}} = {N(tb.AsMainReq, "0")} + {N(tb.AslTorsie, "0")} = {N(tb.AsBenodigd, "0")} \text{{ mm}}^2")
                    .Regel($"Doorsneden op $x = 0$: {tb.DoorsnedenVerticaal} (verticale haarspelden) + {tb.DoorsnedenHorizontaal} (horizontale haarspelden)")
                    .Formule($@"A_{{s,aanw}} = {N(tb.AsAanwezig, "0")} \text{{ mm}}^2 \qquad UC = {N(tb.Uc, "0.00")}")
                    .Toets(tb.Voldoet, "Trekband voldoet.", "Trekband onvoldoende."));
            }

            // 14. Momenten t.b.v. de kolom
            b.Stap("momenten", "Momenten t.b.v. kolom/wand", s => s
                .Formule($@"M_{{rand,Ed}} = F_{{Ed}} \cdot a_c + H_{{Ed}} \cdot a_h = {N(r.FEd, "0.#")} \cdot {N(r.Ac / 1000, "0.000")} + {N(r.HEd, "0.#")} \cdot {N(r.Ah / 1000, "0.000")} = {N(r.Mrand, "0.0")} \text{{ kNm}}")
                .Formule($@"M_{{hart,Ed}} = M_{{rand,Ed}} + \frac{{d_{{kolom}}}}{{2}} \cdot F_{{Ed}} = {N(r.Mhart, "0.0")} \text{{ kNm}}"));

            var slot = new StringBuilder();
            AppendMeldingen(slot, r);
            b.Slot(slot.ToString());

            return b;
        }

        private static string Intro(J3ConsoleInput i, J3ConsoleResult r)
        {
            bool gdl = IsGdl(r);
            string av = i.AvDefinitie == J3ConsoleInput.AvOptie.TotRandOplegplaat ? "$a_v = a_c - a_b/2$" : "$a_v = a_c$";
            string fywd = i.FywdDefinitie == J3ConsoleInput.FywdOptie.Fyd ? "$f_{ywd} = f_{yd}$" : "$f_{ywd} = 0{,}8 f_{yk}$";

            return $@"
### Schema
<div style=""max-width:480px"">
{r.SchemaSvg}
</div>

### Uitgangspunten
| | |
|---|---|
| Rekenmethode | {(gdl ? "gedrongen-liggertheorie 6.1 (10)" : "strut-and-tie, bijlage J.3")} |
| Controle dwarskracht | {(r.ControleDwarskracht ? "ja" : "nee")}{(gdl ? " (verplicht bij gedrongen-liggertheorie)" : "")} |
| Afstand $a_v$ (6.2.2 (6)) | {av} |
| Beugelspanning | {fywd} |

### Invoer
| Symbool | Waarde | Toelichting |
|---|---|---|
| $F_{{Ed}}$ | {N(i.FEd, "0")} kN | Verticale belasting |
| $H_{{Ed}}$ | {N(i.HEd, "0")} kN | Horizontale belasting |
| $f_{{BGT}}$ | {N(i.FactorBgt, "0.00")} | Factor BGT/UGT |
| $a_c$ | {N(i.Ac, "0")} mm | Afstand belasting tot betonrand |
| $a_b \times b_b$ | {N(r.LoadPlateLength, "0")} × {N(r.LoadPlateWidth, "0")} mm | Oplegplaat |
| $L_c$ | {N(i.Lc, "0")} mm | Lengte console |
| $b_c$ | {N(i.Bc, "0")} mm | Breedte console |
| $h_c$ | {N(i.Hc, "0")} mm | Hoogte console |
| $c$ | {N(i.Dekking, "0")} mm | Dekking |
| $d'$ | {(r.DOpgegeven ? $"{N(r.D1, "0")} mm" : "berekend")} | Randafstand trekband |
| $Ø_{{main}}$ | {N(i.HoofdstaafDiameter, "0")} mm | Diameter trekbandwapening |
| $Ø_{{bgl}}$ | {N(i.BeugelDiameter, "0")} mm | Diameter beugels |
| $f_{{ck}}$ | {N(i.Fck, "0")} N/mm² | Druksterkte beton |
| $f_{{yk}}$ | {N(i.Fyk, "0")} N/mm² | Vloeigrens betonstaal |
";
        }

        /// <summary>
        /// Genummerde opmerkingen onder de rapportage: eerst de meldingen uit de berekening (in LaTeX,
        /// nummers gelijk aan <see cref="J3ConsoleResult.OpmerkingNummer"/>), daarna de validaties.
        /// </summary>
        private static void AppendMeldingen(StringBuilder sb, J3ConsoleResult r)
        {
            var opmerkingen = r.Meldingen.Select(m => m.Tex)
                .Concat(r.Validaties)
                .Distinct()
                .ToList();
            if (opmerkingen.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("## Opmerkingen");
            sb.AppendLine();
            for (int n = 0; n < opmerkingen.Count; n++)
                sb.AppendLine($"{n + 1}. {opmerkingen[n]}");
        }

        private static bool IsGdl(J3ConsoleResult r) =>
            r.RekenMethode == J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie;
    }
}
