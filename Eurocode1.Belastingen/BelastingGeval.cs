namespace Eurocode.Belastingen
{
    public class BelastingGeval
    {
        public int Nr { get; set; }
        public string Naam { get { return "BG" + Nr; } }
        public string Omschrijving { get; set; } = "G";
        public BelastingGevalTypeEnum? Type { get; set; } = BelastingGevalTypeEnum.Permanent;
        public BelastOnbelastTypeEnum? BelastOnbelastType { get; set; } = BelastOnbelastTypeEnum.AllesTegelijk;
        public GebruiksklasseEnum? Gebruiksklasse { get; set; } = GebruiksklasseEnum.A_gemeenschappelijke_trappen;
        public OpgelegdeBelastingen OpgelegdeBelastingen { get { return Gebruiksklasse.HasValue ? Gebruiksklasse.Value.GetOpgelegdeBelastingen() : new(); } }
        public MomentaanFactoren MomentaanFactoren { get { return Gebruiksklasse.HasValue ? Gebruiksklasse.Value.GetMomentaanFactoren() : new(); } }



        public enum BelastingGevalTypeEnum
        {
            Permanent,
            Veranderlijk,
        }

        public enum BelastOnbelastTypeEnum
        {
            AllesTegelijk,
            Schaakbord
        }

    }



}
