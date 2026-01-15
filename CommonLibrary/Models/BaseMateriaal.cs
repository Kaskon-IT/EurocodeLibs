using CommonLibrary.Interfaces;
using System.Diagnostics.Contracts;

namespace CommonLibrary.Models
{
    public abstract class BaseMateriaal : BaseEurocodeContext, IMateriaal
    {
        // Parameterloze constructor voor JSON
        public BaseMateriaal() { }

        // Wordt door conrecte materialen ingevuld
        public virtual MateriaalType Type { get; }

        // Gedeelde eigenschappen
        public virtual string Naam { get; set; } = string.Empty;
        public virtual string Omschrijving { get; set; } = string.Empty;
        public virtual string UserFriendlyName { get; } = string.Empty;


        
        public virtual double SoortelijkGewicht { get; set; }
        public abstract string Eurocode { get;  }

        public override bool BerekenEnValideer()
        {
            // Algemene materiaalvalidatie
            base.BerekenEnValideer();
            return true;
        }

        /// <summary>
        /// Partiele factor voor materiaaleigenschappen
        /// Voor beton is dit GammaC
        /// Voor betonstaal GammaS
        /// Voor hout GammaM
        /// NB. voor staal wordt GammaMi (M0, M1 en M2) gebruikt.
        /// </summary>
        public double PartieleFactor { get; set; } = 1.0; // Standaardwaard om delen door nul te voorkomen

        public double GammaM => PartieleFactor; 
        public virtual double GammaM0 { get; set; } = 1.0; 
        public virtual double GammaM1 { get; set; } = 1.0; 
        public virtual double GammaM2 { get; set; } = 1.25;


        public virtual double E { get; set; }
    }

    public enum MateriaalType { 
        Dummy, 
        Beton, 
        Staal, 
        Hout,
        Aluminimium,
        Steen,
    }

}
