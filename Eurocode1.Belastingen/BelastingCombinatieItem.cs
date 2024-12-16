namespace Eurocode.Belastingen
{
    public class BelastingCombinatieItem
    {


        public required BelastingenContext Context;


        /// <summary>
        /// De belastingcombinatie van het item
        /// </summary>
        public required BelastingCombinatie Combinatie { get; set; }
        /// <summary>
        /// Het belastinggeval van het item
        /// </summary>
        public required BelastingGeval Geval { get; set; }

        private double _basisFactorPermanentFundamenteel = 1.35;
        private double _basisFactorVeranderlijkFundamenteel = 1.50;

        public BelastingCombinatieItem() { }

        public BelastingCombinatieItem(BelastingCombinatie combinatie, BelastingGeval geval, bool permanentIsGunstig = false)
        {
            Combinatie = combinatie;
            Geval = geval;
            PermanentIsGunstig = permanentIsGunstig;
        }


        /// <summary>
        /// De NETTO factor wordt bepaald inclusief:
        /// - factor Xi (zie Eurocode 0)
        /// - factor K_FI (zie Eurocode 0)
        /// - gunstig permanent belasting?
        /// </summary>
        public double FactorNetto
        {
            get
            {
                double value = 1.0;
                switch (Geval.Type)
                {
                    case BelastingGeval.BelastingGevalTypeEnum.Permanent:
                        if (PermanentIsGunstig)
                        {
                            value = 0.9;
                        }
                        else
                        {
                            value = Combinatie.Type switch
                            {
                                // fundamenteel
                                BelastingCombinatieTypeEnum.Fundamenteel_A => _basisFactorPermanentFundamenteel * Context.Grondslagen.Kfi,
                                BelastingCombinatieTypeEnum.Fundamenteel_B => _basisFactorPermanentFundamenteel * Context.Grondslagen.Kfi * Context.Grondslagen.Xi,
                                // de rest 
                                _ => 1.0,
                            };
                        }
                        break;

                    case BelastingGeval.BelastingGevalTypeEnum.Veranderlijk:
                        switch (Combinatie.Type)
                        {
                            // Fundamentele combinaties
                            case BelastingCombinatieTypeEnum.Fundamenteel_A:
                                value = _basisFactorVeranderlijkFundamenteel * Context.Grondslagen.Kfi * Geval.MomentaanFactoren.Mom0;
                                break;
                            case BelastingCombinatieTypeEnum.Fundamenteel_B:
                                value = _basisFactorVeranderlijkFundamenteel * Context.Grondslagen.Kfi;
                                break;

                            // Combinaties met mom1
                            case BelastingCombinatieTypeEnum.Frequent:
                                value *= Geval.MomentaanFactoren.Mom1;
                                break;

                            // Combinaties met mom2
                            case BelastingCombinatieTypeEnum.Brand:
                            case BelastingCombinatieTypeEnum.Aardbeving:
                            case BelastingCombinatieTypeEnum.QuasiBlijvend:
                                value *= Geval.MomentaanFactoren.Mom2;
                                break;

                            default:
                                value = 1.00;
                                break;
                        }
                        break;
                }
                return value;
            }
        }



        /// <summary>
        /// Voor als de permanente belasting als gunstig beschouwd dient te worden.
        /// </summary>
        public bool PermanentIsGunstig { get; set; } = false;







    }



}
