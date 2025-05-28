using CommonLibrary;
using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.ComponentModel;
using K = CommonLibrary.EurocodeKeys;

namespace Eurocode.BetonConstructies
{

    public class ScheurwijdteContext : BaseEurocodeContext
    {
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
        [TableColumn(headerText: "positie",
            Weergave = WeergaveEnum.StandaardTabel,
            HeaderTextPivot = "Naam",
            Width = 2)]
        public string Naam { get; set; } = "Schil";

        // input
        [TableColumn("M~E,freq~ [kNm]",
            headerTextPivot: "Moment (BGT) M~E,freq~",
            StringFormat = "0.#",
            Width = 2,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double MomentFrequent { get { return Snedekrachten.My.Kar; } }

        [TableColumn("M~Ed~", Weergave = WeergaveEnum.Geen)]
        public double MomentRekenwaarde { get { return Snedekrachten.My.Ed; } }

        [TableColumn("M~cr~", Weergave = WeergaveEnum.DraaiTabel, HeaderTextPivot = "Scheurmoment M~cr~", StringFormat = "0.0 kNm")]
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

        [TableColumn("A~s,toe~ [mm²]", Weergave = WeergaveEnum.DraaiTabel)]
        public string WapeningToegepastTekst { get; set; } = "8-150"; // todo Profiel +  wapening
        public double WapDiameterEquivalent { get; set; } = 8.0;

        public List<MilieuklasseEnum> Milieuklassen { get; set; } = [];

        //[TableColumn("Belastingduur", Weergave = WeergaveEnum.DraaiTabel)]
        public BelastingduurEnum Belastingduur { get; set; } = BelastingduurEnum.kortdurend;


        public enum BelastingduurEnum { kortdurend = 1, langdurend = 2 };

        // berekende zaken

        public double FctEff { get; set; }


        public double FactorK { get; set; } = 1.0;


        [TableColumn("factor k~t~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "0.##")]

        public double FactorKt { get; set; } = 0.6;

        //[TableColumn("factor k~c~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "0.##")]

        public double FactorKc { get; set; } = 0.4; // naar 7.3.2

        [TableColumn("k~1~", Weergave = WeergaveEnum.DraaiTabel)]
        public double MaximaleScheurAfstandFactorK1 { get; set; } = 0.8; // naar 7.3.2

        //[TableColumn("Type", Weergave = WeergaveEnum.DraaiTabel)]
        public ScheurwijdteTypeEnum ScheurwijdteType { get; set; } = ScheurwijdteTypeEnum.Buiging; // naar 7.3.2


        /// <summary>
        /// is een factor die rekening houdt met de rekverdeling (in 7.11)
        /// </summary>
        [TableColumn("k~2~", Weergave = WeergaveEnum.DraaiTabel)]

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


        [TableColumn("k~3~", Weergave = WeergaveEnum.DraaiTabel)]
        public double MaximaleScheurAfstandFactorK3 { get; set; } = 3.4;

        [TableColumn("k~4~", Weergave = WeergaveEnum.DraaiTabel)]
        public double MaximaleScheurAfstandFactorK4 { get; set; } = 0.425;



        public double Act { get; set; }
        public double Staalspanning { get; set; }

        [TableColumn("|sigma|~s~",
            Weergave = WeergaveEnum.DraaiTabel,
            HeaderTextPivot = "optredende spanning betonstaal |sigma|~s~",
            StringFormat = "0 N/mm²")]
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


        [TableColumn("s~r,max~", StringFormat = "0.##", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double SrMax { get; internal set; }

        //[TableColumn("Art.", Weergave = WeergaveEnum.DraaiTabel, HeaderTextPivot = "gebruikt artikel voor s~r,max~")]
        public string GebruiktArtikel { get; set; } = "";


        [TableColumn("|epsilon|~sm~-|epsilon|~cm~", StringFormat = "e2", Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
        public double EpsSmMinusEpsCm { get; set; }


        [TableColumn("w~k~ [mm]",
            headerTextPivot: "(7.8) berekende scheurwijdte w~k~ = s~r,max~ (|epsilon|~sm~-|epsilon|~cm~)",
            StringFormat = "0.## mm",
            Key = K.ScheurwijdteBerekend,
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center
            )]
        public double Wk { get; internal set; }

        [TableColumn("k~x~",
            headerTextPivot: "k~x~",
            StringFormat = "0.##",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
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

        [TableColumn("w~max~ [mm]",
            headerTextPivot: "grenswaarde scheurwijdte",
            StringFormat = "0.0",
            Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center)]
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


        [TableColumn("|alpha|~e~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "e2")]
        public double ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton
        {
            get
            {
                // α;e			is de verhouding E;s/E;c
                return Beton.BetonStaal.ElasticiteitsModulus / Ec;
            }
        }



        [TableColumn("A~s,min~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "0 mm²")]
        public double ScheurwijdteAsMin { get; set; }



        public WapeningContext Wapening { get; set; } = new() { Tekst = "8-100" };

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

        public override bool IsAkkoord()
        {
            return Valideer();
            //throw new NotImplementedException();
        }

        protected override void Bereken()
        {
            this.VerwerkScheurwijdte();
            //throw new NotImplementedException();
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
            //throw new NotImplementedException();
        }


        public override string ToString()
        {
            string result = "";
            //ME,freq	sr,max	εsm-εcm	wk	kx	wmax
            result += $"M~E,freq~ = {MomentFrequent:0.0 kNm}, ";
            result += $"s~r,max~ = {SrMax:0.## mm}, ";
            result += $"ε~sm~-ε~cm~ = {EpsSmMinusEpsCm:0.## ‰}, ";
            result += $"w~k~ = {Wk:0.00 mm}, ";
            result += $"w~max~ = {ScheurwijdteGrenswaarde.Wmax:0.00 mm}, ";
            result += $"k~x~ = {ScheurwijdteGrenswaarde.FactorKx:0.##}, ";
            result += $"(UC = {(Wk / ScheurwijdteGrenswaarde.Wmax):0.00})";
            return result;
        }
    }



}
