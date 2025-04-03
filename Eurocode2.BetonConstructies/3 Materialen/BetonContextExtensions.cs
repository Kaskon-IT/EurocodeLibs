using static Eurocode.BetonConstructies.BetonContext;


namespace Eurocode.BetonConstructies
{
    public static class BetonContextExtensions
    {




        public static double GetFactorAlpha(double fck)
        {
            if (fck <= 50) return 0.75;
            else if (fck <= 55) return 0.75 - (((fck - 50) / 5) * 0.04);
            else if (fck <= 60) return 0.71 - (((fck - 55) / 5) * 0.04);
            else if (fck <= 70) return 0.67 - (((fck - 60) / 10) * 0.05);
            else if (fck <= 80) return 0.62 - (((fck - 70) / 10) * 0.04);
            else if (fck <= 90) return 0.58 - (((fck - 80) / 10)) * 0.02;
            else return 0.56;
        }


        public static double GetXudMax(this BetonsterkteklasseEnum sterkteKlasse)
        {
            double returnVal = 0;
            switch (sterkteKlasse)
            {
                case BetonsterkteklasseEnum.C12_15:
                case BetonsterkteklasseEnum.C16_20:
                case BetonsterkteklasseEnum.C20_25:
                case BetonsterkteklasseEnum.C25_30:
                case BetonsterkteklasseEnum.C30_37:
                case BetonsterkteklasseEnum.C35_45:
                case BetonsterkteklasseEnum.C40_50:
                case BetonsterkteklasseEnum.C45_55:
                case BetonsterkteklasseEnum.C50_60: returnVal = 0.535; break; // t/m C50/60 is .535

                case BetonsterkteklasseEnum.C55_67: returnVal = 0.507; break;
                case BetonsterkteklasseEnum.C60_75: returnVal = 0.486; break;
                case BetonsterkteklasseEnum.C70_85: returnVal = 0.466; break;
                case BetonsterkteklasseEnum.C80_95: returnVal = 0.461; break;
                case BetonsterkteklasseEnum.C90_105: returnVal = 0.461; break;
            }
            return returnVal;
        }



        public static double GetXudMax(double fcd, double epsilonCu3, double fyd)
        {
            return (epsilonCu3 * 1000000) / (epsilonCu3 * 1000000 + 7 * fyd); // ??? test
            // waar komt dit vandaan? --> 
        }



        public static double GetRho1Max(this BetonContext beton) { return GetRho1Max(beton.Fcd, beton.Alpha, beton.BetonStaal.Fyd); }
        public static double GetRho1Max(double fcd, double alpha, double fyd)
        {
            return alpha * fcd / fyd;
        }


        public static (double Alpha, double Beta) GetFactorOppervlakEnZwaartepunt(this BetonContext beton, double ec, double ecu)
        {
            double a1 = GetFactorA1(ec, ecu);
            double a2 = GetFactorA2(beton, ec, ecu);
            double y1 = GetFactorY1(a1);
            double y2 = GetFactorY2(beton, a1);


            return (1, GetZwaartepuntsFactor(a1, a2, y1, y2));
        }


        public static double GetFactorBeta(this BetonContext beton, double ec, double ecu)
        {



            double a1 = GetFactorA1(ec, ecu);
            double a2 = GetFactorA2(beton, ec, ecu);
            double y1 = GetFactorY1(a1);
            double y2 = GetFactorY2(beton, a1);

            return GetZwaartepuntsFactor(a1, a2, y1, y2);
        }
        //public static double GetFactorAlpha(this BetonContext beton, double epsC2of3, double epsCu)
        // {
        //    return GetVolheidsGraad(GetFactorA1(epsC2of3, epsCu), GetFactorA2(beton, epsC2of3, epsCu, beton.IsParabolischSpanningsRekDiagram));
        //}
        public static double GetFactorAlpha(this BetonContext beton)
        {
            if (beton.EpsilonCu == 0)
                return 0.75; //?

            return beton.SpanningRekDiagram switch
            {
                SpanningRekDiagramType.Parabolisch => GetAlphaParaboolRechthoek(beton),
                SpanningRekDiagramType.BiLineair => GetAlphaBiLineair(beton),
                _ => GetAlphaBiLineair(beton),
            };
        }

        public static double GetAlphaBiLineair(this BetonContext beton)
        {
            double sigma = beton.GetSigmaCd(beton.EpsilonCu);
            double oppDriehoek = (Math.Min(beton.EpsilonCu, beton.EpsilonC3) * sigma) * 0.5;

            double oppRechthoek = 0;
            if (beton.EpsilonCu < beton.EpsilonC3) oppRechthoek = 0; // geen rechthoek
            if (beton.EpsilonCu >= beton.EpsilonC3) oppRechthoek = sigma * (beton.EpsilonCu - beton.EpsilonC3); // rechthoek 

            double oppTotaal = oppRechthoek + oppDriehoek;
            return (oppDriehoek + oppRechthoek) / (sigma * beton.EpsilonCu);
        }

        public static double GetAlphaParaboolRechthoek(this BetonContext beton)
        {
            double sigma = beton.GetSigmaCd(beton.EpsilonCu);
            double epsilonHulp = Math.Min(beton.EpsilonCu, beton.EpsilonC2);

            // eerste hele parabool (sorry ik kwam er niet helemaal uit) door 
            double oppParaboolTotaal = beton.Fcd * (beton.EpsilonC2 - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonC2, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            double oppParaboolEraf = 0;
            if (epsilonHulp < beton.EpsilonC2)
            {
                oppParaboolEraf = beton.Fcd * ((beton.EpsilonC2 - epsilonHulp) - 1.0 / (beton.FactorN + 1) * Math.Pow((beton.EpsilonC2 - epsilonHulp), beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            }
            double oppParabool = oppParaboolTotaal - oppParaboolEraf;

            double oppRechthoek = 0;
            if (beton.EpsilonCu < beton.EpsilonC2) oppRechthoek = 0; // geen rechthoek
            if (beton.EpsilonCu >= beton.EpsilonC2) oppRechthoek = sigma * (beton.EpsilonCu - beton.EpsilonC2); // rechthoek 

            //double oppTotaal = oppRechthoek + oppParabool;
            return (oppParabool + oppRechthoek) / (sigma * beton.EpsilonCu);
        }





        public static double GetAlphaParaboolTotStuik(this BetonContext beton)
        {
            // volheidsgraad Alpha voor een parabool tot aan het stuikmoment!  
            double oppParaboolTotaal = beton.Fcd * (beton.EpsilonC2 - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonC2, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            return oppParaboolTotaal / (beton.Fcd * beton.EpsilonC2);
        }

        public static double GetBetaParabool(this BetonContext beton)
        {
            double h = beton.Fcd;
            double y1 = (beton.EpsilonC2 - beton.EpsilonCu) * 1000;
            if (beton.EpsilonCu > beton.EpsilonC2) y1 = 0.000;   // grafiek stopt bij EpsC2!
            double y2 = beton.EpsilonC2 * 1000;
            double dy = y2 - y1; // is ook afstand xu
            if (dy == 0) dy = 0.0000000000001;
            double b = y2;
            double n = beton.FactorN;

            double sy =
                h * ((1.00 / 2.00) * Math.Pow(y2, 2.00) - Math.Pow(y2, n + 2) / ((n + 2) * Math.Pow(b, n))) -
                h * ((1.00 / 2.00) * Math.Pow(y1, 2.00) - Math.Pow(y1, n + 2) / ((n + 2) * Math.Pow(b, n)));
            double a =
                 h * (y2 - 1.0 / (n + 1) * Math.Pow(y2, n + 1) / Math.Pow(b, n)) -
                  h * (y1 - 1.0 / (n + 1) * Math.Pow(y1, n + 1) / Math.Pow(b, n));
            double beta =
                (sy / a - y1) / dy;
            return beta;

        }

        public static double GetFactorBeta(this BetonContext beton)
        {
            switch (beton.SpanningRekDiagram)
            {
                case SpanningRekDiagramType.Parabolisch:
                    if (beton.EpsilonCu < beton.EpsilonC2) // niet volledige parabool
                        return GetBetaParabool(beton);
                    else
                        return GetFactorBeta(beton, beton.EpsilonC2, beton.EpsilonCu);
                default:
                case SpanningRekDiagramType.BiLineair:
                    if (beton.EpsilonCu < beton.EpsilonC3)
                        return 1.00 / 3.00; // driehoek
                    else
                        return GetFactorBeta(beton, beton.EpsilonC3, beton.EpsilonCu); // driehoek+rechthoek
            }
        }


        public static double GetAlphaGedeelteVoorDeStuikGrens(this BetonContext beton)
        {

            if (beton.SpanningRekDiagram == SpanningRekDiagramType.BiLineair) return 0.5;
            else
            {
                double sigma = beton.GetSigmaCd(beton.EpsilonCu);
                double oppTot = sigma * beton.EpsilonCu;
                double oppA = beton.Fcd * (beton.EpsilonCu - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonCu, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
                return oppA / oppTot;
            }
        }




        public static double GetFactorA1(double ec, double ecu)
        {
            if (ecu < ec) return 0;
            return ((ecu - ec) / ecu);
        }

        public static double GetFactorA2(this BetonContext beton, double ec, double ecu)
        {

            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch)
            {
                return GetAlphaParaboolTotStuik(beton) * (ec / ecu); // nb. de volheidsgraad (alpha-deel) van de parabool is niet altijd 2/3 vanwege de kleinere kromming bij hogere sterkteklasse
            }
            else
                return 1.0 / 2.0 * (ec / ecu); // driehoek dus altijd maal 1/2.
        }
        public static double GetFactorY1(double a1)
        {
            return a1 / 2; // gedeelte van de rechthoek (indien aanwezig)
        }
        public static double GetFactorY2(this BetonContext beton, double a1)
        {
            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch)
            {

                return a1 + GetBetaParabool(beton) * (1 - a1); // nb. beta-deel van de parabool is niet altijd 3/8 aangezien de voor hogere betonsterkte de kromming van de bool afneemt. 

            }
            else
                return a1 + 1.0 / 3.0 * (1 - a1); // gedeelte van de driehoek
        }
        public static double GetVolheidsGraad(double a1, double a2)
        {
            return a1 + a2;
        }
        public static double GetZwaartepuntsFactor(double a1, double a2, double y1, double y2)
        {
            return (a1 * y1 + a2 * y2) / (a1 + a2);
        }






        public static double GetFactorN(this BetonContext beton) { return GetFactorN(beton.Fck); }
        public static double GetFactorN(double fck)
        {
            double returnVal;
            if (fck >= 50) { returnVal = 1.4 + 23.4 * Math.Pow(((90 - fck) / 100), 4); }
            else returnVal = 2.0;
            return returnVal;
        }

        public static double GetEpsilonCu1(this BetonContext beton) { return GetEpsilonCu1(beton.Fck, beton.Fcm); }
        public static double GetEpsilonCu1(double fck, double fcm)
        {
            double returnVal;
            if (fck >= 50) { returnVal = (2.8 + 27 * Math.Pow(((98 - fcm) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }





        public static (double alpha, double beta) GetAlphaBeta(this BetonContext beton, double optredendeBetonrek)
        {
            double betonspanning = beton.GetBetonspanning(optredendeBetonrek);

            double oppervlakte = beton.GetOppervlakteBetonDiagram(optredendeBetonrek) - beton.GetOppervlakteBetonDiagram(0.0);
            double statischMoment = beton.GetStatischMomentBetonDiagram(optredendeBetonrek) - beton.GetStatischMomentBetonDiagram(0.0);

            double alpha = oppervlakte / (betonspanning * optredendeBetonrek);
            double zwaartePunt = optredendeBetonrek - statischMoment / oppervlakte;
            double beta = zwaartePunt / optredendeBetonrek;

            return (alpha, beta);
        }

        public static double GetOppervlakteBetonDiagram(this BetonContext beton, double optredendeBetonrek)
        {
            double output;
            double betonrek;
            double oppervlakteEen = 0.0;
            double oppervlakteTwee = 0.0;

            if (optredendeBetonrek <= beton.GetEpsilonBetonStuik())
            {
                betonrek = optredendeBetonrek;
            }
            else
            {
                betonrek = beton.GetEpsilonBetonStuik();
                oppervlakteTwee = beton.Fcd * (optredendeBetonrek - beton.GetEpsilonBetonStuik());
            }


            switch (beton.SpanningRekDiagram)
            {
                case SpanningRekDiagramType.Parabolisch:
                    oppervlakteEen = beton.Fcd * (betonrek + Math.Pow(1.0 - betonrek / beton.GetEpsilonBetonStuik(), beton.FactorN + 1.0) * beton.GetEpsilonBetonStuik() / (beton.FactorN + 1.0));

                    break;
                case SpanningRekDiagramType.BiLineair:
                    oppervlakteEen = beton.Fcd * Math.Pow(betonrek, 2) / (2 * beton.GetEpsilonBetonStuik());

                    break;
            }



            output = oppervlakteEen + oppervlakteTwee;
            return output;
        }


        public static double GetStatischMomentBetonDiagram(this BetonContext beton, double optredendeBetonrek)
        {
            double betonrek;
            double statischMomentEen = 0.0;
            double statischMomentTwee = 0.0;

            if (optredendeBetonrek <= beton.GetEpsilonBetonStuik())
            {
                betonrek = optredendeBetonrek;
                statischMomentTwee = 0;
            }
            else
            {
                betonrek = beton.GetEpsilonBetonStuik();
                statischMomentTwee = (optredendeBetonrek - beton.GetEpsilonBetonStuik()) * beton.Fcd * (beton.GetEpsilonBetonStuik() + 0.5 * (optredendeBetonrek - beton.GetEpsilonBetonStuik()));
            }

            switch (beton.SpanningRekDiagram)
            {
                case SpanningRekDiagramType.Parabolisch:
                    double component1 = -2.0 * (betonrek - beton.GetEpsilonBetonStuik());
                    double component2 = betonrek * beton.FactorN + beton.GetEpsilonBetonStuik() + betonrek;
                    double component3 = Math.Pow((beton.GetEpsilonBetonStuik() - betonrek) / beton.GetEpsilonBetonStuik(), beton.FactorN);
                    double component4 = Math.Pow(betonrek, 2.0) * (beton.FactorN + 2.0) * (beton.FactorN + 1.0);
                    double component5 = 2.0 * (beton.FactorN + 2.0) * (beton.FactorN + 1.0);

                    statischMomentEen = beton.Fcd * (component1 * component2 * component3 + component4) / component5;
                    break;
                case SpanningRekDiagramType.BiLineair:
                    statischMomentEen = beton.Fcd * Math.Pow(betonrek, 3) / (3 * beton.GetEpsilonBetonStuik());
                    break;

            }


            double output = statischMomentEen + statischMomentTwee;
            return output;
        }


        public static double GetBetonspanning(this BetonContext beton, double optredendeBetonrek)
        {
            double output;

            if (optredendeBetonrek <= beton.GetEpsilonBetonStuik())
            {
                output = beton.SpanningRekDiagram switch
                {
                    SpanningRekDiagramType.Parabolisch => beton.Fcd * (1 - Math.Pow(1 - optredendeBetonrek / beton.GetEpsilonBetonStuik(), beton.FactorN)),
                    _ => beton.Fcd * optredendeBetonrek / beton.GetEpsilonBetonStuik(),
                };
            }
            else
            {
                output = beton.Fcd;
            }

            return output;
        }


        public static double GetEpsilonC2(this BetonContext beton) { return GetEpsilonC2(beton.Fck); }
        public static double GetEpsilonC2(double fck)
        {
            double returnVal;
            if (fck >= 50) { returnVal = (2.0 + 0.085 * Math.Pow((fck - 50), 0.53)) / 1000; }
            else returnVal = 2.0 / 1000;
            return returnVal;
        }

        public static double GetEpsilonCu2(this BetonContext beton) { return GetEpsilonCu2(beton.Fck); }
        public static double GetEpsilonCu2(double fck)
        {
            double returnVal;
            if (fck >= 50) { returnVal = (2.6 + 35 * Math.Pow(((90 - fck) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }



        public static double GetEpsilonC3(this BetonContext beton) { return GetEpsilonC3(beton.Fck); }
        public static double GetEpsilonC3(double fck)
        {
            double returnVal;
            if (fck >= 50) { returnVal = (1.75 + 0.55 * ((fck - 50) / 40)) / 1000; }
            else returnVal = 1.75 / 1000;
            return returnVal;
        }




        public static double GetEpsilonCu3(this BetonContext beton) { return GetEpsilonCu3(beton.Fck); }
        public static double GetEpsilonCu3(double fck)
        {
            double returnVal;
            if (fck >= 50) { returnVal = (2.6 + 35 * Math.Pow(((90 - fck) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }


        public static double GetEcm(this BetonContext beton) { return GetEcm(beton.Fcm); }
        public static double GetEcm(double fcm)
        {
            double returnVal;
            returnVal = 22 * Math.Pow((fcm / 10), 0.3);

            return returnVal;
        }

        public static double GetEpsilonC1(this BetonContext beton) { return GetEpsilonC1(beton.Fcm); }
        public static double GetEpsilonC1(double fcm)
        {
            double returnVal;
            returnVal = 0.7 * Math.Pow(fcm, 0.31) / 1000;
            if (returnVal > 2.8 / 1000) returnVal = 2.8 / 1000;

            return returnVal;
        }




        public static double GetSigmaCd(this BetonContext beton, double epsilonCd)
        {

            if (epsilonCd > beton.GetEpsilonStuik())
                return beton.Fcd; // rechthoek gedeelte

            // ( 1 - (1 - (ec / ec2) )^n ) fcd
            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch)
            {
                return beton.Fcd * (1 - Math.Pow((1 - (epsilonCd / beton.EpsilonC2)), beton.FactorN)); // parabool gedeelte
            }
            else return beton.Fcd * (epsilonCd / beton.EpsilonC3);  // stijgend lineair gedeelte
        }


        public static double GetBetonRekAangepast(this BetonContext beton)
        {
            double epsS = beton.BetonStaal.EpsilonS;
            double epsUd = beton.BetonStaal.EpsilonUd;
            double epsCu = beton.EpsilonCu3;
            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch) epsCu = beton.EpsilonCu2;

            //return epsCu - (epsS - epsUd) / epsS * epsCu;
            return (epsUd / epsS) * epsCu;
        }


        public static double GetEpsilonBetonBreuk(this BetonContext beton)
        {
            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch) return beton.EpsilonCu2;
            else return beton.EpsilonCu3;
        }


        public static double GetEpsilonBetonStuik(this BetonContext beton)
        {
            if (beton.SpanningRekDiagram == SpanningRekDiagramType.Parabolisch) return beton.EpsilonC2;
            else return beton.EpsilonC3;
        }

    }

}
