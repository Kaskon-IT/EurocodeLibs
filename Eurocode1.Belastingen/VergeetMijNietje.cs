namespace Eurocode.Belastingen
{
    public class VergeetMijNietje
    {
        public double L { get; set; }
        public double M { get; set; }
        public double F { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double X { get; set; }

        public enum BelastingTypeEnum
        {
            Puntlast,
            Lijnlast,
            Trapezium
        }

        public enum VergeetMijNietjeEnum
        {
            ScharnierRol,
            InklemmingInklemming,
            InklemmingRol,

        }
    }



}
