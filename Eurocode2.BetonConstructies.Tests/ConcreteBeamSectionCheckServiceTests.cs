using CommonLibrary.Modelling;
using CommonLibrary.Mechanica;
using Eurocode.BetonConstructies;
using FluentAssertions;
using Profielen.Beton;
using Profielen.Parametrisch;
using Xunit;

namespace Eurocode2.BetonConstructies.Tests;

public class ConcreteBeamSectionCheckServiceTests
{
    [Fact]
    public void TryCreateRectangularProfile_ConvertsRhParametricProfile()
    {
        var profile = new ParametrischProfielContext(500, 500)
        {
            Vorm = ParametrischeProfielVormEnum.Rechthoek
        };

        var supported = ConcreteBeamSectionCheckService.TryCreateRectangularProfile(profile, out var concreteProfile);

        supported.Should().BeTrue();
        concreteProfile.B.Should().Be(500);
        concreteProfile.H.Should().Be(500);
    }

    [Fact]
    public void TryCreateRectangularProfile_RejectsNonRectangularParametricProfile()
    {
        var profile = new ParametrischProfielContext(500, 500)
        {
            Vorm = ParametrischeProfielVormEnum.L1,
            B1 = 150,
            H1 = 250
        };

        ConcreteBeamSectionCheckService.TryCreateRectangularProfile(profile, out _).Should().BeFalse();
    }

    [Fact]
    public void FindCriticalSections_ReturnsRequestedGlobalExtremesAndResolvedForces()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 100,
            LoadPosition: 500,
            EccentricityY: 200,
            SuspensionFactor: 0,
            StartOverhang: 1_000,
            EndOverhang: 500,
            ElasticModulus: 35_000,
            SecondMomentAreaY: 3_125_000_000));

        var sections = ConcreteBeamSectionCheckService.FindCriticalSections(result);

        sections.Select(section => section.Type).Should().Equal(
            ConcreteCriticalSectionType.MaximumAbsoluteShear,
            ConcreteCriticalSectionType.MaximumMoment,
            ConcreteCriticalSectionType.MinimumMoment);
        sections[0].Vz.Should().Be(result.Vz.MaxBy(point => Math.Abs(point.Value))!.Value);
        sections[1].My.Should().Be(result.My.Max(point => point.Value));
        sections[2].My.Should().Be(result.My.Min(point => point.Value));
        sections.Should().OnlyContain(section =>
            double.IsFinite(section.My)
            && double.IsFinite(section.Vz)
            && double.IsFinite(section.Tx));
    }

    [Fact]
    public void Check_ReturnsFiveEc2ChecksForEveryCriticalSection()
    {
        var profile = new BetonProfiel(300, 500);
        var material = new BetonContext(BetonsterkteklasseEnum.C40_50);
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 2_000,
            EccentricityY: 250,
            SuspensionFactor: 0,
            DistributedLoadQz: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: 4_000,
            DistributedLoadEccentricityY: 150,
            ElasticModulus: material.E,
            SecondMomentAreaY: profile.Iy));

        var checks = ConcreteBeamSectionCheckService.Check(
            result,
            profile,
            material,
            CreateLongitudinalReinforcement(5, 16),
            CreateLongitudinalReinforcement(3, 12),
            CreateStirrups(8, 200),
            30,
            30,
            30);

        checks.Should().HaveCount(3);
        checks.Should().OnlyContain(section => section.Checks.Count == 5);
        // Namen bevatten de snedekracht, bijv. "Buiging My (-120 kNm)"; vergelijk het deel ervoor.
        checks.SelectMany(section => section.Checks.Select(check => BasisNaam(check.Name))).Distinct().Should().BeEquivalentTo(
            "Buiging My",
            "Dwarskracht Vz",
            "Torsie Tx",
            "Dwarskracht en torsie",
            "Moment en torsie");
        checks.SelectMany(section => section.Checks).Should().OnlyContain(check =>
            check.Norm == "EC2"
            && check.Resistance >= 0
            && !double.IsNaN(check.Utilization));
    }

    [Fact]
    public void Check_SelectsBottomForNegativeMomentAndTopForPositiveMoment()
    {
        var profile = new BetonProfiel(300, 500);
        var material = new BetonContext(BetonsterkteklasseEnum.C30_37);
        var result = new SimpleBeamForceResult
        {
            StartSupportPosition = 0,
            EndSupportPosition = 4_000,
            StartReactionFz = 0,
            EndReactionFz = 0,
            StartReactionFy = 0,
            EndReactionFy = 0,
            DistributedLoadFz = 0,
            PointSuspensionForceFz = 0,
            DistributedSuspensionForceFz = 0,
            SuspensionForceFz = 0,
            AppliedTorque = 0,
            StartReactionTx = 0,
            EndReactionTx = 0,
            My = [new(0, -20), new(2_000, 30)],
            Vz = [new(0, -10), new(2_000, 5)],
            Mz = [],
            Vy = [],
            Tx = [new(0, 2), new(2_000, 1)],
            DeflectionY = [],
            DeflectionZ = [],
            RotationY = [],
            RotationZ = [],
            RotationX = []
        };

        var checks = ConcreteBeamSectionCheckService.Check(
            result,
            profile,
            material,
            CreateLongitudinalReinforcement(5, 16),
            CreateLongitudinalReinforcement(3, 12),
            CreateStirrups(8, 200),
            30,
            30,
            30);

        checks.Single(section => section.Section.Type == ConcreteCriticalSectionType.MinimumMoment)
            .ReinforcementFace.Should().Be("onder");
        checks.Single(section => section.Section.Type == ConcreteCriticalSectionType.MaximumMoment)
            .ReinforcementFace.Should().Be("boven");

        // d = h − c − Øbgl − Ø/2 = 500 − 30 − 8 − 16/2 = 454 mm; de bibliotheek rekent zelf z = 0,9·d.
        var shear = checks.Single(section => section.Section.Type == ConcreteCriticalSectionType.MinimumMoment)
            .Checks.Single(check => check.Article == "6.2")
            .DetailContext.Should().BeOfType<DwarskrachtWapContext>().Subject;
        shear.NutHoogte.Should().BeApproximately(454, 1e-10);
    }

    [Fact]
    public void CheckAtPosition_ReturnsFiveEc2ChecksAtSelectedPosition()
    {
        var result = ConcreteBeamSectionCheckService.CheckAtPosition(
            new BeamPositionResult(1_250, -35, 20, 4),
            new ConcreteSectionContext(
                ConcreteStructuralSystem.StaticallyDeterminate,
                IsDeepBeamRegion: false,
                DeepBeamLength: null),
            new BetonProfiel(300, 500),
            new BetonContext(BetonsterkteklasseEnum.C40_50),
            CreateLongitudinalReinforcement(5, 16),
            CreateLongitudinalReinforcement(3, 12),
            CreateStirrups(8, 200),
            30,
            30,
            30);

        result.ReinforcementFace.Should().Be("onder");
        result.Checks.Should().HaveCount(5);
        result.Checks.Should().OnlyContain(check => check.Position == 1_250 && check.Norm == "EC2");
        result.Checks.Should().OnlyContain(check => check.DetailContext != null);
        result.Checks.Single(check => check.Name.StartsWith("Buiging My"))
            .DetailContext.Should().BeOfType<BendingResults>();
        result.Checks.Single(check => BasisNaam(check.Name) == "Dwarskracht Vz")
            .DetailContext.Should().BeOfType<DwarskrachtWapContext>();
        result.Checks.Single(check => check.Name == "Torsie Tx")
            .DetailContext.Should().BeOfType<J3ConsoleTorsieResult>();
        result.Checks.Single(check => check.Name == "Dwarskracht en torsie")
            .DetailContext.Should().BeOfType<J3ConsoleTorsieDwarskrachtCombinatie>();
        result.Checks.Single(check => check.Name == "Moment en torsie")
            .DetailContext.Should().BeOfType<ConcreteMomentTorsionDetail>();
    }

    private static string BasisNaam(string naam)
    {
        var haakje = naam.IndexOf(" (", StringComparison.Ordinal);
        return haakje < 0 ? naam : naam[..haakje];
    }

    private static WapeningGroep CreateLongitudinalReinforcement(int count, double diameter) => new()
    {
        Diameter = diameter,
        Verdeling = new()
        {
            Type = VerdelingType.Gelijkmatig,
            Aantal = count
        }
    };

    private static WapeningGroep CreateStirrups(double diameter, double spacing) => new()
    {
        Diameter = diameter,
        Verdeling = new()
        {
            Type = VerdelingType.BeoogdeHartOpHart,
            BeoogdeHartOpHart = spacing
        }
    };
}
