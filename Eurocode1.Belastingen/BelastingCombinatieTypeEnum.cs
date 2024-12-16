using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public enum BelastingCombinatieTypeEnum
    {
        [Description("Fundamenteel (6.10a)")]
        Fundamenteel_A = 1001,
        [Description("Fundamenteel (6.10b)")]
        Fundamenteel_B = 1002,


        [Description("Brand (6.11b)")]
        Brand = 1101,

        [Description("Aardbeving (6.12b)")]
        Aardbeving = 1201,

        [Description("Karakteristiek (6.14b)")]
        Karakteristiek = 1401,

        [Description("Frequent (6.15b)")]
        Frequent = 1501,

        [Description("Quasi-blijvend (6.16b)")]
        QuasiBlijvend = 1601,

        [Description("Blijvend")]
        Blijvend = 9999,

    }







}
