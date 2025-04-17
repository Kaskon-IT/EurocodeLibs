using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CommonLibrary.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum? value)
        {
            if (value == null)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if (!string.IsNullOrWhiteSpace(displayAttr?.Name))
                return displayAttr.Name;

            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (!string.IsNullOrWhiteSpace(descriptionAttr?.Description))
                return descriptionAttr.Description;

            return value.ToString();
        }
    }
}
