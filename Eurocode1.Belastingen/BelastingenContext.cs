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
                    Type = BelastingGeval.BelastingGevalTypeEnum.Permanent,
                    Gebruiksklasse = null,
                },
                new BelastingGeval(){
                    Nr = 2,
                    Omschrijving = "Q",
                    Type = BelastingGeval.BelastingGevalTypeEnum.Veranderlijk,
                    Gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_trappen,
                },
                new BelastingGeval(){
                    Nr = 3,
                    Omschrijving = "q",
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

            var bg1 = gevallen.FirstOrDefault(geval => geval.Nr == 1 && geval.Type == BelastingGeval.BelastingGevalTypeEnum.Permanent);
            var bg2 = gevallen.FirstOrDefault(geval => geval.Nr == 2 && geval.Type == BelastingGeval.BelastingGevalTypeEnum.Veranderlijk);
            var bg3 = gevallen.FirstOrDefault(geval => geval.Nr == 3 && geval.Type == BelastingGeval.BelastingGevalTypeEnum.Veranderlijk);

            // clear
            this.BelastingCombinaties = [];

            if (bg1 != null)
            {
                // genereer combinaties
                foreach (var type in combinatieTypes)
                {
                    //int aantalCombinaties = 0;
                    switch (type)
                    {
                        case BelastingCombinatieTypeEnum.Fundamenteel_A:
                        case BelastingCombinatieTypeEnum.Fundamenteel_B:
                            if (bg1 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));

                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                            }


                            if (bg1 != null && bg2 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg2));

                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg2));
                            }


                            if (bg1 != null && bg3 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg3));

                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg3));
                            }



                            break;



                            if (bg1 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));

                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                            }

                            for (int i = 0; i < 2; i++)
                            {
                                // 2 combinaties (ongustig, gunstig) * (G+Q*M0)
                                bool isGunstig = (i > 0);
                                BelastingCombinatie bc = new(BelastingCombinaties.Count + 1, type: type);
                                BelastingCombinatieItem bcItem1 = new(context, bc, bg1, isGunstig);
                                bc.Items.Add(bcItem1);
                                BelastingCombinatieItem bcItem2 = new(context, bc, bg2, isGunstig);
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
                            if (bg1 != null)
                                combi1.Items.Add(new BelastingCombinatieItem(context, combi1, bg1));
                            if (bg2 != null)
                                combi1.Items.Add(new BelastingCombinatieItem(context, combi1, bg2));
                            BelastingCombinaties.Add(combi1);

                            // zonder veranderlijk
                            BelastingCombinatie combi2 = new(BelastingCombinaties.Count + 1, type);
                            if (bg1 != null)
                                combi2.Items.Add(new BelastingCombinatieItem(context, combi2, bg1));
                            BelastingCombinaties.Add(combi2);
                            break;

                        case BelastingCombinatieTypeEnum.Karakteristiek:
                            // 1 combinatie karakteristiek
                            BelastingCombinatie karakteristiek = new(BelastingCombinaties.Count + 1, type);
                            if (bg1 != null)
                                karakteristiek.Items.Add(new BelastingCombinatieItem(context, karakteristiek, bg1));
                            if (bg2 != null)
                                karakteristiek.Items.Add(new BelastingCombinatieItem(context, karakteristiek, bg2));
                            BelastingCombinaties.Add(karakteristiek);
                            break;

                        case BelastingCombinatieTypeEnum.Blijvend:
                            // 1 combinatie blijvend is alleen permanent
                            BelastingCombinatie blijvend = new(BelastingCombinaties.Count + 1, type);
                            if (bg1 != null)
                                blijvend.Items.Add(new BelastingCombinatieItem(context, blijvend, bg1));
                            BelastingCombinaties.Add(blijvend);
                            break;
                    }
                }
            }
        }






    }



}
