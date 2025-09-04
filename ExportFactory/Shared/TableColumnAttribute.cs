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

        public override bool IsAkkoord()
        {
            return true;
        }

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


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class TableColumnAttribute : Attribute
    {
        public string? Key { get; init; }
        public string? HeaderText { get; set; } = null;
        public string? HeaderTextPivot { get; set; } = null;
        public string? StringFormat { get; set; } = null;


        // aanvulling
        public string? Symbol { get; set; } = null;
        public string? Article { get; set; } = null;
        public string? Formula { get; set; } = null;


        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;
        public WeergaveEnum Weergave { get; set; } = WeergaveEnum.AlleTabellen;
        public bool Visible { get; set; } = true;
        public double Width { get; set; } = 3.00;
        public int Order { get; set; } = -1;


        public TableColumnAttribute()
        {

        }

        public TableColumnAttribute(
            string? headerText = null,
            string? headerTextPivot = null,
            string? stringFormat = null,
            ParagraphAlignment alignment = ParagraphAlignment.Left,
            bool visible = true,
            WeergaveEnum weergave = WeergaveEnum.AlleTabellen,
            double width = 2.00,
            int order = -1,
            string? key = null,
            string? symbol = null,
            string? article = null,
            string? formula = null
            )
        {
            Key = key;
            HeaderText = headerText;
            HeaderTextPivot = headerTextPivot;
            StringFormat = stringFormat;
            Alignment = alignment;
            Visible = visible;
            Weergave = weergave;
            Width = width;
            Order = order;
            Symbol = symbol;
            Article = article;
            Formula = formula;
        }


    }

}
