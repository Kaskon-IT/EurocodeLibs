using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    public abstract class SectionElement
    {
        public int Order { get; set; }
    }

    public class HeadingContent : SectionElement
    {
        public HeadingContent() { }


        public HeadingContent(string text, string style)
        {
            Text = text;
            Style = style;

        }


        public HeadingContent(string text, int level)
        {
            Text = text;
            Style = $"Kop {level}";
            AddToTOC = (level == 1 || level == 2);
            Level = level;
        }


        public HeadingContent(string text, string style, bool addToTOC, int level)
        {
            Text = text;
            Style = style;
            AddToTOC = addToTOC;
            Level = level;
        }

        public string Text { get; set; } = "Untitled Heading";
        public string Style { get; set; } = "Normal";
        public bool AddToTOC { get; set; } = true; // optional set to false to skip from TOC
        public int Level { get; set; } = 1; // default h1



    }

    public class ParagraphContent : SectionElement
    {
        public ParagraphContent() { }

        public ParagraphContent(string markdown)
        {
            Markdown = markdown;
        }

        public ParagraphContent(string markdown, string style)
        {
            Markdown = markdown;
            Style = style;
        }
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

    /// <summary>
    /// Gebruik dit om direct een MigraDoc.Table in de documentContent te plaatsen.
    /// </summary>
    public class MigraDocTable : SectionElement
    {
        public MigraDoc.DocumentObjectModel.Tables.Table Table { get; set; } = new();
    }

    /// <summary>
    /// Universele class voor Paragraph, Chart, Image, Table of TextFrame van MigraDoc rechtstreeks te plaatsen in de document bouwstenen.
    /// </summary>
    public class MigraDocElement : SectionElement
    {
        public MigraDoc.DocumentObjectModel.DocumentObject? DocumentObject { get; set; }

        public MigraDocElement()
        {

        }

        public MigraDocElement(DocumentObject documentObject)
        {
            DocumentObject = documentObject;
        }

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
