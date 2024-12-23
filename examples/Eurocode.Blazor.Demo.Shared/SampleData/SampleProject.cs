using Eurocode.Belastingen;
using Eurocode.BetonConstructies;
using Eurocode.Grondslagen;


namespace Eurocode.Blazor.Demo.Shared.SampleData
{
    public class SampleProject
    {
        public int Id { get; set; } = 1;


        public GrondslagenContext Grondslagen { get; set; } = new();
        public BelastingenContext Belastingen { get; set; }
        public BetonContext Beton { get; set; }
        public BetonDekkingContext Dekking { get; set; }

        public SampleProject(GrondslagenContext grondslagen)
        {
            Grondslagen = grondslagen;
            Belastingen = new BelastingenContext(Grondslagen);
            Beton = new();
            Dekking = new BetonDekkingContext(Grondslagen, Beton);
        }

        public SampleProject()
        {
            Grondslagen = new();
            Belastingen = new(Grondslagen);
            Beton = new();
            Dekking = new(Grondslagen, Beton);
        }





    }
}
