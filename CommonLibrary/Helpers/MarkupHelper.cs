using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CommonLibrary.Helpers
{
    public static class MarkupHelper
    {
        public static MarkupString ToMarkupString(string? input, bool withUnityCheck = true, bool withEmoji = false)
        {

            if (input == null || string.IsNullOrEmpty(input))
                return new MarkupString(string.Empty);

            // Omzetten van ~text~ naar <sub>text</sub>
            input = Regex.Replace(input, @"~(.*?)~", "<sub>$1</sub>");

            // Omzetten van ^text^ naar <sup>text</sup>
            input = Regex.Replace(input, @"\^(.*?)\^", "<sup>$1</sup>");

            // Bold: **text** -> <strong>text</strong>
            input = Regex.Replace(input, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

            // Italic: *text* -> <em>text</em>
            input = Regex.Replace(input, @"\*(.*?)\*", "<em>$1</em>");

            // Underline: __text__ -> <u>text</u>
            input = Regex.Replace(input, @"__(.*?)__", "<u>$1</u>");

            // Strikethrough: ~~text~~ -> <s>text</s>
            input = Regex.Replace(input, @"~~(.*?)~~", "<s>$1</s>");

            // Line breaks: dubbele nieuwe regel -> <br/>
            input = Regex.Replace(input, @"\n\s*\n", "<br/>");

            if (withUnityCheck)
            {
                return ToMarkupStringWithUcCheck(input, withEmoji);
            }


            return new MarkupString(input);
        }


        public static MarkupString ToMarkupStringWithUcCheck(string? input, bool withEmoji = false)
        {
            if (string.IsNullOrEmpty(input))
                return new MarkupString(string.Empty);

            var regex = new Regex(@"\(UC\s*=\s*(\d+[.,]?\d*)\)", RegexOptions.IgnoreCase);

            // Zoek eerst of er een UC > 1.0 in zit
            bool hasWarningUc = regex.Matches(input)
                .Select(m => m.Groups[1].Value.Replace(",", "."))
                .Any(val => double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double uc) && uc > 1.0);

            // Voer daarna de vervangingen uit
            var replaced = regex.Replace(input, match =>
            {
                var rawValue = match.Groups[1].Value.Replace(",", ".");
                if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double uc))
                {
                    if (uc > 1.0)
                    {

                        return $"<b>{(withEmoji ? "⚠️" : "")}{match.Value}</b>";
                    }
                    else
                    {
                        return $"<b>{(withEmoji ? "✅" : "")}{match.Value}</b>";
                    }
                }

                return match.Value;
            });

            // Voeg de kleur toe aan de hele string indien nodig
            if (hasWarningUc)
            {
                return new MarkupString($"<span style=\"color: var(--warning);\">{replaced}</span>");
            }

            return new MarkupString(replaced);
        }


        public static MarkupString ToMarkupStringWithUcCheckBAK(string? input)
        {
            if (input == null) return new MarkupString(string.Empty);
            var regex = new Regex(@"\(UC\s*=\s*(\d+[.,]?\d*)\)", RegexOptions.IgnoreCase);

            return new MarkupString(regex.Replace(input, match =>
            {
                var rawValue = match.Groups[1].Value.Replace(",", ".");
                if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double uc) && uc > 1.0)
                {

                    return $"<span style=\"color: var(--warning);0\"><b>⚠️{match.Value}</b></span>";
                }
                else if (uc <= 1.0)
                {
                    return $"<span style=\"color: var(--succes);0\"><b>✅{match.Value}</b></span>";

                }

                return match.Value;
            }));
        }

    }
}
