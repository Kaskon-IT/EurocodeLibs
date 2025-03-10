namespace Eurocode.BetonConstructies
{
    public class WapeningHelper
    {
        /// <summary>
        /// Haalt het doorsnede oppervlak van staven op. 
        /// </summary>
        /// <param name="n">aantal staven</param>
        /// <param name="d">diameter</param>
        /// <returns></returns>
        public static double GetDsnOpp(double n, double d)
        {
            return n * Math.Pow(d, 2) * Math.PI / 4;
        }

        /// <summary>
        /// Voor doorsnede oppervlak van 1 of meerdere staafgroepen (As)
        /// </summary>
        /// <param name="wapening">wapening als string</param>
        /// <returns>Doorsnede oppervlak (As)</returns>
        public static double GetDsnOpp(string wapening)
        {
            // splits 
            // karakters voor Øk
            List<char> diameterChars = ['Ø', 'R', 'r', 'D', 'd'];
            Char[] splitChars = ['+'];
            List<char> hohChars = ['-'];

            // stap 1: splits by 
            var wapgroepen = wapening.Split(splitChars).ToList();

            double dsnOpp = 0;
            foreach (var wapgroep in wapgroepen)
            {
                dsnOpp += GetDsnOppWapGroep(wapgroep.Trim());
            }
            return dsnOpp;


            // 
        }

        private static double GetDsnOppWapGroep(string wapgroep)
        {
            Char[] diamChars = ['Ø', 'R', 'r', 'D', 'd'];
            char hohChar = '-';
            // 8-150
            if (wapgroep.Contains(hohChar))
            {
                // r8-150
                var hohGroep = wapgroep.Split(hohChar).ToList();
                _ = double.TryParse(hohGroep.First(), out double d);
                _ = double.TryParse(hohGroep.Last(), out double hoh);
                return _ = GetDsnOpp(d: d, hoh: hoh);
            }
            else if (IsCharInString(diamChars, wapgroep))
            {
                // 3x16, 3Ø16 etcetera
                var nGroep = wapgroep.Split(diamChars).ToList();
                _ = double.TryParse(nGroep.First(), out double n);
                _ = double.TryParse(nGroep.Last(), out double d);
                return GetDsnOpp(n: n, d: d);
            }
            else return 0;
        }


        public static bool IsCharInString(char[] chars, string inputString)
        {
            // Loop through the array of characters
            foreach (char c in chars)
            {
                // Check if the character is present in the string
                if (inputString.Contains(c))
                {
                    return true; // Return true if any character is found
                }
            }
            return false; // Return false if none of the characters are found
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <param name="hoh"></param>
        /// <returns></returns>
        public static double GetDsnOpp(double n = 1, double d = 8, double hoh = 150)
        {
            return n * Math.Pow(d, 2) * Math.PI / 4 / hoh * 1000;
        } // van aantal (n), diameter (d) en hart-op-hart (hoh) naar As (bijvoorbeeld: 2Ø8-300 geeft 335 mm², handig voor beugels en vloer/wand-wapening)


        public static List<BeugelWap> GetLijstBeugelWap(List<double> lijstBeugelDiameters, List<double> lijstHohAfstanden, List<int> lijstBeugelSneden)
        {
            if (lijstHohAfstanden == null) return null;
            if (lijstBeugelSneden == null) return null;
            List<BeugelWap> returnList = new List<BeugelWap>();
            foreach (double bglDiam in lijstBeugelDiameters)
            {
                foreach (double hohAfstand in lijstHohAfstanden)
                {
                    foreach (int aantal in lijstBeugelSneden)
                    {
                        BeugelWap returnItem = new BeugelWap();
                        returnItem.Hoh = hohAfstand;
                        returnItem.AantalSnede = aantal;
                        returnItem.Diameter = bglDiam;
                        returnList.Add(returnItem);
                    }
                }
            }
            return returnList;
        }

        public static BeugelWap ZoekBeugelWap(double zoekwaarde, List<BeugelWap> lijstBeugelWap)
        {
            BeugelWap returnVal = null;
            if (lijstBeugelWap == null) return null;
            foreach (BeugelWap bglWap in lijstBeugelWap)
            {
                if (bglWap.AswToegepast >= zoekwaarde &&
                    (returnVal == null || returnVal.AswToegepast > bglWap.AswToegepast))
                {
                    returnVal = bglWap;
                }
            }
            return returnVal;
        }


    }
}
