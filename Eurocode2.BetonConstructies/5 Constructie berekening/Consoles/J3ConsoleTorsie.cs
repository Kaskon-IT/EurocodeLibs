namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using ExportFactory.Shared;

    /// <summary>
    /// Torsietoets EC2 6.3.2 voor de J3-console (gedrongen-liggertheorie).
    /// TEd = FEd * e (excentriciteit in de breedterichting), TRd,c op basis van
    /// de equivalente dunwandige doorsnede, plus combinatietoets vgl. (6.31).
    /// </summary>
    public class J3ConsoleTorsieResult
    {
        // invoer / tussenwaarden
        public double B { get; set; }
        public double H { get; set; }
        public double Fctd { get; set; }
        public double Excentriciteit { get; set; }

        /// <summary> T<sub>Ed</sub> = F<sub>Ed</sub> · e [kNm]. </summary>
        public double TEd { get; set; }
        public Formula TEdFormula => new("",
            @"T_{Ed} = F_{Ed} \cdot e",
            $@"= {(TEd * 1.0e6 / Math.Max(Excentriciteit, 1e-9)).ToTeX()} \cdot {Excentriciteit.ToTeX()} \cdot 10^{{-6}} = {TEd:0.00} \text{{ kNm}}");

        /// <summary> t<sub>ef</sub> = A/u – effectieve wanddikte [mm], met ondergrens 2c + 2Øbgl + Ølangs. </summary>
        public double TEf { get; set; }

        /// <summary> Ondergrens t<sub>ef,min</sub> = 2c + 2Ø<sub>bgl</sub> + Ø<sub>langs</sub> [mm]. </summary>
        public double TEfMin { get; set; }
        public double Dekking { get; set; }
        public double DiameterBgl { get; set; }
        public double DiameterLangs { get; set; }
        public bool TEfMinMaatgevend => TEfMin > TEfBerekend;

        /// <summary> t<sub>ef</sub> = A/u vóór toepassing van de ondergrens [mm]. </summary>
        public double TEfBerekend { get; set; }

        public Formula TEfFormula => new("(6.3.2 (1))",
            @"t_{ef} = \max\left(\frac{A}{u};\ 2c + 2\text{Ø}_{bgl} + \text{Ø}_{langs}\right)",
            $@"= \max\left(\frac{{{B.ToTeX()} \cdot {H.ToTeX()}}}{{2 \cdot ({B.ToTeX()} + {H.ToTeX()})}};\ 2 \cdot {Dekking.ToTeX()} + 2 \cdot {DiameterBgl.ToTeX()} + {DiameterLangs.ToTeX()}\right) = \max\left({TEfBerekend:0};\ {TEfMin:0}\right) = {TEf:0} \text{{ mm}}");

        /// <summary> A<sub>k</sub> – oppervlak binnen de hartlijnen van de wanden [mm²]. </summary>
        public double Ak { get; set; }
        public Formula AkFormula => new("(6.3.2 (1))",
            @"A_k = (b - t_{ef})(h - t_{ef})",
            $@"= ({B.ToTeX()} - {TEf:0})({H.ToTeX()} - {TEf:0}) = {Ak:0} \text{{ mm}}^2");

        /// <summary> T<sub>Rd,c</sub> = 2·A<sub>k</sub>·t<sub>ef</sub>·f<sub>ctd</sub> [kNm]. </summary>
        public double TRdc { get; set; }
        public Formula TRdcFormula => new("(6.3.2 (5))",
            @"T_{Rd,c} = 2 \cdot A_k \cdot t_{ef} \cdot f_{ctd}",
            $@"= 2 \cdot {Ak:0} \cdot {TEf:0} \cdot {Fctd:0.00} \cdot 10^{{-6}} = {TRdc:0.00} \text{{ kNm}}");

        public bool IsVoldoende => TEd <= TRdc;

        public double Nu { get; set; }
        public double Fcd { get; set; }

        /// <summary> T<sub>Rd,max</sub> volgens vgl. (6.30) met θ = 45° [kNm]. </summary>
        public double TRdMax { get; set; }
        public Formula TRdMaxFormula => new("(6.30)",
            @"T_{Rd,max} = 2 \cdot \nu \cdot \alpha_{cw} \cdot f_{cd} \cdot A_k \cdot t_{ef} \cdot \sin\theta \cos\theta \quad (\theta = 45^\circ)",
            $@"= 2 \cdot {Nu:0.00} \cdot 1.0 \cdot {Fcd:0.0} \cdot {Ak:0} \cdot {TEf:0} \cdot 0.5 \cdot 10^{{-6}} = {TRdMax:0.0} \text{{ kNm}}");

        public double Fywd { get; set; }

        /// <summary>
        /// Beugelwapening t.b.v. wringing per zijde per lengte-eenheid [mm²/mm]:
        /// A<sub>sw,T</sub>/s = T<sub>Ed</sub> / (2·A<sub>k</sub>·f<sub>ywd</sub>·cotθ) met θ = 45°.
        /// </summary>
        public double AswTPerLengte { get; set; }
        public Formula AswTFormula => new("(6.3.2)",
            @"\frac{A_{sw,T}}{s} = \frac{T_{Ed}}{2 \cdot A_k \cdot f_{ywd} \cdot \cot\theta} \quad (\theta = 45^\circ)",
            $@"= \frac{{{(TEd * 1.0e6).ToTeX()}}}{{2 \cdot {Ak:0} \cdot {Fywd:0} \cdot 1.0}} = {AswTPerLengte:0.000} \text{{ mm}}^2/\text{{mm}}");
    }

    /// <summary>
    /// Combinatietoets dwarskracht + torsie EC2 6.3.2 (5) vgl. (6.31):
    /// TEd/TRd,c + VEd/VRd,c ≤ 1.0 → geen aanvullende wapening vereist.
    /// </summary>
    public class J3ConsoleTorsieDwarskrachtCombinatie
    {
        public double TEd { get; set; }
        public double TRdc { get; set; }
        public double VEd { get; set; }
        public double VRdc { get; set; }

        public double UnityCheck => (TRdc > 0 && VRdc > 0) ? TEd / TRdc + VEd / VRdc : double.NaN;
        public Formula UnityCheckFormula => new("(6.31)",
            @"\frac{T_{Ed}}{T_{Rd,c}} + \frac{V_{Ed}}{V_{Rd,c}} \leq 1.0",
            $@"= \frac{{{TEd:0.00}}}{{{TRdc:0.00}}} + \frac{{{VEd:0}}}{{{VRdc:0}}} = {UnityCheck:0.00}");

        public bool IsVoldoende => UnityCheck <= 1.0;

        /// <summary> V<sub>Ed</sub> zonder reductie [kN], voor de drukdiagonaaltoets (6.29). </summary>
        public double VEdVol { get; set; }
        public double TRdMax { get; set; }
        public double VRdMax { get; set; }

        /// <summary> Controle drukdiagonaal vgl. (6.29). </summary>
        public double UnityCheckMax => (TRdMax > 0 && VRdMax > 0) ? TEd / TRdMax + VEdVol / VRdMax : double.NaN;
        public Formula UnityCheckMaxFormula => new("(6.29)",
            @"\frac{T_{Ed}}{T_{Rd,max}} + \frac{V_{Ed}}{V_{Rd,max}} \leq 1.0",
            $@"= \frac{{{TEd:0.00}}}{{{TRdMax:0.0}}} + \frac{{{VEdVol:0}}}{{{VRdMax:0}}} = {UnityCheckMax:0.00}");

        public bool IsDrukdiagonaalVoldoende => UnityCheckMax <= 1.0;

        /// <summary> a<sub>v</sub> – lengte van de zone waarin de beugels worden aangebracht [mm]. </summary>
        public double AvZone { get; set; }

        /// <summary> Beugelwapening t.b.v. dwarskracht (6.19) [mm²]. </summary>
        public double AswV { get; set; }

        /// <summary> Beugelwapening t.b.v. wringing per zijde in zone a<sub>v</sub> [mm²]. </summary>
        public double AswT { get; set; }
        public Formula AswTZoneFormula => new("",
            @"A_{sw,T} = \frac{A_{sw,T}}{s} \cdot s",
            $@"= {AswTPerLengte:0.000} \cdot {AvZone:0} = {AswT:0} \text{{ mm}}^2");
        public double AswTPerLengte { get; set; }

        /// <summary> Totale beugelwapening (dwarskracht + wringing) in zone a<sub>v</sub> [mm²]. </summary>
        public double AswTotaal => AswV + 2 * AswT;
        public Formula AswTotaalFormula => new("",
            @"A_{sw,tot} = A_{sw,V} + 2 \cdot A_{sw,T}",
            $@"= {AswV:0} + 2 \cdot {AswT:0} = {AswTotaal:0} \text{{ mm}}^2");
    }

    public static class J3ConsoleTorsieCalculator
    {
        public static J3ConsoleTorsieResult Bereken(J3ConsoleInput i, BetonContext beton, double fcd, double nu)
        {
            var r = new J3ConsoleTorsieResult
            {
                B = i.Bc,
                H = i.Hc,
                Fctd = beton.Fctd,
                Excentriciteit = i.ExcentriciteitBreedte,
                TEd = i.TEd,
                Dekking = i.Dekking,
                DiameterBgl = i.BeugelDiameter,
                DiameterLangs = i.HoofdstaafDiameter,
                Fcd = fcd,
                Nu = nu,
            };

            r.TEfBerekend = (i.Bc * i.Hc) / (2.0 * (i.Bc + i.Hc));
            r.TEfMin = 2.0 * i.Dekking + 2.0 * i.BeugelDiameter + i.HoofdstaafDiameter;
            r.TEf = Math.Max(r.TEfBerekend, r.TEfMin);
            r.Ak = (i.Bc - r.TEf) * (i.Hc - r.TEf);
            r.TRdc = 2.0 * r.Ak * r.TEf * beton.Fctd / 1.0e6; // [kNm]

            // (6.30) met theta = 45 graden: sin*cos = 0.5
            r.TRdMax = 2.0 * nu * 1.0 * fcd * r.Ak * r.TEf * 0.5 / 1.0e6; // [kNm]

            // Beugelwapening wringing per zijde: Asw,T/s = TEd / (2*Ak*fywd*cot(45)) 
            r.Fywd = i.Fyk * 0.80; // neem 80% van fyk
            r.AswTPerLengte = r.TEd * 1.0e6 / (2.0 * r.Ak * r.Fywd);

            return r;
        }

        public static J3ConsoleTorsieDwarskrachtCombinatie Combineer(J3ConsoleTorsieResult torsie, J3ConsoleDwarskrachtResult dwarskracht)
            => new()
            {
                TEd = torsie.TEd,
                TRdc = torsie.TRdc,
                VEd = dwarskracht.VEdRed,
                VRdc = dwarskracht.VRdc,
                VEdVol = dwarskracht.VEd,
                TRdMax = torsie.TRdMax,
                VRdMax = dwarskracht.VRdMax,
                AvZone = dwarskracht.Av,
                AswV = dwarskracht.AswV,
                AswTPerLengte = torsie.AswTPerLengte,
                AswT = torsie.AswTPerLengte * dwarskracht.Av,
            };
    }
}
