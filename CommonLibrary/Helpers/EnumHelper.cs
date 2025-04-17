using CommonLibrary.Extensions;

namespace CommonLibrary.Helpers
{
    public class EnumHelper
    {
        public static List<EnumOption<T>> GetEnumOptions<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(v => new EnumOption<T>
                       {
                           Value = v,
                           Description = v.GetDisplayName()
                       })
                       .ToList();
        }

        public class EnumOption<T>
        {
            public T Value { get; set; } = default!;
            public string Description { get; set; } = string.Empty;
        }
    }
}
