using Eurocode.Grondslagen;

namespace Eurocode.BetonConstructies.SpecifiekeRegels
{
    /// <summary>
    /// EC2 §9.5 Kolommen – specifieke regels voor langswapening.
    /// Nationale bijlage-waarden conform NEN-EN 1992-1-1 NB (NL), CEN-EN (EU), NBN-EN (BE), DIN-EN (DE).
    /// </summary>
    public static class Kolommen
    {
        // ================================================================
        // §9.5.2 (1) – Minimale staafdiameter langswapening
        // EU: 8 mm (aanbevolen)
        // NL/NB: 8 mm
        // BE/NB: 12 mm
        // DE/NB: 12 mm
        // ================================================================

        /// <summary>
        /// §9.5.2 (1) Minimale diameter langswapening [mm].
        /// Nationale bijlage waarden.
        /// </summary>
        public static double DiameterMinLangs(NationaleBijlageEnum? nb = NationaleBijlageEnum.NL) =>
            nb switch
            {
                NationaleBijlageEnum.BE => 12.0,
                NationaleBijlageEnum.DE => 12.0,
                _                      => 8.0,   // EU aanbeveling, NL NB
            };

        // ================================================================
        // §9.5.2 (2) – Minimale wapeningsdoorsnede
        // As,min = max(0.10·NEd/fyd ; 0.002·Ac)
        // Geen nationale bijlage-afwijkingen; formule is identiek voor EU/NL/BE/DE.
        // ================================================================

        /// <summary>
        /// §9.5.2 (2) Minimale wapeningsdoorsnede [mm²].
        /// As,min = max(0.10·|NEd|/fyd ; 0.002·Ac)
        /// </summary>
        /// <param name="nEd">Rekenwaarde normaalkracht [kN], positief = druk</param>
        /// <param name="fyd">Rekenweerstand staal [N/mm²]</param>
        /// <param name="ac">Oppervlak betonnen doorsnede [mm²]</param>
        public static double AsMin(double nEd, double fyd, double ac) =>
            Math.Max(0.10 * Math.Abs(nEd) * 1000.0 / fyd, 0.002 * ac);

        // ================================================================
        // §9.5.2 (3) – Maximale wapeningsdoorsnede
        // As,max = 0.04·Ac  buiten lasnaden
        // As,max = 0.08·Ac  ter plaatse van lasnaden (NL NB: toegestaan)
        // ================================================================

        /// <summary>
        /// §9.5.2 (3) Maximale wapeningsdoorsnede buiten lasnaden [mm²].
        /// As,max = 0.04·Ac
        /// </summary>
        public static double AsMax(double ac) => 0.04 * ac;

        /// <summary>
        /// Alias voor AsMax – compatibel met ConstructieveModellen.GetAsMax.
        /// </summary>
        public static double GetAsMax(double ac) => AsMax(ac);

        /// <summary>
        /// §9.5.2 (3) Maximale wapeningsdoorsnede ter plaatse van lasnaden [mm²].
        /// As,max = 0.08·Ac
        /// </summary>
        public static double AsMaxLasnade(double ac) => 0.08 * ac;

        // ================================================================
        // §9.5.2 (4) – Minimaal aantal staven
        // Rechthoekige doorsnede: n ≥ 4
        // Ronde doorsnede:        n ≥ 6
        // Geen nationale bijlage-afwijkingen.
        // ================================================================

        /// <summary>
        /// §9.5.2 (4) Minimaal aantal langsstaven in rechthoekige doorsnede [-].
        /// </summary>
        public static int AantalStavenMinRechthoek => 4;

        /// <summary>
        /// §9.5.2 (4) Minimaal aantal langsstaven in ronde doorsnede [-].
        /// </summary>
        public static int AantalStavenMinRond => 6;

        /// <summary>
        /// §9.5.2 (1) + (4) Gecombineerde minimale wapeningsdoorsnede op basis van minimale staafdiameter
        /// en minimaal aantal staven voor een rechthoekige doorsnede [mm²].
        /// As,min,(1+4) = n_min · π · Ø_min² / 4
        /// </summary>
        /// <param name="nb">Nationale bijlage (bepaalt Ø_min via §9.5.2(1))</param>
        public static double AsMin952_1_en_4(NationaleBijlageEnum? nb = NationaleBijlageEnum.NL)
        {
            double dMin = DiameterMinLangs(nb);
            return AantalStavenMinRechthoek * Math.PI * dMin * dMin / 4.0;
        }

        // ================================================================
        // §9.5.3 – Dwarskrachtwapening (beugels)
        // Maximale hartsafstand beugels: s_cl,max
        // EU:  min(20·Ø_lang ; min(b,h) ; 400 mm)
        // NL NB: idem
        // ================================================================

        /// <summary>
        /// §9.5.3 (1) Maximale hartafstand beugels [mm].
        /// s_cl,max = min(20·Ø_lang ; min(b,h) ; 400)
        /// </summary>
        /// <param name="diameterLangs">Diameter langswapening [mm]</param>
        /// <param name="breedte">Breedte doorsnede b [mm]</param>
        /// <param name="hoogte">Hoogte doorsnede h [mm]</param>
        public static double BeugelsHartAfstandMax(double diameterLangs, double breedte, double hoogte) =>
            Math.Min(Math.Min(20.0 * diameterLangs, Math.Min(breedte, hoogte)), 400.0);

        /// <summary>
        /// §9.5.3 (3) Minimale diameter beugels [mm].
        /// Ø_bgl ≥ max(6 mm ; Ø_lang/4)
        /// </summary>
        public static double BeugelDiameterMin(double diameterLangs) =>
            Math.Max(6.0, diameterLangs / 4.0);
    }
}
