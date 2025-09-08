using System.Globalization;

namespace CommonLibrary.Extensions
{

    public static class NumberFormatter
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        /// <summary>
        /// Globale standaard voor aantal significante cijfers (kan runtime worden aangepast).
        /// </summary>
        public static int DefaultSignificantDigits { get; set; } = 4;



        // ----- Interne DRY-methode -----
        private static string FormatNumber(double? value, int sig, string? unit, bool isTeX)
        {
            if (!value.HasValue)
                return "?";

            double v = value.Value;

            if (double.IsNaN(v) || double.IsInfinity(v))
                return v.ToString(_culture);

            double abs = Math.Abs(v);
            string formatted;

            // Wetenschappelijke notatie voor grote of kleine waarden
            if ((abs >= 1000.0) || (abs > 0 && abs < 0.001))
            {
                if (isTeX)
                {
                    int exponent = (int)Math.Floor(Math.Log10(abs));
                    double mantisse = v / Math.Pow(10, exponent);
                    formatted = mantisse.ToString($"G{sig}", _culture) + $"\\cdot 10^{{{exponent}}}";
                }
                else
                {
                    formatted = v.ToString($"G{sig}", _culture);
                }
            }
            else
            {
                int decimals = Math.Max(0, sig - (int)Math.Floor(Math.Log10(abs)) - 1);
                formatted = v.ToString($"0.{new string('#', decimals)}", _culture);
            }

            if (!string.IsNullOrEmpty(unit))
            {
                if (isTeX)
                    formatted += $@" \,\text{{{unit}}}";
                else
                    formatted += $" {unit}";
            }

            return formatted;
        }

        // ----- Double -----
        public static string ToEng(this double value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this double value, int? sig = null, string? unit = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);

        public static string ToEng(this double? value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this double? value, int? sig = null, string? unit = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);



        // ----- Float -----
        public static string ToEng(this float value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this float value, int? sig = null, string? unit = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);

        public static string ToEng(this float? value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this float? value, int? sig = null, string? unit = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);

        // ----- Int -----
        public static string ToEng(this int value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this int value, int? sig = null, string? unit = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);

        public static string ToEng(this int? value, int? sig = null, string? unit = null, bool isTeX = false)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX);

        public static string ToTeX(this int? value, int? sig = null, string? unit = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true);
    }
}








