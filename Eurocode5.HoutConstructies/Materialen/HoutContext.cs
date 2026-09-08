using CommonLibrary.Models;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurocode.HoutConstructies
{
    public class HoutContext : BaseMateriaal
    {
        public HoutContext() {}

        public HoutContext(Houtkwaliteit kwaliteit)
        {
            Kwaliteit = kwaliteit;
        }


        private Houtkwaliteit _kwaliteit = Houtkwaliteit.C24;
        /// <summary>
        /// De houtkwaliteit bepaalt de eigenschappen.
        /// C = vurenhout, 
        /// D = loofhout, 
        /// GL = gelamineerde ligger
        /// Default is C24
        /// </summary>
        public Houtkwaliteit Kwaliteit
        {
            get => _kwaliteit;
            set
            {
                if (_kwaliteit != value)
                {
                    StelEigenschappenInOpBasisVanKwaliteit(value);
                    SetProperty(ref _kwaliteit, value);
                }
            }
        }


       


        public override MateriaalType Type => MateriaalType.Hout;
        
        public override string UserFriendlyName => $"{Kwaliteit}";
        public override string Naam => UserFriendlyName;
        public override double SoortelijkGewicht { get; set; } = 350;
        public override double E { get; set; } = 11000;
        public override double G { get; set; } = 6900;


        public FabricageType Fabricage { get; set; } = FabricageType.Gezaagd;
        public KlimaatKlasse KlimaatKlasse { get; set; } = KlimaatKlasse.Klasse2;

        // EC5 tabel 2.3: γM = 1.30 voor gezaagd en loofhout (GL: 1.25, ingesteld via StelEigenschappenInOpBasisVanKwaliteit)
        public override double PartieleFactor { get; set; } = 1.3;

        /// <summary>
        /// fm,k (buigsterkte)
        /// </summary>
        public double Fmk { get; set; } = 24;
        /// <summary>
        /// fv,k (dwarskrachtsterkte)
        /// </summary>
        public double Fvk { get; set; } = 4;
        
      
        public double Fc0k { get; set; } = 21;
        public double Ft0k { get; set; } = 14;

        public double GetFmd(BelastingsduurKlasse duur)
        {
            // fmd = (kmod * fmk) / gammaM
            return (GetKmod(duur) * Fmk) / GammaM;
        }

        public double GetFvd(BelastingsduurKlasse duur)
        {
            return (GetKmod(duur) * Fvk) / GammaM;
        }
      


        public override string Eurocode => "EC5 - Houtconstructies";
        
        
        protected override void Bereken()
        {
            // Berekeningen specifiek voor houtmaterialen
        }


        protected override bool Valideer()
        {
            // Validatiespecifiek voor houtmaterialen
            if (Fmk <= 0 || SoortelijkGewicht <= 0)
            {
                return false;
            }

            return true;
        }


        private void StelEigenschappenInOpBasisVanKwaliteit(Houtkwaliteit kwaliteit)
        {
            // fabricage
            if (kwaliteit.ToString().StartsWith("GL"))
                Fabricage = FabricageType.Gelamineeerd;
            else 
                Fabricage = FabricageType.Gezaagd;


            switch (kwaliteit)
            {
                // --------------------------
                // Gezaagd naaldhout – C klassen
                // --------------------------
                case Houtkwaliteit.C18:
                    Fmk = 18;
                    Fvk = 3.2;
                    Ft0k = 11;
                    Fc0k = 18;
                    E = 9000;
                    G = 6000;
                    SoortelijkGewicht = 320;
                    PartieleFactor = 1.3;
                    break;

                case Houtkwaliteit.C24:
                    Fmk = 24;
                    Fvk = 4.0;
                    Ft0k = 14;
                    Fc0k = 21;
                    E = 11000;
                    G = 6900;
                    SoortelijkGewicht = 350;
                    PartieleFactor = 1.3;
                    break;

                case Houtkwaliteit.C27:
                    Fmk = 27;
                    Fvk = 4.0;
                    Ft0k = 16;
                    Fc0k = 22;
                    E = 11500;
                    G = 7200;
                    SoortelijkGewicht = 370;
                    PartieleFactor = 1.3;
                    break;

                case Houtkwaliteit.C30:
                    Fmk = 30;
                    Fvk = 4.0;
                    Ft0k = 18;
                    Fc0k = 23;
                    E = 12000;
                    G = 7500;
                    SoortelijkGewicht = 380;
                    PartieleFactor = 1.3;
                    break;

                case Houtkwaliteit.C40:
                    Fmk = 40;
                    Fvk = 5.0;
                    Ft0k = 24;
                    Fc0k = 26;
                    E = 14000;
                    G = 8500;
                    SoortelijkGewicht = 400;
                    PartieleFactor = 1.3;
                    break;

                case Houtkwaliteit.C50:
                    Fmk = 50;
                    Fvk = 5.0;
                    Ft0k = 30;
                    Fc0k = 29;
                    E = 16000;
                    G = 9500;
                    SoortelijkGewicht = 430;
                    PartieleFactor = 1.3;
                    break;


            // --------------------------
            // Loofhout – D klassen
            // --------------------------

            case Houtkwaliteit.D18:
                    Fmk = 18;
                    Ft0k = 14;
                    Fc0k = 21;
                    Fvk = 3.5;
                    
                    E = 9500;
                    G = 6500;
                    SoortelijkGewicht = 475;
                    PartieleFactor = 1.3;
                    break;

            case Houtkwaliteit.D24:
                    Fmk = 24;
                    Ft0k = 19;
                    Fc0k = 24;
                    Fvk = 4.0;

                    E = 10000;
                    G = 6800;
                    SoortelijkGewicht = 485;
                    PartieleFactor = 1.3;
                    break;

            case Houtkwaliteit.D27:
                    Fmk = 27;
                    Ft0k = 21;
                    Fc0k = 26;
                    Fvk = 4.0;
                    E = 10500;
                    G = 7000;
                    SoortelijkGewicht = 510;
                    PartieleFactor = 1.3;
                    break;

            case Houtkwaliteit.D30:
                    Fmk = 30;
                    Ft0k = 23;
                    Fc0k = 27;
                    Fvk = 4.0;
                    E = 11000;
                    G = 7200;
                    SoortelijkGewicht = 530;
                    PartieleFactor = 1.3;
                    break;

           


            case Houtkwaliteit.D40:
                    Fmk = 40;
                    Ft0k = 30;
                    Fc0k = 30;
                    Fvk = 5.0;
                    E = 13000;
                    G = 8000;
                    SoortelijkGewicht = 550;
                    PartieleFactor = 1.3;
                    break;

            

            case Houtkwaliteit.D50:
                    Fmk = 50;
                    Ft0k = 42;
                    Fc0k = 33;
                    Fvk = 6.0;
                    E = 14000;
                    G = 9000;
                    SoortelijkGewicht = 620;
                    PartieleFactor = 1.3;
                    break;

           

            case Houtkwaliteit.D60:
                    Fmk = 60;
                    Ft0k = 46;
                    Fc0k = 34;
                    Fvk = 6.2;

                    E = 17000;
                    G = 10000;
                    SoortelijkGewicht = 700;
                    PartieleFactor = 1.3;
                    break;

           

            case Houtkwaliteit.D70:
                    Fmk = 70;
                    Ft0k = 54;
                    Fc0k = 36;
                    Fvk = 7.0;

                    E = 20000;
                    G = 11000;
                    SoortelijkGewicht = 800;
                    PartieleFactor = 1.3;
                    break;

           

            case Houtkwaliteit.D80:
                    Fmk = 80;
                    Ft0k = 62;
                    Fc0k = 40;
                    Fvk = 7.0;
                    E = 24000;
                    G = 12000;
                    SoortelijkGewicht = 900;
                    PartieleFactor = 1.3;
                    break;




            // --------------------------
            // Gelamineerd hout – GLh homogeen
            // --------------------------
            case Houtkwaliteit.GL20h:
                    Fmk = 20;
                    Ft0k = 13;
                    Fc0k = 21;
                    Fvk = 3.5;
                    E = 8400;
                    G = 6000;
                    SoortelijkGewicht = 340;
                    PartieleFactor = 1.25;
                    break;
                                    

            case Houtkwaliteit.GL24h:
                    Fmk = 24;
                    Ft0k = 16;
                    Fc0k = 24;
                    Fvk = 3.5;
                    E = 11500;
                    G = 6900;
                    SoortelijkGewicht = 385;
                    PartieleFactor = 1.25;
                    break;

                case Houtkwaliteit.GL28h:
                    Fmk = 28;
                    Ft0k = 19;
                    Fc0k = 28;
                    Fvk = 3.5;
                    E = 12600;
                    G = 7500;
                    SoortelijkGewicht = 425;
                    PartieleFactor = 1.25;
                    break;

                case Houtkwaliteit.GL32h:
                    Fmk = 32;
                    Ft0k = 22;
                    Fc0k = 32;
                    Fvk = 4.0;
                    E = 14200;
                    G = 8000;
                    SoortelijkGewicht = 440;
                    PartieleFactor = 1.25;
                    break;

                case Houtkwaliteit.GL36h:
                    Fmk = 36;
                    Ft0k = 24;
                    Fc0k = 34;
                    Fvk = 4.0;
                    E = 14700;
                    G = 8500;
                    SoortelijkGewicht = 450;
                    PartieleFactor = 1.25;
                    break;

            // --------------------------
            // Gelamineerd hout – GLc gecombineerd
            // --------------------------

            case Houtkwaliteit.GL20c:
                    Fmk = 20;
                    Ft0k = 13;
                    Fc0k = 21;
                    Fvk = 3.5;
                    E = 10400;
                    G = 6500;
                    SoortelijkGewicht = 355;
                    PartieleFactor = 1.25;
                    break;

            case Houtkwaliteit.GL24c:
                    Fmk = 24;
                    Ft0k = 16;
                    Fc0k = 24;
                    Fvk = 3.5;
                    E = 11000;
                    G = 6900;
                    SoortelijkGewicht = 365;
                    PartieleFactor = 1.25;
                    break;

                case Houtkwaliteit.GL28c:
                    Fmk = 28;
                    Ft0k = 19;
                    Fc0k = 28;
                    Fvk = 3.5;
                    E = 12500;
                    G = 7500;
                    SoortelijkGewicht = 390;
                    PartieleFactor = 1.25;
                    break;

                case Houtkwaliteit.GL32c:
                    Fmk = 32;
                    Ft0k = 22;
                    Fc0k = 32;
                    Fvk = 4.0;
                    E = 13500;
                    G = 8000;
                    SoortelijkGewicht = 400;
                    PartieleFactor = 1.25;
                    break;

            case Houtkwaliteit.GL36c:
                    Fmk = 36;
                    Ft0k = 24;
                    Fc0k = 34;
                    Fvk = 4.0;
                    E = 14700;
                    G = 8500;
                    SoortelijkGewicht = 430;
                    PartieleFactor = 1.25;
                    break;

            default:
                    // fallback
                    Fmk = 24;
                    E = 11000;
                    G = 6900;
                    SoortelijkGewicht = 350;
                    PartieleFactor = 1.3;
                    break;
            }
        }

        public double GetKmod(BelastingsduurKlasse duur)
        {
            return (duur, Fabricage, KlimaatKlasse) switch
            {
                (BelastingsduurKlasse.Blijvend, FabricageType.Gezaagd, KlimaatKlasse.Klasse1) => 0.60,
                (BelastingsduurKlasse.Blijvend, FabricageType.Gezaagd, KlimaatKlasse.Klasse2) => 0.60,
                (BelastingsduurKlasse.Blijvend, FabricageType.Gezaagd, KlimaatKlasse.Klasse3) => 0.50,
                (BelastingsduurKlasse.Lang, FabricageType.Gezaagd, KlimaatKlasse.Klasse1) => 0.70,
                (BelastingsduurKlasse.Lang, FabricageType.Gezaagd, KlimaatKlasse.Klasse2) => 0.70,
                (BelastingsduurKlasse.Lang, FabricageType.Gezaagd, KlimaatKlasse.Klasse3) => 0.55,
                (BelastingsduurKlasse.Middellang, FabricageType.Gezaagd, KlimaatKlasse.Klasse1) => 0.80,
                (BelastingsduurKlasse.Middellang, FabricageType.Gezaagd, KlimaatKlasse.Klasse2) => 0.80,
                (BelastingsduurKlasse.Middellang, FabricageType.Gezaagd, KlimaatKlasse.Klasse3) => 0.65,
                (BelastingsduurKlasse.Kort, FabricageType.Gezaagd, KlimaatKlasse.Klasse1) => 0.90,
                (BelastingsduurKlasse.Kort, FabricageType.Gezaagd, KlimaatKlasse.Klasse2) => 0.90,
                (BelastingsduurKlasse.Kort, FabricageType.Gezaagd, KlimaatKlasse.Klasse3) => 0.70,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gezaagd, KlimaatKlasse.Klasse1) => 1.10,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gezaagd, KlimaatKlasse.Klasse2) => 1.10,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gezaagd, KlimaatKlasse.Klasse3) => 0.90,

                (BelastingsduurKlasse.Blijvend, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse1) => 0.60,
                (BelastingsduurKlasse.Blijvend, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse2) => 0.60,
                (BelastingsduurKlasse.Blijvend, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse3) => 0.50,
                (BelastingsduurKlasse.Lang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse1) => 0.70,
                (BelastingsduurKlasse.Lang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse2) => 0.70,
                (BelastingsduurKlasse.Lang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse3) => 0.55,
                (BelastingsduurKlasse.Middellang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse1) => 0.80,
                (BelastingsduurKlasse.Middellang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse2) => 0.80,
                (BelastingsduurKlasse.Middellang, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse3) => 0.65,
                (BelastingsduurKlasse.Kort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse1) => 0.90,
                (BelastingsduurKlasse.Kort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse2) => 0.90,
                (BelastingsduurKlasse.Kort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse3) => 0.70,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse1) => 1.10,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse2) => 1.10,
                (BelastingsduurKlasse.ZeerKort, FabricageType.Gelamineeerd, KlimaatKlasse.Klasse3) => 0.90,
                _ => 1,
            };
        }

    }

    public enum FabricageType
    {
        Gezaagd,
        Gelamineeerd,
    }

    public enum KlimaatKlasse
    {
        [Description("Klasse I (binnen/droog)")] Klasse1 = 1, 
        [Description("Klasse II (binnen/vochtig)")] Klasse2 = 2, 
        [Description("Klasse III (buiten)")] Klasse3 = 3  
    }

    public enum BelastingsduurKlasse
    {
        Blijvend,
        Lang,
        Middellang,
        Kort,
        ZeerKort
    }

    public enum Houtkwaliteit
    {
        C18, C24, C27, C30, C40, C50,
        D18, D24, D27, D30, D40, D50, D60, D70, D80,
        GL20h, GL24h, GL28h, GL32h, GL36h,
        GL20c, GL24c, GL28c, GL32c, GL36c
    }

}
