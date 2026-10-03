using Eurocode.BetonConstructies;
using CommonLibrary.Mechanica;
using FluentAssertions;
using Xunit;

namespace Eurocode2.BetonConstructies.Tests;

public class ConcreteSectionContextServiceTests
{
    [Fact]
    public void ResolveConcreteSectionContext_ClassifiesShortOverhangAsDeepBeamRegion()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 500,
            EccentricityY: 0,
            SuspensionFactor: 0,
            StartOverhang: 750));

        var context = ConcreteSectionContextService.ResolveConcreteSectionContext(result, 500, 500);

        context.StructuralSystem.Should().Be(ConcreteStructuralSystem.Cantilever);
        context.IsDeepBeamRegion.Should().BeTrue();
        context.DeepBeamLength.Should().Be(750);
    }

    [Fact]
    public void ResolveConcreteSectionContext_ClassifiesShortSpanAsDeepBeamRegion()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 2_000,
            EccentricityY: 0,
            SuspensionFactor: 0,
            StartOverhang: 1_250,
            EndOverhang: 1_250));

        var context = ConcreteSectionContextService.ResolveConcreteSectionContext(result, 2_000, 500);

        context.StructuralSystem.Should().Be(ConcreteStructuralSystem.StaticallyDeterminate);
        context.IsDeepBeamRegion.Should().BeTrue();
        context.DeepBeamLength.Should().Be(1_500);
    }

    [Fact]
    public void ResolveConcreteSectionContext_DoesNotClassifyLongSpanAsDeepBeamRegion()
    {
        var result = SimpleBeamForceModel.Calculate(new SimpleBeamForceInput(
            Length: 4_000,
            PointLoadFz: 100,
            LoadPosition: 2_000,
            EccentricityY: 0,
            SuspensionFactor: 0));

        var context = ConcreteSectionContextService.ResolveConcreteSectionContext(result, 2_000, 500);

        context.StructuralSystem.Should().Be(ConcreteStructuralSystem.StaticallyDeterminate);
        context.IsDeepBeamRegion.Should().BeFalse();
        context.DeepBeamLength.Should().Be(4_000);
    }
}
