using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace CommonLibrary.Helpers
{
    public static class MarkupHelper
    {
        public static MarkupString ToMarkupString(string? input)
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


            return new MarkupString(input);
        }
    }
}
