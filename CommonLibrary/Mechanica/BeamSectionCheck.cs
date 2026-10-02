namespace CommonLibrary.Mechanica;

public enum BeamFieldType
{
    StartOverhang,
    MainSpan,
    EndOverhang
}

public sealed record BeamFieldDefinition(
    BeamFieldType Type,
    string Name,
    double StartPosition,
    double EndPosition)
{
    public double Length => EndPosition - StartPosition;
}

public sealed record BeamFieldExtreme(
    string Component,
    double Position,
    double Value,
    string Unit);

/// <summary>
/// Eén toets van een liggerdoorsnede (staal of beton), in een vorm die de UI en rapportage kunnen tonen.
/// </summary>
public sealed record BeamSectionCheck(
    string Name,
    string Norm,
    string Article,
    string Formula,
    double Position,
    double Demand,
    double Resistance,
    string Unit,
    double Utilization,
    bool IsApplicable,
    string Explanation,
    object? DetailContext = null)
{
    public bool Passes => !IsApplicable || Utilization <= 1;
}
