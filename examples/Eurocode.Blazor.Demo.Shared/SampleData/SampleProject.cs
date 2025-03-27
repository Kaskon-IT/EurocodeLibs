using Eurocode.Belastingen;
using Eurocode.BetonConstructies;
using Eurocode.Grondslagen;



namespace Eurocode.Blazor.Demo.Shared.SampleData
{


    public class SampleProject
    {
        public int Id { get; set; } = 1;

        public string Name { get; set; } = "Project X";
        public string Description { get; set; } = "Test voor Eurocode";
        public string Number { get; set; } = "3-TXZ-46";


        public DocumentInfo DocumentInfo { get; set; } = new();


        public GrondslagenContext Grondslagen { get; set; } = new();
        public BelastingenContext Belastingen { get; set; }
        public BetonContext Beton { get; set; }
        public BetonDekkingContext Dekking { get; set; }

        public BendingResults BendingResults { get; set; }

        public BetonProfielen.BetonProfiel BetonProfiel { get; set; } = new();

        public DwarskrachtWapContext DwarskrachtDemo { get; set; }

        public OpleggingContext OpleggingDemo { get; set; } = new();
        public UitkragingContext UitkragingDemo { get; set; } = new();


        public SampleProject(GrondslagenContext grondslagen)
        {
            Grondslagen = grondslagen;
            Belastingen = new BelastingenContext(Grondslagen);
            Beton = new();
            Dekking = new BetonDekkingContext(Grondslagen, Beton);
            BendingResults = new(Beton, 400, 500, 35, 48.3);
            //DwarskrachtDemo = new(Beton, new ParametrischeProfielen.ParametrischProfielContext(), 100);
            DwarskrachtDemo = DwarskrachtWap.GetDwarskrachtWapContext(Beton, new ParametrischeProfielen.ParametrischProfielContext(), 21.8, 350, 102, 2, 8, [75, 150, 300]);
            UitkragingDemo = new() { Beton = Beton };

        }

        //public SampleProject()
        //{
        //    Grondslagen = new();
        //    Belastingen = new(Grondslagen);
        //    Beton = new();
        //    Dekking = new(Grondslagen, Beton);
        //    BendingResults = new(Beton, 400, 500, 35, 48.3);
        //}







    }
}
