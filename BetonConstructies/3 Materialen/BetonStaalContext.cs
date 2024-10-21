namespace Eurocode.BetonConstructies
{
    public class BetonStaalContext
    {
        // copyConstructor
        public BetonStaalContext(BetonStaalContext vorige)
        {
            BetonStaalKwaliteit = vorige.BetonStaalKwaliteit;
            IsOntwerpSituatieBuitenGewoon = vorige.IsOntwerpSituatieBuitenGewoon;
            IsHellendeTakDiagram = vorige.IsHellendeTakDiagram;
            EpsilonS = vorige.EpsilonS;

        }

        public BetonStaalContext(BetonStaalKwaliteitEnum betonStaalKwaliteit = BetonStaalKwaliteitEnum.B500A)
        {
            BetonStaalKwaliteit = betonStaalKwaliteit;

        }

        public BetonStaalContext(BetonStaalKwaliteitEnum betonStaalKwaliteit, bool isHellendeTakDiagram)
        {
            BetonStaalKwaliteit = betonStaalKwaliteit;
            IsHellendeTakDiagram = isHellendeTakDiagram;
        }




        // context voor betonstaal conform art. 3.2
        public enum BetonStaalKwaliteitEnum
        {
            B500A = 1,
            B500B = 2,
            B500C = 3,
            B400A = 11,
            B400B = 12,
            B400C = 13,
            B600A = 21,
            B600B = 22,
            B600C = 23,
            EigenFyk = 31,
        }

        public bool IsHellendeTakDiagram { get; set; }
        public double EigenFyk { get; set; }


        public BetonStaalKwaliteitEnum BetonStaalKwaliteit { get; set; }


        /// <summary>
        /// karakteristieke vloeigrens van betonstaal 
        /// </summary>
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





        public bool IsOntwerpSituatieBuitenGewoon = false;  // default Blijvend en tijdelijk conform art. 2.4.2.4 (1) Partiële factoren voor materialen 

        /// <summary>
        /// conform art. 2.4.2.4 Partiële factoren voor materialen
        /// </summary>
        public double GammaS
        {
            get
            {
                if (!IsOntwerpSituatieBuitenGewoon) return 1.15;
                else return 1.0;
            }
        }


        /// <summary>
        /// rekenwaarde van de vloeigrens van betonstaal
        /// </summary>
        public double Fyd { get { return Fyk / GammaS; } }  // 3.2.7 (2)


        /// <summary>
        /// rekenwaarde van de vloeigrens van dwarskrachtwapening
        /// </summary>
        public double Fywd { get { return Fyk / GammaS; } }	// conform art. 6.2

        public double EpsilonS { get; set; }

        public double SigmaSd { get; set; }
        public double SigmaSk { get; set; }


        /// <summary>
        /// karakteristieke rek van betonstaal of voorspanstaal bij maximale belasting
        /// </summary>
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
                if (IsHellendeTakDiagram)
                {
                    switch (BetonStaalKwaliteit)
                    {
                        case BetonStaalKwaliteitEnum.B400A:
                        case BetonStaalKwaliteitEnum.B500A:
                        case BetonStaalKwaliteitEnum.B600A:
                            return 1.05;

                        case BetonStaalKwaliteitEnum.B400B:
                        case BetonStaalKwaliteitEnum.B500B:
                        case BetonStaalKwaliteitEnum.B600B:
                            return 1.08;

                        case BetonStaalKwaliteitEnum.B400C:
                        case BetonStaalKwaliteitEnum.B500C:
                        case BetonStaalKwaliteitEnum.B600C:
                            return 1.15;

                        default: return 25 / 1000;
                    }
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




        public const double VolumiekeMassa = 7850; // kg/m³ // 3.2.7 (3) Voor de gemiddelde waarde van de volumieke massa mag 7850 kg/m³ zijn aangehouden.

        public const double Es = 200000; // N/mm²	// 3.2.7 (4) Voor de rekenwaarde van de elasticiteitsmodulus Es mag 200 GPa zijn aangenomen.

        /// <summary>
        /// Fyd / Es is de rek op het punt waarop de vloeigrens bereiks wordt;
        /// </summary>
		public double EpsilonYd { get { return Fyd / Es; } }


        public double GetFactorFyd()
        {

            if (!this.IsHellendeTakDiagram && this.EpsilonS >= this.EpsilonYd) return 1; // geen hellende tak boven de vloeigrens
            if (!this.IsHellendeTakDiagram && this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt

            if (this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt

            if (this.EpsilonS > this.EpsilonUd) this.EpsilonS = this.EpsilonUd; // maximale rek bereikt
            if (this.EpsilonS < this.EpsilonYd) return (this.EpsilonS / this.EpsilonYd);    // vloeigrens nog niet bereikt
            double fydMaalFactor =
                this.Fyd + ((this.EpsilonS - this.EpsilonYd) / (this.EpsilonUk - this.EpsilonYd)) * (this.Kfyd - this.Fyd);
            return fydMaalFactor / this.Fyd;
        }

    }
}

