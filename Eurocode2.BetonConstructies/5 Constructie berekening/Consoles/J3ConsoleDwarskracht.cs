namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using ExportFactory.Shared;

    /// <summary>
    /// Dwarskrachtweerstand zonder dwarskrachtwapening EC2 6.2.2 vgl. (6.2a/6.2b)
    /// voor de J3-console (gedrongen-liggertheorie), met NEd = -HEd (trek).
    /// </summary>
    public class J3ConsoleDwarskrachtResult
    {
        // invoer / tussenwaarden
        public double B { get; set; }
        public double D { get; set; }
        public double Fck { get; set; }
        public double GammaC { get; set; } = 1.5;
        public double AsL { get; set; }
        public double NEd { get; set; }
        public double Ac { get; set; }
        public double VEd { get; set; }

        /// <summary> a<sub>v</sub> – afstand van de last tot de rand van de oplegging [mm]. </summary>
        public double Av { get; set; }

        /// <summary>
        /// β = a<sub>v</sub>/2d volgens 6.2.2 (6): reductie van de belastingsbijdrage bij
        /// een last dicht bij de oplegging (0.25d ≤ a<sub>v</sub> ≤ 2d).
        /// </summary>
        public double Beta { get; set; } = 1.0;
        public Formula BetaFormula => new("(6.2.2 (6))",
            @"\beta = \frac{a_v}{2d} \quad (a_v \geq 0.5d)",
            $@"= \frac{{{Math.Max(Av, 0.5 * D).ToTeX()}}}{{2 \cdot {D.ToTeX()}}} = {Beta:0.00}");

        /// <summary> V<sub>Ed,red</sub> = β·V<sub>Ed</sub> [kN]. </summary>
        public double VEdRed { get; set; }
        public Formula VEdRedFormula => new("(6.2.2 (6))",
            @"V_{Ed,red} = \beta \cdot V_{Ed}",
            $@"= {Beta:0.00} \cdot {VEd.ToTeX()} = {VEdRed:0} \text{{ kN}}");

        /// <summary> C<sub>Rd,c</sub> = 0.18/γ<sub>c</sub>. </summary>
        public double CRdc { get; set; }
        public Formula CRdcFormula => new("(6.2.2 (1))",
            @"C_{Rd,c} = \frac{0.18}{\gamma_c}",
            $@"= \frac{{0.18}}{{{GammaC.ToTeX()}}} = {CRdc:0.00}");

        /// <summary> k = 1 + √(200/d) ≤ 2.0. </summary>
        public double K { get; set; }
        public Formula KFormula => new("(6.2.2 (1))",
            @"k = 1 + \sqrt{\frac{200}{d}} \leq 2.0",
            $@"= 1 + \sqrt{{\frac{{200}}{{{D.ToTeX()}}}}} = {K:0.00}");

        /// <summary> ρ<sub>l</sub> = A<sub>sl</sub>/(b<sub>w</sub>·d) ≤ 0.02. </summary>
        public double RhoL { get; set; }
        public Formula RhoLFormula => new("(6.2.2 (1))",
            @"\rho_l = \frac{A_{sl}}{b_w \cdot d} \leq 0.02",
            $@"= \frac{{{AsL.ToTeX()}}}{{{B.ToTeX()} \cdot {D.ToTeX()}}} = {RhoL:0.0000}");

        /// <summary> k<sub>1</sub> = 0.15. </summary>
        public double K1 { get; set; } = 0.15;
        public Formula K1Formula => new("(6.2.2 (1))",
            @$"k_1 = {K1:0.00}",
            "");

        /// <summary> σ<sub>cp</sub> = N<sub>Ed</sub>/A<sub>c</sub> [N/mm²], met NEd = -HEd (trek negatief). </summary>
        public double SigmaCp { get; set; }
        public Formula SigmaCpFormula => new("(6.2.2 (1))",
            @"\sigma_{cp} = \frac{N_{Ed}}{A_c}",
            $@"= \frac{{{(NEd * 1000.0).ToTeX()}}}{{{Ac.ToTeX()}}} = {SigmaCp:0.00} \text{{ N/mm}}^2");

        /// <summary> v<sub>min</sub> volgens vgl. (6.3N). </summary>
        public double VMin { get; set; }

        /// <summary> z – inwendige hefboomsarm voor de drukdiagonaal (θ = 45°) [mm]. </summary>
        public double Zw { get; set; }
        public double Nu1 { get; set; }
        public double Fcd { get; set; }

        /// <summary> V<sub>Rd,max</sub> volgens vgl. (6.9) met θ = 45° [kN]. </summary>
        public double VRdMax { get; set; }
        public Formula VRdMaxFormula => new("(6.9)",
            @"V_{Rd,max} = \frac{\alpha_{cw} \cdot b_w \cdot z \cdot \nu_1 \cdot f_{cd}}{\cot\theta + \tan\theta}",
            $@"= \frac{{1.0 \cdot {B.ToTeX()} \cdot {Zw.ToTeX()} \cdot {Nu1:0.00} \cdot {Fcd:0.0}}}{{2}} \cdot 10^{{-3}} = {VRdMax:0} \text{{ kN}}");

        public bool VRdMaxOk => VEd <= VRdMax;

        /// <summary> V<sub>Rd,c</sub> volgens vgl. (6.2a) [kN]. </summary>
        public double VRdc { get; set; }
        public Formula VRdcFormula => new("(6.2a)",
            @"V_{Rd,c} = \left[ C_{Rd,c} \cdot k \cdot (100 \rho_l f_{ck})^{1/3} + k_1 \sigma_{cp} \right] b_w d \geq (v_{min} + k_1 \sigma_{cp}) b_w d",
            $@"= \left[ {CRdc:0.00} \cdot {K:0.00} \cdot (100 \cdot {RhoL:0.0000} \cdot {Fck.ToTeX()})^{{1/3}} + {K1.ToTeX()} \cdot {SigmaCp:0.00} \right] \cdot {B.ToTeX()} \cdot {D.ToTeX()} = {(VRdc * 1000.0).ToTeX()} \text{{ N}} = {VRdc:0} \text{{ kN}}");

        public bool IsVoldoende => VEdRed <= VRdc;

        public double Fywd { get; set; }

        /// <summary>
        /// A<sub>sw</sub> – beugelwapening t.b.v. dwarskracht volgens vgl. (6.19):
        /// A<sub>sw</sub>·f<sub>ywd</sub> ≥ β·V<sub>Ed</sub>, aan te brengen in het middelste ¾ deel van a<sub>v</sub> [mm²].
        /// </summary>
        public double AswV { get; set; }
        public Formula AswVFormula => new("(6.19)",
            @"A_{sw} \geq \frac{\beta \cdot V_{Ed}}{f_{ywd}}",
            $@"= \frac{{{Beta:0.00} \cdot {(VEd * 1000.0).ToTeX()}}}{{{Fywd:0}}} = {AswV:0} \text{{ mm}}^2");
    }

    public static class J3ConsoleDwarskrachtCalculator
    {
        public static J3ConsoleDwarskrachtResult Bereken(J3ConsoleInput i, double d, double h, double asProv, double zw, double fcd, double nu)
        {
            var r = new J3ConsoleDwarskrachtResult
            {
                B = i.Bc,
                D = d,
                Fck = i.Fck,
                AsL = asProv,
                NEd = -i.HEd, // trek negatief
                Ac = i.Bc * h,
                VEd = i.FEd,
                Av = i.Ac,
                Zw = zw,
                Fcd = fcd,
                Nu1 = nu,
            };

            r.CRdc = 0.18 / r.GammaC;
            r.K = Math.Min(1.0 + Math.Sqrt(200.0 / d), 2.0);
            r.RhoL = Math.Min(asProv / (i.Bc * d), 0.02);
            r.SigmaCp = r.NEd * 1000.0 / r.Ac; // negatief bij trek
            r.VMin = 0.035 * Math.Pow(r.K, 1.5) * Math.Sqrt(i.Fck);

            // 6.2.2 (6): beta = av/2d met ondergrens av >= 0.5d
            double av = Math.Max(r.Av, 0.5 * d);
            r.Beta = Math.Min(av / (2.0 * d), 1.0);
            r.VEdRed = r.Beta * r.VEd;

            double vRdc = (r.CRdc * r.K * Math.Pow(100.0 * r.RhoL * i.Fck, 1.0 / 3.0) + r.K1 * r.SigmaCp) * i.Bc * d;
            double vRdcMin = (r.VMin + r.K1 * r.SigmaCp) * i.Bc * d;
            r.VRdc = Math.Max(vRdc, vRdcMin) / 1000.0; // [kN]

            // (6.9) met theta = 45 graden: cot + tan = 2
            r.VRdMax = 1.0 * i.Bc * zw * nu * fcd / 2.0 / 1000.0; // [kN]

            // (6.19): Asw * fywd >= beta * VEd (aan te brengen in middelste 0.75 av)
            r.Fywd = i.Fyk * 0.80; // neem 80% van fyk
            r.AswV = r.VEdRed * 1000.0 / r.Fywd;

            return r;
        }
    }
}
