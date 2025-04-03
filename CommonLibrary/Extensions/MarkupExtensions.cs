using Microsoft.AspNetCore.Components;

namespace CommonLibrary.Extensions
{


    public static class MarkupExtensions
    {
        public static MarkupString ToMarkup(this string value)
        {
            return new MarkupString(value);
        }
    }
}
