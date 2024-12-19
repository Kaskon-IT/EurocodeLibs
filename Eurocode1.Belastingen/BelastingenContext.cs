using Eurocode.Grondslagen;

namespace Eurocode.Belastingen
{





    public class BelastingenContext(GrondslagenContext grondslagen)
    {
        public GrondslagenContext Grondslagen = grondslagen;

        public List<BelastingGeval> BelastingGevallen { get; set; } =
            [
                new BelastingGeval(){
                    Nr = 1,
                    Omschrijving = "G",
                    Type = BelastingGeval.BelastingGevalTypeEnum.Permanent
                },
                new BelastingGeval(){
                    Nr = 2,
                    Omschrijving = "Q",
                    Type = BelastingGeval.BelastingGevalTypeEnum.Veranderlijk,
                    Gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_trappen,
                }
            ];


        public List<BelastingCombinatieTypeEnum> CombinatiesTypes { get; set; } = [
            BelastingCombinatieTypeEnum.Fundamenteel_A,
            BelastingCombinatieTypeEnum.Fundamenteel_B,
            BelastingCombinatieTypeEnum.Frequent,
            BelastingCombinatieTypeEnum.QuasiBlijvend,
            BelastingCombinatieTypeEnum.Karakteristiek

            ];


        public List<BelastingCombinatie> BelastingCombinaties { get; set; } = [];



        public void GenereerBelastingCombinaties(BelastingenContext context, List<BelastingGeval> gevallen, List<BelastingCombinatieTypeEnum> combinatieTypes)
        {
            // alleen voor situatie met 1 permanent, en 1 verankedelijk...
            // todo meer opties met meerdere gevallen.
            // todo ook keuze zonder combinaties (gunstig permanent)
            // todo ook keuze zonder combinaties met alleen permanent

            var g = gevallen.FirstOrDefault(geval => geval.Type == BelastingGeval.BelastingGevalTypeEnum.Permanent);
            var q = gevallen.FirstOrDefault(geval => geval.Type == BelastingGeval.BelastingGevalTypeEnum.Veranderlijk);

            // clear
            this.BelastingCombinaties = [];

            if (g != null && q != null)
            {
                // genereer combinaties
                foreach (var type in combinatieTypes)
                {
                    switch (type)
                    {
                        case BelastingCombinatieTypeEnum.Fundamenteel_A:

                            // 4 combinaties (gunstig, ongunstig) * (alleen G, G+Q)
                            for (int i = 0; i < 4; i++)
                            {
                                bool isGunstig = (i > 1);
                                BelastingCombinatie bc = new(BelastingCombinaties.Count + 1, type: type);
                                BelastingCombinatieItem bcItem1 = new(context, bc, g, isGunstig);
                                bc.Items.Add(bcItem1);

                                // 
                                if (i == 1 || i == 3)
                                {
                                    BelastingCombinatieItem bcItem2 = new(context, bc, q, isGunstig);
                                    bc.Items.Add(bcItem2);
                                }
                                BelastingCombinaties.Add(bc);
                            }
                            break;

                        case BelastingCombinatieTypeEnum.Fundamenteel_B:
                            for (int i = 0; i < 2; i++)
                            {
                                // 2 combinaties (ongustig, gunstig) * (G+Q*M0)
                                bool isGunstig = (i > 0);
                                BelastingCombinatie bc = new(BelastingCombinaties.Count + 1, type: type);
                                BelastingCombinatieItem bcItem1 = new(context, bc, g, isGunstig);
                                bc.Items.Add(bcItem1);
                                BelastingCombinatieItem bcItem2 = new(context, bc, q, isGunstig);
                                bc.Items.Add(bcItem2);
                                BelastingCombinaties.Add(bc);
                            }
                            break;



                        case BelastingCombinatieTypeEnum.Aardbeving:
                        case BelastingCombinatieTypeEnum.QuasiBlijvend:
                        case BelastingCombinatieTypeEnum.Frequent:
                        case BelastingCombinatieTypeEnum.Brand:
                            // met veranderlijk
                            BelastingCombinatie combi1 = new(BelastingCombinaties.Count + 1, type);
                            combi1.Items.Add(new BelastingCombinatieItem(context, combi1, g));
                            combi1.Items.Add(new BelastingCombinatieItem(context, combi1, q));
                            BelastingCombinaties.Add(combi1);
                            // zonder veranderlijk
                            BelastingCombinatie combi2 = new(BelastingCombinaties.Count + 1, type);
                            combi2.Items.Add(new BelastingCombinatieItem(context, combi2, g));
                            BelastingCombinaties.Add(combi2);
                            break;

                        case BelastingCombinatieTypeEnum.Karakteristiek:
                            // 1 combinatie karakteristiek
                            BelastingCombinatie karakteristiek = new(BelastingCombinaties.Count + 1, type);
                            karakteristiek.Items.Add(new BelastingCombinatieItem(context, karakteristiek, g));
                            karakteristiek.Items.Add(new BelastingCombinatieItem(context, karakteristiek, q));
                            BelastingCombinaties.Add(karakteristiek);
                            break;

                        case BelastingCombinatieTypeEnum.Blijvend:
                            // 1 combinatie blijvend is alleen permanent
                            BelastingCombinatie blijvend = new(BelastingCombinaties.Count + 1, type);
                            blijvend.Items.Add(new BelastingCombinatieItem(context, blijvend, g));
                            BelastingCombinaties.Add(blijvend);
                            break;
                    }
                }
            }
        }






    }



}
