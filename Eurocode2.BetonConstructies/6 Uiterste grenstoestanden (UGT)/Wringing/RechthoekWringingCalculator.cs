namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Wringing van een massieve rechthoekige balk volgens 6.3.2, met de combinatie met dwarskracht.
    /// <list type="bullet">
    /// <item>Equivalente dunwandige doorsnede: t_ef,i = max(A/u ; 2·(c_i + Ø_bgl + Ø_i/2)) per wand;
    /// z_i tussen de hartlijnen van de aangrenzende wanden, A_k en u_k daaruit.</item>
    /// <item>T_Rd,c (scheurmoment, τ = f_ctd) en T_Rd,max (6.30) met de kleinste t_ef,i.</item>
    /// <item>(6.31): als T_Ed/T_Rd,c + V_Ed/V_Rd,c ≤ 1 is alleen minimale wapening nodig.</item>
    /// <item>(6.29): T_Ed/T_Rd,max + V_Ed/V_Rd,max ≤ 1 (drukdiagonalen), V_Rd,max volgens (6.9).</item>
    /// <item>Beugels A_sw,T/s = T_Ed/(2·A_k·f_yd·cot θ) per wand; langswapening (6.28) naar rato van z_i/u_k.</item>
    /// </list>
    /// θ moet gelijk zijn aan die van de dwarskrachtberekening. Reductie van langswapening in de
    /// drukzone (6.3.2 (3)) wordt niet toegepast (veilig).
    /// </summary>
    public static class RechthoekWringingCalculator
    {
        public static RechthoekWringingResult Bereken(RechthoekWringingInput input)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.Beton);
            if (input.Breedte <= 0 || input.Hoogte <= 0 || input.NuttigeHoogte <= 0)
                throw new ArgumentOutOfRangeException(nameof(input), "Breedte, hoogte en nuttige hoogte moeten groter zijn dan nul.");
            if (input.CotTheta < 1 || input.CotTheta > 2.5)
                throw new ArgumentOutOfRangeException(nameof(input), "cot θ moet tussen 1 en 2,5 liggen (6.2.3 (2)).");
            if (input.BeugelSneden < 2)
                throw new ArgumentOutOfRangeException(nameof(input), "Een gesloten beugel heeft minimaal 2 sneden.");

            var meldingen = new List<string>();
            var beton = input.Beton;
            var b = input.Breedte;
            var h = input.Hoogte;
            var tEd = Math.Abs(input.TEd);
            var vEd = Math.Abs(input.VEd);
            var cot = input.CotTheta;
            var tan = 1 / cot;
            var sinCos = cot / (1 + cot * cot);
            var fcd = beton.Fcd;
            var fctd = beton.Fctd;
            var fyd = beton.BetonStaal.Fyd;
            var nu = 0.6 * (1 - beton.Fck / 250);

            // 6.3.2 (1): t_ef = A/u, niet kleiner dan 2× de afstand van de rand tot het hart van de langswapening
            var tefBerekend = b * h / (2 * (b + h));
            double Tef(double dekking, double diameter) => Math.Max(tefBerekend, 2 * (dekking + input.BeugelDiameter + diameter / 2));
            var tefBoven = Tef(input.DekkingBoven, input.DiameterBoven);
            var tefOnder = Tef(input.DekkingOnder, input.DiameterOnder);
            var tefZijkant = Tef(input.DekkingZijkant, input.DiameterZijkant);
            var tefMin = Math.Min(tefBoven, Math.Min(tefOnder, tefZijkant));

            var zBreedte = b - tefZijkant;                    // (t_ef,links + t_ef,rechts)/2
            var zHoogte = h - (tefBoven + tefOnder) / 2;
            if (zBreedte <= 0 || zHoogte <= 0)
                throw new ArgumentOutOfRangeException(nameof(input), "De doorsnede is te klein voor de equivalente dunwandige doorsnede.");
            var ak = zBreedte * zHoogte;
            var uk = 2 * (zBreedte + zHoogte);

            var tRdc = 2 * ak * tefMin * fctd / 1e6;
            var tRdMax = 2 * nu * fcd * ak * tefMin * sinCos / 1e6;
            var vRdMax = b * 0.9 * input.NuttigeHoogte * nu * fcd / (cot + tan) / 1e3;

            var uc631 = input.VRdc > 0 ? tEd / tRdc + vEd / input.VRdc : double.NaN;
            var uc629 = tEd / tRdMax + vEd / vRdMax;
            if (double.IsNaN(uc631))
                meldingen.Add("V_Rd,c niet opgegeven: toets (6.31) overgeslagen, wringwapening altijd berekend.");
            else if (uc631 <= 1)
                meldingen.Add($"(6.31) = {uc631:0.00} ≤ 1: alleen minimale wapening nodig.");
            if (uc629 > 1)
                meldingen.Add($"(6.29) = {uc629:0.00} > 1: capaciteit van de drukdiagonalen overschreden.");

            var aswT = tEd * 1e6 / (2 * ak * fyd * cot) * 1000;
            var aslTotaal = tEd * 1e6 * uk * cot / (2 * ak * fyd);

            var aswTBenodigd = !double.IsNaN(uc631) && uc631 <= 1 ? 0 : aswT;
            var aswTotaal = Math.Max(input.AswVPerMeter + 2 * aswTBenodigd, input.AswMinPerMeter);
            var aswPerSnede = Math.Max(aswTotaal / input.BeugelSneden, aswTBenodigd);

            var sMax = Math.Min(2 * (b + h) / 8, Math.Min(b, h));

            return new RechthoekWringingResult
            {
                TEd = tEd,
                VEd = vEd,
                CotTheta = cot,
                Fctd = fctd,
                Fcd = fcd,
                Fyd = fyd,
                Nu = nu,
                TefBerekend = tefBerekend,
                TefBoven = tefBoven,
                TefOnder = tefOnder,
                TefZijkant = tefZijkant,
                ZBreedte = zBreedte,
                ZHoogte = zHoogte,
                Ak = ak,
                Uk = uk,
                TRdc = tRdc,
                TRdMax = tRdMax,
                VRdMax = vRdMax,
                VRdc = input.VRdc,
                UnityCheck631 = uc631,
                UnityCheck629 = uc629,
                AswTPerMeter = aswT,
                AslTotaal = aslTotaal,
                AswTotaalPerMeter = aswTotaal,
                AswPerSnedePerMeter = aswPerSnede,
                BeugelSneden = input.BeugelSneden,
                BeugelAfstandMax = sMax,
                Meldingen = meldingen,
            };
        }
    }
}
