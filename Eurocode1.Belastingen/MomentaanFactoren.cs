namespace Eurocode.Belastingen
{
    public struct MomentaanFactoren
    {
        public MomentaanFactoren()
        {

        }

        public MomentaanFactoren(double mom0, double mom1, double mom2)
        {
            Mom0 = mom0;
            Mom1 = mom1;
            Mom2 = mom2;
        }

        public double Mom0 { get; set; } = 0.4;
        public double Mom1 { get; set; } = 0.5;
        public double Mom2 { get; set; } = 0.3;
    }



}
