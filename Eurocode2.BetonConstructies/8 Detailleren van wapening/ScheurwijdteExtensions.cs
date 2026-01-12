namespace Eurocode.BetonConstructies
{
    public static class ScheurwijdteExtensions
    {
        public static void VerwerkScheurwijdte(this ScheurwijdteContext sw)
        {

            // verplaats naar 7.3.1
            //sw.SetScheurwijdteMax(sw.Aanhechting);
            //sw.SetFactorKx();
            sw.ScheurwijdteGrenswaarde.Initialiseer();
            sw.ScheurwijdteMinimumWapening.Beton = sw.Beton;



            sw.SetFactorKt(sw.Belastingduur);


            sw.FactorK = GetFactorK(sw);
            sw.SetFctEff();

            sw.Mcr = sw.SetMcr();
            sw.Staalspanning = sw.GetStaalspanning();


            sw.Act = sw.GetAct();
            sw.SetHoogte();
            sw.SetBreedte();
            sw.SetNuttigeHoogte();
            //sw.SetDekking();

            sw.ScheurwijdteAsMin = sw.GetAsMin();

            //sw.AsToe = WapeningHelper.GetDsnOpp(sw.WapeningToegepastTekst);

            //sw.StaalspanningOptredend = sw.GetStaalspanningOptredend();
            sw.Rho = sw.GetRho();
            sw.HoogteBetonDrukZoneBGT = sw.GetHoogteBetonDrukZoneBGT();
            sw.StaalspanningOptredend = sw.GetStaalspanningOptredendVerbeterd();


            sw.AcEff = sw.GetAcEff();
            sw.HcEff = sw.GetHcEff();


            // deel 7.3.4
            sw.EpsSmMinusEpsCm = sw.GetEpsSmMinusEpsCm();
            sw.DekkingOpLangsWapening = sw.Dekking.DekkingToe + sw.AfstandVerdeelWapening;

            var hohMaat = 150.0;
            if (sw.Wapening != null)
            {
                hohMaat = sw.Wapening.HohMaat;
            }

            sw.SrMax = sw.GetSrMax(hohMaat, formule: out string formule);
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
                double srMax = sw.MaximaleScheurAfstandFactorK3 * sw.DekkingOpLangsWapening + sw.MaximaleScheurAfstandFactorK1 * sw.MaximaleScheurAfstandFactorK2 * sw.MaximaleScheurAfstandFactorK4 * sw.WapDiameterEquivalent / sw.VerhoudingWapeningBetonEffectief;

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


        public static void SetHoogte(this ScheurwijdteContext sw)
        {
            if (sw.Profiel != null) sw.Hoogte = sw.Profiel.Hoogte;
        }
        public static void SetBreedte(this ScheurwijdteContext sw)
        {
            if (sw.Profiel != null) sw.Breedte = sw.Profiel.BreedteDwarskracht;
        }
        public static void SetNuttigeHoogte(this ScheurwijdteContext sw)
        {
            if (sw.Wapening != null)
            {
                sw.NuttigeHoogte = sw.Hoogte - sw.Wapening.ZRef;
            }
        }


        public static double GetAct(this ScheurwijdteContext sw)
        {
            if (sw.Profiel != null)
            {
                return sw.Profiel.Area;
            }
            else
                return 0.5 * sw.Breedte * sw.Hoogte;
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




        //public static double GetStaalspanningOptredend(this ScheurwijdteContext sw)
        //{
        //    // σ;s = (M;frequent / M;Ed) * (A;s,ben / A;s,toegepast) * f;yd
        //    return (sw.MomentFrequent / sw.MomentRekenwaarde) * (sw.AsBen / sw.AsToe) * sw.Staalspanning;

        //}

        public static double GetStaalspanningOptredendVerbeterd(this ScheurwijdteContext sw)
        {
            // σ_s =  M_frequent/(A_s  ( d-x/3)) 
            return Math.Abs(sw.MomentFrequent) * 1e6 / (sw.AsToe * (sw.NuttigeHoogte - sw.HoogteBetonDrukZoneBGT / 3));
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
