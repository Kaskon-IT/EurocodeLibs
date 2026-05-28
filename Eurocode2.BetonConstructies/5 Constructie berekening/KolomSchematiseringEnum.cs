namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Excentriciteitstype: bepaalt of de kolom bi-axiaal (dubbel) of uni-axiaal
    /// (geschoord uit het vlak van de Y-as) wordt beschouwd.
    /// </summary>
    public enum ExcentriciteitTypeEnum
    {
        /// <summary>Dubbele excentriciteit – X-as én Y-as worden beiden beschouwd.</summary>
        Dubbel = 0,

        /// <summary>Geschoord uit het vlak (Y-as) – alleen X-as maatgevend voor buiging.</summary>
        GeschoordUitHetVlak = 1,
    }

    public enum KolomBelastingSchemaEnum
    {
        Geschoord,
    }

    public enum KolomTypeEnum
    {
        /// <summary>Stalen kolom: geen knik, dus geen schematisering nodig.</summary>
        Kolom = 0,
        /// <summary>Betonnen kolom: knik mogelijk, dus schematisering vereist.</summary>
        Wand = 1,
    }

    public enum KolomVormEnum
    {
        /// <summary>
        /// Rechthoekige doorsnede, met B en H. 
        /// </summary>
        Rechthoek = 0,

        /// <summary>
        /// Vierkante kolommen, waarbij de breedte B gelijk aan H is.
        /// </summary>
        Vierkant = 1,

        /// <summary>
        /// Ellipsvormige doorsnede, met B en H.
        /// </summary>
        Ellips = 2,
        /// <summary>
        /// Ronde kolommen, waarbij de diameter D = B = H is.
        /// </summary>
        Cirkel = 3
    }

    /// <summary>
    /// Opleggingsschema van een kolom conform EC2 Tabel 5.1.
    /// Bepaalt de effectieve kniklengte l₀ = β · L.
    /// </summary>
    public enum KolomSchematiseringEnum
    {
        /// <summary>Geschoord, beide uiteinden ingeklemd: β = 0.5 → l₀ = 0.5·L</summary>
        GeschoordBeideIngeklemd = 0,

        /// <summary>Geschoord, één uiteinde ingeklemd, één vrij roteerbaar: β = 0.7 → l₀ = 0.7·L</summary>
        GeschoordEénIngeklemdEénRoteerbaar = 1,

        /// <summary>Geschoord, beide uiteinden vrij roteerbaar (pendelkolom): β = 1.0 → l₀ = 1.0·L</summary>
        GeschoordBeideRoteerbaar = 2,

        /// <summary>Ongeschoord, beide uiteinden ingeklemd: β ≈ 1.2 → l₀ = 1.2·L</summary>
        OngeschoordBeideIngeklemd = 3,

        /// <summary>Ongeschoord, één uiteinde ingeklemd, één vrij roteerbaar: β = 2.0 → l₀ = 2.0·L (console)</summary>
        OngeschoordConsole = 4,
    }

    public static class KolomSchematiseringExtensions
    {
        /// <summary>Effectieve kniklengte β-factor conform EC2 Tabel 5.1.</summary>
        public static double Beta(this KolomSchematiseringEnum schema) => schema switch
        {
            KolomSchematiseringEnum.GeschoordBeideIngeklemd           => 0.5,
            KolomSchematiseringEnum.GeschoordEénIngeklemdEénRoteerbaar => 0.7,
            KolomSchematiseringEnum.GeschoordBeideRoteerbaar          => 1.0,
            KolomSchematiseringEnum.OngeschoordBeideIngeklemd         => 1.2,
            KolomSchematiseringEnum.OngeschoordConsole                => 2.0,
            _                                                          => 1.0,
        };

        public static string Omschrijving(this KolomSchematiseringEnum schema) => schema switch
        {
            KolomSchematiseringEnum.GeschoordBeideIngeklemd            => "Geschoord – beide ingeklemd (β=0.5)",
            KolomSchematiseringEnum.GeschoordEénIngeklemdEénRoteerbaar => "Geschoord – 1× ingeklemd (β=0.7)",
            KolomSchematiseringEnum.GeschoordBeideRoteerbaar           => "Geschoord – beide roteerbaar (β=1.0)",
            KolomSchematiseringEnum.OngeschoordBeideIngeklemd          => "Ongeschoord – beide ingeklemd (β=1.2)",
            KolomSchematiseringEnum.OngeschoordConsole                 => "Ongeschoord – console (β=2.0)",
            _                                                           => schema.ToString(),
        };
    }
}
