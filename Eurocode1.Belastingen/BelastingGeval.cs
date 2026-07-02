using CommonLibrary;
using CommonLibrary.Extensions;
using CommonLibrary.Interfaces;
using Eurocode.Grondslagen;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Text.Json.Serialization;

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

        [TableColumn("belastinggeval", order: 0, width: 3.0)]
        public string Naam { get { return "BG" + Nr.ToString("D1"); } }
        public string Omschrijving { get; set; } = "G";

        [TableColumn("type", order: 10, width: 3.0)]
        public BelastingGevalTypeEnum? Type { get; set; } = BelastingGevalTypeEnum.Permanent;

        public BelastOnbelastTypeEnum? BelastOnbelastType { get; set; } = BelastOnbelastTypeEnum.AllesTegelijk;


        private GebruiksklasseEnum? _gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_vloeren;

        [TableColumn("gebruiksklasse", order: 20, width: 7.0)]
        public GebruiksklasseEnum? Gebruiksklasse
        {
            get => _gebruiksklasse;
            set => SetProperty(ref _gebruiksklasse, value);
        }




        private OpgelegdeBelastingen _eigenOpgaveOpgelegdeBelastingen = new(3.0, 3.0);
        /// <summary>
        /// Opgelegde belastingen bij <see cref="GebruiksklasseEnum.EigenOpgave"/>.
        /// Wordt geserialiseerd; bij andere gebruiksklassen genegeerd.
        /// </summary>
        public OpgelegdeBelastingen EigenOpgaveOpgelegdeBelastingen
        {
            get => _eigenOpgaveOpgelegdeBelastingen;
            set => SetProperty(ref _eigenOpgaveOpgelegdeBelastingen, value);
        }

        private MomentaanFactoren _eigenOpgaveMomentaanFactoren = new(0.4, 0.5, 0.3);
        /// <summary>
        /// Momentaanfactoren bij <see cref="GebruiksklasseEnum.EigenOpgave"/>.
        /// Wordt geserialiseerd; bij andere gebruiksklassen genegeerd.
        /// </summary>
        public MomentaanFactoren EigenOpgaveMomentaanFactoren
        {
            get => _eigenOpgaveMomentaanFactoren;
            set => SetProperty(ref _eigenOpgaveMomentaanFactoren, value);
        }

        // Platte bindbare properties voor de eigen-opgave waarden (structs zijn niet direct bindbaar)
        [JsonIgnore] public double EigenOpgaveVlaklast  { get => EigenOpgaveOpgelegdeBelastingen.Vlaklast;  set { var v = EigenOpgaveOpgelegdeBelastingen; v.Vlaklast  = value; EigenOpgaveOpgelegdeBelastingen = v; } }
        [JsonIgnore] public double EigenOpgavePuntlast  { get => EigenOpgaveOpgelegdeBelastingen.Puntlast;  set { var v = EigenOpgaveOpgelegdeBelastingen; v.Puntlast  = value; EigenOpgaveOpgelegdeBelastingen = v; } }
        [JsonIgnore] public double EigenOpgaveMom0      { get => EigenOpgaveMomentaanFactoren.Mom0;         set { var v = EigenOpgaveMomentaanFactoren;      v.Mom0      = value; EigenOpgaveMomentaanFactoren     = v; } }
        [JsonIgnore] public double EigenOpgaveMom1      { get => EigenOpgaveMomentaanFactoren.Mom1;         set { var v = EigenOpgaveMomentaanFactoren;      v.Mom1      = value; EigenOpgaveMomentaanFactoren     = v; } }
        [JsonIgnore] public double EigenOpgaveMom2      { get => EigenOpgaveMomentaanFactoren.Mom2;         set { var v = EigenOpgaveMomentaanFactoren;      v.Mom2      = value; EigenOpgaveMomentaanFactoren     = v; } }

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


        /// <summary>
        /// Grondslagen van het project. Wordt gezet via BelastingenContext.
        /// Gebruikt voor de ontwerplevensduur-factor op de opgelegde belastingen.
        /// </summary>
        [JsonIgnore]
        public GrondslagenContext? Grondslagen { get; set; }

        public OpgelegdeBelastingen OpgelegdeBelastingen
        {
            get
            {
                if (!Gebruiksklasse.HasValue) return new();
                if (Gebruiksklasse == GebruiksklasseEnum.EigenOpgave) return EigenOpgaveOpgelegdeBelastingen;
                var basis = Gebruiksklasse.Value.GetOpgelegdeBelastingen();
                if (Grondslagen?.OntwerpLevensduur == OntwerpLevensduurEnum.Honderd)
                    return new OpgelegdeBelastingen(basis.Vlaklast * 1.04, basis.Puntlast * 1.04)
                    {
                        LijnlastRand = basis.LijnlastRand * 1.04
                    };
                return basis;
            }
        }


        public MomentaanFactoren MomentaanFactoren
        {
            get
            {
                if (Gebruiksklasse == GebruiksklasseEnum.EigenOpgave) return EigenOpgaveMomentaanFactoren;
                return Gebruiksklasse.HasValue ? Gebruiksklasse.Value.GetMomentaanFactoren() : new();
            }
        }



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
                else return $"*|psi|~0~*={MomentaanFactoren.Mom0} *|psi|~1~*={MomentaanFactoren.Mom1} *|psi|~2~*={MomentaanFactoren.Mom2}";
            }
        }


        [TableColumn("*ψ~0~*", order: 40, width: 1.0)]
        public string Mom0
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return MomentaanFactoren.Mom0.ToString("0.##");
            }
        }

        [TableColumn("*ψ~1~*", order: 41, width: 1.0)]
        public string Mom1
        {
            get
            {
                if (Type == BelastingGevalTypeEnum.Permanent) return "";
                else return MomentaanFactoren.Mom1.ToString("0.##");
            }
        }

        [TableColumn("*|psi|~2~*", order: 42, width: 1.0)]
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
                    return "*q~k~*=" + OpgelegdeBelastingen.Vlaklast.ToString("0.## kN/m²");
                }
                if (Nr == 3)
                {
                    return "*Q~k~*=" + OpgelegdeBelastingen.Puntlast.ToString("0.## kN");
                }
                else return "";
            }
        }




    }



}
