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

            var type = value.GetType();
            var field = type.GetField(value.ToString());
            if (field == null)
                return value.ToString();

            // 1. DisplayAttribute.Name
            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if (!string.IsNullOrWhiteSpace(displayAttr?.Name))
                return displayAttr.Name;

            // 2. DescriptionAttribute.Description
            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (!string.IsNullOrWhiteSpace(descriptionAttr?.Description))
                return descriptionAttr.Description;

            // 3. Fallback: Enum value
            return value.ToString();
        }
    }
}
