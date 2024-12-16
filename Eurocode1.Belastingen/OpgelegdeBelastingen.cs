namespace Eurocode.Belastingen
{
    public struct OpgelegdeBelastingen
    {
        public OpgelegdeBelastingen()
        {

        }

        public OpgelegdeBelastingen(double vlaklast, double puntlast)
        {
            Vlaklast = vlaklast;
            Puntlast = puntlast;
        }

        public double Vlaklast { get; set; } = 3.0;
        public double Puntlast { get; set; } = 3.0;
        public double LijnlastRand { get; set; } = 5.0;
    }



}
