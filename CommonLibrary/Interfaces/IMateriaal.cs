using CommonLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonLibrary.Interfaces
{
    public interface IMateriaal
    {
        MateriaalType Type { get; }
        string Naam { get; }
        string Eurocode { get; }
        double SoortelijkGewicht { get; }
        double E { get; } // Elasticiteitsmodulus
        double G { get; } // Shear modulus

        // Partiele materiaalfactoren
        double GammaM { get; }
        double GammaM0 { get; }
        double GammaM1 { get; }
        double GammaM2 { get; }



    }
}
