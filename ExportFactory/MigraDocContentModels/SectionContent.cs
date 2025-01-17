namespace ExportFactory.MigraDocContentModels
{
    public class SectionContent
    {
        public int Order { get; set; } = 0; // Default order
        public string Title { get; set; } // Optional section title
        public List<SectionElement> Elements { get; set; } = new List<SectionElement>();
    }






}
