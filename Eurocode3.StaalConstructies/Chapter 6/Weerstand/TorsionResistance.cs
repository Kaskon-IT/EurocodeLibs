namespace Eurocode.StaalConstructies
{
    public static class TorsionResistance
    {
        public static double TauEd(double T, double It, double t)
        {
            return Math.Abs(T) * 1e6 * t / It; // kNm naar Nmm
        }

        public static double TauEdClosedSection(double T, double enclosedMedianArea, double t)
        {
            return Math.Abs(T) * 1e6 / (2 * enclosedMedianArea * t); // kNm naar Nmm
        }

        public static double TRd(double It, double t, double fy, double gammaM)
        {
            return It / t * fy / (Math.Sqrt(3) * gammaM) * 1e-6; // Nmm naar kNm
        }

        public static double TRdClosedSection(double enclosedMedianArea, double t, double fy, double gammaM)
        {
            return 2 * enclosedMedianArea * t * fy / (Math.Sqrt(3) * gammaM) * 1e-6; // Nmm naar kNm
        }

        public static double VPlTRdIH(double vPlRd, double tauEd, double fy, double gammaM)
        {
            double reduction = 1 - tauEd / (1.25 * fy / (Math.Sqrt(3) * gammaM));
            return Math.Sqrt(Math.Max(0, reduction)) * vPlRd;
        }

        public static double VPlTRdChannel(double vPlRd, double tauEd, double fy, double gammaM)
        {
            double reduction = 1 - tauEd / (fy / (Math.Sqrt(3) * gammaM));
            return Math.Sqrt(Math.Max(0, reduction)) * vPlRd;
        }

        public static double VPlTRdHollowSection(double vPlRd, double tauEd, double fy, double gammaM)
        {
            double stressRatio = tauEd / (fy / (Math.Sqrt(3) * gammaM));
            return Math.Sqrt(Math.Max(0, 1 - Math.Pow(stressRatio, 2))) * vPlRd;
        }
    }



}
