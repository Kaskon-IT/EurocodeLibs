using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Eurocode.Belastingen
{
    public class BelastingCombinatieItem
    {

        [JsonInclude]
        public required BelastingenContext Context;

        /// <summary>
        /// De belastingcombinatie van het item
        /// </summary>
        [JsonInclude]
        public required BelastingCombinatie Combinatie { get; set; }

        /// <summary>
        /// Het belastinggeval van het item
        /// </summary>
        [JsonInclude]
        public required BelastingGeval Geval { get; set; }

        private readonly double _basisFactorPermanentFundamenteel = 1.35;
        private readonly double _basisFactorVeranderlijkFundamenteel = 1.50;

        /// <summary>
        /// Nullable waarde voor als er een momentaan-factor is toegepast
        /// </summary>
        public double? MomentFactor { get; private set; }
        public string MomentaanTekst { get; private set; } = "";

        /// <summary>
        /// De uiteindelijke factor van het belastinggeval
        /// </summary>
        public double FactorNetto
        {
            get { return GetFactoren(); }
        }

        /// <summary>
        /// De basisfactor voor permanente gevallen, inclusief de factoren KFI en/of Xi.
        /// </summary>
        public double FactorG { get; private set; }

        /// <summary>
        /// De basisfactor voor veranderlijke belastingen inclusief KFI maar exclusief MomentaanFactor
        /// </summary>
        public double FactorQ { get; private set; }


        public BelastingCombinatieItem() { } // ✅ Nodig voor deserialisatie

        [SetsRequiredMembers]
        public BelastingCombinatieItem(BelastingenContext context, BelastingCombinatie combinatie, BelastingGeval geval, bool permanentIsGunstig = false)
        {
            Context = context;
            Combinatie = combinatie;
            Geval = geval;
            PermanentIsGunstig = permanentIsGunstig;
            GetFactoren();
        }


        /// <summary>
        /// De NETTO factor wordt bepaald inclusief:
        /// - factor Xi (zie Eurocode 0)
        /// - factor K_FI (zie Eurocode 0)
        /// - gunstig permanent belasting?
        /// - momentaanfactoren
        /// </summary>
        private double GetFactoren()
        {
            double factorNetto = 1.0;

            if (Combinatie == null || Geval == null || Context == null || Context.Grondslagen == null)
            {
                return factorNetto;
            }

            switch (Geval.Type)
            {
                case BelastingGeval.BelastingGevalTypeEnum.Permanent:
                    if (PermanentIsGunstig)
                    {
                        factorNetto = 0.9;
                    }
                    else
                    {
                        switch (Combinatie.Type)
                        {
                            case BelastingCombinatieTypeEnum.Fundamenteel_A:
                                this.FactorG = _basisFactorPermanentFundamenteel * Context.Grondslagen.Kfi;
                                factorNetto = this.FactorG;
                                break;
                            case BelastingCombinatieTypeEnum.Fundamenteel_B:
                                this.FactorG = _basisFactorPermanentFundamenteel * Context.Grondslagen.Kfi * Context.Grondslagen.Xi;
                                factorNetto = this.FactorG;
                                break;
                            default:
                                factorNetto = 1.0;
                                break;
                        }
                    }
                    break;

                case BelastingGeval.BelastingGevalTypeEnum.Veranderlijk:
                    switch (Combinatie.Type)
                    {
                        // Fundamentele combinaties
                        case BelastingCombinatieTypeEnum.Fundamenteel_A:

                            this.MomentFactor = Geval.MomentaanFactoren.Mom0;
                            this.MomentaanTekst = "*|psi|~0~*";
                            this.FactorQ = _basisFactorVeranderlijkFundamenteel * Context.Grondslagen.Kfi;
                            factorNetto = this.FactorQ * this.MomentFactor.Value;
                            break;
                        case BelastingCombinatieTypeEnum.Fundamenteel_B:
                            this.FactorQ = factorNetto = _basisFactorVeranderlijkFundamenteel * Context.Grondslagen.Kfi;
                            break;

                        // Combinaties met mom1
                        case BelastingCombinatieTypeEnum.Frequent:
                            this.MomentFactor = Geval.MomentaanFactoren.Mom1;
                            this.MomentaanTekst = "*|psi|~1~*";
                            this.FactorQ = 1.00;
                            factorNetto *= Geval.MomentaanFactoren.Mom1;
                            break;

                        // Combinaties met mom2
                        case BelastingCombinatieTypeEnum.Brand:
                        case BelastingCombinatieTypeEnum.Aardbeving:
                        case BelastingCombinatieTypeEnum.QuasiBlijvend:
                            this.MomentFactor = Geval.MomentaanFactoren.Mom2;
                            this.MomentaanTekst = "*|psi|~2~*";
                            this.FactorQ = 1.00;
                            factorNetto *= Geval.MomentaanFactoren.Mom2;
                            break;

                        default:
                            factorNetto = 1.00;
                            break;
                    }
                    break;
            }

            //this.FactorNetto = factorNetto;
            return factorNetto;
        }
        /// <summary>
        /// Voor als de permanente belasting als gunstig beschouwd dient te worden.
        /// </summary>
        public bool PermanentIsGunstig { get; set; } = false;

    }
}
