using System.ComponentModel;

namespace Eurocode.Grondslagen
{
    /// <summary>
    /// 2.3 (1) De ontwerplevensduur behoort te zijn vastgelegd.
    /// Keuze voor het ontwerplevensduur in jaren volgens tabel 2.1
    /// </summary>
    /// <Opmerking>Keuze is aangevuld met optie 75 jaar, als gevolg van Eurocode2 - tabel 4.3N - Constructieve classificatie.</Opmerking>
    public enum OntwerpLevensduurEnum
    {
        [Description("5 jaar")]
        Vijf = 5,
        [Description("15 jaar")]
        Vijftien = 15,
        [Description("50 jaar")]
        Vijftig = 50,
        [Description("75 jaar")]
        VijfEnZeventig = 75,
        [Description("100 jaar")]
        Honderd = 100,
    }
}
