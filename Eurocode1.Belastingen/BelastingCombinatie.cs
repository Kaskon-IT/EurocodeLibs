using CommonLibrary;
using CommonLibrary.Extensions;
using CommonLibrary.Helpers;
using CommonLibrary.Interfaces;

namespace Eurocode.Belastingen
{
    public class BelastingCombinatie(int nr, BelastingCombinatieTypeEnum type) : BaseEurocodeContext, IMarkupConvertible
    {
        public override string ToString()
        {
            return $"{Naam,-8} {Type.GetDisplayName()} {UserFriendlyTextInclusiefMomentaanFactoren}";
        }



        protected override void Bereken()
        {
            // geen berekeningen
        }

        protected override bool Valideer()
        {
            // voeg eventueel validaties toe
            return true;
        }

        //public MarkupString ToMarkupString()
        //{
        //    return new MarkupString(ToString());
        // }

        [TableColumn("naam", order: -2, width: 2.0)]
        public string Naam { get { return "BC" + Nr.ToString("D1"); } }

        [TableColumn("type", order: 11, width: 4.0)]
        public BelastingCombinatieTypeEnum Type { get; set; } = type;

        public int Nr { get; set; } = nr;




        public List<BelastingCombinatieItem> Items { get; set; } = [];


        public string UserFriendlyText
        {
            get
            {
                List<string> strItems = [];
                foreach (var item in Items)
                {
                    strItems.Add(item.FactorNetto.ToString("0.00#") + " BG" + item.Geval.Nr);
                }
                return string.Join(" + ", [.. strItems]);
            }
        }

        [TableColumn("combinatie factoren", order: 21, width: 6.0)]
        public string UserFriendlyTextInclusiefMomentaanFactoren
        {
            get
            {
                List<string> strItems = [];
                foreach (var item in Items)
                {
                    if (item.MomentFactor.HasValue)
                    {
                        strItems.Add(item.FactorQ.ToString("0.00#") + " BG" + item.Geval.Nr + " × " + MarkupHelper.ToMarkupString(ExportFactory.Services.HtmlCreator.MarkdownToHtml(item.MomentaanTekst)) + " (=" + item.MomentFactor.Value.ToString("0.0") + ")");
                    }
                    else
                    {
                        strItems.Add(item.FactorNetto.ToString("0.00#") + " BG" + item.Geval.Nr);
                    }
                }
                return string.Join(" + ", [.. strItems]);
            }
        }






    }



}
