using System.ComponentModel;

namespace Eurocode.Grondslagen
{
    /// <summary>
    /// Enum voor het gebruik van nationale bijlage.
    /// Indien geen nationale bijlage gebruikt wordt van wordt de standaard Europese norm toepast.
    /// </summary>
    public enum NationaleBijlageEnum
    {

        [Description("(eu) CEN")]
        EU = 0,
        [Description("(nl) Nederland")]
        NL = 1,
        [Description("(be) Belgie")]
        BE = 2,
        [Description("(de) Deutschland")]
        DE = 3,

    }
}
