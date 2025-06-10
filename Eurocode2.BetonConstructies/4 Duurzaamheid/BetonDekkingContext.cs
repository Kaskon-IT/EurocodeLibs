using CommonLibrary;
using Eurocode.Grondslagen;
using ExportFactory.Extensions;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public partial class BetonDekkingContext : BaseEurocodeContext
    {



        /// <summary>
        /// Referentie naar de context uit de Eurocode1
        /// </summary>
        public GrondslagenContext Grondslagen;

        public BetonDekkingContext()
        {
            Grondslagen = new();
            Beton = new();
            Constructieklasse = new(this, Beton);
        }

        public BetonDekkingContext(GrondslagenContext grondslagen, BetonContext beton)
        {
            Grondslagen = grondslagen;
            Beton = beton;
            Constructieklasse = new(this, Beton);
            BerekenEnValideer();
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
            SelectedMilieuklassen = context.Milieuklassen;
            GrootsteKorrelDiameter = context.GrootsteKorrelDiameter;
            Constructieklasse = new(this, Beton);

        }

        // verplaatst naar Grondslagen
        //public NationaleBijlageEnum NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        public Constructieklasse Constructieklasse { get; private set; }





        [TableColumn("constructieklasse", Order = 20, Weergave = WeergaveEnum.DraaiTabel)]
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
        [TableColumn("positie", order: 0, Weergave = WeergaveEnum.StandaardTabel)]
        public string Naam { get; set; } = "Bovenzijde";


        /// <summary>
        /// De toegepaste dekking (c,toe) in mm.
        /// </summary>


        private double _dekkingToe = 20;
        public double DekkingToe
        {
            get => _dekkingToe;
            set
            {
                if (_dekkingToe != value)
                {
                    _dekkingToe = value;
                    OnPropertyChanged(nameof(DekkingToe));
                    BerekenEnValideer();
                }


            }
        }


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
        [TableColumn("plaatgeometrie?", headerTextPivot: "-", Order = 2, Weergave = WeergaveEnum.DraaiTabel)]
        public bool IsPlaatGeometrie
        {
            get => _isPlaatGeometrie;
            set { _isPlaatGeometrie = value; BerekenEnValideer(); }
        }

        /// <summary>
        /// Indien specifieke kwaliteitsbeheersing (bijvoorbeeld bij prefab beton) vermindering met 1 op constructieklasse.
        /// </summary>
        [TableColumn("kwaliteitsbeheersing?", Order = 3, Weergave = WeergaveEnum.DraaiTabel)]
        public bool IsKwaliteitsBeheersing
        {
            get => _isKwaliteitsBeheersing;
            set { _isKwaliteitsBeheersing = value; BerekenEnValideer(); }
        }

        /// <summary>
        /// De milieuklasse(n) hebben invloed op de minimale dekking duurzaamheid (c,min,dur)
        /// Bij meerdere milieuklassen wordt de maatgevende dekking bepaald.
        /// </summary>
        //public IEnumerable<MilieuklasseEnum> Milieuklassen { get; set; } = [MilieuklasseEnum.XC3];


        public IEnumerable<MilieuklasseEnum> Milieuklassen =>
            SelectedMilieuklassen.Any() ? SelectedMilieuklassen : [MilieuklasseEnum.X0];

        public IEnumerable<Eurocode.BetonConstructies.MilieuklasseEnum> SelectedMilieuklassen
        {
            get => _selectedMilieuklassen;
            set
            {


                _selectedMilieuklassen = value ?? [MilieuklasseEnum.X0]; // mag niet leeg gelaten worden! 

                //
                //var mk = this.Milieuklassen;
                BerekenEnValideer(); // Roep de validatie aan

            }
        }

        [TableColumn("milieuklasse", Order = 1)]
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
            get => _dekkingNom;
            private set => _dekkingNom = value;
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


        private double _wapeningDiameterGelijkwaardig = 10;
        private double _dekkingNom;
        private bool _isPlaatGeometrie;
        private bool _isKwaliteitsBeheersing;
        private IEnumerable<MilieuklasseEnum> _selectedMilieuklassen = [MilieuklasseEnum.X0];
        private double _grootsteKorrelDiameter = 31.5;

        /// <summary>
        /// De diameter van de staaf of gelijkwaardige diameter van de staafbundel.
        /// </summary>
        [TableColumn(
            headerText: "Ø~eq~",
            headerTextPivot: "Ø~eq~\tgelijkwaardige diameter",
            Order = 24, StringFormat = "Ø0.##")]
        public double WapeningDiameterGelijkwaardig
        {
            get => _wapeningDiameterGelijkwaardig;
            set
            {
                if (_wapeningDiameterGelijkwaardig != value)
                {
                    _wapeningDiameterGelijkwaardig = value;
                    BerekenEnValideer();
                }
            }
        }

        /// <summary>
        /// De grootste korreldiameter. Heeft invloed op de dekking c,min,b 
        /// </summary>
        [TableColumn(
            headerText: "korrel",
            headerTextPivot: "Grootste korrel",
            Order = 5, StringFormat = "≤ 0 mm", Weergave = WeergaveEnum.DraaiTabel)]
        public double GrootsteKorrelDiameter
        {
            get => _grootsteKorrelDiameter;
            set
            {
                if (_grootsteKorrelDiameter != value)
                {
                    _grootsteKorrelDiameter = value;
                    BerekenEnValideer();
                }
            }
        }

        public string GrootsteKorrelDiameterUserFriendlyName
        {
            get
            {
                if (GrootsteKorrelDiameter <= 32)
                    return "≤ 32 mm";
                else
                    return "> 32 mm";
            }
        }

        public BetonAfwerkingOppervlakEnum? BetonAfwerkingOppervlak { get; set; } = BetonAfwerkingOppervlakEnum.Glad;



        public bool GestortTegenBestaandBeton { get; set; } // art. 4.4.1.2(9)
        public BetonStortOndergrondEnum? BetonStortOndergrond { get; set; } = BetonStortOndergrondEnum.GladdeBekistingOfNvt;




        /// <summary>
        /// keuzes voor ondergrond van de betonstort. 
        /// </summary>
        public enum BetonStortOndergrondEnum { [Description("Gladde bekisting of n.v.t.")] GladdeBekistingOfNvt = 1, [Description("Werkvloer")] Werkvloer = 2, [Description("Op of tegen de grond")] OpOfTegenGrond = 3 }

        /// <summary>
        /// keuze voor afwerking van de oppervlakte
        /// </summary>
        public enum BetonAfwerkingOppervlakEnum { [Description("Glad")] Glad = 1, [Description("Nabewerkt of oneffen")] NabewerktOnEffen = 2 }

        public override bool IsAkkoord()
        {
            return BerekenEnValideer();
        }

        public override string? ToString()
        {
            List<string> results = [];

            results.Add($"c~nom~ = c~min~ + Δ~c,dev~ = {DekkingMin} + {DekkingToeslagUitvoeringsToleranties} = {DekkingNom: 0 mm}");
            results.Add($"{ConstructieklasseUserFriendlyName}");
            results.Add($"{MilieuklassenUserFriendlyName}");
            if (IsKwaliteitsBeheersing) results.Add($"kwaliteitsbeheersing");
            if (IsPlaatGeometrie) results.Add($"plaatgeometrie");
            if (GrootsteKorrelDiameter <= 32)
                results.Add("korrel ≤ 32mm");
            else
                results.Add("korrel > 32mm");
            results.Add($"c~toe~ = {DekkingToe: 0 mm}");


            return string.Join(", ", results);
        }



        [Flags]
        public enum ToStringTypeEnum
        {
            [Description("c~nom~ = c~min~ + Δ~c,dev~ = {DekkingMin} + {DekkingToeslagUitvoeringsToleranties} = {DekkingNom: 0 mm}")]
            Cnom = 1,
            [Description("c~min~ = {DekkingMin}")]
            Cmin = 2,
            [Description("c~toe~ = {DekkingToe: 0 mm}")]
            Ctoe = 4,
            [Description("kwaliteitsbeheersing")]
            Kwaliteitsbeheersing = 8,
            [Description("plaatgeometrie")]
            Plaatgeometrie = 16,
            [Description("korrel ≤ 32mm")]
            Korrel32 = 32,
            [Description("milieuklasse")]
            Milieuklasse = 64,
            [Description("constructieklasse")]
            Constructieklasse = 128,
        }

        public List<string>? ToStrings(ToStringTypeEnum stringType)
        {
            List<string> results = [];
            if (stringType.HasFlag(ToStringTypeEnum.Cnom))
                results.Add($"c~nom~ = c~min~ + Δ~c,dev~ = {DekkingMin} + {DekkingToeslagUitvoeringsToleranties} = {DekkingNom: 0 mm}");
            if (stringType.HasFlag(ToStringTypeEnum.Cmin))
                results.Add($"c~min~ = {DekkingMin}");
            if (stringType.HasFlag(ToStringTypeEnum.Ctoe))
                results.Add($"c~toe~ = {DekkingToe: 0 mm}");
            if (stringType.HasFlag(ToStringTypeEnum.Kwaliteitsbeheersing))
            {
                if (IsKwaliteitsBeheersing)
                    results.Add($"✅ kwaliteitsbeheersing");

            }
            if (stringType.HasFlag(ToStringTypeEnum.Plaatgeometrie))
            {
                if (IsPlaatGeometrie)
                    results.Add($"✅ plaatgeometrie");

            }
            if (stringType.HasFlag(ToStringTypeEnum.Korrel32))
            {
                if (GrootsteKorrelDiameter <= 32)
                    results.Add($"✅ korreldiameter ≤ 32mm");
                else
                    results.Add($"korreldiameter > 32mm");
            }
            if (stringType.HasFlag(ToStringTypeEnum.Milieuklasse))
                results.Add($"milieuklasse: {MilieuklassenUserFriendlyName}");
            if (stringType.HasFlag(ToStringTypeEnum.Constructieklasse))
                results.Add($"constructieklasse: {ConstructieklasseUserFriendlyName}");

            return results;
        }


        protected override void Bereken()
        {
            this.Constructieklasse = new Constructieklasse(this, this.Beton);
            DekkingNom = this.GetDekkingNominaal();
            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {

            // Waarschuwingen (niet akkoord, aktie vereist)
            if (DekkingNom > DekkingToe)
            {
                AddMeldingWaarschuwing("<b>nominale dekking c<sub>nom</sub> is groter dan toegepaste dekking c<sub>toe</sub></b>");
                return false;
            }

            // Neutrale opmerkingen
            if (WapeningDiameterGelijkwaardig == 0)
            {
                AddMeldingOpmerking("voor de nominale dekking c<sub>nom</sub> tenmiste Ø<sub>k</sub> + 5mm aanhouden");
            }

            return true;

        }


        public override MarkupString ToHtml(bool isDraaiTabel = true)
        {
            return this.ToHtmlTable(isDraaiTabel);
        }
    }
}