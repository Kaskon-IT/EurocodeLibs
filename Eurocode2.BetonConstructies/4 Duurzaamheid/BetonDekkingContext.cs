using CommonLibrary;
using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public partial class BetonDekkingContext : BaseEurocodeContext
    {

        public override string Heading { get; set; } = "Dekking en duurzaamheid";

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





        [TableColumn("constructieklasse",
            Order = 20,
            Article = "4.4.1.2 (5)"

            )]
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
        [TableColumn("positie", order: 0
            )]
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
        [TableColumn("plaatgeometrie?", description: "-", Order = 2
            )]
        public bool IsPlaatGeometrie
        {
            get => _isPlaatGeometrie;
            set { _isPlaatGeometrie = value; BerekenEnValideer(); }
        }

        /// <summary>
        /// Indien specifieke kwaliteitsbeheersing (bijvoorbeeld bij prefab beton) vermindering met 1 op constructieklasse.
        /// </summary>
        [TableColumn("kwaliteitsbeheersing?", Order = 3
            )]
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

        [TableColumn(Label = "milieuklassen", Article = "4.2 (2)")]
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
        [TableColumn(Label = "nominale dekking",
            Symbol = "<i>c</i><sub>nom</sub>",
            Description = "nominale dekking",
            Article = "4.4.1.1",
            Unit = "mm"
            )]
        public double DekkingNom
        {
            get => _dekkingNom;
            private set => _dekkingNom = value;
        }
        public Formula DekkingNomFormula
        {
            get
            {
                return new()
                {
                    Name = "(4.1)",
                    StaticValue = @"c_{nom} = c_{min} + \Delta c_{dev}",
                    DynamicValue = @$"c_{{nom}} = {DekkingMin} + {DekkingToeslagUitvoeringsToleranties} = {DekkingNom} \;mm"
                };
            }
        }

        /// <summary>
        /// Is de minimumdekking op basis van de milieu-omstandigheden, zie 4.4.1.2 (5)
        /// </summary>
        [TableColumn(Label = "dekking duurzaamheid", Symbol = "<i>c</i><sub>min,dur</sub>", Unit = "mm", Article = "4.4.1.2 (5)")]
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
            Label = "minimale dekking",
            Symbol = "<i>c</i><sub>min</sub>",
            Unit = "mm",
            Description = "minimale dekking art.4.4.1.2",
            Article = "4.4.1.2"
            )]
        public double DekkingMin
        {
            get { return this.GetMinimaleBetondekking(); }
        }
        public Formula DekkingMinFormula
        {
            get
            {
                return new()
                {
                    Name = "(4.2)",
                    StaticValue = @"c_\text{min} = \max \{ c_{\text{min},b} \; ; \; c_{\text{min},dur} + \Delta c_{dur,\gamma} - \Delta c_{dur,st} - \Delta c_{dur,add} \; ; \; 10 \, \}",
                    DynamicValue = $@"c_\text{{min}} = \max \{{ {DekkingMinAanhechting} \; ; \; {DekkingMinDuurzaamheid} + {BetondekkingMinVeiligheidsmarge} - {BetondekkingMinRoestvastStaal} - {BetondekkingMinBescherming} \; ; \; 10  \}} = {DekkingMin}"

                };
            }
        }





        /// <summary>
        /// Minimale dekking tbv aanhechting betonstaal
        /// </summary>
        [TableColumn(Label = "dekking aanhechting",
            Symbol = "<i>c</i><sub>min,b</sub>",
            Unit = "mm",
            Description = "minimumdekking aanhechting op basis van aanhechtingseisen",
            Article = "4.4.1.2 (3)"
            )]
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
        [TableColumn(Label = "toeslag tolerantie",
            Symbol = "Δc<sub>dev</sub>",
            Unit = "mm",
            Description = "Voor het berekenen van de nominale dekking, <i>c</i><sub>nom</sub>, moet de minimumdekking bij het ontwerp zijn vermeerderd om rekening te houden met uitvoeringstoleranties (Δc<sub>dev</sub>).",
            Article = "4.4.1.3 (1)P"

            )]
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
        [TableColumn(Label = "staafdiameter",
            Symbol = "Ø<sub>eq</sub>",
            Description = "gelijkwaardige diameter, gebruikt voor bepalen van de minimumdekking met betrekking tot aanhechting",
            Article = "4.4.1.2 (3)",
            Unit = "mm",
            Order = 24)]
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
            label: "korrelafmeting",
            description: "de nominale maximale korrelafmeting",
            Symbol = "<i>d</i><sub>g</sub>",
            Order = 5,
            Unit = "mm",
            Article = "4.4.1.2 (3)")]
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


        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtmlTable(isDraaiTabel);
        //}
    }
}