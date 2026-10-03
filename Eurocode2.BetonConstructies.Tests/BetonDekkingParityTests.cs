using Eurocode.BetonConstructies;
using Eurocode.Grondslagen;

namespace Eurocode2.BetonConstructies.Tests
{
    /// <summary>
    /// Handberekende nominale dekking (4.4.1, NL: Δc_dev = 5 mm), dezelfde gevallen als in WapeningUI.
    /// </summary>
    public class BetonDekkingParityTests
    {
        private static BetonDekkingContext Dekking(
            BetonsterkteklasseEnum klasse, MilieuklasseEnum milieuklasse, double diameter,
            OntwerpLevensduurEnum levensduur = OntwerpLevensduurEnum.Vijftig)
        {
            var grondslagen = new GrondslagenContext
            {
                NationaleBijlage = NationaleBijlageEnum.NL,
                OntwerpLevensduur = levensduur,
            };
            var dekking = new BetonDekkingContext(grondslagen, new BetonContext(klasse))
            {
                SelectedMilieuklassen = [milieuklasse],
                WapeningDiameterGelijkwaardig = diameter,
                IsPlaatGeometrie = false,
                IsKwaliteitsBeheersing = false,
            };
            dekking.BerekenEnValideer();
            return dekking;
        }

        [Theory]
        // XC1, C30/37: S4 − 1 (sterkte ≥ C30/37) = S3 → c_min,dur = 10 ; c_min,b = 12 → c_nom = 12 + 5 = 17
        [InlineData(BetonsterkteklasseEnum.C30_37, MilieuklasseEnum.XC1, 12, OntwerpLevensduurEnum.Vijftig, 3, 17)]
        // XC4, C40/50: S4 − 1 = S3 → c_min,dur = 25 → c_nom = 30
        [InlineData(BetonsterkteklasseEnum.C40_50, MilieuklasseEnum.XC4, 12, OntwerpLevensduurEnum.Vijftig, 3, 30)]
        // XC3, C30/37, 100 jaar: S4 + 2 (geen sterkteverlaging, grens XC2/XC3 is C35/45) = S6 → c_min,dur = 35 → c_nom = 40
        [InlineData(BetonsterkteklasseEnum.C30_37, MilieuklasseEnum.XC3, 12, OntwerpLevensduurEnum.Honderd, 6, 40)]
        // gebundeld 2Ø12: Ø_n = 12·√2 = 16,97 → c_min,b = 17 (naar boven) → c_nom = 22
        [InlineData(BetonsterkteklasseEnum.C30_37, MilieuklasseEnum.XC1, 16.97, OntwerpLevensduurEnum.Vijftig, 3, 22)]
        public void NominaleDekking(BetonsterkteklasseEnum klasse, MilieuklasseEnum milieuklasse, double diameter,
            OntwerpLevensduurEnum levensduur, int verwachteKlasse, double verwachtCnom)
        {
            var dekking = Dekking(klasse, milieuklasse, diameter, levensduur);

            Assert.Equal(verwachteKlasse, dekking.Constructieklasse.Klasse);
            Assert.Equal(verwachtCnom, dekking.DekkingNom, 6);
        }
    }
}
