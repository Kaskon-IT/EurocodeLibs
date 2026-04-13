using System.Globalization;

namespace CommonLibrary.Extensions
{
    public static class NumberFormatter
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private static int _defaultSignificantDigits = 3;

        /// <summary>
        /// Globale standaard voor aantal significante cijfers (kan runtime worden aangepast).
        /// </summary>
        public static int DefaultSignificantDigits
        {
            get => _defaultSignificantDigits;
            set
            {
                if (_defaultSignificantDigits != value)
                {
                    _defaultSignificantDigits = value;
                    DefaultSignificantDigitsChanged?.Invoke(null, value);
                }
            }
        }

        /// <summary>
        /// Event dat afgaat zodra de default significant digits verandert.
        /// </summary>
        public static event EventHandler<int>? DefaultSignificantDigitsChanged;
        public static double DefaultMaxValueWithoutExponent { get; set; } = 9999;
        public static double DefaultMinValueWithoutExponent { get; set; } = 0.0001;


        // ---- Verbeterde universele helper met afronding ----
        public static double RoundToSignificantDigits(double value, int digits)
        {
            if (value == 0.0 || double.IsNaN(value) || double.IsInfinity(value))
                return value;

            double abs = Math.Abs(value);
            int exponent = (int)Math.Floor(Math.Log10(abs)); // orde van grootte
            double scale = Math.Pow(10, exponent - digits + 1);

            double rounded = Math.Round(value / scale, 0, MidpointRounding.AwayFromZero) * scale;
            return rounded;
        }

        // ---- Formatter ----
        private static string FormatNumber(double? value, int sig, string? unit, bool isTeX, int? forcedExponent = null)
        {
            if (!value.HasValue)
                return "?";

            double v = value.Value;

            if (double.IsNaN(v) || double.IsInfinity(v))
                return v.ToString(_culture);

            double rounded = RoundToSignificantDigits(v, sig);
            double abs = Math.Abs(rounded);

            string formatted;

            int exponent;
            double mantisse;

            if (forcedExponent.HasValue)
            {
                exponent = forcedExponent.Value;
                mantisse = rounded / Math.Pow(10, exponent);
            }
            else if ((abs >= DefaultMaxValueWithoutExponent) || (abs > 0 && abs < DefaultMinValueWithoutExponent))
            {
                exponent = (int)Math.Floor(Math.Log10(abs));
                mantisse = rounded / Math.Pow(10, exponent);
            }
            else
            {
                int decimals = Math.Max(0, sig - (int)Math.Floor(Math.Log10(abs)) - 1);
                formatted = rounded.ToString("0." + new string('#', decimals), _culture);

                if (!string.IsNullOrEmpty(unit))
                {
                    formatted += isTeX ? $@" \,\text{{{unit}}}" : $" {unit}";
                }
                return formatted;
            }

            // wetenschappelijke notatie
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

            if (!string.IsNullOrEmpty(unit))
            {
                formatted += isTeX ? $@" \,\text{{{unit}}}" : $" {unit}";
            }

            return formatted;
        }

        // ----- Double -----
        public static string ToEng(this double value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this double value, int? sig = null, string? unit = null, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);

        public static string ToEng(this double? value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this double? value, int? sig = null, string? unit = null, int? forcedExponent = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);

        // ----- Float -----
        public static string ToEng(this float value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this float value, int? sig = null, string? unit = null, int? forcedExponent = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);

        public static string ToEng(this float? value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this float? value, int? sig = null, string? unit = null, int? forcedExponent = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);

        // ----- Int -----
        public static string ToEng(this int value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this int value, int? sig = null, string? unit = null, int? forcedExponent = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);

        public static string ToEng(this int? value, int? sig = null, string? unit = null, bool isTeX = false, int? forcedExponent = null)
            => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, isTeX, forcedExponent);

        public static string ToTeX(this int? value, int? sig = null, string? unit = null, int? forcedExponent = null)
           => FormatNumber(value, sig ?? DefaultSignificantDigits, unit, true, forcedExponent);
    }

    public class ScientificFormatter : IFormatProvider, ICustomFormatter
    {
        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (arg is not IFormattable && arg is not IConvertible)
                return arg?.ToString() ?? string.Empty;

            if (arg is not double && arg is not float && arg is not int)
                return arg?.ToString() ?? string.Empty;

            double value = Convert.ToDouble(arg, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(format))
                return value.ToString(CultureInfo.InvariantCulture);

            // match patronen zoals SIG4E6, TEX6, etc.
            if (format.StartsWith("SIG", StringComparison.OrdinalIgnoreCase))
            {
                // voorbeeld: SIG4E6 = 4 significante cijfers, exponent 6
                var parts = System.Text.RegularExpressions.Regex.Match(format, @"SIG(?<sig>\d+)E(?<exp>-?\d+)");
                if (parts.Success)
                {
                    int sig = int.Parse(parts.Groups["sig"].Value);
                    int exp = int.Parse(parts.Groups["exp"].Value);
                    return NumberFormatter.ToEng(value, sig, null, false, exp);
                }
            }
            else if (format.StartsWith("TEX", StringComparison.OrdinalIgnoreCase))
            {
                // voorbeeld: TEX6 = TeX weergave met exponent 6
                var parts = System.Text.RegularExpressions.Regex.Match(format, @"TEX(?<exp>-?\d+)");
                if (parts.Success)
                {
                    int exp = int.Parse(parts.Groups["exp"].Value);
                    return NumberFormatter.ToTeX(value, NumberFormatter.DefaultSignificantDigits, null, exp);
                }
            }

            // fallback: normale .NET notatie
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }


}
