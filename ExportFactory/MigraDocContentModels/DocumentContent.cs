using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    public class DocumentContent
    {

        public Color AccentColor { get; set; } = Colors.Teal;
        public Font Font { get; set; } = new Font("Verdana", 9);

        public CoverPageContent CoverPage { get; set; } = new CoverPageContent();
        public PageHeaderContent PageHeader { get; set; } = new PageHeaderContent();
        //public PageFooterContent PageFooter { get; set; } = new PageFooterContent();    
        public List<SectionContent> Sections { get; set; } = new List<SectionContent>();
        public TableOfContentsContent TableOfContents { get; set; }



    }






}
