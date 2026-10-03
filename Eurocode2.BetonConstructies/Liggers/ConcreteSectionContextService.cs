using CommonLibrary.Mechanica;

namespace Eurocode.BetonConstructies;

// Hoort bij de betoncontrole (ConcreteBeamSectionCheckService).

public enum ConcreteStructuralSystem
{
    Cantilever,
    StaticallyDeterminate,
    StaticallyIndeterminate
}

public sealed record ConcreteSectionContext(
    ConcreteStructuralSystem StructuralSystem,
    bool IsDeepBeamRegion,
    double? DeepBeamLength);

public static class ConcreteSectionContextService
{
    public static ConcreteSectionContext ResolveConcreteSectionContext(
        SimpleBeamForceResult forces,
        double position,
        double sectionHeight,
        ConcreteStructuralSystem? structuralSystemOverride = null)
    {
        ArgumentNullException.ThrowIfNull(forces);
        if (!double.IsFinite(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (!double.IsFinite(sectionHeight) || sectionHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sectionHeight));
        }

        var totalLength = forces.My.Count > 0
            ? forces.My.Max(point => point.Position)
            : forces.EndSupportPosition;
        if (position < 0 || position > totalLength)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        var isStartOverhang = position < forces.StartSupportPosition;
        var isEndOverhang = position > forces.EndSupportPosition;
        var structuralSystem = structuralSystemOverride
            ?? (isStartOverhang || isEndOverhang
                ? ConcreteStructuralSystem.Cantilever
                : ConcreteStructuralSystem.StaticallyDeterminate);
        var regionLength = isStartOverhang
            ? forces.StartSupportPosition
            : isEndOverhang
                ? totalLength - forces.EndSupportPosition
                : forces.EndSupportPosition - forces.StartSupportPosition;
        var deepBeamLimit = structuralSystem == ConcreteStructuralSystem.Cantilever
            ? 1.5 * sectionHeight
            : 3 * sectionHeight;
        var isDeepBeamRegion = regionLength > 0 && regionLength <= deepBeamLimit;

        return new ConcreteSectionContext(structuralSystem, isDeepBeamRegion, regionLength);
    }
}
