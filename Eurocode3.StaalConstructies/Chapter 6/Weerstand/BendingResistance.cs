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


            double avz = section switch
            {
                CircularHollowSection circular => circular.Avz,
                RectangularHollowSection hollow => hollow.Avz,
                ProfielIH profielIH => profielIH.Avz,
                ProfielC profC => profC.Avz,
                _ => throw new NotSupportedException(
                    $"Dwarskracht om de z-as is niet geïmplementeerd voor profieltype {section.GetType().Name}")
            };
            double vRd = ShearResistance.VRd(avz, steel.Fy, steel.GammaM0);


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

    public class TorsionCheck : BaseEurocodeToets
    {
        public override string Positie { get; set; } = "";
        public override string Titel => "Torsie";
        public override string Norm => "EC3";
        public override string Artikel { get; set; } = "6.2.7";
        public override string Formule { get; set; } = "(6.23)";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.T) > 1e-6;

        public override EurocodeResultaat Check(InternalForces f, IProfiel p, IMateriaal m)
        {
            var steel = m as StaalContext
                ?? throw new InvalidOperationException("Materiaal is geen staal");
            double tRd = p switch
            {
                CircularHollowSection circular => TorsionResistance.TRdClosedSection(
                    Math.PI * Math.Pow(circular.Diameter - circular.T, 2) / 4,
                    circular.T,
                    steel.Fy,
                    steel.GammaM0),
                RectangularHollowSection hollow => TorsionResistance.TRdClosedSection(
                    (hollow.H - hollow.T) * (hollow.B - hollow.T),
                    hollow.T,
                    steel.Fy,
                    steel.GammaM0),
                ProfielIH section => TorsionResistance.TRd(
                    section.It,
                    Math.Max(section.Tf, section.Tw),
                    steel.Fy,
                    steel.GammaM0),
                _ => throw new NotSupportedException(
                    $"Torsie is niet geïmplementeerd voor profieltype {p.GetType().Name}")
            };

            return new EurocodeResultaat
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Artikel = Artikel,
                Formule = Formule,
                Fy = steel.Fy,
                Waarde = Math.Abs(f.T),
                Forces = f,
                Unit = "kNm",
                Toelaatbaar = tRd,
                Toelichting = "Saint-Venant-torsie; eventuele oorlogstorsie is niet beschouwd",
            };
        }
    }

    public class CombinedTorsionAndShearVzCheck : BaseEurocodeToets
    {
        public override string Positie { get; set; } = "";
        public override string Titel => "Torsie en dwarskracht";
        public override string Norm => "EC3";
        public override string Artikel { get; set; } = "6.2.7";
        public override string Formule { get; set; } = "(6.26)";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.T) > 1e-6 && Math.Abs(f.Vz) > 1e-6;

        public override EurocodeResultaat Check(InternalForces f, IProfiel p, IMateriaal m)
        {
            var steel = m as StaalContext
                ?? throw new InvalidOperationException("Materiaal is geen staal");
            double tauEd;
            double vPlTRd;

            switch (p)
            {
                case CircularHollowSection circular:
                    Formule = "(6.28)";
                    tauEd = TorsionResistance.TauEdClosedSection(
                        f.T,
                        Math.PI * Math.Pow(circular.Diameter - circular.T, 2) / 4,
                        circular.T);
                    vPlTRd = TorsionResistance.VPlTRdHollowSection(
                        ShearResistance.VRd(circular.Avz, steel.Fy, steel.GammaM0),
                        tauEd,
                        steel.Fy,
                        steel.GammaM0);
                    break;
                case RectangularHollowSection hollow:
                    Formule = "(6.28)";
                    tauEd = TorsionResistance.TauEdClosedSection(
                        f.T,
                        (hollow.H - hollow.T) * (hollow.B - hollow.T),
                        hollow.T);
                    vPlTRd = TorsionResistance.VPlTRdHollowSection(
                        ShearResistance.VRd(hollow.Avz, steel.Fy, steel.GammaM0),
                        tauEd,
                        steel.Fy,
                        steel.GammaM0);
                    break;
                case ProfielIH section:
                    Formule = "(6.26)";
                    tauEd = TorsionResistance.TauEd(
                        f.T,
                        section.It,
                        Math.Max(section.Tf, section.Tw));
                    vPlTRd = TorsionResistance.VPlTRdIH(
                        ShearResistance.VRd(section.Avz, steel.Fy, steel.GammaM0),
                        tauEd,
                        steel.Fy,
                        steel.GammaM0);
                    break;
                default:
                    throw new NotSupportedException(
                        $"De combinatie van torsie en dwarskracht is niet geïmplementeerd voor profieltype {p.GetType().Name}");
            }

            return new EurocodeResultaat
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Artikel = Artikel,
                Formule = Formule,
                Fy = steel.Fy,
                Waarde = Math.Abs(f.Vz),
                Forces = f,
                Unit = "kN",
                Toelaatbaar = vPlTRd,
                Toelichting = $"Gereduceerde dwarskrachtweerstand bij τt,Ed={tauEd:F2} N/mm²",
            };
        }
    }



}
