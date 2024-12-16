using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public enum GebruiksklasseEnum
    {
        [Description("A : Gemeenschappelijke trappen")] A_gemeenschappelijke_trappen = 11,
        [Description("B : Kantoorgebouwen")] B_kantoorgebouwen = 21,
        [Description("C1 : Bijeenkomstgebouwen - gebieden met tafels")] C1_bijeenkomstgebouwen_tafels = 31,
        [Description("C2 : Bijeenkomstgebouwen - gebieden met vaste stoelen")] C2_bijeenkomstgebouwen_vaste_stoelen = 32,
        [Description("C3 : Bijeenkomstgebouwen - gebieden zonder obstakels")] C3_bijeenkomstgebouwen_zonder_obstakels = 33,
        [Description("C4 : Bijeenkomstgebouwen - gebieden met fysieke activiteiten")] C4_bijeenkomstgebouwen_fysieke_activiteiten = 34,
        [Description("C5 : Bijeenkomstgebouwen - gebieden voor grote menigtes")] C5_bijeenkomst_grote_menigtes = 35,
        [Description("D1 : Winkelruimte - kleinhandel")] D1_winkelruimte_kleinhandel = 41,
        [Description("D1 : Winkelruimte - warenhuizen")] D2_winkelruimte_warenhuizen = 42,
        [Description("E1 : Opslag - winkels")] E1_opslag_winkels = 51,
        [Description("E1 : Opslag - bibliotheken")] E1_opslag_bibliotheken = 52,
        [Description("E1 : Opslag - overige")] E1_opslag_overige = 53,
        [Description("E2 : Industrieel gebruik")] E2_industrieel_gebruik = 54,
    }
}
