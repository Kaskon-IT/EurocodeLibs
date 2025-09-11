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



        // ---- Universele helper ----
        public static double TruncateToSignificantDigits(double value, int digits)
        {
            if (value == 0.0 || double.IsNaN(value) || double.IsInfinity(value))
                return value;

            double abs = Math.Abs(value);
            int exponent = (int)Math.Floor(Math.Log10(abs));    // orde van grootte
            double scale = Math.Pow(10, exponent - digits + 1);

            double truncated = Math.Truncate(value / scale) * scale;
            return truncated;
        }

        // ---- Formatter ----
        private static string FormatNumber(double? value, int sig, string? unit, bool isTeX)
        {
            if (!value.HasValue)
                return "?";

            double v = value.Value;

            if (double.IsNaN(v) || double.IsInfinity(v))
                return v.ToString(_culture);

            // Eerst trunceren
            double truncated = TruncateToSignificantDigits(v, sig);
            double abs = Math.Abs(truncated);

            string formatted;

            // Wetenschappelijke notatie voor grote of kleine waarden
            if ((abs >= 1000.0) || (abs > 0 && abs < 0.001))
            {
                int exponent = (int)Math.Floor(Math.Log10(abs));
                double mantisse = truncated / Math.Pow(10, exponent);

                if (isTeX)
                {
                    formatted = mantisse.ToString($"0.{new string('#', sig - 1)}", _culture)
                                + $"\\cdot 10^{{{exponent}}}";
                }
                else
                {
                    formatted = mantisse.ToString($"0.{new string('#', sig - 1)}", _culture)
                                + $"E{exponent}";
                }
            }
            else
            {
                int decimals = Math.Max(0, sig - (int)Math.Floor(Math.Log10(abs)) - 1);
                formatted = truncated.ToString("0." + new string('#', decimals), _culture);
            }

            // Eenheid toevoegen
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








