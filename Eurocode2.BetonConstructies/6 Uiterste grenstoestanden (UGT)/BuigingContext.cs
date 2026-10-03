using Eurocode.Grondslagen;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// 6.1 
    /// </summary>
    class BuigingContext
    {
        // (9) In doorsneden van liggers belast op buiging zonder normaalkracht – anders dan door voorspanning –
        //moet de hoogte van de betondrukzone xu zijn beperkt.Deze beperking geldt ook als de
        //desbetreffende doorsnede is belast door een normaaldrukkracht – anders dan door voorspanning –
        //kleiner dan 0,1 fcd Ac.
        public static double GetMaximaleHoogteDrukzoneZonderVoorspanning(BetonContext beton, double nuttigeHoogte, double betonDoorsnedeOppervlak, double normaalkracht = 0, NationaleBijlageEnum nationaleBijlage = NationaleBijlageEnum.NL)
        {
            // xu is de hoogte van de betondrukzone bij dat deel van de wapening dat nodig is voor het
            // opnemen van de voorgeschreven belasting;

            // x~u,max~ = (|epsilon|~cu~E+6 / (|epsilon|~cu~E+6 + 7f) ) * d
            // waarbij f = f~yd~A~s~ / A~s~, of terwijl f~yd~ (zonder voorspanning)

            // we hebben nodig de NB, Beton en Betonstaal

            // controleer de normaalkracht
            double grenswaardeNormaalkracht;
            switch (nationaleBijlage)
            {
                default:
                case NationaleBijlageEnum.NL:
                    grenswaardeNormaalkracht = 0.1 * beton.Fcd * betonDoorsnedeOppervlak;
                    break;
            }

            // controleer of             
            if (normaalkracht > grenswaardeNormaalkracht)
            {
                return double.MaxValue; // geen 
                throw new NotImplementedException("maximale hoogte betondrukzone niet van toepassing");
            }

            double xuMax;
            switch (nationaleBijlage)
            {
                default:
                case NationaleBijlageEnum.NL:
                    double f = beton.BetonStaal.Fyd;
                    //double d = nuttigeHoogte;
                    xuMax = beton.EpsilonCu * 1e6 / (beton.EpsilonCu * 1e6 + 7 * f) * nuttigeHoogte;
                    break;
            }

            return xuMax;



        }


        // 5.3.1 (3) Een balk is een element waarvan de overspanning niet kleiner is dan driemaal de totale hoogte van de 
        // doorsnede.In andere gevallen behoort deze te zijn beschouwd als een gedrongen ligger.
        public static bool IsGedrongen(double overspanning, double totaleHoogteVanDeDoorsnede)
        {
            // een balk is een element waarvan de overspanning niet kleiner is dan driemaal de totale hoogte van de doorsnede.
            // In andere gevallen behoort deze te zijn beschouwd als een gedrongen ligger.
            return overspanning < 3 * totaleHoogteVanDeDoorsnede;
        }





        // (10) Voor gedrongen constructies, zoals de in (1)P beschreven discontinue gebieden, mag de grootte van 
        // de inwendige hefboomarm zijn afgeleid uit de volgende relaties:
        /// <summary>
        /// (10) Voor gedrongen constructies, zoals de in (1)P beschreven discontinue gebieden, mag de grootte van 
        /// de inwendige hefboomarm zijn afgeleid uit de volgende relaties:
        /// </summary>
        /// <param name="lengte">afstand l, l~0~ of a</param>
        /// <param name="h">hoogte van de doorsnede</param>
        /// <param name="statischBepaald"></param>
        /// <param name="isUitkraging"></param>
        /// <param name="nationaleBijlage"></param>
        /// <returns></returns>
        public static double GetInwendigeHefboomArmGedrongenLigger(double lengte, double h, bool statischBepaald = true, bool isUitkraging = false, NationaleBijlageEnum nationaleBijlage = NationaleBijlageEnum.NL)
        {
            switch (nationaleBijlage)
            {
                default:
                case NationaleBijlageEnum.NL:
                    if (isUitkraging)
                    {
                        // 0,4 a + 0,4 h ≤ 1,6 a
                        return Math.Min(0.4 * lengte + 0.4 * h, 1.6 * lengte);
                    }
                    else if (statischBepaald)
                    {
                        // 0,2 l + 0,4 h ≤ 0,6 l
                        return Math.Min(0.2 * lengte + 0.4 * h, 0.6 * lengte);
                    }
                    else
                    {
                        // 0,3 lo + 0,3 h ≤ 0,8 lo
                        return Math.Min(0.3 * lengte + 0.3 * h, 0.8 * lengte);
                    }
            }

        }

        public static double GetInwendigeHefboomArm(double d, double x, BetonContext beton)
        {
            // geen hellende tak B500
            if (beton.BetonStaal.SpanningRekDiagram == BetonStaalContext.SpanningRekDiagramType.HellendeTak)
            {
                return d - x * beton.GetBeta();
                throw new NotImplementedException("Hellende tak betonstaal, nog niet verwerkt");

            }
            return d - x * beton.GetBeta();
        }

        public static double GetAsBenodigdGedrongenLigger(double m, double z, BetonContext beton)
        {
            // MRd = z As fyd, maar niet groter dan volgt uit(1)P.
            // As = MRd [Nmm] / z / fyd


            return m * 1e+6 / z / beton.BetonStaal.Fyd;

        }

        public static double GetMomentOpneembaarGedrongenLigger(double dsnOpp_As, double z, BetonContext beton)
        {
            return z * dsnOpp_As * beton.BetonStaal.Fyd * 1e-6;
        }




        public static double GetLengteArmInBerekeningUitkraging(double ab, double l, double h)
        {
            // 6.1 (10)
            // a is de afstand tussen de resultante van de belasting en de resultante van de reactiekracht of 
            // de kracht in de ophangwapening. Voor de positie van de resultante van de reactiekracht bij
            // een console of een uitkraging mag zijn aangenomen dat deze ligt op een afstand gelijk aan
            // de kleinste waarde van ab / 2, L / 4 en h/ 4 binnen de dag van de oplegging
            var doubles = new List<double>()
            {
                ab/2, l/4, h/4
            };
            return doubles.Min();

        }


    }
}
