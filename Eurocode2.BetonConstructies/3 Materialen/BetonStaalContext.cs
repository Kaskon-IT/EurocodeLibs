using CommonLibrary;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{

    /// <summary>
    /// 3.2 Betonstaal
    /// Verzameling gegevens voor betonstaal
    /// </summary>
    public class BetonStaalContext : BaseEurocodeContext
    {
        public BetonStaalContext()
        {

        }
        public override string ToString()
        {
            return $"{BetonStaalKwaliteit}";
        }


        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vorige"></param>
        public BetonStaalContext(BetonStaalContext vorige)
        {
            BetonStaalKwaliteit = vorige.BetonStaalKwaliteit;
            IsOntwerpSituatieBuitenGewoon = vorige.IsOntwerpSituatieBuitenGewoon;
            SpanningRekDiagram = vorige.SpanningRekDiagram;
            EpsilonS = vorige.EpsilonS;
        }

        public BetonStaalContext(BetonStaalKwaliteitEnum betonStaalKwaliteit = BetonStaalKwaliteitEnum.B500A)
        {
            BetonStaalKwaliteit = betonStaalKwaliteit;
        }

        public BetonStaalContext(BetonStaalKwaliteitEnum betonStaalKwaliteit, SpanningRekDiagramType spanningRekDiagram)
        {
            BetonStaalKwaliteit = betonStaalKwaliteit;
            SpanningRekDiagram = spanningRekDiagram;
        }

        #endregion



        /// <summary>
        /// Voor gebruik hellende (stijgende) tak in het spannings-rek-diagram.
        /// Gebruik <see langword="true"/> voor hellende tak.
        /// Gebruik false voor horizontale tak.
        /// </summary>
        //public bool IsHellendeTakDiagram { get; set; }

        [TableColumn("Diagram", Order = 2)]
        public SpanningRekDiagramType? SpanningRekDiagram { get; set; } = SpanningRekDiagramType.HorizontaleTak;
        public enum SpanningRekDiagramType { HellendeTak, HorizontaleTak }


        /// <summary>
        /// Eigen opgave fyk (indien gekozen is voor betonstaalkwaliteit 'eigen opgave')
        /// </summary>
        public double EigenFyk { get; set; }

        /// <summary>
        /// De betonstaalkwaliteit volgens tabel C.1 
        /// </summary>
        [TableColumn("Betonstaalkwaliteit", Order = 0)]
        public BetonStaalKwaliteitEnum? BetonStaalKwaliteit { get; set; }


        /// <summary>
        /// karakteristieke vloeigrens van betonstaal 
        /// </summary>
        [TableColumn("f~yk~", StringFormat = "0 N/mm²")]
        public double Fyk
        {
            get
            {
                return BetonStaalKwaliteit switch
                {
                    BetonStaalKwaliteitEnum.B500A or BetonStaalKwaliteitEnum.B500B or BetonStaalKwaliteitEnum.B500C => 500,
                    BetonStaalKwaliteitEnum.B400A or BetonStaalKwaliteitEnum.B400B or BetonStaalKwaliteitEnum.B400C => 400,
                    BetonStaalKwaliteitEnum.B600A or BetonStaalKwaliteitEnum.B600B or BetonStaalKwaliteitEnum.B600C => 600,
                    BetonStaalKwaliteitEnum.EigenFyk => EigenFyk,
                    _ => 500,
                };
            }
        }

        public double Fywk { get { return Fyk; } }




        public bool IsOntwerpSituatieBuitenGewoon = false;  // default Blijvend en tijdelijk conform art. 2.4.2.4 (1) Partiële factoren voor materialen 

        /// <summary>
        /// conform art. 2.4.2.4 Partiële factoren voor materialen
        /// </summary>
        [TableColumn(Weergave = WeergaveEnum.DraaiTabel)]
        public double GammaS
        {
            get
            {
                if (!IsOntwerpSituatieBuitenGewoon) return 1.15;
                else return 1.0;
            }
        }


        /// <summary>
        /// 3.2.7 (2) rekenwaarde van de vloeigrens van betonstaal
        /// </summary>
        [TableColumn(Weergave = WeergaveEnum.DraaiTabel)]
        public double Fyd { get { return Fyk / GammaS; } }  // 3.2.7 (2)


        /// <summary>
        /// 6.2 rekenwaarde van de vloeigrens van dwarskrachtwapening
        /// </summary>
        [TableColumn(Weergave = WeergaveEnum.DraaiTabel)]
        public double Fywd { get { return Fywk / GammaS; } }	// conform art. 6.2



        public double EpsilonS { get; set; }

        public double SigmaSd { get; set; }
        public double SigmaSk { get; set; }


        /// <summary>
        /// karakteristieke rek van betonstaal of voorspanstaal bij maximale belasting
        /// </summary>
        [TableColumn("|epsilon|~uk~", Order = 2, StringFormat = "0.00 ‰")]
        public double EpsilonUk
        {
            get
            {
                switch (BetonStaalKwaliteit)
                {
                    case BetonStaalKwaliteitEnum.B400A:
                    case BetonStaalKwaliteitEnum.B500A:
                    case BetonStaalKwaliteitEnum.B600A:
                        return 25.0 / 1000;

                    case BetonStaalKwaliteitEnum.B400B:
                    case BetonStaalKwaliteitEnum.B500B:
                    case BetonStaalKwaliteitEnum.B600B:
                        return 50.0 / 1000;

                    case BetonStaalKwaliteitEnum.B400C:
                    case BetonStaalKwaliteitEnum.B500C:
                    case BetonStaalKwaliteitEnum.B600C:
                        return 75.0 / 1000;

                    default: return 25.0 / 1000;
                }
            }
        }

        public double EpsilonUd
        {
            get
            {
                return 0.9 * EpsilonUk;
            }
        }



        public double FactorKmin
        {
            get
            {
                if (SpanningRekDiagram == SpanningRekDiagramType.HellendeTak)
                {
                    return BetonStaalKwaliteit switch
                    {
                        BetonStaalKwaliteitEnum.B400A or BetonStaalKwaliteitEnum.B500A or BetonStaalKwaliteitEnum.B600A => 1.05,
                        BetonStaalKwaliteitEnum.B400B or BetonStaalKwaliteitEnum.B500B or BetonStaalKwaliteitEnum.B600B => 1.08,
                        BetonStaalKwaliteitEnum.B400C or BetonStaalKwaliteitEnum.B500C or BetonStaalKwaliteitEnum.B600C => 1.15,
                        _ => 1.0,
                    };
                }
                else return 1.0;        // factor k stellen we op 1 indien we niet met hellende tak rekenen.
            }
        }



        public double Kfyk
        {
            get
            {
                return FactorKmin * Fyk;
            }
        }

        public double Kfyd
        {
            get
            {
                return Kfyk / GammaS;
            }
        }



        /// <summary>
        /// 3.2.7 (3) Voor de gemiddelde waarde van de volumieke massa mag 7850 kg/m³ zijn aangehouden.
        /// </summary>
        public const double VolumiekeMassa = 7850;



        /// <summary>
        /// 3.2.7 (4) Voor de rekenwaarde van de elasticiteitsmodulus Es mag 200 GPa zijn aangenomen.
        /// </summary>

        public const double Es = 200000;

        [TableColumn("E~s~", StringFormat = "0 N/mm²")]
        public double ElasticiteitsModulus
        {
            get
            {
                return Es;
            }
        }


        /// <summary>
        /// Fyd / Es is de rek op het punt waarop de vloeigrens bereiks wordt;
        /// </summary>
        public double EpsilonYd { get { return Fyd / Es; } }


        public double GetFactorFyd()
        {

            if (this.SpanningRekDiagram != SpanningRekDiagramType.HellendeTak && this.EpsilonS >= this.EpsilonYd) return 1; // geen hellende tak boven de vloeigrens
            if (this.SpanningRekDiagram != SpanningRekDiagramType.HorizontaleTak && this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt

            if (this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt

            if (this.EpsilonS > this.EpsilonUd) this.EpsilonS = this.EpsilonUd; // maximale rek bereikt
            if (this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt
            double fydMaalFactor =
                this.Fyd + ((this.EpsilonS - this.EpsilonYd) / (this.EpsilonUk - this.EpsilonYd)) * (this.Kfyd - this.Fyd);
            return fydMaalFactor / this.Fyd;
        }



        protected override void Bereken()
        {
            // niets te berekenen
        }

        protected override bool Valideer()
        {
            if (BetonStaalKwaliteit == null)
            {
                Meldingen.Add(new Melding(MeldingType.Error, "Betonstaalkwaliteit is niet opgegeven"));
                return false;
            }

            return true;
        }
    }
}

