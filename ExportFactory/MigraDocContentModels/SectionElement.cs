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
        public bool AddToTOC { get; set; } = true; // optional set to false to skip from TOC
        public int Level { get; set; } = 1; // default h1



    }

    public class ParagraphContent : SectionElement
    {
        public string Markdown { get; set; } = ""; // Markdown string for formatting
        public string Style { get; set; } = "Normal"; // Default style
    }


    public class TableContent : SectionElement
    {
        public string Title { get; set; } = "";
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
        public List<TableCellHeaderContent> Headers { get; set; } = new List<TableCellHeaderContent>();
        public List<double> ColumnWidths { get; set; } = new List<double>();
        public List<List<TableCellContent>> Rows { get; set; } = new List<List<TableCellContent>>();
    }



    //public class TableContent<T> : SectionElement
    //{
    //     public List<T> TableData { get; set; } = [];
    // }

    //public class TableContent2


    public enum TableAlignment
    {
        Left,
        Center
    }



}
