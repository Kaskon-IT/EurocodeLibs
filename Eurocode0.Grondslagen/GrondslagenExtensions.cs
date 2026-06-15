namespace Eurocode.Grondslagen
{
    public static class GrondslagenExtensions
    {
        public static double GetKfi(this BetrouwbaarheidsklasseEnum betrouwbaarheidsklasse)
        {

            return betrouwbaarheidsklasse switch
            {
                BetrouwbaarheidsklasseEnum.RC1 => 0.9,
                BetrouwbaarheidsklasseEnum.RC2 => 1.0,
                BetrouwbaarheidsklasseEnum.RC3 => 1.1,
                _ => 1.1,
            };
        }

        public static BetrouwbaarheidsklasseEnum GetBetrouwbaarheidsklasse(this GevolgklasseEnum gevolgklasse)
        {
            return gevolgklasse switch
            {
                GevolgklasseEnum.CC1 or GevolgklasseEnum.CC1a or GevolgklasseEnum.CC1b => BetrouwbaarheidsklasseEnum.RC1,
                GevolgklasseEnum.CC2 or GevolgklasseEnum.CC2a or GevolgklasseEnum.CC2b => BetrouwbaarheidsklasseEnum.RC2,
                GevolgklasseEnum.CC3 => BetrouwbaarheidsklasseEnum.RC3,
                _ => BetrouwbaarheidsklasseEnum.RC2
            };
        }

        /// <summary>
        /// ξ (xi) is een reductiefactor voor ongunstige, blijvende belastingen G
        /// Deze wordt gebruikt in de fundamentele combinatie (6.10b) en is afhankelijk van de nationale bijlage.
        /// </summary>
        /// <param name="nb"></param>
        /// <returns></returns>
        public static double GetReductieFactorVoorOngunstigeBlijvendeBelastingen(this NationaleBijlageEnum nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.EU => 1.15 / 1.35,
                NationaleBijlageEnum.NL => 1.20 / 1.35,
                _ => 1.15 / 1.35
            };
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
                NationaleBijlageEnum.EU => 10.0,
                NationaleBijlageEnum.NL => 5.0,
                _ => 10.0,
            };


        }


        public static string GetOntwerplevensduurTekst(this OntwerpLevensduurEnum OntwerpLevensduur)
        {
            var jaren = (int)OntwerpLevensduur;
            return $"{jaren} jaar";
        }

        //public static Dictionary<string, string> GetRowData(this GrondslagenContext context)
        //{
        //    Dictionary<string, string> rowData = new Dictionary<string, string>();

        //    foreach (var kvp in context.Headers)
        //    {
        //        var value = "";
        //        switch (kvp.Key)
        //        {
        //            case "NationaleBijlage": value = context.NationaleBijlage.GetValueOrDefault().ToString(); break;
        //            case "OntwerpLevensduur": value = context.OntwerpLevensduur.GetValueOrDefault().ToString(); break;
        //            case "Gevolgklasse": value = context.Gevolgklasse.GetValueOrDefault().ToString(); break;
        //            case "Betrouwbaarheidsklasse": value = context.Betrouwbaarheidsklasse.ToString(); break;
        //            case "Kfi": value = context.Kfi.ToString("0.0"); break;
        //            case "Xi": value = context.Xi.ToString("0.00"); break;
        //        }
        //        rowData.Add(kvp.Key, value);
        //    }
        //    return rowData;
        //}


    }
}
