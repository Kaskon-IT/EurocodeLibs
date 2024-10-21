using System.ComponentModel;

namespace Eurocode.BetonConstructies
{




    [Flags]
    public enum MilieuklasseEnum
    {
        [Description("Geen aantasting")]
        X0 = 1,

        [Description("Aantasting door carbonatatie (Carbonisation), droog of blijvend nat")]
        XC1 = 2,
        [Description("Aantasting door carbonatatie (Carbonisation), nat, zelden droog")]
        XC2 = 4,
        [Description("Aantasting door carbonatatie (Carbonisation), matig vochtig")]
        XC3 = 8,
        [Description("Aantasting door carbonatatie (Carbonisation), wisseld nat en droog")]
        XC4 = 16,

        [Description("Aantasting door dooizouten (De-icing salts), matig vochtig")]
        XD1 = 32,
        [Description("Aantasting door dooizouten (De-icing salts), nat zelden droog")]
        XD2 = 64,
        [Description("Aantasting door dooizouten (De-icing salts), wisselend nat en droog")]
        XD3 = 128,

        [Description("Aantasting door zeewater (Seawater), blootgesteld aan zouten, maar niet direct in contact met zeewater")]
        XS1 = 512,
        [Description("Aantasting door zeewater (Seawater), blijvend onder water")]
        XS2 = 1024,
        [Description("Aantasting door zeewater (Seawater), getijdenzone, spat- en stuifzone")]
        XS3 = 2048,

        [Description("Aantasting door vorst en dooiwisselingen (Frost), deels verzadigd met water, zonder dooizouten")]
        XF1 = 4096,
        [Description("Aantasting door vorst en dooiwisselingen (Frost), deels verzadigd met water, met dooizouten")]
        XF2 = 8192,
        [Description("Aantasting door vorst en dooiwisselingen (Frost), verzadigd met water, zonder dooizouten")]
        XF3 = 16384,
        [Description("Aantasting door vorst en dooiwisselingen (Frost), verzadigd met water, met dooizouten")]
        XF4 = 32768,

        [Description("Chemische aantasting (Aggresive), aantasting door bijvoorbeeld: boorzuur, , creosoot, kresol, visolie, varkensvet, glycerine, bier, bleekwater")]
        XA1 = 65536,
        [Description("Chemische aantasting (Aggresive), aantasting door bijvoorbeeld: azijnzuur, carbolzuur, melkzuur, mierenzuur, amandelolie, kokosolie, terpentijn, fenol, glucose, urine, wei")]
        XA2 = 131072,
        [Description("Chemische aantasting (Aggresive), citroenzuur, humuszuur, zoutzuur, zwavelzuur, kuilvoer, cider, appelwijn, mest, waterstofsulfide, vruchtensap")]
        XA3 = 262144
    }






}
