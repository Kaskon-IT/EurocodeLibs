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


        public List<BelastingCombinatie> BelastingCombinaties { get; set; } =
            [
                new BelastingCombinatie(1, BelastingCombinatieTypeEnum.Fundamenteel_A),
                new BelastingCombinatie(2, BelastingCombinatieTypeEnum.Fundamenteel_B),
                new BelastingCombinatie(3, BelastingCombinatieTypeEnum.Frequent),
                new BelastingCombinatie(4, BelastingCombinatieTypeEnum.QuasiBlijvend),
                new BelastingCombinatie(5, BelastingCombinatieTypeEnum.Karakteristiek),
            ];



        public void GenereerBelastingCombinaties(BelastingenContext context, List<BelastingGeval> gevallen, List<BelastingCombinatieTypeEnum> combinatieTypes)
        {
            // alleen voor situatie met 1 permanent, en 1 verankedelijk... todo meer opties met meerdere gevallen.

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
                                bool gunstig = (i > 1);
                                BelastingCombinatie bc = new(BelastingCombinaties.Count + 1, type: type);
                                BelastingCombinatieItem bcItem1 = new()
                                {
                                    Context = context,
                                    Combinatie = bc,
                                    Geval = g,
                                    PermanentIsGunstig = gunstig,
                                };

                                bc.Items.Add(bcItem1);

                                // 
                                if (i == 1 || i == 3)
                                {
                                    BelastingCombinatieItem bcItem2 = new()
                                    {
                                        Context = context,
                                        Combinatie = bc,
                                        Geval = q,
                                        PermanentIsGunstig = gunstig,
                                    };
                                    bc.Items.Add(bcItem2);
                                }
                                BelastingCombinaties.Add(bc);
                            }
                            break;

                        case BelastingCombinatieTypeEnum.Fundamenteel_B:
                            for (int i = 0; i < 2; i++)
                            {
                                // 2 combinaties (ongustig, gunstig) * (G+Q*M0)
                                bool gunstig = (i > 0);
                                BelastingCombinatie bc = new(BelastingCombinaties.Count + 1, type: type);
                                BelastingCombinatieItem bcItem1 = new()
                                {
                                    Context = context,
                                    Combinatie = bc,
                                    Geval = g,
                                    PermanentIsGunstig = gunstig,
                                };
                                bc.Items.Add(bcItem1);
                                BelastingCombinatieItem bcItem2 = new()
                                {
                                    Context = context,
                                    Combinatie = bc,
                                    Geval = q,
                                    PermanentIsGunstig = gunstig,
                                };
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
                            combi1.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = combi1, Geval = g });
                            combi1.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = combi1, Geval = q });
                            BelastingCombinaties.Add(combi1);
                            // zonder veranderlijk
                            BelastingCombinatie combi2 = new(BelastingCombinaties.Count + 1, type);
                            combi2.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = combi2, Geval = g });
                            BelastingCombinaties.Add(combi2);
                            break;

                        case BelastingCombinatieTypeEnum.Karakteristiek:
                            // 1 combinatie karakteristiek
                            BelastingCombinatie karakteristiek = new(BelastingCombinaties.Count + 1, type);
                            karakteristiek.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = karakteristiek, Geval = g });
                            karakteristiek.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = karakteristiek, Geval = q });
                            BelastingCombinaties.Add(karakteristiek);
                            break;

                        case BelastingCombinatieTypeEnum.Blijvend:
                            // 1 combinatie blijvend is alleen permanent
                            BelastingCombinatie blijvend = new(BelastingCombinaties.Count + 1, type);
                            blijvend.Items.Add(new BelastingCombinatieItem() { Context = context, Combinatie = blijvend, Geval = g });
                            BelastingCombinaties.Add(blijvend);
                            break;
                    }
                }
            }
        }






    }




    public class VergeetMijNietje
    {
        public double L { get; set; }
        public double M { get; set; }
        public double F { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double X { get; set; }

        public enum BelastingTypeEnum
        {
            Puntlast,
            Lijnlast,
            Trapezium
        }

        public enum VergeetMijNietjeEnum
        {
            ScharnierRol,
            InklemmingInklemming,
            InklemmingRol,

        }
    }



}
