using Eurocode.Belastingen;
using Eurocode.BetonConstructies;
using Eurocode.Grondslagen;
using ExportFactory.MigraDocContentModels;

namespace ProjectLibrary
{
    public class DemoProject
    {
        public int Id { get; set; } = 1;

        public string Name { get; set; } = "Project X";
        public string Description { get; set; } = "Test voor Eurocode";
        public string Number { get; set; } = "3-TXZ-46";


        public CoverPageContent CoverPage { get; set; } = new CoverPageContent
        {
            Title = "Demo Project",
            Subtitle = "Eurocode Test",
            ProjectNumber = "3-TXZ-46",
            CompanyName = "Your Company Name",
            DocumentNumber = "Document Number",
            Author = "Author Name",
            //CheckedBy = "Checked By",



        };


        public GrondslagenContext Grondslagen { get; set; } = new();
        public BelastingenContext Belastingen { get; set; } = new();
        public BetonContext Beton { get; set; } = new();
        public BetonDekkingContext Dekking { get; set; } = new();

        public BendingResults BendingResults { get; set; } = new();




        //public DemoProject(GrondslagenContext grondslagen)
        //{
        //    Grondslagen = grondslagen;
        //    Belastingen = new BelastingenContext(Grondslagen);
        //    Beton = new();
        //    Dekking = new BetonDekkingContext(Grondslagen, Beton);
        //    //BendingResults = new(Beton, new(), )
        //    //BendingResults = new(Beton, 400, 500, 35, 48.3);

        //}

        //public DemoProject()
        //{
        //    Grondslagen = new();
        //    Belastingen = new(Grondslagen);
        //    Beton = new();
        //    Dekking = new(Grondslagen, Beton);
        //    //BendingResults = new(Beton, 400, 500, 35, 48.3);
        //}


    }
}
