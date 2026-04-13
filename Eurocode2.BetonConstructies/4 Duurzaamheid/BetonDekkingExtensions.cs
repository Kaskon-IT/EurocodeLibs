using Eurocode.Grondslagen;

namespace Eurocode.BetonConstructies
{
    public static class BetonDekkingExtensions
    {


        // tabel voor c,min,dur
        readonly static int[,] _tabelCminDur = new int[,]
        {
            // X0   XC1     XC2/XC3     XC4     XD1/XS1     XD2/XS3     XD3/XS3
            { 10,   10,     10,         15,     20,         25,         25},        // S1
            { 10,   10,     15,         20,     25,         30,         30 },       // S2
            { 10,   10,     20,         25,     30,         35,         35 },       // S3
            { 10,   15,     25,         30,     35,         40,         40 },       // S4
            { 15,   20,     30,         35,     40,         45,         45 },       // S5
            { 20,   25,     35,         40,     45,         50,         50}         // S6
        };


        // haalt c,min,dur op uit tabel
        public static double GetCminDur(this BetonDekkingContext dekking)
        {
            double returnval = 0;
            int row = dekking.Constructieklasse.Klasse - 1;

            if (dekking.Milieuklassen.Any())
            {
                foreach (MilieuklasseEnum mk in dekking.Milieuklassen)
                {
                    var cMinDur = _tabelCminDur[row, mk.GetCminDurColumnIndex()];
                    if (cMinDur > returnval)
                        returnval = cMinDur;
                }
            }



            return returnval;
        }



        // haalt de kolom index op voor gebruik in de tabel
        public static int GetCminDurColumnIndex(this MilieuklasseEnum milieuklasse)
        {
            switch (milieuklasse)
            {
                default: return 0;
                case MilieuklasseEnum.X0: return 0;
                case MilieuklasseEnum.XC1: return 1;
                case MilieuklasseEnum.XC2:
                case MilieuklasseEnum.XC3: return 2;
                case MilieuklasseEnum.XC4: return 3;
                case MilieuklasseEnum.XD1:
                case MilieuklasseEnum.XS1: return 4;
                case MilieuklasseEnum.XD2:
                case MilieuklasseEnum.XS2: return 5;
                case MilieuklasseEnum.XD3:
                case MilieuklasseEnum.XS3: return 6;
            }
        }



        /// <summary>
        /// Vermeerdering (k1) van de betondekking (slijtlaag)
        /// </summary>
        /// <param name="nb">De nationale bijlage</param>
        /// <returns></returns>
        public static double GetK1(this NationaleBijlageEnum nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.EU => 5.0,
                NationaleBijlageEnum.NL => 0.0,
                _ => 5.0
            };
        }

        /// <summary>
        /// Vermeerdering (k2) van de betondekking (slijtlaag)
        /// </summary>
        /// <param name="nb">De nationale bijlage</param>
        /// <returns></returns>
        public static double GetK2(this NationaleBijlageEnum nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.EU => 10.0,
                NationaleBijlageEnum.NL => 0.0,
                _ => 10.0
            };
        }

        /// <summary>
        /// Vermeerdering (k3) van de betondekking (slijtlaag)
        /// </summary>
        /// <param name="nb">De nationale bijlage</param>
        /// <returns></returns>
        public static double GetK3(this NationaleBijlageEnum nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.EU => 15.0,
                NationaleBijlageEnum.NL => 0.0,
                _ => 15.0
            };
        }


        /// <summary>
        /// 4.4.1.3 (4) Voor beton gestort tegen oneffen oppervlakken behoort de nominale dekking in het algemeen te zijn 
        /// vermeerderd door het aanhouden van grotere ontwerptoleranties.De toename behoort in overeenstemming te
        /// zijn met het verschil veroorzaakt door de oneffenheid, maar de nominale dekking behoort ten minste k1 mm te
        /// bedragen voor beton gestort op een voorbereide ondergrond(inclusief schraalbeton)
        /// </summary>
        /// <param name="dekking">Context van de dekking</param>
        /// <returns>k1</returns>
        public static double GetK1OneffenOppervlakken(this BetonDekkingContext dekking)
        {
            return dekking.Grondslagen.NationaleBijlage switch
            {
                NationaleBijlageEnum.EU => 40.00,
                NationaleBijlageEnum.NL => dekking.GetCminDur() + 10,
                _ => 40.00
            };
        }

        /// <summary>
        /// 4.4.1.3 (4) Voor beton direct gestort op of tegen de grond dient de dekking c,min tenmiste zo groot as k2 te zijn.
        /// </summary>
        /// <param name="dekking">Context van de dekking</param>
        /// <returns>k2</returns>
        public static double GetK2DirectGestortOpOfTegenDeGrond(this BetonDekkingContext dekking)
        {
            return dekking.Grondslagen.NationaleBijlage switch
            {
                NationaleBijlageEnum.EU => 75.00,
                NationaleBijlageEnum.NL => dekking.GetCminDur() + 50,
                _ => 75.00
            };
        }


        public static double GetMinimaleBetondekking(this BetonDekkingContext dekking)
        {
            List<double> doubles = [dekking.GetCminDur(), dekking.DekkingMinAanhechting, 10.00]; // 4.4.1.2 vergelijking (4.2)
            return doubles.Max();
        }


        public static double GetDekkingBetonstaalMinimaal(this BetonDekkingContext dekking)
        {
            if (dekking.GrootsteKorrelDiameter <= 32)
                return (int)dekking.WapeningDiameterGelijkwaardig;
            else
                return (int)dekking.WapeningDiameterGelijkwaardig + 5;

        }



        public static double GetDekkingNominaal(this BetonDekkingContext dekking)
        {

            var cNom = dekking.GetMinimaleBetondekking() + dekking.DekkingToeslagUitvoeringsToleranties; // 4.4.1.1 vergelijking (4.1)
            List<double> values = [cNom];
            switch (dekking.BetonStortOndergrond) // 4.4.1.3 (4)
            {
                case BetonDekkingContext.BetonStortOndergrondEnum.OpOfTegenGrond:
                    values.Add(dekking.GetK2DirectGestortOpOfTegenDeGrond());
                    break;
                case BetonDekkingContext.BetonStortOndergrondEnum.Werkvloer:
                    values.Add(dekking.GetK1OneffenOppervlakken());
                    break;
            }
            switch (dekking.BetonAfwerkingOppervlak) // 4.4.1.2 (11)
            {
                case BetonDekkingContext.BetonAfwerkingOppervlakEnum.NabewerktOnEffen:
                    values.Add(cNom + 5);
                    break;
            }


            double returnVal = values.Max();






            return values.Max();
        }


        //static string GetPlaatgeometrieDisplay(this BetonDekkingContext dekking) => dekking.IsPlaatGeometrie  ? "Plaatgeometrie" : "";

        public static string ToUserFriendlyString(this BetonDekkingContext dekking)
        {
            List<string> values = [dekking.Constructieklasse.UserFriendlyName];

            if (dekking.IsPlaatGeometrie)
                values.Add("Plaatgeometrie");
            if (dekking.IsKwaliteitsBeheersing)
                values.Add("Kwaliteitsbeheersing");



            return string.Join(", ", values);
        }




    }
}