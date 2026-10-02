using CommonLibrary.Mechanica;
using CommonLibrary.Models;
using Eurocode.StaalConstructies;
using FluentAssertions;
using Profielen.Staal;
using Xunit;

namespace Eurocode3.StaalConstructies.Tests;

public class SteelBeamSectionCheckServiceTests
{
    [Fact]
    public void FindFieldExtremes_UsesCorrectSideOfSupportJumps()
    {
        var forces = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 100,
            LoadPosition: 500,
            EccentricityY: 200,
            SuspensionFactor: 0,
            StartOverhang: 1_000,
            EndOverhang: 500,
            ElasticModulus: 210_000,
            SecondMomentAreaY: 36_920_000));

        var fields = SteelBeamSectionCheckService.FindFieldExtremes(forces, 6_000);

        fields.Select(field => field.Field.Type).Should().Equal(
            BeamFieldType.StartOverhang,
            BeamFieldType.MainSpan,
            BeamFieldType.EndOverhang);
        fields[0].MaximumVz.Value.Should().BeApproximately(-100, 1e-10);
        Math.Abs(fields[1].MaximumVz.Value).Should().BeApproximately(100.0 / 9.0, 1e-10);
        fields[2].MaximumVz.Value.Should().Be(0);
    }

    [Fact]
    public void Check_ReturnsFiveEc3ChecksPerFieldUsingProfileClassAndDeflectionLimit()
    {
        var profile = Doorsneden.HEA200;
        var material = new StaalContext { StaalKwaliteit = StaalKwaliteitEnum.S235 };
        var forces = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 2_000,
            EccentricityY: 100,
            SuspensionFactor: 0,
            DistributedLoadQz: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: 4_000,
            DistributedLoadEccentricityY: 100,
            ElasticModulus: material.E,
            SecondMomentAreaY: profile.Iy));

        var field = SteelBeamSectionCheckService.Check(forces, 4_000, profile, material, 250)
            .Should().ContainSingle().Which;

        field.Checks.Should().HaveCount(5);
        field.Checks.Single(check => check.Name == "Buiging My").Norm.Should().Be("EC3");
        field.Checks.Single(check => check.Name == "Dwarskracht Vz").Norm.Should().Be("EC3");
        field.Checks.Single(check => check.Name == "Torsie Tx").Article.Should().Be("6.2.7");
        field.Checks.Single(check => check.Name == "Torsie en dwarskracht Vz").Should().Match<BeamSectionCheck>(check =>
            check.Norm == "EC3"
            && check.Article == "6.2.7"
            && check.Formula == "(6.26)");
        field.Checks.Single(check => check.Name == "Zakking uz").Resistance.Should().Be(16);
    }

    [Fact]
    public void CheckAtPosition_ReturnsFourEc3SectionChecksAtSelectedPosition()
    {
        var checks = SteelBeamSectionCheckService.CheckAtPosition(
            new BeamPositionResult(1_750, 42, -18, 3),
            Doorsneden.HEA200,
            new StaalContext { StaalKwaliteit = StaalKwaliteitEnum.S235 });

        checks.Should().HaveCount(4);
        checks.Should().OnlyContain(check => check.Position == 1_750 && check.Norm == "EC3");
        checks.Should().OnlyContain(check => check.DetailContext is EurocodeResultaat);
        checks.Select(check => check.Name).Should().BeEquivalentTo(
            "Buiging My",
            "Dwarskracht Vz",
            "Torsie Tx",
            "Torsie en dwarskracht Vz");
    }

    [Theory]
    [InlineData("SHS")]
    [InlineData("CHS")]
    public void Check_SupportsHollowSectionsWithEc3TorsionAndShearInteraction(string profileType)
    {
        StaalProfiel profile = profileType switch
        {
            "SHS" => Doorsneden.SHS80x6,
            "CHS" => Doorsneden.CHS76x5,
            _ => throw new ArgumentOutOfRangeException(nameof(profileType))
        };
        var material = new StaalContext { StaalKwaliteit = StaalKwaliteitEnum.S235 };
        var forces = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 1,
            LoadPosition: 2_000,
            EccentricityY: 10,
            SuspensionFactor: 0,
            ElasticModulus: material.E,
            SecondMomentAreaY: profile.Iy));

        var field = SteelBeamSectionCheckService.Check(forces, 4_000, profile, material, 250)
            .Should().ContainSingle().Which;

        field.Checks.Single(check => check.Name == "Buiging My").Resistance.Should().BeGreaterThan(0);
        field.Checks.Single(check => check.Name == "Torsie Tx").Resistance.Should().BeGreaterThan(0);
        field.Checks.Single(check => check.Name == "Torsie en dwarskracht Vz")
            .Should().Match<BeamSectionCheck>(check =>
                check.Formula == "(6.28)"
                && check.Resistance > 0
                && double.IsFinite(check.Utilization));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-250)]
    public void Check_RejectsInvalidDeflectionDivisor(double divisor)
    {
        var profile = Doorsneden.HEA200;
        var material = new StaalContext();
        var forces = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            4_000, 0, 2_000, 0, 0,
            ElasticModulus: material.E,
            SecondMomentAreaY: profile.Iy));

        var action = () => SteelBeamSectionCheckService.Check(
            forces, 4_000, profile, material, divisor);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
