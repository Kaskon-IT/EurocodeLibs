namespace Eurocode.BetonConstructies
{
    public partial class BetonContext
    {
        // 3.1.4 Kruip en krimp

        // (1)P Kruip en krimp van beton hangen af van de vochtigheid van de omgeving, de afmetingen van het 
        // element en de samenstelling van het beton. Kruip wordt ook beïnvloed door de mate van verharding van
        // het beton op het moment dat de belasting voor het eerst is aangebracht en hangt af van de duur en de
        // grootte van de belasting.



        public double TangentModulus
        {
            get
            {
                return 1.05 * Ecm;
            }
        }


        //public string KruipEigenWaarde { get; set; } = "";
        public double? KruipEigenWaarde { get; set; } = null;



        /// <summary>
        /// (B.1)
        /// </summary>
        public double KruipCoefficient
        {
            get
            {
                if (KruipEigenWaarde == null)
                    return TheoretischeKruipCoefficient * VergelijkingB7;
                else
                    return KruipEigenWaarde.Value;


                //if (string.IsNullOrEmpty(KruipEigenWaarde))
                //    return TheoretischeKruipCoefficient * VergelijkingB7;
                //else
                //{
                //    double.TryParse(KruipEigenWaarde, out double kruip);
                //    return kruip;
                //}

            }
        }

        /// <summary>
        /// (B.2)
        /// </summary>
        public double TheoretischeKruipCoefficient
        {
            get
            {
                return FactorRelatieveVochtigheid * BetaFcm * BetaOuderdom;
            }
        }

        /// <summary>
        /// (B.3a) en (B.3b)
        /// </summary>
        public double FactorRelatieveVochtigheid
        {
            get
            {
                double hulp = (1 - (double)RelatieveVochtigheid / 100) / (0.1 * Math.Pow(TheoretischeDikteBeton_h0, 1.0 / 3.0));

                if (Fcm <= 35)
                {
                    // f_cm kleiner of gelijk aan 35 MPa (B.3a)
                    return 1 + hulp;
                }
                else
                {
                    // f_cm > 35 MPa (B.3b)
                    return (1 + hulp * CoefficentInvloedBetonsterkteAlpha1) * CoefficentInvloedBetonsterkteAlpha2;
                }
            }
        }


        public double CoefficentInvloedBetonsterkteAlpha1
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.7);
            }
        }
        public double CoefficentInvloedBetonsterkteAlpha2
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.2);
            }
        }

        public double CoefficentInvloedBetonsterkteAlpha3
        {
            get
            {
                return Math.Pow(35 / Fcm, 0.5);
            }
        }


        public int MachtCementsoort
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S: return -1;
                    default: return 0;
                    case CementklasseEnum.N: return 0;
                    case CementklasseEnum.R: return 1;
                }
            }
        }



        /// <summary>
        /// (B.4)
        /// een factor die rekening houdt met het effect van de betonsterkte op de theoretische kruipcoefficient.
        /// = 16,8 / Wortel(fcm)  
        /// </summary>
        public double BetaFcm
        {
            get
            {
                return 16.8 / Math.Sqrt(Fcm);
            }
        }

        /// <summary>
        /// (B.5)
        /// een factor die rekening houdt met het effect van de ouderdom van het beton op het tijdstip van belasten op de theoretische kruipcoefficient.
        /// </summary>
        public double BetaOuderdom
        {
            get
            {
                return 1 / (0.1 + Math.Pow(OuderdomInclusiefCement, 0.20));

            }
        }



        public int RelatieveVochtigheid { get; set; } = 50;







        public int OuderdomBeton_t { get; set; } = 18250;

        public int OuderdomBetonOpMomentVanBelasten_t0 { get; set; } = 30;

        public double OuderdomInclusiefCement
        {
            get
            {
                // NB. (B.10) is niet voorzien, gebruik t0 voor t0T.
                double t0T = OuderdomBetonOpMomentVanBelasten_t0;
                return this.GetOuderdomVergelijkingB9(t0T);
            }
        }

        public int OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts { get; set; } = 1;


        public ParametrischeProfielen.ParametrischProfielContext Profiel { get; set; } = new(500, 700);


        /// <summary>
        /// 3.1.8 Buigtreksterkte
        /// (1) De gemiddelde buigtreksterkte van gewapend-betonelementen hangt af van de gemiddelde axiale treksterkte en de hoogte van de dwarsdoorsnede. De volgende relatie mag zijn gebruikt:
        /// Formule (3.23)
        /// </summary>
        public double FctmFl
        {
            get
            {
                double val1 = (1.6 - Profiel.Hoogte / 1000) * this.Fctm;
                double val2 = this.Fctm;
                return Math.Max(val1, val2);
            }
        }


        /// <summary>
        /// de oppervalkte van de dwarsdoorsnede.
        /// VERVALLEN --> ProfielContext Profiel
        /// </summary>
        public double OppervlakteDwarsdoorsnedeBeton_Ac { get; set; } = 500 * 700;

        /// <summary>
        /// de omtrek van het element dat in aanraking komt met de buitenlucht.
        /// VERVALLEN --> ProfielContext Profiel
        /// </summary>
        public double OmtrekDeelDwarsdoorsnedeBlootgesteldAanUitdroging_u { get; set; } = 2 * (500 + 700);

        /// <summary>
        /// (B.6)
        /// </summary>
        public double TheoretischeDikteBeton_h0
        {
            get
            {
                double u = 2 * (this.Profiel.Breedte + this.Profiel.Hoogte);
                return 2 * this.Profiel.Area / u;
                //return 2 * OppervlakteDwarsdoorsnedeBeton_Ac / OmtrekDeelDwarsdoorsnedeBlootgesteldAanUitdroging_u;
            }
        }


        public double VergelijkingB7
        {
            get
            {
                return Math.Pow((double)(OuderdomBeton_t - OuderdomBetonOpMomentVanBelasten_t0) / (double)(BetaH + OuderdomBeton_t - OuderdomBetonOpMomentVanBelasten_t0), 0.3);
            }
        }

        public double BetaH
        {
            get
            {
                if (Fcm <= 35)
                {
                    var value1 = 1.5 * (1 + Math.Pow(0.012 * RelatieveVochtigheid, 18)) * TheoretischeDikteBeton_h0 + 250;
                    var value2 = 1500;
                    return Math.Min(value1, value2);
                }
                else
                {
                    var value1 = 1.5 * (1 + Math.Pow(0.012 * RelatieveVochtigheid, 18)) * TheoretischeDikteBeton_h0 + 250 * CoefficentInvloedBetonsterkteAlpha3;
                    var value2 = 1500 * CoefficentInvloedBetonsterkteAlpha3;
                    return Math.Min(value1, value2);
                }
            }
        }


        // 3.1.4 Kruip en krimp
        // (6) De totale krimpverkorting is samengesteld uit twee componenten:
        // de uitdrogingskrimpverkorting en de autogene krimpverkorting.

        /// <summary>
        /// Vergelijking (3.8)
        /// </summary>
        public double TotaleKrimpverkorting
        {
            get
            {
                return UitdrogingsKrimpverkorting + AutogeneKrimpverkorting;
            }
        }

        /// <summary>
        /// Vergelijking (3.9)
        /// 
        /// </summary>
        public double UitdrogingsKrimpverkorting
        {
            get
            {
                return BetaDs * CoefficientFictieveHoogte * BasisVerkortingUitdrogingskrimp;
            }
        }


        /// <summary>
        /// Vergelijking (3.10)
        /// Wordt gebruikt in vergelijking (3.9)
        /// Factor gebruikt bij bepalen van de uitdrogingskrimpverkorting
        /// </summary>
        public double BetaDs
        {
            get
            {
                double deltaT = OuderdomBeton_t - OuderdomBetonBeginUitdrogingsKrimpOfZwelling_ts;
                double hulp = 0.04 * Math.Sqrt(Math.Pow(TheoretischeDikteBeton_h0, 3));
                return deltaT / (deltaT + hulp);
            }
        }

        /// <summary>
        /// Tabel 3.3
        /// </summary>
        public double CoefficientFictieveHoogte
        {
            get
            {
                // 100 - 1.00
                // 200 - 0.85
                // 300 - 0.75
                // >500 - 0.70
                if (TheoretischeDikteBeton_h0 <= 100)
                    return 1.00;
                else if (TheoretischeDikteBeton_h0 <= 200)
                {
                    return 1.00 - (TheoretischeDikteBeton_h0 - 100.0) / 100.0 * 0.15;
                }
                else if (TheoretischeDikteBeton_h0 <= 300)
                {
                    return 0.85 - (TheoretischeDikteBeton_h0 - 200.0) / 100.0 * 0.10;
                }
                else if (TheoretischeDikteBeton_h0 <= 500)
                {
                    return 0.75 - (TheoretischeDikteBeton_h0 - 300.0) / 200.0 * 0.05;
                }
                else return 0.70;

            }
        }


        /// <summary>
        /// Vergelijking (3.11)
        /// De autogene krimpverkorting
        /// Is BetaAS * EpsilonCaOneindig
        /// </summary>
        public double AutogeneKrimpverkorting
        {
            get
            {
                return BetaAS * EpsilonCaOneindig;
            }
        }


        /// <summary>
        /// Vergelijking (3.12)
        /// Deze wordt gebruikt in vergelijking (3.11)
        /// = 2,5 (fck – 10) ×10-6
        /// </summary>
        public double EpsilonCaOneindig
        {
            get
            {
                return 2.5 * (Fck - 10) * 0.000001;
            }
        }


        /// <summary>
        /// Vergelijking (3.13)
        /// = 1 – exp (– 0,2 t ^0,5)
        /// </summary>
        public double BetaAS
        {
            get
            {
                return 1 - Math.Pow(Math.E, (-0.2 * Math.Pow(OuderdomBeton_t, 0.5)));
            }
        }


        /// <summary>
        /// Basis verkorting uitdrogingskrimp
        /// Vergelijking (B.11)
        /// 
        /// </summary>
        public double BasisVerkortingUitdrogingskrimp
        {
            get
            {
                double hulp1 = (220 + 110 * AlphaDs1);
                double macht = -AlphaDs2 * (Fcm / Fcmo);
                double hulp2 = Math.Pow(Math.E, macht);
                return 0.85 * (hulp1 * hulp2) * 0.000001 * BetaRH;
            }
        }

        /// <summary>
        /// Vergelijking (B.12)
        /// Wordt gebruikt in vergelijking (B.11)
        /// Bijlage B.2
        /// </summary>
        public double BetaRH
        {
            get
            {

                double hulp = Math.Pow(RelatieveVochtigheid / RH0, 3);
                return 1.55 * (1 - hulp);
            }
        }

        public double RH0
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Gebruikt in vergelijking (B.11)
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        public double Fcmo
        {
            get
            {
                return 10;
            }
        }

        /// <summary>
        /// is een coëfficiënt die afhangt van de cementsoort
        /// Gebruikt in vergelijking (B.11) 
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        public double AlphaDs1
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S:
                        return 3;
                    default:
                    case CementklasseEnum.N:
                        return 4;
                    case CementklasseEnum.R:
                        return 6;
                }
            }
        }


        /// <summary>
        /// is een coëfficiënt die afhangt van de cementsoort
        /// Gebruikt in vergelijking (B.11)
        /// Bijlage B.2 Basisvergelijkingen voor het bepalen van de verkorting ten gevolge van uitdrogingskrimp
        /// </summary>
        public double AlphaDs2
        {
            get
            {
                switch (CementKlasse)
                {
                    case CementklasseEnum.S:
                        return 0.13;
                    default:
                    case CementklasseEnum.N:
                        return 0.12;
                    case CementklasseEnum.R:
                        return 0.11;
                }
            }
        }

        /// <summary>
        /// De effectieve elasticiteitsmodulus van beton. (inclusief kruip)
        /// Ec,eff = Ecm / (1 + KruipCoefficient)
        /// </summary>
        public double EcEff
        {
            get
            {
                return Ecm / (1 + KruipCoefficient);
            }
        }

        public double Alphae
        {
            get
            {
                return BetonStaalContext.Es / (EcEff * 1000);
            }
        }




    }

    public static class KruipEnKrimpExtensions
    {
        public static double GetOuderdomVergelijkingB9(this BetonContext context, double t0t)
        {
            //(2) Het effect van de cementsoort(zie 3.1.2(6)) op de kruipcoëfficiënt van het beton mag in rekening zijn
            //gebracht door het aanpassen van de duur van de belasting t0 in vergelijking(B.5) volgens de volgende
            //vergelijking: 

            double minValue = 0.5;
            var helper = 2 + Math.Pow(t0t, 1.2);
            double value = t0t * Math.Pow(9 / helper + 1, context.MachtCementsoort);

            return Math.Max(minValue, value);

        }





    }

}
