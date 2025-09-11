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
        private static Services.DataTableMappingService _mappingService { get; } = new();
        private static Dictionary<string, AttributesMapping> _mappingDict { get; } = _mappingService.Data;


        public static MigraDoc.DocumentObjectModel.Document ToMigraDocDocument(this DataTable dataTable, Type objectType, bool isPivotTable = false, bool hideHeader = false, List<CommonLibrary.Melding>? meldingen = null)
        {



            // deze methode wordt ALLEEN gebruikt voor een leeg document met 
            // een ENKELE Tabel 
            // bijvoorbeeld voor export of voorbeeld doeleinde


            MigraDoc.DocumentObjectModel.Document document = new();
            var table = dataTable.ToTable(objectType, isPivotTable, hideHeader);



            if (table != null)
            {
                document.AddSection();
                document.LastSection.Add(table);

                // voeg de opmerkingen toe 
                if (meldingen != null && meldingen.Count != 0)
                {
                    var parMeldingen = document.LastSection.AddParagraph("Opmerkingen\r\n");
                    foreach (var melding in meldingen)
                    {
                        var inspecteer = melding.ToMarkupString().Value;

                        parMeldingen.AddText(melding.ToMarkupString().Value + "\r\n"); // Gebruik Markup (want gaat niet door markdown Parser)
                    }
                }
            }
            return document;
        }

        public static Table? ToTable(this DataTable dataTable, Type objectType, bool isPivotTable = true, bool hideHeader = false, bool vereenvoudigdeWeergave = true)
        {
            Table migraDocTable = new();

            migraDocTable.Borders.Width = 0.25;
            migraDocTable.Borders.Color = Colors.Transparent; // Mogelijk aanpassen voor debug
            migraDocTable.Borders.Visible = false;
            migraDocTable.KeepTogether = true; // Zorgt ervoor dat de tabel niet wordt gesplitst over pagina's

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

                // col[0]
                Column descriptionColumn = migraDocTable.AddColumn(); descriptionColumn.Width = "8cm";
                Column symbolColumn = migraDocTable.AddColumn(); symbolColumn.Width = "2cm";

                if (!vereenvoudigdeWeergave)
                {
                    Column articleColumn = migraDocTable.AddColumn(); articleColumn.Width = "2cm";
                }


                Column valueColumn = migraDocTable.AddColumn(); valueColumn.Width = "6cm";



                //for (int k = 0; k < dataTable.Rows.Count + 2; k++)   // col[0,1,2] zijn voor de header. 
                //                                                     // col[0]=Symbol
                //                                                     // col[1]=Description
                //                                                     // col[2]=Article

                //// of mogelijk [description][article][symbol][value]

                //{


                //    migraDocTable.AddColumn();
                //}

                int indexDescription = 0;
                int indexArticle = 1;
                int indexSymbol = 2;
                int indexValue = 1; // data starts at 


                if (vereenvoudigdeWeergave)
                {
                    indexDescription = 0;
                    indexArticle = -1;
                    indexSymbol = 1;
                    indexValue = 2;
                }
                else
                {
                    indexDescription = 0;
                    indexArticle = 1;
                    indexSymbol = 2;
                    indexValue = 3;
                }



                // Add rows
                foreach (var propertyWithAttribute in propertiesWithAttributes)
                {
                    Row row = migraDocTable.AddRow();
                    ParagraphAlignment alignment = propertyWithAttribute.Attribute.Alignment;
                    string headerText = propertyWithAttribute.Attribute.Description ?? propertyWithAttribute.Attribute.Label ?? propertyWithAttribute.Property.Name;

                    string? format = null;

                    AttributesMapping? mapping = null;

                    // als de property.attribute.key is gegeveven de mapping ophalen
                    if (propertyWithAttribute.Attribute.Key != null)
                    {
                        _mappingDict.TryGetValue(propertyWithAttribute.Attribute.Key, out mapping);
                    }
                    else
                    {
                        // geen specifieke key opgegeven, controleer of de name in het woordenboek staat
                        _mappingDict.TryGetValue(propertyWithAttribute.Property.Name, out mapping);
                    }




                    // mapping
                    if (mapping != null)
                    {
                        if (vereenvoudigdeWeergave)
                        {
                            var parEenvoudigeTekst = row.Cells[indexDescription].AddParagraph();
                            parEenvoudigeTekst.Tag = "description";
                            parEenvoudigeTekst.Format.Alignment = ParagraphAlignment.Left;
                            if (mapping.Description != null)
                            {
                                MigraDocCreator.AddMarkdownToParagraph(parEenvoudigeTekst, mapping.Description);
                            }

                            var parEenvoudgieSymbol = row.Cells[indexSymbol].AddParagraph();
                            parEenvoudgieSymbol.Tag = "symbol";
                            parEenvoudgieSymbol.Format.Alignment = ParagraphAlignment.Right;
                            if (mapping.Symbol != null)
                            {

                                //parEenvoudigeTekst.AddText(" ");
                                MigraDocCreator.AddMarkdownToParagraph(parEenvoudgieSymbol, mapping.Symbol);
                                //parEenvoudgieSymbol.AddText(" =");
                            }

                        }
                        else
                        {
                            if (mapping.Symbol != null)
                            {
                                var parSymbol = row.Cells[indexSymbol].AddParagraph();
                                parSymbol.Tag = "symbol";
                                MigraDocCreator.AddMarkdownToParagraph(parSymbol, mapping.Symbol);
                            }
                            if (mapping.Description != null)
                            {
                                var parDesc = row.Cells[indexDescription].AddParagraph();
                                parDesc.Tag = "description";
                                MigraDocCreator.AddMarkdownToParagraph(parDesc, mapping.Description);
                            }
                            if (mapping.Article != null)
                            {
                                var parArticle = row.Cells[indexArticle].AddParagraph();
                                parArticle.Tag = "article";
                                MigraDocCreator.AddMarkdownToParagraph(parArticle, mapping.Article);
                            }
                            if (mapping.Format != null)
                            {

                            }
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
                colIndex = indexValue; // set the column index for data values
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    int rowIndex = 0;

                    if (colIndex >= migraDocTable.Columns.Count)
                    {
                        // Add a new row for each data row in the DataTable
                        Console.WriteLine($"Te weining kolommen in migradoc tabel. Gedraaide tabel maximum bereikt.");
                        continue;

                        //Row migraDocRow = migraDocTable.AddRow();
                        //migraDocRow.Tag = "datarow"; // Optional: Tag for identification
                    }


                    //Column dataColumn = migraDocTable.AddColumn();
                    foreach (var columnName in columnNames)
                    {
                        //var columnName = dataTable.Columns[i].ColumnName;
                        var value = dataRow[columnName];
                        AttributesMapping? mapping = null;




                        // Get the column's custom string format if applied

                        int i = columnNames.IndexOf(columnName);

                        var rowProperties = propertiesWithAttributes[i];
                        string? format = rowProperties.Attribute.StringFormat;

                        bool formatGevonden = false;
                        try
                        {
                            var prop = propertiesWithAttributes.FirstOrDefault(p => p.Property?.Name == columnName);

                            if (prop != null)
                            {
                                _mappingDict.TryGetValue(prop.Attribute?.Key, out mapping);

                                if (mapping != null && mapping.Format != null)
                                {
                                    formatGevonden = true;
                                    format = mapping.Format;
                                }
                            }



                        }
                        catch (Exception ex)
                        {

                        }


                        if (!formatGevonden)
                        {
                            if (_mappingDict.ContainsKey(columnName))
                            {
                                var mapping3 = _mappingDict[columnName];


                                if (mapping3.Format != null)
                                {
                                    format = mapping3.Format;

                                }


                                //headerText = $"DICTIONARY{mapping.Symbol}";
                            }
                        }





                        if (value is bool myBool)
                        {
                            value = myBool ? "ja" : "nee";
                        }


                        if (value is double && string.IsNullOrEmpty(format))
                        {
                            // getallen altijd een default format meegeven;
                            format = "0.##e+0";
                        }



                        // Format the value if stringFormat exists
                        if (!string.IsNullOrEmpty(format))
                        {
                            // voor formats die beginnen met een 0 zoals 0.0 of 0.# moeten we controleren of ze niet wetenschappenlijk weergave nodig hebben.
                            if (format.StartsWith("0"))
                            {

                                if (value is double)
                                {
                                    // aanvulling, waardes groter dan 10000 gaan we wetenschappelijk aanpassen, sowieso.
                                    // ook als er per ongeluk geen rekening mee gehouden is.
                                    var waarde = (double)value;
                                    string suffix = "";
                                    if (waarde != 0 && Math.Abs(waarde) >= 10000)
                                    {
                                        int index = format.IndexOf('\t');  // Zoek de index van het tab-teken

                                        if (index != -1)  // Controleer of het tab-teken is gevonden
                                        {
                                            suffix = format.Substring(index);  // Haal alles vanaf de tab, inclusief de tab
                                            //Console.WriteLine(result);
                                        }

                                        format = $"0.##e+0{suffix}";
                                    }
                                }
                            }




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

                // header zichtbaar
                if (hideHeader)
                {
                    migraDocTable.Tag = "hideheader";
                }

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
                headerRow.Style = "TableHeader";

                foreach (var propertyWithAttribute in propertiesWithAttributes)
                {
                    ParagraphAlignment alignment = propertyWithAttribute.Attribute.Alignment;
                    string headerText = propertyWithAttribute.Attribute.Label ?? propertyWithAttribute.Property.Name;

                    // add a paragraph with and apply markdown (if any) to it.
                    var par = headerRow.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].AddParagraph();
                    MigraDocCreator.AddMarkdownToParagraph(par, headerText);

                    // set alignment
                    headerRow.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].Format.Alignment = alignment;

                    // set style
                    //headerRow.Cells[propertiesWithAttributes.IndexOf(propertyWithAttribute)].Format.Font.Italic = true;



                }

                // Add the data rows
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    Row row = migraDocTable.AddRow();
                    //row.Format.Font.Italic = false;

                    foreach (var columnName in columnNames)
                    {
                        //var columnName = dataTable.Columns[i].ColumnName;
                        var value = dataRow[columnName];




                        // Get the column's custom string format if applied
                        int i = columnNames.IndexOf(columnName);

                        var rowProperties = propertiesWithAttributes[i];

                        string? format = rowProperties?.Attribute?.StringFormat;

                        // Alleen foratteren als value geen getal is
                        if (value is IFormattable formattableValue)
                        {
                            // Gebruik standaard "0.#" als er geen format is opgegeven
                            if (string.IsNullOrEmpty(format))
                            {
                                format = "0.#";
                            }

                            if (!format.StartsWith("{"))
                            {
                                format = "{0:" + format + "}";
                            }

                            value = string.Format(format, formattableValue);


                        }

                        if (value is bool myBool)
                        {
                            value = myBool ? "ja" : "nee";
                        }




                        // Apply value to the cell (with markdown support)
                        var cellPar = row.Cells[i].AddParagraph();
                        MigraDocCreator.AddMarkdownToParagraph(cellPar, value?.ToString() ?? string.Empty);
                    }
                }
            }





            return migraDocTable;


        }
    }






}
