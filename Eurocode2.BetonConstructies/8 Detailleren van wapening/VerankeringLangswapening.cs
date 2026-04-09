using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Shared;
using System.ComponentModel;

namespace Eurocode.BetonConstructies
{
    public class VerankeringLangswapeningContext : BaseEurocodeContext, INotifyPropertyChanged
    {
        public override string Heading { get; set; } = "Verankering van langswapening";

        //8.4.1 Algemeen
        //8.4.1 (1)P Wapeningstaven, draden en gepuntlaste wapeningsnetten moeten zo zijn verankerd dat de
        //aanhechtkrachten veilig zijn overgedragen op het beton zodat langsscheuren en spatten worden vermeden.
        //Zonodig moet dwarswapening zijn aangebracht. 
        //8.4.1 (2) Verankeringsmethoden zijn gegeven in figuur 8.1 (zie ook 8.8 (3)).


        // 8.4.2 Uiterst opneembare aanhechtspanning
        // 8.4.2 (2) f~bd~
        // f~bd~ = 2.25 n1 n2 f~ctd~ 


        private bool _goedeAanhechtingOmstandigheden = false;
        private double _diameter = 8.0; // Diameter van de wapening in mm, standaardwaarde is 8.0 mm
        private BetonContext _beton = new BetonContext(); // Context voor beton, kan verder worden uitgebreid
        private double _rekenwaardeStaafspanning = 435; // Rekenwaarde van de treksterkte van de wapening in N/mm², standaardwaarde is 435 N/mm²
        private double _alpha1 = 1.0; // factor vorm van de staaf, standaardwaarde is 1.0
        private double _alpha2 = 1.0; // factor betondekking, standaardwaarde is 1.0
        private double _alpha3 = 1.0; // factor opsluiting, standaardwaarde is 1.0
        private double _alpha4 = 1.0; // factor opsluiting door gelast dwarswapening, standaardwaarde is 1.0
        private double _alpha5 = 1.0; // factor opsluiting door dwarsdruk
        private double _alpha6 = 1.0; // 
        private StaafTypeEnum _staafType = StaafTypeEnum.Trekstaaf; // Type staaf, standaard is Trekstaaf

        public StaafTypeEnum StaafType
        {
            get => _staafType;
            set
            {
                if (_staafType != value)
                {
                    _staafType = value;
                    OnPropertyChanged(nameof(StaafType));
                }
            }
        }

        /// <summary>
        /// l~b,min~
        /// </summary>
        [TableColumn(Label = "minimum verankeringslengte", Symbol = "<i>l</i><sub>b,min</sub>", Unit = "mm")]
        public double MinimumVerankeringsLengte
        {
            get
            {
                if (StaafType == StaafTypeEnum.Trekstaaf)
                {
                    List<double> minimaTrekverankeringen = [
                        0.3 * BasisVerankeringsLengte,
                        10 * Diameter,
                        100
                        ];
                    return minimaTrekverankeringen.Max(); // (8.6) Minimum verankeringslengte voor trekstaven
                }
                else if (StaafType == StaafTypeEnum.Drukstaaf)
                {
                    List<double> minimaDrukverankeringen = [
                        0.6 * BasisVerankeringsLengte,
                        10 * Diameter,
                        100
                        ];
                    return minimaDrukverankeringen.Max(); // (8.7) Minimum verankeringslengte voor drukstaven

                }
                else
                {
                    return 9999; // Onbekend type, return een hoge waarde
                }
            }
        }



        public enum StaafTypeEnum
        {
            Trekstaaf = 1,
            Drukstaaf = 2,
        }

        [TableColumn(Label = "goede aanhechtingsomstandigheden?")]
        public bool GoedeAanhechtingOmstandigheden
        {
            get => _goedeAanhechtingOmstandigheden;
            set
            {
                if (_goedeAanhechtingOmstandigheden != value)
                {
                    _goedeAanhechtingOmstandigheden = value;
                    OnPropertyChanged(nameof(GoedeAanhechtingOmstandigheden));
                    OnPropertyChanged(nameof(FactorN1));
                }
            }
        }

        public double Diameter
        {
            get => _diameter;
            set
            {
                if (_diameter != value)
                {
                    _diameter = value;
                    OnPropertyChanged(nameof(Diameter));
                    OnPropertyChanged(nameof(FactorN2)); // Update FactorN2 if diameter changes
                }
            }
        }

        public BetonContext Beton
        {
            get => _beton;
            set
            {
                if (_beton != value)
                {
                    _beton = value;
                    OnPropertyChanged(nameof(Beton));
                    OnPropertyChanged(nameof(Fctd)); // Update Fctd if beton changes
                }
            }
        }

        public double Fctd
        {
            //            is de rekenwaarde van de treksterkte van het beton volgens 3.1.6 (2)P.Ten gevolge van
            //toenemende brosheid van beton met hogere sterkte behoort fctk,0,05 hierbij te zijn beperkt tot de
            //waarde voor C60/75, tenzij kan zijn getoetst dat de gemiddelde aanhechtsterkte toeneemt boven die
            //grens
            get
            {
                if (Beton.Fck <= 60.0)
                {
                    return Beton.Fctd;
                }
                else
                {
                    BetonContext c60 = new(BetonsterkteklasseEnum.C60_75); // Maak een nieuwe context voor C60/75
                    return c60.Fctd; // Gebruik de Fctd waarde van C60/75
                }
            }
        }

        /// <summary>
        /// f~bd~ (8.2)
        /// </summary>
        [TableColumn(Label = "rekenwaarde opneembare aanhechtspanning", Symbol = "<i>f</i><sub>bd</sub>", Unit = "N/mm²", Article = "8.4.2 (2)")]
        public double Fbd
        {
            get
            {
                return 2.25 * FactorN1 * FactorN2 * Fctd; // (8.2)
            }
        }
        public Formula FbdFormula => new()
        {
            Name = "(8.2)",
            StaticValue = @"f_{bd} = 2.25 \; \eta_1 \; \eta_2 \; f_{ctd}",
            DynamicValue = @$"= 2.25\cdot{FactorN1.ToTeX()}\cdot{FactorN2.ToTeX()}\cdot{Fctd.ToTeX()} = {Fbd.ToTeX()}"
        };

        /// <summary>
        /// l~b,rqd~ (8.3)
        /// </summary>
        /// 
        [TableColumn(Label = "basisverankeringslengte", Symbol = "<i>l</i><sub>b,rqd</sub>", Unit = "mm", Article = "8.4.3 (2)")]
        public double BasisVerankeringsLengte
        {
            get
            {
                // lb,rqd = (φ / 4)(σsd / fbd)(8.3)
                return (Diameter / 4.0) * (RekenwaardeStaafspanning / Fbd); // Diameter in mm, SigmaSd in N/mm², Fbd in N/mm²
            }
        }
        public Formula BasisVerankeringsLengteFormula => new()
        {
            Name = "(8.3)",
            StaticValue = @"l_{b,rqd} = (Ø/4) \;(\sigma_{sd} / f_{bd})",
            DynamicValue = @$"= ({Diameter.ToTeX()} / 4 )\cdot ({RekenwaardeStaafspanning.ToTeX()} / {Fbd.ToTeX()}) = {BasisVerankeringsLengte.ToTeX()}"
        };

        /// <summary>
        /// l~bd~ (8.4)
        /// </summary>
        /// 
        [TableColumn(
            Label = "verankeringslengte",
            Description = "is de rekenwaarde van de verankeringslengte",
            Symbol = "<i>l</i><sub>bd</sub>",
            Unit = "mm",
            Article = "8.4.4 (1)"
            )]
        public double RekenwaardeVerankeringsLengte
        {
            get
            {
                return Math.Max(BasisVerankeringsLengte * _alpha1 * _alpha2 * _alpha3 * _alpha4 * _alpha5, MinimumVerankeringsLengte);
            }
        }
        public Formula RekenwaardeVerankeringsLengteFormula => new()
        {
            Name = "(8.4)",
            StaticValue = @"l_{bd} = \alpha_1 \; \alpha_2 \; \alpha_3 \;\alpha_4 \;\alpha_5 \; l_{b,rqd} \geq l_{b,min}",

        };




        public double FactorN1
        {
            get
            {
                return GoedeAanhechtingOmstandigheden ? 1.0 : 0.7;
            }
        }




        public double FactorN2
        {
            get
            {
                if (Diameter <= 32.0)
                {
                    return 1.0;
                }
                else
                {
                    return (132.0 - Diameter) / 100.0;
                }
            }
        }

        public double RekenwaardeStaafspanning
        {
            get => _rekenwaardeStaafspanning;
            set
            {
                if (_rekenwaardeStaafspanning != value)
                {
                    _rekenwaardeStaafspanning = value;
                    OnPropertyChanged(nameof(RekenwaardeStaafspanning));
                }
            }
        }


        public double Alpha1
        {
            get => _alpha1;
            set
            {
                if (_alpha1 != value)
                {
                    _alpha1 = value;
                    OnPropertyChanged(nameof(Alpha1));
                }
            }
        }
        public double Alpha2
        {
            get => _alpha2;
            set
            {
                if (_alpha2 != value)
                {
                    _alpha2 = value;
                    OnPropertyChanged(nameof(Alpha2));
                }
            }
        }

        public double Alpha3
        {
            get => _alpha3;
            set
            {
                if (_alpha3 != value)
                {
                    _alpha3 = value;
                    OnPropertyChanged(nameof(Alpha3));
                }
            }
        }

        public double Alpha4
        {
            get => _alpha4;
            set
            {
                if (_alpha4 != value)
                {
                    _alpha4 = value;
                    OnPropertyChanged(nameof(Alpha4));
                }
            }
        }

        public double Alpha5
        {
            get => _alpha5; set
            {
                if (_alpha5 != value)
                {
                    _alpha5 = value;
                    OnPropertyChanged(nameof(Alpha5));
                }
            }
        }

        public double Alpha6
        {
            get => _alpha6;
            set
            {
                if (_alpha6 != value)
                {
                    _alpha6 = value;
                    OnPropertyChanged(nameof(Alpha6));
                }
            }
        }







        //public override MarkupString ToHtml(bool isDraaiTabel = true)
        //{
        //    // Implement the logic to convert the context to HTML format
        //    throw new NotImplementedException();
        //}

        protected override void Bereken()
        {
            // Implement the calculation logic for the anchoring of long reinforcement
        }

        protected override bool Valideer()
        {
            return true; // Placeholder, implement actual validation logic
        }

        public override string ToString()
        {
            return $"l~bd~ = {this.RekenwaardeVerankeringsLengte:0} mm, l~b,min~ = {this.MinimumVerankeringsLengte:0} mm, l~b,rqd~ = {this.BasisVerankeringsLengte:0} mm, f~bd~ = {this.Fbd:0.##} MPa, f~ctd~={this.Fctd:0.##} MPa";

            //return base.ToString();
        }
    }
}
