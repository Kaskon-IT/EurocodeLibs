using ExportFactory.Shared;

namespace Eurocode.Belastingen
{
    public class BelastingGeval
    {
        public int Nr { get; set; }

        [TableColumn("Naam", order: 0)]
        public string Naam { get { return "BG" + Nr; } }
        public string Omschrijving { get; set; } = "G";

        [TableColumn("Type", order: 10)]
        public BelastingGevalTypeEnum? Type { get; set; } = BelastingGevalTypeEnum.Permanent;

        public BelastOnbelastTypeEnum? BelastOnbelastType { get; set; } = BelastOnbelastTypeEnum.AllesTegelijk;

        [TableColumn("Gebruiksklasse", order: 20)]
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



        // userFriendlyHelpers
        [TableColumn("Momentaan", order: 30)]
        public string MomentaanFactorenUserFriendly
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return $"|psi|~0~={MomentaanFactoren.Mom0} |psi|~1~={MomentaanFactoren.Mom1} |psi|~2~={MomentaanFactoren.Mom2}";
            }
        }


        [TableColumn("Opgelegde belasting", order: 40)]
        public string OpgelegdeBelasting
        {
            get
            {
                if (Nr == 2)
                {
                    return "q~k~=" + OpgelegdeBelastingen.Vlaklast.ToString("0.## kN/m²");
                }
                if (Nr == 3)
                {
                    return "Q~k~=" + OpgelegdeBelastingen.Puntlast.ToString("0.## kN");
                }
                else return "";
            }
        }




    }



}
