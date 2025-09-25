using System.Reflection;


namespace ExportFactory.Shared
{
    public record PropertyRow : IPropertyRow
    {
        public string Id { get; }

        public string Label { get; init; } = "";
        public string? Symbol { get; init; }
        public string? Unit { get; init; }
        public object? RawValue { get; set; } // mutable voor live update in UI
        public string DisplayValue { get; set; } = ""; // mutable voor live update in UI
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
            this.Id = $"{property.DeclaringType?.FullName}.{property.Name}";
        }
    }


    public record SubPropertyRow : IPropertyRow
    {
        public string Id { get; init; }
        public string Label { get; init; }
        public string Value { get; init; }
        public object SourceObject { get; init; }

        public SubPropertyRow(string label, string value, object sourceObject)
        {
            Label = label;
            Value = value;
            SourceObject = sourceObject;

            // unieke Id gebaseerd op type en label
            Id = $"{SourceObject.GetType().FullName}.{Label}";
        }
    }




    public interface IPropertyRow { }


}
