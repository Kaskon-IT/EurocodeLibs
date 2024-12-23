namespace Eurocode.BetonConstructies
{
    public class BuigingBasic
    {


        /// <summary>
        /// Beton
        /// </summary>
        public required BetonContext Beton { get; set; }
        public double D { get; set; }
        public double M { get; set; }




        // berekende waarde
        public double X { get; private set; }
        public double Z { get; private set; }
        public double As { get; private set; }




    }




}
