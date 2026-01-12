using CommonLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Models
{
    public abstract class BaseEurocodeToets
    {
        public abstract string Titel { get; }
        public abstract string Norm { get; }
        public abstract string Artikel { get; set; }
        public abstract string Formule { get; set; }
        public abstract bool IsRelevant(InternalForces internalForces);
        public abstract EurocodeResultaat Check(InternalForces f, IProfiel p, IMateriaal m);
    }

    public record InternalForces
    (
        double N = 0,
        double My = 0,
        double Mz = 0,
        double Vy = 0,
        double Vz = 0,
        double T = 0
    );


    public class EurocodeResultaat
    {
        public string Titel { get; init; } = "?";
        public string Norm { get; init; } = "?";
        public string Artikel { get; init; } = "";
        public string Formule { get; init; } = "";
       

        public double Waarde { get; init; }
        public string Unit { get; init; } = "kN";
        public double Toelaatbaar { get; init; }
        public double Fy { get; init; }
        public double StaalSpanning => Benutting * Fy;

        public double Benutting => Waarde / Toelaatbaar;
        public bool Voldoet => Benutting <= 1.01;

        public string Toelichting { get; init; } = "";
    }


}
