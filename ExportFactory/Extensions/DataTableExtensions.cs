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

    public static partial class DataTableExtensions
    {




        public static Table? ToMigraDocTable(this DataTable dataTable, Type objectType, bool isPivotTable = true)
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




            WeergaveEnum weergave = WeergaveEnum.StandaardTabel;
            if (isPivotTable)
            {
                weergave = WeergaveEnum.DraaiTabel;
            }



            // Get the properties of the object type and their associated ColumnAttribute
            var propertiesWithAttributes = objectType.GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<TableColumnAttribute>()
                })
                .Where(pa =>
                    pa.Attribute != null &&
                    pa.Attribute.Weergave != WeergaveEnum.Geen &&
                    (pa.Attribute.Weergave == WeergaveEnum.AlleTabellen || pa.Attribute.Weergave == weergave)) // 2025-02-24
                .OrderBy(pa => pa.Attribute.Order) // Sort by ColumnOrder
                .ToList();

            // i




            // Create a list of column names to use for matching
            var columnNames = propertiesWithAttributes
                .Select(p => p.Property.Name)
                .ToList();


            if (isPivotTable)
            {
                // gebruik Tag om aan te geven dat het een gedraaide tabel is
                migraDocTable.Tag = "pivot";
                // Add description colum + data columns
                int colIndex = 0;
                Column descriptionColumn = migraDocTable.AddColumn();

                for (int k = 0; k < dataTable.Rows.Count + 2; k++)   // col[0,1,2] zijn voor de header. 
                                                                     // col[0]=Symbol
                                                                     // col[1]=Description
                                                                     // col[2]=Article
                {
                    migraDocTable.AddColumn();
                }

                // Add rows
                foreach (var propertyWithAttribute in propertiesWithAttributes)
                {
                    Row row = migraDocTable.AddRow();
                    ParagraphAlignment alignment = propertyWithAttribute.Attribute.Alignment;
                    string headerText = propertyWithAttribute.Attribute.HeaderTextPivot ?? propertyWithAttribute.Attribute.HeaderText ?? propertyWithAttribute.Property.Name;

                    string? format = null;


                    // mapping
                    if (_propertieMap.ContainsKey(propertyWithAttribute.Property.Name))
                    {
                        var mapping = _propertieMap[propertyWithAttribute.Property.Name];

                        if (mapping.Symbol != null)
                        {
                            var parSymbol = row.Cells[0].AddParagraph();
                            parSymbol.Tag = "symbol";
                            MigraDocCreator.AddMarkdownToParagraph(parSymbol, mapping.Symbol);
                        }
                        if (mapping.Description != null)
                        {
                            var parDesc = row.Cells[1].AddParagraph();
                            parDesc.Tag = "description";
                            MigraDocCreator.AddMarkdownToParagraph(parDesc, mapping.Description);
                        }
                        if (mapping.Article != null)
                        {
                            var parArticle = row.Cells[2].AddParagraph();
                            parArticle.Tag = "article";
                            MigraDocCreator.AddMarkdownToParagraph(parArticle, mapping.Article);
                        }
                        if (mapping.Format != null)
                        {

                        }


                        //headerText = $"DICTIONARY{mapping.Symbol}";
                    }
                    else
                    {
                        // add a paragraph with and apply markdown (if any) to it.
                        var par = row.Cells[colIndex].AddParagraph();
                        MigraDocCreator.AddMarkdownToParagraph(par, headerText);
                    }


                }

                // Fill data column(s)
                colIndex = 3; // start with index 3 when pivottable
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    int rowIndex = 0;
                    //Column dataColumn = migraDocTable.AddColumn();
                    foreach (var columnName in columnNames)
                    {
                        //var columnName = dataTable.Columns[i].ColumnName;
                        var value = dataRow[columnName];





                        // Get the column's custom string format if applied

                        int i = columnNames.IndexOf(columnName);

                        var rowProperties = propertiesWithAttributes[i];
                        string? format = rowProperties.Attribute.StringFormat;
                        if (_propertieMap.ContainsKey(columnName))
                        {
                            var mapping = _propertieMap[columnName];


                            if (mapping.Format != null)
                            {
                                format = mapping.Format;

                            }


                            //headerText = $"DICTIONARY{mapping.Symbol}";
                        }






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
                        var cellPar = migraDocTable.Rows[rowIndex++].Cells[colIndex].AddParagraph();
                        MigraDocCreator.AddMarkdownToParagraph(cellPar, value.ToString());
                    }

                    // naar de volgende kolom
                    colIndex++;

                    //foreach (var propertyWithAttribute in propertiesWithAttributes)
                    //{
                    //    Row row = migraDocTable.AddRow();
                    //    ParagraphAlignment alignment = propertyWithAttribute.Attribute.Alignment;
                    //    string headerText = propertyWithAttribute.Attribute.HeaderText ?? propertyWithAttribute.Property.Name;

                    //    // add a paragraph with and apply markdown (if any) to it.
                    //    var par = row.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].AddParagraph();
                    //    MigraDocCreator.AddMarkdownToParagraph(par, headerText);
                    //}
                }


            }
            else if (!isPivotTable)
            {
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
                }
            }





            return migraDocTable;


        }
    }






}
