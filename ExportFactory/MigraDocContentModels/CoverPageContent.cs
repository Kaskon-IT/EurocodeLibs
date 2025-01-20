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
    }

    public class PageHeaderContent
    {
        public string Text { get; set; } = "Header text";
        public string SvgLogo { get; set; } = "";
    }

    public class PageFooterContent
    {
        public Color Color { get; set; } = Colors.LightGoldenrodYellow;
    }





}
