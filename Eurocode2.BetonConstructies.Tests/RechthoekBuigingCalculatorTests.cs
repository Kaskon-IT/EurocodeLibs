using Eurocode.BetonConstructies;

namespace Eurocode2.BetonConstructies.Tests
{
    /// <summary>
    /// Handberekeningen voor buiging van een rechthoekige doorsnede (6.1).
    /// C30/37 (f_cd = 20), B500 (f_yd = 434,78), parabool-rechthoek: ε_c2 = 2 ‰, ε_cu2 = 3,5 ‰, n = 2.
    /// Met a = ε_c2/ε_cu2 = 4/7: α = 1 − a/3 = 17/21 = 0,80952; β = 1 − (5a²/12 + (1 − a²)/2)/α = 99/238 = 0,41597.
    /// b × h / d = 300 × 500 / 450 → k = α·b·f_cd = 4857,1 N/mm; x_u,max = 3500/(3500 + 7·434,78)·450 = 240,70 mm.
    /// </summary>
    public class RechthoekBuigingCalculatorTests
    {
        private static BetonContext C30() => new(BetonsterkteklasseEnum.C30_37);

        private static RechthoekBuigingResult Bereken(double mEd, double d2 = 50, double as1 = 0, double as2 = 0, BetonContext? beton = null) =>
            RechthoekBuigingCalculator.Bereken(new RechthoekBuigingInput
            {
                Beton = beton ?? C30(),
                Breedte = 300,
                Hoogte = 500,
                NuttigeHoogte = 450,
                AfstandDrukwapening = d2,
                MEd = mEd,
                AsTrekToegepast = as1,
                AsDrukToegepast = as2,
            });

        [Fact]
        public void StandaardDiagram_IsParaboolRechthoek()
        {
            var beton = C30();
            Assert.Equal(BetonContext.SpanningRekDiagramType.Parabolisch, beton.SpanningRekDiagram);
            Assert.Equal(BetonContext.SpanningRekDiagramType.Parabolisch,
                new BetonContext(BetonsterkteklasseEnum.C30_37, BetonStaalKwaliteitEnum.B500B).SpanningRekDiagram);

            var r = Bereken(150);
            Assert.Equal(17.0 / 21, r.Alpha, 4);
            Assert.Equal(99.0 / 238, r.Beta, 4);
        }

        [Fact]
        public void KopieBehoudtSpanningRekDiagram()
        {
            var beton = C30();
            beton.SpanningRekDiagram = BetonContext.SpanningRekDiagramType.BiLineair;
            Assert.Equal(BetonContext.SpanningRekDiagramType.BiLineair, new BetonContext(beton).SpanningRekDiagram);
        }

        [Fact]
        public void BiLineair_AlphaBeta()
        {
            // ε_c3 = 1,75 ‰, ε_cu3 = 3,5 ‰: α = 1 − ½·½ = 0,75 ; β = 7/18
            var beton = C30();
            beton.SpanningRekDiagram = BetonContext.SpanningRekDiagramType.BiLineair;
            var r = Bereken(150, beton: beton);
            Assert.Equal(0.75, r.Alpha, 4);
            Assert.Equal(7.0 / 18, r.Beta, 4);
        }

        [Fact]
        public void Ontwerp_ZonderDrukwapening()
        {
            // M_Ed = 150 kNm: x_u = (450 − √(450² − 4·0,41597·150e6/4857,1)) / (2·0,41597) = 73,64 mm
            // A_s = 4857,1·73,64/434,78 = 822,7 mm² ; z = 450 − 0,41597·73,64 = 419,37 mm
            var r = Bereken(150);
            Assert.False(r.IsDrukwapeningNodig);
            Assert.Equal(73.64, r.Xu, 2);
            Assert.Equal(240.70, r.XuMax, 2);
            Assert.Equal(419.37, r.Z, 2);
            Assert.Equal(822.7, r.AsBerekend, 1);
            Assert.Equal(822.7, r.AsTrekBenodigd, 1);
            Assert.Equal(0, r.AsDrukBenodigd);
            Assert.False(r.IsMinimaleWapeningMaatgevend);
        }

        [Fact]
        public void Ontwerp_MinimaleWapening()
        {
            // f_ctm = 0,3·30^(2/3) = 2,896 ; M_cr = 2,896·300·500²/6 = 36,21 kNm → x = 16,83 → A_s,min1 = 188,0 mm²
            // M_Ed = 20 kNm: x_u = 9,23 → A_s,ber = 103,1 ; A_s,min = min(188,0 ; 1,25·103,1 = 128,9) = 128,9
            var r = Bereken(20);
            Assert.Equal(188.0, r.AsMin1, 1);
            Assert.Equal(103.1, r.AsBerekend, 1);
            Assert.Equal(128.9, r.AsMin, 1);
            Assert.Equal(128.9, r.AsTrekBenodigd, 1);
            Assert.True(r.IsMinimaleWapeningMaatgevend);
        }

        [Fact]
        public void Ontwerp_MetDrukwapening_Vloeiend()
        {
            // M_lim = 4857,1·240,70·(450 − 0,41597·240,70) = 409,04 kNm
            // d₂ = 50: ε_s2 = 3,5‰·(240,70 − 50)/240,70 = 2,773 ‰ > ε_yd = 2,174 ‰ → σ_s2 = f_yd
            // A_s2 = (450 − 409,04)e6 / (434,78·400) = 235,5 mm² ; A_s1 = 4857,1·240,70/434,78 + 235,5 = 2924,4 mm²
            var r = Bereken(450, d2: 50);
            Assert.True(r.IsDrukwapeningNodig);
            Assert.Equal(409.04, r.MRdMaxZonderDrukwapening, 2);
            Assert.Equal(0.002773, r.EpsilonS2, 6);
            Assert.Equal(434.78, r.SigmaS2, 2);
            Assert.Equal(235.5, r.AsDrukBenodigd, 1);
            Assert.Equal(2924.4, r.AsTrekBenodigd, 1);
        }

        [Fact]
        public void Ontwerp_MetDrukwapening_NietVloeiend()
        {
            // d₂ = 100: ε_s2 = 3,5‰·140,70/240,70 = 2,046 ‰ < ε_yd → σ_s2 = 200000·0,002046 = 409,18 N/mm²
            // A_s2 = 40,96e6 / (409,18·350) = 286,0 mm² ; A_s1 = 2689,0 + 286,0·409,18/434,78 = 2958,1 mm²
            var r = Bereken(450, d2: 100);
            Assert.Equal(409.18, r.SigmaS2, 2);
            Assert.Equal(286.0, r.AsDrukBenodigd, 1);
            Assert.Equal(2958.1, r.AsTrekBenodigd, 1);
            Assert.Contains(r.Meldingen, m => m.Contains("vloeit niet"));
        }

        [Fact]
        public void MRd_AlleenTrekwapening()
        {
            // 3Ø20 = 942,5 mm²: x = 942,5·434,78/4857,1 = 84,37 mm (ε_s = 15,2 ‰, vloeit)
            // M_Rd = 942,5·434,78·(450 − 0,41597·84,37) = 170,02 kNm
            var r = Bereken(150, as1: 3 * Math.PI * 100);
            Assert.Equal(84.37, r.XMRd, 2);
            Assert.Equal(434.78, r.SigmaS1MRd, 2);
            Assert.Equal(170.02, r.MRd, 2);
            Assert.Equal(150 / 170.02, r.UnityCheck, 3);
        }

        [Fact]
        public void MRd_MetDrukwapening_Vloeiend()
        {
            // 4Ø25 = 1963,5 ; 2Ø16 = 402,1 op d₂ = 50
            // beide vloeien: x = (1963,5 − 402,1)·434,78/4857,1 = 139,76 mm ; ε_s2 = 3,5‰·89,76/139,76 = 2,248 ‰ > 2,174 ‰
            // M_Rd = 4857,1·139,76·(450 − 0,41597·139,76) + 402,1·434,78·400 = 335,95 kNm
            var r = Bereken(300, d2: 50, as1: Math.PI * 625, as2: Math.PI * 128);
            Assert.Equal(139.76, r.XMRd, 2);
            Assert.Equal(434.78, r.SigmaS2MRd, 2);
            Assert.Equal(335.95, r.MRd, 2);
        }

        [Fact]
        public void MRd_MetDrukwapening_NietVloeiend()
        {
            // d₂ = 100: evenwicht 4857,1·x + 402,1·200000·0,0035·(x − 100)/x = 1963,5·434,78
            // → x = 155,16 mm ; σ_s2 = 700·55,16/155,16 = 248,85 N/mm²
            // M_Rd = 4857,1·155,16·(450 − 0,41597·155,16) + 402,1·248,85·350 = 325,52 kNm
            var r = Bereken(300, d2: 100, as1: Math.PI * 625, as2: Math.PI * 128);
            Assert.Equal(155.16, r.XMRd, 2);
            Assert.Equal(248.85, r.SigmaS2MRd, 2);
            Assert.Equal(325.52, r.MRd, 2);
        }

        [Fact]
        public void BendingResults_MRdUitEvenwicht()
        {
            // zelfde als MRd_AlleenTrekwapening, via BendingResults (3Ø20, d = 450)
            var bending = new BendingResults(
                C30(),
                new Profielen.Beton.BetonProfiel { Breedte = 300, Hoogte = 500 },
                new WapeningContext("3r20", 40), // dekking op de staaf; d = 500 − 40 − 20/2 = 450
                new Eurocode.Belastingen.SectionForces(my: 150));

            Assert.Equal(450, bending.D, 6);
            Assert.Equal(170.02, bending.MRd, 2);
        }
    }
}
