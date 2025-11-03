using CommonLibrary;
using ExportFactory.Services;


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






}
