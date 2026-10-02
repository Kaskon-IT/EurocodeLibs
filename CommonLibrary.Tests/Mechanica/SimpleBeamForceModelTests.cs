using CommonLibrary.Mechanica;
using FluentAssertions;
using Xunit;

namespace CommonLibrary.Tests.Mechanica;

public class SimpleBeamForceModelTests
{
    [Fact]
    public void Calculate_ReturnsReactionsAndForceDiagrams()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 1_500,
            EccentricityY: 250,
            SuspensionFactor: 0.4));

        result.StartReactionFz.Should().BeApproximately(62.5, 1e-10);
        result.EndReactionFz.Should().BeApproximately(37.5, 1e-10);
        result.SuspensionForceFz.Should().BeApproximately(40, 1e-10);
        result.AppliedTorque.Should().BeApproximately(25, 1e-10);
        result.StartReactionTx.Should().BeApproximately(15.625, 1e-10);
        result.EndReactionTx.Should().BeApproximately(9.375, 1e-10);
        result.My.Select(point => point.Value).Should().Equal(0, -93.75, 0);
        result.Vz.Should().Equal(
            new BeamForcePoint(0, 0),
            new BeamForcePoint(0, 62.5),
            new BeamForcePoint(1_500, 62.5),
            new BeamForcePoint(1_500, -37.5),
            new BeamForcePoint(4_000, -37.5),
            new BeamForcePoint(4_000, 0));
        result.Tx.Should().Equal(
            new BeamForcePoint(0, 0),
            new BeamForcePoint(0, 15.625),
            new BeamForcePoint(1_500, 15.625),
            new BeamForcePoint(1_500, -9.375),
            new BeamForcePoint(4_000, -9.375),
            new BeamForcePoint(4_000, 0));
        result.Nx.Should().Be(0);
        result.Vy.Should().OnlyContain(point => point.Value == 0);
        result.Mz.Should().OnlyContain(point => point.Value == 0);
    }

    [Fact]
    public void Calculate_IncludesContinuousZeroShearAndExtremeMomentPosition()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 0,
            LoadPosition: 0,
            EccentricityY: 0,
            SuspensionFactor: 0,
            DistributedLoadQz: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: 3_000));

        var zeroShear = result.Vz.Single(point =>
            point.Position > 0
            && point.Position < 3_000
            && Math.Abs(point.Value) < 1e-9);
        var extremeMoment = result.My.MaxBy(point => Math.Abs(point.Value))!;

        zeroShear.Position.Should().BeApproximately(extremeMoment.Position, 1e-9);
        zeroShear.Position.Should().NotBeApproximately(1_500, 1e-9);
    }

    [Fact]
    public void ResolvePosition_InterpolatesAndUsesLargestAbsoluteJumpValue()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            4_000, 100, 1_500, 250, 0));

        var interpolated = BeamPositionResultService.Resolve(result, 750, 4_000);
        var atLoad = BeamPositionResultService.Resolve(result, 1_500, 4_000);

        interpolated.My.Should().BeApproximately(-46.875, 1e-10);
        interpolated.Vz.Should().BeApproximately(62.5, 1e-10);
        atLoad.Vz.Should().BeApproximately(62.5, 1e-10);
        atLoad.Tx.Should().BeApproximately(15.625, 1e-10);
    }

    [Fact]
    public void Calculate_LocalYPointLoad_ReturnsVyAndRightHandRuleMz()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 0,
            LoadPosition: 2_000,
            EccentricityY: 0,
            SuspensionFactor: 0,
            PointLoadFy: 100));

        result.StartReactionFy.Should().BeApproximately(50, 1e-10);
        result.EndReactionFy.Should().BeApproximately(50, 1e-10);
        result.Vy.Should().Contain(new BeamForcePoint(2_000, 50));
        result.Mz.Single(point => point.Position == 2_000).Value.Should().BeApproximately(100, 1e-10);

        var atMidspan = BeamPositionResultService.Resolve(result, 2_000, 4_000);
        atMidspan.Vy.Should().BeApproximately(50, 1e-10);
        atMidspan.Mz.Should().BeApproximately(100, 1e-10);
    }

    [Fact]
    public void Calculate_LocalYDistributedLoad_ReturnsVyAndMz()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 0,
            LoadPosition: 0,
            EccentricityY: 0,
            SuspensionFactor: 0,
            DistributedLoadQy: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: 4_000));

        result.StartReactionFy.Should().BeApproximately(20, 1e-10);
        result.EndReactionFy.Should().BeApproximately(20, 1e-10);
        result.Mz.Max(point => point.Value).Should().BeApproximately(20, 1e-10);
        result.Vz.Should().OnlyContain(point => point.Value == 0);
        result.My.Should().OnlyContain(point => point.Value == 0);
    }

    [Fact]
    public void Calculate_TorqueCombinesEyFzAndEzFyUsingRightHandRule()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 2_000,
            EccentricityY: 200,
            SuspensionFactor: 0,
            PointLoadFy: 40,
            PointLoadEccentricityZ: 300));

        result.AppliedTorque.Should().BeApproximately(8, 1e-10);
        result.StartReactionTx.Should().BeApproximately(4, 1e-10);
        result.EndReactionTx.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void ResolvePosition_ReturnsResultantMomentAndDirection()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 80,
            LoadPosition: 2_000,
            EccentricityY: 0,
            SuspensionFactor: 0,
            PointLoadFy: 60));

        var atMidspan = BeamPositionResultService.Resolve(result, 2_000, 4_000);

        atMidspan.My.Should().BeApproximately(-80, 1e-10);
        atMidspan.Mz.Should().BeApproximately(60, 1e-10);
        atMidspan.Mres.Should().BeApproximately(100, 1e-10);
        atMidspan.MomentAngleDegrees.Should().BeApproximately(143.130102, 1e-6);
        result.Mres.Single(point => point.Position == 2_000).Value.Should().BeApproximately(100, 1e-10);
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(4_000, 0, 100)]
    public void Calculate_AllowsLoadAtSupport(
        double position,
        double expectedStartReaction,
        double expectedEndReaction)
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            4_000, 100, position, 0, 0));

        result.StartReactionFz.Should().Be(expectedStartReaction);
        result.EndReactionFz.Should().Be(expectedEndReaction);
        result.Vz.Count(point => point.Position == position).Should().Be(1);
    }

    [Fact]
    public void Calculate_LoadOnStartOverhang_ReturnsNegativeEndReactionAndSupportMoment()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 100,
            LoadPosition: 500,
            EccentricityY: 250,
            SuspensionFactor: 0.5,
            StartOverhang: 1_000,
            EndOverhang: 500));

        result.StartSupportPosition.Should().Be(1_000);
        result.EndSupportPosition.Should().Be(5_500);
        result.StartReactionFz.Should().BeApproximately(111.111111, 1e-6);
        result.EndReactionFz.Should().BeApproximately(-11.111111, 1e-6);
        result.My.Single(point => point.Position == 1_000).Value.Should().BeApproximately(50, 1e-10);
        result.My.Single(point => point.Position == 5_500).Value.Should().BeApproximately(0, 1e-10);
        result.Vz.Count(point => point.Position == 500).Should().Be(2);
        result.Vz.Count(point => point.Position == 1_000).Should().Be(2);
        result.Vz.Count(point => point.Position == 5_500).Should().Be(2);
        result.Vz[^1].Should().Be(new BeamForcePoint(6_000, 0));
    }

    [Fact]
    public void Calculate_TorqueOnStartOverhang_GoesEntirelyToStartSupport()
    {
        // T = 100 mm × 10 kN = 1 kNm op de linkeroverstek: geen torsie in de overspanning.
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 10,
            LoadPosition: 0,
            EccentricityY: 100,
            SuspensionFactor: 0,
            StartOverhang: 1_000));

        result.StartReactionTx.Should().BeApproximately(1, 1e-10);
        result.EndReactionTx.Should().BeApproximately(0, 1e-10);
        // Vanaf de beginoplegging (na de sprong) is de torsie nul tot aan het einde.
        result.Tx.Where(point => point.Position >= 1_000).Skip(1)
            .Should().NotBeEmpty()
            .And.OnlyContain(point => Math.Abs(point.Value) < 1e-10);
    }

    [Fact]
    public void Calculate_TorqueOnEndOverhang_GoesEntirelyToEndSupport()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 10,
            LoadPosition: 6_000,
            EccentricityY: 100,
            SuspensionFactor: 0,
            StartOverhang: 0,
            EndOverhang: 1_000));

        result.StartReactionTx.Should().BeApproximately(0, 1e-10);
        result.EndReactionTx.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Calculate_DistributedTorqueAcrossSupport_SplitsOverhangAndSpan()
    {
        // 1 kNm/m over 0..6000 mm, beginoplegging op 1000 mm:
        // overstek 1 kNm volledig naar begin, overspanning 5 kNm half/half → 3,5 en 2,5 kNm.
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 0,
            LoadPosition: 0,
            EccentricityY: 0,
            SuspensionFactor: 0,
            StartOverhang: 1_000,
            DistributedLoadQz: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: 6_000,
            DistributedLoadEccentricityY: 100));

        result.AppliedTorque.Should().BeApproximately(6, 1e-10);
        result.StartReactionTx.Should().BeApproximately(3.5, 1e-10);
        result.EndReactionTx.Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Calculate_LoadCoincidentWithInternalSupport_CombinesJumps()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            6_000, 100, 1_000, 250, 0, 1_000, 500));

        result.Vz.Count(point => point.Position == 1_000).Should().Be(1);
        result.Tx.Count(point => point.Position == 1_000).Should().Be(1);
        result.Vz.Single(point => point.Position == 1_000).Value.Should().Be(0);
    }

    [Fact]
    public void Calculate_FullSpanDistributedLoad_ReturnsForcesTorqueAndDeflection()
    {
        const double length = 4_000;
        const double elasticModulus = 210_000;
        const double secondMomentArea = 100_000_000;
        var input = new SimpleBeamForceInput(
            Length: length,
            PointLoadFz: 0,
            LoadPosition: 2_000,
            EccentricityY: 0,
            SuspensionFactor: 0,
            DistributedLoadQz: 10,
            DistributedLoadStart: 0,
            DistributedLoadEnd: length,
            DistributedLoadEccentricityY: 200,
            DistributedLoadSuspensionFactor: 0.25,
            ElasticModulus: elasticModulus,
            SecondMomentAreaY: secondMomentArea);

        var result = SimpleBeamForceModel.Calculate(input);
        var expectedDeflection = 5 * 10 * Math.Pow(length, 4)
            / (384 * elasticModulus * secondMomentArea);

        result.DistributedLoadFz.Should().Be(40);
        result.StartReactionFz.Should().BeApproximately(20, 1e-10);
        result.EndReactionFz.Should().BeApproximately(20, 1e-10);
        result.DistributedSuspensionForceFz.Should().Be(10);
        result.SuspensionForceFz.Should().Be(10);
        result.AppliedTorque.Should().Be(8);
        result.StartReactionTx.Should().BeApproximately(4, 1e-10);
        result.EndReactionTx.Should().BeApproximately(4, 1e-10);
        result.My.Single(point => point.Position == 2_000).Value.Should().BeApproximately(-20, 1e-10);
        result.DeflectionZ.Single(point => point.Position == 0).Value.Should().Be(0);
        result.DeflectionZ.Single(point => point.Position == length).Value.Should().Be(0);
        result.DeflectionZ.Single(point => point.Position == 2_000).Value
            .Should().BeApproximately(-expectedDeflection, 1e-10);
        result.MaximumAbsoluteDeflection.Should().BeApproximately(expectedDeflection, 1e-10);
    }

    [Fact]
    public void Calculate_WithFlexuralRigidity_ReturnsRotationY()
    {
        const double length = 4_000;
        const double load = 100;
        const double elasticModulus = 210_000;
        const double secondMomentArea = 100_000_000;
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: length,
            PointLoadFz: load,
            LoadPosition: length / 2,
            EccentricityY: 0,
            SuspensionFactor: 0,
            ElasticModulus: elasticModulus,
            SecondMomentAreaY: secondMomentArea));
        var expectedSupportRotation = load * 1_000 * Math.Pow(length, 2)
            / (16 * elasticModulus * secondMomentArea);

        result.RotationY.Single(point => point.Position == 0).Value
            .Should().BeApproximately(-expectedSupportRotation, 1e-12);
        result.RotationY.Single(point => point.Position == length / 2).Value.Should().BeApproximately(0, 1e-12);
        result.RotationY.Single(point => point.Position == length).Value
            .Should().BeApproximately(expectedSupportRotation, 1e-12);
    }

    [Fact]
    public void Calculate_WithLocalYFlexuralRigidity_ReturnsDeflectionYAndRotationZ()
    {
        const double length = 4_000;
        const double load = 100;
        const double elasticModulus = 210_000;
        const double secondMomentArea = 50_000_000;
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: length,
            PointLoadFz: 0,
            LoadPosition: length / 2,
            EccentricityY: 0,
            SuspensionFactor: 0,
            ElasticModulus: elasticModulus,
            PointLoadFy: load,
            SecondMomentAreaZ: secondMomentArea));
        var expectedDeflection = load * 1_000 * Math.Pow(length, 3)
            / (48 * elasticModulus * secondMomentArea);

        result.DeflectionY.Single(point => point.Position == length / 2).Value
            .Should().BeApproximately(-expectedDeflection, 1e-10);
        result.RotationZ.Should().Contain(point => point.Value != 0);
    }

    [Fact]
    public void Calculate_WithTorsionalRigidity_ReturnsRotationX()
    {
        const double length = 4_000;
        const double load = 100;
        const double eccentricity = 250;
        const double shearModulus = 81_000;
        const double torsionConstant = 10_000_000;
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: length,
            PointLoadFz: load,
            LoadPosition: length / 2,
            EccentricityY: eccentricity,
            SuspensionFactor: 0,
            ShearModulus: shearModulus,
            TorsionConstant: torsionConstant));
        var expectedMidspanRotation = load * eccentricity / 1_000 / 2
            * 1_000_000 * (length / 2) / (shearModulus * torsionConstant);

        result.RotationX.First(point => point.Position == length / 2).Value
            .Should().BeApproximately(expectedMidspanRotation, 1e-12);
        result.RotationX[^1].Value.Should().BeApproximately(0, 1e-12);
    }

    [Fact]
    public void Calculate_WithOverhangAndTorsionalRigidity_ReturnsZeroRotationXAtSupports()
    {
        const double startSupportPosition = 1_000;
        const double endSupportPosition = 5_500;
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 6_000,
            PointLoadFz: 100,
            LoadPosition: 500,
            EccentricityY: 250,
            SuspensionFactor: 0,
            StartOverhang: startSupportPosition,
            EndOverhang: 500,
            ShearModulus: 81_000,
            TorsionConstant: 10_000_000));

        result.RotationX.Where(point => point.Position == startSupportPosition)
            .Should().OnlyContain(point => Math.Abs(point.Value) < 1e-12);
        result.RotationX.Where(point => point.Position == endSupportPosition)
            .Should().OnlyContain(point => Math.Abs(point.Value) < 1e-12);
        result.RotationX.First(point => point.Position == 0).Value.Should().NotBe(0);
    }

    [Fact]
    public void Calculate_WithMultipleLoads_SuperimposesAllLoads()
    {
        var input = new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 0,
            LoadPosition: 0,
            EccentricityY: 0,
            SuspensionFactor: 0,
            PointLoads:
            [
                new SimpleBeamPointLoadInput(Guid.NewGuid(), 40, 1_000, 100, 0.25),
                new SimpleBeamPointLoadInput(Guid.NewGuid(), 60, 3_000, 200, 0.5)
            ],
            DistributedLoads:
            [
                new SimpleBeamDistributedLoadInput(Guid.NewGuid(), 10, 0, 2_000, 50, 0.1),
                new SimpleBeamDistributedLoadInput(Guid.NewGuid(), 20, 2_000, 4_000, 150, 0.2)
            ]);

        var result = SimpleBeamForceModel.Calculate(input);

        result.StartReactionFz.Should().BeApproximately(70, 1e-10);
        result.EndReactionFz.Should().BeApproximately(90, 1e-10);
        result.DistributedLoadFz.Should().BeApproximately(60, 1e-10);
        result.PointSuspensionForceFz.Should().BeApproximately(40, 1e-10);
        result.DistributedSuspensionForceFz.Should().BeApproximately(10, 1e-10);
        result.AppliedTorque.Should().BeApproximately(23, 1e-10);
        result.Vz.Count(point => point.Position == 1_000).Should().Be(2);
        result.Vz.Count(point => point.Position == 3_000).Should().Be(2);
    }

    [Theory]
    [InlineData(-1, 1_000)]
    [InlineData(2_000, 1_000)]
    [InlineData(0, 4_001)]
    public void Calculate_RejectsInvalidDistributedLoadPositions(double start, double end)
    {
        var action = () => SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            4_000, 0, 2_000, 0, 0,
            DistributedLoadQz: 10,
            DistributedLoadStart: start,
            DistributedLoadEnd: end));

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 100, -1, 0, 0)]
    [InlineData(4_000, 100, -1, 0, 0)]
    [InlineData(4_000, 100, 4_001, 0, 0)]
    [InlineData(4_000, 100, 2_000, 0, -0.1)]
    [InlineData(4_000, 100, 2_000, 0, 1.1)]
    public void Calculate_RejectsInvalidInput(
        double length,
        double force,
        double position,
        double eccentricity,
        double suspensionFactor)
    {
        var action = () => SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            length, force, position, eccentricity, suspensionFactor));

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2_000, 2_000)]
    [InlineData(3_000, 2_000)]
    public void Calculate_RejectsInvalidOverhangs(double startOverhang, double endOverhang)
    {
        var action = () => SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            4_000, 100, 2_000, 0, 0, startOverhang, endOverhang));

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
