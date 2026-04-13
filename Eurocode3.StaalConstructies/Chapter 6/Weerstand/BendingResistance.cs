using CommonLibrary.Interfaces;
using CommonLibrary.Models;
using Microsoft.AspNetCore.Builder;
using Profielen.Staal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Eurocode.StaalConstructies
{
    public static class BendingResistance
    {
        /// <summary>
        /// Geeft MRd met opgave van W, fy en GammaM
        /// </summary>
        /// <param name="W">W in mm³</param>
        /// <param name="fy">fy in N/mm²</param>
        /// <param name="gammaM">partiele materiaalfactor</param>
        /// <returns>M_Rd in kNm</returns>
        public static double MRd(double W, double fy, double gammaM)
        {
            return W * fy / gammaM * 1e-6; // van Nmm naar kNm
        }

       
    }


    public static class ShearResistance
    {
        public static double VRd(double Av, double fy, double gammaM)
        {
            return Av * fy / (Math.Sqrt(3) * gammaM) * 1e-3; // N naar kN
        }
    }

    public class ShearVzCheck : BaseEurocodeToets
    {
        public override string Positie { get; set; } = "";
        public override string Titel => "Dwarskracht (afschuiving)";
        public override string Norm => "EC3";
        public override string Artikel { get; set; } = "6.2.6";
        public override string Formule { get; set; } = "(6.17)";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.Vz) > 1e-6;

        public override EurocodeResultaat Check(InternalForces f, IProfiel p, IMateriaal m)
        {
            double vRd = 0;

            // Controleer of het een staalsectie is
            //var steel = m as StaalContext ?? throw new NotSupportedException("Materiaal is geen staal");
            if (m is not StaalContext steel)
            {
                throw new InvalidOperationException(
                    $"Verwacht StaalContext maar kreeg {m?.GetType().FullName ?? "<null>"}"
                );
            }

            if (p is not IStaalProfiel section)
            {
                throw new InvalidOperationException(
                  $"Verwacht staalprofiel maar kreeg {p?.GetType().FullName ?? "<null>"}"
              );
            }
            //var section = p as IStaalProfiel ?? throw new NotSupportedException("Profiel is geen staalprofiel");


            if (section is ProfielIH profielIH)
            {
                vRd = ShearResistance.VRd(profielIH.Avz, steel.Fy, steel.GammaM0);
                Formule = "(6.17)";
            }


            return new EurocodeResultaat()
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Fy = steel.Fy,
                Waarde = Math.Abs(f.Vz),
                Forces = f,
                Unit = "kN",
                Toelaatbaar = vRd,
                Artikel = Artikel,
                Formule = Formule,
                
            };

            

        }


    }


    public class BendingMyToets : BaseEurocodeToets
    {

        private readonly int _sectionClass; // 1..4

        public BendingMyToets(int sectionClass)
        {
            _sectionClass = sectionClass;
        }

        public override string Positie { get; set; } = "";
        public override string Titel => "Buiging om y-as";
        public override string Norm => "EC3";
        public override string Formule { get; set; } = "(6.12)";
        public override string Artikel { get; set; } = "6.2.5";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.My) > 1e-6;

        // ✅ Hier is de volledige Check() implementatie
        public override EurocodeResultaat Check(
            InternalForces f,
            IProfiel profile,
            IMateriaal material)
        {


            // Controleer of het een staalsectie is
            var steel = material as StaalContext ?? throw new Exception("Materiaal is geen staal");
            var section = profile as IStaalProfiel ?? throw new Exception("Profiel is geen staalprofiel");

            // Kies juiste weerstand op basis van doorsnedeklasse
            double MRd = _sectionClass switch
            {
                1 or 2 => BendingResistance.MRd(section.WplY, steel.Fy, steel.GammaM0),
                3 => BendingResistance.MRd(section.WelY, steel.Fy, steel.GammaM0),
                4 => throw new NotSupportedException(
                              "Klasse 4 vereist effectieve doorsnede"),
                _ => throw new InvalidOperationException("Ongeldige section class")
            };

            // Resultaat object maken
            return new EurocodeResultaat
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Artikel = Artikel,
                Formule = Formule,
                Fy = steel.Fy,
                Waarde = Math.Abs(f.My),
                Forces = f,
                Unit = "kNm",
                Toelaatbaar = MRd,
                Toelichting = $"Zuivere buiging om y-as, SectionClass={_sectionClass}",
                
            };
        }

        
    }



}
