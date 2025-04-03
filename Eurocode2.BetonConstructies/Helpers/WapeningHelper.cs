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


        public static List<string>? GetWapGroepen(string wapening)
        {
            Char[] splitChars = ['+'];
            return [.. wapening.Split(splitChars)];
        }


        /// <summary>
        /// Voor doorsnede oppervlak van 1 of meerdere staafgroepen (As)
        /// </summary>
        /// <param name="wapening">wapening als string</param>
        /// <returns>Doorsnede oppervlak (As)</returns>
        public static double GetDsnOpp(string wapening)
        {
            // stap 1: splits by 
            var wapgroepen = GetWapGroepen(wapening);


            double dsnOpp = 0;
            if (wapgroepen == null) return dsnOpp;

            foreach (var wapgroep in wapgroepen)
            {
                dsnOpp += GetWapDetails(wapgroep.Trim()).dsnOpp;
            }
            return dsnOpp;


            // 
        }

        private static (double dsnOpp, double diam, double? hoh, double? n) GetWapDetails(string wapgroep)
        {
            Char[] diamChars = ['Ø', 'R', 'r', 'D', 'd', 'x', 'X', 'ø', '®', '×', '*'];
            char hohChar = '-';
            // 8-150
            if (wapgroep.Contains(hohChar))
            {
                // r8-150 (n = 1 met hoh-maat)
                var hohGroep = wapgroep.Split(hohChar).ToList();
                _ = double.TryParse(hohGroep.First(), out double d);
                _ = double.TryParse(hohGroep.Last(), out double hoh);

                // 2r8-100 (n > 1 met hoh-maat)
                if (IsCharInString(diamChars, hohGroep.First()))
                {
                    var nGroep = hohGroep.First().Split(diamChars).ToList();
                    double.TryParse(nGroep.First(), out double n);
                    double.TryParse(nGroep.Last(), out d);

                    if (n == 0) n = 1;

                    return (GetDsnOpp(n, d, hoh), d, hoh, n);
                }
                else
                {
                    return (GetDsnOpp(d: d, hoh: hoh), d, hoh, null);

                }

            }
            else if (IsCharInString(diamChars, wapgroep))
            {
                // zonder hoh-maat dus
                // 3x16, 3Ø16 etcetera
                var nGroep = wapgroep.Split(diamChars).ToList();
                _ = double.TryParse(nGroep.First(), out double n);
                _ = double.TryParse(nGroep.Last(), out double d);

                if (n == 0)
                    n = 1;


                return (GetDsnOpp(n: n, d: d), d, null, n);
            }
            else return (0, 0, 1000, 0);
        }


        public static double GetKleinsteHohMaat(List<string>? wapGroepen)
        {
            if (wapGroepen == null) return 1000;

            List<double> hohMaten = [9999];

            foreach (var wapGroep in wapGroepen)
            {
                var hohMaat = GetWapDetails(wapGroep).hoh;
                if (hohMaat.HasValue)
                    hohMaten.Add(hohMaat.Value);
            }



            return hohMaten.Min();

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
        public static double GetDsnOpp(double n = 1, double d = 8, double hoh = 1000)
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


        public static string GetWapeningVoorstel(double asBen, int hohMax)
        {
            // doe een voorstel met n staven Øk
            // de hoh-afstand mag niet kleiner dan 50 zijn.
            double min1 = 50;
            // de tussenruimte mag niet kleiner dan 1,5 Øk zijn, zodat hoh-maat gelijk aan 2,5 Øk is.
            double factorMin2 = 2.5;

            List<double> staafDiameters = [6, 8, 10, 12, 16, 20, 32, 40];
            List<string> voorstellen = [];

            foreach (var diameter in staafDiameters)
            {
                double min2 = factorMin2 * diameter;
                double hohMin = Math.Max(min1, min2);

                double nBen = (asBen / GetDsnOpp(1, diameter));
                int hohBen = (int)(1000.00 / nBen);
                int hohToe = 10;
                if (hohBen >= hohMin)
                {
                    hohToe = Math.Min(hohBen, hohMax);
                    voorstellen.Add($"Ø{diameter}-{hohToe}");
                }

                // stop wanneer hohMax bereikt is
                if (hohToe == hohMax && diameter > 6)
                {
                    break;
                }


            }

            if (voorstellen.Any())
            {
                return string.Join(", ", voorstellen);
            }
            else
            {
                return "niet gevonden";
            }





        }


    }
}
