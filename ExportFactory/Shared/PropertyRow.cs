using System.Reflection;


namespace ExportFactory.Shared
{
    public record PropertyRow : IPropertyRow
    {
        public string Label { get; init; } = "";
        public string? Symbol { get; init; }
        public string? Unit { get; init; }
        public object? RawValue { get; init; }
        public string DisplayValue { get; init; } = "";
        public string? Description { get; init; }
        public string? Article { get; init; }
        public Formula? Formula { get; init; }
        public bool Editable { get; init; }
        public PropertyInfo Property { get; init; } = null!; // tijdelijk null! voor lege/fallback rows

        // Optioneel: constructor voor echte rows
        public PropertyRow(
            string label,
            PropertyInfo property,
            string? symbol = null,
            string? unit = null,
            object? rawValue = null,
            string displayValue = "",
            string? description = null,
            string? article = null,
            Formula? formula = null,
            bool editable = false
            )
        {
            this.Label = label;
            this.Property = property;
            this.Symbol = symbol;
            this.Unit = unit;
            this.RawValue = rawValue;
            this.DisplayValue = displayValue;
            this.Description = description;
            this.Article = article;
            this.Formula = formula;
            this.Editable = editable;
        }
    }


    public record SubPropertyRow(string Label, string Value, object SourceObject) : IPropertyRow;


    public interface IPropertyRow { }


}
