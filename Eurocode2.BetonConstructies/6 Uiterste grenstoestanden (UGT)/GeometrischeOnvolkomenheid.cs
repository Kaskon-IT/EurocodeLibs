using Eurocode.Grondslagen;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// EC2 §5.2 / §6.1(4) – Geometrische onvolkomenheid voor geïsoleerde kolommen.
    /// </summary>
    public static class GeometrischeOnvolkomenheid
    {
        /// <summary>§5.2(7) Basishoek θ₀ = 1/200 [-].</summary>
        /// NL 5.2(5) De waarde van θ₀ = 1/300
        /// 
        public const double Theta0 = 1.0 / 300.0;

        /// <summary>
        /// §5.2(7) Reductiefactor voor lengte αh = 2/√l [-], begrensd op [2/3 ; 1].
        /// </summary>
        /// <param name="l0">Kniklengte l₀ [mm]</param>
        public static double AlphaH(double l0)
        {
            double l_m = l0 / 1000.0;
            return Math.Clamp(2.0 / Math.Sqrt(l_m), 2.0 / 3.0, 1.0);
        }

        /// <summary>
        /// §5.2(7) Reductiefactor voor aantal leden αm = √(0.5·(1 + 1/m)) [-].
        /// Voor een geïsoleerde kolom: m = 1 → αm = 1.0.
        /// </summary>
        /// <param name="m">Aantal leden m [-] (default 1 voor geïsoleerde kolom)</param>
        public static double AlphaM(int m = 1) =>
            Math.Sqrt(0.5 * (1.0 + 1.0 / Math.Max(m, 1)));

        /// <summary>
        /// §5.2(7) Hellingshoek θᵢ = θ₀ · αh · αm [-].
        /// </summary>
        public static double ThetaI(double l0, int m = 1) =>
            Theta0 * AlphaH(l0) * AlphaM(m);

        /// <summary>
        /// §5.2(7) Imperfectie-excentriciteit eᵢ = θᵢ · l₀/2 [mm].
        /// </summary>
        public static double Ei(double l0, int m = 1) =>
            ThetaI(l0, m) * l0 / 2.0;

        /// <summary>
        /// Geometrische onvolkomenheid e₀ voor een geschoorde kolom [mm].
        /// e₀ = max(eᵢ §5.2(7) ; h/30 ; 20 mm) — EC2 §6.1(4) + §5.2(7).
        /// </summary>
        /// <param name="l0">Kniklengte l₀ [mm]</param>
        /// <param name="h">Hoogte van de doorsnede in de buigrichting [mm]</param>
        /// <param name="nb">Nationale bijlage (niet van invloed op de formule)</param>
        public static double E0(double l0, double h, NationaleBijlageEnum? nb = NationaleBijlageEnum.NL) =>
            Math.Max(Math.Max(Ei(l0), h / 30.0), 20.0);
    }
}
