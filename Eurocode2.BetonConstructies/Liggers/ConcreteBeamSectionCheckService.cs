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
    public required RechthoekWringingResult Torsion { get; init; }

    /// <summary>Zijde van de toegepaste langswapening: "onder" of "boven".</summary>
    public required string ReinforcementFace { get; init; }

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
    public double AsTorsionRequired => RequiredTorsionReinforcement;


    public string Vlak => Bending.PosWapBovenOnder;

    public double AsTotalRequired => RequiredTotalReinforcement;


}

public static class ConcreteBeamSectionCheckService
{
    private const int StirrupLegs = 2;

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
        var reinforcementFace = usesBottomReinforcement ? "onder" : "boven";
        var cover = usesBottomReinforcement ? coverBottom : coverTop;
        var appliedReinforcement = GetLongitudinalArea(longitudinalReinforcement);
        // ReferentieDekking is de dekking op de langsstaaf; WapeningContext telt zelf Ø/2 op
        var reinforcementContext = new WapeningContext(
            $"{GetCount(longitudinalReinforcement):0.####}r{longitudinalReinforcement.Diameter:0.####}",
            cover + stirrups.Diameter);
        
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
        var suppliedStirrups = GetStirrupAreaPerMetre(stirrups);

        // NutHoogte is d; DwarskrachtWapContext rekent zelf z = 0,9·d en gebruikt d voor VRd,c, k en ρ1.
        var shear = new DwarskrachtWapContext()
        {
            BerekeningType = BerekeningTypeEnum.ControleerWapening,
            Beton = material,
            Profiel = profile,
            Snedekrachten = new SectionForces { Vz = section.Vz },
            AswToegepast = suppliedStirrups,
            NutHoogte = effectiveDepth,
            AsLangs = appliedReinforcement,
        };
        shear.BerekenEnValideer();

        // Wringing 6.3.2 met dezelfde θ als de dwarskracht
        var shearReinforcementRequired = Math.Abs(shear.Ved) > shear.DwarskrachtWeerstandBeton ? shear.AswBerekend : 0;
        var torsion = RechthoekWringingCalculator.Bereken(new RechthoekWringingInput
        {
            Beton = material,
            Breedte = profile.B,
            Hoogte = profile.H,
            DekkingBoven = coverTop,
            DekkingOnder = coverBottom,
            DekkingZijkant = coverSide,
            BeugelDiameter = stirrups.Diameter,
            DiameterBoven = topReinforcement.Diameter,
            DiameterOnder = bottomReinforcement.Diameter,
            DiameterZijkant = Math.Max(topReinforcement.Diameter, bottomReinforcement.Diameter),
            NuttigeHoogte = effectiveDepth,
            TEd = section.Tx,
            VEd = section.Vz,
            VRdc = shear.DwarskrachtWeerstandBeton,
            CotTheta = Math.Clamp(shear.CotTheta, 1, 2.5),
            AswVPerMeter = shearReinforcementRequired,
            AswMinPerMeter = shear.AswMin,
            BeugelSneden = StirrupLegs,
        });

        var suppliedStirrupsPerLeg = suppliedStirrups / StirrupLegs;
        var torsionLongitudinal = usesBottomReinforcement ? torsion.AslOnderBenodigd : torsion.AslBovenBenodigd;
        var requiredCombinedLongitudinal = bending.AsRequired + torsionLongitudinal;
        var momentTorsionDetail = new ConcreteMomentTorsionDetail
        {
            Bending = bending,
            Torsion = torsion,
            ReinforcementFace = reinforcementFace,
            RequiredBendingReinforcement = bending.AsRequired,
            RequiredTorsionReinforcement = torsionLongitudinal,
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
                CreateCheck(
                    $"Dwarskracht Vz ({shear.Ved:0.#} kN)",
                    "6.2",
                    "...",
                    section.Position,
                    demand: shear.AswBenPerMeter,
                    resistance: shear.AswToegepast,
                    "mm²/m",
                    $"VRd = {shear.DwarskrachtWeerstand:0.##} kN",
                    shear),
                CreateCheck(
                    "Torsie Tx",
                    "6.3.2 (4)",
                    "T_Ed/T_Rd,max + V_Ed/V_Rd,max",
                    section.Position,
                    torsion.UnityCheck629,
                    1,
                    "-",
                    $"TEd = {torsion.TEd:0.##} kNm; TRd,max = {torsion.TRdMax:0.##} kNm; VRd,max = {torsion.VRdMax:0.#} kN.",
                    torsion),
                CreateCheck(
                    "Dwarskracht en torsie",
                    "6.3.2 (2)",
                    "A_sw,V/n + A_sw,T per snede",
                    section.Position,
                    torsion.AswPerSnedePerMeter,
                    suppliedStirrupsPerLeg,
                    "mm²/m",
                    torsion.IsAlleenMinimaleWapening
                        ? $"(6.31) = {torsion.UnityCheck631:0.00} ≤ 1: geen wringwapening nodig; beugels per snede {torsion.AswPerSnedePerMeter:0} mm²/m."
                        : $"(6.31) = {torsion.UnityCheck631:0.00} > 1: wringing vraagt {torsion.AswTPerMeter:0} mm²/m per wand; s ≤ {torsion.BeugelAfstandMax:0} mm (9.2.3).",
                    torsion),
                CreateCheck(
                    "Moment en torsie",
                    "6.1 en 6.3.2",
                    "(Aₛ,M,req + Aₛ,T,req) / Aₛ,prov",
                    section.Position,
                    requiredCombinedLongitudinal,
                    appliedReinforcement,
                    "mm²",
                    $"Torsie vraagt {torsionLongitudinal:0.##} mm² extra {reinforcementFace}wapening en {torsion.AslZijkantBenodigd:0.##} mm² per zijkant.",
                    momentTorsionDetail)
            ]
        };
    }

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

        return StirrupLegs * Math.PI * Math.Pow(stirrups.Diameter, 2) / 4 * 1_000 / spacing;
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
