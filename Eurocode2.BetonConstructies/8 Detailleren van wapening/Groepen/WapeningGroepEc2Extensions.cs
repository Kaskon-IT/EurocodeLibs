using CommonLibrary.Modelling;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// EC2-specifieke detailleringsregels voor <see cref="WapeningGroep"/>
    /// (de geometrie zelf leeft in CommonLibrary.Modelling).
    /// </summary>
    public static class WapeningGroepEc2Extensions
    {
        /// <summary> Equivalente diameter Øn = Ø·√n ≤ 55 mm bij bundels (§8.9.1). </summary>
        public static double EquivalenteDiameter(this WapeningGroep groep)
        {
            var n = 1;
            if (groep.Tussenruimte() == 0)
                n = 2;

            return Math.Min(groep.Diameter * Math.Sqrt(n), 55);
        }
            


        public static double BuigdoornDiameter(this WapeningGroep groep) =>
            groep.Buigstralen?.Length > 0
                ? groep.Buigstralen.Max() * 2 
                : 0;

        /// <summary>
        /// Minimale vrije tussenruimte tussen staven volgens §8.2:
        /// max(k1·Ø; dg + k2; 20 mm) met k1 = 1, k2 = 5 mm (NL-bijlage).
        /// </summary>
        public static double MinimaleTussenruimte(this WapeningGroep groep, double dg) =>
            Math.Max(Math.Max(groep.Diameter, dg + 5), 20);

        /// <summary> Vrije tussenruimte tussen de staafposities [mm]. </summary>
        public static double Tussenruimte(this WapeningGroep groep) =>
            groep.WerkelijkeHartOpHart > 0
                ? groep.WerkelijkeHartOpHart - groep.Diameter
                : double.PositiveInfinity;

        public static bool TussenruimteVoldoet(this WapeningGroep groep, double dg) =>
            groep.Tussenruimte() >= groep.MinimaleTussenruimte(dg);
    }
}
