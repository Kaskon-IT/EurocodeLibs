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

        public static double GetGemiddeldeDiameter(string wapening)
        {

            var wapgroepen = GetWapGroepen(wapening);
            if (wapgroepen == null) return 0;

            return BerekenGemiddeldeDiameter(wapgroepen);
        }

        public static double BerekenGemiddeldeDiameter(List<string> wapgroepen)
        {
            var teller = 0.0;
            var noemer = 0.0;

            var oppTotaal = 0.0;
            var nTotaal = 0.0;

            foreach (var wapgroep in wapgroepen)
            {

                var details = GetWapDetails(wapgroep);
                //var n = details.n.HasValue ? details.n.Value : 1.0;
                oppTotaal += details.dsnOpp;

                if (details.n.HasValue)
                {
                    nTotaal += details.n.Value;
                }
                else
                {
                    nTotaal += 1.0; // als n niet is opgegeven, dan is het 1
                }



                //nTotaal += details.n.HasValue ? details.n.Value : ;
            }

            // Gemiddeld oppervlak per staaf
            double gemiddeldOppervlak = oppTotaal / nTotaal;

            // Equivalent gemiddelde diameter
            double gemiddeldeDiameter = Math.Sqrt((4 * gemiddeldOppervlak) / Math.PI);

            return Math.Round(gemiddeldeDiameter, 3);

            //return noemer > 0 ? teller / noemer : 0.0;
        }

        public double BerekenGemiddeldeDiamter(List<(double a, double d, double? hoh, double? n)> groepen)
        {

            double teller = groepen.Sum(g => g.n.Value * g.d);
            double noemer = groepen.Sum(g => g.n.Value);

            return noemer > 0 ? teller / noemer : 0.0;
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
                    double.TryParse(nGroep.First(), out double nSnede);
                    double.TryParse(nGroep.Last(), out d);

                    if (nSnede == 0) nSnede = 1;

                    return (GetDsnOpp(nSnede, d, hoh), d, hoh, nSnede * 1000.0 / hoh);
                }
                else
                {
                    return (GetDsnOpp(d: d, hoh: hoh), d, hoh, 1000.0 / hoh);

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
            else return (0, 0, 1000.0, 0);
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
            var voorstellen = GetWapeningVoorstellen(asBen, hohMax);
            if (voorstellen.Any())
            {
                return string.Join(", ", voorstellen);
            }
            else
            {
                return "error";
            }

        }

        public static List<string> GetWapeningVoorstellen(double asBen, int hohMax, int nauwkeurigheid, List<double> diameters)
        {
            List<string> voorstellen = [];

            int aantalKeerDatHohMaxIsToegepast = 0;
            foreach (var diameter in diameters)
            {

                var voorstel = GetWapeningVoorstel(asBen, hohMax, nauwkeurigheid, diameter, out int hohToe);
                if (voorstel != null)
                {
                    if (hohToe == hohMax)
                    {
                        aantalKeerDatHohMaxIsToegepast++;
                    }

                    // stop als meerdere keren maximale hoh-maat is toegepast.
                    if (aantalKeerDatHohMaxIsToegepast < 3)
                    {
                        voorstellen.Add(voorstel);

                    }
                }
            }


            return voorstellen;
        }

        public static List<string> GetWapeningVoorstellenAantalDiameter(double asBen, double maxHoh, double werkendeBreede, List<double> diameters)
        {
            List<string> voorstellen = [];
            int? nAppliedPrevious = null;
            int nApplied = 0;
            foreach (var diameter in diameters)
            {
                var voorstel = GetWapeningVoorstelAantalDiameter(asBen, maxHoh, diameter, werkendeBreede, out nApplied);
                if (voorstel != null)
                {
                    // stop als meerdere keren maximale hoh-maat is toegepast.
                    if (nAppliedPrevious != null && nAppliedPrevious == nApplied)
                    {
                        break;
                    }
                    voorstellen.Add(voorstel);
                    nAppliedPrevious = nApplied;
                }
            }
            return voorstellen;
        }


        public static string? GetWapeningVoorstelAantalDiameter(double asBen, double maxHoh, double d, double werkendeBreedte, out int nApplied)
        {
            double hohMin = 2.5 * d;
            double nBenodigdVoorDoorsnede = (asBen / GetDsnOpp(1, d));
            double nBenodigdOmAanHohMaxTeVoldoen = werkendeBreedte / maxHoh;

            double nBen = Math.Max(nBenodigdVoorDoorsnede, nBenodigdOmAanHohMaxTeVoldoen);

            // we gebruiken hele staven, dus rond af naar boven
            nApplied = (int)Math.Ceiling(nBen);
            // controleer of we aan de hoh-min voldoen (mits meerdere staven zijn toegepast)
            if (nApplied > 1)
            {
                var hohToe = werkendeBreedte / (nApplied - 1);
                if (hohToe >= hohMin && hohToe <= maxHoh)
                {
                    return $"{nApplied}Ø{d}";
                }
                else
                {
                    return null; // geen config gevonden.
                }
            }
            else if (nApplied == 1)
            {
                return $"{nApplied}Ø{d}";
            }
            else
            {
                return null;
            }






        }

        public static string? GetWapeningVoorstel(double asBen, int hohMax, int nauwkeurigheid, double d, out int hohToe)
        {
            double min1 = 50.0;
            double min2 = 2.5 * d;
            double hohMin = Math.Max(min1, min2);
            double nBen = (asBen / GetDsnOpp(1, d));
            int hohBen = (int)(1000.00 / nBen);

            if (hohBen >= hohMin)
            {
                // Rond hohToe naar beneden af op basis van nauwkeurigheid
                hohToe = (int)(Math.Floor(Math.Min(hohBen, hohMax) / (double)nauwkeurigheid) * nauwkeurigheid);
                return $"Ø{d}-{hohToe}";
            }
            else
            {
                hohToe = 1;
                return null;
            }
        }



        public static List<string> GetWapeningVoorstellen(double asBen, int hohMax)
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



            return voorstellen;






        }


    }
}
