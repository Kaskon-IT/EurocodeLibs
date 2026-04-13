using CommonLibrary;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace ExportFactory.Extensions
{
    public static class ListExtensions
    {





        // Extensiemethode om een IEnumerable<BaseEurocodeContext> om te zetten naar een DataTable
        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : BaseEurocodeContext
        {
            // Maak een lijst van T om de extensiemethode te gebruiken
            return ToDataTableInternal(list.ToList());
        }



        private static DataTable ToDataTableInternalOPT<T>(List<T> list) where T : BaseEurocodeContext
        {
            ArgumentNullException.ThrowIfNull(list);
            if (list.Count == 0)
                return new DataTable(typeof(T).Name);

            var type = list.First().GetType();
            var dataTable = new DataTable(type.Name);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // === KOLOMMEN AANMAKEN ===
            foreach (var prop in properties)
            {
                // Check zichtbaarheid per object (dynamisch)
                var visibleOverrideProp = type.GetProperty(prop.Name + "Visible");
                if (visibleOverrideProp?.GetValue(list.First()) is bool visible && !visible)
                    continue;

                var columnAttribute = prop.GetCustomAttribute<TableColumnAttribute>();

                // Bepaal kolomnaam (voorkom mismatchen)
                var columnName = columnAttribute?.Label ?? prop.Name;

                // Controleer of kolom al bestaat (veiligheid bij duplicate namen)
                if (dataTable.Columns.Contains(columnName))
                    continue;

                // Kies correcte type (nullable afhandelen)
                var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                var column = new DataColumn(columnName, columnType)
                {
                    // optioneel: standaardwaarde
                    AllowDBNull = true
                };

                // Extra eigenschappen uit attribute
                if (columnAttribute is not null)
                {
                    column.ExtendedProperties["Visible"] = columnAttribute.Visible;
                    if (!string.IsNullOrEmpty(columnAttribute.StringFormat))
                        column.ExtendedProperties["StringFormat"] = columnAttribute.StringFormat;
                }

                dataTable.Columns.Add(column);
            }

            // === RIJEN VULLEN ===
            foreach (var item in list)
            {
                var row = dataTable.NewRow();

                foreach (var prop in properties)
                {
                    // Zichtbaarheid check opnieuw (voor object-specifieke overrides)
                    var visibleOverrideProp = type.GetProperty(prop.Name + "Visible");
                    if (visibleOverrideProp?.GetValue(item) is bool visible && !visible)
                        continue;

                    var columnAttribute = prop.GetCustomAttribute<TableColumnAttribute>();
                    var columnName = columnAttribute?.Label ?? prop.Name;

                    if (!dataTable.Columns.Contains(columnName))
                        continue;

                    var value = prop.GetValue(item);

                    // Enum → Description
                    if (TryGetEnumDescription(value, prop, out string description))
                        value = description;

                    row[columnName] = value ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }


        // Extensiemethode om een List<T> om te zetten naar een DataTable, inclusief kolominstellingen zoals header, uitlijning en zichtbaarheid
        private static DataTable ToDataTableInternal<T>(List<T> list) where T : BaseEurocodeContext
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(list.FirstOrDefault());
            var type = list.FirstOrDefault()?.GetType();
            ArgumentNullException.ThrowIfNull(type);

            var dataTable = new DataTable();

            if (list.Count == 0)
            {
                return dataTable;
            }

            var fod = list.FirstOrDefault();

            try
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Voeg kolommen toe aan de DataTable op basis van de eigenschappen van T
                foreach (var prop in properties)
                {
                    // Haal het ColumnAttribute op (indien aanwezig)
                    var columnAttribute = prop.GetCustomAttribute<TableColumnAttribute>();

                    // zichtbaarheid per object (dynamisch) 
                    var visibleOverrideProp = type.GetProperty(prop.Name + "Visible");
                    if (visibleOverrideProp?.GetValue(fod) is bool visible)
                    {
                        if (!visible)
                            continue; // skip kolom, zorg ervoor dat deze niet wordt toegevoegd aan de datatable
                    }

                    // Aanvulling dynamisch vullen mbv Dictionary 
                    if (columnAttribute != null)
                    {
                        string columnName = prop.Name;
                    }


                    //if (columnAttribute == null)
                    //    continue;

                    var column = new DataColumn();
                    // Maak een nieuwe DataColumn voor elke eigenschap
                    //Console.WriteLine($"Adding column: {prop.Name} of type {prop.PropertyType}");

                    // check if nullable enum (not supported in datatable)
                    if (IsNullableEnum(prop) || IsEnum(prop) || IsNullable(prop))
                    {
                        //continue; // skip Enum?
                        column = new DataColumn(prop.Name);
                    }
                    else
                    {
                        column = new DataColumn(prop.Name, prop.PropertyType);
                    }


                    if (columnAttribute != null)
                    {
                        // Stel de headertekst in
                        //if (!string.IsNullOrEmpty(columnAttribute.HeaderText))
                        //    column.ColumnName = columnAttribute.HeaderText;
                        //else
                        //    column.ColumnName = prop.Name;

                        // Stel de uitlijning in
                        //if (columnAttribute.Alignment != ParagraphAlignment.Center)
                        //    column.SetOrdinal(0);  // Stel in de gewenste plaats van de kolom indien nodig (optie)

                        // Stel de zichtbaarheid in
                        column.ExtendedProperties["Visible"] = columnAttribute.Visible;

                        // Hier kan je custom formatting of andere eigenschappen toevoegen
                        if (!string.IsNullOrEmpty(columnAttribute.StringFormat))
                        {
                            column.ExtendedProperties["StringFormat"] = columnAttribute.StringFormat;
                        }
                    }

                    dataTable.Columns.Add(column);
                }

                // Voeg rijen toe aan de DataTable op basis van de objecten in de lijst
                // Add rows to the DataTable
                foreach (var item in list)
                {
                    // Create a new DataRow for each item
                    var row = dataTable.NewRow();

                    // koppel de instance in ExtendedProperties van DataTable
                    dataTable.ExtendedProperties[row] = item;

                    // Populate the DataRow with property values
                    foreach (var prop in properties)
                    {
                        // Assign the property value to the corresponding column in the DataRow
                        if (dataTable.Columns.Contains(prop.Name))
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                            // description
                            if (TryGetEnumDescription(row[prop.Name], prop, out string description))
                            {
                                row[prop.Name] = description;
                            }
                        }
                    }
                    // Add the populated row to the DataTable
                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Algemene fout: {ex.Message} \r\n{ex.InnerException?.Message}");
            }
            return dataTable;
        }


        public static MigraDoc.DocumentObjectModel.Document ToMigraDocDocument<T>(this List<T> list, Type type, bool isPivotTable = true, bool hideHeader = false) where T : BaseEurocodeContext
        {
            // step 1: make a datatable
            var dataTable = list.ToDataTable();

            // step 2: make a migradoc table
            return dataTable.ToMigraDocDocument(type, isPivotTable, hideHeader);
        }


        public static MigraDoc.DocumentObjectModel.Tables.Table? ToMigraDocTable<T>(this List<T> list, Type type, bool isPivotTable = false, bool hideHeader = false) where T : BaseEurocodeContext
        {
            // step 1: make a datatable
            var dataTable = list.ToDataTable();

            // step 2: make a migradoc table
            return dataTable.ToTable(type, isPivotTable, hideHeader);
        }



        // Check if the property is a nullable enum
        static bool IsNullableEnum(PropertyInfo prop)
        {
            // Get the underlying type of the property if it is nullable
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

            // Check if the property is nullable and its underlying type is an enum
            return underlyingType != null && underlyingType.IsEnum;
        }

        // Retrieves the description of an enum (works for both nullable and non-nullable enums)
        // Retrieves the description of an enum (works for both nullable and non-nullable enums)
        public static bool TryGetEnumDescription(object instance, PropertyInfo prop, out string description)
        {
            description = string.Empty;

            // Ensure the instance is a string (name of the enum value)
            if (instance is string enumStringValue)
            {
                // Get the underlying type of the property if it is nullable
                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

                // If the property is nullable, use the underlying type; otherwise, use the property type directly
                var enumType = underlyingType ?? prop.PropertyType;

                // Check if the type is an enum
                if (enumType.IsEnum)
                {
                    // Try to parse the string to the enum value
                    if (Enum.TryParse(enumType, enumStringValue, out var enumValue))
                    {
                        // Get the field info for the enum value
                        if (enumValue != null)
                        {
                            var valueString = enumValue.ToString();
                            if (valueString != null)
                            {

                                FieldInfo? field = enumType.GetField(valueString);

                                if (field == null)
                                {
                                    description = string.Empty;
                                    return false;
                                }

                                // Retrieve the DescriptionAttribute if present
                                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                                if (attribute != null)
                                {
                                    var descAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                                    // Return the description if available, or the enum name if not
                                    description = descAttribute != null ? descAttribute.Description : valueString;
                                    return true;
                                }

                            }
                        }

                    }
                }
            }

            return false; // Not a valid enum string or type mismatch
        }



        // Checks if the property is a nullable enum and retrieves its description if available
        public static bool IsNullableEnum(PropertyInfo prop, out string description)
        {
            description = string.Empty;

            // Get the underlying type of the property if it is nullable
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

            // Check if the property is nullable and its underlying type is an enum
            if (underlyingType != null && underlyingType.IsEnum)
            {
                // Get the enum value (using a default value like the first one if no instance is available)
                var enumValue = Enum.GetValues(underlyingType).GetValue(0); // Using the first enum value as default (you can customize this part)

                // Get the field info for the enum value
                FieldInfo field = underlyingType.GetField(enumValue.ToString());

                // Retrieve the DescriptionAttribute
                var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                // Return the description if available, otherwise the enum value name
                description = attribute != null ? attribute.Description : enumValue.ToString();

                return true; // It is a nullable enum and description was found
            }

            return false; // It is not a nullable enum
        }

        // Check if the property is a simple enum (non-nullable)
        static bool IsEnum(PropertyInfo prop)
        {
            return IsEnum(prop.PropertyType);
        }

        // Check if the property is nullable
        static bool IsNullable(PropertyInfo prop)
        {
            return Nullable.GetUnderlyingType(prop.PropertyType) != null;
        }

        // Check if the type is an enum
        static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }


        public static string GetEnumDescription(PropertyInfo property, object target)
        {
            // Get the value of the enum property
            var enumValue = property.GetValue(target);

            // Check if the property is an enum
            if (enumValue is Enum enumEnum)
            {
                // Get the field info for the enum value
                FieldInfo field = enumEnum.GetType().GetField(enumEnum.ToString());

                // Get the Description attribute
                var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                // Return the description or the enum name
                return attribute != null ? attribute.Description : enumEnum.ToString();
            }

            return string.Empty; // Return empty string if it's not an enum
        }


    }


}
