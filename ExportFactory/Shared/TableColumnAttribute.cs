using CommonLibrary;
using ExportFactory.Services;
using MigraDoc.DocumentObjectModel;


namespace ExportFactory.Shared
{
    [Flags]
    public enum WeergaveEnum
    {
        Geen = 0,
        StandaardTabel = 1,
        DraaiTabel = 2,
        AlleTabellen = StandaardTabel | DraaiTabel,
    }

    public class KeyValueMappingModel : BaseEurocodeContext
    {

        [TableColumn("Key")]
        public required string Key { get; set; }
        public required AttributesMapping Mapping { get; set; }

        [TableColumn("Symbool")]
        public string? Symbool { get { return Mapping.Symbol; } }

        public string SymboolHtml { get { return HtmlCreator.MarkdownToHtml(Symbool); } }

        [TableColumn("Omschrijving")]
        public string? Omschrijving { get { return Mapping.Description; } }

        public string OmschrijvingHtml { get { return HtmlCreator.MarkdownToHtml(Omschrijving); } }



        [TableColumn("Norm")]
        public string? Norm { get { return Mapping.Norm; } }

        [TableColumn("Artikel")]
        public string? Artikel { get { return Mapping.Article; } }

        [TableColumn("Formaat")]
        public string? Formaat { get { return Mapping.Format; } }

        [TableColumn("Vergelijking")]
        public string? Vergelijking { get { return Mapping.Vergelijking; } }



        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    var dt = this.ToDataTable(); // maak een DataTable van de context
        //    var mdd = dt.ToMigraDocDocument(objectType: this.GetType(), isPivotTable: isDraaiTabel); // maak een MigraDocDocument
        //    var html = HtmlCreator.GenerateHtmlFromDocument(mdd); // genereer HTML vanuit het MigraDocDocument
        //    return new MarkupString(html); // retourneer als MarkupString
        //}

        protected override void Bereken()
        {
            // Geen specifieke berekening nodig voor deze context
        }

        protected override bool Valideer()
        {
            return true; // altijd akkoord, geen specifieke validatie nodig
        }
    }



    public class AttributesMapping
    {
        public AttributesMapping()
        {

        }




        public AttributesMapping(string? sym, string? desc, string? art = null, string? format = null, string? norm = null, string? vgl = null)
        {

            Symbol = sym;
            Description = desc;

            Article = art;
            Format = format;
            Norm = norm;
            Vergelijking = vgl;

        }


        public string? Symbol { get; set; }
        public string? Description { get; set; } = "";
        public string? Article { get; set; } = "";
        public string? Norm { get; set; } = "EC";
        public string? Vergelijking { get; set; } = null;
        public string? Formule { get; set; } = null;
        public string? Format { get; set; } = null;


        public string? Referentie
        {
            get
            {
                if (Norm != null || Article != null || Vergelijking != null)
                {
                    string referentie = $"conform {Norm}";
                    if (Article != null)
                        referentie += $" art. {Article}";
                    if (Vergelijking != null)
                    {
                        if (Vergelijking.StartsWith('('))
                        {
                            referentie += $" vgl. {Vergelijking}";
                        }
                        else
                        {
                            referentie += $" {Vergelijking}";
                        }
                    }

                    return referentie;
                }
                else return null;

            }
        }

        public EurocodeParagraaf? Paragraaf { get; set; } = null;


    }


    public record TableColumnDto(
        string Label,
        string Value,
        string? Description,
        string? Symbol,
        string? Article,
        string? Unit,
        Formula? Formula
    );




    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class TableColumnAttribute : Attribute
    {
        [Obsolete("vervang door de info uit deze attribute zelf te halen en niet extern")]
        public string? Key { get; init; }


        /// <summary>
        /// Korte label voor de kolomkop of voor het inputveld en dergelijke.
        /// </summary>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Langere omschrijving voor de property. Te gebruiken in tooltips en dergelijke.
        /// </summary>
        public string? Description { get; set; } = null;


        /// <summary>
        /// Optie om een eigen stringformat op te geven. 
        /// Indien leeg (null) gelaten wordt de globale standaard gebruikt voor getalnotatie.
        /// </summary>
        public string? StringFormat { get; set; } = null;


        /// <summary>
        /// Een symbool in Markdown notatie (kan ook LaTeX zijn)
        /// </summary>
        public string? Symbol { get; set; } = null;

        /// <summary>
        /// Mogelijke verwijzing naar een artikel in de norm
        /// </summary>
        public string? Article { get; set; } = null;

        [Obsolete("use formula instead")]
        public string? Formula { get; set; } = null;
        [Obsolete("Use formula class instead")]
        public string? DynamicFormulaProperty { get; set; } = null;

        /// <summary>
        /// De mogelijkheid om een eenheid op te geven die achter het getal komt.
        /// Standaard lege string (geen eenheid).
        /// </summary>
        public string? Unit { get; set; } = "";


        // niet nodig, Formules worden buiten deze attribute om afgehandeld
        // De regel is, dat een property met de naam <prop.Name>Formula gezocht wordt
        // bijvoorbeeld bij een property "ScheurwijdteBerekend" wordt gezocht naar "ScheurwijdteBerekendFormula"
        //public Formula? Vergelijking { get; set; } = null;


        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public WeergaveEnum Weergave { get; set; } = WeergaveEnum.AlleTabellen;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public bool Visible { get; set; } = true;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public double Width { get; set; } = 3.00;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public int Order { get; set; } = -1;


        public TableColumnAttribute()
        {

        }

        public TableColumnAttribute(
            string? label = null,
            string? description = null,
            string? stringFormat = null,
            ParagraphAlignment alignment = ParagraphAlignment.Left,
            bool visible = true,
            WeergaveEnum weergave = WeergaveEnum.AlleTabellen,
            double width = 2.00,
            int order = -1,
            string? key = null,
            string? symbol = null,
            string? article = null
            )
        {
            Key = key;
            Label = label;
            Description = description;
            StringFormat = stringFormat;
            Alignment = alignment;
            Visible = visible;
            Weergave = weergave;
            Width = width;
            Order = order;
            Symbol = symbol;
            Article = article;
        }


    }

}
