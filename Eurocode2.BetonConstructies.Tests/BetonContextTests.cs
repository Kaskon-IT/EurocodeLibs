using CommonLibrary;
using Eurocode.BetonConstructies;
using System.Reflection;

namespace Eurocode2.BetonConstructies.Tests;




public class BaseEurocodeContextGeneriekeTests
{
    /// <summary>
    /// Alle afgeleide classes van BaseEurocodeContext in dezelfde assembly.
    /// </summary>
    public static IEnumerable<object[]> AlleAfgeleideContexten()
    {
        var baseType = typeof(BaseEurocodeContext);

        var assemblies = new[]
        {
        typeof(BaseEurocodeContext).Assembly,
        typeof(BetonContext).Assembly
    };

        var afgeleiden = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        // 🔹 Debug check: zie je niets? Dan zit je in de verkeerde assembly
        Assert.True(afgeleiden.Any(),
            $"Geen afgeleiden van {baseType.Name} gevonden in assemblies: {string.Join(", ", assemblies.Select(a => a.GetName().Name))}");

        foreach (var type in afgeleiden)
        {
            yield return new object[] { type };
        }
    }


    [Theory]
    [MemberData(nameof(AlleAfgeleideContexten))]
    public void Kan_Instance_Maken_En_Berekenen(Type contextType)
    {
        // Arrange
        var instance = Activator.CreateInstance(contextType) as BaseEurocodeContext;
        Assert.NotNull(instance);

        // Act
        var ex = Record.Exception(() => instance!.BerekenEnValideer());

        // Assert
        Assert.Null(ex); // mag geen exception gooien
    }

    [Theory]
    [MemberData(nameof(AlleAfgeleideContexten))]
    public void GammaC_IsAltijdGroterDanNul(Type contextType)
    {
        var instance = Activator.CreateInstance(contextType) as BaseEurocodeContext;
        Assert.NotNull(instance);

        var prop = contextType.GetProperty("GammaC", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(double))
        {
            var value = (double)prop.GetValue(instance)!;
            Assert.True(value > 0, $"{contextType.Name}.GammaC moet > 0 zijn (nu {value})");
        }
    }
}



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
        Assert.Equal(2.0275277, fctk, precision: 6);
        // Voor C30/37 is fctk,5% = 2.6 N/mm² volgens Eurocode 2 tabel 3.1
    }


    [Fact]
    public void Fctd_WordtCorrectBerekenend_VoorDefaultWaarden()
    {
        // Arrange
        var context = new BetonContext("C30/37"); // de ba


        // Act
        var fctd = context.Fctd;

        // Assert
        Assert.Equal(1.3516851, fctd, precision: 6);

    }


}
