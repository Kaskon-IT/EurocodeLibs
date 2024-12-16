namespace Eurocode.Belastingen
{
    public class BelastingCombinatie(int nr, BelastingCombinatieTypeEnum type)
    {
        public BelastingCombinatieTypeEnum Type { get; set; } = type;

        public int Nr { get; set; } = nr;
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





        /// <summary>
        /// Verwijzing naar de Eurocode 0.
        /// Noodzakelijk voor factoren Xi en KFI.
        /// </summary>
        //public Eurocode.Grondslagen.GrondslagenContext Grondslagen;



    }



}
