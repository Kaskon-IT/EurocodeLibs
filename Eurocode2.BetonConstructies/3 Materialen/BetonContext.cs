namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// 3 Materialen
    /// 3.1 Beton en 3.2 Betonstaal
    /// Verzameling gegevens voor berekening van betonconstructies.
    /// </summary>
    public partial class BetonContext
    {
        public BetonContext()
        {

        }
        /// <summary>
        /// Maakt een kopie.
        /// </summary>
        /// <param name="vorige"></param>
        public BetonContext(BetonContext vorige)
        {
            Betonsterkteklasse = vorige.Betonsterkteklasse;
            IsOntwerpSituatieBuitenGewoon = vorige.IsOntwerpSituatieBuitenGewoon;
            CementKlasse = vorige.CementKlasse;
            Alpha = vorige.Alpha;
            Beta = vorige.Beta;
            FckEigenOpgave = vorige.FckEigenOpgave;
            FckCubeEigenOpgave = vorige.FckCubeEigenOpgave;
            BetonStaal = new BetonStaalContext(vorige.BetonStaal);
        }



        // 3.2 betonstaal als onderdeel
        /// <summary>
        /// 3.2 Betonstaal is onderdeel van de betoncontext.
        /// </summary>
        public BetonStaalContext BetonStaal = new();

        /// <summary>
        /// Wordt er een parabolisch spannings-rek-diagram toegepast?
        /// Bij <see langword="false"/> wordt een bi-lineair spannings-rek-diagram toegepast.
        /// </summary>
        //public bool IsParabolischSpanningsRekDiagram { get; set; } = !true;


        public SpanningRekDiagramType? SpanningRekDiagram { get; set; } = SpanningRekDiagramType.BiLineair;
        public enum SpanningRekDiagramType { Parabolisch, BiLineair }




        /// <summary>
        /// 3.1.2 (6) Het soort cement.
        /// </summary>
        public CementklasseEnum? CementKlasse { get; set; }

        public double CoefficientCementKlasse
        {
            get
            {
                return CementKlasse switch
                {
                    CementklasseEnum.S => 0.38,// slow
                    CementklasseEnum.N => 0.25,// normal
                    CementklasseEnum.R => 0.20,// rapid
                    _ => 0.20,
                };
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



        public BetonsterkteklasseEnum? Betonsterkteklasse { get; set; } = BetonsterkteklasseEnum.C40_50;

        public string BetonSterkteKlasseGebruiksvriendelijkeNaam
        {
            get
            {
                return $"C{Fck}/{FckCube}";
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
            return this.GetFactorBeta();
        }

        public double GetAlpha()
        {
            return this.GetFactorAlpha();

            //if (IsParabolischSpanningsRekDiagram)
            //    return this.GetFactorAlpha(EpsilonC2, EpsilonCu2);
            //else
            //    return this.GetFactorAlpha(EpsilonC3, EpsilonCu3);

        }

        public double GetEpsilonBreuk() { return this.GetEpsilonBetonBreuk(); }

        public double GetEpsilonStuik() { return this.GetEpsilonBetonStuik(); }

        public void SetBeta(double beta)
        {
            this.Beta = beta;
        }
        public void SetAlpha(double alpha)
        {
            this.Alpha = alpha;
        }


        public double Alpha { get; set; }


        public double XudMax { get { return BetonContextExtensions.GetXudMax(Fcd, EpsilonCu3, BetonStaal.Fyd); } }

        public double Rho1Max { get { return this.GetRho1Max(); } }

        public double Ecm { get { return this.GetEcm(); } }


        public double EpsilonC
        {
            get
            {
                return SpanningRekDiagram switch
                {
                    SpanningRekDiagramType.Parabolisch => EpsilonCu2,
                    SpanningRekDiagramType.BiLineair => EpsilonCu3,
                    _ => EpsilonCu2
                };
            }
        }
        public double SigmaCd { get; set; }
        public double SigmaCk { get; set; }

        public double EpsilonC1 { get { return this.GetEpsilonC1(); } }

        public double EpsilonCu1 { get { return this.GetEpsilonCu1(); } }

        public double EpsilonC2 { get { return this.GetEpsilonC2(); } }
        public double EpsilonCu2 { get { return this.GetEpsilonCu2(); } }

        public double FactorN { get { return this.GetFactorN(); } }

        public double EpsilonC3 { get { return this.GetEpsilonC3(); } }
        public double EpsilonCu3 { get { return this.GetEpsilonCu3(); } }

        public double? EpsilonCuAangepast
        {
            get
            {
                if (BetonStaal.EpsilonS > BetonStaal.EpsilonUd)
                {
                    return this.GetBetonRekAangepast();
                }
                else return null;
            }
        }


        public BetonContext(string sterkteklasse)
        {
            sterkteklasse = sterkteklasse.Replace("/", "_");
            _ = Enum.TryParse(sterkteklasse, out BetonsterkteklasseEnum sterkteklasseEnum);
            Betonsterkteklasse = sterkteklasseEnum;
        }


        public BetonContext(BetonsterkteklasseEnum klasse)
        {
            Betonsterkteklasse = klasse;
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, BetonStaalKwaliteitEnum betonStaalKwaliteit)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            BetonStaal = new BetonStaalContext(betonStaalKwaliteit);
            SpanningRekDiagram = SpanningRekDiagramType.BiLineair;
        }

        public BetonContext(BetonsterkteklasseEnum betonSterkteKlasse, BetonStaalContext betonStaal)
        {
            Betonsterkteklasse = betonSterkteKlasse;
            BetonStaal = betonStaal;
        }
















    }

}
