using Eurocode.BetonConstructies.Aanvullende_regels_prefab;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public static class OpleggingExtensions
    {
        public static void Update(this OpleggingContext context)
        {
            //context.SetOplegSterkteRekenwaarde();
            //context.SetAfstandA2();
            //context.SetAfstandDeltaA2();
            //context.SetAfstandA3();
            //context.SetOplegLengteNetto();
            context.SetOplegLengteNominaal();
        }





        public static double SetOplegLengteNominaal(this OpleggingContext context)
        {
            //throw new NotImplementedException();
            return context.GetOplegLengteNominmaal();
        }
        public static double GetOplegLengteNominmaal(this OpleggingContext context)
        {
            return context.OplegLengteNetto + context.AfstandA2 + context.AfstandA3 + Math.Sqrt(Math.Pow(context.AfstandDeltaA2, 2) + Math.Pow(context.AfstandDeltaA3, 2)) + context.AfstandDeltaElementType;
        }


        public static double SetOplegLengteNetto(this OpleggingContext context)
        {
            return context.GetOplegLengteNetto();
        }


        public static double GetOplegLengteNetto(this OpleggingContext context)
        {
            double a1 = context.OplegReactieRekenwaarde * 1000 / (context.OplegBreedteNetto * context.OplegSterkteRekenwaarde);
            double a1Min = context.GetMinimaleNettoOplegLengte();
            context.OplegLengteNettoAanwezig = Math.Max(a1Min, a1); // todo controleer hoe we hier mee om moeten gaan, mogelijk bied nieuwe uitgave norm duidelijkheid.
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


        public static double SetAfstandA2(this OpleggingContext context)
        {
            double returnVal = context.GetAfstandA2(out bool vellingkantenNoodzakelijk);
            context.VellingkantenNoodzakelijk = vellingkantenNoodzakelijk;
            return returnVal;
        }

        public static double GetAfstandA2(this OpleggingContext context, out bool vellingkantNoodzakelijk)
        {
            double val = 35;
            vellingkantNoodzakelijk = false;
            switch (context.OplegMateriaal, context.OplegType, context.RelatieveOplegspanning)
            {
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    val = 0.0;
                    break;
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    val = 0.0;
                    break;
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    val = 10.0;
                    break;
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    val = 5.0;
                    break;
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    val = 10.0;
                    break;
                case (OplegMateriaalEnum.STAAL, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    val = 15.0;
                    break;

                case (OplegMateriaalEnum.PREFAB_BETON or OplegMateriaalEnum.IHWG_BETON, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 5.0;
                    else
                        val = 10.0;
                    break;
                case (OplegMateriaalEnum.PREFAB_BETON, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 10.0;
                    else
                        val = 15.0;
                    break;
                case (OplegMateriaalEnum.PREFAB_BETON, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 15.0;
                    else
                        val = 25.0;
                    break;
                case (OplegMateriaalEnum.PREFAB_BETON, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 10.0;
                    else
                        val = 20.0;
                    break;
                case (OplegMateriaalEnum.PREFAB_BETON, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 15.0;
                    else
                        val = 25.0;
                    break;
                case (OplegMateriaalEnum.PREFAB_BETON, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    if (context.BetonsterkteklasseDragendeElement != null && (int)context.BetonsterkteklasseDragendeElement.Value >= 30)
                        val = 25.0;
                    else
                        val = 35.0;
                    break;



                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, <= 0.15):
                    val = 10.0;
                    break;
                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, > 0.15 and <= 0.40):
                    val = 15.0;
                    break;
                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.LIJNVORMIG, > 0.40):
                    val = 25.0;
                    vellingkantNoodzakelijk = true;// Vellingkant
                    break;
                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, <= 0.15):
                    val = 20.0;
                    break;
                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.15 and <= 0.40):
                    val = 25.0;
                    break;
                case (OplegMateriaalEnum.METSELWERK, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER, > 0.40):
                    val = 35.0;
                    vellingkantNoodzakelijk = true;// Vellingkant
                    break;



            }

            return val;
        }


        public static double SetAfstandDeltaA2(this OpleggingContext context)
        {
            return context.GetAfstandDeltaA2();
        }

        public static double GetAfstandDeltaA2(this OpleggingContext context)
        {
            double ondergrens = 10;
            double bovengrens = 40;
            double val = 0;
            switch (context.OplegMateriaal)
            {
                default:
                case OplegMateriaalEnum.STAAL or OplegMateriaalEnum.PREFAB_BETON:
                    ondergrens = 10; bovengrens = 30; val = context.LengteOndersteundeElement / 1200;
                    break;
                case OplegMateriaalEnum.IHWG_BETON or OplegMateriaalEnum.METSELWERK:
                    ondergrens = 15; bovengrens = 40; val = context.LengteOndersteundeElement / 1200 + 5;
                    break;
            }
            if (val < ondergrens) return ondergrens;
            if (val > bovengrens) return bovengrens;
            return val;
        }


        public static double SetAfstandA3(this OpleggingContext context)
        {
            return context.GetAfstandA3();
        }

        /// <summary>
        /// Tabel 10.4
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static double GetAfstandA3(this OpleggingContext context)
        {
            switch (context.DetailleringWapening, context.OplegType)
            {
                default: return 15;
                case (OpleggingContext.DetailleringWapeningEnum.DoorgaandeStavenBovenOndersteuning, OplegTypeEnum.LIJNVORMIG): return 0;
                case (OpleggingContext.DetailleringWapeningEnum.DoorgaandeStavenBovenOndersteuning, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER): return 0;

                case (OpleggingContext.DetailleringWapeningEnum.RechteStavenHorizontaleHaarspelden, OplegTypeEnum.LIJNVORMIG): return 5;
                case (OpleggingContext.DetailleringWapeningEnum.RechteStavenHorizontaleHaarspelden, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER): return Math.Max(15, context.EindDekking);

                case (OpleggingContext.DetailleringWapeningEnum.Voorspanelementen, OplegTypeEnum.LIJNVORMIG): return 5;
                case (OpleggingContext.DetailleringWapeningEnum.Voorspanelementen, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER): return 15;

                case (OpleggingContext.DetailleringWapeningEnum.VerticaleHaarspelden, OplegTypeEnum.LIJNVORMIG): return 15;
                case (OpleggingContext.DetailleringWapeningEnum.VerticaleHaarspelden, OplegTypeEnum.GECONCENTREERD or OplegTypeEnum.RIBBENVLOER): return context.EindDekking + context.BinnensteBuigstraal;
            }
        }


        public static double SetOplegSterkteRekenwaarde(this OpleggingContext context)
        {
            return context.GetOplegSterkteRekenwaarde();
        }



        public static double GetOplegSterkteRekenwaarde(this OpleggingContext context)
        {
            if (context.DrogeVerbinding)
            {
                return 0.4 * context.LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement;
            }
            else
            {
                double bovengrens = 0.85 * context.LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement;
                return Math.Min(context.RekenwaardeOplegmateriaal, bovengrens);
            }
        }


        public enum OplegMateriaalEnumerator
        {
            [Description("Staal")]
            STAAL,
            [Description("Prefabbeton")]
            PREFABBETON,
            [Description("Metselwerk")]
            METSELWERK,
            [Description("In het werk gestort beton")]
            IN_SITU_GESTORT_BETON
        }






    }

    public class UitkragingContext
    {
        internal double AsBen;

        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } = new(1000, 100);

        public BetonContext Beton { get; set; } = new();

        public double Lengte { get; set; } = 100;
        public double NuttigeHoogte { get; set; } = 100 - 20 - 4;
        public double Reactiekracht { get; set; } = 8;


        public TandMetHals Tand { get; set; } = new();
        public double Arm { get; internal set; }
        public double Moment { get; internal set; }

        public void Bereken()
        {
            Tand.BerekeningNeus(this);
        }

    }


    public class OpleggingContext
    {
        // 10 aanvullende regels prefab elementen
        // 10.9 bijzondere regels
        // 10.9.5 opleggingen
        // 10.9.5.2 opleggingen doorgaande elementen

        [TableColumn("detaillering")]
        public DetailleringWapeningEnum? DetailleringWapening { get; set; } = DetailleringWapeningEnum.VerticaleHaarspelden;
        public enum DetailleringWapeningEnum
        {
            [Description("Doorgaande staven boven de ondersteuning (ingeklemd of niet)")]
            DoorgaandeStavenBovenOndersteuning,
            [Description("Rechte staven, horizontale haarspelden, dichtbij het einde van het element")]
            RechteStavenHorizontaleHaarspelden,
            [Description("Voorspanelementen of rechte staven die aan het \r\neinde van het element zichtbaar zijn")]
            Voorspanelementen,
            [Description("Verticale haarspelden")]
            VerticaleHaarspelden
        }

        public OpleggingElementTypeEnum? OpleggingElementType { get; set; } = OpleggingElementTypeEnum.AfzonderlijkElement;
        public enum OpleggingElementTypeEnum
        {
            [Description("Doorgaand element (meerdere steunpunten)")]
            DoorgaandElement,
            [Description("Afzonderlijk element")]
            AfzonderlijkElement
        }


        public double EindDekking { get; set; } = 20;
        public double BinnensteBuigstraal { get; set; } = 50;
        public BetonsterkteklasseEnum? BetonsterkteklasseDragendeElement { get; set; } = BetonsterkteklasseEnum.C20_25;
        public BetonContext BetonOndersteundeElement { get; set; } = new();


        /// <summary>
        /// a of a~nom~
        /// </summary>
        [TableColumn("a~nom~", "nominale opleglengte")]
        public double OplegLengteNominaal { get { return this.SetOplegLengteNominaal(); } }

        /// <summary>
        /// a~1~
        /// </summary>
        [TableColumn("a~1~", "netto opleglengte")]
        public double OplegLengteNetto { get { return this.SetOplegLengteNetto(); } }

        /// <summary>
        /// a~aanw~
        /// </summary>
        [TableColumn("a~1,aanw~", "aanwezige netto opleglengte")]
        public double OplegLengteNettoAanwezig { get; set; } = 75;


        /// <summary>
        /// F~Ed~
        /// </summary>
        [TableColumn("F~Ed~", "rekenwaarde oplegreactie")]
        public double OplegReactieRekenwaarde { get; set; } = 50;


        /// <summary>
        /// b~1~
        /// </summary>
        [TableColumn("b~1~", "oplegbreedte netto")]
        public double OplegBreedteNetto { get; set; } = 1000;

        /// <summary>
        /// f~Rd~
        /// </summary>
        [TableColumn("f~Rd~", "rekenwaarde oplegsterkte")]
        public double OplegSterkteRekenwaarde { get { return this.SetOplegSterkteRekenwaarde(); } }


        [TableColumn("a~2~", "randafstand dragende element")]
        public double AfstandA2 { get { return this.SetAfstandA2(); } }

        [TableColumn("a~3~", "randafstand ondersteunde element")]

        public double AfstandA3 { get { return this.SetAfstandA3(); } }

        [TableColumn("|Delta|a~2~", "tolerantie afstand tussen dragende elementen")]

        public double AfstandDeltaA2 { get { return this.SetAfstandDeltaA2(); } }

        [TableColumn("|Delta|a~3~", "tolerantie lengteafwijkingen ondersteunde element")]

        public double AfstandDeltaA3
        {
            get { return LengteOndersteundeElement / 2500; }
        }


        [TableColumn("|Delta|~e~")]
        public double AfstandDeltaElementType
        {
            get
            {
                if (OpleggingElementType == OpleggingElementTypeEnum.AfzonderlijkElement) return 20;
                else return 0;
            }
        }

        [TableColumn("l~n~", "lengte ondersteunde element")]
        public double LengteOndersteundeElement { get; set; } = 2400;


        /// <summary>
        /// Indien metselwerk gekozen, dient Fcd opgegeven te worden.
        /// </summary>
        public double OpgaveDruksterkteMetselwerk { get; set; } = 5.00;

        /// <summary>
        /// f~cd~
        /// </summary>
        [TableColumn("f~cd~", "laagste rekenwaarde van de sterktes van het ondersteunde en het ondersteunende element")]
        public double LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement
        {
            get
            {
                if (OplegMateriaal == OplegMateriaalEnum.STAAL)
                {
                    return this.BetonOndersteundeElement.Fcd;
                }
                else if (OplegMateriaal == OplegMateriaalEnum.METSELWERK)
                {
                    return Math.Min(this.OpgaveDruksterkteMetselwerk, this.BetonOndersteundeElement.Fcd);
                }
                else
                {
                    double fcdDragendeElement = 13.33;
                    if (this.BetonsterkteklasseDragendeElement.HasValue)
                    {
                        fcdDragendeElement = (int)BetonsterkteklasseDragendeElement.Value / 1.5;
                    }

                    return Math.Min(this.BetonOndersteundeElement.Fcd, fcdDragendeElement);
                }

            }
        }




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


        [TableColumn("|sigma|~Ed~", "oplegspanning")]
        public double OplegSpanningRekenwaarde
        {
            get { return OplegReactieRekenwaarde * 1000 / (OplegBreedteNetto * OplegLengteNettoAanwezig); }
        }




        [TableColumn("|sigma|~Ed~ / f~cd~", "relatieve oplegspanning")]
        public double RelatieveOplegspanning
        {
            get { return OplegSpanningRekenwaarde / LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement; }
        }


        public OplegMateriaalEnum? OplegMateriaal { get; set; } = OplegMateriaalEnum.PREFAB_BETON;

        public OplegTypeEnum? OplegType { get; set; } = OplegTypeEnum.LIJNVORMIG;

    }

    public enum OplegMateriaalEnum
    {
        [Description("Staal")]
        STAAL,
        [Description("Prefabbeton")]
        PREFAB_BETON,
        [Description("Metselwerk")]
        METSELWERK,
        [Description("In het werk gestort beton")]
        IHWG_BETON

    }

    public enum OplegTypeEnum
    {
        [Description("Lijnvormig")]
        LIJNVORMIG,
        [Description("Ribbenvloer")]
        RIBBENVLOER,
        [Description("Geconcentreerd")]
        GECONCENTREERD
    }









}
