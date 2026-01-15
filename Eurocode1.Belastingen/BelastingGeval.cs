using CommonLibrary;
using CommonLibrary.Extensions;
using CommonLibrary.Interfaces;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Eurocode.Belastingen
{
    public class BelastingGeval : BaseEurocodeContext, IContext, IMarkupConvertible
    {
        public override string ToString()
        {
            return $"{Naam,-8} {Type.GetDisplayName(),-12} {Gebruiksklasse.GetDisplayName()} {OpgelegdeBelasting,-12} {MomentaanFactorenUserFriendly}";
        }

        public MarkupString ToMarkupString()
        {
            return new MarkupString(ToString());
        }



        protected override void Bereken()
        {
            return;
        }

        protected override bool Valideer()
        {
            return true;
        }

        public int Nr { get; set; }

        [TableColumn("naam", order: 0, width: 2.0)]
        public string Naam { get { return "BG" + Nr.ToString("D1"); } }
        public string Omschrijving { get; set; } = "G";

        [TableColumn("type", order: 10, width: 3.0)]
        public BelastingGevalTypeEnum? Type { get; set; } = BelastingGevalTypeEnum.Permanent;

        public BelastOnbelastTypeEnum? BelastOnbelastType { get; set; } = BelastOnbelastTypeEnum.AllesTegelijk;


        private GebruiksklasseEnum? _gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_trappen;

        [TableColumn("gebruiksklasse", order: 20, width: 7.0)]
        public GebruiksklasseEnum? Gebruiksklasse
        {
            get => _gebruiksklasse;
            set => SetProperty(ref _gebruiksklasse, value);
        }

        public string GebruiksklasseUserFriendly
        {
            get
            {
                if (Gebruiksklasse.HasValue)
                {
                    return Gebruiksklasse.Value.GetDisplayName();
                }
                else
                {
                    return "";
                }
            }
        }


        public string Opmerking { get; set; } = "";


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
        //[TableColumn("momentaan factoren", order: 30, width: 4.0)]
        public string MomentaanFactorenUserFriendly
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return $"|psi|~0~={MomentaanFactoren.Mom0} |psi|~1~={MomentaanFactoren.Mom1} |psi|~2~={MomentaanFactoren.Mom2}";
            }
        }


        [TableColumn("|psi|~0~", order: 40, width: 2.0)]
        public string Mom0
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return MomentaanFactoren.Mom0.ToString("0.##");
            }
        }

        [TableColumn("|psi|~1~", order: 41, width: 2.0)]
        public string Mom1
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return MomentaanFactoren.Mom1.ToString("0.##");
            }
        }

        [TableColumn("|psi|~2~", order: 42, width: 2.0)]
        public string Mom2
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return MomentaanFactoren.Mom2.ToString("0.##");
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
