using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{
    public class DocumentContent
    {

        public Color AccentColor { get; set; } = Colors.Teal;

        public Color HeaderBackgroundColor { get; set; } = Colors.LightBlue;
        public Color HeaderColor { get; set; } = Colors.Black;

        public Color FooterBackgroundColor { get; set; } = Colors.DarkBlue;
        public Color FooterColor { get; set; } = Colors.White;


        public Font Font { get; set; } = new Font("Verdana", 9);

        public CoverPageContent CoverPage { get; set; } = new CoverPageContent();
        public PageHeaderContent PageHeader { get; set; } = new PageHeaderContent();
        //public PageFooterContent PageFooter { get; set; } = new PageFooterContent();    
        public List<SectionContent> Sections { get; set; } = new List<SectionContent>();
        public TableOfContentsContent TableOfContents { get; set; }



    }






}
