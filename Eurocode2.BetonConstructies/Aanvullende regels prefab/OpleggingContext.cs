using CommonLibrary;
using CommonLibrary.Extensions;
using CommonLibrary.Helpers;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Eurocode.BetonConstructies
{


    public class OpleggingContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Oplegging";
        // 10 aanvullende regels prefab elementen
        // 10.9 bijzondere regels
        // 10.9.5 opleggingen
        // 10.9.5.2 opleggingen doorgaande elementen


        public OpleggingContext()
        {

        }

        public OpleggingContext Clone(OpleggingContext context)
        {
            OpleggingContext clone = new OpleggingContext()
            {
                OplegType = context.OplegType,
                OpleggingElementType = context.OpleggingElementType,
                DetailleringWapening = context.DetailleringWapening,
                BetonsterkteklasseDragendeElement = context.BetonsterkteklasseDragendeElement,
                BetonOndersteundeElement = new BetonContext(context.BetonOndersteundeElement),
                OpgaveDruksterkteMetselwerk = context.OpgaveDruksterkteMetselwerk,
                DrogeVerbinding = context.DrogeVerbinding,
                VellingkantenNoodzakelijk = context.VellingkantenNoodzakelijk
            };
            return clone;
        }




        // Deze delegate wordt vanuit de parent ingesteld
        [JsonIgnore]
        public Func<double>? BerekenOplegReactieRekenwaarde { get; set; }
        [JsonIgnore]
        public Func<double>? BerekenLengteOndersteundeElement { get; set; }


        // Deze property blijft altijd in sync
        /// <summary>
        /// F~Ed~
        /// </summary>
        [TableColumn("F~Ed~", "rekenwaarde oplegreactie")]
        public double OplegReactieRekenwaarde => BerekenOplegReactieRekenwaarde?.Invoke() ?? 100;





        public double EindDekking { get; set; } = 20;
        public double BinnensteBuigstraal { get; set; } = 50;
        public double Voegbreedte { get; set; } = 10; // Voegbreedte in mm, standaard 10 mm
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
        public double OplegLengteNetto
        {
            get
            {
                BerekenEnValideer();
                return this.SetOplegLengteNetto();
            }
        }

        /// <summary>
        /// a~aanw~
        /// </summary>
        //[TableColumn("a~1,aanw~", "aanwezige netto opleglengte")]
        public double OplegLengteNettoAanwezig { get; set; } = 90;





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



        /// <summary>
        /// randafstand dragende element
        /// </summary>
        [TableColumn("a~2~", "randafstand dragende element")]
        public double AfstandA2 { get { return this.SetAfstandA2(); } }


        /// <summary>
        /// randafstand ondersteunde element
        /// </summary>
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
        public double LengteOndersteundeElement => BerekenLengteOndersteundeElement?.Invoke() ?? 8000;


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




        [TableColumn("droge verbinding?", Weergave = WeergaveEnum.DraaiTabel)]
        public bool DrogeVerbinding { get; set; } = false;


        [TableColumn("vellingkanten noodzakelijk?", Weergave = WeergaveEnum.DraaiTabel)]
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

        [TableColumn("oplegmat.", Weergave = WeergaveEnum.DraaiTabel)]
        public OplegMateriaalEnum? OplegMateriaal { get; set; } = OplegMateriaalEnum.PREFAB_BETON;

        [TableColumn("type", Weergave = WeergaveEnum.DraaiTabel)]
        public OplegTypeEnum? OplegType { get; set; } = OplegTypeEnum.LIJNVORMIG;



        [TableColumn("detaillering", Weergave = WeergaveEnum.DraaiTabel)]
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

        [TableColumn("elementtype", Weergave = WeergaveEnum.DraaiTabel)]

        public OpleggingElementTypeEnum? OpleggingElementType { get; set; } = OpleggingElementTypeEnum.AfzonderlijkElement;

        public enum OpleggingElementTypeEnum
        {
            [Description("Doorgaand element (meerdere steunpunten)")]
            DoorgaandElement,
            [Description("Afzonderlijk element")]
            AfzonderlijkElement
        }


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

        public string ToLongstring()
        {
            List<string> results = [];
            results.Add($"De oplegreactie F~z,Ed~ = {OplegReactieRekenwaarde:0.0 kN}.");
            results.Add($"Oplegtype: {OplegType.GetDisplayName().ToLower()} op {OplegMateriaal.GetDisplayName().ToLower()}.");
            results.Add($"De detaillering van de wapening is met {DetailleringWapening.GetDisplayName().ToLower()}.");
            results.Add($"Het element is een {OpleggingElementType.GetDisplayName().ToLower()}, met een lengte van {LengteOndersteundeElement:0 mm}.");
            results.Add($"De netto oplegbreedte b~1~ = {OplegBreedteNetto:0 mm}.");
            results.Add($"De netto opleglengte a~1~ = {OplegLengteNetto: 0 mm}.");
            results.Add($"De randafstand dragende element a~2~ = {AfstandA2: 0 mm}.");

            results.Add($"De randafstand ondersteunde element a~3~ = {AfstandA3: 0 mm}.");
            results.Add($"De tolerantie Δa~2~ = {AfstandDeltaA2: 0 mm}.");
            results.Add($"De tolerantie Δa~3~ = {AfstandDeltaA3: 0 mm}.");

            if (OplegType == OplegTypeEnum.GECONCENTREERD)
            {
                if (DetailleringWapening == DetailleringWapeningEnum.RechteStavenHorizontaleHaarspelden)
                {
                    results.Add($"Einddekking = {EindDekking: 0 mm.}");
                }
                if (DetailleringWapening == DetailleringWapeningEnum.VerticaleHaarspelden)
                {
                    results.Add($"Einddekking + binnenste buigstraal = {EindDekking: 0} + {BinnensteBuigstraal: 0} = {(EindDekking + BinnensteBuigstraal):0 mm}.");
                }
            }



            results.Add($"De rekenwaarde van de oplegsterkte f~Rd~ = {OplegSterkteRekenwaarde:0.00 N/mm²}.");
            results.Add($"De laagste rekenwaarde van de sterkte van ondersteunde en ondersteunende element f~cd~ = {LaagsteRekenwaardeVanOndersteundeEnHetOndersteunendeElement:0.00 N/mm²}.");
            results.Add($"De rekenwaarde van de oplegspanning σ~Ed~ = {OplegSpanningRekenwaarde:0.00 N/mm²}.");
            results.Add($"De relatieve oplegspanning σ~Ed~/f~cd~ = {RelatieveOplegspanning:P2}.");
            results.Add($"De nominale opleglengte a = a~1~+a~2~+a~3~+≤√(Δa~2~²+Δa~3~²) {(AfzonderlijkeElementen ? "+20" : "")} " +
                $"= {OplegLengteNetto:0.#}+{AfstandA2:0.#}+{AfstandA3:0.#}+√({AfstandDeltaA2:0.#}²+{AfstandDeltaA3:0.#}²)  {(AfzonderlijkeElementen ? "+20" : "")}" +
                $"= {OplegLengteNominaal:0 mm}.");

            results.Add($"Uitgaande van een voegbreedte van {Voegbreedte:0 mm} dient de tandlengte minimaal {(Voegbreedte + OplegLengteNominaal): 0 mm} te zijn.");

            return string.Join("<br />", results);
        }

        public MarkupString ToSamenvatting()
        {
            return MarkupHelper.ToMarkupString(this.ToLongstring());
        }

        //public MarkupString ToMarkupString()
        //{
        //    return MarkupHelper.ToMarkupString(this.ToString());
        //}

        public override bool IsAkkoord()
        {
            // de aanwezige opleglengte moet groter of gelijk zijn aan de nominale opleglengte
            if (OplegLengteNettoAanwezig < OplegLengteNominaal)
            {
                this.AddMeldingWaarschuwing("onvoldoende opleglengte aanwezig");
                return false;
            }

            // de oplegspanning mag niet groter zijn dan de oplegsterkte rekenwaarde
            if (OplegSpanningRekenwaarde > OplegSterkteRekenwaarde)
            {
                this.AddMeldingWaarschuwing("overschrijding oplegspanning");
                return false;
            }


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

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtmlTable(isDraaiTabel);

        //    //throw new NotImplementedException();
        //}
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
