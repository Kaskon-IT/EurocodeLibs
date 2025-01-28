namespace ExportFactory.MigraDocContentModels
{
    public class TableCellContent
    {
        public TableCellContent() { }
        public TableCellContent(string markdown) { Markdown = markdown; }

        public string Markdown { get; set; } = ""; // Markdown string for cell content
        public int ColSpan { get; set; } = 1; // Default column span
        public int RowSpan { get; set; } = 1; // Default row span
        public string SvgImage { get; set; } = ""; // Default to no image

        // settings
        public ColumnStyleSettings Style { get; set; } = new ColumnStyleSettings();




    }











}
