
using MigraDoc.DocumentObjectModel;
namespace ExportFactory.MigraDocContentModels
{
    public class DocumentContent
    {
        public CoverPageContent CoverPage { get; set; }
        public List<SectionContent> Sections { get; set; } = new List<SectionContent>();
        public TableOfContentsContent TableOfContents { get; set; }
    }






    public class TableOfContentsContent
    {
        public string Title { get; set; } = "Table of Contents";
    }











    public class DefineStylesContent
    {
        public string Name { get; set; } // Style name
        public string FontName { get; set; } = "Arial Narrow"; // Font
        public double FontSize { get; set; } = 10; // Font size
        public bool IsBold { get; set; } = false; // Bold
        public bool IsItalic { get; set; } = false; // Italic
        public string TextColor { get; set; } = "Black"; // Text color
        public double SpaceBefore { get; set; } = 0; // Space before paragraph
        public double SpaceAfter { get; set; } = 0; // Space after paragraph
    }





    public class TableCellContent
    {
        public string Markdown { get; set; } = ""; // Markdown string for cell content
        public int ColSpan { get; set; } = 1; // Default column span
        public int RowSpan { get; set; } = 1; // Default row span
        public string SvgImage { get; set; } = ""; // Default to no image

        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center; // Default alignment


    }






}
