using CommonLibrary;
using CommonLibrary.Models;
using CommonLibrary.Mechanica;
using CommonLibrary.Modelling;
using Eurocode.Belastingen;
using Profielen.Beton;
using Profielen.Parametrisch;
using ExportFactory.Shared;

namespace Eurocode.BetonConstructies;

public enum ConcreteCriticalSectionType
{
    MaximumAbsoluteShear,
    MaximumMoment,
    MinimumMoment
}

public sealed record ConcreteCriticalSection(
    ConcreteCriticalSectionType Type, // waarvoor nodig dan?
    string Name,
    double Position,
    double My,
    double Vz,
    double Tx);

public sealed class ConcreteBeamSectionCheckResult
{
    public required ConcreteCriticalSection Section { get; init; }
    public required string ReinforcementFace { get; init; }
    public required IReadOnlyList<BeamSectionCheck> Checks { get; init; }
}

public sealed class ConcreteMomentTorsionDetail
{
    [TableColumn(Label = "Buigingsberekening")]
    public required BendingResults Bending { get; init; }

    [TableColumn(Label = "Torsieberekening")]
    public required J3ConsoleTorsieResult Torsion { get; init; }

    [TableColumn(Label = "Benodigde buigwapening", Symbol = "A~s,M,req~", Unit = "mm²")]
    public required double RequiredBendingReinforcement { get; init; }

    [TableColumn(Label = "Benodigde torsiewapening", Symbol = "A~s,T,req~", Unit = "mm²")]
    public required double RequiredTorsionReinforcement { get; init; }

    [TableColumn(Label = "Benodigde totale wapening", Symbol = "A~s,req~", Unit = "mm²")]
    public required double RequiredTotalReinforcement { get; init; }

    [TableColumn(Label = "Aanwezige wapening", Symbol = "A~s,prov~", Unit = "mm²")]
    public required double SuppliedReinforcement { get; init; }

    [TableColumn(Label = "Benutting", Symbol = "η", Unit = "-")]
    public double Utilization => SuppliedReinforcement > 0
        ? RequiredTotalReinforcement / SuppliedReinforcement
        : double.PositiveInfinity;


    // Userfriendly summary of the check result
    [TableColumn(Label = "$M_{Ed}$", Unit = "kNm", Description = "Rekenwaarde moment")]
    public double M => Bending.Moment;
    
    [TableColumn(Label = "$A_{s,M,req}$", Unit = "mm²", Description = "Benodigde buigwapening")]
    public double AsMomentRequired => Bending.AsRequired;
    

    [TableColumn(Label = "$T_{Ed}$", Unit = "kNm", Description = "Rekenwaarde torsie")]
    public double T => Torsion.TEd;

    [TableColumn(Label = "$A_{s,T,req}$", Unit = "mm²", Description = "Benodigde torsiewapening")]
    public double AsTorsionRequired => Torsion.AslBovenOnder;


    public string Vlak => Bending.PosWapBovenOnder;

    public double AsTotalRequired => RequiredTotalReinforcement;


}

public static class ConcreteBeamSectionCheckService
{
    private const double ReinforcementSteelPartialFactor = 1.15;

    public static bool TryCreateRectangularProfile(BaseProfiel profile, out BetonProfiel concreteProfile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (profile is BetonProfiel existingConcreteProfile)
        {
            concreteProfile = existingConcreteProfile;
            return true;
        }

        if (profile is ParametrischProfielContext { Vorm: ParametrischeProfielVormEnum.Rechthoek })
        {
            concreteProfile = new BetonProfiel(profile.B, profile.H);
            return true;
        }

        concreteProfile = null!;
        return false;
    }

    public static IReadOnlyList<ConcreteBeamSectionCheckResult> Check(
        SimpleBeamForceResult forces,
        BetonProfiel profile,
        BetonContext material,
        WapeningGroep bottomReinforcement,
        WapeningGroep topReinforcement,
        WapeningGroep stirrups,
        double coverTop,
        double coverBottom,
        double coverSide,
        Func<double, ConcreteSectionContext>? sectionContextResolver = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(bottomReinforcement);
        ArgumentNullException.ThrowIfNull(topReinforcement);
        ArgumentNullException.ThrowIfNull(stirrups);

        ValidateInputs(profile, bottomReinforcement, topReinforcement, stirrups, coverTop, coverBottom, coverSide);

        return FindCriticalSections(forces)
            .Select(section => CheckSection(
                section,
                sectionContextResolver?.Invoke(section.Position)
                    ?? ConcreteSectionContextService.ResolveConcreteSectionContext(forces, section.Position, profile.H),
                profile,
                material,
                bottomReinforcement,
                topReinforcement,
                stirrups,
                coverTop,
                coverBottom,
                coverSide))
            .ToArray();
    }

    public static ConcreteBeamSectionCheckResult CheckAtPosition(
        BeamPositionResult positionResult,
        ConcreteSectionContext sectionContext,
        BetonProfiel profile,
        BetonContext material,
        WapeningGroep bottomReinforcement,
        WapeningGroep topReinforcement,
        WapeningGroep stirrups,
        double coverTop,
        double coverBottom,
        double coverSide)
    {
        ArgumentNullException.ThrowIfNull(positionResult);
        ValidateInputs(profile, bottomReinforcement, topReinforcement, stirrups, coverTop, coverBottom, coverSide);

        return CheckSection(
            new ConcreteCriticalSection(
                ConcreteCriticalSectionType.MaximumMoment,
                "Geselecteerde positie",
                positionResult.Position,
                positionResult.My,
                positionResult.Vz,
                positionResult.Tx),
            sectionContext,
            profile,
            material,
            bottomReinforcement,
            topReinforcement,
            stirrups,
            coverTop,
            coverBottom,
            coverSide);
    }

    public static IReadOnlyList<ConcreteCriticalSection> FindCriticalSections(SimpleBeamForceResult forces)
    {
        ArgumentNullException.ThrowIfNull(forces);

        var maximumAbsoluteShear = FindExtreme(forces.Vz, points => points.MaxBy(point => Math.Abs(point.Value)));
        var maximumMoment = FindExtreme(forces.My, points => points.MaxBy(point => point.Value));
        var minimumMoment = FindExtreme(forces.My, points => points.MinBy(point => point.Value));

        return
        [
            CreateSection(ConcreteCriticalSectionType.MaximumAbsoluteShear, "Grootste |Vz|", maximumAbsoluteShear, forces),
            CreateSection(ConcreteCriticalSectionType.MaximumMoment, "Grootste My", maximumMoment, forces),
            CreateSection(ConcreteCriticalSectionType.MinimumMoment, "Kleinste My", minimumMoment, forces)
        ];
    }

    private static BeamForcePoint FindExtreme(
        IReadOnlyList<BeamForcePoint> points,
        Func<IEnumerable<BeamForcePoint>, BeamForcePoint?> selector)
    {
        if (points.Count == 0)
        {
            return new BeamForcePoint(0, 0);
        }

        return selector(points) ?? new BeamForcePoint(0, 0);
    }

    private static ConcreteCriticalSection CreateSection(
        ConcreteCriticalSectionType type,
        string name,
        BeamForcePoint governingPoint,
        SimpleBeamForceResult forces)
    {
        var position = governingPoint.Position;
        return new ConcreteCriticalSection(
            type,
            name,
            position,
            type is ConcreteCriticalSectionType.MaximumMoment or ConcreteCriticalSectionType.MinimumMoment
                ? governingPoint.Value
                : ResolveValue(forces.My, position),
            type == ConcreteCriticalSectionType.MaximumAbsoluteShear
                ? governingPoint.Value
                : ResolveValue(forces.Vz, position),
            ResolveValue(forces.Tx, position));
    }

    private static double ResolveValue(
        IReadOnlyList<BeamForcePoint> points,
        double position)
    {
        var values = points
            .Where(point => Math.Abs(point.Position - position) < 1e-9)
            .Select(point => point.Value)
            .ToArray();
        if (values.Length == 0)
        {
            return Interpolate(points, position);
        }

        return values.MaxBy(Math.Abs);
    }

    private static double Interpolate(IReadOnlyList<BeamForcePoint> points, double position)
    {
        if (points.Count == 0)
        {
            return 0;
        }

        var lower = points.LastOrDefault(point => point.Position < position);
        var upper = points.FirstOrDefault(point => point.Position > position);
        if (lower is null)
        {
            return points[0].Value;
        }

        if (upper is null)
        {
            return points[^1].Value;
        }

        var fraction = (position - lower.Position) / (upper.Position - lower.Position);
        return lower.Value + fraction * (upper.Value - lower.Value);
    }

    private static ConcreteBeamSectionCheckResult CheckSection(
        ConcreteCriticalSection section,
        ConcreteSectionContext sectionContext,
        BetonProfiel profile,
        BetonContext material,
        WapeningGroep bottomReinforcement,
        WapeningGroep topReinforcement,
        WapeningGroep stirrups,
        double coverTop,
        double coverBottom,
        double coverSide)
    {
        var usesBottomReinforcement = section.My < 0;
        var longitudinalReinforcement = usesBottomReinforcement ? bottomReinforcement : topReinforcement;
        var oppositeReinforcement = usesBottomReinforcement ? topReinforcement : bottomReinforcement;
        var reinforcementFace = usesBottomReinforcement ? "onder" : "boven";
        var cover = usesBottomReinforcement ? coverBottom : coverTop;
        var appliedReinforcement = GetLongitudinalArea(longitudinalReinforcement);
        var reinforcementContext = new WapeningContext(
            $"{GetCount(longitudinalReinforcement):0.####}r{longitudinalReinforcement.Diameter:0.####}",
            cover + stirrups.Diameter + longitudinalReinforcement.Diameter / 2);
        
        var bending = new BendingResults(
            material,
            profile,
            reinforcementContext,
            new SectionForces { My = section.My });

        bending.IsGedrongenLigger = sectionContext.IsDeepBeamRegion;
        switch (sectionContext.StructuralSystem)
        {
            case ConcreteStructuralSystem.Cantilever:
                bending.Gedrongen = Schematisering.GedrongenEnum.Uitkraging;
                break;
            case ConcreteStructuralSystem.StaticallyDeterminate:
                bending.Gedrongen = Schematisering.GedrongenEnum.StatischBepaald;
                break;
            case ConcreteStructuralSystem.StaticallyIndeterminate:
                bending.Gedrongen = Schematisering.GedrongenEnum.StatischOnbepaald;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(sectionContext.StructuralSystem),
                    sectionContext.StructuralSystem,
                    "Onbekend structureel systeem.");
        }

        if (sectionContext.IsDeepBeamRegion)
        {
            bending.LengteMaatBijGedrongenLiggerInMM = sectionContext.DeepBeamLength!.Value;
        }

        bending.BerekenEnValideer();

        var effectiveDepth = profile.H - cover - stirrups.Diameter - longitudinalReinforcement.Diameter / 2;

        // NutHoogte is d; DwarskrachtWapContext rekent zelf z = 0,9·d en gebruikt d voor VRd,c, k en ρ1.
        var shearNew = new DwarskrachtWapContext()
        {
            BerekeningType = BerekeningTypeEnum.ControleerWapening,
            Beton = material,
            Profiel = profile,
            Snedekrachten = new SectionForces { Vz = section.Vz },
            AswToegepast = GetStirrupAreaPerMetre(stirrups),
            NutHoogte = effectiveDepth,
        };
        shearNew.BerekenEnValideer();

        var leverArm = 0.9 * effectiveDepth;
        var nu = 0.6 * (1 - material.Fck / 250);
        var shearInput = CreateCalculatorInput(
            profile,
            material,
            longitudinalReinforcement,
            oppositeReinforcement,
            stirrups,
            coverSide,
            Math.Abs(section.Vz),
            0);
        var shear = J3ConsoleDwarskrachtCalculator.Bereken(
            shearInput,
            effectiveDepth,
            profile.H,
            appliedReinforcement,
            leverArm,
            material.Fcd,
            nu);

        var torsionEccentricity = Math.Max(profile.B / 2, 1);
        var torsionForce = Math.Abs(section.Tx) * 1_000 / torsionEccentricity;
        var torsionInput = CreateCalculatorInput(
            profile,
            material,
            longitudinalReinforcement,
            oppositeReinforcement,
            stirrups,
            coverSide,
            torsionForce,
            torsionEccentricity);
        
        
        var torsion = J3ConsoleTorsieCalculator.Bereken(
            torsionInput,
            material,
            material.Fcd,
            nu,
            profile.H, cotTheta: 2.5);
        

        var combined = J3ConsoleTorsieCalculator.Combineer(torsion, shear);

        var suppliedStirrups = GetStirrupAreaPerMetre(stirrups);
        var requiredShearStirrups = shear.VEd <= shear.VRdc ? 0 : shear.AswV;
        var requiredTorsionStirrups = torsion.AswTPerLengte * 1_000;
        var requiredCombinedStirrups = requiredShearStirrups + requiredTorsionStirrups;
        var requiredCombinedLongitudinal = bending.AsRequired + torsion.AslBovenOnder;
        var momentTorsionDetail = new ConcreteMomentTorsionDetail
        {
            Bending = bending,
            Torsion = torsion,
            RequiredBendingReinforcement = bending.AsRequired,
            RequiredTorsionReinforcement = torsion.AslBovenOnder,
            RequiredTotalReinforcement = requiredCombinedLongitudinal,
            SuppliedReinforcement = appliedReinforcement
        };

        return new ConcreteBeamSectionCheckResult
        {
            Section = section,
            ReinforcementFace = reinforcementFace,
            Checks =
            [
                CreateCheck(
                    $"Buiging My ({bending.Moment:0.#} kNm)",
                    "6.1",
                    "A_{s,req} / A_{s_prov}",
                    section.Position,
                    bending.AsRequired,
                    appliedReinforcement,
                    "mm²",
                    $"Toets met {reinforcementFace}wapening.",
                    bending),
                //CreateCheck(
                //    "Dwarskracht Vz",
                //    "6.2",
                //    "Aₛw,V,req / Aₛw,prov",
                //    section.Position,
                //    requiredShearStirrups,
                //    suppliedStirrups,
                //    "mm²/m",
                //    $"VEd = {Math.Abs(section.Vz):0.##} kN; VRd,c = {shear.VRdc:0.##} kN.",
                //    shear),
                CreateCheck(
                    $"Dwarskracht Vz ({shearNew.Ved:0.#} kN)",
                    "6.2",
                    "...",
                    section.Position,
                    demand: shearNew.AswBenPerMeter,
                    resistance:shearNew.AswToegepast,
                    "mm²/m",
                    $"VRd = {shearNew.DwarskrachtWeerstand:0.##} kN",
                    shearNew),

                CreateCheck(
                    "Torsie Tx",
                    "6.3",
                    "Aₛw,T,req / Aₛw,prov",
                    section.Position,
                    requiredTorsionStirrups,
                    suppliedStirrups,
                    "mm²/m",
                    $"TEd = {Math.Abs(section.Tx):0.##} kNm; TRd,c = {torsion.TRdc:0.##} kNm.",
                    torsion),
                CreateCheck(
                    "Dwarskracht en torsie",
                    "6.3.2",
                    "VEd/VRd,c + TEd/TRd,c",
                    section.Position,
                    combined.UnityCheck,
                    1,
                    "-",
                    $"Benodigde totale beugelwapening: {requiredCombinedStirrups:0.##} mm²/m; aanwezig: {suppliedStirrups:0.##} mm²/m.",
                    combined),
                CreateCheck(
                    "Moment en torsie",
                    "6.1 en 6.3.2",
                    "(Aₛ,M,req + Aₛ,T,req) / Aₛ,prov",
                    section.Position,
                    requiredCombinedLongitudinal,
                    appliedReinforcement,
                    "mm²",
                    $"Torsie vraagt {torsion.AslBovenOnder:0.##} mm² extra {reinforcementFace}wapening.",
                    momentTorsionDetail)
            ]
        };
    }

    private static J3ConsoleInput CreateCalculatorInput(
        BetonProfiel profile,
        BetonContext material,
        WapeningGroep primaryReinforcement,
        WapeningGroep secondaryReinforcement,
        WapeningGroep stirrups,
        double cover,
        double force,
        double eccentricity) => new()
    {
        Bc = profile.B,
        Hc = profile.H,
        Ac = profile.A,
        FEd = force,
        HEd = eccentricity,
        ExcentriciteitBreedte = eccentricity,
        Dekking = cover,
        BeugelDiameter = stirrups.Diameter,
        HoofdstaafDiameter = primaryReinforcement.Diameter,
        HoofdstaafAantal = GetCount(primaryReinforcement),
        HoofdstaafDiameter2 = secondaryReinforcement.Diameter,
        HoofdstaafAantal2 = GetCount(secondaryReinforcement),
        Fck = material.Fck,
        AlphaCc = 1,
        GammaC = material.PartieleFactor,
        Fyk = material.BetonStaal.Fyk,
        GammaS = ReinforcementSteelPartialFactor,
        
    };

    private static BeamSectionCheck CreateCheck(
        string name,
        string article,
        string formula,
        double position,
        double demand,
        double resistance,
        string unit,
        string explanation,
        object? detailContext = null) => new(
            name,
            "EC2",
            article,
            formula,
            position,
            demand,
            resistance,
            unit,
            resistance > 0 ? demand / resistance : demand == 0 ? 0 : double.PositiveInfinity,
            resistance > 0,
            explanation,
            detailContext);

    private static double GetLongitudinalArea(WapeningGroep reinforcement) =>
        GetCount(reinforcement) * Math.PI * Math.Pow(reinforcement.Diameter, 2) / 4;

    private static double GetStirrupAreaPerMetre(WapeningGroep stirrups)
    {
        var spacing = stirrups.Verdeling.BeoogdeHartOpHart;
        if (spacing <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stirrups), "De beugelafstand moet groter zijn dan nul.");
        }

        const int legs = 2;
        return legs * Math.PI * Math.Pow(stirrups.Diameter, 2) / 4 * 1_000 / spacing;
    }

    private static double GetCount(WapeningGroep reinforcement) => reinforcement.Verdeling.Aantal;

    private static void ValidateInputs(
        BetonProfiel profile,
        WapeningGroep bottomReinforcement,
        WapeningGroep topReinforcement,
        WapeningGroep stirrups,
        double coverTop,
        double coverBottom,
        double coverSide)
    {
        if (profile.B <= 0 || profile.H <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        if (GetCount(bottomReinforcement) <= 0
            || GetCount(topReinforcement) <= 0
            || bottomReinforcement.Diameter <= 0
            || topReinforcement.Diameter <= 0
            || stirrups.Diameter <= 0)
        {
            throw new ArgumentException("Langs- en beugelwapening moeten aanwezig zijn.");
        }

        if (coverTop < 0 || coverBottom < 0 || coverSide < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(coverTop));
        }
    }
}
