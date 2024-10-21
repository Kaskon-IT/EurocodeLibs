using System.ComponentModel;


namespace Eurocode.BetonConstructies
{
    public partial class BetonContext
    {
        // copyConstructor
        public BetonContext(BetonContext vorige)
        {
            Betonsterkteklasse = vorige.Betonsterkteklasse;
            IsOntwerpSituatieBuitenGewoon = vorige.IsOntwerpSituatieBuitenGewoon;
            IsParabolischSpanningsRekDiagram = vorige.IsParabolischSpanningsRekDiagram;
            CementKlasse = vorige.CementKlasse;
            Alpha = vorige.Alpha;
            Beta = vorige.Beta;
            EpsilonC = vorige.EpsilonC;
            FckEigenOpgave = vorige.FckEigenOpgave;
            FckCubeEigenOpgave = vorige.FckCubeEigenOpgave;
            BetonStaal = new BetonStaalContext(vorige.BetonStaal);  // copy constructor van het betonstaal.
        }



        // 3.2 betonstaal als onderdeel
        public BetonStaalContext BetonStaal = new();


        public bool IsParabolischSpanningsRekDiagram { get; set; } = !true;



        public enum BetonsterkteklasseEnum
        {
            [Description("C12/15")] C12_15 = 1,
            [Description("C16/20")] C16_20 = 2,
            [Description("C20/25")] C20_25 = 3,
            [Description("C25/30")] C25_30 = 4,
            [Description("C30/37")] C30_37 = 5,
            [Description("C35/45")] C35_45 = 6,
            [Description("C40/50")] C40_50 = 7,
            [Description("C45/55")] C45_55 = 8,
            [Description("C50/60")] C50_60 = 9,
            [Description("C55/67")] C55_67 = 10,
            [Description("C60/75")] C60_75 = 11,
            [Description("C70/85")] C70_85 = 12,
            [Description("C80/95")] C80_95 = 13,
            [Description("C90/105")] C90_105 = 14,
            [Description("Eigen opgave")] Eigen_Opgave = 99,

        }

        public enum CementklasseEnum
        {
            [Description("Klasse R (Rapid)")] R = 1,
            [Description("Klasse N (Normal)")] N = 2,
            [Description("Klasse S (Slow")] S = 3,
        }

        public CementklasseEnum CementKlasse { get; set; }

        public double CoefficientCementKlasse
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S: return 0.38;      // slow
                    case CementklasseEnum.N: return 0.25;      // normal
                    case CementklasseEnum.R: return 0.20;      // rapid
                    default: return 0.20;
                }
            }
        }


        /// <summary>
        /// Eigen opgave cilinder druksterkte
        /// </summary>
        public double FckEigenOpgave { get; set; }

        /// <summary>
        /// Eigen opgave kubus druksterkte 
        /// </summary>
        public double FckCubeEigenOpgave { get; set; }



        public BetonsterkteklasseEnum Betonsterkteklasse { get; set; }

        public string BetonSterkteKlasseGebruiksvriendelijkeNaam
        {
            get
            {
                return $"C{Fck}/{FckCube}";
            }
        }
        public string BetonDiagramGebruiksvriendelijkeNaam
        {
            get
            {
                if (IsParabolischSpanningsRekDiagram)
                    return "Parabolisch";
                else
                    return "Bi-lineair";
            }
        }
        public string BetonStaalDiagramGebruiksvriendelijkeNaam
        {
            get
            {
                if (BetonStaal.IsHellendeTakDiagram)
                    return "Bi-lineair met hellende tak";
                else
                    return "Bi-lineair met horizontale tak";
            }

        }

        /// <summary>
        /// De representieve cilinder druksterkte in N/mm²
        /// </summary>
        public double Fck
        {
            get
            {
                return Betonsterkteklasse switch
                {
                    BetonsterkteklasseEnum.C12_15 => 12,
                    BetonsterkteklasseEnum.C16_20 => 16,
                    BetonsterkteklasseEnum.C20_25 => 20,
                    BetonsterkteklasseEnum.C25_30 => 25,
                    BetonsterkteklasseEnum.C30_37 => 30,
                    BetonsterkteklasseEnum.C35_45 => 35,
                    BetonsterkteklasseEnum.C40_50 => 40,
                    BetonsterkteklasseEnum.C45_55 => 45,
                    BetonsterkteklasseEnum.C50_60 => 50,
                    BetonsterkteklasseEnum.C55_67 => 55,
                    BetonsterkteklasseEnum.C60_75 => 60,
                    BetonsterkteklasseEnum.C70_85 => 70,
                    BetonsterkteklasseEnum.C80_95 => 80,
                    BetonsterkteklasseEnum.C90_105 => 90,
                    BetonsterkteklasseEnum.Eigen_Opgave => FckEigenOpgave,
                    _ => 40,
                };
            }
        }


        //(MPa)
        public double FckCube
        {
            get
            {
                return Betonsterkteklasse switch
                {
                    BetonsterkteklasseEnum.C12_15 => 15,
                    BetonsterkteklasseEnum.C16_20 => 20,
                    BetonsterkteklasseEnum.C20_25 => 25,
                    BetonsterkteklasseEnum.C25_30 => 30,
                    BetonsterkteklasseEnum.C30_37 => 37,
                    BetonsterkteklasseEnum.C35_45 => 45,
                    BetonsterkteklasseEnum.C40_50 => 50,
                    BetonsterkteklasseEnum.C45_55 => 55,
                    BetonsterkteklasseEnum.C50_60 => 60,
                    BetonsterkteklasseEnum.C55_67 => 67,
                    BetonsterkteklasseEnum.C60_75 => 75,
                    BetonsterkteklasseEnum.C70_85 => 85,
                    BetonsterkteklasseEnum.C80_95 => 95,
                    BetonsterkteklasseEnum.C90_105 => 105,
                    BetonsterkteklasseEnum.Eigen_Opgave => FckCubeEigenOpgave,
                    _ => 50,
                };
            }
        }

        /// <summary>
        /// gemiddelde waarde van de cilinderdruksterkte van beton 
        /// </summary>
        public double Fcm { get { return Fck + 8; } }

        /// <summary>
        /// gemiddelde waarde van de axiale treksterkte van beton 
        /// </summary>
        public double Fctm
        {
            get
            {
                if (Fck <= 50.0)
                {
                    // Fctm tot C50/60
                    return 0.3 * Math.Pow(Fck, 2.0 / 3.0);
                }
                else
                {
                    // Fctm wanneer groter dan C50/60
                    return 2.12 * Math.Log(1.0 + (Fcm / 10.0));
                }
            }
        }


        public double FctkVijfProcent { get { return 0.7 * Fctm; } }    // 5% fractiel

        public double FcktVijfEnNegentigProcent { get { return 1.3 * Fctm; } } // 95% fractiel

        public bool IsOntwerpSituatieBuitenGewoon = false;  // default Blijvend en tijdelijk conform art. 2.4.2.4 (1) Partiële factoren voor materialen 


        public double GammaC
        {
            get
            {
                if (!IsOntwerpSituatieBuitenGewoon) return 1.5;  // is de partiële veiligheidsfactor voor beton, zie 2.4.2.4
                else return 1.2;
            }
        }
        // conform art. 2.4.2.4 (1) Partiële factoren voor materialen

        public double Fcd { get { return AlphaCC * Fck / GammaC; } }

        public double Fctd { get { return AlphaCT * FctkVijfProcent / GammaC; } }

        public const double AlphaCT = 1; // 3.1.6 Dit is de coëfficiënt die rekening houdt met langeduureffecten op de treksterkte en met ongunstige effecten als gevolg van de manier waarop de belasting aangrijpt.
        public const double AlphaCC = 1; // 3.1.6 Dit is de coëfficiënt die rekening houdt met langeduureffecten op de druksterkte en met ongunstige effecten als gevolg van de manier waarop de belasting aangrijpt.

        /// <summary>
        /// De maxiale waarde van de druksterkte Cmax moet gelijk aan C90/105 zijn genomen.
        /// </summary>
        public const double FckMax = 90;
        /// <summary>
        /// De maxiale waarde van de druksterkte Cmax moet gelijk aan C90/105 zijn genomen.
        /// </summary>
        public const double FckCubeMax = 105;



        //public double Beta { get { return GetFactorBeta(BetonSterkteKlasse, Fck, IsParabolischSpanningsRekDiagram); } } 
        public double Beta { get; set; }

        public double GetBeta()
        {
            if (IsParabolischSpanningsRekDiagram)
            {
                return GetFactorBeta(this, IsParabolischSpanningsRekDiagram, EpsilonC2, EpsilonCu2);
            }
            else
            {
                return GetFactorBeta(this, IsParabolischSpanningsRekDiagram, EpsilonC3, EpsilonCu3);
            }
        }

        public double GetAlpha()
        {
            if (IsParabolischSpanningsRekDiagram)
            {
                return GetFactorAlpha(this, IsParabolischSpanningsRekDiagram, EpsilonC2, EpsilonCu2);
            }
            else
            {
                return GetFactorAlpha(this, IsParabolischSpanningsRekDiagram, EpsilonC3, EpsilonCu3);
            }
        }

        public double GetEpsilonBreuk()
        {
            return GetEpsilonBetonBreuk(this);
        }

        public double GetEpsilonStuik()
        {
            return GetEpsilonBetonStuik(this);
        }

        public void SetBeta(double beta)
        {
            this.Beta = beta;
        }
        public void SetAlpha(double alpha)
        {
            this.Alpha = alpha;
        }


        public double Alpha { get; set; }


        public double XudMax { get { return GetXudMax(Fcd, EpsilonCu3, BetonStaal.Fyd); } }

        public double Rho1Max { get { return GetRho1Max(Fcd, XudMax, Alpha, BetonStaal.Fyd); } }

        //public double KruipFactor { get; set; }
        //public double EcMetKruip { get; set; }
        public double Ecm { get { return GetEcm(Betonsterkteklasse, Fcm); } }


        public double EpsilonC { get; set; }
        public double SigmaCd { get; set; }
        public double SigmaCk { get; set; }

        public double EpsilonC1 { get { return GetEpsilonC1(Betonsterkteklasse, Fcm); } }

        public double EpsilonCu1 { get { return GetEpsilonCu1(Betonsterkteklasse, Fck, Fcm); } }

        public double EpsilonC2 { get { return GetEpsilonC2(Betonsterkteklasse, Fck); } }
        public double EpsilonCu2 { get { return GetEpsilonCu2(Betonsterkteklasse, Fck); } }

        public double FactorN { get { return GetFactorN(Betonsterkteklasse, Fck); } }

        public double EpsilonC3 { get { return GetEpsilonC3(Betonsterkteklasse, Fck); } }
        public double EpsilonCu3 { get { return GetEpsilonCu3(Betonsterkteklasse, Fck); } }

        public double? EpsilonCuAangepast
        {
            get
            {
                if (BetonStaal.EpsilonS > BetonStaal.EpsilonUd)
                {
                    return GetBetonRekAangepast(this);
                }
                else return null;
            }
        }


        public BetonContext(string sterkteklasse)
        {
            sterkteklasse = sterkteklasse.Replace("/", "_");
            Enum.TryParse(sterkteklasse, out BetonsterkteklasseEnum sterkteklasseEnum);
            Betonsterkteklasse = sterkteklasseEnum;
        }


        public BetonContext(BetonsterkteklasseEnum klasse)
        {
            Betonsterkteklasse = klasse;
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, bool isParabolischSpanningsRekDiagram, BetonStaalContext.BetonStaalKwaliteitEnum betonStaalKwaliteit, bool isHellendeTakDiagram)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            IsParabolischSpanningsRekDiagram = isParabolischSpanningsRekDiagram;
            BetonStaal = new BetonStaalContext(betonStaalKwaliteit, isHellendeTakDiagram);
            if (isParabolischSpanningsRekDiagram)
            {
                EpsilonC = EpsilonCu2;
            }
            else
            {
                EpsilonC = EpsilonCu3;
            }
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, bool isParabolischSpanningsRekDiagram, BetonStaalContext betonStaal)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            IsParabolischSpanningsRekDiagram = isParabolischSpanningsRekDiagram;
            BetonStaal = betonStaal;
            if (isParabolischSpanningsRekDiagram)
            {
                EpsilonC = EpsilonCu2;
            }
            else
            {
                EpsilonC = EpsilonCu3;
            }
        }














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


        public static double GetXudMax(BetonsterkteklasseEnum sterkteKlasse)
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



        public static double GetRho1Max(double fcd, double xud, double alpha, double fyd)
        {
            return alpha * fcd / fyd;
        }



        public static double GetFactorBeta(BetonContext beton, bool isParabolisch, double ec, double ecu)
        {
            double a1 = GetFactorA1(ec, ecu);
            double a2 = GetFactorA2(beton, ec, ecu, beton.IsParabolischSpanningsRekDiagram);
            double y1 = GetFactorY1(a1);
            double y2 = GetFactorY2(beton, a1, beton.IsParabolischSpanningsRekDiagram);

            return GetZwaartepuntsFactor(a1, a2, y1, y2);
        }
        public static double GetFactorAlpha(BetonContext beton, bool isParabolisch, double epsC2of3, double epsCu)
        {
            double a1 = GetFactorA1(epsC2of3, epsCu);
            double a2 = GetFactorA2(beton, epsC2of3, epsCu, isParabolisch);

            return GetVolheidsGraad(GetFactorA1(epsC2of3, epsCu), GetFactorA2(beton, epsC2of3, epsCu, isParabolisch));
        }
        public static double GetFactorAlpha(BetonContext beton)
        {
            if (beton.EpsilonC == 0) beton.EpsilonC = 0.0001;
            if (!beton.IsParabolischSpanningsRekDiagram)
            {
                return GetAlphaBiLineair(beton);
            }
            else
            {
                return GetAlphaParaboolRechthoek(beton);
            }
        }

        public static double GetAlphaBiLineair(BetonContext beton)
        {
            double sigma = BetonContext.GetSigmaCd(beton, beton.EpsilonC);
            double oppDriehoek = (Math.Min(beton.EpsilonC, beton.EpsilonC3) * sigma) * 0.5;

            double oppRechthoek = 0;
            if (beton.EpsilonC < beton.EpsilonC3) oppRechthoek = 0; // geen rechthoek
            if (beton.EpsilonC > beton.EpsilonCu3) beton.EpsilonC = beton.EpsilonCu3;   // maximale rek
            if (beton.EpsilonC >= beton.EpsilonC3) oppRechthoek = sigma * (beton.EpsilonC - beton.EpsilonC3); // rechthoek 

            double oppTotaal = oppRechthoek + oppDriehoek;
            return (oppDriehoek + oppRechthoek) / (sigma * beton.EpsilonC);
        }

        public static double GetAlphaParaboolRechthoek(BetonContext beton)
        {
            double sigma = BetonContext.GetSigmaCd(beton, beton.EpsilonC);
            double epsilonHulp = Math.Min(beton.EpsilonC, beton.EpsilonC2);

            // eerste hele parabool (sorry ik kwam er niet helemaal uit) door 
            double oppParaboolTotaal = beton.Fcd * (beton.EpsilonC2 - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonC2, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            double oppParaboolEraf = 0;
            if (epsilonHulp < beton.EpsilonC2)
            {
                oppParaboolEraf = beton.Fcd * ((beton.EpsilonC2 - epsilonHulp) - 1.0 / (beton.FactorN + 1) * Math.Pow((beton.EpsilonC2 - epsilonHulp), beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            }
            double oppParabool = oppParaboolTotaal - oppParaboolEraf;

            double oppRechthoek = 0;
            if (beton.EpsilonC < beton.EpsilonC2) oppRechthoek = 0; // geen rechthoek
            if (beton.EpsilonC > beton.EpsilonCu2) beton.EpsilonC = beton.EpsilonCu2;   // maximale rek
            if (beton.EpsilonC >= beton.EpsilonC2) oppRechthoek = sigma * (beton.EpsilonC - beton.EpsilonC2); // rechthoek 

            //double oppTotaal = oppRechthoek + oppParabool;
            return (oppParabool + oppRechthoek) / (sigma * beton.EpsilonC);
        }

        public static double GetAlphaParaboolTotStuik(BetonContext beton)
        {
            // volheidsgraad Alpha voor een parabool tot aan het stuikmoment!  
            double oppParaboolTotaal = beton.Fcd * (beton.EpsilonC2 - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonC2, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
            return oppParaboolTotaal / (beton.Fcd * beton.EpsilonC2);
        }

        public static double GetBetaParabool(BetonContext beton)
        {
            double h = beton.Fcd;
            double y1 = (beton.EpsilonC2 - beton.EpsilonC) * 1000;
            if (beton.EpsilonC > beton.EpsilonC2) y1 = 0.000;   // grafiek stopt bij EpsC2!
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

        public static double GetFactorBeta(BetonContext beton)
        {
            // parabool-rechthoek
            if (beton.IsParabolischSpanningsRekDiagram)
            {
                if (beton.EpsilonC < beton.EpsilonC2) // niet volledige parabool
                {
                    return GetBetaParabool(beton); // werkt nog niet als n != 2;

                }
                else return GetFactorBeta(beton, beton.IsParabolischSpanningsRekDiagram, beton.EpsilonC2, beton.EpsilonC);
            }
            // bi-lineair
            else
            {
                if (beton.EpsilonC < beton.EpsilonC3) return 1.00 / 3.00; // driehoek
                else return GetFactorBeta(beton, false, beton.EpsilonC3, beton.EpsilonC); // driehoek+rechthoek
            }
        }


        public static double GetAlphaGedeelteVoorDeStuikGrens(BetonContext beton)
        {

            if (!beton.IsParabolischSpanningsRekDiagram) return 0.5;
            else
            {
                double sigma = GetSigmaCd(beton, beton.EpsilonC);
                double oppTot = sigma * beton.EpsilonC;
                double oppA = beton.Fcd * (beton.EpsilonC - 1.0 / (beton.FactorN + 1) * Math.Pow(beton.EpsilonC, beton.FactorN + 1) / Math.Pow(beton.EpsilonC2, beton.FactorN));
                return oppA / oppTot;
            }
        }




        public static double GetFactorA1(double ec, double ecu)
        {
            if (ecu < ec) return 0;
            return ((ecu - ec) / ecu);
        }

        public static double GetFactorA2(BetonContext beton, double ec, double ecu, bool isParabolisch)
        {
            if (isParabolisch)
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
        public static double GetFactorY2(BetonContext beton, double a1, bool isParabolisch)
        {
            if (isParabolisch)
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







        public static double GetFactorN(BetonsterkteklasseEnum sterkteKlasse, double fck)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = 1.4 + 23.4 * Math.Pow(((90 - fck) / 100), 4); }
            else returnVal = 2.0;
            return returnVal;
        }

        public static double GetEpsilonCu1(BetonsterkteklasseEnum sterkteKlasse, double fck, double fcm)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = (2.8 + 27 * Math.Pow(((98 - fcm) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }

        public static double GetEpsilonC2(BetonsterkteklasseEnum sterkteKlasse, double fck)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = (2.0 + 0.085 * Math.Pow((fck - 50), 0.53)) / 1000; }
            else returnVal = 2.0 / 1000;
            return returnVal;
        }


        public static double GetEpsilonCu2(BetonsterkteklasseEnum sterkteKlasse, double fck)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = (2.6 + 35 * Math.Pow(((90 - fck) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }



        public static double GetEpsilonC3(BetonsterkteklasseEnum sterkteKlasse, double fck)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = (1.75 + 0.55 * ((fck - 50) / 40)) / 1000; }
            else returnVal = 1.75 / 1000;
            return returnVal;
        }


        public static double GetEpsilonCu3(BetonsterkteklasseEnum sterkteKlasse, double fck)
        {
            double returnVal = 0;
            if (fck >= 50) { returnVal = (2.6 + 35 * Math.Pow(((90 - fck) / 100), 4)) / 1000; }
            else returnVal = 3.5 / 1000;
            return returnVal;
        }




        public static double GetEcm(BetonsterkteklasseEnum sterkteKlasse, double fcm)
        {
            double returnVal = 0;
            returnVal = 22 * Math.Pow((fcm / 10), 0.3);

            return returnVal;
        }

        public static double GetEpsilonC1(BetonsterkteklasseEnum sterkteKlasse, double fcm)
        {
            double returnVal = 0;
            returnVal = 0.7 * Math.Pow(fcm, 0.31) / 1000;
            if (returnVal > 2.8 / 1000) returnVal = 2.8 / 1000;

            return returnVal;
        }




        public static double GetSigmaCd(BetonContext beton, double epsilonCd)
        {

            if (epsilonCd > beton.GetEpsilonStuik())
                return beton.Fcd; // rechthoek gedeelte

            // ( 1 - (1 - (ec / ec2) )^n ) fcd
            if (beton.IsParabolischSpanningsRekDiagram)
            {
                return beton.Fcd * (1 - Math.Pow((1 - (epsilonCd / beton.EpsilonC2)), beton.FactorN)); // parabool gedeelte
            }
            else return beton.Fcd * (epsilonCd / beton.EpsilonC3);  // stijgend lineair gedeelte
        }


        public static double GetBetonRekAangepast(BetonContext beton)
        {
            double epsS = beton.BetonStaal.EpsilonS;
            double epsUd = beton.BetonStaal.EpsilonUd;
            double epsCu = beton.EpsilonCu3;
            if (beton.IsParabolischSpanningsRekDiagram) epsCu = beton.EpsilonCu2;

            //return epsCu - (epsS - epsUd) / epsS * epsCu;
            return (epsUd / epsS) * epsCu;
        }


        public static double GetEpsilonBetonBreuk(BetonContext beton)
        {
            if (beton.IsParabolischSpanningsRekDiagram) return beton.EpsilonCu2;
            else return beton.EpsilonCu3;
        }


        public static double GetEpsilonBetonStuik(BetonContext beton)
        {
            if (beton.IsParabolischSpanningsRekDiagram) return beton.EpsilonC2;
            else return beton.EpsilonC3;
        }






    }
}
