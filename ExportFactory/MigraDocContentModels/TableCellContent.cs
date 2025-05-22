using MigraDoc.DocumentObjectModel;

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

        public Unit Width { get; set; } = Unit.FromCentimeter(0); // Default to no width

        // settings
        public ColumnStyleSettings Style { get; set; } = new ColumnStyleSettings();

        public TableCellContent Clone()
        {
            return new TableCellContent
            {
                Markdown = this.Markdown,
                ColSpan = this.ColSpan,
                RowSpan = this.RowSpan,
                SvgImage = this.SvgImage,
                Width = this.Width,
                Style = new ColumnStyleSettings
                {
                    AutoSize = this.Style.AutoSize,
                    Alignment = this.Style.Alignment
                }
            };
        }


    }











}
