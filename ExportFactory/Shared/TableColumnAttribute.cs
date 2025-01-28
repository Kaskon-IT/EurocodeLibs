using MigraDoc.DocumentObjectModel;


namespace ExportFactory.Shared
{

    [AttributeUsage(AttributeTargets.Property)]
    public class TableColumnAttribute : Attribute
    {
        public string? HeaderText { get; set; } = null;
        public string? StringFormat { get; set; } = null;
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;
        public bool Visible { get; set; } = true;
        public double Width { get; set; } = 3.00;
        public int Order { get; set; } = -1;


        public TableColumnAttribute() { }

        public TableColumnAttribute(
            string? headerText = null,
            string? stringFormat = null,
            ParagraphAlignment alignment = ParagraphAlignment.Center,
            bool visible = true,
            double width = 3.00,
            int order = -1)
        {
            HeaderText = headerText;
            StringFormat = stringFormat;
            Alignment = alignment;
            Visible = visible;
            Width = width;
            Order = order;
        }


    }

}
