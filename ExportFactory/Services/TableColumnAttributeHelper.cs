using CommonLibrary.Extensions;
using ExportFactory.Shared;
using Microsoft.AspNetCore.Components;
using System.Globalization;
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


        public static IEnumerable<PropertyRow> GetRows<T>(T model)
        {

            // ter info
            // foreach (var prop in typeof(T).GetProperties())
            // veranderd naar 
            foreach (var prop in model!.GetType().GetProperties())
            {
                Formula? formula = null;
                var attr = prop.GetCustomAttribute<TableColumnAttribute>();

                if (attr == null)
                    continue; // alleen properties met [TableColumn]

                var rawValue = prop.GetValue(model);
                string displayValue = rawValue?.ToString() ?? "-";

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
                    // ✅ Als StringFormat wél is opgegeven
                    else if (!string.IsNullOrEmpty(attr.StringFormat))
                    {
                        displayValue = string.Format(attr.StringFormat, rawValue);
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

                yield return new PropertyRow(
                    Label: attr.Label ?? prop.Name,
                    Symbol: attr.Symbol,
                    Unit: attr.Unit,
                    RawValue: rawValue,
                    DisplayValue: displayValue,
                    Description: attr.Description,
                    Article: attr.Article,
                    Formula: formula, // via conventie
                    Editable: editable, // als de property een setter heeft, is die bewerkbaar
                    Property: prop // property meegeven zodat we kunnen wijzigen
                );
            }
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
