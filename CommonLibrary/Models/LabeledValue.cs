using System.Globalization;

namespace CommonLibrary.Models
{
    public class LabeledValue
    {
        public string Label { get; set; }
        public object? Value { get; set; }
        public string? Format { get; set; }
        public string NullDisplayText { get; set; } = "-";
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public LabeledValue(string label, object? value, string? format = null)
        {
            Label = label;
            Value = value;
            Format = format;
        }

        /// <summary>
        /// Geeft de waarde als geformatteerde string, rekening houdend met Format en null.
        /// </summary>
        public string ValueAsString
        {
            get
            {
                if (Value == null)
                    return NullDisplayText;

                if (Value is IFormattable formattable && !string.IsNullOrWhiteSpace(Format))
                    return formattable.ToString(Format, FormatProvider);

                return Value.ToString() ?? NullDisplayText;
            }
        }

        public override string ToString()
        {
            return $"{Label} : {ValueAsString}";
        }
    }

}
