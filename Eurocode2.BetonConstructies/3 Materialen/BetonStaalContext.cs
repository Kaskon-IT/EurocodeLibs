using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Services;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies
{

    /// <summary>
    /// 3.2 Betonstaal
    /// Verzameling gegevens voor betonstaal
    /// </summary>
    public class BetonStaalContext : BaseEurocodeContext
    {
        public override string Heading { get; set; } = "Betonstaal";

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



        [TableColumn(Label = "spanning-rekrelatie", Article = "3.2.3")]
        public SpanningRekDiagramType? SpanningRekDiagram
        {
            get => _spanningRekDiagram;
            set => SetProperty(ref _spanningRekDiagram, value);
        }
        public enum SpanningRekDiagramType { HellendeTak, HorizontaleTak }
        private SpanningRekDiagramType? _spanningRekDiagram = SpanningRekDiagramType.HorizontaleTak;



        //3.2.2 (3)P De toepassingsregels voor ontwerp en berekening en detaillering in deze Eurocode zijn geldig voor een
        //bereik van de gespecificeerde vloeigrens fyk = 400 Mpa tot en met 600 MPa.

        private BetonStaalKwaliteitEnum? _betonStaalKwaliteit = BetonStaalKwaliteitEnum.B500A;

        /// <summary>
        /// De betonstaalkwaliteit volgens tabel C.1 
        /// </summary>
        [TableColumn(Label = "Betonstaalkwaliteit", Article = "3.2", Order = 0)]
        public BetonStaalKwaliteitEnum? BetonStaalKwaliteit
        {
            get => _betonStaalKwaliteit;
            set => SetProperty(ref _betonStaalKwaliteit, value);
        }


        /// <summary>
        /// karakteristieke vloeigrens van betonstaal 
        /// </summary>
        [TableColumn(Label = "vloeigrens betonstaal", Description = "is de karakteristieke vloeigrens van betonstaal", Symbol = "<i>f</i><sub>yk</sub>", Unit = "N/mm²", Article = "3.2.3")]
        public double Fyk
        {
            get
            {
                return BetonStaalKwaliteit switch
                {
                    BetonStaalKwaliteitEnum.B500A or BetonStaalKwaliteitEnum.B500B or BetonStaalKwaliteitEnum.B500C => 500,
                    BetonStaalKwaliteitEnum.B400A or BetonStaalKwaliteitEnum.B400B or BetonStaalKwaliteitEnum.B400C => 400,
                    BetonStaalKwaliteitEnum.B600A or BetonStaalKwaliteitEnum.B600B or BetonStaalKwaliteitEnum.B600C => 600,
                    _ => 500,
                };
            }
        }

        public double Fywk { get { return Fyk; } }




        public bool IsOntwerpSituatieBuitenGewoon = false;  // default Blijvend en tijdelijk conform art. 2.4.2.4 (1) Partiële factoren voor materialen 

        /// <summary>
        /// conform art. 2.4.2.4 Partiële factoren voor materialen
        /// </summary>
        [TableColumn(
            Article = "2.4.2.4",
            Label = "Partiële factor voor betonstaal",
            Symbol = $"<i>{GreekLetters.gamma}</i><sub>S</sub>")]
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
        [TableColumn(Article = "3.2.7 (2)",
            Label = "vloeigrens", Description = "is de rekenwaarde van de vloeigrens van betonstaal",
            Symbol = "<i>f</i><sub>yd</sub>", Unit = "N/mm²"
            )]
        public double Fyd { get { return Fyk / GammaS; } }  // 3.2.7 (2)
        public Formula FydFormula => new("", @"f_{yd} = f_{yk} / \gamma_s", $"={Fyk.ToTeX()}/{GammaS.ToTeX()} = {Fyd.ToTeX()}");


        /// <summary>
        /// 6.2 rekenwaarde van de vloeigrens van dwarskrachtwapening
        /// </summary>
        public double Fywd { get { return Fywk / GammaS; } }	// conform art. 6.2


        public double EpsilonS { get; set; }

        public double SigmaSd { get; set; }
        public double SigmaSk { get; set; }


        /// <summary>
        /// karakteristieke rek van betonstaal of voorspanstaal bij maximale belasting
        /// </summary>
        [TableColumn(Symbol = $"<i>{GreekLetters.epsilon}</i><sub>uk</sub>", Label = "staalrek",
            Description = "is de karakteristieke rek van betonstaal bij maximale belasting",
            StringFormat = "0.00 ‰")]
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

        [TableColumn(Symbol = "<i>E</i><sub>s</sub>", Unit = "N/mm²")]
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


        public double GetFactorFyd(double staalrek)
        {

            if (this.SpanningRekDiagram != SpanningRekDiagramType.HellendeTak && staalrek >= this.EpsilonYd) return 1; // geen hellende tak boven de vloeigrens

            if (staalrek < this.EpsilonYd)
                return (staalrek / this.EpsilonYd);    // vloeigrens nog niet bereikt


            if (staalrek > this.EpsilonUd) staalrek = this.EpsilonUd; // maximale rek bereikt

            double fydMaalFactor =
                this.Fyd + ((staalrek - this.EpsilonYd) / (this.EpsilonUk - this.EpsilonYd)) * (this.Kfyd - this.Fyd);

            return fydMaalFactor / this.Fyd;
        }

        public double GetFydByStaalrek(double staalrek) => GetFactorFyd(staalrek) * this.Fyd;



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

