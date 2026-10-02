using Eurocode.BetonConstructies;

namespace Eurocode2.BetonConstructies.Tests
{
    /// <summary>
    /// J3-console: één berekening met keuze gedrongen-liggertheorie (GDL) of strut-and-tie (STM),
    /// optionele dwarskrachtcontrole en compacte of uitgebreide rapportage.
    /// Standaardconsole: h = 500, c = 30, Ø_bgl = 8, Ø_main = 12, F_Ed = 245 kN, H_Ed = 49 kN, a_c = 150, C30, B500.
    /// </summary>
    public class J3ConsoleCalculatorTests
    {
        private static J3ConsoleInput Invoer(J3ConsoleInput.RekenMethodeOptie methode, Action<J3ConsoleInput>? pas = null)
        {
            var i = new J3ConsoleInput { RekenMethode = methode };
            pas?.Invoke(i);
            return i;
        }

        private const J3ConsoleInput.RekenMethodeOptie Gdl = J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie;
        private const J3ConsoleInput.RekenMethodeOptie Stm = J3ConsoleInput.RekenMethodeOptie.StrutAndTieModel;

        [Fact]
        public void NuttigeHoogte_ZonderOpgave_DPrimeIsDekkingPlusBeugelPlusHalveHoofdstaaf()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm));

            // d' = 30 + 8 + 12/2 = 44 ; d = 500 - 44 = 456
            Assert.False(r.DOpgegeven);
            Assert.Equal(44.0, r.D1, 6);
            Assert.Equal(456.0, r.D, 6);
        }

        [Fact]
        public void NuttigeHoogte_MetOpgave_GebruiktOpgegevenDPrime()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => i.D1Opgave = 50));

            Assert.True(r.DOpgegeven);
            Assert.Equal(450.0, r.D, 6);
        }

        [Fact]
        public void Gdl_HefboomsarmVolgensFormule6_1_10()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Gdl));

            // a_r = min(a_b/2 ; L/4 ; H/4) = min(75 ; 75 ; 125) = 75 ; a = 150 + 75 = 225
            // z = min(0.4·225 + 0.4·500 ; 1.6·225) = min(290 ; 360) = 290 ; tanθ = 290/225
            Assert.Equal(225.0, r.A, 6);
            Assert.Equal(290.0, r.Z, 6);
            Assert.Equal(290.0 / 225.0, r.TanTheta, 6);
            Assert.False(r.ZBegrensd);
        }

        [Fact]
        public void Stm_HefboomsarmUitEvenwichtKnoop1()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm));

            // f_cd = 20 ; ν' = 0.6(1 - 30/250) = 0.528 ; σ1Rd,max = 10.56
            // x1 = 245000/(10.56·250) = 92.80 ; Δa = 49/245·44 = 8.8 ; a = 150 + 46.40 + 8.8 = 205.20
            // z = (456 + √(456² - 2·245000·205.20/(250·10.56)))/2 = 434.07
            Assert.Equal(10.56, r.Sigma1RdMax, 6);
            Assert.Equal(205.20, r.A, 1);
            Assert.Equal(434.07, r.Z, 1);
            Assert.False(r.ZBegrensd);
        }

        [Fact]
        public void TanTheta_GroterDan2_5_BegrenstZ()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => { i.Ac = 80; i.FEd = 100; i.HEd = 10; }));

            Assert.True(r.ZBegrensd);
            Assert.True(r.ZOnbegrensd > 2.5 * r.A);
            Assert.Equal(2.5 * r.A, r.Z, 6);
            Assert.Equal(2.5, r.TanTheta, 6);
            Assert.Contains(r.Meldingen, m => m.Contains("z begrensd"));
        }

        [Fact]
        public void BeugelType_KorteConsole_Horizontaal()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm));

            // a_c = 150 ≤ 0.5·500
            Assert.Equal(J3ConsoleLinkType.HorizontaalOfSchuin, r.LinkType);
            Assert.Equal(0.25 * r.AsMain, r.AsLnkMin, 6);
        }

        [Fact]
        public void BeugelType_SlankeConsole_VerticaalAlsFEdGroterDanVRdc()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => { i.Ac = 300; i.Lc = 450; i.AfschuiningOnderzijde = false; }));

            Assert.True(r.FEd > r.Dwarskracht!.VRdc);
            Assert.Equal(J3ConsoleLinkType.Verticaal, r.LinkType);
            Assert.Equal(0.5 * 245_000 / (500 / 1.15), r.AsLnkMin, 6);
            Assert.True(r.ControleDwarskracht); // verplicht bij verticale beugels
        }

        [Fact]
        public void BeugelType_SlankeConsole_GeenAlsFEdNietGroterDanVRdc()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i =>
            {
                i.Ac = 300; i.Lc = 450; i.AfschuiningOnderzijde = false;
                i.FEd = 30; i.HEd = 0; i.ControleDwarskracht = false;
            }));

            Assert.True(r.FEd <= r.Dwarskracht!.VRdc);
            Assert.Equal(J3ConsoleLinkType.Geen, r.LinkType);
            Assert.False(r.VerticaleBeugelsNodig);
        }

        [Fact]
        public void Dwarskracht_VerticaleBeugels_NietMinderDanNodigVoorDwarskracht()
        {
            // a_c = 500: β = 500/(2·456) ≈ 0.55 → A_sw = β·V_Ed/f_ywd ≈ 337 mm² > k2·F_Ed/f_yd ≈ 282 mm²
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => { i.Ac = 500; i.Lc = 650; i.AfschuiningOnderzijde = false; }));

            Assert.Equal(J3ConsoleLinkType.Verticaal, r.LinkType);
            Assert.True(r.Dwarskracht!.WapeningNodig);
            Assert.True(r.DwarskrachtMaatgevend);
            Assert.Equal(Math.Max(r.AswJ3, r.Dwarskracht.AswV), r.Asw, 6);
            Assert.Equal(r.Dwarskracht.AswV, r.Asw, 6);
        }

        [Fact]
        public void Dwarskracht_HorizontaleBeugels_ExtraVerticaleBeugelsAlsDwarskrachtDatVraagt()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => i.ControleDwarskracht = true));

            Assert.Equal(J3ConsoleLinkType.HorizontaalOfSchuin, r.LinkType);
            Assert.True(r.Dwarskracht!.WapeningNodig);
            Assert.True(r.VerticaleBeugelsVoorDwarskracht);
            Assert.True(r.VerticaleBeugelsNodig);
            Assert.Equal(r.Dwarskracht.AswV, r.AswVerticaalDwarskracht, 6);
            Assert.Equal(r.AswJ3, r.Asw, 6); // de horizontale beugels blijven volgens J.3
        }

        [Fact]
        public void Dwarskracht_StmZonderControle_WordtNietMeegenomen()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => i.ControleDwarskracht = false));

            Assert.False(r.ControleDwarskracht);
            Assert.False(r.VerticaleBeugelsVoorDwarskracht);
            Assert.Equal(r.AswJ3, r.Asw, 6);
        }

        [Fact]
        public void Dwarskracht_GdlIsAltijdVerplicht()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Gdl, i => i.ControleDwarskracht = false));

            Assert.True(r.ControleDwarskracht);
        }

        [Fact]
        public void Av_Keuze_TotRandOplegplaat()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i =>
            {
                i.Ac = 400; i.Lc = 550; i.AfschuiningOnderzijde = false;
                i.AvDefinitie = J3ConsoleInput.AvOptie.TotRandOplegplaat;
            }));

            // a_v = a_c - a_b/2 = 400 - 150/2 = 325 (> ondergrens 0.5d = 228) ; β = a_v/2d
            Assert.Equal(325.0, r.Dwarskracht!.Av, 6);
            Assert.Equal(325.0 / (2 * r.Dwarskracht.D), r.Dwarskracht.Beta, 6);
        }

        [Theory]
        [InlineData(J3ConsoleInput.FywdOptie.TachtigProcentFyk, 400.0)]
        [InlineData(J3ConsoleInput.FywdOptie.Fyd, 500.0 / 1.15)]
        public void Fywd_Keuze(J3ConsoleInput.FywdOptie keuze, double verwacht)
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Gdl, i => i.FywdDefinitie = keuze));

            Assert.Equal(verwacht, r.Dwarskracht!.Fywd, 6);
            Assert.Equal(r.Dwarskracht.VEdRed * 1000.0 / verwacht, r.Dwarskracht.AswV, 6);
        }

        [Fact]
        public void BgtEnWringing_OokBijStm()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm));

            Assert.NotNull(r.MinimumWapening);
            Assert.NotNull(r.Scheurwijdte);
            Assert.NotNull(r.Torsie);
        }

        [Theory]
        [InlineData(J3ConsoleInput.RekenMethodeOptie.GedrongenLiggerTheorie, "6.1 (10)")]
        [InlineData(J3ConsoleInput.RekenMethodeOptie.StrutAndTieModel, "positie drukknoop")]
        public void Rapportage_Uitgebreid_BevatStappenEnMethode(J3ConsoleInput.RekenMethodeOptie methode, string verwacht)
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(methode));

            Assert.Equal(r.RekenvoorbeeldMarkdown, r.RapportMarkdown);
            Assert.Contains("## Stap", r.RapportMarkdown);
            Assert.Contains(verwacht, r.RapportMarkdown);
            Assert.Contains(@"\tan\theta", r.RapportMarkdown);
            Assert.Contains("Controle dwarskracht", r.RapportMarkdown);
        }

        [Fact]
        public void Rapportage_Compact_IsTabel()
        {
            var r = J3ConsoleCalculator.Bereken(Invoer(Stm, i => i.Rapportage = J3ConsoleInput.RapportageOptie.Compact));

            Assert.Equal(r.RapportCompactMarkdown, r.RapportMarkdown);
            Assert.Contains("| Omschrijving |", r.RapportMarkdown);
            Assert.DoesNotContain("## Stap", r.RapportMarkdown);
        }
    }
}
