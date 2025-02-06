using System.ComponentModel;

namespace Eurocode.Grondslagen
{
    /// <summary>
    /// B3.2 (2) Drie betrouwbaarheidsklassen RC1, RC2 en RC3 mogen in één verband worden gezien met de drie 
    /// gevolgklassen CC1, CC2 en CC3.
    /// </summary>
    /// <remarks>RC is een afkorting voor Reliability Classes (betrouwbaarheidsklassen).</remarks>
    public enum BetrouwbaarheidsklasseEnum
    {
        /// <summary>
        /// Reliability Class 1 (betrouwbaarheidsklasse) 
        /// </summary>
        [Description("RC1")]
        RC1 = 1,
        /// <summary>
        /// Reliability Class 2 (betrouwbaarheidsklasse) 
        /// </summary>
        [Description("RC2")]
        RC2 = 2,
        /// <summary>
        /// Reliability Class 3 (betrouwbaarheidsklasse) 
        /// </summary>
        [Description("RC3")]
        RC3 = 3,
    }




}
