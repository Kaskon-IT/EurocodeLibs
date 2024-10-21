namespace Eurocode.Grondslagen
{
    public class GrondslagenContext
    {

        public NationaleBijlageEnum? NationaleBijlage { get; set; } = NationaleBijlageEnum.NL;

        /// <summary>
        /// B.3.1 Gevolgklassen
        /// (1) Ten behoeve van de betrouwbaarheidsdifferentiatie, mogen gevolgklassen (CC), zoals gegeven in tabel B1, 
        /// worden gedefinieerd door het beschouwen van de gevolgen van bezwijken of het slecht functioneren van de
        /// constructie
        /// </summary>
        public GevolgklasseEnum? Gevolgklasse { get; set; } = GevolgklasseEnum.CC2;

        public OntwerpLevensduurEnum? OntwerpLevensduur { get; set; } = OntwerpLevensduurEnum.Vijftig;




        public BetrouwbaarheidsklasseEnum Betrouwbaarheidsklasse
        {
            get { return this.Gevolgklasse.GetBetrouwbaarheidsklasse(); }
        }


        /// <summary>
        /// B.3.3
        /// Vermenigvuldigingsfactor KFI die wordt toegepast op de partiele factoren.
        /// </summary>
        public double Kfi
        {
            get { return this.Betrouwbaarheidsklasse.GetKfi(); }
        }

        /// <summary>
        /// ξ (xi) is een reductiefactor voor ongunstige, blijvende belastingen G
        /// Deze wordt gebruikt in de fundamentele combinatie (6.10b) en is afhankelijk van de nationale bijlage.
        /// </summary>
        public double Xi
        {
            get { return this.NationaleBijlage.GetReductieFactorVoorOngunstigeBlijvendeBelastingen(); }
        }




    }

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

        public static BetrouwbaarheidsklasseEnum GetBetrouwbaarheidsklasse(this GevolgklasseEnum? gevolgklasse)
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
        public static double GetReductieFactorVoorOngunstigeBlijvendeBelastingen(this NationaleBijlageEnum? nb)
        {
            return nb switch
            {
                NationaleBijlageEnum.Geen => 1.15 / 1.35,
                NationaleBijlageEnum.NL => 1.2 / 1.35,
                _ => 1.15 / 1.35
            };
        }

    }
}
