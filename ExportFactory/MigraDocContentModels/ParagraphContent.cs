namespace ExportFactory.MigraDocContentModels
{
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



}
