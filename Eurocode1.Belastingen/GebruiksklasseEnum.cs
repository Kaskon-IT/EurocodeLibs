using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public enum GebruiksklasseEnum
    {
        [Description("A : gemeenschappelijke trappen")] A_gemeenschappelijke_trappen = 11,
        [Description("B : kantoorgebouwen")] B_kantoorgebouwen = 21,
        [Description("C1 : bijeenkomstgebouwen - gebieden met tafels")] C1_bijeenkomstgebouwen_tafels = 31,
        [Description("C2 : bijeenkomstgebouwen - gebieden met vaste stoelen")] C2_bijeenkomstgebouwen_vaste_stoelen = 32,
        [Description("C3 : bijeenkomstgebouwen - gebieden zonder obstakels")] C3_bijeenkomstgebouwen_zonder_obstakels = 33,
        [Description("C4 : bijeenkomstgebouwen - gebieden met fysieke activiteiten")] C4_bijeenkomstgebouwen_fysieke_activiteiten = 34,
        [Description("C5 : bijeenkomstgebouwen - gebieden voor grote menigtes")] C5_bijeenkomst_grote_menigtes = 35,
        [Description("D1 : winkelruimte - kleinhandel")] D1_winkelruimte_kleinhandel = 41,
        [Description("D1 : winkelruimte - warenhuizen")] D2_winkelruimte_warenhuizen = 42,
        [Description("E1 : opslag - winkels")] E1_opslag_winkels = 51,
        [Description("E1 : opslag - bibliotheken")] E1_opslag_bibliotheken = 52,
        [Description("E1 : opslag - overige")] E1_opslag_overige = 53,
        [Description("E2 : industrieel gebruik")] E2_industrieel_gebruik = 54,
    }
}
