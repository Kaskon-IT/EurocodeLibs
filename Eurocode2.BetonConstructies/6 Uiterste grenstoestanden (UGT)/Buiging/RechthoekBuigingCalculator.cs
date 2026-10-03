namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Buiging zonder normaalkracht van een rechthoekige doorsnede (6.1), met de drukzone als
    /// spanningsblok (α, β uit het spanning-rekdiagram van het beton bij ε_cu) en betonstaal met
    /// horizontale tak (een hellende tak wordt veilig genegeerd).
    /// <list type="bullet">
    /// <item>Ontwerp: benodigde trek- en drukwapening voor <i>M</i>Ed. Boven <i>x</i>u,max (6.1 (9))
    /// wordt drukwapening toegevoegd; de spanning daarin volgt uit de rek bij <i>x</i>u,max.</item>
    /// <item>Controle: <i>M</i>Rd van de aanwezige wapening uit krachtenevenwicht.</item>
    /// </list>
    /// </summary>
    public static class RechthoekBuigingCalculator
    {
        public static RechthoekBuigingResult Bereken(RechthoekBuigingInput input)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.Beton);
            if (input.Breedte <= 0 || input.Hoogte <= 0 || input.NuttigeHoogte <= 0)
                throw new ArgumentOutOfRangeException(nameof(input), "Breedte, hoogte en nuttige hoogte moeten groter zijn dan nul.");
            if (input.NuttigeHoogte > input.Hoogte)
                throw new ArgumentOutOfRangeException(nameof(input), "De nuttige hoogte mag niet groter zijn dan de hoogte.");

            var meldingen = new List<string>();
            var beton = input.Beton;
            var alpha = beton.GetAlpha();
            var beta = beton.GetBeta();
            var epsCu = beton.EpsilonCu;
            var fcd = beton.Fcd;
            var fyd = beton.BetonStaal.Fyd;
            const double es = BetonStaalContext.Es;

            var b = input.Breedte;
            var h = input.Hoogte;
            var d = input.NuttigeHoogte;
            var d2 = input.AfstandDrukwapening;
            var mEd = Math.Abs(input.MEd);

            // Nc = α·b·x·fcd = k·x
            var k = alpha * b * fcd;

            var xuMax = BuigingContext.GetMaximaleHoogteDrukzoneZonderVoorspanning(beton, d, b * h);
            var mRdMax = k * xuMax * (d - beta * xuMax) / 1e6;

            // M = k·x·(d − β·x) → x = (d − √(d² − 4β·M/k)) / 2β
            var xu = GetX(mEd, d, beta, k);

            double z, asBerekend, asDruk = 0, epsS2 = 0, sigmaS2 = 0;
            var drukwapeningNodig = double.IsNaN(xu) || xu > xuMax;
            if (!drukwapeningNodig)
            {
                z = d - beta * xu;
                asBerekend = k * xu / fyd;
            }
            else
            {
                meldingen.Add($"x_u > x_u,max = {xuMax:0.#} mm (6.1 (9)): drukwapening toegepast.");
                z = d - beta * xuMax;
                epsS2 = epsCu * (xuMax - d2) / xuMax;
                if (d2 <= 0 || d2 >= xuMax)
                {
                    meldingen.Add("De drukwapening ligt niet in de drukzone (d₂ ≤ 0 of d₂ ≥ x_u,max): moment niet opneembaar.");
                    asBerekend = double.NaN;
                    asDruk = double.NaN;
                }
                else
                {
                    sigmaS2 = Math.Min(es * epsS2, fyd);
                    asDruk = (mEd - mRdMax) * 1e6 / (sigmaS2 * (d - d2));
                    asBerekend = k * xuMax / fyd + asDruk * sigmaS2 / fyd;
                    if (sigmaS2 < fyd)
                        meldingen.Add($"De drukwapening vloeit niet: σ_s2 = {sigmaS2:0} N/mm² < f_yd.");
                }
            }

            // Minimale wapening zoals in BendingResults: scheurmoment met f_ctm·b·h²/6, begrensd op 1,25·A_s,ber
            var mCr = beton.Fctm * b * h * h / 6 / 1e6;
            var xe = GetX(mCr, d, beta, k);
            var asMin1 = double.IsNaN(xe) ? double.PositiveInfinity : k * xe / fyd;
            var asMin = Math.Min(asMin1, 1.25 * asBerekend);
            var asTrek = Math.Max(asBerekend, asMin);

            var (xMRd, sigmaS1MRd, sigmaS2MRd, mRd) = BerekenMRd(beton, b, d, input.AsTrekToegepast, input.AsDrukToegepast, d2);
            if (input.AsDrukToegepast > 0 && d2 <= 0)
                meldingen.Add("Drukwapening opgegeven zonder afstand d₂: drukwapening bij M_Rd aan de rand aangenomen.");
            if (input.AsTrekToegepast > 0 && xMRd > xuMax)
                meldingen.Add($"Drukzone bij M_Rd x = {xMRd:0.#} mm > x_u,max = {xuMax:0.#} mm: onvoldoende vervormingscapaciteit (6.1 (9)).");

            return new RechthoekBuigingResult
            {
                Alpha = alpha,
                Beta = beta,
                EpsilonCu = epsCu,
                Fcd = fcd,
                Fyd = fyd,
                MEd = mEd,
                Xu = xu,
                XuMax = xuMax,
                Z = z,
                MRdMaxZonderDrukwapening = mRdMax,
                IsDrukwapeningNodig = drukwapeningNodig,
                EpsilonS2 = epsS2,
                SigmaS2 = sigmaS2,
                AsBerekend = asBerekend,
                AsMin1 = asMin1,
                AsMin = asMin,
                AsTrekBenodigd = asTrek,
                AsDrukBenodigd = asDruk,
                AsTrekToegepast = input.AsTrekToegepast,
                AsDrukToegepast = input.AsDrukToegepast,
                XMRd = xMRd,
                SigmaS1MRd = sigmaS1MRd,
                SigmaS2MRd = sigmaS2MRd,
                MRd = mRd,
                Meldingen = meldingen,
            };
        }

        /// <summary>
        /// Momentcapaciteit uit krachtenevenwicht bij betonstuik ε_cu aan de gedrukte rand:
        /// α·b·x·f_cd + A_s2·σ_s2(x) = A_s1·σ_s1(x), met |σ_s| = min(E_s·ε_s; f_yd).
        /// </summary>
        /// <returns>Drukzonehoogte x [mm], σ_s1 en σ_s2 [N/mm², druk positief voor σ_s2] en M_Rd [kNm].</returns>
        public static (double X, double SigmaS1, double SigmaS2, double MRd) BerekenMRd(
            BetonContext beton, double breedte, double nuttigeHoogte, double asTrek, double asDruk = 0, double afstandDrukwapening = 0)
        {
            ArgumentNullException.ThrowIfNull(beton);
            if (asTrek <= 0 || breedte <= 0 || nuttigeHoogte <= 0)
                return (0, 0, 0, 0);

            var k = beton.GetAlpha() * breedte * beton.Fcd;
            var beta = beton.GetBeta();
            var epsCu = beton.EpsilonCu;
            var fyd = beton.BetonStaal.Fyd;
            var d = nuttigeHoogte;
            var d2 = Math.Max(afstandDrukwapening, 0);

            double SigmaS1(double x) => Begrens(BetonStaalContext.Es * epsCu * (d - x) / x, fyd);
            double SigmaS2(double x) => Begrens(BetonStaalContext.Es * epsCu * (x - d2) / x, fyd);
            // stijgend in x: negatief bij x → 0, positief bij x = d (σ_s1 = 0)
            double Resultante(double x) => k * x + asDruk * SigmaS2(x) - asTrek * SigmaS1(x);

            double onder = 1e-9 * d, boven = d;
            for (var i = 0; i < 200 && boven - onder > 1e-9 * d; i++)
            {
                var midden = (onder + boven) / 2;
                if (Resultante(midden) < 0) onder = midden; else boven = midden;
            }

            var xRd = (onder + boven) / 2;
            var s1 = SigmaS1(xRd);
            var s2 = asDruk > 0 ? SigmaS2(xRd) : 0;
            var mRd = (k * xRd * (d - beta * xRd) + asDruk * s2 * (d - d2)) / 1e6;
            return (xRd, s1, s2, mRd);
        }

        private static double GetX(double mKNm, double d, double beta, double k)
        {
            var discriminant = d * d - 4 * beta * mKNm * 1e6 / k;
            return discriminant < 0 ? double.NaN : (d - Math.Sqrt(discriminant)) / (2 * beta);
        }

        private static double Begrens(double waarde, double grens) => Math.Max(-grens, Math.Min(grens, waarde));
    }
}
