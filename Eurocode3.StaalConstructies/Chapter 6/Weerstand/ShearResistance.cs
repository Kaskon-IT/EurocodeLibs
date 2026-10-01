namespace Eurocode.StaalConstructies
{
    public static class ShearResistance
    {
        public static double VRd(double Av, double fy, double gammaM)
        {
            return Av * fy / (Math.Sqrt(3) * gammaM) * 1e-3; // N naar kN
        }
    }



}
