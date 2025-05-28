using System.ComponentModel.DataAnnotations;

namespace Eurocode.BetonConstructies
{
    public static class Schematisering
    {

        public enum ConstructiefModelEnum
        {
            Balk, Plaat, Kolom
        }

        public enum GedrongenEnum
        {
            //[Display(Name = "Niet gedrongen")]
            //NietGedrongen = 0,
            [Display(Name = "uitkraging")]
            Uitkraging = 1,
            [Display(Name = "statisch bepaald")]
            StatischBepaald = 2,
            [Display(Name = "statisch onbepaald")]
            StatischOnbepaald = 3

        }

        public static double GetGedrongenZ(double lengte, double h, GedrongenEnum gedrongen)
        {
            switch (gedrongen)
            {
                default:
                case GedrongenEnum.Uitkraging:
                    // 0,4 a + 0,4 h ≤ 1,6 a
                    return Math.Min(0.4 * lengte + 0.4 * h, 1.6 * lengte);
                case GedrongenEnum.StatischBepaald:
                    // 0,2 l + 0,4 h ≤ 0,6 l
                    return Math.Min(0.2 * lengte + 0.4 * h, 0.6 * lengte);
                case GedrongenEnum.StatischOnbepaald:
                    // 0,3 lo + 0,3 h ≤ 0,8 lo
                    return Math.Min(0.3 * lengte + 0.3 * h, 0.8 * lengte);



            }

        }



        public static double GetAsMax(this ConstructiefModelEnum? constructiefModel, double betonDoorsnedeOppervlak)
        {

            switch (constructiefModel)
            {
                default:
                case ConstructiefModelEnum.Balk:
                case ConstructiefModelEnum.Plaat:
                    return SpecifiekeRegels.Balken.GetAsMax(betonDoorsnedeOppervlak);
                case ConstructiefModelEnum.Kolom:
                    return SpecifiekeRegels.Kolommen.GetAsMax(betonDoorsnedeOppervlak);


            }
        }

        /// <summary>
        /// 5.3.1 (3) Een balk is een element waarvan de overspanning niet kleiner is dan driemaal te totale hoogte van de doorsnede. 
        /// In andere gevallen als gedrongen ligger beschouwen. 
        /// </summary>
        /// <param name="overspanning"></param>
        /// <param name="totaleHoogte"></param>
        /// <returns></returns>
        public static bool IsGedrongenLigger(double overspanning, double totaleHoogte)
        {
            return overspanning < 3 * totaleHoogte;
        }

        /// <summary>
        /// 5.3.1 (3) Als het geen gedrongen ligger is als balk beschouwen.
        /// </summary>
        /// <param name="overspanning"></param>
        /// <param name="totaleHoogte"></param>
        /// <returns></returns>
        public static bool IsBalk(double overspanning, double totaleHoogte)
        {
            return !IsGedrongenLigger(overspanning, totaleHoogte);
        }


        /// <summary>
        /// 5.3.1 (4) Een plaat is een element waarvan de kleinste waarde van de lengte of de breedte niet kleiner is dan
        /// vijfmaal de totale plaatdikte.
        /// </summary>
        /// <param name="lengte"></param>
        /// <param name="breedte"></param>
        /// <param name="totaleDikte"></param>
        /// <returns></returns>
        public static bool IsPlaat(double lengte, double breedte, double totaleDikte)
        {
            return Math.Min(lengte, breedte) > 5 * totaleDikte;
        }


        /// <summary>
        /// 5.3.1 (7) Een kolom is een element waarvan de langste zijde van de doorsnede niet groter is dan viermaal de 
        /// kortste zijde en waarvan de hoogte ten minste gelijk is aan driemaal de langste zijde van de doorsnede.In
        /// andere gevallen behoort het element te zijn beschouwd als een wand.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool IsKolom(double maatLangsteZijde, double maatKortsteZijde, double hoogte)
        {
            bool voorwaarde1 = maatLangsteZijde <= 4 * maatKortsteZijde;
            bool voorwaarde2 = hoogte >= 3 * maatLangsteZijde;
            return voorwaarde1 && voorwaarde2;
        }

        /// <summary>
        /// 5.3.1 (7) Als het geen kolom is dan als wand beschouwen.
        /// </summary>
        /// <param name="maatLangsteZijde"></param>
        /// <param name="maatKortsteZijde"></param>
        /// <param name="hoogte"></param>
        /// <returns></returns>
        public static bool IsWand(double maatLangsteZijde, double maatKortsteZijde, double hoogte)
        {
            return !IsKolom(maatLangsteZijde, maatKortsteZijde, hoogte);
        }


    }
}
