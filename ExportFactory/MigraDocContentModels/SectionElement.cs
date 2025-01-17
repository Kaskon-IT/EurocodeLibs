namespace ExportFactory.MigraDocContentModels
{
    public abstract class SectionElement
    {
        public int Order { get; set; }
    }

    public class HeadingContent : SectionElement
    {
        public string Text { get; set; } = "Untitled Heading";
        public string Style { get; set; } = "Normal";
        public bool AddToTOC { get; set; } = false;
    }

    public class ParagraphContent : SectionElement
    {
        public string Markdown { get; set; } = ""; // Markdown string for formatting
        public string Style { get; set; } = "Normal"; // Default style
    }


    public class TableContent : SectionElement
    {
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
        public List<string> Headers { get; set; } = new List<string>();
        public List<double> ColumnWidths { get; set; } = new List<double>();
        public List<List<TableCellContent>> Rows { get; set; } = new List<List<TableCellContent>>();
    }



    public enum TableAlignment
    {
        Left,
        Center
    }



}
