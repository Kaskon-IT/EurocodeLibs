using CommonLibrary;
using ExportFactory.Shared;
using Kaskon_it.Algemeen;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Kaskon_it.Algemeen.Geometrie;

namespace ParametrischeProfielen
{
    public class ParametrischProfielContext : BaseEurocodeContext, INotifyPropertyChanged
    {
        public override string Heading { get; set; } = "Profiel";

        public ParametrischProfielContext() { }


        public ParametrischProfielContext(double breedte, double hoogte)
        {
            Heading = "Profiel";
            Breedte = breedte;
            Hoogte = hoogte;

        }

        private ParametrischeProfielVormEnum? _vorm = ParametrischeProfielVormEnum.Rechthoek;
        public ParametrischeProfielVormEnum? Vorm
        {
            get
            {
                return this._vorm;
            }
            set
            {
                if (value != this._vorm)
                {
                    this._vorm = value;
                    NotifyPropertyChanged();
                }
            }
        }



        public int Nr { get; set; }




        private double _breedte = 300;


        [TableColumn(Label = "breedte", Unit = "mm", Symbol = "b")]
        public double Breedte
        {
            get => _breedte;
            set
            {
                if (value != _breedte)
                {
                    _breedte = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private double _hoogte = 400;

        [TableColumn(Label = "hoogte", Unit = "mm", Symbol = "h")]
        public double Hoogte
        {
            get => _hoogte;
            set
            {
                if (value != _hoogte)
                {
                    _hoogte = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private double _b1 = 40;

        public double B1
        {
            get => _b1;
            set
            {
                if (value != _b1)
                {
                    _b1 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _b2 = 40;
        public double B2
        {
            get => _b2;
            set
            {
                if (value != _b2)
                {
                    _b2 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _h1 = 40;
        public double H1
        {
            get => _h1;
            set
            {
                if (value != _h1)
                {
                    _h1 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _h2 = 40;
        public double H2
        {
            get => _h2;
            set
            {
                if (value != _h2)
                {
                    _h2 = value;
                    NotifyPropertyChanged();
                }
            }
        }





        public double BreedteDwarskracht { get { return Breedte; } }

        public PuntD[] Polygon { get { return this.GetPolygon(); } }
        public PuntD Zwaartepunt { get { return ZoekZwaartePunt(Polygon); } }
        public PuntD[] PolygonOmZwPnt { get { return this.GetPolygonRondomZwaartePunt(this.Zwaartepunt); } }

        [TableColumn(Label = "Weerstandsmoment", Unit = "mm³", Symbol = "<i>W</i><sub>y</sub>")]
        public double Wy
        {
            get
            {
                switch (Vorm)
                {
                    default:
                    case ParametrischeProfielVormEnum.Rechthoek:
                        return Breedte * Hoogte * Hoogte / 6.0;
                }
            }
        }

        public double Area
        {
            get
            {
                return Polygon.GetSignedPolygonArea();
            }
        }

        [TableColumn(Label = "Traagheidsmoment", Unit = "mm⁴", Symbol = "<i>I</i><sub>y</sub>")]
        public double Iy
        {
            get
            {
                return Vorm switch
                {
                    _ => Breedte * Math.Pow(Hoogte, 3) / 12.0,
                };
            }
        }



        public override string ToString()
        {
            //return $"{Breedte}×{Hoogte}";

            if (Vorm != null)
            {
                switch (Vorm.Value)
                {
                    default: return $"{Breedte}×{Hoogte}";
                    case ParametrischeProfielVormEnum.Rechthoek:
                        return $"RH{Breedte}×{Hoogte}";
                    case ParametrischeProfielVormEnum.Rond:
                        return $"D{Breedte}";
                    case ParametrischeProfielVormEnum.T1:
                    case ParametrischeProfielVormEnum.T2:
                    case ParametrischeProfielVormEnum.T3:
                    case ParametrischeProfielVormEnum.T4:
                        return $"T{Breedte}/{BreedteDwarskracht}×{Hoogte}";
                    case ParametrischeProfielVormEnum.L1:
                    case ParametrischeProfielVormEnum.L2:
                    case ParametrischeProfielVormEnum.L3:
                    case ParametrischeProfielVormEnum.L4:
                        return $"L{Breedte}/{BreedteDwarskracht}×{Hoogte}";


                }
            }
            else
            {
                return $"{Breedte}×{Hoogte}";
            }


        }

        public string UserFriendlyName
        {
            get
            {
                return this.ToString();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void Bereken()
        {
            // profiel eigenschappen worden direct berekend via properties
        }

        protected override bool Valideer()
        {
            Meldingen.Clear();
            // Voorbeeldvalidatie
            if (Breedte <= 0)
            {
                AddMelding(new Melding(MeldingType.Waarschuwing, "Breedte moet groter zijn dan 0."));
                return false;
            }

            if (Hoogte <= 0)
            {
                AddMelding(new Melding(MeldingType.Waarschuwing, "Hoogte moet groter zijn dan 0."));
                return false;
            }

            if (Vorm == null)
            {
                AddMelding(new Melding(MeldingType.Waarschuwing, "Vorm moet zijn geselecteerd."));
                return false;
            }

            return true;
        }
    }


}
