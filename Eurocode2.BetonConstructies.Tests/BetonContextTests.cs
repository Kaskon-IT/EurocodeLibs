using Eurocode.BetonConstructies;

namespace Eurocode2.BetonConstructies.Tests;

public class BetonContextTests
{
    [Theory]
    [InlineData(BetonsterkteklasseEnum.C20_25, 20, 25)]
    [InlineData(BetonsterkteklasseEnum.C25_30, 25, 30)]
    [InlineData(BetonsterkteklasseEnum.C30_37, 30, 37)]
    public void BetonContext_MetKlasse_SteltFckEnFckCubeCorrectIn(
            BetonsterkteklasseEnum klasse, double expectedFck, double expectedFckCube)
    {
        // Arrange & Act
        var context = new BetonContext(klasse);

        // Assert
        Assert.Equal(expectedFck, context.Fck);
        Assert.Equal(expectedFckCube, context.FckCube);
    }

    [Fact]
    public void Fcd_WordtCorrectBerekenend_VoorDefaultWaarden()
    {
        // Arrange
        var context = new BetonContext
        {
            Betonsterkteklasse = BetonsterkteklasseEnum.C30_37,
            GammaC = 1.5,
        };

        // Act
        var fcd = context.Fcd;

        // Assert
        Assert.Equal(20.0, fcd, precision: 6);
        // 30 / 1.5 = 20.0
    }




    [Fact]
    public void Fctk_WordtCorrectBerekenend_VoorDefaultWaarden()
    {
        // Arrange
        var context = new BetonContext
        {
            Betonsterkteklasse = BetonsterkteklasseEnum.C30_37,
        };
        // Act
        var fctk = context.FctkVijfProcent;
        // Assert
        Assert.Equal(2.6, fctk, precision: 6);
        // Voor C30/37 is fctk,5% = 2.6 N/mm² volgens Eurocode 2 tabel 3.1
    }


    [Fact]
    public void Fctd_WordtCorrectBerekenend_VoorDefaultWaarden()
    {
        // Arrange
        var context = new BetonContext(); // de ba


        // Act
        var fctd = context.Fctd;

        // Assert
        Assert.Equal(1.4667, fctd, precision: 4);
        // 2.2 / 1.5 ≈ 1.4667
    }


}
