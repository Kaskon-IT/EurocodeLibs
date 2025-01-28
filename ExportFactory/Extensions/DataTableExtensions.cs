namespace ExportFactory.Extensions
{
    using ExportFactory.Shared;
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Tables;
    using Services;
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    public static class DataTableExtensions
    {
        public static Table? ToMigraDocTable(this DataTable dataTable, Type objectType)
        {
            Table migraDocTable = new();
            migraDocTable.Borders.Width = 0.25;


            // Check if the TableColumnAttribute is applied to any of the properties
            bool hasTableColumnAttribute = objectType.GetProperties()
                .Any(p => p.GetCustomAttributes(typeof(TableColumnAttribute), false).Any());

            if (!hasTableColumnAttribute)
            {
                Console.WriteLine("No properties with the TableColumnAttribute found.");
                return null;
            }



            // Get the properties of the object type and their associated ColumnAttribute
            var propertiesWithAttributes = objectType.GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<TableColumnAttribute>()
                })
                .Where(pa => pa.Attribute != null && pa.Attribute.Visible)
                .OrderBy(pa => pa.Attribute.Order) // Sort by ColumnOrder
                .ToList();

            // Create a list of column names to use for matching
            var columnNames = propertiesWithAttributes
                .Select(p => p.Property.Name)
                .ToList();





            // Add columns to the MigraDoc table based on the property attributes
            foreach (var propertyWithAttribute in propertiesWithAttributes)
            {
                Column migraDocColumn = migraDocTable.AddColumn();
                migraDocColumn.Format.Alignment = propertyWithAttribute.Attribute.Alignment;

                // Set column width if specified in ColumnAttribute
                if (propertyWithAttribute.Attribute.Width > 0)
                {
                    migraDocColumn.Width = Unit.FromCentimeter(propertyWithAttribute.Attribute.Width);
                }
            }

            // Add the header row
            Row headerRow = migraDocTable.AddRow();
            foreach (var propertyWithAttribute in propertiesWithAttributes)
            {
                ParagraphAlignment alignment = propertyWithAttribute.Attribute.Alignment;
                string headerText = propertyWithAttribute.Attribute.HeaderText ?? propertyWithAttribute.Property.Name;

                // add a paragraph with and apply markdown (if any) to it.
                var par = headerRow.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].AddParagraph();
                MigraDocCreator.AddMarkdownToParagraph(par, headerText);

                // set alignment
                headerRow.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].Format.Alignment = alignment;



            }

            // Add the data rows
            foreach (DataRow dataRow in dataTable.Rows)
            {





                Row row = migraDocTable.AddRow();
                foreach (var columnName in columnNames)
                {
                    //var columnName = dataTable.Columns[i].ColumnName;
                    var value = dataRow[columnName];

                    // Get the column's custom string format if applied
                    int i = columnNames.IndexOf(columnName);

                    var rowProperties = propertiesWithAttributes[i];

                    string? format = rowProperties.Attribute.StringFormat;

                    // Get the property info of the column (use reflection to find it)
                    //var property = typeof(Product).GetProperty(columnName);

                    // Check if the property has a FormatColumn attribute
                    //var formatAttribute = property?.GetCustomAttribute<FormatColumnAttribute>();

                    // If the attribute exists, format the value accordingly
                    //if (formatAttribute != null && value != DBNull.Value)
                    //{
                    //    value = string.Format(formatAttribute.StringFormat, value);
                    //}



                    // Format the value if stringFormat exists
                    if (!string.IsNullOrEmpty(format))
                    {
                        if (!format.StartsWith("{"))
                        {
                            format = "{0:" + format + "}";
                        }
                        value = string.Format(format, value);
                    }

                    // Apply value to the cell (with markdown support)
                    var cellPar = row.Cells[i].AddParagraph();
                    MigraDocCreator.AddMarkdownToParagraph(cellPar, value.ToString());



                }



                //foreach (var propertyWithAttribute in propertiesWithAttributes)
                //{
                //    string cellValue = dataRow[propertyWithAttribute.Property.Name]?.ToString() ?? string.Empty;
                //    row.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].AddParagraph(cellValue);
                //}
            }
            return migraDocTable;


        }
    }






}
