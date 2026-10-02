namespace CommonLibrary.Mechanica;

public sealed record SimpleBeamForceInput(
    double Length,
    double PointLoadFz,
    double LoadPosition,
    double EccentricityY,
    double SuspensionFactor,
    double StartOverhang = 0,
    double EndOverhang = 0,
    double DistributedLoadQz = 0,
    double DistributedLoadStart = 0,
    double DistributedLoadEnd = 0,
    double DistributedLoadEccentricityY = 0,
    double DistributedLoadSuspensionFactor = 0,
    double? ElasticModulus = null,
    double? SecondMomentAreaY = null,
    double? ShearModulus = null,
    double? TorsionConstant = null,
    IReadOnlyList<SimpleBeamPointLoadInput>? PointLoads = null,
    IReadOnlyList<SimpleBeamDistributedLoadInput>? DistributedLoads = null,
    double PointLoadFy = 0,
    double DistributedLoadQy = 0,
    double? SecondMomentAreaZ = null,
    double PointLoadEccentricityZ = 0,
    double DistributedLoadEccentricityZ = 0)
{
    public IReadOnlyList<SimpleBeamPointLoadInput> EffectivePointLoads => PointLoads ??
    [
        new(Guid.Empty, PointLoadFz, LoadPosition, EccentricityY, SuspensionFactor, PointLoadFy, PointLoadEccentricityZ)
    ];

    public IReadOnlyList<SimpleBeamDistributedLoadInput> EffectiveDistributedLoads => DistributedLoads ??
    [
        new(
            Guid.Empty,
            DistributedLoadQz,
            DistributedLoadStart,
            DistributedLoadEnd,
            DistributedLoadEccentricityY,
            DistributedLoadSuspensionFactor,
            DistributedLoadQy,
            DistributedLoadEccentricityZ)
    ];
}

public sealed record SimpleBeamPointLoadInput(
    Guid Id,
    double ForceFz,
    double Position,
    double EccentricityY,
    double SuspensionFactor,
    double ForceFy = 0,
    double EccentricityZ = 0);

public sealed record SimpleBeamDistributedLoadInput(
    Guid Id,
    double LoadQz,
    double StartPosition,
    double EndPosition,
    double EccentricityY,
    double SuspensionFactor,
    double LoadQy = 0,
    double EccentricityZ = 0);

public sealed record BeamForcePoint(double Position, double Value);

public sealed class SimpleBeamForceResult
{
    public required double StartSupportPosition { get; init; }
    public required double EndSupportPosition { get; init; }
    public required double StartReactionFz { get; init; }
    public required double EndReactionFz { get; init; }
    public required double StartReactionFy { get; init; }
    public required double EndReactionFy { get; init; }
    public required double DistributedLoadFz { get; init; }
    public required double PointSuspensionForceFz { get; init; }
    public required double DistributedSuspensionForceFz { get; init; }
    public required double SuspensionForceFz { get; init; }
    public required double AppliedTorque { get; init; }
    public required double StartReactionTx { get; init; }
    public required double EndReactionTx { get; init; }
    public double Nx => 0;
    public required IReadOnlyList<BeamForcePoint> My { get; init; }
    public required IReadOnlyList<BeamForcePoint> Vz { get; init; }
    public required IReadOnlyList<BeamForcePoint> Mz { get; init; }
    public required IReadOnlyList<BeamForcePoint> Vy { get; init; }
    public required IReadOnlyList<BeamForcePoint> Tx { get; init; }
    public IReadOnlyList<BeamForcePoint> Mres => BeamPositionResultService.CreateResultantMoment(this);
    public IReadOnlyList<BeamForcePoint> Vres => BeamPositionResultService.CreateResultantShear(this);
    public required IReadOnlyList<BeamForcePoint> DeflectionY { get; init; }
    public required IReadOnlyList<BeamForcePoint> DeflectionZ { get; init; }
    public required IReadOnlyList<BeamForcePoint> RotationY { get; init; }
    public required IReadOnlyList<BeamForcePoint> RotationZ { get; init; }
    public required IReadOnlyList<BeamForcePoint> RotationX { get; init; }
    public double MaximumAbsoluteDeflection => DeflectionZ.Count == 0
        ? 0
        : DeflectionZ.Max(point => Math.Abs(point.Value));
}

public static class SimpleBeamForceModel
{
    private const double MillimetresPerMetre = 1_000;

    public static SimpleBeamForceResult Calculate(SimpleBeamForceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Validate(input);

        var startSupportPosition = input.StartOverhang;
        var endSupportPosition = input.Length - input.EndOverhang;
        var supportSpan = endSupportPosition - startSupportPosition;
        var pointLoads = input.EffectivePointLoads;
        var distributedLoads = input.EffectiveDistributedLoads;
        var distributedLoadFz = distributedLoads.Sum(load => CalculateTotalLoad(load));
        var totalPointLoadFz = pointLoads.Sum(load => load.ForceFz);
        var startReactionFz =
            (pointLoads.Sum(load => load.ForceFz * (endSupportPosition - load.Position))
             + distributedLoads.Sum(load => CalculateTotalLoad(load)
                 * (endSupportPosition - CalculateCentroid(load)))) / supportSpan;
        var endReactionFz = totalPointLoadFz + distributedLoadFz - startReactionFz;
        var distributedLoadFy = distributedLoads.Sum(load => CalculateTotalLoad(load, item => item.LoadQy));
        var totalPointLoadFy = pointLoads.Sum(load => load.ForceFy);
        var startReactionFy =
            (pointLoads.Sum(load => load.ForceFy * (endSupportPosition - load.Position))
             + distributedLoads.Sum(load => CalculateTotalLoad(load, item => item.LoadQy)
                 * (endSupportPosition - CalculateCentroid(load)))) / supportSpan;
        var endReactionFy = totalPointLoadFy + distributedLoadFy - startReactionFy;
        var pointTorque = pointLoads.Sum(load =>
            CalculateTorque(load.ForceFy, load.ForceFz, load.EccentricityY, load.EccentricityZ));
        var distributedTorque = distributedLoads.Sum(load =>
            CalculateDistributedTorque(load));
        var appliedTorque = pointTorque + distributedTorque;
        // Torsie: een moment tussen de opleggingen verdeelt zich volgens de hefboomregel;
        // een moment op een overstek gaat volledig naar de dichtstbijzijnde oplegging.
        var startReactionTx =
            pointLoads.Sum(load => CalculateTorque(
                    load.ForceFy, load.ForceFz, load.EccentricityY, load.EccentricityZ)
                * StartSupportTorqueShare(load.Position, startSupportPosition, endSupportPosition))
            + distributedLoads.Sum(load => CalculateDistributedTorqueIntensity(load)
                * StartSupportTorqueLength(load, startSupportPosition, endSupportPosition)
                / MillimetresPerMetre);
        var endReactionTx = appliedTorque - startReactionTx;
        var diagramPositions = CreateDiagramPositions(
            input, pointLoads, distributedLoads, startSupportPosition, endSupportPosition);
        var shearEvents = new List<(double Position, double Jump)>
        {
            (startSupportPosition, startReactionFz)
        };
        shearEvents.AddRange(pointLoads.Select(load => (load.Position, -load.ForceFz)));
        shearEvents.Add((endSupportPosition, endReactionFz));
        var sampledShear = CreateDistributedDiagram(
            diagramPositions,
            distributedLoads,
            load => load.LoadQz,
            shearEvents);
        diagramPositions = AddContinuousZeroCrossings(diagramPositions, sampledShear);
        var shearYEvents = new List<(double Position, double Jump)>
        {
            (startSupportPosition, startReactionFy)
        };
        shearYEvents.AddRange(pointLoads.Select(load => (load.Position, -load.ForceFy)));
        shearYEvents.Add((endSupportPosition, endReactionFy));
        var sampledShearY = CreateDistributedDiagram(
            diagramPositions,
            distributedLoads,
            load => load.LoadQy,
            shearYEvents);
        diagramPositions = AddContinuousZeroCrossings(diagramPositions, sampledShearY);
        var deflectionZ = CalculateDeflection(
            input,
            pointLoads,
            distributedLoads,
            startSupportPosition,
            endSupportPosition,
            startReactionFz,
            endReactionFz,
            input.SecondMomentAreaY,
            load => load.ForceFz,
            load => load.LoadQz);
        var deflectionY = CalculateDeflection(
            input,
            pointLoads,
            distributedLoads,
            startSupportPosition,
            endSupportPosition,
            startReactionFy,
            endReactionFy,
            input.SecondMomentAreaZ,
            load => load.ForceFy,
            load => load.LoadQy);
        var rotationY = CalculateRotationY(
            input,
            pointLoads,
            distributedLoads,
            startSupportPosition,
            endSupportPosition,
            startReactionFz,
            endReactionFz,
            input.SecondMomentAreaY,
            load => load.ForceFz,
            load => load.LoadQz);
        var rotationZ = CalculateRotationY(
            input,
            pointLoads,
            distributedLoads,
            startSupportPosition,
            endSupportPosition,
            startReactionFy,
            endReactionFy,
            input.SecondMomentAreaZ,
            load => load.ForceFy,
            load => load.LoadQy);
        var pointSuspensionForce = pointLoads.Sum(load => load.SuspensionFactor * load.ForceFz);
        var distributedSuspensionForce = distributedLoads.Sum(load =>
            load.SuspensionFactor * CalculateTotalLoad(load));

        var torque = CreateDistributedDiagram(
            diagramPositions,
            distributedLoads,
            CalculateDistributedTorqueIntensity,
            [(startSupportPosition, startReactionTx),
             .. pointLoads.Select(load =>
                 (load.Position, -CalculateTorque(
                     load.ForceFy, load.ForceFz, load.EccentricityY, load.EccentricityZ))),
             (endSupportPosition, endReactionTx)]);

        return new SimpleBeamForceResult
        {
            StartSupportPosition = startSupportPosition,
            EndSupportPosition = endSupportPosition,
            StartReactionFz = startReactionFz,
            EndReactionFz = endReactionFz,
            StartReactionFy = startReactionFy,
            EndReactionFy = endReactionFy,
            DistributedLoadFz = distributedLoadFz,
            PointSuspensionForceFz = pointSuspensionForce,
            DistributedSuspensionForceFz = distributedSuspensionForce,
            SuspensionForceFz = pointSuspensionForce + distributedSuspensionForce,
            AppliedTorque = appliedTorque,
            StartReactionTx = startReactionTx,
            EndReactionTx = endReactionTx,
            My = diagramPositions.Select(position => new BeamForcePoint(
                position,
                CalculateMoment(position, startSupportPosition, endSupportPosition,
                    pointLoads, distributedLoads, startReactionFz, endReactionFz))).ToArray(),
            Vz = CreateDistributedDiagram(
                diagramPositions,
                distributedLoads,
                load => load.LoadQz,
                shearEvents),
            Mz = diagramPositions.Select(position => new BeamForcePoint(
                position,
                CalculateMoment(position, startSupportPosition, endSupportPosition,
                    pointLoads, distributedLoads, startReactionFy, endReactionFy,
                    load => load.ForceFy, load => load.LoadQy, 1))).ToArray(),
            Vy = CreateDistributedDiagram(
                diagramPositions,
                distributedLoads,
                load => load.LoadQy,
                shearYEvents),
            Tx = torque,
            DeflectionY = deflectionY,
            DeflectionZ = deflectionZ,
            RotationY = rotationY,
            RotationZ = rotationZ,
            RotationX = CalculateRotationX(
                input,
                torque,
                startSupportPosition,
                endSupportPosition)
        };
    }

    private static double CalculateMoment(
        double position,
        double startSupportPosition,
        double endSupportPosition,
        IReadOnlyList<SimpleBeamPointLoadInput> pointLoads,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        double startReaction,
        double endReaction,
        Func<SimpleBeamPointLoadInput, double>? pointLoadSelector = null,
        Func<SimpleBeamDistributedLoadInput, double>? distributedLoadSelector = null,
        double sign = -1)
    {
        pointLoadSelector ??= load => load.ForceFz;
        distributedLoadSelector ??= load => load.LoadQz;
        var distributedMoment = distributedLoads.Sum(load =>
        {
            var loadedLength = Math.Clamp(
                position - load.StartPosition,
                0,
                load.EndPosition - load.StartPosition);
            return distributedLoadSelector(load)
                * loadedLength / MillimetresPerMetre
                * (position - load.StartPosition - loadedLength / 2)
                / MillimetresPerMetre;
        });

        return sign * (
            (startReaction * Math.Max(0, position - startSupportPosition)
             + endReaction * Math.Max(0, position - endSupportPosition)
             - pointLoads.Sum(load => pointLoadSelector(load) * Math.Max(0, position - load.Position)))
            / MillimetresPerMetre
            - distributedMoment);
    }

    private static IReadOnlyList<BeamForcePoint> CreateDistributedDiagram(
        IReadOnlyList<double> positions,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        Func<SimpleBeamDistributedLoadInput, double> intensitySelector,
        IReadOnlyList<(double Position, double Jump)> events)
    {
        var points = new List<BeamForcePoint>();
        foreach (var position in positions)
        {
            var continuousLoad = distributedLoads.Sum(load => intensitySelector(load)
                * Math.Clamp(position - load.StartPosition, 0, load.EndPosition - load.StartPosition)
                / MillimetresPerMetre);
            var valueBefore = events.Where(item => item.Position < position).Sum(item => item.Jump) - continuousLoad;
            var eventsAtPosition = events.Where(item => item.Position == position).Sum(item => item.Jump);

            if (eventsAtPosition != 0)
            {
                points.Add(new BeamForcePoint(position, Normalize(valueBefore)));
                points.Add(new BeamForcePoint(position, Normalize(valueBefore + eventsAtPosition)));
            }
            else
            {
                points.Add(new BeamForcePoint(position, Normalize(valueBefore)));
            }
        }

        return points;
    }

    private static double[] AddContinuousZeroCrossings(
        IReadOnlyList<double> positions,
        IReadOnlyList<BeamForcePoint> shear)
    {
        var result = positions.ToList();
        for (var index = 1; index < shear.Count; index++)
        {
            var before = shear[index - 1];
            var after = shear[index];
            var distance = after.Position - before.Position;
            if (distance <= 1e-9 || before.Value * after.Value >= 0)
            {
                continue;
            }

            var zeroPosition = before.Position
                + distance * -before.Value / (after.Value - before.Value);
            if (zeroPosition > before.Position + 1e-9
                && zeroPosition < after.Position - 1e-9)
            {
                result.Add(zeroPosition);
            }
        }

        return result.Distinct().Order().ToArray();
    }

    private static double[] CreateDiagramPositions(
        SimpleBeamForceInput input,
        IReadOnlyList<SimpleBeamPointLoadInput> pointLoads,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        double startSupportPosition,
        double endSupportPosition)
    {
        var positions = new List<double>
        {
            0,
            startSupportPosition,
            endSupportPosition,
            input.Length
        };
        positions.AddRange(pointLoads.Select(load => load.Position));
        positions.AddRange(distributedLoads.SelectMany(load =>
            new[] { load.StartPosition, load.EndPosition }));

        foreach (var load in distributedLoads.Where(load =>
                     (load.LoadQz != 0 || load.LoadQy != 0) && load.EndPosition > load.StartPosition))
        {
            const int segmentCount = 32;
            var segmentLength = (load.EndPosition - load.StartPosition) / segmentCount;
            positions.AddRange(Enumerable.Range(1, segmentCount - 1)
                .Select(index => load.StartPosition + index * segmentLength));
        }

        return positions.Distinct().Order().ToArray();
    }

    private static IReadOnlyList<BeamForcePoint> CalculateDeflection(
        SimpleBeamForceInput input,
        IReadOnlyList<SimpleBeamPointLoadInput> pointLoads,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        double startSupportPosition,
        double endSupportPosition,
        double startReaction,
        double endReaction,
        double? secondMomentArea,
        Func<SimpleBeamPointLoadInput, double> forceSelector,
        Func<SimpleBeamDistributedLoadInput, double> loadSelector)
    {
        if (input.ElasticModulus is null || secondMomentArea is null)
        {
            return [];
        }

        var flexuralRigidity = input.ElasticModulus.Value * secondMomentArea.Value;
        double Particular(double position) =>
            (startReaction * MillimetresPerMetre * Macaulay(position - startSupportPosition, 3) / 6
             + endReaction * MillimetresPerMetre * Macaulay(position - endSupportPosition, 3) / 6
             - pointLoads.Sum(load => forceSelector(load) * MillimetresPerMetre
                 * Macaulay(position - load.Position, 3) / 6)
             - distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.StartPosition, 4) / 24)
             + distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.EndPosition, 4) / 24))
            / flexuralRigidity;

        var slopeConstant = -(Particular(endSupportPosition) - Particular(startSupportPosition))
            / (endSupportPosition - startSupportPosition);
        var displacementConstant = -Particular(startSupportPosition) - slopeConstant * startSupportPosition;
        var positions = CreateDeformationPositions(
            input, pointLoads, distributedLoads, startSupportPosition, endSupportPosition);

        return positions.Select(position => new BeamForcePoint(
            position,
            Normalize(Particular(position) + slopeConstant * position + displacementConstant))).ToArray();
    }

    private static IReadOnlyList<BeamForcePoint> CalculateRotationY(
        SimpleBeamForceInput input,
        IReadOnlyList<SimpleBeamPointLoadInput> pointLoads,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        double startSupportPosition,
        double endSupportPosition,
        double startReaction,
        double endReaction,
        double? secondMomentArea,
        Func<SimpleBeamPointLoadInput, double> forceSelector,
        Func<SimpleBeamDistributedLoadInput, double> loadSelector)
    {
        if (input.ElasticModulus is null || secondMomentArea is null)
        {
            return [];
        }

        var flexuralRigidity = input.ElasticModulus.Value * secondMomentArea.Value;
        double Particular(double position) =>
            (startReaction * MillimetresPerMetre * Macaulay(position - startSupportPosition, 3) / 6
             + endReaction * MillimetresPerMetre * Macaulay(position - endSupportPosition, 3) / 6
             - pointLoads.Sum(load => forceSelector(load) * MillimetresPerMetre
                 * Macaulay(position - load.Position, 3) / 6)
             - distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.StartPosition, 4) / 24)
             + distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.EndPosition, 4) / 24))
            / flexuralRigidity;
        double ParticularRotation(double position) =>
            (startReaction * MillimetresPerMetre * Macaulay(position - startSupportPosition, 2) / 2
             + endReaction * MillimetresPerMetre * Macaulay(position - endSupportPosition, 2) / 2
             - pointLoads.Sum(load => forceSelector(load) * MillimetresPerMetre
                 * Macaulay(position - load.Position, 2) / 2)
             - distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.StartPosition, 3) / 6)
             + distributedLoads.Sum(load => loadSelector(load)
                 * Macaulay(position - load.EndPosition, 3) / 6))
            / flexuralRigidity;

        var slopeConstant = -(Particular(endSupportPosition) - Particular(startSupportPosition))
            / (endSupportPosition - startSupportPosition);

        return CreateDeformationPositions(
                input, pointLoads, distributedLoads, startSupportPosition, endSupportPosition)
            .Select(position => new BeamForcePoint(
                position,
                Normalize(ParticularRotation(position) + slopeConstant)))
            .ToArray();
    }

    private static IReadOnlyList<BeamForcePoint> CalculateRotationX(
        SimpleBeamForceInput input,
        IReadOnlyList<BeamForcePoint> torque,
        double startSupportPosition,
        double endSupportPosition)
    {
        if (input.ShearModulus is null || input.TorsionConstant is null || torque.Count == 0)
        {
            return [];
        }

        var torsionalRigidity = input.ShearModulus.Value * input.TorsionConstant.Value;
        var rotations = new BeamForcePoint[torque.Count];
        rotations[0] = new BeamForcePoint(torque[0].Position, 0);

        for (var index = 1; index < torque.Count; index++)
        {
            var previousTorque = torque[index - 1];
            var currentTorque = torque[index];
            var segmentLength = currentTorque.Position - previousTorque.Position;
            var averageTorqueNmm = (previousTorque.Value + currentTorque.Value) / 2 * 1_000_000;
            var rotation = rotations[index - 1].Value + averageTorqueNmm * segmentLength / torsionalRigidity;
            rotations[index] = new BeamForcePoint(currentTorque.Position, Normalize(rotation));
        }

        var supportRotation = rotations.First(point => point.Position == startSupportPosition).Value;
        return rotations.Select(point => new BeamForcePoint(
            point.Position,
            point.Position == startSupportPosition || point.Position == endSupportPosition
                ? 0
                : Normalize(point.Value - supportRotation))).ToArray();
    }

    private static IReadOnlyList<double> CreateDeformationPositions(
        SimpleBeamForceInput input,
        IReadOnlyList<SimpleBeamPointLoadInput> pointLoads,
        IReadOnlyList<SimpleBeamDistributedLoadInput> distributedLoads,
        double startSupportPosition,
        double endSupportPosition)
    {
        var positions = Enumerable.Range(0, 81)
            .Select(index => input.Length * index / 80)
            .Append(startSupportPosition)
            .Append(endSupportPosition)
            .Concat(pointLoads.Select(load => load.Position))
            .Concat(distributedLoads.SelectMany(load =>
                new[] { load.StartPosition, load.EndPosition }))
            .Distinct()
            .Order()
            .ToArray();

        return positions;
    }

    private static double CalculateTotalLoad(
        SimpleBeamDistributedLoadInput load,
        Func<SimpleBeamDistributedLoadInput, double>? intensitySelector = null) =>
        (intensitySelector?.Invoke(load) ?? load.LoadQz)
        * (load.EndPosition - load.StartPosition)
        / MillimetresPerMetre;

    private static double CalculateCentroid(SimpleBeamDistributedLoadInput load) =>
        load.StartPosition + (load.EndPosition - load.StartPosition) / 2;

    private static double CalculateTorque(
        double forceY,
        double forceZ,
        double eccentricityY,
        double eccentricityZ) =>
        (eccentricityY * forceZ - eccentricityZ * forceY) / MillimetresPerMetre;

    private static double CalculateDistributedTorque(SimpleBeamDistributedLoadInput load) =>
        CalculateDistributedTorqueIntensity(load)
        * (load.EndPosition - load.StartPosition)
        / MillimetresPerMetre;

    /// <summary>Aandeel van een torsiemoment op <paramref name="position"/> dat de beginoplegging opneemt.</summary>
    private static double StartSupportTorqueShare(double position, double startSupport, double endSupport)
    {
        if (position <= startSupport)
        {
            return 1;
        }

        if (position >= endSupport)
        {
            return 0;
        }

        return (endSupport - position) / (endSupport - startSupport);
    }

    /// <summary>
    /// Integraal van <see cref="StartSupportTorqueShare"/> over de lengte van een verdeelde last [mm]:
    /// het deel op de linkeroverstek telt volledig, het deel in de overspanning lineair, het deel
    /// op de rechteroverstek niet.
    /// </summary>
    private static double StartSupportTorqueLength(
        SimpleBeamDistributedLoadInput load,
        double startSupport,
        double endSupport)
    {
        var leftOverhang = Math.Max(0, Math.Min(load.EndPosition, startSupport) - load.StartPosition);
        var spanStart = Math.Max(load.StartPosition, startSupport);
        var spanEnd = Math.Min(load.EndPosition, endSupport);
        var inSpan = spanEnd > spanStart
            ? (Math.Pow(endSupport - spanStart, 2) - Math.Pow(endSupport - spanEnd, 2))
              / (2 * (endSupport - startSupport))
            : 0;

        return leftOverhang + inSpan;
    }

    private static double CalculateDistributedTorqueIntensity(SimpleBeamDistributedLoadInput load) =>
        CalculateTorque(load.LoadQy, load.LoadQz, load.EccentricityY, load.EccentricityZ);

    private static double Macaulay(double value, int power) => value <= 0 ? 0 : Math.Pow(value, power);

    private static double Normalize(double value) => Math.Abs(value) < 1e-12 ? 0 : value;

    private static void Validate(SimpleBeamForceInput input)
    {
        if (!double.IsFinite(input.Length) || input.Length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "De liggerlengte moet groter zijn dan nul.");
        }

        foreach (var load in input.EffectivePointLoads)
        {
            if (!double.IsFinite(load.ForceFz))
            {
                throw new ArgumentOutOfRangeException(nameof(input), "Fz moet een eindig getal zijn.");
            }

            if (!double.IsFinite(load.Position) || load.Position < 0 || load.Position > input.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(input), "Positie a moet tussen 0 en L liggen.");
            }

            if (!double.IsFinite(load.EccentricityY) || !double.IsFinite(load.EccentricityZ))
            {
                throw new ArgumentOutOfRangeException(nameof(input), "Excentriciteit e moet een eindig getal zijn.");
            }

            if (!double.IsFinite(load.SuspensionFactor) || load.SuspensionFactor < 0 || load.SuspensionFactor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(input), "Factor fz moet tussen 0,0 en 1,0 liggen.");
            }
        }

        if (!double.IsFinite(input.StartOverhang) || input.StartOverhang < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Overstek begin moet nul of groter zijn.");
        }

        if (!double.IsFinite(input.EndOverhang) || input.EndOverhang < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Overstek eind moet nul of groter zijn.");
        }

        if (input.StartOverhang + input.EndOverhang >= input.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "De som van de oversteken moet kleiner zijn dan L.");
        }

        foreach (var load in input.EffectiveDistributedLoads)
        {
            if (!double.IsFinite(load.LoadQz))
            {
                throw new ArgumentOutOfRangeException(nameof(input), "qz moet een eindig getal zijn.");
            }

            if (!double.IsFinite(load.StartPosition)
                || !double.IsFinite(load.EndPosition)
                || load.StartPosition < 0
                || load.EndPosition < load.StartPosition
                || load.EndPosition > input.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(input), "De lijnlastposities moeten voldoen aan 0 ≤ posA ≤ posB ≤ L.");
            }

            if (!double.IsFinite(load.EccentricityY) || !double.IsFinite(load.EccentricityZ))
            {
                throw new ArgumentOutOfRangeException(nameof(input), "De excentriciteit van qz moet een eindig getal zijn.");
            }

            if (!double.IsFinite(load.SuspensionFactor)
                || load.SuspensionFactor < 0
                || load.SuspensionFactor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(input), "De ophangfactor van qz moet tussen 0,0 en 1,0 liggen.");
            }
        }

        var hasSecondMomentArea = input.SecondMomentAreaY.HasValue || input.SecondMomentAreaZ.HasValue;
        if (input.ElasticModulus.HasValue != hasSecondMomentArea
            || input.ElasticModulus is <= 0
            || input.SecondMomentAreaY is <= 0
            || input.SecondMomentAreaZ is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "E en Iy en/of Iz moeten samen worden opgegeven en groter zijn dan nul.");
        }

        if (input.ShearModulus.HasValue != input.TorsionConstant.HasValue
            || input.ShearModulus is <= 0
            || input.TorsionConstant is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "G en It moeten samen worden opgegeven en groter zijn dan nul.");
        }
    }
}
