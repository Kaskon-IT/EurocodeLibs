using CommonLibrary;
using CommonLibrary.Extensions;
using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.ComponentModel;
using K = CommonLibrary.EurocodeKeys;

namespace Eurocode.BetonConstructies
{

    public class ScheurwijdteContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Scheurwijdte";

        public ScheurwijdteContext()
        {
            // default constructor, let op geen referentie naar dekking, beton en NB
            // maar dit kan later gedaan worden.
            BetonContext beton = new();
            BetonDekkingContext dekking = new();

            NationaleBijlageEnum nationaleBijlage = NationaleBijlageEnum.NL;

            //Beton = beton;

            SetDekking(dekking);
            SetBeton(beton);


            ScheurwijdteGrenswaarde = new(dekking, nationaleBijlage);
            ScheurwijdteMinimumWapening = new() { Beton = beton };

            NationaleBijlage = nationaleBijlage;



        }

        public ScheurwijdteContext(
            Snedekrachten snedekrachten,
            BetonContext beton,
            BetonDekkingContext dekking,
            ParametrischeProfielen.ParametrischProfielContext profiel,
            WapeningContext wapening,
            NationaleBijlageEnum nationaleBijlage)
        {
            Snedekrachten = snedekrachten;
            SetDekking(dekking);
            SetBeton(beton);

            //Beton = beton;
            //Dekking = dekking;
            Profiel = profiel;
            Wapening = wapening;
            ScheurwijdteGrenswaarde = new(dekking, nationaleBijlage);
            ScheurwijdteMinimumWapening = new() { Beton = beton };
            NationaleBijlage = nationaleBijlage;

            //ScheurwijdteExtensions.VerwerkScheurwijdte(this);
        }

        public void SetBeton(BetonContext beton)
        {
            this.Beton = beton;

            if (this.Dekking != null)
                this.Dekking.Beton = beton;


            if (this.ScheurwijdteMinimumWapening != null)
                this.ScheurwijdteMinimumWapening.Beton = beton;

        }
        public void SetDekking(BetonDekkingContext dekking)
        {
            this.Dekking = dekking;

            if (this.ScheurwijdteGrenswaarde != null)
                this.ScheurwijdteGrenswaarde.DekkingEnDuurzaamheid = dekking;


        }
        public void SetNationaleBijlage(NationaleBijlageEnum nationaleBijlage)
        {
            this.NationaleBijlage = nationaleBijlage;
            this.ScheurwijdteGrenswaarde.NationaleBijlage = nationaleBijlage;
        }

        // gebruik naam voor positie
        [TableColumn(label: "positie",

            Description = "gebied waar deze toets van toepassing is",
            Width = 2)]
        public string Naam { get; set; } = "";

        // input
        [TableColumn(
            Label = "moment BGT",
            Description = "Moment in de bruikbaarheidsgrenstoestand (BGT)",
            Symbol = "M<sub>E,freq</sub>",
            Unit = "kNm")]
        public double MomentFrequent { get { return Snedekrachten.My.Kar; } }

        [TableColumn(
            Label = "moment UGT",
            Symbol = "M<sub>Ed</sub>",
            Description = "moment in de uiterste grenstoestand (UGT)",
            Unit = "kNm")]
        public double MomentRekenwaarde { get { return Snedekrachten.My.Ed; } }

        [TableColumn(
            Label = "scheurmoment",
            Symbol = "M<sub>cr</sub>",
            Description = "scheurmoment",
            Unit = "kNm")]
        public double Mcr { get; set; }

        public NationaleBijlageEnum NationaleBijlage { get; set; } = NationaleBijlageEnum.EU;

        public Snedekrachten Snedekrachten { get; set; } = new();
        public BetonContext Beton { get; set; } = new();
        public BetonDekkingContext Dekking { get; set; } = new();
        public double DekkingOpLangsWapening { get; set; }
        public double AfstandVerdeelWapening { get; set; } = 0;

        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; }

        public double Breedte { get; set; } = 1000; // todo Profiel gebruiken
        public double Hoogte { get; set; } = 100;
        public double NuttigeHoogte { get; set; } = 80;

        //[TableColumn("A<sub>s,toe<sub> [mm²]",
        //    Key = K.AsToe,
        //     Weergave = WeergaveEnum.DraaiTabel)]
        //public string WapeningToegepastTekst { get; set; } = "8-150"; // todo Profiel +  wapening
        public double WapDiameterEquivalent { get; set; } = 8.0;

        public List<MilieuklasseEnum> Milieuklassen { get; set; } = [];

        //[TableColumn("Belastingduur", Weergave = WeergaveEnum.DraaiTabel)]
        public BelastingduurEnum Belastingduur { get; set; } = BelastingduurEnum.kortdurend;


        public enum BelastingduurEnum { kortdurend = 1, langdurend = 2 };

        // berekende zaken

        public double FctEff { get; set; }


        public double FactorK { get; set; } = 1.0;


        [TableColumn(Label = "factor",
            Symbol = "<i>k</i><sub>t</sub>",
            Article = "7.3.4 (2)",
            Description = "is een factor die afhangt van de belastingsduur: <ul>" +
            "<li><i>k</i><sub>t</sub> = 0,6 voor kortdurende belasting; " +
            "<li><i>k</i><sub>t<</sub> = 0,4 voor langdurende belasting. </ul>"


            )]
        public double FactorKt { get; internal set; } = 0.6;


        public double FactorKc { get; internal set; } = 0.4; // naar 7.3.2

        [TableColumn(
            Label = "factor",
            Symbol = "<i>k</i><sub>1</sub>",
            Article = "7.3.4 (3)",
            Description = "is een coëfficiënt die rekening houdt met de aanhechteigenschappen" +
            " van de hechtende wapening"
            )]
        public double MaximaleScheurAfstandFactorK1 { get; private set; } = 0.8; // naar 7.3.2

        //[TableColumn("Type", Weergave = WeergaveEnum.DraaiTabel)]
        public ScheurwijdteTypeEnum ScheurwijdteType { get; set; } = ScheurwijdteTypeEnum.Buiging; // naar 7.3.2


        /// <summary>
        /// is een factor die rekening houdt met de rekverdeling (in 7.11)
        /// </summary>
        [TableColumn(
            Label = "factor",
            Symbol = "<i>k</i><sub>2<sub>",
            Article = "7.3.4 (3)",
            Description = "is een coëfficiënt die rekening houdt met de rekverdeling"
            )]

        public double MaximaleScheurAfstandFactorK2
        {
            get
            {
                return ScheurwijdteType switch
                {
                    ScheurwijdteTypeEnum.Trek => 1.0,
                    _ => 0.5,
                };
            }
        }


        [TableColumn(
            Label = "factor",
            Symbol = "<i>k</i><sub>3</sub>",
            Article = "7.3.4 (3)",
            Description = "zie nationale bijlage",
            Key = K.ScheurwijdteK3
            )]
        public double MaximaleScheurAfstandFactorK3 { get; private set; } = 3.4;

        [TableColumn(
            Label = "factor",
            Symbol = "<i>k</i><sub>4</sub>",
            Article = "7.3.4 (3)",
            Description = "zie nationale bijlage"
            )]
        public double MaximaleScheurAfstandFactorK4 { get; private set; } = 0.425;



        public double Act { get; set; }
        public double Staalspanning { get; set; }

        [TableColumn(Label = "staalspanning",
            Symbol = "<i>σ</i><sub>s</sub>",
            Description = "is de spanning in de trekwapening, uitgaande van een" +
            " gescheurde doorsnede",
            Unit = "N/mm²",
            Article = "7.4.3 (2)"
            )]
        public double StaalspanningOptredend { get; set; }
        public double Rho { get; set; }
        public double HoogteBetonDrukZoneBGT { get; set; }
        public double AcEff { get; set; }
        public double HcEff { get; set; }
        public double VerhoudingWapeningBetonEffectief
        {
            get
            {
                return AsToe / AcEff;
            }
        }


        [TableColumn(
            Label = "maximale scheurafstand",
            Symbol = "s<sub>r,max</sub>",
            Description = "maximale scheurafstand",
            Article = "7.3.4 (1)",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left
            )]
        public double SrMax { get; internal set; }
        public Formula SrMaxFormula
        {
            get
            {
                switch (GebruiktArtikel)
                {
                    default:
                    case "7.14":
                        return new()
                        {
                            Name = "(7.14)",
                            StaticValue = @"s_{r,max} = 1.3 (h - x)",
                            DynamicValue = $"s_{{r,max}} = 1.3 \\cdot ({Hoogte} - {HoogteBetonDrukZoneBGT}) = {SrMax:0.##} mm"
                        };
                    case "7.11":
                        return new()
                        {
                            Name = "(7.11)",
                            StaticValue = @"k_3 \cdot c + k_1 \cdot k_2 \cdot k_4 \cdot Ø / \varphi_{p,eff} \leq MAX[(50 – 0,8 fck)Ø en 15Ø]",
                            DynamicValue = $"s_{{r,max}} = {MaximaleScheurAfstandFactorK3:0.##} \\cdot {DekkingOpLangsWapening:0.##} + {MaximaleScheurAfstandFactorK1:0.##} \\cdot {MaximaleScheurAfstandFactorK2:0.##} \\cdot {MaximaleScheurAfstandFactorK4:0.###} \\cdot {WapDiameterEquivalent:0.##} / {VerhoudingWapeningBetonEffectief:0.####} = {SrMax:0.##} mm"
                        };

                }
            }
        }



        //[TableColumn("Art.", Weergave = WeergaveEnum.DraaiTabel, HeaderTextPivot = "gebruikt artikel voor s<sub>r,max<sub>")]
        public string GebruiktArtikel { get; set; } = "";


        [TableColumn(
            Symbol = "ε<sub>sm</sub>-ε<sub>cm</sub>",
            Label = "rekverschil",
            Article = "7.3.4 (2)",
            Description = "mag zijn berekend uit de vergelijking (7.9)",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left
            )]
        public double EpsSmMinusEpsCm { get; set; }
        public Formula EpsSmMinusEpsCmFormula
        {
            get
            {
                return new()
                {
                    Name = "(7.9)",
                    StaticValue = @"\epsilon_{sm} - \epsilon_{cm} = \frac { \sigma_s - k_t \frac { f_{ct,eff} } { \rho_{p,eff} } \left( 1 + \alpha_e \cdot \rho_{p,eff}  \right)  } {E_s} \geq 0.6 \frac {\sigma_s} {E_s}",
                    DynamicValue = $@"\epsilon_{{sm}} - \epsilon_{{cm}} = {EpsSmMinusEpsCm:0.##} ‰"
                };
            }
        }


        [TableColumn(
            Article = "7.3.4 (1)",
            Label = "scheurwijdte",
            Symbol = "<i>w</i><sub>k</sub>",
            Unit = "mm",
            Description = "De scheurwijdte <i>w</i><sub>k</sub> mag zijn berekend met vergelijking (7.8):",
            StringFormat = "0.##",
            Key = K.ScheurwijdteBerekend,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left
            )]
        public double Wk { get; internal set; }
        public Formula WkFormula
        {
            get
            {
                return new()
                {
                    Name = "7.8",
                    StaticValue = @"w_k = s_{r,max} \cdot (\epsilon_{sm}-\epsilon_{cm})",
                    DynamicValue = $@"w_k = {SrMax.ToTeX()} \cdot {EpsSmMinusEpsCm.ToTeX()} = {Wk.ToTeX()} \;mm"
                };
            }
        }




        [TableColumn(
            Article = "7.3.1",
            Label = "factor",
            Symbol = "<i>k</i><sub>x</sub>",
            Description = " voor de bepaling van de duurzaamheid, mogen de waarden in tabel 7.1N zijn vermenigvuldigd met een factor kx.",
            Key = K.ScheurwijdteKx,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double ScheurwijdteGrenswaardeFactorKx
        {
            get
            {
                return ScheurwijdteGrenswaarde.FactorKx;
            }
        }


        public double Ec
        {
            get
            {
                //E;c is E;cm		NB Er wordt geen kruip meegenomen in deze berekening
                return Beton.Ecm;
            }
        }

        [TableColumn(
            Article = "7.3.1",
            Symbol = "<i>w</i><sub>max</sub>",
            Unit = "mm",
            Label = "grenswaarde scheurwijdte",
            Key = K.ScheurwijdteMax,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Left)]
        public double ScheurwijdteMax
        {
            get
            {
                return ScheurwijdteGrenswaarde.Wmax;
            }
        }


        public Scheurbeheersing.ScheurwijdteGrenswaarde ScheurwijdteGrenswaarde { get; set; }


        public Scheurbeheersing.ScheurwijdteMinimumWapening ScheurwijdteMinimumWapening { get; set; }




        //[TableColumn("U.C.", "Unity Check", StringFormat = "0.00")]
        public double UnityCheck
        {
            get
            {
                return Wk / ScheurwijdteMax;
            }
        }


        [TableColumn(Symbol = "<i>α</i><sub>e<sub>",
            Label = "verhouding",
            Article = "7.3.4 (2)",
            Description = "is de verhouding <i>E</i><sub>s</sub>/<i>E</i><sub>cm</sub>"
            )]
        public double ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton
        {
            get
            {
                // α;e			is de verhouding E;s/E;c
                return Beton.BetonStaal.ElasticiteitsModulus / (Ec); // Ec in N/mm² (gewijzigd 9-9-2025)
            }
        }



        [TableColumn(
            Article = "7.3.2 (2)",
            Label = "minimale wapening scheurbeheersing",
            Symbol = "<i>A</i><sub>s,min</sub>",
            Unit = "mm²"
            )]
        public double ScheurwijdteAsMin { get; set; }



        public WapeningContext Wapening { get; set; } = new() { Tekst = "8-100" };

        [TableColumn(
            Symbol = "<i>A</i><sub>s,toe</sub>",
            Label = "toegepast wapening",
            Unit = "mm²"
            )]
        public double AsToe
        {
            get { return Wapening.As; }
        }
        public double AsBen { get; set; }




        //[TableColumn("Element type", Weergave = WeergaveEnum.DraaiTabel)]
        public AanhechtingTypeEnum Aanhechting { get; set; } = AanhechtingTypeEnum.Standaard;




        public enum AanhechtingTypeEnum
        {
            [Description("Elementen met betonstaal en/of voorspanstaal ZONDER aanhechting")]
            Standaard = 1,
            [Description("Elementen met een combinatie van betonstaal en voorspanstaal MET aanhechting")]
            ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting = 4,
            [Description("Elementen met uitsluitend voorspanstaal MET aanhechting")]
            ElementenMetUitsluitendVoorspanstaalMetAanhechting = 8,
        }

        public enum ScheurwijdteTypeEnum
        {
            Buiging, Trek
        }



        protected override void Bereken()
        {
            this.VerwerkScheurwijdte();
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();

            // foutmeldingen
            if (Wk > ScheurwijdteGrenswaarde.Wmax)
            {
                AddMeldingWaarschuwing("overschrijding maximale scheurwijdte");
                return false;
            }


            if (AsToe < ScheurwijdteAsMin)
            {
                AddMeldingWaarschuwing("toegepaste wapening is kleiner dan minimale wapening scheurwijdte");
                return false;
            }

            return true;
        }




        public override string ToString()
        {
            string result = "";
            //ME,freq	sr,max	εsm-εcm	wk	kx	wmax
            result += $"M<sub>E,freq<sub> = {MomentFrequent:0.0 kNm}, ";
            result += $"s<sub>r,max<sub> = {SrMax:0.## mm}, ";
            result += $"ε<sub>sm<sub>-ε<sub>cm<sub> = {EpsSmMinusEpsCm:0.## ‰}, ";
            result += $"w<sub>k<sub> = {Wk:0.00 mm}, ";
            result += $"w<sub>max<sub> = {ScheurwijdteGrenswaarde.Wmax:0.00 mm}, ";
            result += $"k<sub>x<sub> = {ScheurwijdteGrenswaarde.FactorKx:0.##}, ";
            result += $"(UC = {(Wk / ScheurwijdteGrenswaarde.Wmax):0.00})";
            return result;
        }

        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    return this.ToHtmlTable(isDraaiTabel);
        //    throw new NotImplementedException();
        //}
    }



}
