using CommonLibrary.Models;
using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    public class CoverPageContent
    {
        public string Title { get; set; } = "Untitled Document";
        public string Subtitle { get; set; } = "Subtitle";
        public string ProjectNumber { get; set; } = "0000000";
        public string CompanyName { get; set; } = "";
        public string CompanyLogoPath { get; set; } = "";
        public Color BackgroundColor { get; set; } = Colors.NavajoWhite;


        public string DocumentNumber { get; set; } = "Document Number";
        public string Author { get; set; } = "Author Name";
        public string CheckedBy { get; set; } = "Checked By";




        public List<LabeledValue> ProjectLabeledValues { get; set; } = [];
        public List<LabeledValue> DocumentLabeledValues { get; set; } = [];
        public RevisionContent RevisionContent { get; set; } = new RevisionContent();




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
