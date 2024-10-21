using Eurocode.Grondslagen;

namespace Eurocode.BetonConstructies
{



    public partial class BetonDekkingContext
    {

        public BetonDekkingContext()
        {

        }

        public BetonDekkingContext(BetonDekkingContext context)
        {
            Naam = context.Naam;
            IsPlaatGeometrie = context.IsPlaatGeometrie;
            IsKwaliteitsBeheersing = context.IsKwaliteitsBeheersing;
            OnEffenBetonOppervlak = context.OnEffenBetonOppervlak;
            GestortTegenBestaandBeton = context.GestortTegenBestaandBeton;
            BetonAfwerkingOppervlak = context.BetonAfwerkingOppervlak;
            OntwerpLevensduur = context.OntwerpLevensduur;
            BetonStortOndergrond = context.BetonStortOndergrond;
            Milieuklassen = context.Milieuklassen;
            Korreldiameter = context.Korreldiameter;

        }

        public Eurocode.Grondslagen.NationaleBijlageEnum NationaleBijlage { get; set; } = Grondslagen.NationaleBijlageEnum.NL;

        public Constructieklasse Constructieklasse
        {
            get
            {
                return new Constructieklasse(this, this.Beton);
            }
        }


        /// <summary>
        /// Naam van de betondekking context, bijvoorbeeld 'bovenzijde' of 'onderzijde' 
        /// </summary>
        public string Naam { get; set; } = "Mijn dekkingscontext";


        // Ontwerplevensduur komt uit EN-1990
        public Eurocode.Grondslagen.OntwerpLevensduurEnum OntwerpLevensduur { get; set; }

        // Uit dezelfde norm
        public BetonConstructies.BetonContext Beton { get; set; } = new(BetonContext.BetonsterkteklasseEnum.C20_25);



        /// <summary>
        /// Indien plaatgeometrie van toepassing dan een vermindering van 1 op de constructieklasse.
        /// </summary>
        public bool IsPlaatGeometrie { get; set; }

        /// <summary>
        /// Indien specifieke kwaliteitsbeheersing (bijvoorbeeld bij prefab beton) vermindering met 1 op constructieklasse.
        /// </summary>
        public bool IsKwaliteitsBeheersing { get; set; }


        public List<MilieuklasseEnum> Milieuklassen { get; set; } = [MilieuklasseEnum.XC1];




        // specifieke context voor de dekking



        public double BetondekkingNominaal
        {
            get { return BetondekkingMin + BetondekkingMinUitvoeringsToleranties; }
        }

        /// <summary>
        /// Is de minimumdekking op basis van de milieu-omstandigheden, zie 4.4.1.2 (5)
        /// </summary>
        public double BetondekkingMinDuurzaamheid
        {
            get { return this.GetCminDur(); }
        }

        /// <summary>
        /// 4.4.1.2 Minimale dekking (c,min), moet zorgen voor:
        /// - een veilige overdracht van de aanhechtkrachten (zie ook hoofdstukken 7 en 8)
        /// - de bescherming van het staal tegen corrosie (duurzaamheid)
        /// - voldoende brandwerendheid (zie EN 1992-1-2)
        /// zie 4.4.1.2
        /// </summary>
        public double BetondekkingMin { get { return this.GetMinimaleBetondekking(); } }


        /// <summary>
        /// Minimale dekking tbv aanhechting betonstaal
        /// </summary>
        public double BetondekkingMinBetonstaal
        {
            get
            {
                if (Korreldiameter < 32)
                    return (int)WapeningDiameterGelijkwaardig;
                else return (int)WapeningDiameterGelijkwaardig + 5;
            }
        }


        // constanten dekking

        /// <summary>
        /// Verhoging van de dekking tbv uitvoeringstoleranties (Δc,dev) volgens 4.4.1.3 (1)
        /// </summary>
        public double BetondekkingMinUitvoeringsToleranties { get { return this.NationaleBijlage.GetUitvoeringstoleraties(); } }

        /// <summary>
        /// Is een reductie van de minimumdekking bij gebruik van aanvullende bescherming, zie 4.4.1.2 (8)
        /// </summary>
        public const double BetondekkingMinBescherming = 0;

        /// <summary>
        /// Is een aanvullende veiligheidsmarge, zie 4.4.1.2 (6)
        /// </summary>
        public const double BetondekkingMinVeiligheidsmarge = 0;

        /// <summary>
        /// Is een reductie van de minimumdekking bij gebruik van roestvast staal, zie 4.4.1.2 (7)
        /// </summary>
        public const double BetondekkingMinRoestvastStaal = 0;

        /// <summary>
        /// k1 bij oneffen oppervlakken.
        /// </summary>
        public double BetonDekkingFactorK1
        {
            get { return this.GetK1OneffenOppervlakken(); }
        }


        /// <summary>
        /// k2 bij beton direct gestort op of tegen de grond.
        /// </summary>
        public double BetonDekkingFactorK2
        {
            get { return this.GetK2DirectGestortOpOfTegenDeGrond(); }
        }



        // wapening
        public double WapeningDiameterGelijkwaardig { get; set; }
        public double WapeningDiameterToegepast { get; set; }
        public int WapeningAantalStavenInDeBundel { get; set; }

        // beton
        //public BetonContext.SterkteKlassen BetonSterkteKlasse { get; set; }

        public double Korreldiameter { get; set; } = 31.5;

        public BetonAfwerkingOppervlakEnum? BetonAfwerkingOppervlak { get; set; } = BetonAfwerkingOppervlakEnum.Glad;

        public bool OnEffenBetonOppervlak { get; set; }

        public bool GestortTegenBestaandBeton { get; set; } // art. 4.4.1.2(9)
        public BetonStortOndergrondEnum? BetonStortOndergrond { get; set; } = BetonStortOndergrondEnum.GladdeBekistingOfNvt;

        public static Eurocode.Grondslagen.OntwerpLevensduurEnum GetLevensduurEnum(int jaren)
        {
            return jaren switch
            {
                5 => Grondslagen.OntwerpLevensduurEnum.Vijf,
                15 => Grondslagen.OntwerpLevensduurEnum.Vijftien,
                50 => Grondslagen.OntwerpLevensduurEnum.Vijftig,
                75 => Grondslagen.OntwerpLevensduurEnum.VijfEnZeventig,
                100 => Grondslagen.OntwerpLevensduurEnum.Honderd,
                _ => Grondslagen.OntwerpLevensduurEnum.Vijftig,
            };
        }
    }

    public static class BetonDekkingExtensions
    {



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



        public static double GetCminDur(this BetonDekkingContext dekking)
        {
            double returnval = 0;
            int row = dekking.Constructieklasse.Klasse - 1;

            foreach (MilieuklasseEnum mk in dekking.Milieuklassen)
            {
                var cMinDur = _tabelCminDur[row, mk.GetCminDurColumnIndex()];
                if (cMinDur > returnval)
                    returnval = cMinDur;
            }

            return returnval;
        }




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
        /// Verhoging van de dekking die rekening houd met uitvoeringstoleranties (Δc,dev), zie 4.4.1.3 (1)
        /// Ter info: De nominale dekking (c,nom) is gelijk aan c,min + Δc,dev volgens vergelijking (4.1)
        /// </summary>
        /// <param name="nb">De nationale bijlage</param>
        /// <returns></returns>
        public static double GetUitvoeringstoleraties(this NationaleBijlageEnum nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.Geen => 10.0,
                NationaleBijlageEnum.NL => 5.0,
                _ => 10.0,
            };
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
                NationaleBijlageEnum.Geen => 5.0,
                NationaleBijlageEnum.NL => 5.0,
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
                NationaleBijlageEnum.Geen => 10.0,
                NationaleBijlageEnum.NL => 10.0,
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
                NationaleBijlageEnum.Geen => 15.0,
                NationaleBijlageEnum.NL => 15.0,
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
            return dekking.NationaleBijlage switch
            {
                NationaleBijlageEnum.Geen => 40.00,
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
            return dekking.NationaleBijlage switch
            {
                NationaleBijlageEnum.Geen => 75.00,
                NationaleBijlageEnum.NL => dekking.GetCminDur() + 50,
                _ => 75.00
            };
        }


        public static double GetMinimaleBetondekking(this BetonDekkingContext dekking)
        {
            List<double> doubles = [dekking.GetCminDur(), dekking.BetondekkingMinBetonstaal, 10.00];
            return doubles.Max();
        }



    }
}