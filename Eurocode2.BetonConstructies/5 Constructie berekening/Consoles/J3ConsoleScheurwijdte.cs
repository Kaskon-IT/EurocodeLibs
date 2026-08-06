namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using ExportFactory.Shared;

    /// <summary>
    /// Scheurwijdtetoetsing EC2 7.3.4 (vgl. 7.8/7.9/7.11/7.12) voor de J3-console
    /// (gedrongen-liggertheorie). BGT-belastingen via FactorBgt.
    /// </summary>
    public class J3ConsoleScheurwijdteResult
    {
        // invoer / tussenwaarden
        public double B { get; set; }
        public double H { get; set; }
        public double D { get; set; }
        public double Z { get; set; }
        public double AsProv { get; set; }
        public double Dekking { get; set; }
        public double FctEff { get; set; }
        public double Es { get; set; } = 200_000;
        public double Ecm { get; set; }
        public double FactorKt { get; set; } = 0.4; // langdurend
        public double K1 { get; set; } = 0.8;
        public double K2 { get; set; } = 0.5; // buiging
        public double K3 { get; set; } = 3.4;
        public double K4 { get; set; } = 0.425;

        /// <summary> M in BGT [kN·mm] (incl. bijdrage horizontale kracht). </summary>
        public double MBgt { get; set; }

        /// <summary> Ø<sub>eq</sub> volgens vgl. (7.12) [mm]. </summary>
        public double DiameterEq { get; set; }
        public Formula DiameterEqFormula => new("(7.12)",
            @"\text{Ø}_{eq} = \frac{n_1 \text{Ø}_1^2 + n_2 \text{Ø}_2^2}{n_1 \text{Ø}_1 + n_2 \text{Ø}_2}",
            $@"= {DiameterEq.ToTeX()} \text{{ mm}}");

        /// <summary> σ<sub>s</sub> – staalspanning in BGT [N/mm²]. </summary>
        public double SigmaS { get; set; }
        public Formula SigmaSFormula => new("",
            @"\sigma_s = \frac{M_{BGT}}{z \cdot A_{s,prov}}",
            $@"= \frac{{{(MBgt * 1000.0).ToTeX()}}}{{{Z.ToTeX()} \cdot {AsProv.ToTeX()}}} = {SigmaS.ToTeX()} \text{{ N/mm}}^2");

        public double AlphaE => Es / Ecm;

        /// <summary> x – hoogte betondrukzone in BGT (gescheurde doorsnede) [mm]. </summary>
        public double X { get; set; }

        /// <summary> h<sub>c,eff</sub> – hoogte effectieve trekzone [mm]. </summary>
        public double HcEff { get; set; }
        public Formula HcEffFormula => new("(7.3.4 (3))",
            @"h_{c,eff} = \min\left(2.5(h-d);\ \frac{h-x}{3};\ \frac{h}{2}\right)",
            $@"= \min\left(2.5 \cdot ({H.ToTeX()} - {D.ToTeX()});\ \frac{{{H.ToTeX()} - {X.ToTeX()}}}{{3}};\ \frac{{{H.ToTeX()}}}{{2}}\right) = {HcEff.ToTeX()} \text{{ mm}}");

        public double AcEff => B * HcEff;
        public double RhoPEff => AsProv / AcEff;

        /// <summary> s<sub>r,max</sub> volgens vgl. (7.11) [mm]. </summary>
        public double SrMax { get; set; }
        public Formula SrMaxFormula => new("(7.11)",
            @"s_{r,max} = k_3 c + k_1 k_2 k_4 \text{Ø}_{eq} / \rho_{p,eff}",
            $@"= {K3.ToTeX()} \cdot {Dekking.ToTeX()} + {K1.ToTeX()} \cdot {K2.ToTeX()} \cdot {K4.ToTeX()} \cdot {DiameterEq.ToTeX()} / {RhoPEff.ToTeX()} = {SrMax.ToTeX()} \text{{ mm}}");

        /// <summary> (ε<sub>sm</sub> − ε<sub>cm</sub>) volgens vgl. (7.9). </summary>
        public double EpsSmMinusEpsCm { get; set; }
        public Formula EpsSmMinusEpsCmFormula => new("(7.9)",
            @"\epsilon_{sm} - \epsilon_{cm} = \frac{\sigma_s - k_t \frac{f_{ct,eff}}{\rho_{p,eff}} (1 + \alpha_e \rho_{p,eff})}{E_s} \geq 0.6 \frac{\sigma_s}{E_s}",
            $@"= {EpsSmMinusEpsCm:0.00000}");

        /// <summary> w<sub>k</sub> volgens vgl. (7.8) [mm]. </summary>
        public double Wk { get; set; }
        public Formula WkFormula => new("(7.8)",
            @"w_k = s_{r,max} \cdot (\epsilon_{sm} - \epsilon_{cm})",
            $@"= {SrMax.ToTeX()} \cdot {EpsSmMinusEpsCm:0.00000} = {Wk:0.00} \text{{ mm}}");

        /// <summary> Grenswaarde scheurwijdte [mm]. </summary>
        public double WMax { get; set; } = 0.3;
        public bool IsVoldoende => Wk <= WMax;
    }

    public static class J3ConsoleScheurwijdteCalculator
    {
        public static J3ConsoleScheurwijdteResult Bereken(J3ConsoleInput i, double a, double z, double d, double asProv, BetonContext beton)
        {
            var r = new J3ConsoleScheurwijdteResult
            {
                B = i.Bc,
                H = i.Hc,
                D = d,
                Z = z,
                AsProv = asProv,
                Dekking = i.Dekking,
                FctEff = beton.Fctm,
                Ecm = beton.Ecm,
            };

            // M in BGT (zelfde momentdefinitie als UGT, geschaald met FactorBgt)
            r.MBgt = i.FactorBgt * (a * i.FEd + (z + i.Hc - d) * i.HEd);

            // sigma_s in BGT
            r.SigmaS = r.MBgt * 1000.0 / (z * asProv);

            // equivalente diameter (7.12)
            double n1 = i.HoofdstaafAantal, o1 = i.HoofdstaafDiameter;
            double n2 = i.HoofdstaafAantal2, o2 = i.HoofdstaafDiameter2;
            r.DiameterEq = (n2 > 0 && o2 > 0)
                ? (n1 * o1 * o1 + n2 * o2 * o2) / (n1 * o1 + n2 * o2)
                : o1;

            // hoogte drukzone x (gescheurd, elastisch)
            double rho = asProv / (i.Bc * d);
            double alphaRho = r.AlphaE * rho;
            r.X = d * (-alphaRho + Math.Sqrt(alphaRho * alphaRho + 2.0 * alphaRho));

            // effectieve trekzone
            r.HcEff = new[] { 2.5 * (i.Hc - d), (i.Hc - r.X) / 3.0, i.Hc / 2.0 }.Min();

            // sr,max (7.11) met bovengrens
            double srMax = r.K3 * i.Dekking + r.K1 * r.K2 * r.K4 * r.DiameterEq / r.RhoPEff;
            double bovengrens = Math.Max((50.0 - 0.8 * i.Fck) * r.DiameterEq, 15.0 * r.DiameterEq);
            r.SrMax = Math.Min(srMax, bovengrens);

            // (7.9) met ondergrens
            double eps = (r.SigmaS - r.FactorKt * (r.FctEff / r.RhoPEff) * (1.0 + r.AlphaE * r.RhoPEff)) / r.Es;
            r.EpsSmMinusEpsCm = Math.Max(eps, 0.6 * r.SigmaS / r.Es);

            r.Wk = r.SrMax * r.EpsSmMinusEpsCm;

            return r;
        }
    }
}
