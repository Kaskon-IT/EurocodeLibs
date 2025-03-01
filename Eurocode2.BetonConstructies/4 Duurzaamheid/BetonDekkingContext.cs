using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public partial class BetonDekkingContext
    {
        /// <summary>
        /// Referentie naar de context uit de Eurocode1
        /// </summary>
        public GrondslagenContext Grondslagen;

        public BetonDekkingContext()
        {
            Grondslagen = new();
            Beton = new();
        }

        public BetonDekkingContext(GrondslagenContext grondslagen, BetonContext beton)
        {
            Grondslagen = grondslagen;
            Beton = beton;
        }

        public BetonDekkingContext(BetonDekkingContext context)
        {
            Naam = context.Naam;
            Beton = context.Beton;
            Grondslagen = context.Grondslagen;
            IsPlaatGeometrie = context.IsPlaatGeometrie;
            IsKwaliteitsBeheersing = context.IsKwaliteitsBeheersing;
            GestortTegenBestaandBeton = context.GestortTegenBestaandBeton;
            BetonAfwerkingOppervlak = context.BetonAfwerkingOppervlak;
            BetonStortOndergrond = context.BetonStortOndergrond;
            Milieuklassen = context.Milieuklassen;
            GrootsteKorrelDiameter = context.GrootsteKorrelDiameter;

        }

        // verplaatst naar Grondslagen
        //public NationaleBijlageEnum NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        public Constructieklasse Constructieklasse
        {
            get
            {
                return new Constructieklasse(this, this.Beton);
            }
        }

        [TableColumn("Constructieklasse", Order = 20)]
        public string ConstructieklasseUserFriendlyName
        {
            get
            {
                return Constructieklasse.UserFriendlyName;
            }
        }


        /// <summary>
        /// Naam van de betondekking context, bijvoorbeeld 'bovenzijde' of 'onderzijde' 
        /// </summary>
        [TableColumn("Dekking (positie)", order: 0)]
        public string Naam { get; set; } = "Bovenzijde";


        /// <summary>
        /// De toegepaste dekking (c,toe) in mm.
        /// </summary>
        public double DekkingToe { get; set; } = 20;


        /// <summary>
        /// Ontwerplevensduur (Eurocode 1), heeft invloed op de constructieklasse en minimale dekking duurzaamheid (c,min,dur)
        /// </summary>
        public OntwerpLevensduurEnum OntwerpLevensduur { get; set; } = OntwerpLevensduurEnum.Vijftig;



        /// <summary>
        /// Beton eigenschappen, heeft invloed op de constructieklasse en minimale dekking duurzaamheid (c,min,dur)
        /// </summary>
        public BetonContext Beton { get; set; } = new(BetonsterkteklasseEnum.C20_25);



        /// <summary>
        /// Indien plaatgeometrie van toepassing dan een vermindering van 1 op de constructieklasse.
        /// </summary>
        [TableColumn("Plaatgeometrie?", Order = 2)]
        public bool IsPlaatGeometrie { get; set; }

        /// <summary>
        /// Indien specifieke kwaliteitsbeheersing (bijvoorbeeld bij prefab beton) vermindering met 1 op constructieklasse.
        /// </summary>
        [TableColumn("Kwaliteitsbeheersing?", Order = 3)]
        public bool IsKwaliteitsBeheersing { get; set; }

        /// <summary>
        /// De milieuklasse(n) hebben invloed op de minimale dekking duurzaamheid (c,min,dur)
        /// Bij meerdere milieuklassen wordt de maatgevende dekking bepaald.
        /// </summary>
        //public IEnumerable<MilieuklasseEnum> Milieuklassen { get; set; } = [MilieuklasseEnum.XC3];


        public IEnumerable<MilieuklasseEnum> Milieuklassen = [MilieuklasseEnum.XC1];

        public IEnumerable<Eurocode.BetonConstructies.MilieuklasseEnum> SelectedMilieuklassen = [];

        [TableColumn("Milieuklasse", Order = 1)]
        public string MilieuklassenUserFriendlyName
        {
            get
            {
                if (this.Milieuklassen.Any())
                    return string.Join(", ", this.Milieuklassen);
                else return "X0";

            }
        }

        public string UserFriendlyName { get { return this.ToUserFriendlyString(); } }


        // specifieke context voor de dekking


        /// <summary>
        /// De nominale betondekking (c,nom) is de minimale betondekking inclusief uitvoeringstoleranties (Δc,dev)
        /// </summary>
        [TableColumn(
            headerText: "c~nom~ ",
            HeaderTextPivot = "c~nom~\tnominale dekking art. 4.4.1.1",
            Order = 1, StringFormat = "0 mm")]
        public double DekkingNom
        {
            get { return this.GetDekkingNominaal(); }
        }

        /// <summary>
        /// Is de minimumdekking op basis van de milieu-omstandigheden, zie 4.4.1.2 (5)
        /// </summary>
        [TableColumn(
            headerText: "c~min,dur~",
            headerTextPivot: "c~min,dur~\tminimumdekking duurzaamheid art. 4.4.1.2 (5)",
            order: 41, StringFormat = "0 mm")]
        public double DekkingMinDuurzaamheid
        {
            get { return this.GetCminDur(); }
        }

        /// <summary>
        /// 4.4.1.2 Minimale dekking (c,min), moet zorgen voor:
        /// - een veilige overdracht van de aanhechtkrachten (zie ook hoofdstukken 7 en 8)
        /// - de bescherming van het staal tegen corrosie (duurzaamheid)
        /// - voldoende brandwerendheid (zie EN 1992-1-2)
        /// zie 4.4.1.2
        /// </summary>
        [TableColumn(
            headerText: "c~min~",
            headerTextPivot: "c~min~\tminimale dekking art.4.4.1.2",
            order: 39, StringFormat = "0 mm")]
        public double DekkingMin
        {
            get { return this.GetMinimaleBetondekking(); }
        }


        /// <summary>
        /// Minimale dekking tbv aanhechting betonstaal
        /// </summary>
        [TableColumn(
            headerText: "c~min,b~",
            headerTextPivot: "c~min,b~\tminimumdekking aanhechting art. 4.4.1.2 (3)",
            order: 40, StringFormat = "0 mm")]
        public double DekkingMinAanhechting
        {
            get
            {
                return this.GetDekkingBetonstaalMinimaal();
            }
        }




        /// <summary>
        /// Verhoging van de dekking tbv uitvoeringstoleranties (Δc,dev) volgens 4.4.1.3 (1)
        /// </summary>
        [TableColumn(
            headerText: "|Delta|c~dev~",
            headerTextPivot: "|Delta|c~dev~\t toeslag uitvoeringstoleranties art. 4.4.1.3(1)", order: 50, StringFormat = "0 mm")]
        public double DekkingToeslagUitvoeringsToleranties { get { return this.Grondslagen.NationaleBijlage.GetUitvoeringstoleraties(); } }

        /// <summary>
        /// Is een reductie van de minimumdekking bij gebruik van aanvullende bescherming, zie 4.4.1.2 (8)
        /// </summary>
        public const double BetondekkingMinBescherming = 0;

        /// <summary>
        /// Is een aanvullende veiligheidsmarge, zie 4.4.1.2 (6)
        /// </summary>
        public const double BetondekkingMinVeiligheidsmarge = 0;

        /// <summary>
        /// Is een reductie van de minimumdekking bij gebruik van roestvast staal, zie 4.4.1.2 (7)
        /// </summary>
        public const double BetondekkingMinRoestvastStaal = 0;

        /// <summary>
        /// k1 bij oneffen oppervlakken.
        /// </summary>
        public double BetonDekkingFactorK1
        {
            get { return this.GetK1OneffenOppervlakken(); }
        }


        /// <summary>
        /// k2 bij beton direct gestort op of tegen de grond.
        /// </summary>
        public double BetonDekkingFactorK2
        {
            get { return this.GetK2DirectGestortOpOfTegenDeGrond(); }
        }


        /// <summary>
        /// De diameter van de staaf of gelijkwaardige diameter van de staafbundel.
        /// </summary>
        [TableColumn(
            headerText: "Ø~eq~",
            headerTextPivot: "Ø~eq~\tgelijkwaardige diameter",
            Order = 24, StringFormat = "Ø0.##")]
        public double WapeningDiameterGelijkwaardig { get; set; } = 10;

        /// <summary>
        /// De grootste korreldiameter. Heeft invloed op de dekking c,min,b 
        /// </summary>
        [TableColumn(
            headerText: "korrel",
            headerTextPivot: "Grootste korrel",
            Order = 5, StringFormat = "≤ 0 mm")]
        public double GrootsteKorrelDiameter { get; set; } = 31.5;


        public BetonAfwerkingOppervlakEnum? BetonAfwerkingOppervlak { get; set; } = BetonAfwerkingOppervlakEnum.Glad;



        public bool GestortTegenBestaandBeton { get; set; } // art. 4.4.1.2(9)
        public BetonStortOndergrondEnum? BetonStortOndergrond { get; set; } = BetonStortOndergrondEnum.GladdeBekistingOfNvt;


        #region enums voor betondekking

        /// <summary>
        /// keuzes voor ondergrond van de betonstort. 
        /// </summary>
        public enum BetonStortOndergrondEnum { [Description("Gladde bekisting of n.v.t.")] GladdeBekistingOfNvt = 1, [Description("Werkvloer")] Werkvloer = 2, [Description("Op of tegen de grond")] OpOfTegenGrond = 3 }

        /// <summary>
        /// keuze voor afwerking van de oppervlakte
        /// </summary>
        public enum BetonAfwerkingOppervlakEnum { [Description("Glad")] Glad = 1, [Description("Nabewerkt of oneffen")] NabewerktOnEffen = 2 }
        #endregion


    }
}