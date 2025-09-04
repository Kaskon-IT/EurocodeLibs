using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace ExportFactory.Services
{


    public static class TableColumnAttributeHelper
    {
        public static IEnumerable<PropertyInfo> GetTableColumns<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<TableColumnAttribute>() != null);
        }

        public static (MarkupString Header, string Value, MarkupString Symbol, string? Article, string? Formula)
            GetColumnInfo(PropertyInfo prop, object model)
        {
            var attr = prop.GetCustomAttribute<TableColumnAttribute>();
            var header = (MarkupString)(attr?.HeaderText ?? prop.Name);
            var symbol = (MarkupString)(attr?.Symbol ?? "");
            var formula = attr?.Formula;

            var rawValue = prop.GetValue(model);
            string value = "-";

            if (rawValue != null)
            {
                if (!string.IsNullOrEmpty(attr?.StringFormat))
                    value = string.Format(attr.StringFormat, rawValue);
                else
                    value = rawValue.ToString() ?? "-";
            }

            return (header, value, symbol, attr?.Article, attr?.Formula);
        }



        [Obsolete("Use GetColumnInfo instead")]
        public static MarkupString GetHeaderMarkup(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<TableColumnAttribute>();
            return (MarkupString)(attr?.HeaderText ?? prop.Name);
        }

        [Obsolete("Use GetColumnInfo instead")]
        public static string GetFormattedValue(PropertyInfo prop, object model)
        {
            var attr = prop.GetCustomAttribute<TableColumnAttribute>();
            var rawValue = prop.GetValue(model);

            if (rawValue is null)
                return "-";

            if (!string.IsNullOrEmpty(attr?.StringFormat))
                return string.Format(attr.StringFormat, rawValue);

            return rawValue.ToString() ?? "";
        }
    }

}
