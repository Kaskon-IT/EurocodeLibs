using ExportFactory.Shared;
using System.Data;
using System.Reflection;

namespace ExportFactory.Extensions
{


    public static class ListExtensions
    {
        // Extensiemethode om een List<T> om te zetten naar een DataTable, inclusief kolominstellingen zoals header, uitlijning en zichtbaarheid
        public static DataTable ToDataTable<T>(this List<T> list) where T : class
        {
            var dataTable = new DataTable();

            if (list.Count == 0)
            {
                return dataTable;
            }

            // Verkrijg de eigenschappen van T (de kolommen)
            var properties = typeof(T).GetProperties();

            // Voeg kolommen toe aan de DataTable op basis van de eigenschappen van T
            foreach (var prop in properties)
            {
                // Maak een nieuwe DataColumn voor elke eigenschap
                Console.WriteLine($"Adding column: {prop.Name} of type {prop.PropertyType}");
                var column = new DataColumn(prop.Name, prop.PropertyType);

                // Haal het ColumnAttribute op (indien aanwezig)
                var columnAttribute = prop.GetCustomAttribute<TableColumnAttribute>();

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

                foreach (var prop in properties)
                {
                    // Assign the property value to the corresponding column in the DataRow
                    Console.WriteLine($"Setting value for {prop.Name}: {prop.GetValue(item)}");
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                // Add the populated row to the DataTable
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }




}
