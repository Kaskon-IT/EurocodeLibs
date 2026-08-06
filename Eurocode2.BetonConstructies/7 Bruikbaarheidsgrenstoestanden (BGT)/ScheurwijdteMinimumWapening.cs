namespace Eurocode.BetonConstructies
{
    using CommonLibrary.Extensions;
    using ExportFactory.Shared;

    public partial class Scheurbeheersing
    {
        /// <summary>
        /// 7.3.2 Oppervlakte van de minimumwapening
        /// </summary>
        public class ScheurwijdteMinimumWapening
        {
            public ScheurwijdteMinimumWapening()
            {

            }


            public double AsMin
            {
                get
                {
                    return this.FactorKc * this.FactorK * this.FctEff * this.Act / this.SigmaS;
                }
            }

            public Formula AsMinFormula => new("(7.1)",
                @"A_{s,min} = k_c \cdot k \cdot f_{ct,eff} \cdot A_{ct} / \sigma_s",
                $@"= {FactorKc.ToTeX()} \cdot {FactorK.ToTeX()} \cdot {FctEff.ToTeX()} \cdot {Act.ToTeX()} / {SigmaS.ToTeX()} = {AsMin.ToTeX()} \text{{ mm}}^2");

            // ---- Geometrie en normaalkracht t.b.v. hcr / Act ----

            /// <summary> Breedte van de doorsnede [mm]. </summary>
            public double B { get; set; }

            /// <summary> Hoogte van de doorsnede [mm]. </summary>
            public double H { get; set; }

            /// <summary> Normaalkracht [kN], trek positief. </summary>
            public double N { get; set; }

            /// <summary> σ<sub>N</sub> = N/(b·h) [N/mm²]. </summary>
            public double SigmaN => (B > 0 && H > 0) ? N * 1000.0 / (B * H) : 0;

            /// <summary> Spanning in de meest getrokken vezel net voor scheurvorming: σ<sub>boven</sub> = f<sub>ct,eff</sub>. </summary>
            public double SigmaBoven => FctEff;

            /// <summary> σ<sub>onder</sub> = 2·σ<sub>N</sub> − f<sub>ct,eff</sub> (lineair spanningsverloop). </summary>
            public double SigmaOnder => 2.0 * SigmaN - FctEff;

            /// <summary> h<sub>cr</sub> – hoogte van de trekzone net voor scheurvorming [mm]. </summary>
            public double Hcr => SigmaBoven - SigmaOnder > 0
                ? Math.Min(SigmaBoven / (SigmaBoven - SigmaOnder) * H, H)
                : H;

            public Formula HcrFormula => new("",
                @"h_{cr} = \frac{\sigma_{boven}}{\sigma_{boven} - \sigma_{onder}} \cdot h",
                $@"= \frac{{{SigmaBoven.ToTeX()}}}{{{SigmaBoven.ToTeX()} - {SigmaOnder.ToTeX()}}} \cdot {H.ToTeX()} = {Hcr.ToTeX()} \text{{ mm}}");

            public Formula ActFormula => new("",
                @"A_{ct} = b \cdot h_{cr}",
                $@"= {B.ToTeX()} \cdot {Hcr.ToTeX()} = {(B * Hcr).ToTeX()} \text{{ mm}}^2");

            public double Act { get; set; }

            private double? _sigmaS;
            /// <summary>
            /// σs – maximale toelaatbare spanning in de wapening direct na scheurvorming.
            /// Expliciet instelbaar (bijv. fyk); zonder waarde wordt teruggevallen op
            /// de staalspanning uit de betoncontext.
            /// </summary>
            public double SigmaS
            {
                get => _sigmaS ?? Beton.BetonStaal.SigmaSd;
                set => _sigmaS = value;
            }
            public double FctEff { get; set; }
            public double FactorK { get; set; }
            public double FactorKc { get; set; }
            public required BetonContext Beton { get; set; }


            public void Update()
            {

            }



            public double GetFactorK(double lijfHoogteOfFlensBreedte)

            {
                // stap1: als h kleiner of gelijk is aan 300, dan geldt k = 1,0 
                // stap2: als h groter of gelijk is dan 800 dan geldt k = 0,65
                // stap3: hiertussen kunnen we lekker interpoleren. (rechtlijnige grafiek dus!!)
                // Methode met if, if else, else (ondergrens, bovengrens, interpolatie)
                if (lijfHoogteOfFlensBreedte <= 300)
                {
                    return 1.0;
                }
                else if (lijfHoogteOfFlensBreedte >= 800)
                {
                    return 0.65;
                }
                else
                {
                    return (lijfHoogteOfFlensBreedte - 300) / 500 * 0.35 + 0.65;
                }
            }   // factor k in berekening scheurwijdte

            public double GetFctEff(BetonContext beton)
            {
                if (beton.Tijdstip < 28)
                {
                    throw new NotImplementedException("tijdstip kleiner dan 28 dagen niet geimplementeerd.");
                }
                else
                {
                    return beton.Fctm;
                }
            }



        }
    }
}
