using MigraDoc.DocumentObjectModel;
using System.ComponentModel;

namespace ExportFactory.MigraDocContentModels
{
    public class DocumentContent
    {

        public DocumentInfo DocumentInfo { get; set; } = new DocumentInfo()
        {
            Author = "Kaskon",
            Title = "Title",
            Comment = "",
            Keywords = "prefabbeton, trappen en bordessen",
            Subject = "",


        };

        public Color AccentColor { get; set; } = Colors.Teal;

        public Color HeaderBackgroundColor { get; set; } = Colors.Transparent;
        public Color HeaderLineColor { get; set; } = Colors.Teal;
        public Color HeaderColor { get; set; } = Colors.Black;

        public Color FooterBackgroundColor { get; set; } = Colors.Transparent;
        public Color FooterLineColor { get; set; } = Colors.Teal;
        public Color FooterColor { get; set; } = Colors.Black;

        public RevisionContent Revisions { get; set; } = new();
        public Font Font { get; set; } = new Font("Verdana", 9);
        public CoverPageContent CoverPage { get; set; } = new CoverPageContent();
        public PageHeaderContent PageHeader { get; set; } = new PageHeaderContent();
        public PageFooterContent PageFooter { get; set; } = new PageFooterContent();
        public List<SectionContent> Sections { get; set; } = new List<SectionContent>();
        public TableOfContentsContent TableOfContents { get; set; }

        public PageMarginAndPageNumberSettingsEnum? PageMarginSetting { get; set; } = PageMarginAndPageNumberSettingsEnum.EvenOnevenGespiegeld;


        public enum PageMarginAndPageNumberSettingsEnum
        {
            [Description("Alles gecentreerd")]
            Gecentreerd,
            [Description("Marge links")]
            MargeLinks_PaginaNummerRechts,
            [Description("Even/Oneven gespiegeld")]
            EvenOnevenGespiegeld,

        }

    }


    public class RevisionContent
    {
        public List<Revision> Revisions { get; set; } = [];
        public Dictionary<string, string> ColumnNames { get; set; } = new Dictionary<string, string>()
        {
            { "Name", "Rev."},
            { "Date", "Datum" },
            { "Description", "Beschrijving" }
        };
       
    }

    public class Revision
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
    }






}
