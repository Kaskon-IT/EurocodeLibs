using CommonLibrary;
using CommonLibrary.Extensions;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Reflection;

namespace ExportFactory.Services
{


    public static class TableColumnAttributeHelper
    {

        private static readonly IFormatProvider SciFmt = new ScientificFormatter();


        public static IEnumerable<PropertyInfo> GetTableColumns<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<TableColumnAttribute>() != null);
        }

        public static IEnumerable<IPropertyRow> GetRows<T>(
     T model,
     int depth = 0,
     int maxDepth = 5,
     HashSet<object>? visited = null)
        {
            if (model == null)
                yield break;

            // Initialiseer visited-set bij eerste call
            visited ??= new HashSet<object>();

            // Stop als we te diep gaan
            if (depth > maxDepth)
                yield break;

            // Stop als we dit object al gezien hebben (circular reference!)
            if (!visited.Add(model))
                yield break;

            foreach (var prop in model.GetType().GetProperties())
            {
                Formula? formula = null;

                // ✅ Collections
                if (prop.PropertyType.IsGenericType)
                {
                    var genericArgs = prop.PropertyType.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        var elementType = genericArgs[0];
                        var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

                        if (enumerableType.IsAssignableFrom(prop.PropertyType))
                        {
                            var value = prop.GetValue(model) as System.Collections.IEnumerable;
                            if (value != null)
                            {
                                foreach (var item in value)
                                {
                                    if (item is Melding m)
                                    {
                                        yield return new SubPropertyRow(m.Id.ToString(), m.GetEmoji, m.ToMarkupString().Value, m);
                                    }
                                    else
                                    {
                                        foreach (var childRow in GetRows(item, depth + 1, maxDepth, visited))
                                            yield return childRow;
                                    }
                                }
                            }
                        }
                    }
                }

                // ✅ Normale properties met [TableColumn]
                var attr = prop.GetCustomAttribute<TableColumnAttribute>();
                if (attr != null)
                {
                    var rawValue = prop.GetValue(model);
                    string displayValue = rawValue?.ToString() ?? "";

                    if (rawValue is Enum enumValue)
                    {
                        displayValue = enumValue.GetDisplayName();
                    }
                    else if (rawValue is IFormattable && string.IsNullOrEmpty(attr.StringFormat))
                    {
                        displayValue = ((double)Convert.ChangeType(rawValue, typeof(double))).ToEng();
                    }
                    else if (!string.IsNullOrEmpty(attr.StringFormat))
                    {
                        var format = attr.StringFormat.Contains("{0") ? attr.StringFormat : "{0:" + attr.StringFormat + "}";
                        displayValue = string.Format(SciFmt, format, rawValue);
                    }

                    var formulaProp = model.GetType().GetProperty(prop.Name + "Formula");
                    if (formulaProp?.GetValue(model) is Formula f)
                        formula = f;

                    bool editable = prop.GetSetMethod(nonPublic: false) != null;

                    var label = attr?.Label ?? prop.Name;
                    var symbol = attr?.Symbol;

                    var symbolOverrideProp = model.GetType().GetProperty(prop.Name + "Symbol");
                    if (symbolOverrideProp?.GetValue(model) is string sym)
                        symbol = sym;

                    yield return new PropertyRow(
                        label: DeCapitalizeFirstLetter(label) ?? "",
                        symbol: symbol,
                        unit: attr?.Unit,
                        rawValue: rawValue,
                        displayValue: displayValue,
                        description: attr?.Description,
                        article: attr?.Article,
                        formula: formula,
                        editable: editable,
                        property: prop
                    );

                    continue;

                }

                //---Recursie voor complex object(geen TableColumn)
                //var val = prop.GetValue(model);
                //if (val != null && !IsSimpleType(prop.PropertyType))
                //{
                //    foreach (var childRow in GetRows(val, depth + 1, maxDepth, visited))
                //        yield return childRow;
                //}

                //else
                //{
                //    //// ✅ Recursie voor complex object
                //    //var value = prop.GetValue(model);
                //    //if (value != null
                //    //    && !prop.PropertyType.IsPrimitive
                //    //    && prop.PropertyType != typeof(string))
                //    //{
                //    //    foreach (var childRow in GetRows(value, depth + 1, maxDepth, visited))
                //    //        yield return childRow;
                //    //}

                //}
            }
        }

        static bool IsSimpleType(Type t)
        {
            if (t.IsPrimitive || t.IsEnum) return true;
            if (t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) ||
                t == typeof(TimeSpan) || t == typeof(Guid)) return true;
            return false;
        }


        public static IEnumerable<IPropertyRow> GetRowsBAK<T>(T model)
        {
            foreach (var prop in model!.GetType().GetProperties())
            {
                Formula? formula = null;

                // aanvulling => haal de meldingen apart op
                if (prop.PropertyType.IsGenericType)
                {
                    // Pak de generieke argumenten, bv. Melding bij ObservableCollection<Melding>
                    var genericArgs = prop.PropertyType.GetGenericArguments();

                    if (genericArgs.Length == 1)
                    {
                        var elementType = genericArgs[0];
                        var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

                        // Controleer of de property een IEnumerable<T> implementeert
                        if (enumerableType.IsAssignableFrom(prop.PropertyType))
                        {
                            // Titel-row voor de collectie
                            // Mocht je een titel willen, dan zou je die hier kunnen toevoegen


                            // Haal de waarde op en cast naar IEnumerable
                            var value = prop.GetValue(model) as System.Collections.IEnumerable;
                            if (value != null)
                            {

                                foreach (var item in value)
                                {
                                    if (item is Melding m)
                                    {
                                        yield return new SubPropertyRow(m.Id.ToString(), m.GetEmoji, m.ToMarkupString().Value, m);
                                    }


                                }
                            }
                        }
                    }
                }




                var attr = prop.GetCustomAttribute<TableColumnAttribute>();

                if (attr == null)
                    continue; // alleen properties met [TableColumn]

                var rawValue = prop.GetValue(model);
                string displayValue = rawValue?.ToString() ?? "DEBUG";


                if (displayValue == "DEBUG")
                {
                    bool error = true;
                }

                if (rawValue != null)
                {
                    var type = rawValue.GetType();

                    // ✅ Eerst: Enum afvangen
                    if (rawValue is Enum enumValue)
                    {
                        displayValue = enumValue.GetDisplayName(); // mijn extension
                    }
                    // ✅ Dan: numerieke types zonder StringFormat
                    else if (rawValue is IFormattable && string.IsNullOrEmpty(attr.StringFormat))
                    {
                        displayValue = ((double)Convert.ChangeType(rawValue, typeof(double)))
                            .ToEng();
                    }
                    // ✅ Als StringFormat wél is opgegeven (gebruik custom formatter!)
                    else if (!string.IsNullOrEmpty(attr.StringFormat))
                    {
                        string format = attr.StringFormat;

                        // Als de gebruiker alleen "SIG3E6" geeft → maak er "{0:SIG3E6}" van
                        if (!format.Contains("{0"))
                        {
                            format = "{0:" + format + "}";
                        }

                        displayValue = string.Format(SciFmt, format, rawValue);
                    }
                }

                // Afgesproken conventie: als er een property bestaat met de naam <prop.Name>Formula
                var formulaProp = model?.GetType().GetProperty(prop.Name + "Formula");
                if (formulaProp != null)
                {
                    var formulaValue = formulaProp.GetValue(model);
                    if (formulaValue is Formula f)
                    {
                        formula = f;
                    }
                }

                bool editable = prop.GetSetMethod(nonPublic: false) != null; // alleen publieke setters!

                var label = attr?.Label ?? prop.Name;
                var symbol = attr?.Symbol;

                // overrides
                var symbolOverrideProp = model?.GetType().GetProperty(prop.Name + "Symbol");
                if (symbolOverrideProp != null)
                {
                    // Eerst runtime label
                    var symbolOverrideValue = symbolOverrideProp.GetValue(model);
                    if (symbolOverrideValue is String sym)
                    {
                        symbol = sym;
                    }
                }


                yield return new PropertyRow(
                    label: DeCapitalizeFirstLetter(label) ?? "",
                    symbol: symbol,
                    unit: attr?.Unit,
                    rawValue: rawValue,
                    displayValue: displayValue,
                    description: attr?.Description,
                    article: attr?.Article,
                    formula: formula,
                    editable: editable,
                    property: prop
                );

            }
        }


        private static string? DeCapitalizeFirstLetter(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var first = input[0];

            // Grieks (Unicode range 0370–03FF)
            if (first >= '\u0370' && first <= '\u03FF')
                return input;

            // Normale case
            if (char.IsLetter(first))
                return char.ToLower(first) + input.Substring(1);

            return input;
        }


        public static TableColumnDto GetMetaDto(PropertyInfo prop, object model)
        {

            var attr = prop.GetCustomAttribute<TableColumnAttribute>();


            var article = attr?.Article;
            var unit = attr?.Unit;
            var description = attr?.Description;
            var label = (attr?.Label ?? prop.Name);
            var symbol = (attr?.Symbol ?? "");
            Formula? formula = null;

            var rawValue = prop.GetValue(model);
            string value = "-";

            if (rawValue != null)
            {
                if (!string.IsNullOrEmpty(attr?.StringFormat))
                {
                    value = string.Format(attr.StringFormat, rawValue);
                }
                else
                {
                    if (rawValue.GetType().IsEnum)
                    {
                        var enumValue = (Enum)rawValue;
                        value = enumValue.GetDisplayName();

                    }
                    else
                    {
                        TypeCode code = Type.GetTypeCode(rawValue.GetType());
                        if (code == TypeCode.Int32 || code == TypeCode.Double || code == TypeCode.Single || code == TypeCode.Decimal)
                        {
                            double d = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture);
                            value = d.ToEng(unit: attr?.Unit, isTeX: !true);
                        }
                        else
                        {
                            value = rawValue.ToString() ?? "-";
                        }
                    }

                }
            }

            // Zoek naar een property met de naam <prop.Name>Formula
            // Deze property moet van het type Formula zijn
            // Als die bestaat, gebruik die dan
            var formulaProp = model.GetType().GetProperty(prop.Name + "Formula");
            if (formulaProp != null)
            {
                var formulaValue = formulaProp.GetValue(model);
                if (formulaValue is Formula f)
                {
                    formula = f;
                }
            }


            return new TableColumnDto(label, value, description, symbol, article, unit, formula);
        }


        public static (MarkupString Header, string Value, MarkupString Symbol, string? Article, Formula? Vergelijking)
            GetColumnInfo(PropertyInfo prop, object model)
        {
            var attr = prop.GetCustomAttribute<TableColumnAttribute>();



            var header = (MarkupString)(attr?.Label ?? prop.Name);
            var symbol = (MarkupString)(attr?.Symbol ?? "");
            //var formula = attr?.Formula;
            //string? dynamicFormula = null;
            Formula? vergelijking = null;

            var rawValue = prop.GetValue(model);
            string value = "-";

            if (rawValue != null)
            {
                if (!string.IsNullOrEmpty(attr?.StringFormat))
                {
                    value = string.Format(attr.StringFormat, rawValue);
                }
                else
                {
                    if (rawValue.GetType().IsEnum)
                    {
                        var enumValue = (Enum)rawValue;
                        value = enumValue.GetDisplayName();

                    }
                    else
                    {
                        TypeCode code = Type.GetTypeCode(rawValue.GetType());
                        if (code == TypeCode.Int32 || code == TypeCode.Double || code == TypeCode.Single || code == TypeCode.Decimal)
                        {
                            double d = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture);
                            value = d.ToEng(unit: attr?.Unit, isTeX: !true);
                        }
                        else
                        {
                            value = rawValue.ToString() ?? "-";
                        }
                    }

                }
            }

            // Zoek naar een property met de naam <prop.Name>Formula
            // Deze property moet van het type Formula zijn
            // Als die bestaat, gebruik die dan
            var formulaProp = model.GetType().GetProperty(prop.Name + "Formula");
            if (formulaProp != null)
            {
                var formulaValue = formulaProp.GetValue(model);
                if (formulaValue is Formula f)
                {
                    vergelijking = f;
                }
            }



            return (header, value, symbol, attr?.Article, vergelijking);
        }



        [Obsolete("Use GetColumnInfo instead")]
        public static MarkupString GetHeaderMarkup(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<TableColumnAttribute>();
            return (MarkupString)(attr?.Label ?? prop.Name);
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
