namespace CommonLibrary.Mechanica;

public sealed record BeamPositionResult(
    double Position,
    double My,
    double Vz,
    double Tx,
    double Mz = 0,
    double Vy = 0)
{
    public double Mres => Math.Sqrt(My * My + Mz * Mz);
    public double MomentAngleDegrees => Math.Atan2(Mz, My) * 180 / Math.PI;
}

public static class BeamPositionResultService
{
    public static IReadOnlyList<BeamForcePoint> CreateResultantMoment(SimpleBeamForceResult forces)
    {
        ArgumentNullException.ThrowIfNull(forces);

        return forces.My
            .Select(point => point.Position)
            .Concat(forces.Mz.Select(point => point.Position))
            .Distinct()
            .Order()
            .Select(position =>
            {
                var my = ResolveValue(forces.My, position);
                var mz = ResolveValue(forces.Mz, position);
                return new BeamForcePoint(position, Math.Sqrt(my * my + mz * mz));
            })
            .ToArray();
    }

    public static IReadOnlyList<BeamForcePoint> CreateResultantShear(SimpleBeamForceResult forces)
    {
        ArgumentNullException.ThrowIfNull(forces);

        return forces.Vz
            .Select(point => point.Position)
            .Concat(forces.Vy.Select(point => point.Position))
            .Distinct()
            .Order()
            .Select(position =>
            {
                var vz = ResolveValue(forces.Vz, position);
                var vy = ResolveValue(forces.Vy, position);
                return new BeamForcePoint(position, Math.Sqrt(vz * vz + vy * vy));
            })
            .ToArray();
    }

    public static BeamPositionResult Resolve(
        SimpleBeamForceResult forces,
        double position,
        double totalLength)
    {
        ArgumentNullException.ThrowIfNull(forces);
        if (!double.IsFinite(totalLength) || totalLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalLength));
        }

        if (!double.IsFinite(position) || position < 0 || position > totalLength)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return new BeamPositionResult(
            position,
            ResolveValue(forces.My, position),
            ResolveValue(forces.Vz, position),
            ResolveValue(forces.Tx, position),
            ResolveValue(forces.Mz, position),
            ResolveValue(forces.Vy, position));
    }

    private static double ResolveValue(IReadOnlyList<BeamForcePoint> points, double position)
    {
        if (points.Count == 0)
        {
            return 0;
        }

        var exactValues = points
            .Where(point => Math.Abs(point.Position - position) < 1e-9)
            .Select(point => point.Value)
            .ToArray();
        if (exactValues.Length > 0)
        {
            return exactValues.MaxBy(Math.Abs);
        }

        var before = points.LastOrDefault(point => point.Position < position);
        var after = points.FirstOrDefault(point => point.Position > position);
        if (before is null)
        {
            return points[0].Value;
        }

        if (after is null)
        {
            return points[^1].Value;
        }

        var factor = (position - before.Position) / (after.Position - before.Position);
        return before.Value + factor * (after.Value - before.Value);
    }
}
