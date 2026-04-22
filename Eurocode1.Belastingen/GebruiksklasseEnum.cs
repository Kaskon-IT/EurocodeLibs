using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eurocode.Belastingen
{
    public enum GebruiksklasseEnum
    {
        [Description("A : gemeenschappelijke vloeren, trappenhuizen en balkons")]
        [Display(Name = "A : gemeenschappelijke vloeren, trappenhuizen en balkons", ShortName = "A")]
        A_gemeenschappelijke_vloeren = 11,

        [Description("A : woongebouwen - vloeren")]
        [Display(Name = "A : woongebouwen - vloeren", ShortName = "A")]
        A_woongebouwen_vloeren = 12,

        [Description("A : woongebouwen - trappen")]
        [Display(Name = "A : woongebouwen - trappen", ShortName = "A")]
        A_woongebouwen_trappen = 13,

        [Description("A : woongebouwen - ontsluitingswegen")]
        [Display(Name = "A : woongebouwen - ontsluitingswegen", ShortName = "A")]
        A_woongebouwen_ontsluitingswegen = 14,

        [Description("A : woongebouwen - balkons")]
        [Display(Name = "A : woongebouwen - balkons", ShortName = "A")]
        A_woongebouwen_balkons = 15,

        [Description("A : niet-gemeenschappelijke vloeren")]
        [Display(Name = "A : niet-gemeenschappelijke vloeren", ShortName = "A")]
        A_niet_gemeenschappelijk_vloeren = 16,

        [Description("A : niet-gemeenschappelijke trappen")]
        [Display(Name = "A : niet-gemeenschappelijke trappen", ShortName = "A")]
        A_niet_gemeenschappelijk_trappen = 17,

        [Description("A : niet-gemeenschappelijke balkons")]
        [Display(Name = "A : niet-gemeenschappelijke balkons", ShortName = "A")]
        A_niet_gemeenschappelijk_balkons = 18,


        [Description("B : kantoorgebouwen")]
        [Display(Name = "B : kantoorgebouwen", ShortName = "B")]
        B_kantoorgebouwen = 21,


        [Description("C1 : bijeenkomstgebouwen - gebieden met tafels")]
        [Display(Name = "C1 : bijeenkomstgebouwen - gebieden met tafels", ShortName = "C1")]
        C1_bijeenkomstgebouwen_tafels = 31,
        [Description("C2 : bijeenkomstgebouwen - gebieden met vaste stoelen")]
        [Display(Name = "C2 : bijeenkomstgebouwen - gebieden met vaste stoelen", ShortName = "C2")]
        C2_bijeenkomstgebouwen_vaste_stoelen = 32,
        [Description("C3 : bijeenkomstgebouwen - gebieden zonder obstakels")]
        [Display(Name = "C3 : bijeenkomstgebouwen - gebieden zonder obstakels", ShortName = "C3")]
        C3_bijeenkomstgebouwen_zonder_obstakels = 33,
        [Description("C4 : bijeenkomstgebouwen - gebieden met fysieke activiteiten")]
        [Display(Name = "C4 : bijeenkomstgebouwen - gebieden met fysieke activiteiten", ShortName = "C4")]
        C4_bijeenkomstgebouwen_fysieke_activiteiten = 34,
        [Description("C5 : bijeenkomstgebouwen - gebieden voor grote menigtes")]
        [Display(Name = "C5 : bijeenkomstgebouwen - gebieden voor grote menigtes", ShortName = "C5")]
        C5_bijeenkomst_grote_menigtes = 35,


        [Description("D1 : winkelruimte - kleinhandel")]
        [Display(Name = "D1 : winkelruimte - kleinhandel", ShortName = "D1")]
        D1_winkelruimte_kleinhandel = 41,
        [Description("D2 : winkelruimte - warenhuizen")]
        [Display(Name = "D2 : winkelruimte - warenhuizen", ShortName = "D2")]
        D2_winkelruimte_warenhuizen = 42,


        [Description("E1 : opslag - winkels")]
        [Display(Name = "E1 : opslag - winkels", ShortName = "E1")]
        E1_opslag_winkels = 51,
        [Description("E1 : opslag - bibliotheken")]
        [Display(Name = "E1 : opslag - bibliotheken", ShortName = "E1")]
        E1_opslag_bibliotheken = 52,
        [Description("E1 : opslag - overige")]
        [Display(Name = "E1 : opslag - overige", ShortName = "E1")]
        E1_opslag_overige = 53,
        [Description("E2 : industrieel gebruik")]
        [Display(Name = "E2 : industrieel gebruik", ShortName = "E2")]
        E2_industrieel_gebruik = 54,
    }
}
