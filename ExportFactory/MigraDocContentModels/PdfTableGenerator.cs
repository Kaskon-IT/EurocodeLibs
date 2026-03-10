using CommonLibrary;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using System.Reflection;

namespace ExportFactory.MigraDocContentModels
{






    public class TableColumn
    {
        public string Name { get; set; }            // Naam voor Property (mapping)
        public string HeaderName { get; set; }      // Naam gebruikt in tabel 
        public string? Format { get; set; } = null; // Format string for the column (e.g., "C2", "N0")
        public double Width { get; set; } = 3; // Width of the column in centimeters
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center; // Alignment for the column (e.g., Left, Center, Right)
        public int ColumnPosition { get; set; } = -1;

        public TableColumn(string name, string headerName, string? format = null, double width = 3, ParagraphAlignment alignment = ParagraphAlignment.Center, int columnPosition = -1)
        {
            Name = name;
            HeaderName = headerName;
            Format = format;
            Width = width;
            Alignment = alignment;
            ColumnPosition = columnPosition;
        }

        public TableColumn(string name)
        {
            Name = name;
            HeaderName = name;
        }

    }

    public class TableModel<T> : SectionElement
    {
        public List<TableColumn> Columns { get; set; } // List of columns with their settings
        public List<T> Data { get; set; }  // Data to be displayed in the table

        public TableModel()
        {
            Columns = [];
            Data = [];
        }
    }






    public class PdfTableGenerator
    {
        public void CreateTable<T>(Section section, TableModel<T> tableModel)
        {
            var table = new Table();

            // Create columns based on the column configuration
            foreach (var column in tableModel.Columns)
            {
                Column newColumn = table.AddColumn(Unit.FromCentimeter(column.Width));
                newColumn.Format.Alignment = column.Alignment;
            }

            // Add header row
            Row headerRow = table.AddRow();
            foreach (var column in tableModel.Columns)
            {
                var headerPar = headerRow.Cells[tableModel.Columns.IndexOf(column)].AddParagraph(column.HeaderName);

                // add markdown

            }

            // Add data rows
            foreach (var dataItem in tableModel.Data)
            {
                if (dataItem != null)
                {
                    Row dataRow = table.AddRow();
                    var properties = dataItem.GetType().GetProperties();


                    foreach (var column in tableModel.Columns)
                    {
                        PropertyInfo property = properties.FirstOrDefault(p => string.Equals(p.Name, column.Name, StringComparison.OrdinalIgnoreCase));

                        if (property != null)
                        {
                            int columnIndex = tableModel.Columns.IndexOf(column);
                            var value = property.GetValue(dataItem);
                            string formattedValue = value?.ToString();

                            if (!string.IsNullOrEmpty(column.Format))
                            {
                                string format = column.Format;

                                if (format.StartsWith("{") && format.EndsWith("}"))
                                {
                                    // format opgave bijvoorbeeld gedaan als "{0:D}"
                                }
                                else
                                {
                                    // 
                                    format = "{0:" + column.Format + "}";
                                }

                                formattedValue = string.Format(format, value);
                            }

                            dataRow.Cells[columnIndex].AddParagraph(formattedValue);
                        }
                    }
                }


                //foreach (var column in tableModel.Columns)
                //{



                //    // Find the property manually using a foreach loop (without lambda)
                //    PropertyInfo property = null;
                //    foreach (var p in properties)
                //    {
                //        if (string.Equals(p.Name, column.HeaderName, StringComparison.OrdinalIgnoreCase))
                //        {
                //            property = p;
                //            break;  // No need to continue once we find the matching property
                //        }
                //    }

                //    if (property != null)
                //    {
                //        int columnIndex = tableModel.Columns.IndexOf(column);
                //        var value = property.GetValue(dataItem);
                //        string formattedValue = value?.ToString();

                //        // Apply custom formatting if specified
                //        if (!string.IsNullOrEmpty(column.Format) && (value is int || value is double))
                //        {
                //            formattedValue = string.Format("{0:" + column.Format + "}", value);
                //        }

                //        // Add the formatted value to the corresponding cell
                //        dataRow.Cells[columnIndex].AddParagraph(formattedValue);
                //    }
                //}
            }

            // Add the table to the document
            section.Add(table);
        }


        public void MakeDemoTable(dynamic dataModel)
        {
            // Create a sample document
            Document doc = new();
            Section section = doc.AddSection();

            // Define a data model


            //var generator = new PdfTableGenerator();
            // Create a PDF table from the data model using GetTableModel
            var pdfGenerator = new PdfTableGenerator();
            var tableModel = pdfGenerator.GetTableModel(dataModel);  // Pass the dynamic data model

            pdfGenerator.CreateTable(section, tableModel);

            // Export the document to a PDF
            string filename = "wwwroot/demo/TableExample.pdf";
            PdfDocumentRenderer pdfRenderer = new(true);
            pdfRenderer.Document = doc;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(filename);
            Console.WriteLine($"Document saved to {filename}");

            filename = "wwwroot/demo/TableExample.rtf";
            RtfDocumentRenderer rtfRenderer = new();
            rtfRenderer.Render(doc, filename, null);







        }




        // Create a table model dynamically from a data model
        public TableModel<T> GetTableModel<T>(List<T> dataModel)
        {
            var tableModel = new TableModel<T>();



            // Get column headers by reflecting the properties of the first item in the data model
            var firstItem = dataModel.FirstOrDefault();
            if (firstItem != null)
            {
                var properties = firstItem.GetType().GetProperties();
                var columns = new List<TableColumn>();

                foreach (var property in properties)
                {
                    // Check if the propery has a ColumnAttribute
                    var columnAttribute = property.GetCustomAttribute<CustomColumnAttribute>();
                    if (columnAttribute != null && columnAttribute.Visible)
                    {
                        // Use values from attribute 
                        columns.Add(new TableColumn(
                            property.Name,
                            columnAttribute.HeaderName ?? property.Name,
                            columnAttribute.Format,
                            columnAttribute.Width,
                            columnAttribute.Alignment,
                            columnAttribute.ColumnPosition
                            ));
                    }
                    else if (columnAttribute == null)
                    {
                        // fallback to default
                        columns.Add(new TableColumn(property.Name));
                    }
                }

                // Sort columns based on ColumnPosition (if specified)
                columns = [.. columns.OrderBy(c => c.ColumnPosition)];

                // Add columns to the TableModel
                tableModel.Columns.AddRange(columns);

                // Add data rows
                tableModel.Data = dataModel;
            }

            return tableModel;
        }





    }















}
