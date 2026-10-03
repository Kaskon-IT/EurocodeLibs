using CommonLibrary;
using Eurocode.Belastingen;
using Eurocode.BetonConstructies;
using Profielen.Beton;

namespace Eurocode2.BetonConstructies.Tests
{
    /// <summary>
    /// Handberekende controles bij drie correcties (fase 4, bij het overzetten van WapeningUI).
    /// </summary>
    public class CorrectiesFase4Tests
    {
        [Fact]
        public void VRdc_6_2a_RekentMetFck()
        {
            // C30/37 (fck = 30, fcd = 20), b = 1000, d = 250, As = 1250 mm² → ρl = 0,005
            var context = new DwarskrachtWapContext(
                new BetonContext(BetonsterkteklasseEnum.C30_37),
                new BetonProfiel { Breedte = 1000, Hoogte = 300 },
                new SectionForces(vz: 100))
            {
                NutHoogte = 250,
                AsLangs = 1250,
            };

            // k = 1 + √(200/250) = 1,8944 ; C_Rd,c = 0,18/1,5 = 0,12
            // v_Rd,c = 0,12 · 1,8944 · (100 · 0,005 · 30)^(1/3) = 0,5607 N/mm²
            // (met fcd = 20 zou dit 0,4898 zijn, kleiner dan v_min = 0,035·k^1,5·√30 = 0,4999)
            Assert.Equal(1000, context.Breedte, 6);
            Assert.Equal(0.005, context.RhoLangs, 6);
            Assert.Equal(1.8944, context.FactorK, 4);
            Assert.Equal(0.5607, context.SchuifspanningWeerstandBeton, 3);

            // V_Rd,c = 0,5607 · 1000 · 250 = 140,2 kN
            Assert.Equal(140.2, context.DwarskrachtWeerstandBeton, 1);
        }

        [Fact]
        public void AcEff_7_3_2_GebruiktDrukzoneHoogte()
        {
            // h = 300, d = 250, x = 60: h_c,ef = min(2,5·50 ; (300 − 60)/3 ; 300/2) = min(125 ; 80 ; 150) = 80
            // (met (h − d)/3 zou het 16,7 zijn)
            var sw = new ScheurwijdteContext
            {
                Breedte = 1000,
                Hoogte = 300,
                NuttigeHoogte = 250,
                HoogteBetonDrukZoneBGT = 60,
            };

            Assert.Equal(80_000, sw.GetAcEff(), 6);
        }

        [Theory]
        [InlineData(1000, 500, true)]   // L < 3h: gedrongen
        [InlineData(1500, 500, false)]  // L = 3h: balk (5.3.1 (3))
        [InlineData(2000, 500, false)]  // L > 3h: balk
        public void IsGedrongen_5_3_1(double overspanning, double hoogte, bool verwacht)
        {
            Assert.Equal(verwacht, BuigingContext.IsGedrongen(overspanning, hoogte));
        }
    }
}
