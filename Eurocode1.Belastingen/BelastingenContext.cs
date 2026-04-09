using CommonLibrary;
using Eurocode.Grondslagen;
using System.Text.Json.Serialization;

namespace Eurocode.Belastingen
{





    public class BelastingenContext : BaseEurocodeContext
    {

        public BelastingenContext()
        {
            Grondslagen = new();
        }

        public override string? ToString()
        {
            return base.ToString();
        }


        [JsonConstructor]
        public BelastingenContext(GrondslagenContext grondslagen)
        {
            Grondslagen = grondslagen;


            GenereerBelastingCombinaties(this, this.BelastingGevallen, this.CombinatiesTypes);
        }

        private GrondslagenContext _grondslagen = new();

        public GrondslagenContext Grondslagen
        {
            get => _grondslagen;
            set
            {
                _grondslagen = value;
                foreach (var bg in BelastingGevallen)
                    bg.Grondslagen = value;
            }
        }




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
                    Omschrijving = "q",
                    Type = BelastingGeval.BelastingGevalTypeEnum.Veranderlijk,
                    Gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_trappen,
                    Opmerking = "vlaklast"
                },

                //new BelastingGeval(){
                //    Nr = 3,
                //    Omschrijving = "Q",
                //    Type = BelastingGeval.BelastingGevalTypeEnum.Veranderlijk,
                //    Gebruiksklasse = GebruiksklasseEnum.A_gemeenschappelijke_trappen,
                //    Opmerking = "puntlast"
                //},
            ];


        public List<BelastingCombinatieTypeEnum> CombinatiesTypes { get; set; } = [
            BelastingCombinatieTypeEnum.Fundamenteel_A,
            BelastingCombinatieTypeEnum.Fundamenteel_B,
            BelastingCombinatieTypeEnum.Karakteristiek,
            BelastingCombinatieTypeEnum.Frequent,
            BelastingCombinatieTypeEnum.QuasiBlijvend,
            //BelastingCombinatieTypeEnum.Karakteristiek

            ];






        public List<BelastingCombinatie> BelastingCombinaties { get; set; } = [];



        public void GenereerBelastingCombinaties(
            BelastingenContext context,
            List<BelastingGeval> gevallen,
            List<BelastingCombinatieTypeEnum> combinatieTypes,
            bool permanentOokGunstig = false,
            bool ookAlleenPermanenteBelasting = false


            )
        {
            // alleen voor situatie met 1 permanent (BG1), en veranderlijk q (BG2) en Q(BG3)
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
                            if (bg1 != null && 
                                ookAlleenPermanenteBelasting && 
                                type != BelastingCombinatieTypeEnum.Fundamenteel_B)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));

                                if (permanentOokGunstig)
                                {
                                    BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                    BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                                }
                            }


                            if (bg1 != null && bg2 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg2));

                                if (permanentOokGunstig)
                                {
                                    BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                    BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                                    BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg2));
                                }
                            }


                            if (bg1 != null && bg3 != null)
                            {
                                BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, false));
                                BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg3));

                                if (permanentOokGunstig)
                                {
                                    BelastingCombinaties.Add(new(BelastingCombinaties.Count + 1, type: type));
                                    BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg1, !false));
                                    BelastingCombinaties.Last().Items.Add(new(context, BelastingCombinaties.Last(), bg3));
                                }
                            }



                            break;




                        case BelastingCombinatieTypeEnum.Karakteristiek:
                        case BelastingCombinatieTypeEnum.Aardbeving:
                        case BelastingCombinatieTypeEnum.QuasiBlijvend:
                        case BelastingCombinatieTypeEnum.Frequent:
                        case BelastingCombinatieTypeEnum.Brand:

                            // zonder veranderlijk (niet karateristiek);
                            if (ookAlleenPermanenteBelasting)
                            {
                                BelastingCombinatie combi1 = new(BelastingCombinaties.Count + 1, type);
                                if (bg1 != null)
                                    combi1.Items.Add(new BelastingCombinatieItem(context, combi1, bg1));
                                BelastingCombinaties.Add(combi1);
                            }



                            // met veranderlijk q (BG2)
                            if (bg2 != null)
                            {
                                BelastingCombinatie combi12 = new(BelastingCombinaties.Count + 1, type);
                                if (bg1 != null)
                                    combi12.Items.Add(new BelastingCombinatieItem(context, combi12, bg1));
                                if (bg2 != null)
                                    combi12.Items.Add(new BelastingCombinatieItem(context, combi12, bg2));
                                BelastingCombinaties.Add(combi12);
                            }
                           

                            // met veranderlijk Q (BG3)
                            if (bg3 != null)
                            {
                                BelastingCombinatie combi13 = new(BelastingCombinaties.Count + 1, type);
                                if (bg1 != null)
                                    combi13.Items.Add(new BelastingCombinatieItem(context, combi13, bg1));
                                if (bg3 != null)
                                    combi13.Items.Add(new BelastingCombinatieItem(context, combi13, bg3));
                                BelastingCombinaties.Add(combi13);
                            }
                            



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



        protected override void Bereken()
        {
            return; // geen berekening
        }

        protected override bool Valideer()
        {
            return true; // geen validatie
        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return new MarkupString("BelastingenContext.ToHtml() not implemented yet.");
        //    //throw new NotImplementedException();
        //}
    }



}
