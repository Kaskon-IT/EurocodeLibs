using CommonLibrary.Interfaces;
using CommonLibrary.Materialen;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonLibrary.Models
{

    public abstract class StaalProfiel : BaseProfiel, IStaalProfiel
    {
        public abstract double WelY { get; }
        public abstract double WelZ { get; }
        public abstract double WplY { get; }
        public abstract double WplZ { get; }
    }

    public abstract class BaseProfiel : IProfiel, INotifyPropertyChanged
    {
        protected BaseProfiel()
        {
            //Materiaal = DummyMateriaal.Instance;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (Equals(field, value)) return false;
            field = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name!));
            return true;
        }

        public virtual string SvgPath { get; set; } = string.Empty;
        public virtual string SvgViewBox { get; set; } = "0 0 100 100";


        private double _b;
        public virtual double B
        {
            get => _b;
            set => SetProperty(ref _b, value);
        }

        private double _h;
        public virtual double H
        {
            get => _h;
            set => SetProperty(ref _h, value);
        }

        //protected BaseProfiel(IMateriaal materiaal)
        //{
        //    //Materiaal = materiaal;
        //}

        /// <summary>
        /// In de basis, heeft een profiel een materiaal. 
        /// </summary>
        /// 
        //[Obsolete("gebruik specieke MatProfiel, zoals BetonProfiel, StaalProfiel. ")]
        //public IMateriaal Materiaal { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public virtual string Naam { get; set; } = string.Empty;
        //public double B { get; set; }
        //public double H { get; set; }

        // virtual properties
        /// <summary>
        /// Doorsnede oppervlak mm²
        /// </summary>
        public virtual double A { get; set; } // Doorsnede oppervlak mm²
        public virtual double Iy { get; set; } // Traagheidsmoment mm⁴
        public virtual double Iz { get; set; } // Traagheidsmoment mm⁴
        public virtual double It { get; set; } // Torsie-inertiemoment mm⁴






        /// <summary>
        /// Traagheidsstraal in sterke richting.
        /// </summary>
        public double TraagheidsStraalIy { get { return Math.Sqrt((Iy / A)); } }

        /// <summary>
        /// Traagheidsstraal in de zwakke richting.
        /// </summary>
        public double TraagheidsStraalIz { get { return Math.Sqrt((Iz / A)); } }

        /// <summary>
        /// Gewicht in kilogram per strekkende meter. (kg/m¹)
        /// </summary>
        //public double KgPerM => A * 1e-6 * (Materiaal?.SoortelijkGewicht ?? 7850);
        




       
    }

}
