namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Brandwerendheidseis voor massieve platen in 1 richting (vrijdragend).
    /// Ref: NEN-EN 1992-1-2:2004, Tabel 5.8
    /// </summary>
    public static class PlaatBrandwerendheid
    {
        /// <summary>
        /// Tabelwaarden voor vrijdragende massieve plaat in 1 richting.
        /// Kolommen: REI (min), minimale plaatdikte hs (mm), minimale hartafstand a (mm).
        /// Ref: NEN-EN 1992-1-2:2004, Tabel 5.8
        /// </summary>
        private static readonly (int Rei, double HsMin, double AMin)[] TabelVrijdragend =
        [
            ( 30,  60, 10),
            ( 60,  80, 20),
            ( 90, 100, 30),
            (120, 120, 40),
            (180, 150, 55),
            (240, 175, 65),
        ];

        /// <summary>
        /// Publieke toegang tot de tabelwaarden voor gebruik in UI en rapportage.
        /// Ref: NEN-EN 1992-1-2:2004, Tabel 5.8
        /// </summary>
        public static IEnumerable<(int Rei, double HsMin, double AMin)> Tabel => TabelVrijdragend;

        /// <summary>
        /// Bepaal de REI in minuten voor een vrijdragende massieve plaat in 1 richting.
        /// Ref: NEN-EN 1992-1-2:2004, Tabel 5.8
        /// </summary>
        /// <param name="h">Plaatdikte in mm</param>
        /// <param name="a">Hartafstand (as-afstand) in mm</param>
        /// <returns>Maximale REI in minuten, of 0 indien niet voldaan aan REI 30.</returns>
        public static int GetRei(double h, double a)
        {
            int rei = 0;
            foreach (var (Rei, HsMin, AMin) in TabelVrijdragend)
            {
                if (h >= HsMin && a >= AMin)
                    rei = Rei;
                else
                    break;
            }
            return rei;
        }




        /// <summary>
        /// Bepaal de minimale hartafstand a voor een vrijdragende massieve plaat in 1 richting.
        /// Ref: NEN-EN 1992-1-2:2004, Tabel 5.8
        /// </summary>
        /// <param name="rei">Brandweerheidseis in minuten (bijv. 30, 60, 90, 120, 180, 240)</param>
        /// <param name="h">Plaatdikte in mm</param>
        /// <returns>
        /// Vereiste hartafstand a in mm.
        /// Geeft <see cref="double.NaN"/> terug indien de plaatdikte onvoldoende is of de REI-waarde niet in de tabel staat.
        /// </returns>
        public static double GetAfstandBenodigdVoorREI(int rei, double h)
        {
            foreach (var (Rei, HsMin, AMin) in TabelVrijdragend)
            {
                if (Rei == rei)
                    return h >= HsMin ? AMin : double.NaN;
            }
            return double.NaN;
        }

        public static double GetMinimaleDikteVoorREI(int rei, double v)
        {
            foreach (var (Rei, HsMin, AMin) in TabelVrijdragend)
            {
                if (Rei == rei)
                    return v >= AMin ? HsMin : double.NaN;
            }
            return double.NaN;
        }
    }
}
