using ExportFactory.Shared;

namespace Eurocode.Belastingen
{
    public class BelastingCombinatie(int nr, BelastingCombinatieTypeEnum type)
    {
        [TableColumn("Type", order: 11)]
        public BelastingCombinatieTypeEnum Type { get; set; } = type;

        public int Nr { get; set; } = nr;

        [TableColumn("Naam", order: -2)]
        public string Naam { get { return "BC" + Nr; } }


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

        [TableColumn("Combinatie", order: 21)]
        public string UserFriendlyTextInclusiefMomentaanFactoren
        {
            get
            {
                List<string> strItems = [];
                foreach (var item in Items)
                {
                    if (item.MomentFactor.HasValue)
                    {
                        strItems.Add(item.FactorQ.ToString("0.00#") + " × " + item.MomentaanTekst + " (=" + item.MomentFactor.Value.ToString("0.0") + ")" + " BG" + item.Geval.Nr);
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
