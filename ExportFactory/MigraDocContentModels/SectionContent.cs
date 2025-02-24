namespace ExportFactory.MigraDocContentModels
{
    public class SectionContent
    {
        public int Order { get; set; } = 0; // Default order
        public string? Title { get; set; } // Optional section title
        public string? TitleStyle { get; set; } //
        public List<SectionElement> Elements { get; set; } = new List<SectionElement>();

        public SectionContent() { }

        public SectionContent(string title, string? titleStyle)
        {
            Title = title;
            TitleStyle = titleStyle;
        }



    }

    public static class SectionContentExtensions
    {
        public static void AddHeading(this SectionContent section, string headingText, string headingStyle)
        {
            section.Elements.Add(new HeadingContent(headingText, headingStyle));
        }

        public static void AddParagraph(this SectionContent section, string text, string style)
        {
            section.Elements.Add(new ParagraphContent(text, style));
        }
        public static void AddParagraph(this SectionContent section, string text)
        {
            section.Elements.Add(new ParagraphContent(text));
        }



    }








}
