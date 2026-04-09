using System.Reflection;


namespace ExportFactory.Shared
{
    public record PropertyRow : IPropertyRow
    {
        public string Id { get; }
        public string Key => Id; // Unieke sleutel voor deze rij
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
        public object Target { get; init; }   // 👈 nieuw: het object waarop de property hoort


        // Optioneel: constructor voor echte rows
        public PropertyRow(
            string label,
            PropertyInfo property,
            object target,
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
            this.Target = target;
            this.Symbol = symbol;
            this.Unit = unit;
            this.RawValue = rawValue;
            this.DisplayValue = displayValue.Replace("True", "ja").Replace("False", "nee");
            this.Description = description;
            this.Article = article;
            this.Formula = formula;
            this.Editable = editable;
            this.Id = $"{property.DeclaringType?.FullName}.{property.Name}.{target.GetHashCode()}"; // aanvulling met HashCode (voor unieke id)
        }
    }


    public record SubPropertyRow : IPropertyRow
    {
        public string Id { get; init; }
        public string Label { get; init; }
        public string Value { get; init; }
        public object Target { get; init; }


        //public object? Target { get; init; }
        public PropertyInfo? Property { get; init; }
        public string Key => Id;

        public SubPropertyRow(string id, string label, string value, object target)
        {
            Id = id;

            Label = label;
            Value = value;
            Target = target;
            //Property = property;


        }
    }




    public interface IPropertyRow
    {
        string Key { get; }
        object? Target { get; }          // het object waar de property bij hoort
        PropertyInfo? Property { get; }  // de property zelf
    }



}
