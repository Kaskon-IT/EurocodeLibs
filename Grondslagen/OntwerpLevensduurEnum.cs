using System.ComponentModel;

namespace Eurocode.Grondslagen
{
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
