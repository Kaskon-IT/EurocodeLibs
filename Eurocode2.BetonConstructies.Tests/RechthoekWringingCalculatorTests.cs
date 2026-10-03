using Eurocode.BetonConstructies;

namespace Eurocode2.BetonConstructies.Tests
{
    /// <summary>
    /// Handberekeningen voor wringing van een massieve rechthoekige balk (6.3.2).
    /// C30/37: f_cd = 20, f_ctd = 0,7·2,896/1,5 = 1,352, ν = 0,6·(1 − 30/250) = 0,528; B500: f_yd = 434,78.
    /// b × h = 300 × 500, c = 30 rondom, beugel Ø8, langs Ø12 boven/zijkant en Ø20 onder, d = 452, cot θ = 2,5.
    /// A/u = 150000/1600 = 93,75 ; t_ef,onder = max(93,75 ; 2·(30 + 8 + 10)) = 96, overige 93,75.
    /// z_b = 300 − 93,75 = 206,25 ; z_h = 500 − (93,75 + 96)/2 = 405,125 → A_k = 83557 mm², u_k = 1222,75 mm.
    /// </summary>
    public class RechthoekWringingCalculatorTests
    {
        private static RechthoekWringingResult Bereken(double tEd, double vEd, double vRdc = 80) =>
            RechthoekWringingCalculator.Bereken(new RechthoekWringingInput
            {
                Beton = new BetonContext(BetonsterkteklasseEnum.C30_37),
                Breedte = 300,
                Hoogte = 500,
                DekkingBoven = 30,
                DekkingOnder = 30,
                DekkingZijkant = 30,
                BeugelDiameter = 8,
                DiameterBoven = 12,
                DiameterOnder = 20,
                DiameterZijkant = 12,
                NuttigeHoogte = 452,
                TEd = tEd,
                VEd = vEd,
                VRdc = vRdc,
                CotTheta = 2.5,
                AswVPerMeter = 300,
                AswMinPerMeter = 262.9, // 0,08·√30/500·300·1000
            });

        [Fact]
        public void DunwandigeDoorsnede()
        {
            var r = Bereken(20, 100);
            Assert.Equal(93.75, r.TefBerekend, 6);
            Assert.Equal(93.75, r.TefBoven, 6);
            Assert.Equal(96, r.TefOnder, 6);
            Assert.Equal(93.75, r.TefZijkant, 6);
            Assert.Equal(206.25, r.ZBreedte, 6);
            Assert.Equal(405.125, r.ZHoogte, 6);
            Assert.Equal(83557.03, r.Ak, 2);
            Assert.Equal(1222.75, r.Uk, 6);
        }

        [Fact]
        public void Weerstanden()
        {
            // T_Rd,c = 2·83557·93,75·1,352 = 21,18 kNm
            // T_Rd,max = 2·0,528·20·83557·93,75·(2,5/7,25) = 57,05 kNm
            // V_Rd,max = 300·0,9·452·0,528·20/(2,5 + 0,4) = 444,39 kN
            var r = Bereken(20, 100);
            Assert.Equal(21.18, r.TRdc, 2);
            Assert.Equal(57.05, r.TRdMax, 2);
            Assert.Equal(444.39, r.VRdMax, 2);
        }

        [Fact]
        public void WringwapeningNodig()
        {
            // (6.31) = 20/21,18 + 100/80 = 2,19 > 1 → wringwapening
            // (6.29) = 20/57,05 + 100/444,39 = 0,58
            // A_sw,T/s = 20e6/(2·83557·434,78·2,5)·1000 = 110,1 mm²/m per wand
            // ΣA_sl = 20e6·1222,75·2,5/(2·83557·434,78) = 841,4 mm² ; boven = onder = 206,25/1222,75·841,4 = 141,9 ; zijkant = 278,8
            // beugels: max(300 + 2·110,1 ; 262,9) = 520,2 mm²/m ; per snede 260,1 mm²/m
            var r = Bereken(20, 100);
            Assert.Equal(2.194, r.UnityCheck631, 3);
            Assert.Equal(0.576, r.UnityCheck629, 3);
            Assert.False(r.IsAlleenMinimaleWapening);
            Assert.Equal(110.1, r.AswTPerMeter, 1);
            Assert.Equal(841.4, r.AslTotaal, 1);
            Assert.Equal(141.9, r.AslBovenBenodigd, 1);
            Assert.Equal(141.9, r.AslOnderBenodigd, 1);
            Assert.Equal(278.8, r.AslZijkantBenodigd, 1);
            Assert.Equal(520.2, r.AswTotaalPerMeter, 1);
            Assert.Equal(260.1, r.AswPerSnedePerMeter, 1);
            Assert.Equal(200, r.BeugelAfstandMax, 6); // min(1600/8 ; 300)
        }

        [Fact]
        public void AlleenMinimaleWapening()
        {
            // (6.31) = 2/21,18 + 20/80 = 0,344 ≤ 1 → geen wringwapening; beugels max(300 ; 262,9) = 300, per snede 150
            var r = Bereken(2, 20);
            Assert.Equal(0.344, r.UnityCheck631, 3);
            Assert.True(r.IsAlleenMinimaleWapening);
            Assert.Equal(0, r.AswTBenodigdPerMeter);
            Assert.Equal(0, r.AslBovenBenodigd);
            Assert.Equal(300, r.AswTotaalPerMeter, 6);
            Assert.Equal(150, r.AswPerSnedePerMeter, 6);
        }

        [Fact]
        public void ZonderVRdc_RekentAltijdWapening()
        {
            var r = Bereken(2, 20, vRdc: 0);
            Assert.True(double.IsNaN(r.UnityCheck631));
            Assert.False(r.IsAlleenMinimaleWapening);
            Assert.Equal(11.0, r.AswTBenodigdPerMeter, 1);
        }
    }
}
