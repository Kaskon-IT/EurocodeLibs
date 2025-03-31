namespace Eurocode.BetonConstructies.SpecifiekeRegels
{
    public class Balken
    {

        public static double GetAsMax(ParametrischeProfielen.ParametrischProfielContext parametrischProfiel)
        {
            return GetAsMax(parametrischProfiel.Area);
        }

        /// <summary>
        /// Maximale wapening voor een balk (rechthoekige doorsnede)
        /// </summary>
        /// <param name="breedte">breedte</param>
        /// <param name="hoogte">hoogte</param>
        /// <returns></returns>
        public static double GetAsMax(double breedte, double hoogte)
        {
            return GetAsMax(breedte * hoogte);
        }


        /// <summary>
        /// 9.2.1.1 Minimum- en maximumwapeningsdoorsneden
        /// Haal A~s.max~ op
        /// (3) De waarde van A~s,max~ voor liggers moet gelijk aan 0,04 A~c~ zijn genomen.
        /// </summary>
        /// <returns></returns>
        public static double GetAsMax(double betonOppervlakAc)
        {
            return 0.04 * betonOppervlakAc;
        }






        /// <summary>
        /// 9.2.1.2
        /// (1)  In monoliete constructies behoort, zelfs indien in het ontwerp is uitgegaan van vrije opleggingen, de 
        /// doorsnede bij de opleggingen te zijn berekend op een buigend moment als gevolg van gedeeltelijke
        /// inklemming van ten minste β~1~ maal het maximale veldmoment.
        /// </summary>
        /// <param name="maximaalVeldMoment"></param>
        /// <returns>Toevallig inklemmingsmoment</returns>
        public static double GetToevalligInklemmingsMoment(double maximaalVeldMoment)
        {
            return _beta1 * maximaalVeldMoment;
        }

        const double _beta1 = 0.15;

        /// <summary>
        /// 9.2.2 Dwarskrachtwapening
        /// (1) De dwarskrachtwapening behoort een hoek |alpha| tussen 45° en 90° met de lengteas van het constructie-element te maken. 
        /// </summary>
        /// <returns></returns>
        public static (double min, double max) GetGrenswaardeHoekDwarskrachtWapening()
        {
            return (45, 90);
        }


    }


    /// <summary>
    /// 9.3 Massieve platen
    /// </summary>
    class MassievePlaten
    {

    }

    /// <summary>
    /// 9.4 Vlakke plaatvloeren
    /// </summary>
    class VlakkePlaatvloeren
    {

    }



    /// <summary>
    /// 9.5 Kolommen
    /// </summary>
    class Kolommen
    {

    }


    /// <summary>
    /// 9.6 Wanden
    /// </summary>
    class Wanden
    {

    }

    /// <summary>
    /// 9.7 Gedrongen liggers
    /// </summary>
    class GedrongenLiggers
    {

    }

    /// <summary>
    /// 9.8 Funderingen
    /// </summary>
    class Funderingen
    {

    }




    /// <summary>
    /// 9.10 Trekbanden
    /// </summary>
    class TrekBanden
    {

    }


}
