namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// EC2 §5.8 – Tweede-orde effecten voor slanke kolommen (geschoorde constructie).
    /// Methode op basis van nominale kromming conform §5.8.8.
    /// </summary>
    public static class TweedeOrdeEffecten
    {
        // ================================================================
        // §5.8.3 – Slankheidsverhouding
        // λ = l₀ / i    met  i = √(I/A) = h/√12  (rechthoekige doorsnede)
        // ================================================================

        /// <summary>
        /// §5.8.3 Slankheidsverhouding λ voor een rechthoekige doorsnede [-].
        /// λ = l₀ / (h / √12)
        /// </summary>
        /// <param name="l0">Kniklengte l₀ [mm]</param>
        /// <param name="h">Hoogte van de doorsnede in de buigrichting [mm]</param>
        public static double Slankheid(double l0, double h) =>
            l0 / (h / Math.Sqrt(12.0));

        // ================================================================
        // §5.8.3.1 – Grensslankheid λlim
        // λlim = 20 · A · B · C / √n
        //
        //  A = 1 / (1 + 0.2·φef) ≈ 0.7  (conservatief, kruip onbekend)
        //  B = √(1 + 2ω)               (ω = As·fyd / (Ac·fcd))
        //  C = 1.7 − rm ≈ 0.7          (conservatief, enkelvoudige kromming rm=1)
        //  n = NEd / (Ac · fcd)
        // ================================================================

        /// <summary>
        /// §5.8.3.1 Grensslankheid λlim [-].
        /// A = 0.7 (geen kruip), C = 0.7 (enkelvoudige kromming).
        /// B = √(1 + 2ω) met ω = As·fyd / (Ac·fcd).
        /// </summary>
        /// <param name="n">Relatieve normaalkracht n = NEd / (Ac·fcd) [-]</param>
        /// <param name="omega">Mechanische wapeningsgraad ω = As·fyd / (Ac·fcd) [-]</param>
        public static double SlankheidsGrens(double n, double omega) =>
            SlankheidsGrens(n, omega, 1.0);

        /// <summary>
        /// §5.8.3.1 Grensslankheid λlim [-] met momentverhouding rm = M01/M02.
        /// C = 1.7 − rm (min 0.7 voor enkelvoudige kromming, dubbelzijdig rm ≤ 1).
        /// </summary>
        /// <param name="n">Relatieve normaalkracht n = NEd / (Ac·fcd) [-]</param>
        /// <param name="omega">Mechanische wapeningsgraad ω = As·fyd / (Ac·fcd) [-]</param>
        /// <param name="rm">Momentverhouding rm = M01/M02 [-] (rm ≤ 1, negatief bij dubbelzijdige kromming)</param>
        public static double SlankheidsGrens(double n, double omega, double rm)
            => SlankheidsGrens(n, omega, rm, 0.0);

        /// <summary>
        /// §5.8.3.1 Grensslankheid λlim [-] met momentverhouding rm en kruipfactor φef.
        /// A = 1/(1+0.2·φef); bij φef=0 conservatief A=0.7.
        /// </summary>
        public static double SlankheidsGrens(double n, double omega, double rm, double phiEf)
        {
            double A = phiEf > 0.0 ? 1.0 / (1.0 + 0.2 * phiEf) : 0.7;
            double C = 1.7 - Math.Clamp(rm, -1.0, 1.0);
            return 20.0 * A * Math.Sqrt(1.0 + 2.0 * omega) * C / Math.Sqrt(Math.Max(n, 0.01));
        }

        // ================================================================
        // §5.8.8.3 – Additionele excentriciteit e₂ (nominale kromming)
        //
        // e₂ = (1/r) · l₀² / c
        // 1/r = Kr · Kφ · fyd / (Es · 0.45 · d)
        // Kr  = (nu − n) / (nu − nbal)  ≤ 1   (correctie voor normaalkrachtniveau)
        // nu  = 1 + ω
        // nbal = 0.4
        // Kφ  = 1 + β · φef ≥ 1             (§5.8.8.3 (4))
        // β   = 0.35 + fck/200 − λ/150       (§5.8.8.3 (4))
        // φef = effectieve kruipverhouding    (§5.8.4)
        // c   = π²   (sinusvormige deformatie)
        // ================================================================

        /// <summary>
        /// §5.8.8.3 Kruipfactor Kφ = max(1 ; 1 + β·φef).
        /// β = 0.35 + fck/200 − λ/150
        /// </summary>
        /// <param name="fck">Karakteristieke cilinderdruksterkte beton [N/mm²]</param>
        /// <param name="lambda">Slankheidsverhouding λ [-]</param>
        /// <param name="phiEf">Effectieve kruipverhouding φef [-]</param>
        public static double Kphi(double fck, double lambda, double phiEf)
        {
            double beta = 0.35 + fck / 200.0 - lambda / 150.0;
            return Math.Max(1.0, 1.0 + beta * phiEf);
        }

        /// <summary>
        /// §5.8.8.3 Additionele excentriciteit door tweede-orde effecten e₂ [mm].
        /// </summary>
        /// <param name="nEd">Rekenwaarde normaalkracht [kN] (positief = druk)</param>
        /// <param name="asTotal">Totale langswapening As [mm²]</param>
        /// <param name="l0">Kniklengte l₀ [mm]</param>
        /// <param name="h">Hoogte doorsnede [mm]</param>
        /// <param name="d2">Hartmaat wapening vanaf rand [mm]</param>
        /// <param name="fcd">Rekendruksterkte beton [N/mm²]</param>
        /// <param name="fck">Karakteristieke cilinderdruksterkte beton [N/mm²]</param>
        /// <param name="fyd">Rekensterkte staal [N/mm²]</param>
        /// <param name="es">Elasticiteitsmodulus staal [N/mm²]</param>
        /// <param name="ac">Betonoppervlak Ac [mm²]</param>
        /// <param name="phiEf">Effectieve kruipverhouding φef [-] (0 = geen kruip)</param>
        public static double E2(double nEd, double asTotal, double l0, double h, double d2,
                                 double fcd, double fck, double fyd, double es, double ac,
                                 double phiEf = 0.0, double c = 10.0)
        {
            double d       = h - d2;
            double lambda  = Slankheid(l0, h);
            double n       = nEd * 1000.0 / (ac * fcd);            // relatieve normaalkracht [-]
            double omega   = asTotal * fyd / (ac * fcd);            // mechanische wapeningsgraad [-]
            double nu      = 1.0 + omega;
            double Kr      = Math.Clamp((nu - n) / (nu - 0.4), 0.0, 1.0);
            double kPhi    = Kphi(fck, lambda, phiEf);

            double curvature = Kr * kPhi * fyd / (es * 0.45 * d);  // 1/r [mm⁻¹]

            return curvature * l0 * l0 / c;                        // e₂ [mm]
        }
    }
}
