using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    public class CoverPageContent
    {
        public string Title { get; set; } = "Untitled Document";
        public string Subtitle { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string CompanyLogoPath { get; set; } = "";
        public Color BackgroundColor { get; set; } = Colors.NavajoWhite;
    }

    public class PageHeaderContent
    {
        public string Text1 { get; set; } = "Header text";
        public string Text2 { get; set; } = "...";
        public string Text3 { get; set; } = "...";

        public string SvgLogo { get; set; } = "";
    }

    public class PageFooterContent
    {
        public Color Color { get; set; } = Colors.LightGoldenrodYellow;
        public string Text1 { get; set; } = "...";
        public string Text2 { get; set; } = "...";



    }





}
