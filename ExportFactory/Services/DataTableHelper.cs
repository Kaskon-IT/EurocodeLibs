using CommonLibrary;
using System.Data;
using System.Reflection;

namespace ExportFactory.Services
{
    public class TableModelHelper
    {
        public static DataTable CreateDataTable<T>(List<T> data)
        {
            DataTable table = new DataTable();

            // Get the properties of the data model
            var properties = typeof(T).GetProperties()
                                      .Where(p => p.IsDefined(typeof(CustomColumnAttribute), false))
                                      .ToList();

            // Add columns to the DataTable based on the property attributes
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<CustomColumnAttribute>();

                if (attribute.Visible)
                {
                    // Create the column with the name from the attribute
                    table.Columns.Add(attribute.HeaderName, property.PropertyType);

                    // Set column width (if necessary for rendering in UI)
                    // In a DataTable, we cannot set width directly, but we can keep track of it for UI rendering later.
                    Console.WriteLine($"Column: {attribute.HeaderName}, Width: {attribute.Width}");
                }
            }

            // Populate the DataTable with the data
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<CustomColumnAttribute>();
                    if (attribute.Visible)
                    {
                        // Get the value from the property and apply string format if necessary
                        var value = property.GetValue(item);

                        if (!string.IsNullOrEmpty(attribute.Format))
                        {
                            value = string.Format("{0:" + attribute.Format + "}", value);
                        }

                        // Add the value to the DataRow
                        row[attribute.HeaderName] = value;
                    }
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
