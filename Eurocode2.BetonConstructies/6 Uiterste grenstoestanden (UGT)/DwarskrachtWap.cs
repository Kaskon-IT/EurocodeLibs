namespace Eurocode.BetonConstructies
{
    //public class DwarskrachtWap
    //{
    //    // 6.2 Dwarskracht
    //    // Berekeningen dwarskracht conform NEN-EN-1992-1-1
    //    // Dit hoofdstuk bestrijkt vergelijkingen (6.1) t/m (6.25)


    //    #region Berekeningen aan te roepen vanuit interface


    //    public static DwarskrachtWapContext GetDwarskrachtWapContext(BetonContext beton, double theta, double b, double d, double z, double dwarskracht,
    //        double aantalSnede, double diameter, List<double> hohAfstanden)
    //    {
    //        DwarskrachtWapContext context = new DwarskrachtWapContext() { Theta = theta, Breedte = b, NutHoogte = d, Z = z, Ved = dwarskracht };
    //        context.LijstBeugelWap = new List<BeugelWap>();
    //        foreach (double hohAfstand in hohAfstanden)
    //        {
    //            context.LijstBeugelWap.Add(new BeugelWap() { AantalSnede = aantalSnede, Diameter = diameter, Hoh = hohAfstand });
    //        }


    //        context.Fywk = beton.BetonStaal.Fyk;      // noodzakelijk voor (6.10)
    //        context.Fywd = beton.BetonStaal.Fywd;
    //        context.Fck = beton.Fck;

    //        context.FactorK = GetFactorK(context.NutHoogte); // 19Feb2018
    //        context.Rho1 = GetRho1(context.AsLangs, context.Breedte, context.NutHoogte);
    //        context.RhoMin = GetRhoMin(beton.Fck, beton.BetonStaal.Fyk);
    //        context.SterkteReductieV = GetSterkteReductieV(beton.Fck);

    //        context.SchuifspanningWeerstandZonderDwarskrachtWapeningMin = GetVergelijkingZesPuntDrieN(context.FactorK, beton.Fck);
    //        context.Crdc = GetCrdC(beton.GammaC);
    //        context.SchuifspanningWeerstandZonderDwarskrachtWapening = GetSchuifSpanningWeerstandZonderDwarskrachtWapening(context.SchuifspanningWeerstandZonderDwarskrachtWapeningMin, context.Crdc, context.FactorK, context.Rho1, beton.Fcd);


    //        //A;sw,min minimaal benodigde wapening = Rho;w,min * b * 1000mm conform art. 9.2.2(5)
    //        context.AswMin = context.RhoMin / 100 * context.Breedte * 1000;
    //        // Asw = 1000 * Ved / ( z * Fywd * cotTheta ) , afgeleid uit art. 6.2.3 (3) (6.8)
    //        context.AswBerekend = 1000000 * context.Ved / (context.Z * beton.BetonStaal.Fywd * context.CotTheta);
    //        // controle of AswMin van toepassing is
    //        context.AswBenPerMeter = Math.Max(context.AswMin, context.AswBerekend);

    //        // zorg dat eerst de benodigde wapening bekend is voordat we VrdMax berekenen
    //        context.VrdMax = GetVrdMax(context.AlphaCw, context.Breedte, context.Z, context.SterkteReductieV1, beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha);

    //        // s;l,max
    //        // standaard s;l,max = 0,75×d;
    //        context.BeugelAfstandMaxLangs = 0.75 * context.NutHoogte;
    //        // maar s;l,max mag niet groter dan 300mm
    //        context.BeugelAfstandMaxLangs = context.BeugelAfstandMaxLangs > 300 ? 300 : context.BeugelAfstandMaxLangs;  // maar niet groter dan 300mm

    //        // indien geen dwarskrachtwapening nodig dan geldt s;l,max = 300;
    //        if (context.SchuifspanningD <= context.SchuifspanningWeerstandZonderDwarskrachtWapening) { context.BeugelAfstandMaxLangs = 300; }


    //        // zoek de beugelwapening (indien aanwezig & Ved opgave)
    //        if (context.LijstBeugelWap != null)
    //        {
    //            // indien de VRdMax groter is dan VEd kunnen we nog met een grotere v1 rekenen indien de staalspanning < 80%fyk
    //            double factor = 1;

    //            if (context.Ved > context.VrdMax)
    //            {
    //                factor = context.Ved / context.VrdMax;
    //                if (factor > 1.25) factor = 1.25;   // meer dan 125% heeft geen zin.
    //            }

    //            BeugelWap beugelWap = BerekeningenBeton.ZoekBeugelWap(context.AswBenPerMeter * factor, context.LijstBeugelWap);
    //            if (beugelWap != null)
    //            {
    //                context.BeugelDiameter = beugelWap.Diameter;
    //                context.BeugelHartOpHartAfstand = beugelWap.Hoh;
    //                context.BeugelSnedeAantal = beugelWap.AantalSnede;
    //                context.AswToegepast = beugelWap.AswToegepast;
    //            }

    //            // Beschouw opnieuw de VRdMax
    //            context.VrdMax = GetVrdMax(context.AlphaCw, context.Breedte, context.Z, context.SterkteReductieV1, beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha);
    //            // en controleer of VEd (nog steeds) niet groter is dan VRdMax 
    //            if (context.Ved > context.VrdMax)
    //            {
    //                // foutmelding!!!!!!!! verhoog de drukdiagonaal!
    //                // maak melding
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtOverschrijdingVRdMax);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }

    //            if (context.BeugelHartOpHartAfstand > context.BeugelAfstandMaxLangs)
    //            {
    //                // Kleinere beugelafstand kiezen?
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtBeugelAfstandTeGrootHohMaat);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }

    //            if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
    //            {
    //                // Meer beugelsneden toepassen?
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtBeugelAfstandDwarsTeGroot);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }
    //        }






    //        // s;t,max  art. 9.2.2 (8) NB
    //        bool testBeugelsnedeAfstand;                  // true or false om te kijken of we de maximale s;t,max kunnen opschroeven
    //        testBeugelsnedeAfstand = context.Ved <= 0.5 * context.VrdMax ? true : false;     // als true, dan s;t,max = 500
    //        if (testBeugelsnedeAfstand)
    //        {
    //            context.BeugelAfstandMaxDwars = 500;                                                         // als true, dan s;t,max = 500
    //        }
    //        else
    //        {
    //            context.BeugelAfstandMaxDwars = 0.75 * context.NutHoogte;                                           // als fase, dan 0.75 d
    //            context.BeugelAfstandMaxDwars = context.BeugelAfstandMaxDwars > 500 ? 500 : context.BeugelAfstandMaxDwars; // maar niet groter dan 500 met de ?operator
    //        }

    //        // s;t,applied (bij gelijkmatige verdeling)

    //        do
    //        {
    //            context.BeugelAfstandDwarsToegepast = (context.Breedte - context.DekkingZijkantToegepast * 2 - context.BeugelDiameter) / (context.BeugelSnedeAantal - 1);

    //            if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
    //            {
    //                context.BeugelSnedeAantal += 2; // ToDO keuze of 2/4/6/8 of 2/3/4/5/
    //                // TODO melding maken, beugelsnede verhoogd ivm maximale beugelafstand in de dwarsrichting!
    //                // Daarna opnieuw Vrd bereken!!!
    //            }
    //        } while ((context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars));


    //        // V_Rd,s volgens art. 6.2.3(3) formule (6.8)
    //        double reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
    //        if (context.SpanningWapeningKleinerDan80ProcentKarakteristiekeVloeigrens)
    //        {
    //            //OPMERKING Indien vergelijking (6.10) is gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot
    //            //0,8 fywk.
    //            reductiefactorIndienVergelijkingZesPuntTienGebruikt = (beton.BetonStaal.Fyk * 0.8) / (beton.BetonStaal.Fyk / beton.BetonStaal.GammaS);
    //            context.VrdS = 0.001 * (context.AswToegepast / 1000) * context.Z * beton.BetonStaal.Fyk * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;
    //        }
    //        else
    //        {
    //            reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
    //            context.VrdS = 0.001 * (context.AswToegepast / 1000) * context.Z * beton.BetonStaal.Fywd * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;
    //        }



    //        // V_Rd is de kleinste van V_Rd,s en V_Rd,max
    //        context.Vrd = Math.Min(context.VrdS, context.VrdMax);

    //        if (context.AswBenPerMeter > context.AswBerekend && !context.BerekeningVrd)
    //        {
    //            // maak melding
    //            Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtMinimaleWapening);
    //            context.MeldingenBeton.Meldingen.Add(melding);
    //        }

    //        return context;

    //    }


    //    public static void BerekenDwarskrachtWapening(BetonContext beton, DwarskrachtWapContext context)
    //    {
    //        // vanuit beton en wapening
    //        context.Fywk = beton.BetonStaal.Fyk;      // noodzakelijk voor (6.10)
    //        context.Fywd = beton.BetonStaal.Fywd;
    //        context.Fck = beton.Fck;

    //        context.FactorK = GetFactorK(context.NutHoogte); // 19Feb2018
    //        context.Rho1 = GetRho1(context.AsLangs, context.Breedte, context.NutHoogte);
    //        context.RhoMin = GetRhoMin(beton.Fck, beton.BetonStaal.Fyk);
    //        context.SterkteReductieV = GetSterkteReductieV(beton.Fck);

    //        context.SchuifspanningWeerstandZonderDwarskrachtWapeningMin = GetVergelijkingZesPuntDrieN(context.FactorK, beton.Fck);
    //        context.Crdc = GetCrdC(beton.GammaC);
    //        context.SchuifspanningWeerstandZonderDwarskrachtWapening = GetSchuifSpanningWeerstandZonderDwarskrachtWapening(context.SchuifspanningWeerstandZonderDwarskrachtWapeningMin, context.Crdc, context.FactorK, context.Rho1, beton.Fcd);

    //        // TODO: Keuze inbouwen voor bepaling van hoogte z  (0,9d, op basis van MEd, of op basis van MRd)

    //        // z
    //        context.Z = 0.9 * context.NutHoogte;


    //        //A;sw,min minimaal benodigde wapening = Rho;w,min * b * 1000mm conform art. 9.2.2(5)
    //        context.AswMin = context.RhoMin / 100 * context.Breedte * 1000;
    //        // Asw = 1000 * Ved / ( z * Fywd * cotTheta ) , afgeleid uit art. 6.2.3 (3) (6.8)
    //        context.AswBerekend = 1000000 * context.Ved / (context.Z * beton.BetonStaal.Fywd * context.CotTheta);
    //        // controle of AswMin van toepassing is
    //        context.AswBenPerMeter = Math.Max(context.AswMin, context.AswBerekend);

    //        // zorg dat eerst de benodigde wapening bekend is voordat we VrdMax berekenen
    //        context.VrdMax = GetVrdMax(context.AlphaCw, context.Breedte, context.Z, context.SterkteReductieV1, beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha);

    //        // s;l,max
    //        // standaard s;l,max = 0,75×d;
    //        context.BeugelAfstandMaxLangs = 0.75 * context.NutHoogte;
    //        // maar s;l,max mag niet groter dan 300mm
    //        context.BeugelAfstandMaxLangs = context.BeugelAfstandMaxLangs > 300 ? 300 : context.BeugelAfstandMaxLangs;  // maar niet groter dan 300mm

    //        // indien geen dwarskrachtwapening nodig dan geldt s;l,max = 300;
    //        if (context.SchuifspanningD <= context.SchuifspanningWeerstandZonderDwarskrachtWapening) { context.BeugelAfstandMaxLangs = 300; }

    //        // zoek de beugelwapening (indien aanwezig & Ved opgave)
    //        if (context.LijstBeugelWap != null && context.BerekeningVrd == false)
    //        {
    //            // indien de VRdMax groter is dan VEd kunnen we nog met een grotere v1 rekenen indien de staalspanning < 80%fyk
    //            double factor = 1;

    //            if (context.Ved > context.VrdMax)
    //            {
    //                factor = context.Ved / context.VrdMax;
    //                if (factor > 1.25) factor = 1.25;   // meer dan 125% heeft geen zin.
    //            }

    //            BeugelWap beugelWap = BerekeningenBeton.ZoekBeugelWap(context.AswBenPerMeter * factor, context.LijstBeugelWap);
    //            if (beugelWap != null)
    //            {
    //                context.BeugelDiameter = beugelWap.Diameter;
    //                context.BeugelHartOpHartAfstand = beugelWap.Hoh;
    //                context.BeugelSnedeAantal = beugelWap.AantalSnede;
    //                context.AswToegepast = beugelWap.AswToegepast;
    //            }

    //            // Beschouw opnieuw de VRdMax
    //            context.VrdMax = GetVrdMax(context.AlphaCw, context.Breedte, context.Z, context.SterkteReductieV1, beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha);
    //            // en controleer of VEd (nog steeds) niet groter is dan VRdMax 
    //            if (context.Ved > context.VrdMax)
    //            {
    //                // foutmelding!!!!!!!! verhoog de drukdiagonaal!
    //                // maak melding
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtOverschrijdingVRdMax);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }

    //            if (context.BeugelHartOpHartAfstand > context.BeugelAfstandMaxLangs)
    //            {
    //                // Kleinere beugelafstand kiezen?
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtBeugelAfstandTeGrootHohMaat);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }

    //            if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
    //            {
    //                // Meer beugelsneden toepassen?
    //                Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtBeugelAfstandDwarsTeGroot);
    //                context.MeldingenBeton.Meldingen.Add(melding);
    //            }
    //        }


    //        // s;t,max  art. 9.2.2 (8) NB
    //        bool testBeugelsnedeAfstand;                  // true or false om te kijken of we de maximale s;t,max kunnen opschroeven
    //        testBeugelsnedeAfstand = context.Ved <= 0.5 * context.VrdMax ? true : false;     // als true, dan s;t,max = 500
    //        if (testBeugelsnedeAfstand)
    //        {
    //            context.BeugelAfstandMaxDwars = 500;                                                         // als true, dan s;t,max = 500
    //        }
    //        else
    //        {
    //            context.BeugelAfstandMaxDwars = 0.75 * context.NutHoogte;                                           // als fase, dan 0.75 d
    //            context.BeugelAfstandMaxDwars = context.BeugelAfstandMaxDwars > 500 ? 500 : context.BeugelAfstandMaxDwars; // maar niet groter dan 500 met de ?operator
    //        }

    //        // s;t,applied (bij gelijkmatige verdeling)

    //        do
    //        {
    //            context.BeugelAfstandDwarsToegepast = (context.Breedte - context.DekkingZijkantToegepast * 2 - context.BeugelDiameter) / (context.BeugelSnedeAantal - 1);

    //            if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
    //            {
    //                context.BeugelSnedeAantal += 2;
    //                context.AswToegepast = BerekeningenBeton.WapStavenNaarDoorsnedeAs(context.BeugelSnedeAantal, context.BeugelDiameter, context.BeugelHartOpHartAfstand);

    //                // ToDO keuze of 2/4/6/8 of 2/3/4/5/
    //                // TODO melding maken, beugelsnede verhoogd ivm maximale beugelafstand in de dwarsrichting!
    //                // Daarna opnieuw Vrd bereken!!!
    //            }
    //        } while ((context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars));


    //        // V_Rd,s volgens art. 6.2.3(3) formule (6.8)
    //        double reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
    //        if (context.SpanningWapeningKleinerDan80ProcentKarakteristiekeVloeigrens)
    //        {
    //            //OPMERKING Indien vergelijking (6.10) is gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot
    //            //0,8 fywk.
    //            reductiefactorIndienVergelijkingZesPuntTienGebruikt = (beton.BetonStaal.Fyk * 0.8) / (beton.BetonStaal.Fyk / beton.BetonStaal.GammaS);
    //        }
    //        else { reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1; }
    //        context.VrdS = 0.001 * (context.AswToegepast / 1000) * context.Z * beton.BetonStaal.Fywd * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;


    //        // V_Rd is de kleinste van V_Rd,s en V_Rd,max
    //        context.Vrd = Math.Min(context.VrdS, context.VrdMax);

    //        if (context.AswBenPerMeter > context.AswBerekend && !context.BerekeningVrd)
    //        {
    //            // maak melding
    //            //Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtMinimaleWapening);
    //            //context.MeldingenBeton.Meldingen.Add(melding);
    //        }
    //    }

    //    #endregion


    //    // diverse (binnen vergelijkingen)
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="d">Nuttige hoogte</param>
    //    /// <returns></returns>
    //    public static double GetFactorK(double d)
    //    {
    //        double _k = 1 + Math.Sqrt(200 / d);
    //        if (_k <= 2)
    //        {
    //            return _k;
    //        }
    //        else
    //        {
    //            return 2.0;
    //        }
    //    } // factor k in berekening dwarskrachtwapening = 1 + √ (200 / d)	≤ 2,0
    //    public static double GetRho1(double OppLangsWap, double b, double d)
    //    {
    //        //context.Rho1 = (context.AsLangs / context.Breedte / context.NutHoogte) * 100;
    //        return OppLangsWap / b / d;
    //    }
    //    public static double GetRhoMin(double fck, double fyk)
    //    {
    //        //context.RhoMin = (0.08 * Math.Sqrt(beton.Fck)) / betonstaal.Fyk * 100;      // art. 9.xx (NB)
    //        return (0.08 * Math.Sqrt(fck)) / fyk * 100;      // art. 9.xx (NB)
    //    }

    //    // vgl. (6.10)
    //    public static double GetSterkteReductieV1(double fck, double staalSpanning, double fyk)
    //    {

    //        // (3) De waarde van v1 moet gelijk aan v zijn genomen (indien de spanning groter of gelijk 80% fyk)
    //        double percentage = staalSpanning / fyk;
    //        if (percentage >= 0.80) return GetSterkteReductieV(fck);
    //        if (percentage < 0.80)
    //        {
    //            double returnVal = fck < 60 ? 0.6 : 0.9 - fck / 200;// als f;ck < 60 dan geldt [v1 = 0,6] , anders geldt [v1 =  0,9 - f;ck/200]
    //            if (returnVal < 0.5) { returnVal = 0.5; }// maar niet kleinder dan 0,5 (dit is bij f;ck => 90)
    //            return returnVal;
    //        }
    //        else return 0;

    //    }

    //    public static double GetSchuifSpanningWeerstandZonderDwarskrachtWapeningMin(double k, double fck)
    //    {
    //        // 6.2.2(1) Nationale bijlage 
    //        //De waarde van vmin moet gelijk aan 0,035 k^1,5 × fck^0,5 zijn genomen.
    //        // N.B. dit is het gedeeldte tussen ( ) in Vergelijking (6.2.b)
    //        return 0.035 * Math.Pow(k, (1.5)) * Math.Sqrt(fck);
    //    }

    //    public static double GetSchuifSpanningWeerstandZonderDwarskrachtWapening(double vmin, double cRdc, double k, double rho1, double fcd)
    //    {
    //        // N.B. dit is het gedeeldte tussen [ ] in Vergelijking (6.2.a)
    //        double returnVal = cRdc * k * Math.Pow((100 * rho1 * fcd), (1.0 / 3.0));
    //        if (returnVal < vmin) { return vmin; }
    //        else return returnVal;
    //    }

    //    public static double GetCrdC(double gammaC)
    //    {
    //        // 6.2.2(1) Nationale bijlage 
    //        return 0.18 / gammaC;
    //    }


    //    // Vergelijking (6.1)
    //    public static double GetVergelijkingZesPuntEen(double dwarskrachtVrdS, double dwarskrachtVccd, double dwarskrachtVtd)
    //    {
    //        //VRd = VRd,s + Vccd + Vtd
    //        return dwarskrachtVrdS + dwarskrachtVccd + dwarskrachtVtd;
    //    }

    //    // Vergelijking (6.2.a)
    //    //public static double GetVrdC(double cRdC, double rho1, double fck, double bw, double d)
    //    //{
    //    //	//VRd,c = [CRd,ck(100 ρ l fck)1/3] bwd (6.2.a)
    //    //	double k = GetFactorK(d);
    //    //	double vMin = GetVmin(k, fck);
    //    //	double cRdC


    //    //	// inclusief invloed langswapening
    //    //	context.Crdc = 0.18 / beton.GammaC;
    //    //	context.SchuifspanningMin2 = context.Crdc * context.FactorK * Math.Pow((100 * context.Rho1 * beton.Fcd), (1 / 3));
    //    //	// uiteindelijk de grootste van de twee hierboven gebruiken
    //    //	context.SchuifspanningMin = Math.Max(context.SchuifspanningMin1, context.SchuifspanningMin2);


    //    //}

    //    // Vergelijking (6.2.b)
    //    // Vergelijking (6.3N)
    //    public static double GetVergelijkingZesPuntDrieN(double k, double fck)
    //    {
    //        //vmin = 0,035 × k^1.5 × fck^0.5
    //        double vMinTest = 0.035 * Math.Pow(k, 1.5) * Math.Pow(fck, 0.5);
    //        return 0.035 * Math.Pow(k, 1.5) * Math.Pow(fck, 0.5);
    //    }

    //    // Vergelijking (6.4) nog niet geprogrammeerd 

    //    // Vergelijking (6.5)
    //    public static bool GetVergelijkingZesPuntVijf(double dwarskrachtVed, double bw, double d, double nu, double fcd)
    //    {
    //        //VEd ≤ 0,5 × bw × d × ν × fcd ?
    //        if (dwarskrachtVed <= 0.5 * bw * d * nu * fcd) return true;
    //        else return false;
    //    }


    //    // Vergelijking (6.6N)

    //    public static double GetSterkteReductieV(double fck)
    //    {
    //        // sterktereductie voor beotn, gescheurd door dwarskracht 
    //        return 0.6 * (1 - (fck / 250));
    //    }



    //    // Vergelijking (6.7N)
    //    public static bool GetVergelijkingZesPuntZevenN(double cotTheta)
    //    {
    //        if (cotTheta < 1.0 || cotTheta > 2.5) return false;
    //        else return true;
    //    }

    //    // Vergelijking (6.8)
    //    public static double GetVergelijkingZesPuntAcht(double oppAsw, double s, double z, double fywd, double cotTheta)
    //    {
    //        return (oppAsw / s) * z * fywd * cotTheta;
    //    }





    //    // Vergelijking (6.9) 
    //    public static double GetVrdMaxVoorElementenMetHaakseDwarskrachtWapening(double alphaCW, double bW, double z, double nu1, double fcd, double cotTheta, double tanTheta)
    //    {
    //        //VRd,max = αcw bw z ν1 fcd/(cot θ + tan θ )
    //        return alphaCW * bW * z * nu1 * fcd / (cotTheta + tanTheta) / 1000;
    //    }

    //    // Vergelijking (6.14 of 6.9) 
    //    public static double GetVrdMax(double alphaCW, double bW, double z, double nu1, double fcd, double cotTheta, double tanTheta, double hoekDwarskrachtWapening)
    //    {
    //        //Overload met opgave hoek van de dwarskrachtwapening

    //        // Als de hoek 90 graden is dan vergelijking 6.9 gebruiken!
    //        if (hoekDwarskrachtWapening == 90)
    //        {
    //            return GetVrdMaxVoorElementenMetHaakseDwarskrachtWapening(alphaCW, bW, z, nu1, fcd, cotTheta, tanTheta);
    //        }
    //        // als de dwarskrachtwapening niet haaks maar met een hoek kleiner dan 90 graden dan vergelijking 6.xx gebruiken
    //        double tanAlpha = Math.Tan(hoekDwarskrachtWapening * Math.PI / 180);
    //        double cotAlpha = 1 / tanAlpha;
    //        //(4) Voor elementen met hellende dwarskrachtwapening
    //        //VRd,max = αcw bw z ν1 fcd/(cotθ + cotα ) / (1 + cot²θ)	
    //        return alphaCW * bW * z * nu1 * fcd * (cotTheta + cotAlpha) / (1 + Math.Pow(cotTheta, 2));
    //    }
    //}
}
