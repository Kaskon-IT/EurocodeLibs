using CommonLibrary;
using CommonLibrary.Mechanica;
using CommonLibrary.Models;
using Profielen.Staal;

namespace Eurocode.StaalConstructies;

public sealed class SteelBeamFieldCheckResult
{
    public required BeamFieldDefinition Field { get; init; }
    public required BeamFieldExtreme MaximumMy { get; init; }
    public required BeamFieldExtreme MaximumVz { get; init; }
    public required BeamFieldExtreme MaximumTx { get; init; }
    public required BeamFieldExtreme MaximumDeflection { get; init; }
    public required IReadOnlyList<BeamSectionCheck> Checks { get; init; }
}

public static class SteelBeamSectionCheckService
{
    public static IReadOnlyList<BeamSectionCheck> CheckAtPosition(
        BeamPositionResult positionResult,
        StaalProfiel profile,
        StaalContext material)
    {
        ArgumentNullException.ThrowIfNull(positionResult);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(material);

        var myResult = new BendingMyToets(profile is ProfielIH ihProfile ? ihProfile.KlasseBuiging : 1)
        {
            Positie = FormatPosition(positionResult.Position)
        }.Check(new InternalForces { My = positionResult.My }, profile, material);
        var vzResult = new ShearVzCheck
        {
            Positie = FormatPosition(positionResult.Position)
        }.Check(new InternalForces { Vz = positionResult.Vz }, profile, material);
        var torsionResult = new TorsionCheck
        {
            Positie = FormatPosition(positionResult.Position)
        }.Check(new InternalForces { T = positionResult.Tx }, profile, material);
        var combinedResult = new CombinedTorsionAndShearVzCheck
        {
            Positie = FormatPosition(positionResult.Position)
        }.Check(new InternalForces { Vz = positionResult.Vz, T = positionResult.Tx }, profile, material);

        return
        [
            FromEurocodeResult("Buiging My", positionResult.Position, myResult),
            FromEurocodeResult("Dwarskracht Vz", positionResult.Position, vzResult),
            FromEurocodeResult("Torsie Tx", positionResult.Position, torsionResult),
            FromEurocodeResult("Torsie en dwarskracht Vz", positionResult.Position, combinedResult)
        ];
    }

    public static IReadOnlyList<SteelBeamFieldCheckResult> Check(
        SimpleBeamForceResult forces,
        double totalLength,
        StaalProfiel profile,
        StaalContext material,
        double deflectionLimitDivisor)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(material);
        if (!double.IsFinite(deflectionLimitDivisor) || deflectionLimitDivisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deflectionLimitDivisor));
        }

        return FindFieldExtremes(forces, totalLength)
            .Select(fieldResult => AddChecks(fieldResult, profile, material, deflectionLimitDivisor))
            .ToArray();
    }

    public static IReadOnlyList<SteelBeamFieldCheckResult> FindFieldExtremes(
        SimpleBeamForceResult forces,
        double totalLength)
    {
        ArgumentNullException.ThrowIfNull(forces);
        if (!double.IsFinite(totalLength) || totalLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalLength));
        }

        return CreateFields(forces, totalLength)
            .Select(field => new SteelBeamFieldCheckResult
            {
                Field = field,
                MaximumMy = FindExtreme(forces.My, field, "My", "kNm"),
                MaximumVz = FindExtreme(forces.Vz, field, "Vz", "kN"),
                MaximumTx = FindExtreme(forces.Tx, field, "Tx", "kNm"),
                MaximumDeflection = FindExtreme(forces.DeflectionZ, field, "uz", "mm"),
                Checks = []
            })
            .ToArray();
    }

    private static IReadOnlyList<BeamFieldDefinition> CreateFields(
        SimpleBeamForceResult forces,
        double totalLength)
    {
        var fields = new List<BeamFieldDefinition>();
        if (forces.StartSupportPosition > 0)
        {
            fields.Add(new BeamFieldDefinition(
                BeamFieldType.StartOverhang,
                "Overstek begin",
                0,
                forces.StartSupportPosition));
        }

        fields.Add(new BeamFieldDefinition(
            BeamFieldType.MainSpan,
            "Veld tussen steunpunten",
            forces.StartSupportPosition,
            forces.EndSupportPosition));

        if (forces.EndSupportPosition < totalLength)
        {
            fields.Add(new BeamFieldDefinition(
                BeamFieldType.EndOverhang,
                "Overstek eind",
                forces.EndSupportPosition,
                totalLength));
        }

        return fields;
    }

    private static BeamFieldExtreme FindExtreme(
        IReadOnlyList<BeamForcePoint> points,
        BeamFieldDefinition field,
        string component,
        string unit)
    {
        if (points.Count == 0)
        {
            return new BeamFieldExtreme(component, field.StartPosition, 0, unit);
        }

        var candidates = points
            .Where(point => point.Position >= field.StartPosition && point.Position <= field.EndPosition)
            .GroupBy(point => point.Position)
            .SelectMany(group =>
            {
                if (group.Key == field.StartPosition)
                {
                    return [group.Last()];
                }

                if (group.Key == field.EndPosition)
                {
                    return [group.First()];
                }

                return group;
            })
            .ToArray();

        if (candidates.Length == 0)
        {
            return new BeamFieldExtreme(component, field.StartPosition, 0, unit);
        }

        var extreme = candidates.MaxBy(point => Math.Abs(point.Value))!;
        return new BeamFieldExtreme(component, extreme.Position, extreme.Value, unit);
    }

    private static SteelBeamFieldCheckResult AddChecks(
        SteelBeamFieldCheckResult fieldResult,
        StaalProfiel profile,
        StaalContext material,
        double deflectionLimitDivisor)
    {
        var myForces = new InternalForces { My = fieldResult.MaximumMy.Value };
        var sectionClass = profile is ProfielIH ihProfile ? ihProfile.KlasseBuiging : 1;
        var myResult = new BendingMyToets(sectionClass)
        {
            Positie = FormatPosition(fieldResult.MaximumMy.Position)
        }.Check(myForces, profile, material);

        var vzForces = new InternalForces { Vz = fieldResult.MaximumVz.Value };
        var vzResult = new ShearVzCheck
        {
            Positie = FormatPosition(fieldResult.MaximumVz.Position)
        }.Check(vzForces, profile, material);

        var torsionForces = new InternalForces { T = fieldResult.MaximumTx.Value };
        var torsionResult = new TorsionCheck
        {
            Positie = FormatPosition(fieldResult.MaximumTx.Position)
        }.Check(torsionForces, profile, material);

        var combinedForces = new InternalForces
        {
            Vz = fieldResult.MaximumVz.Value,
            T = fieldResult.MaximumTx.Value
        };
        var combinedResult = new CombinedTorsionAndShearVzCheck
        {
            Positie = FormatField(fieldResult.Field)
        }.Check(combinedForces, profile, material);

        var deflectionCheck = CreateDeflectionCheck(
            fieldResult.MaximumDeflection,
            fieldResult.Field,
            deflectionLimitDivisor);
        var myCheck = FromEurocodeResult("Buiging My", fieldResult.MaximumMy.Position, myResult);
        var vzCheck = FromEurocodeResult("Dwarskracht Vz", fieldResult.MaximumVz.Position, vzResult);
        var torsionCheck = FromEurocodeResult("Torsie Tx", fieldResult.MaximumTx.Position, torsionResult);
        var combinedCheck = FromEurocodeResult(
            "Torsie en dwarskracht Vz",
            fieldResult.MaximumVz.Position,
            combinedResult);

        return new SteelBeamFieldCheckResult
        {
            Field = fieldResult.Field,
            MaximumMy = fieldResult.MaximumMy,
            MaximumVz = fieldResult.MaximumVz,
            MaximumTx = fieldResult.MaximumTx,
            MaximumDeflection = fieldResult.MaximumDeflection,
            Checks = [myCheck, vzCheck, torsionCheck, combinedCheck, deflectionCheck]
        };
    }

    private static BeamSectionCheck FromEurocodeResult(
        string name,
        double position,
        EurocodeResultaat result) => new(
            name,
            result.Norm,
            result.Artikel,
            result.Formule,
            position,
            result.Waarde,
            result.Toelaatbaar,
            result.Unit,
            result.Benutting,
            result.Toelaatbaar > 0,
            result.Toelichting,
            result);

    private static BeamSectionCheck CreateDeflectionCheck(
        BeamFieldExtreme extreme,
        BeamFieldDefinition field,
        double divisor)
    {
        var allowable = field.Length / divisor;
        var demand = Math.Abs(extreme.Value);
        return new BeamSectionCheck(
            "Zakking uz",
            "BGT",
            "Instelbare grenswaarde",
            $"Lveld/{divisor:0}",
            extreme.Position,
            demand,
            allowable,
            "mm",
            allowable > 0 ? demand / allowable : double.PositiveInfinity,
            allowable > 0,
            "Gebaseerd op de absolute verplaatsing binnen het veld.");
    }

    private static string FormatPosition(double position) => $"x = {position:0.##} mm";

    private static string FormatField(BeamFieldDefinition field) =>
        $"x = {field.StartPosition:0.##}–{field.EndPosition:0.##} mm";
}
