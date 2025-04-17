using CommonLibrary.Extensions;
using CommonLibrary.Interfaces;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public class BelastingGeval : IMarkupConvertible
    {
        public override string ToString()
        {
            return $"{Naam,-8} {Type.GetDisplayName(),-12} {Gebruiksklasse.GetDisplayName()} {OpgelegdeBelasting,-12} {MomentaanFactorenUserFriendly}";
        }

        public MarkupString ToMarkupString()
        {
            return new MarkupString(ToString());
        }

        public int Nr { get; set; }

        [TableColumn("naam", order: 0)]
        public string Naam { get { return "BG" + Nr.ToString("D1"); } }
        public string Omschrijving { get; set; } = "G";

        [TableColumn("type", order: 10)]
        public BelastingGevalTypeEnum? Type { get; set; } = BelastingGevalTypeEnum.Permanent;

        public BelastOnbelastTypeEnum? BelastOnbelastType { get; set; } = BelastOnbelastTypeEnum.AllesTegelijk;

        [TableColumn("gebruiksklasse", order: 20)]
        public GebruiksklasseEnum? Gebruiksklasse { get; set; } = GebruiksklasseEnum.A_gemeenschappelijke_trappen;


        public OpgelegdeBelastingen OpgelegdeBelastingen { get { return Gebruiksklasse.HasValue ? Gebruiksklasse.Value.GetOpgelegdeBelastingen() : new(); } }


        public MomentaanFactoren MomentaanFactoren { get { return Gebruiksklasse.HasValue ? Gebruiksklasse.Value.GetMomentaanFactoren() : new(); } }



        public enum BelastingGevalTypeEnum
        {
            [Description("permanent")]
            Permanent,
            [Description("veranderlijk")]
            Veranderlijk,
        }

        public enum BelastOnbelastTypeEnum
        {
            AllesTegelijk,
            Schaakbord
        }



        // userFriendlyHelpers
        [TableColumn("momentaan factoren", order: 30)]
        public string MomentaanFactorenUserFriendly
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return $"|psi|~0~={MomentaanFactoren.Mom0} |psi|~1~={MomentaanFactoren.Mom1} |psi|~2~={MomentaanFactoren.Mom2}";
            }
        }


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
