using System.ComponentModel;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Standaard betonsterkteklassen conform Tabel 3.1
    /// </summary>
    public enum BetonsterkteklasseEnum
    {
        [Description("C12/15")] C12_15 = 1,
        [Description("C16/20")] C16_20 = 2,
        [Description("C20/25")] C20_25 = 3,
        [Description("C25/30")] C25_30 = 4,
        [Description("C30/37")] C30_37 = 5,
        [Description("C35/45")] C35_45 = 6,
        [Description("C40/50")] C40_50 = 7,
        [Description("C45/55")] C45_55 = 8,
        [Description("C50/60")] C50_60 = 9,
        [Description("C55/67")] C55_67 = 10,
        [Description("C60/75")] C60_75 = 11,
        [Description("C70/85")] C70_85 = 12,
        [Description("C80/95")] C80_95 = 13,
        [Description("C90/105")] C90_105 = 14,
        [Description("Eigen opgave")] Eigen_Opgave = 99,

    }

    /// <summary>
    /// Cementklassen
    /// </summary>
    public enum CementklasseEnum
    {
        /// <summary>
        /// Klasse R (Rapid)
        /// </summary>
        [Description("Klasse R (Rapid)")]
        R = 1,
        /// <summary>
        /// Klasse N (Normal)
        /// </summary>
        [Description("Klasse N (Normal)")] N = 2,
        /// <summary>
        /// Klasse S (Slow)
        /// </summary>
        [Description("Klasse S (Slow")] S = 3,
    }


    // context voor betonstaal conform art. 3.2
    public enum BetonStaalKwaliteitEnum
    {
        B500A = 1,
        B500B = 2,
        B500C = 3,
        B400A = 11,
        B400B = 12,
        B400C = 13,
        B600A = 21,
        B600B = 22,
        B600C = 23,
        EigenFyk = 31,
    }

}
