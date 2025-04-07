using CommonLibrary;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{


    public class OpleggingContext : BaseEurocodeContext
    {
        // 10 aanvullende regels prefab elementen
        // 10.9 bijzondere regels
        // 10.9.5 opleggingen
        // 10.9.5.2 opleggingen doorgaande elementen


        // Deze delegate wordt vanuit de parent ingesteld
        public Func<double>? BerekenOplegReactieRekenwaarde { get; set; }
        public Func<double>? BerekenLengteOndersteundeElement { get; set; }


        // Deze property blijft altijd in sync
        /// <summary>
        /// F~Ed~
        /// </summary>
        [TableColumn("F~Ed~", "rekenwaarde oplegreactie")]
        public double OplegReactieRekenwaarde => BerekenOplegReactieRekenwaarde?.Invoke() ?? 0;


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
        public double LengteOndersteundeElement => BerekenLengteOndersteundeElement?.Invoke() ?? 0;


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
        public bool AfzonderlijkeElementen
        {
            get
            {
                if (OpleggingElementType == null) return true;
                else
                {
                    if (OpleggingElementType.Value == OpleggingElementTypeEnum.AfzonderlijkElement) return true;
                    if (OpleggingElementType.Value == OpleggingElementTypeEnum.DoorgaandElement) return false;
                }
                return true;

            }

        }


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



        public override string ToString()
        {
            List<string> results = [];

            results.Add($"F~z,Ed~ = {OplegReactieRekenwaarde:0.0 kN}");
            results.Add($"a = a~1~+a~2~+a~3~+≤√(Δa~2~²+Δa~3~²) {(AfzonderlijkeElementen ? "+20" : "")} " +
                $"= {OplegLengteNetto:0.#}+{AfstandA2:0.#}+{AfstandA3:0.#}+√({AfstandDeltaA2:0.#}²+{AfstandDeltaA3:0.#}²)  {(AfzonderlijkeElementen ? "+20" : "")}" +
                $"= {OplegLengteNominaal:0 mm}");

            //results.Add( "a")





            return string.Join(", ", results);
        }

        //public MarkupString ToMarkupString()
        //{
        //    return MarkupHelper.ToMarkupString(this.ToString());
        //}

        public override bool IsAkkoord()
        {
            return true;
            //throw new NotImplementedException();
        }

        protected override void Bereken()
        {

            //throw new NotImplementedException();
        }

        protected override bool Valideer()
        {
            return true;
            //throw new NotImplementedException();
        }
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
