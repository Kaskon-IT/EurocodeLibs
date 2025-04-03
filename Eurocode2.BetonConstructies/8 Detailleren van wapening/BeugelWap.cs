namespace Eurocode.BetonConstructies

{
    public class BeugelWap
    {
        public double AantalSnede;
        public double Diameter;
        public double Hoh;


        public double AswToegepast
        {
            get
            {
                return WapeningHelper.GetDsnOpp(AantalSnede, Diameter, Hoh);
            }

        }   // Asw in mm²/m

        public static List<BeugelWap> GetLijstBeugelWap(string aantalSnedenString, string hohMatenString, string diamString)
        {
            char splitTeken = ' ';

            // aantal snede
            List<string> lijstSneden = [.. aantalSnedenString.Split(splitTeken)];
            List<int> bglSneden = [.. lijstSneden.Select(x => int.Parse(x))];
            // aantal hoh-maten
            List<string> lijstHohMaten = [.. hohMatenString.Split(splitTeken)];
            List<double> hohMaten = [.. lijstHohMaten.Select(x => double.Parse(x))];
            // diameters
            List<string> lijstDiameters = [.. diamString.Split(splitTeken)];
            List<double> diameters = [.. lijstDiameters.Select(x => double.Parse(x))];

            // lijst ophalen
            List<BeugelWap> lijstBeugelWap = WapeningHelper.GetLijstBeugelWap(diameters, hohMaten, bglSneden);


            return [.. lijstBeugelWap.OrderBy(wap => wap.AswToegepast)];
        }




    }



}
