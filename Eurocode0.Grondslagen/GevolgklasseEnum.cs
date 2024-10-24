namespace Eurocode.Grondslagen
{
    /// <summary>
    /// B3.1 Gevolgklassen
    /// (1) Ten behoeve van de betrouwbaarheidsdifferentiatie, mogen gevolgklassen (CC), zoals gegeven in tabel B1, 
    /// worden gedefinieerd door het beschouwen van de gevolgen van bezwijken of het slecht functioneren van de
    /// constructie.
    /// </summary>
    /// <remarks>CC is een afkorting voor Consequences Classes (gevolgklassen)</remarks>
    public enum GevolgklasseEnum
    {
        /// <summary>
        /// Geringe gevolgen ten aanzien van het verlies van mensenlevens, of kleine of verwaarloosbare economische gevolgen, sociale gevolgen of 
        /// gevolgen voor de omgeving.
        /// </summary>
        /// <remarks>
        /// Voorbeeld: Gebouwen voor de landbouw waar mensen normaal niet verblijven (bijv. opslagschuren, tuinbouwkassen)
        /// </remarks>
        CC1 = 101,
        CC1a = 102,
        CC1b = 103,

        /// <summary>
        /// Middelmatige gevolgen ten aanzien van het verlies van mensenlevens, aanzienlijke economische gevolgen, sociale gevolgen of gevolgen voor de omgeving.
        /// </summary>
        /// <remarks>
        /// Voorbeeld: Woon- en kantoorgebouwen.
        /// </remarks>
        CC2 = 201,
        CC2a = 202,
        CC2b = 203,

        /// <summary>
        /// Grote gevolgen ten aanzien van het verlies van mensenlevens, of zeer grote economische gevolgen, sociale gevolgen of gevolgen voor de omgeving.
        /// </summary>
        /// <remarks>
        /// Voorbeeld: Tribunes, openbare gebouwen waarbij de gevolgen van het bezwijken groot zijn.
        /// </remarks>
        CC3 = 301
    }




}
