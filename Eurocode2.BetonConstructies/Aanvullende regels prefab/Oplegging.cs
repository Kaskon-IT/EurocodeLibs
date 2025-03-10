using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{

    public static class OpleggingExtensions
    {
        public static void Update(this OpleggingContext context)
        {

        }

        public static void SetOplegLengteNominaal(this OpleggingContext context)
        {
            throw new NotImplementedException();
            //context.OplegLengteNominaal = context.GetOplegLengte
        }
        public static double GetOplegLengteNominmaal(this OpleggingContext context)
        {
            return context.OplegBreedteNetto + context.AfstandA2 + context.AfstandA3 + Math.Sqrt(Math.Pow(context.AfstandDeltaA2, 2) + Math.Pow(context.AfstandDeltaA3, 2));
        }


        public static void SetOplegLengteNetto(this OpleggingContext context)
        {
            context.OplegLengteNetto = context.GetOplegLengteNetto();
            return;
        }


        public static double GetOplegLengteNetto(this OpleggingContext context)
        {
            double a1 = context.OplegReactieRekenwaarde * 1000 / (context.OplegBreedteNetto * context.OplegSterkteRekenwaarde);
            double a1Min = context.GetMinimaleNettoOplegLengte();
            return Math.Max(a1Min, a1);
        }


        public static double GetMinimaleNettoOplegLengte(this OpleggingContext context)
        {
            // tabel 10.2
            switch (context.OplegType)
            {
                case OplegTypeEnum.LIJNVORMIG:
                    if (context.RelatieveOplegspanning < 0.15)
                    {
                        return 25.0;
                    }
                    else if (context.RelatieveOplegspanning < 0.40)
                    {
                        return 30.0;
                    }
                    else
                    {
                        return 40.0;
                    }
                case OplegTypeEnum.RIBBENVLOER:
                    if (context.RelatieveOplegspanning < 0.15)
                    {
                        return 55.0;
                    }
                    else if (context.RelatieveOplegspanning < 0.40)
                    {
                        return 70.0;
                    }
                    else
                    {
                        return 80.0;
                    }
                default:
                case OplegTypeEnum.GECONCENTREERD:
                    if (context.RelatieveOplegspanning < 0.15)
                    {
                        return 90.0;
                    }
                    else if (context.RelatieveOplegspanning < 0.40)
                    {
                        return 110.0;
                    }
                    else
                    {
                        return 140.0;
                    }

            }
        }


        public static void SetAfstandA2(this OpleggingContext context)
        {
            context.AfstandA2 = context.GetAfstandA2(out bool vellingkantenNoodzakelijk);
            context.VellingkantenNoodzakelijk = vellingkantenNoodzakelijk;
            return;
        }

        public static double GetAfstandA2(this OpleggingContext context, out bool vellingkantNoodzakelijk)
        {
            double returVal = 35;
            vellingkantNoodzakelijk = false;
            switch (context.OplegMateriaal, context.OplegType, context.RelatieveOplegspanning)
            {
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    returVal = 0.0;
                    break;
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    returVal = 0.0;
                    break;
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    returVal = 5.0;
                    break;
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    returVal = 15.0;
                    break;

                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    returVal = 5.0;
                    break;
                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    returVal = 15.0;
                    break;
                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    returVal = 15.0;
                    break;
                case (OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    returVal = 25.0;
                    break;

                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    returVal = 15.0;
                    break;
                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    returVal = 25.0;
                    break;
                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    returVal = 20.0;
                    break;
                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    returVal = 25.0;
                    break;
                case (OplegmateriaalEnum.ONGEWAPEND_BETON or OplegmateriaalEnum.GEWAPEND_BETON_TOT_C30_37, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    returVal = 35.0;
                    break;

                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    returVal = 10.0;
                    break;
                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    returVal = 15.0;
                    break;
                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    returVal = 25.0;
                    vellingkantNoodzakelijk = true;// Vellingkant
                    break;
                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    returVal = 20.0;
                    break;
                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    returVal = 25.0;
                    break;
                case (OplegmateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    returVal = 35.0;
                    vellingkantNoodzakelijk = true;// Vellingkant
                    break;



            }

            return returVal;
        }


        public static void SetOplegSterkteRekenwaarde(this OpleggingContext context)
        {
            context.OplegSterkteRekenwaarde = context.GetOplegSterkteRekenwaarde();
            return;
        }



        public static double GetOplegSterkteRekenwaarde(this OpleggingContext context)
        {
            if (context.DrogeVerbinding)
            {
                return 0.4 * context.LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement;
            }
            else
            {
                double bovengrens = 0.85;
                return Math.Max(context.RekenwaardeOplegmateriaal, bovengrens);
            }
        }



    }

    public class OpleggingContext
    {
        // 10 aanvullende regels prefab elementen
        // 10.9 bijzondere regels
        // 10.9.5 opleggingen
        // 10.9.5.2 opleggingen doorgaande elementen

        /// <summary>
        /// a of a~nom~
        /// </summary>
        [TableColumn("a~nom~", "nominale opleglengte")]
        public double OplegLengteNominaal { get; set; }

        /// <summary>
        /// a~1~
        /// </summary>
        [TableColumn("a~1~", "netto opleglengte")]
        public double OplegLengteNetto { get; set; }

        /// <summary>
        /// a~aanw~
        /// </summary>
        [TableColumn("a~aanw~", "aanwezige opleglengte")]
        public double OplegLengteAanwezig { get; set; }


        /// <summary>
        /// F~Ed~
        /// </summary>
        [TableColumn("F~Ed~", "rekenwaarde oplegreactie")]
        public double OplegReactieRekenwaarde { get; set; }


        /// <summary>
        /// b~1~
        /// </summary>
        [TableColumn("b~1~", "oplegbreedte netto")]
        public double OplegBreedteNetto { get; set; }

        /// <summary>
        /// f~Rd~
        /// </summary>
        [TableColumn("f~Rd~", "rekenwaarde oplegsterkte")]
        public double OplegSterkteRekenwaarde { get; set; }


        [TableColumn("a~2~", "randafstand dragende element")]
        public double AfstandA2 { get; set; }

        [TableColumn("a~3~", "randafstand ondersteunde element")]

        public double AfstandA3 { get; set; }

        [TableColumn("|Delta|a~2~", "tolerantie afstand tussen dragende elementen")]

        public double AfstandDeltaA2 { get; set; }

        [TableColumn("|Delta|a~3~", "tolerantie lengteafwijkingen ondersteunde element")]

        public double AfstandDeltaA3 { get; set; }

        [TableColumn("l~n~", "lengte ondersteunde element")]
        public double LengteOndersteundeElement { get; set; }


        /// <summary>
        /// f~cd~
        /// </summary>
        [TableColumn("f~cd~", "laagste rekenwaarde van de sterktes van het ondersteunde en het ondersteunende element")]
        public double LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement { get; set; } = 13.333;

        /// <summary>
        /// f~bed~
        /// </summary>
        [TableColumn("f~bed~", "rekenwaarde van de sterkte van het oplegmateriaal")]
        public double RekenwaardeOplegmateriaal { get; set; } = 5.0;




        public bool DrogeVerbinding { get; set; } = false;

        public bool VellingkantenNoodzakelijk { get; set; } = false;

        /// <summary>
        /// 10.9.5.3 Oplegging voor afzonderlijke elementen
        /// 10.9.5.3 (1) De nominale lengte moet 20 mm groter zijn dan voor doorgaande elementen
        /// </summary>
        public bool AfzonderlijkeElementen { get; set; } = true;


        public double OplegSpanningRekenwaarde
        {
            get { return OplegReactieRekenwaarde * 1000 / (OplegBreedteNetto * OplegLengteAanwezig); }
        }


        [TableColumn("|sigma|~Ed~ / f~cd~", "relatieve oplegspanning")]
        public double RelatieveOplegspanning
        {
            get { return OplegSpanningRekenwaarde / LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement; }
        }


        public OplegmateriaalEnum OplegMateriaal { get; set; } = OplegmateriaalEnum.GEWAPEND_BETON_VANAF_C30_37;

        public OplegTypeEnum OplegType { get; set; } = OplegTypeEnum.LIJNVORMIG;

    }

    public enum OplegmateriaalEnum
    {
        STAAL,
        GEWAPEND_BETON_VANAF_C30_37,
        GEWAPEND_BETON_TOT_C30_37,
        ONGEWAPEND_BETON,
        METSELWERK

    }

    public enum OplegTypeEnum
    {
        LIJNVORMIG,
        RIBBENVLOER,
        GECONCENTREERD
    }






}
