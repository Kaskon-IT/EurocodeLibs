using CommonLibrary.Interfaces;
using CommonLibrary.Models;
using System;

namespace Eurocode.HoutConstructies
{
    /// <summary>
    /// Hulpklasse voor weerstandsberekeningen volgens EN 1995-1-1
    /// </summary>
    public static class BendingResistance
    {
        /// <summary>
        /// Berekent de buigweerstand voor hout (EN 1995-1-1, §6.1.6)
        /// </summary>
        /// <param name="W">Weerstandsmoment in mm³</param>
        /// <param name="fmd">Rekenwaarde buigsterkte in N/mm²</param>
        /// <returns>M_Rd in kNm</returns>
        public static double MRd(double W, double fmd)
        {
            // M_Rd = W * f_m,d
            return W * fmd * 1e-6; // van Nmm naar kNm
        }
    }

    /// <summary>
    /// Toets voor dwarskracht in houten constructies volgens EN 1995-1-1 §6.1.7
    /// </summary>
    public class ShearVzCheck : BaseEurocodeToets
    {
        private readonly BelastingsduurKlasse _belastingsduur;

        public ShearVzCheck(BelastingsduurKlasse belastingsduur = BelastingsduurKlasse.Middellang)
        {
            _belastingsduur = belastingsduur;
        }

        public override string Positie { get; set; } = "";
        public override string Titel => "Dwarskracht (afschuiving)";
        public override string Norm => "EN 1995-1-1";
        public override string Artikel { get; set; } = "6.1.7";
        public override string Formule { get; set; } = "(6.13)";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.Vz) > 1e-6;

        public override EurocodeResultaat Check(InternalForces f, IProfiel p, IMateriaal m)
        {
            // Controleer of het hout is
            if (m is not HoutContext hout)
            {
                throw new InvalidOperationException(
                    $"Verwacht HoutContext maar kreeg {m?.GetType().FullName ?? "<null>"}"
                );
            }

            // Voor hout gebruiken we BaseProfiel properties (B en H)
            if (p is not BaseProfiel profiel)
            {
                throw new InvalidOperationException(
                    $"Verwacht BaseProfiel maar kreeg {p?.GetType().FullName ?? "<null>"}"
                );
            }

            // Bereken rekenwaarde dwarskrachtsterkte
            double fvd = hout.GetFvd(_belastingsduur);

            // Bereken dwarskrachtweerstand (gebruik effectieve oppervlakte voor rechthoekige doorsnede)
            // Voor rechthoekige doorsneden: A_ef = b * h (vereenvoudigd, zonder kcr factor)
            double Aef = profiel.B * profiel.H; // mm²
            double vRd = Aef * fvd * 1e-3; // van N naar kN

            return new EurocodeResultaat()
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Fy = fvd,
                Waarde = Math.Abs(f.Vz),
                Forces = f,
                Unit = "kN",
                Toelaatbaar = vRd,
                Artikel = Artikel,
                Formule = Formule,
                Toelichting = $"Belastingsduur: {_belastingsduur}, f_v,d = {fvd:F2} N/mm²"
            };
        }
    }

    /// <summary>
    /// Toets voor buiging om y-as in houten constructies volgens EN 1995-1-1 §6.1.6
    /// </summary>
    public class BendingMyToets : BaseEurocodeToets
    {
        private readonly BelastingsduurKlasse _belastingsduur;

        public BendingMyToets(BelastingsduurKlasse belastingsduur = BelastingsduurKlasse.Middellang)
        {
            _belastingsduur = belastingsduur;
        }

        public override string Positie { get; set; } = "";
        public override string Titel => "Buiging om y-as";
        public override string Norm => "EN 1995-1-1";
        public override string Formule { get; set; } = "(6.11)";
        public override string Artikel { get; set; } = "6.1.6";

        public override bool IsRelevant(InternalForces f)
            => Math.Abs(f.My) > 1e-6;

        public override EurocodeResultaat Check(
            InternalForces f,
            IProfiel profile,
            IMateriaal material)
        {
            // Controleer of het hout is
            var hout = material as HoutContext ?? throw new Exception("Materiaal is geen hout");
            var profiel = profile as BaseProfiel ?? throw new Exception("Profiel is geen basisprofiel");

            // Bereken rekenwaarde buigsterkte
            double fmd = hout.GetFmd(_belastingsduur);

            // Bereken weerstandsmoment: W = I / (h/2) = 2*I/h
            // Voor rechthoekige doorsnede: Iy = b*h³/12, dus Wy = b*h²/6
            double Wy = profiel.B * profiel.H * profiel.H / 6.0;  // in mm³

            // Bereken buigweerstand
            double MRd = BendingResistance.MRd(Wy, fmd);

            // Resultaat object maken
            return new EurocodeResultaat
            {
                Positie = Positie,
                Titel = Titel,
                Norm = Norm,
                Artikel = Artikel,
                Formule = Formule,
                Fy = fmd,
                Waarde = Math.Abs(f.My),
                Forces = f,
                Unit = "kNm",
                Toelaatbaar = MRd,
                Toelichting = $"Buiging om y-as, belastingsduur: {_belastingsduur}, f_m,d = {fmd:F2} N/mm²"
            };
        }
    }
}
