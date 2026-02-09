using Eurocode.Belastingen;
using Profielen.Parametrisch;
using Profielen.Beton;

namespace Eurocode.BetonConstructies
{
    public class DwarskrachtWap
    {
        // 6.2 Dwarskracht
        // Berekeningen dwarskracht conform NEN-EN-1992-1-1
        // Dit hoofdstuk bestrijkt vergelijkingen (6.1) t/m (6.25)


        #region Berekeningen aan te roepen vanuit interface


        public static DwarskrachtWapContext GetDwarskrachtWapContext(
            BetonContext beton,
            BetonProfiel profiel,
            double theta,
            double d,
            SectionForces snedekrachten,
            double aantalSnede,
            double diameter,
            List<double> hohAfstanden)
        {

            // tijdelijk test => verwijderen na afronding

            List<string> artikelen = new List<string>();

            DwarskrachtWapContext context = new(beton, profiel, snedekrachten)
            {
                Theta = theta,
                NutHoogte = d,
                LijstBeugelWap = []
            };

            foreach (double hohAfstand in hohAfstanden)
            {
                context.LijstBeugelWap.Add(new BeugelWap() { AantalSnede = aantalSnede, Diameter = diameter, Hoh = hohAfstand });
            }


            context.Fywk = beton.BetonStaal.Fyk;      // noodzakelijk voor (6.10)
            context.Fywd = beton.BetonStaal.Fywd;
            //context.Fck = beton.Fck;

            //context.SetFactorK();
            //context.SetRho1();
            //context.SetRhoMin();
            //context.SetSterkteReductieV1();
            //context.SetSchuifspanningWeerstandZonderWapeningMin();
            //context.SetCrdc();
            //context.SetSchuifspanningWeerstandZonderDwarskrachtWapening();
            //artikelen.Add(context.SetAswMin());
            //artikelen.Add(context.SetAswBerekend());

            //context.AswBenPerMeter = Math.Max(context.AswMin, context.AswBerekend);

            //context.SetVrdMax();
            //artikelen.Add(context.SetBeugelAfstandMaxLangs());

            //context.ControleerVrdMax();

            // stel wapening in
            // maak de berekening
            context.Update();

            if (Math.Abs(context.Ved) > context.DwarskrachtWeerstandMax)
            {
                // overschrijding
                double overschrijding = Math.Abs(context.Ved) / context.DwarskrachtWeerstandMax;

                //  
                bool spanningBlijftOnderDeGrens = context.SchuifspanningD < 0.8 * context.Beton.BetonStaal.Fyk;




                if (!spanningBlijftOnderDeGrens)
                {
                    // mogelijk kunnen we nog proberen om Asw te verhogen om onder de 80% te blijven.

                }
                else
                {
                    // overschrijding, stop

                }

            }



            // zoek de beugelwapening (indien aanwezig & Ved opgave)
            if (context.LijstBeugelWap != null)
            {
                // indien de VRdMax groter is dan VEd kunnen we nog met een grotere v1 rekenen indien de staalspanning < 80%fyk
                double factor = 1;

                if (Math.Abs(context.Ved) > context.DwarskrachtWeerstandMax)
                {
                    factor = Math.Abs(context.Ved) / context.DwarskrachtWeerstandMax;
                    if (factor > 1.25) factor = 1.25;   // meer dan 125% heeft geen zin.
                }

                BeugelWap beugelWap = WapeningHelper.ZoekBeugelWap(context.AswBenPerMeter * factor, context.LijstBeugelWap);
                if (beugelWap != null)
                {
                    context.BeugelDiameter = beugelWap.Diameter;
                    context.BeugelHartOpHartAfstand = beugelWap.Hoh;
                    context.BeugelSnedeAantal = beugelWap.AantalSnede;
                    context.AswToegepast = beugelWap.AswToegepast;
                }

                // Beschouw opnieuw de VRdMax
                //context.DwarskrachtWeerstandMax = DwarskrachtHelpers.GetVrdMax(context.AlphaCw, context.Breedte, context.Z, context.SterkteReductieFactorBetonGescheurdDoorDwarskracht1, beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha, out _);

                // en controleer of VEd (nog steeds) niet groter is dan VRdMax 
                if (context.Ved > context.DwarskrachtWeerstandMax)
                {
                    // foutmelding!!!!!!!! verhoog de drukdiagonaal!
                    // maak melding
                    var melding = CommonLibrary.Helpers.MeldingenBetonHelper.GetMelding(2003);
                    context.Meldingen.Add(melding);
                }
                else
                {
                    //var melding = MeldingenBetonHelper.GetMelding(204);
                    //context.Meldingen.Add(melding);
                }

                if (context.BeugelHartOpHartAfstand > context.BeugelAfstandMaxLangs)
                {
                    // Kleinere beugelafstand kiezen?
                    //Melding melding = new Melding((int)MeldingenBetonHelper.LijstMeldingen.DwarskrachtBeugelAfstandTeGrootHohMaat);
                    //context.MeldingenBeton.Meldingen.Add(melding);
                }

                if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
                {
                    // Meer beugelsneden toepassen?
                    //Melding melding = new Melding((int)MeldingenBetonHelper.LijstMeldingen.DwarskrachtBeugelAfstandDwarsTeGroot);
                    //context.MeldingenBeton.Meldingen.Add(melding);
                }
            }






            // s;t,max  art. 9.2.2 (8) NB
            bool testBeugelsnedeAfstand;                  // true or false om te kijken of we de maximale s;t,max kunnen opschroeven
            testBeugelsnedeAfstand = Math.Abs(context.Ved) <= 0.5 * context.DwarskrachtWeerstandMax ? true : false;     // als true, dan s;t,max = 500
            if (testBeugelsnedeAfstand)
            {
                context.BeugelAfstandMaxDwars = 500;                                                         // als true, dan s;t,max = 500
            }
            else
            {
                context.BeugelAfstandMaxDwars = 0.75 * context.NutHoogte;                                           // als fase, dan 0.75 d
                context.BeugelAfstandMaxDwars = context.BeugelAfstandMaxDwars > 500 ? 500 : context.BeugelAfstandMaxDwars; // maar niet groter dan 500 met de ?operator
            }

            // s;t,applied (bij gelijkmatige verdeling)

            do
            {
                context.BeugelAfstandDwarsToegepast = (context.Breedte - context.DekkingZijkantToegepast * 2 - context.BeugelDiameter) / (context.BeugelSnedeAantal - 1);

                if (context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars)
                {
                    context.BeugelSnedeAantal += 1; // ToDO keuze of 2/4/6/8 of 2/3/4/5/




                    // TODO melding maken, beugelsnede verhoogd ivm maximale beugelafstand in de dwarsrichting!
                    // Daarna opnieuw Vrd bereken!!!
                }
            } while ((context.BeugelAfstandDwarsToegepast > context.BeugelAfstandMaxDwars));


            // V_Rd,s volgens art. 6.2.3(3) formule (6.8)
            double reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
            if (context.SpanningWapeningKleinerDan80ProcentKarakteristiekeVloeigrens)
            {
                //OPMERKING Indien vergelijking (6.10) is gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot
                //0,8 fywk.
                reductiefactorIndienVergelijkingZesPuntTienGebruikt = (beton.BetonStaal.Fyk * 0.8) / (beton.BetonStaal.Fyk / beton.BetonStaal.GammaS);
                //context.DwarskrachtWeerstandStaal = 0.001 * (context.AswToegepast / 1000) * context.Z * beton.BetonStaal.Fyk * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;
            }
            else
            {
                reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
                //context.DwarskrachtWeerstandStaal = 0.001 * (context.AswToegepast / 1000) * context.Z * beton.BetonStaal.Fywd * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;
            }



            // V_Rd is de kleinste van V_Rd,s en V_Rd,max
            //context.DwarskrachtWeerstand = Math.Min(context.DwarskrachtWeerstandStaal, context.DwarskrachtWeerstandMax);

            if (context.AswBenPerMeter > context.AswBerekend && !context.BerekeningVrd)
            {
                // maak melding
                //Melding melding = new Melding((int)MeldingenBeton.LijstMeldingen.DwarskrachtMinimaleWapening);
                //context.MeldingenBeton.Meldingen.Add(melding);
            }

            return context;

        }




        #endregion

    }


    public static class DwarskrachtExtensions
    {


        public static void Update(this DwarskrachtWapContext context)
        {
            //context.SetFactorK();
            //context.SetRho1();
            //context.SetRhoWMin();
            //context.SetSterkteReductieFactorBetonGescheurdDoorDwarskracht();
            //context.SetSterkteReductieFactorBetonGescheurdDoorDwarskracht1();
            //context.SetSchuifspanningWeerstandZonderWapeningMin();
            //context.SetCrdc();
            //context.SetSchuifspanningWeerstandZonderDwarskrachtWapening();
            //context.Artikelen.Add(context.SetAswMin());
            //context.Artikelen.Add(context.SetAswBerekend());

            //context.AswBenPerMeter = Math.Max(context.AswMin, context.AswBerekend);

            if (context.BerekeningType == BerekeningTypeEnum.BepaalBenodigeWapening)
            {
                context.AswToegepast = context.AswBenPerMeter + 0.01;
            }



            //context.SetVrdMax();
            //context.SetVrds();


            context.Artikelen.Add(context.SetBeugelAfstandMaxLangs());
        }



        public static double SetFactorK(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetFactorK(context.NutHoogte);
        }

        public static double SetRho1(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetRho1(context.AsLangs, context.Profiel.BreedteDwarskracht, context.NutHoogte);
        }

        public static double SetRhoWMin(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetRhoWMin(context.Beton.Fck, context.Beton.BetonStaal.Fyk);
        }

        public static double SetNu(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetSterkteReductieV(context.Beton.Fck);
        }

        public static double SetNu1(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetSterkteReductieV1(context.Beton.Fck, context.SpanningDwarskrachtWapening, context.Beton.BetonStaal.Fyk);
        }

        public static double SetSchuifspanningWeerstandZonderWapeningMin(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetSchuifSpanningWeerstandZonderDwarskrachtWapeningMin(context.FactorK, context.Beton.Fck);
        }

        public static double SetSchuifspanningWeerstandZonderDwarskrachtWapening(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetSchuifSpanningWeerstandZonderDwarskrachtWapening(
                context.SchuifspanningMin, context.Crdc, context.FactorK, context.RhoLangs, context.Beton.Fcd, context.FactorK1DwarskrachtWeerstandBeton, context.SigmaCp);
        }

        public static double SetCrdc(this DwarskrachtWapContext context)
        {
            return DwarskrachtHelpers.GetCrdC(context.Beton.PartieleFactor);
        }

        public static string SetBeugelAfstandMaxLangs(this DwarskrachtWapContext context)
        {
            context.BeugelAfstandMaxLangs = DwarskrachtHelpers.GetAslMax(context.NutHoogte, context.SchuifspanningD, context.SchuifspanningWeerstandBeton, context.CotAlpha, out string art);
            return art;
        }



        public static (double value, string art) SetVrdMax(this DwarskrachtWapContext context)
        {
            double returnVal = DwarskrachtHelpers.GetVrdMax(context.AlphaCw, context.Profiel.BreedteDwarskracht, context.Z, context.Nu1, context.Beton.Fcd, context.CotTheta, context.TanTheta, context.Alpha, out string art);
            return (returnVal, art);
        }

        public static (double vrds, string art) SetVrds(this DwarskrachtWapContext context)
        {
            // V_Rd,s volgens art. 6.2.3(3) formule (6.8)
            string art = "";
            double reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
            double returnVal = 0;
            if (context.SpanningWapeningKleinerDan80ProcentKarakteristiekeVloeigrens)
            {
                //OPMERKING Indien vergelijking (6.10) is gebruikt behoort de waarde van fywd in vergelijking (6.8) te zijn verminderd tot
                //0,8 fywk.
                art = "(6.10)";
                reductiefactorIndienVergelijkingZesPuntTienGebruikt = (context.Beton.BetonStaal.Fyk * 0.8) / (context.Beton.BetonStaal.Fyk / context.Beton.BetonStaal.GammaS);
                returnVal = 0.001 * (context.AswToegepast / 1000) * context.Z * context.Beton.BetonStaal.Fyk * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;

            }
            else
            {
                art = "(6.14)";
                reductiefactorIndienVergelijkingZesPuntTienGebruikt = 1;
                returnVal = 0.001 * (context.AswToegepast / 1000) * context.Z * context.Beton.BetonStaal.Fywd * reductiefactorIndienVergelijkingZesPuntTienGebruikt * context.CotTheta;
            }

            // werk de schuifspanning weerstandswaarde bij
            //context.SchuifspanningWeerstandStaal = returnVal / (context.Profiel.BreedteDwarskracht * 1000); // in N/mm²



            return (returnVal, art);

            //context.DwarskrachtWeerstandStaal = 
        }

        public static (double value, string art) SetAswMin(this DwarskrachtWapContext context)
        {
            double returnVal = DwarskrachtHelpers.GetAsMin(context.RhoWMin, context.Profiel.BreedteDwarskracht, out string art);
            return (returnVal, art);
        }

        public static (double value, string art) SetAswBerekend(this DwarskrachtWapContext context)
        {


            double returnVal = DwarskrachtHelpers.GetAswBerekend(Math.Abs(context.Ved), context.Z, context.Beton.BetonStaal.Fywd, context.CotTheta, out string art);
            return (returnVal, art);
        }


    }

    public static class DwarskrachtHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double GetFactorK(double d)
        {
            double _k = 1 + Math.Sqrt(200 / d);
            if (_k <= 2)
            {
                return _k;
            }
            else
            {
                return 2.0;
            }
        } // factor k in berekening dwarskrachtwapening = 1 + √ (200 / d)	≤ 2,0
        public static double GetRho1(double OppLangsWap, double b, double d)
        {
            //context.Rho1 = (context.AsLangs / context.Breedte / context.NutHoogte) * 100;
            return OppLangsWap / b / d;
        }


        public static double GetRhoWMin(double fck, double fyk)
        {
            //context.RhoMin = (0.08 * Math.Sqrt(beton.Fck)) / betonstaal.Fyk * 100;      // art. 9.xx (NB)
            return (0.08 * Math.Sqrt(fck)) / fyk * 100;      // art. 9.2.2 (5) (NB)
        }

        // vgl. (6.10)
        public static double GetSterkteReductieV1(double fck, double staalSpanning, double fyk)
        {

            // (3) De waarde van v1 moet gelijk aan v zijn genomen (indien de spanning groter of gelijk 80% fyk)
            double percentage = staalSpanning / fyk;
            if (percentage >= 0.80) return GetSterkteReductieV(fck);
            if (percentage < 0.80)
            {
                double returnVal = fck < 60 ? 0.6 : 0.9 - fck / 200;// als f;ck < 60 dan geldt [v1 = 0,6] , anders geldt [v1 =  0,9 - f;ck/200]
                if (returnVal < 0.5) { returnVal = 0.5; }// maar niet kleinder dan 0,5 (dit is bij f;ck => 90)
                return returnVal;
            }
            else return 0;

        }

        public static double GetSchuifSpanningWeerstandZonderDwarskrachtWapeningMin(double k, double fck)
        {
            // 6.2.2(1) Nationale bijlage 
            //De waarde van vmin moet gelijk aan 0,035 k^1,5 × fck^0,5 zijn genomen.
            // N.B. dit is het gedeeldte tussen ( ) in Vergelijking (6.2.b)
            return 0.035 * Math.Pow(k, (1.5)) * Math.Sqrt(fck);
        }

        public static double GetSchuifSpanningWeerstandZonderDwarskrachtWapening(double vmin, double cRdc, double k, double rho1, double fcd, double k1 = 0.15, double sigmacp = 0)
        {
            // N.B. dit is het gedeeldte tussen [ ] in Vergelijking (6.2.a) met een minimum van vmin + k1 * sigmacp
            return Math.Max(cRdc * k * Math.Pow((100 * rho1 * fcd), (1.0 / 3.0)) + k1 * sigmacp, vmin + k1 * sigmacp);
        }

        public static double GetCrdC(double gammaC)
        {
            // 6.2.2 (1) Nationale bijlage 
            return 0.18 / gammaC;
        }


        public static double GetAsMin(double rhoMin, double b, out string art)
        {

            //A;sw,min minimaal benodigde wapening = Rho;w,min * b * 1000mm conform art. 9.2.2 (5)
            art = "9.2.2 (5)";
            return rhoMin / 100 * b * 1000;
        }

        public static double GetAswBerekend(double Ved, double z, double fywd, double cotTheta, out string art)
        {
            art = "6.2.3 (3) (vgl 6.8)";
            return 1e6 * Ved / (z * fywd * cotTheta);
        }


        public static double GetAslMax(double d, double schuifspanningRekenwaarde, double schuifspanningWeerstandZonderDwarskrachtWapening, double cotAlpha, out string art)
        {
            art = "9.2.2 (6)";

            // (9.6N)
            // (6) Indien geen dwarskrachtwapening is vereist, moet de waarde van sl,max gelijk aan 300 mm zijn genomen.
            // Indien wel dwarskrachtwapening is vereist, moet de waarde van sl,max gelijk aan de kleinste waarde van
            // 0,75 d(1 + cot |alpha|) en 300 mm zijn genomen.
            if (schuifspanningRekenwaarde <= schuifspanningWeerstandZonderDwarskrachtWapening) return 300;

            double bovengrens = 300;
            double returnVal = 0.75 * d * (1 + cotAlpha);

            return Math.Min(bovengrens, returnVal);

        }







        // Vergelijking (6.1)
        public static double GetVergelijkingZesPuntEen(double dwarskrachtVrdS, double dwarskrachtVccd, double dwarskrachtVtd)
        {
            //VRd = VRd,s + Vccd + Vtd
            return dwarskrachtVrdS + dwarskrachtVccd + dwarskrachtVtd;
        }

        // Vergelijking (6.2.a)
        //public static double GetVrdC(double cRdC, double rho1, double fck, double bw, double d)
        //{
        //	//VRd,c = [CRd,ck(100 ρ l fck)1/3] bwd (6.2.a)
        //	double k = GetFactorK(d);
        //	double vMin = GetVmin(k, fck);
        //	double cRdC


        //	// inclusief invloed langswapening
        //	context.Crdc = 0.18 / beton.GammaC;
        //	context.SchuifspanningMin2 = context.Crdc * context.FactorK * Math.Pow((100 * context.Rho1 * beton.Fcd), (1 / 3));
        //	// uiteindelijk de grootste van de twee hierboven gebruiken
        //	context.SchuifspanningMin = Math.Max(context.SchuifspanningMin1, context.SchuifspanningMin2);


        //}

        // Vergelijking (6.2.b)
        // Vergelijking (6.3N)

        /// <summary>
        /// (6.3N) Schuifspanning zonder wapening min |nu|~min~
        /// </summary>
        /// <param name="k"></param>
        /// <param name="fck"></param>
        /// <returns></returns>
        public static double GetSchuifspanningZonderWapeningMin(double k, double fck)
        {
            //vmin = 0,035 × k^1.5 × fck^0.5
            double vMinTest = 0.035 * Math.Pow(k, 1.5) * Math.Pow(fck, 0.5);
            return 0.035 * Math.Pow(k, 1.5) * Math.Pow(fck, 0.5);
        }

        // Vergelijking (6.4) nog niet geprogrammeerd 

        // Vergelijking (6.5)
        public static bool GetVergelijkingZesPuntVijf(double dwarskrachtVed, double bw, double d, double nu, double fcd)
        {
            //VEd ≤ 0,5 × bw × d × ν × fcd ?
            if (dwarskrachtVed <= 0.5 * bw * d * nu * fcd) return true;
            else return false;
        }


        // Vergelijking (6.6N)

        public static double GetSterkteReductieV(double fck)
        {
            // sterktereductie voor beotn, gescheurd door dwarskracht 
            return 0.6 * (1 - (fck / 250));
        }



        // Vergelijking (6.7N)
        public static bool GetVergelijkingZesPuntZevenN(double cotTheta)
        {
            if (cotTheta < 1.0 || cotTheta > 2.5) return false;
            else return true;
        }

        // Vergelijking (6.8)
        public static double GetVergelijkingZesPuntAcht(double oppAsw, double s, double z, double fywd, double cotTheta)
        {
            return (oppAsw / s) * z * fywd * cotTheta;
        }





        // Vergelijking (6.9) 
        public static double GetVrdMaxVoorElementenMetHaakseDwarskrachtWapening(double alphaCW, double bW, double z, double nu1, double fcd, double cotTheta, double tanTheta)
        {
            //VRd,max = αcw bw z ν1 fcd/(cot θ + tan θ )
            return alphaCW * bW * z * nu1 * fcd / (cotTheta + tanTheta) / 1000;
        }

        // Vergelijking (6.14 of 6.9) 
        public static double GetVrdMax(double alphaCW, double bW, double z, double nu1, double fcd, double cotTheta, double tanTheta, double hoekDwarskrachtWapening, out string art)
        {
            //Overload met opgave hoek van de dwarskrachtwapening
            art = "";
            // Als de hoek 90 graden is dan vergelijking 6.9 gebruiken!
            if (hoekDwarskrachtWapening == 90)
            {
                art = "6.2.3 (3) (vgl. 6.9)";
                return GetVrdMaxVoorElementenMetHaakseDwarskrachtWapening(alphaCW, bW, z, nu1, fcd, cotTheta, tanTheta);
            }
            // als de dwarskrachtwapening niet haaks maar met een hoek kleiner dan 90 graden dan vergelijking 6.xx gebruiken
            double tanAlpha = Math.Tan(hoekDwarskrachtWapening * Math.PI / 180);
            double cotAlpha = 1 / tanAlpha;

            //(4) Voor elementen met hellende dwarskrachtwapening
            //VRd,max = αcw bw z ν1 fcd/(cotθ + cotα ) / (1 + cot²θ)	
            art = "6.2.3 (4) (vgl. 6.14)";
            return alphaCW * bW * z * nu1 * fcd * (cotTheta + cotAlpha) / (1 + Math.Pow(cotTheta, 2)) * 1e-3;
        }
    }

}
