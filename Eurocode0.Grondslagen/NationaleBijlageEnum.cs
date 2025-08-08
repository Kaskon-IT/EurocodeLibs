using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eurocode.Grondslagen
{
    /// <summary>
    /// Enum voor het gebruik van nationale bijlage.
    /// Indien geen nationale bijlage gebruikt wordt van wordt de standaard Europese norm toepast.
    /// </summary>
    public enum NationaleBijlageEnum
    {

        [Description("(eu) CEN")]
        [Display(Name = "CEN-EN (eu)", ShortName = "CEN")]
        EU = 0,
        [Description("(nl) Nederland")]
        [Display(Name = "NEN-EN (nl)", ShortName = "NL")]
        NL = 1,
        [Description("(be) Belgie")]
        [Display(Name = "NBN-EN (be)", ShortName = "BE")]
        BE = 2,
        [Description("(de) Deutschland")]
        [Display(Name = "DIN-EN (de)", ShortName = "DE")]
        DE = 3,

    }
}
