using Eurocode.Grondslagen;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{

    public class ScheurwijdteContext
    {
        public ScheurwijdteContext()
        {

        }

        public ScheurwijdteContext(double momBGT, double momUGT, BetonContext beton, BetonDekkingContext dekking, NationaleBijlageEnum nationaleBijlage)
        {
            MomentBGT = momBGT;
            MomentUGT = momUGT;
            Beton = beton;
            Dekking = dekking;
            ScheurwijdteGrenswaarde = new(dekking, nationaleBijlage);
            NationaleBijlage = nationaleBijlage;

            //ScheurwijdteExtensions.VerwerkScheurwijdte(this);
        }

        // input
        [TableColumn("M~E,freq~", headerTextPivot: "Moment (BGT) M~E,freq~", StringFormat = "0.# kNm")]
        public double MomentBGT { get; set; }

        [TableColumn("M~Ed~", Weergave = WeergaveEnum.Geen)]
        public double MomentUGT { get; set; }

        [TableColumn("M~cr~", Weergave = WeergaveEnum.DraaiTabel, HeaderTextPivot = "Scheurmoment M~cr~", StringFormat = "0.0 kNm")]
        public double Mcr { get; set; }

        public NationaleBijlageEnum NationaleBijlage { get; set; } = NationaleBijlageEnum.EU;


        public BetonContext Beton { get; set; } = new();
        public BetonDekkingContext Dekking { get; set; } = new();
        public double DekkingOpLangsWapening { get; set; }
        public double AfstandVerdeelWapening { get; set; } = 0;
        public double Breedte { get; set; } = 1000; // todo Profiel gebruiken
        public double Hoogte { get; set; } = 100;
        public double NuttigeHoogte { get; set; } = 80;

        [TableColumn("A~s,toe~", Weergave = WeergaveEnum.DraaiTabel)]
        public string WapString { get; set; } = "8-150"; // todo Profiel +  wapening
        public double WapDiameterEquivalent { get; set; } = 8.0;

        public List<MilieuklasseEnum> Milieuklassen { get; set; } = [];

        [TableColumn("Belastingduur", Weergave = WeergaveEnum.DraaiTabel)]
        public BelastingduurEnum Belastingduur { get; set; } = BelastingduurEnum.kortdurend;


        public enum BelastingduurEnum { kortdurend = 1, langdurend = 2 };

        // berekende zaken

        public double FctEff { get; set; }


        public double FactorK { get; set; } = 1.0;


        [TableColumn("factor k~t~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "0.##")]

        public double FactorKt { get; set; } = 0.6;

        [TableColumn("factor k~c~", Weergave = WeergaveEnum.DraaiTabel, StringFormat = "0.##")]

        public double FactorKc { get; set; } = 0.4;

        [TableColumn("k~1~", Weergave = WeergaveEnum.DraaiTabel)]
        public double FactorK1 { get; set; } = 0.8;

        [TableColumn("Type", Weergave = WeergaveEnum.DraaiTabel)]
        public ScheurwijdteTypeEnum ScheurwijdteType { get; set; } = ScheurwijdteTypeEnum.Buiging;

        [TableColumn("k~2~", Weergave = WeergaveEnum.DraaiTabel)]

        public double FactorK2
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
        public double FactorK3 { get; set; } = 3.4;

        [TableColumn("k~4~", Weergave = WeergaveEnum.DraaiTabel)]
        public double FactorK4 { get; set; } = 0.425;



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


        [TableColumn("s~r,max~", StringFormat = "0.##")]
        public double SrMax { get; internal set; }

        [TableColumn("Art.", Weergave = WeergaveEnum.DraaiTabel, HeaderTextPivot = "gebruikt artikel voor s~r,max~")]
        public string GebruiktArtikel { get; set; } = "";


        [TableColumn("|epsilon|~sm~-|epsilon|~cm~", StringFormat = "e3")]
        public double EpsSmMinusEpsCm { get; set; }


        [TableColumn("w~k~", headerTextPivot: "(7.8) berekende scheurwijdte w~k~ = s~r,max~ (|epsilon|~sm~-|epsilon|~cm~)", StringFormat = "0.## mm")]
        public double Wk { get; internal set; }

        [TableColumn("k~x~", headerTextPivot: "k~x~", StringFormat = "0.0")]
        public double FactorKx
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

        [TableColumn("w~max~", headerTextPivot: "grenswaarde scheurwijdte", StringFormat = "0.0 mm")]
        public double ScheurwijdteMax
        {
            get
            {
                return ScheurwijdteGrenswaarde.Wmax;
            }
        }


        public Scheurbeheersing.ScheurwijdteGrenswaarde ScheurwijdteGrenswaarde { get; set; }




        [TableColumn("U.C.", "Unity Check", StringFormat = "0.00")]
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
        public double AsMin { get; set; }
        public double AsToe { get; set; }
        public double AsBen { get; set; }



        [TableColumn("Element type", Weergave = WeergaveEnum.DraaiTabel)]
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
    }

    public static class ScheurwijdteExtensions
    {
        public static void VerwerkScheurwijdte(this ScheurwijdteContext sw)
        {

            // verplaats naar 7.3.1
            //sw.SetScheurwijdteMax(sw.Aanhechting);
            //sw.SetFactorKx();

            // verplaats naar 7.3.2
            sw.SetFactorKt(sw.Belastingduur);
            sw.FactorK = GetFactorK(sw);
            sw.SetFctEff();

            sw.Mcr = sw.SetMcr();
            sw.Staalspanning = sw.GetStaalspanning();
            sw.Act = sw.GetAct();
            sw.AsMin = sw.GetAsMin();
            sw.AsToe = Wapening.GetDsnOpp(sw.WapString);

            sw.StaalspanningOptredend = sw.GetStaalspanningOptredend();
            sw.Rho = sw.GetRho();
            sw.HoogteBetonDrukZoneBGT = sw.GetHoogteBetonDrukZoneBGT();
            sw.StaalspanningOptredend = sw.GetStaalspanningOptredendVerbeterd();


            sw.AcEff = sw.GetAcEff();
            sw.HcEff = sw.GetHcEff();


            // deel 7.3.4
            sw.EpsSmMinusEpsCm = sw.GetEpsSmMinusEpsCm();
            sw.DekkingOpLangsWapening = sw.Dekking.DekkingToe + sw.AfstandVerdeelWapening;

            sw.SrMax = sw.GetSrMax(150, formule: out string formule); // todo HOH validaties
            sw.GebruiktArtikel = formule;
            sw.Wk = sw.GetKarakteristiekeScheurwijdte();




        }

        public static double GetSrMax(this ScheurwijdteContext sw, double hohMaat, out string formule)
        {
            double maxHohMaat = 5 * (sw.DekkingOpLangsWapening + sw.WapDiameterEquivalent / 2);
            if (hohMaat > maxHohMaat)
            {
                formule = "7.14";
                return 1.3 * (sw.Hoogte - sw.HoogteBetonDrukZoneBGT);

            }
            else
            {
                formule = "7.11";
                // s;r,max = k3 × c + k1 × k2 × k4 × Øeq / ρ;p,eff              conform (7.11)
                double srMax = sw.FactorK3 * sw.DekkingOpLangsWapening + sw.FactorK1 * sw.FactorK2 * sw.FactorK4 * sw.WapDiameterEquivalent / sw.VerhoudingWapeningBetonEffectief;

                List<double> bovengrenzen = [
                     (50 - 0.8 * sw.Beton.Fck) * sw.WapDiameterEquivalent,
                     15 * sw.WapDiameterEquivalent
                    ];

                var bovengrens = bovengrenzen.Max();

                if (srMax <= bovengrens)
                    return srMax;
                else return bovengrens;

                //betonElement.ScheurwijdteSrMax = 3.4 * betonElement.ScheurwijdteDekkingLangswapening + 0.8 * betonElement.FactorK2 * 0.425 * betonElement.WapeningDiameterEquivalent / betonElement.gScheurwijdteWapeningsverhoudingEffectief;
                //betonElement.HulpScheurwijdteScheurafstandMaxBovengrens[0] =  // bovengrens A
                //betonElement.HulpScheurwijdteScheurafstandMaxBovengrens[1] = 15 * betonElement.WapeningDiameterEquivalent;                  // bovengrens B
                //betonElement.ScheurwijdteSrMax = (betonElement.ScheurwijdteSrMax > betonElement.HulpScheurwijdteScheurafstandMaxBovengrens.Max()) ? betonElement.HulpScheurwijdteScheurafstandMaxBovengrens.Max() : betonElement.ScheurwijdteSrMax; // Als s;r,max > bovengrens neem de bovengrens.

            }
        }

        public static double GetKarakteristiekeScheurwijdte(this ScheurwijdteContext sw)
        {
            return sw.SrMax * sw.EpsSmMinusEpsCm;
        }

        public static double GetEpsSmMinusEpsCm(this ScheurwijdteContext sw)
        {
            // ε;sm - ε;cm	
            double returnVal = (sw.StaalspanningOptredend - sw.FactorKt * (sw.FctEff / sw.VerhoudingWapeningBetonEffectief) *
                (1 + sw.ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton * sw.VerhoudingWapeningBetonEffectief)) / sw.Beton.BetonStaal.ElasticiteitsModulus;

            // (ε;sm - ε;cm) minimaal
            double minimum = 0.6 * (sw.StaalspanningOptredend / sw.Beton.BetonStaal.ElasticiteitsModulus);



            // als kleiner dan minimum moeten we de minimum-waarde aanhouden
            if (returnVal < minimum)
            {
                returnVal = minimum;
            }

            return returnVal;


        }

        public static double GetHoogteBetonDrukZoneBGT(this ScheurwijdteContext sw)
        {
            // x is de hoogte betondrukzone in frequente combinatie
            double i = sw.ScheurwijdteVerhoudingElasticiteitsmodulusStaalBeton * sw.Rho;
            return (-i + Math.Sqrt(Math.Pow((i), 2) + 2 * i)) * sw.NuttigeHoogte;
        }


        public static double GetHcEff(this ScheurwijdteContext sw)
        {
            // h;c,eff
            return sw.AcEff / sw.Breedte;
        }

        public static double GetAcEff(this ScheurwijdteContext sw)
        {
            // A;c,eff is kleinste van
            List<double> values =
            [
                sw.Breedte * 2.5 * (sw.Hoogte - sw.NuttigeHoogte),
                sw.Breedte * (sw.Hoogte - sw.NuttigeHoogte) / 3,
                sw.Breedte * sw.Hoogte / 2,
            ];

            return values.Min();
            //betonElement.HulpScheurwijdteOppervlakAcEffectiefHulp[0] = betonElement.Breedte * 2.5 * (betonElement.Hoogte - betonElement.NutHoogte);
            //betonElement.HulpScheurwijdteOppervlakAcEffectiefHulp[1] = betonElement.Breedte * (betonElement.Hoogte - betonElement.HoogteBetondrukzoneBGT) / 3;
            //betonElement.HulpScheurwijdteOppervlakAcEffectiefHulp[2] = betonElement.Breedte * betonElement.Hoogte / 2;
            //betonElement.ScheurwijdteOppervlakAcEffectief = betonElement.HulpScheurwijdteOppervlakAcEffectiefHulp.Min();                // is kleinste van hierboven

        }


        public static double GetRho(this ScheurwijdteContext sw)
        {
            //  ρ = A;s / (b d)

            return sw.AsToe / (sw.Breedte * sw.NuttigeHoogte);
        }


        public static double GetAct(this ScheurwijdteContext sw)
        {
            return 0.5 * sw.Breedte * sw.Hoogte; // todo voor ieder profiel
        }

        public static double GetAsMin(this ScheurwijdteContext sw)
        {
            return sw.FactorKc * sw.FactorK * sw.FctEff * sw.Act / sw.Staalspanning;
            // A;s,min × σ;s = k;c × k × f;ct,eff × A;ct
            // A;s,min = k;c × k × f;ct,eff × A;ct / σ;s 
        }

        public static double SetMcr(this ScheurwijdteContext sw)
        {
            //  nog geen profiel
            return (1.0 / 6.0) * sw.Breedte * sw.Hoogte * sw.Hoogte / 1000000.0 * sw.FctEff;
        }

        public static void SetFctEff(this ScheurwijdteContext sw)
        {
            sw.FctEff = sw.Beton.Fctm;
        }




        public static double GetStaalspanningOptredend(this ScheurwijdteContext sw)
        {
            // σ;s = (M;frequent / M;Ed) * (A;s,ben / A;s,toegepast) * f;yd
            return (sw.MomentBGT / sw.MomentUGT) * (sw.AsBen / sw.AsToe) * sw.Staalspanning;

        }

        public static double GetStaalspanningOptredendVerbeterd(this ScheurwijdteContext sw)
        {
            // σ_s =  M_frequent/(A_s  ( d-x/3)) 
            return sw.MomentBGT * 1e6 / (sw.AsToe * (sw.NuttigeHoogte - sw.HoogteBetonDrukZoneBGT / 3));
            //betonElement.ScheurwijdteSpanningTrekwapening = betonElement.Moment_Mf * 1000000 / (betonElement.gScheurwijdteWapeningToegepast * (betonElement.NutHoogte - betonElement.HoogteBetondrukzoneBGT / 3));

        }

        public static double GetStaalspanning(this ScheurwijdteContext sw)
        {
            return sw.Beton.BetonStaal.Fyk;
        }

        public static double GetFactorK(this ScheurwijdteContext sw)

        {
            // stap1: als h kleiner of gelijk is aan 300, dan geldt k = 1,0 
            // stap2: als h groter of gelijk is dan 800 dan geldt k = 0,65
            // stap3: hiertussen kunnen we lekker interpoleren. (rechtlijnige grafiek dus!!)
            // Methode met if, if else, else (ondergrens, bovengrens, interpolatie)
            if (sw.Breedte <= 300)
            {
                return 1.0;
            }
            else if (sw.Breedte >= 800)
            {
                return 0.65;
            }
            else
            {
                return (sw.Breedte - 300) / 500 * 0.35 + 0.65;
            }
        }   // factor k in berekening scheurwijdte

        public static void SetFactorKt(this ScheurwijdteContext sw, ScheurwijdteContext.BelastingduurEnum belastingduur)
        {
            switch (belastingduur)
            {
                default:
                case ScheurwijdteContext.BelastingduurEnum.kortdurend:
                    sw.FactorKt = 0.6;
                    break;
                case ScheurwijdteContext.BelastingduurEnum.langdurend:
                    sw.FactorKt = 0.4;
                    break;
            }
        }

        //public static void SetFactorKx(this ScheurwijdteContext sw)
        //{
        //    // factor k;x = c;toegepast / c;nom 
        //    sw.FactorKx = sw.Dekking.Betondekking / sw.Dekking.BetondekkingNominaal;

        //    if (sw.FactorKx > 2)
        //        sw.FactorKx = 2.0;

        //    sw.ScheurwijdteMax *= sw.FactorKx;



        //}


        //public static void SetScheurwijdteMax(this ScheurwijdteContext sw, ScheurwijdteContext.AanhechtingTypeEnum aanhechting)
        //{
        //    sw.ScheurwijdteMax = sw.Milieuklassen.GetMaxScheurwijdte(aanhechting);
        //}

        //public static double GetMaxScheurwijdte(this List<MilieuklasseEnum> milieuklassen, ScheurwijdteContext.AanhechtingTypeEnum aanhechting)
        //{
        //    double max = 0.40;
        //    foreach (var mk in milieuklassen)
        //    {
        //        if (mk.GetMaxScheurwijdte(aanhechting) < max)
        //        {
        //            max = mk.GetMaxScheurwijdte(aanhechting);
        //        }
        //    }
        //    return max;
        //}

        ///// <summary>
        ///// Tabel 7.1N Aanbevolen waarden van wmax (mm) 
        ///// </summary>
        ///// <param name="milieuklasse"></param>
        ///// <returns></returns>
        //public static double GetMaxScheurwijdte(this MilieuklasseEnum milieuklasse, ScheurwijdteContext.AanhechtingTypeEnum aanhechting)
        //{
        //    return milieuklasse switch
        //    {
        //        MilieuklasseEnum.XC2 or MilieuklasseEnum.XC3 or MilieuklasseEnum.XC4 => aanhechting switch
        //        {
        //            ScheurwijdteContext.AanhechtingTypeEnum.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting => 0.20,
        //            _ => 0.30,
        //        },
        //        MilieuklasseEnum.XD1 or MilieuklasseEnum.XD2 or MilieuklasseEnum.XD3 or MilieuklasseEnum.XS1 or MilieuklasseEnum.XS2 or MilieuklasseEnum.XS3 => aanhechting switch
        //        {
        //            ScheurwijdteContext.AanhechtingTypeEnum.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting => 0.10,
        //            _ => 0.20,
        //        },
        //        _ => aanhechting switch
        //        {
        //            ScheurwijdteContext.AanhechtingTypeEnum.ElementenMetEenCombinatieVanBetonstaalEnVoorspanstaalMetAanhechting => 0.30,
        //            _ => 0.40,
        //        },
        //    };
        //}



    }



}
