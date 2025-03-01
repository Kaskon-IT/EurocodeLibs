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


    public class AttributesMapping
    {
        public AttributesMapping()
        {

        }

        public AttributesMapping(string? symbol = null, string? description = null, string? article = null, string? format = null)
        {

            Symbol = symbol;
            Description = description;
            Article = article;
            Format = format;
        }


        public string? Symbol { get; set; }
        public string? Description { get; set; } = "";
        public string? Article { get; set; } = "";
        public string? Format { get; set; } = null;



    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class TableColumnAttribute : Attribute
    {
        public string? HeaderText { get; set; } = null;
        public string? HeaderTextPivot { get; set; } = null;
        public string? StringFormat { get; set; } = null;
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;
        public WeergaveEnum Weergave { get; set; } = WeergaveEnum.AlleTabellen;
        public bool Visible { get; set; } = true;
        public double Width { get; set; } = 3.00;
        public int Order { get; set; } = -1;


        public TableColumnAttribute() { }

        public TableColumnAttribute(
            string? headerText = null,
            string? headerTextPivot = null,
            string? stringFormat = null,
            ParagraphAlignment alignment = ParagraphAlignment.Center,
            bool visible = true,
            WeergaveEnum weergave = WeergaveEnum.AlleTabellen,
            double width = 3.00,
            int order = -1)
        {
            HeaderText = headerText;
            HeaderTextPivot = headerTextPivot;
            StringFormat = stringFormat;
            Alignment = alignment;
            Visible = visible;
            Weergave = weergave;
            Width = width;
            Order = order;
        }


    }

}
