using CommonLibrary;
using ExportFactory.MigraDocContentModels;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Fonts;
using System.Reflection;


//using System.Reflection.Metadata;

//



//using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static ExportFactory.MigraDocContentModels.ExampleDocumentContent;

namespace ExportFactory.Services
{
    public class MigraDocCreator
    {
        /// <summary>
        /// Create a document.
        /// </summary>
        /// <param name="content">DocumentContent for creating the document</param>
        /// <returns>A Migradoc Document</returns>
        public static Document GenerateDocument(DocumentContent content, bool includeToc = false)
        {




            // Font resolver mag maar 1x gedaan worden!
            if (GlobalFontSettings.FontResolver is not CustomFontResolver)
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();
            }


            var document = new Document();
            SetDocumentInfo(document, content);
            DefineStyles(document, content);

            // Add cover page
            if (content.CoverPage != null)
            {
                AddCoverPage(document, content.CoverPage);
            }


            // empty Tabel of Contents (TOC)
            // gebruik een aparte section zodat later deze section kan worden gevuld.
            Section tocSection = new();
            if (includeToc)
            {
                AddTableOfContents(document, out tocSection);
            }


            // Add header and footer
            DefineHeaderAndFooter(document.LastSection, content);


            // save bookmarks to be used later in the TOC
            var bookmarks = new List<BookmarkContent>();


            // Add new section 
            document.AddSection();

            // Add sections (iterate through all contents)
            foreach (var sectionContent in content.Sections.OrderBy(sc => sc.Order))
            {
                AddSection(document, sectionContent, bookmarks);
            }

            // Bookmarks bijwerken (with saved bookmarks)
            if (includeToc)
                UpdateTableOfContent(tocSection, bookmarks);

            return document;
        }


        /// <summary>
        /// Exports the document to a RTF file.
        /// </summary>
        /// <param name="document">Migradoc document</param>
        /// <param name="filename">Filename</param>
        public static void ExportToRtf(Document document, string filename)
        {
            // Save the document as RTF
            var rtfRenderer = new MigraDoc.RtfRendering.RtfDocumentRenderer();

            // Check whether the filename ends with ".rtf" and add it if not
            if (!filename.EndsWith(".rtf"))
            {
                filename += ".rtf";
            }
            rtfRenderer.Render(document, filename, null);
        }

        public static string ExportToRtfString(Document document)
        {
            // Save the document as RTF
            var rtfRenderer = new MigraDoc.RtfRendering.RtfDocumentRenderer();
            return rtfRenderer.RenderToString(document, null);
        }

        public static void ExportToPdf(Document document, string filename)
        {
            // Save the document as PDF
            var pdfRenderer = new MigraDoc.Rendering.PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            // Check whether the filename ends with ".pdf" and add it if not
            if (!filename.EndsWith(".pdf"))
            {
                filename += ".pdf";
            }
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(filename);
        }


        public static void ExportToPdf(Document document, Stream stream)
        {
            // Save the document as PDF
            var pdfRenderer = new MigraDoc.Rendering.PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(stream, false);
        }

        public static byte[] ExportToPdfBytes(Document document)
        {
            using var stream = new MemoryStream();
            ExportToPdf(document, stream);
            return stream.ToArray();
        }



        private static readonly Dictionary<string, string> GreekLetters = new()
        {
            { "alpha", "α" },
            { "beta", "β" },
            { "gamma", "γ" },
            { "delta", "δ" },
            { "epsilon", "ε" },
            { "zeta", "ζ" },
            { "eta", "η" },
            { "theta", "θ" },
            { "iota", "ι" },
            { "kappa", "κ" },
            { "lambda", "λ" },
            { "mu", "μ" },
            { "nu", "ν" },
            { "xi", "ξ" },
            { "omicron", "ο" },
            { "pi", "π" },
            { "rho", "ρ" },
            { "sigma", "σ" },
            { "tau", "τ" },
            { "upsilon", "υ" },
            { "phi", "φ" },
            { "chi", "χ" },
            { "psi", "ψ" },
            { "omega", "ω" },
            { "Alpha", "Α" },
            { "Beta", "Β" },
            { "Gamma", "Γ" },
            { "Delta", "Δ" },
            { "Epsilon", "Ε" },
            { "Zeta", "Ζ" },
            { "Eta", "Η" },
            { "Theta", "Θ" },
            { "Iota", "Ι" },
            { "Kappa", "Κ" },
            { "Lambda", "Λ" },
            { "Mu", "Μ" },
            { "Nu", "Ν" },
            { "Xi", "Ξ" },
            { "Omicron", "Ο" },
            { "Pi", "Π" },
            { "Rho", "Ρ" },
            { "Sigma", "Σ" },
            { "Tau", "Τ" },
            { "Upsilon", "Υ" },
            { "Phi", "Φ" },
            { "Chi", "Χ" },
            { "Psi", "Ψ" },
            { "Omega", "Ω" }
        };


        private static readonly PageSetup CoverPageSetup = new PageSetup()
        {
            DifferentFirstPageHeaderFooter = false,
            HorizontalPageBreak = true,

            HeaderDistance = 0,
            FooterDistance = 0,
            OddAndEvenPagesHeaderFooter = true,
            PageFormat = PageFormat.A4,
            Orientation = Orientation.Portrait,

            MirrorMargins = true,
            LeftMargin = Unit.FromMillimeter(30), // inner
            TopMargin = Unit.FromMillimeter(30),
            BottomMargin = Unit.FromMillimeter(30),
            RightMargin = Unit.FromMillimeter(30), // outer
        };

        private static PageSetup GetPageSetupForDocument(DocumentContent content)
        {
            PageSetup pageSetup = new PageSetup()
            {


                DifferentFirstPageHeaderFooter = false,
                HorizontalPageBreak = true,   /// <summary>
                                              /// Gets or sets a value which defines whether a page should break horizontally.
                                              /// Currently only tables are supported.
                                              /// </summary>
                SectionStart = BreakType.BreakNextPage, /// start mogelijk op oneven pagina met nieuwe sectie! 
                                                        /// automatisch leeg blad wordt gemaakt.
                                                        /// NB. (dit wordt niet ondersteund in de RTF!!!, dus niet gebruiken)
                HeaderDistance = Unit.FromMillimeter(5),
                FooterDistance = Unit.FromMillimeter(5),
                OddAndEvenPagesHeaderFooter = true,
                PageFormat = PageFormat.A4,
                Orientation = Orientation.Portrait,
                MirrorMargins = true,
                LeftMargin = Unit.FromMillimeter(5), // inner
                TopMargin = Unit.FromMillimeter(15),
                BottomMargin = Unit.FromMillimeter(10),
                RightMargin = Unit.FromMillimeter(25), // outer

            };

            if (content.PageMarginSetting != null)
            {
                switch (content.PageMarginSetting)
                {
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.Gecentreerd:
                        pageSetup.LeftMargin = pageSetup.RightMargin = Unit.FromMillimeter(15);
                        pageSetup.OddAndEvenPagesHeaderFooter = false;
                        pageSetup.MirrorMargins = false;
                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.MargeLinks_PaginaNummerRechts:
                        pageSetup.LeftMargin = Unit.FromMillimeter(20);
                        pageSetup.RightMargin = Unit.FromMillimeter(10);
                        pageSetup.OddAndEvenPagesHeaderFooter = false;
                        pageSetup.MirrorMargins = false;
                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.EvenOnevenGespiegeld:
                        pageSetup.LeftMargin = Unit.FromMillimeter(10);
                        pageSetup.RightMargin = Unit.FromMillimeter(20);
                        pageSetup.OddAndEvenPagesHeaderFooter = true;
                        pageSetup.MirrorMargins = true;
                        break;

                }
            }



            return pageSetup;

        }


        /// <summary>
        /// Adds a Section to a Document. Bookmarks are tracked/updated.
        /// </summary>
        /// <param name="document">a MigraDoc Document</param>
        /// <param name="sectionContent">Data content for this Section</param>
        /// <param name="bookmarks">List of bookmarks</param>
        private static void AddSection(Document document, SectionContent sectionContent, List<BookmarkContent> bookmarks)
        {
            var section = document.LastSection;

            // If a Title is provided, een titel toevoegen aan deze sectie
            if (sectionContent.Title != null)
            {
                section.AddParagraph(sectionContent.Title, sectionContent.TitleStyle);
            }


            // Sort elements by order
            var sortedElements = sectionContent.Elements.OrderBy(e => e.Order);

            foreach (var element in sortedElements)
            {
                // Check if the element is a TableModel<T> where T is derived from BaseClass
                if (IsTableModelDerivedFromBaseClass(element))
                {
                    var tableModel = (TableModel<DemoDataClass>)element; // Cast to TableModel<BaseClass>
                    HandleTableModel(section, tableModel); // Handle the TableModel<BaseClass> case
                }
                else
                {
                    switch (element)
                    {
                        case HeadingContent headingContent:
                            AddHeading(section, headingContent, bookmarks);
                            break;

                        case ParagraphContent paragraphContent:
                            var par = section.AddParagraph("", paragraphContent.Style);
                            AddMarkdownToParagraph(par, paragraphContent.Markdown);
                            break;

                        case TableContent tableContent:
                            //section = document.AddSection(); // new section? needed for center
                            //var target = section.AddTextFrame();
                            var target = document.LastSection;




                            //frame.Left = "4cm"; // todo uitlijnen
                            AddTable(target, tableContent);
                            break;

                        case MigraDocTable table:
                            document.LastSection.Add(table.Table);
                            break;

                        case MigraDocElement migraDocElement:
                            if (migraDocElement.DocumentObject != null)
                            {
                                target = document.LastSection;

                                if (migraDocElement.DocumentObject is MigraDoc.DocumentObjectModel.Tables.Table table)
                                {
                                    target.Add(table);
                                }
                                else if (migraDocElement.DocumentObject is Paragraph paragraph)
                                {
                                    target.Add(paragraph);
                                }
                                else if (migraDocElement.DocumentObject is Chart chart)
                                {
                                    target.Add(chart);
                                }
                                else if (migraDocElement.DocumentObject is TextFrame textFrame)
                                {
                                    target.Add(textFrame);
                                }
                                else if (migraDocElement.DocumentObject is Image image)
                                {
                                    target.Add(image);
                                }

                            }

                            break;

                        // Handle other types if necessary
                        default:
                            break;

                    }





                }
            }
        }

        private static bool IsTableModelDerivedFromBaseClass(object element)
        {
            // Check if the element is a TableModel<T> and T is derived from BaseClass
            var elementType = element.GetType();

            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(TableModel<>))
            {
                var genericArgType = elementType.GetGenericArguments()[0]; // Get the type argument T
                return genericArgType.IsSubclassOf(typeof(BaseDataClass)); // Check if T is derived from BaseClass
            }

            return false;
        }



        private static bool IsTableModel(object element)
        {
            var type = element.GetType();
            var isGeneric = type.IsGenericType;
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var isTableModelT = genericTypeDefinition == typeof(TableModel<>);



            // Check if the element is of type TableModel<T> dynamically using reflection
            return
                element.GetType().IsGenericType &&
                element.GetType().GetGenericTypeDefinition() == typeof(TableModel<>);
        }


        // Generic method to handle different TableModel<T>
        private static void HandleTableModelBAK<T>(Section section, TableModel<T> tableModel)
        {
            // Your logic to process TableModel<T>
            // Example:
            AddTable(section, tableModel, $"{typeof(T).Name} Table");
        }


        // Generic method to handle any TableModel<T> where T is derived from BaseClass
        private static void HandleTableModel<T>(Section section, TableModel<T> tableModel) where T : BaseDataClass
        {
            // Process TableModel<T> here, where T is a type derived from BaseClass
            Console.WriteLine($"Handling table with {tableModel.Data.Count} rows of type {typeof(T).Name}");

            // Add logic for adding a table to the section
            AddTable(section, tableModel, $"{typeof(T).Name} Table");
        }



        public static Table AddTableToContainer<T>(T target) where T : DocumentObject
        {
            if (target is Section section)
            {
                // Add table to Section
                return section.AddTable();
            }
            else if (target is TextFrame textFrame)
            {
                // Add table to TextFrame
                return textFrame.Elements.AddTable();
            }
            else
            {
                throw new InvalidOperationException("Target must be a Section or TextFrame.");
            }
        }

        public static Paragraph AddParagraphToContainer<T>(T target) where T : DocumentObject
        {
            if (target is Section section)
            {
                return section.AddParagraph();
            }
            else if (target is TextFrame textFrame)
            {
                return textFrame.Elements.AddParagraph();
            }
            else
            {
                throw new InvalidOperationException("Target must be a Section of TextFrame.");
            }
        }



        private static void AddTable<T>(DocumentObject target, TableModel<T> tableModel, string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var par = AddParagraphToContainer(target);
                AddMarkdownToParagraph(par, title);
                par.Style = "TableHeading";
            }

            var table = AddTableToContainer(target);
            table.Borders.Width = 0.1;

            // haal de header op
            var firstRow = tableModel.Data.FirstOrDefault();
            if (firstRow != null)
            {
                var properties = firstRow.GetType().GetProperties();


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

            }




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
                var headerPar = headerRow.Cells[tableModel.Columns.IndexOf(column)].AddParagraph();

                // add markdown
                AddMarkdownToParagraph(headerPar, column.HeaderName);
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
            }
        }


        private static void AddTable(DocumentObject target, TableContent tableContent)
        {
            // title?
            if (!string.IsNullOrWhiteSpace(tableContent.Title))
            {
                var title = AddParagraphToContainer(target);
                title.AddText(tableContent.Title);
                title.Style = "TableHeading";
            }

            // table
            var table = AddTableToContainer(target);
            //var parent = target.Document.Styles;
            var defaultFont = target.Document.Styles["Normal"].Font;

            defaultFont ??= new Font("Arial", 9);

            //var _font = table.Format._font;

            // apply styling

            table.Borders.Width = 0.25; // todo apply formating with content
            //table.Borders.Left = new Border() { Visible = false };


            // Define columns
            foreach (var header in tableContent.Headers)
            {
                //if (tableContent.)
                var width = header.Width; // default

                // tenzij AutoSize is set
                switch (header.CellContent.Style.AutoSize)
                {
                    case AutoColumnSizeOption.None:
                    case AutoColumnSizeOption.NotSet:
                        // no action
                        break;
                    case AutoColumnSizeOption.ColumnHeader:
                        TextMeasurement tm = new(defaultFont);
                        var size = tm.MeasureString(header.CellContent.Markdown);
                        width = size.Width;
                        break;

                }

                table.AddColumn(width);
            }

            // Align table
            table.Format.Alignment = tableContent.Alignment == TableAlignment.Center
                ? ParagraphAlignment.Center
                : ParagraphAlignment.Left;

            // Add header row
            if (tableContent.HideHeaders)
                table.Tag = "hideheader"; // for HtmlCreator

            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            for (int i = 0; i < tableContent.Headers.Count; i++)
            {
                var cell = headerRow.Cells[i];
                var headerPar = cell.AddParagraph();
                AddMarkdownToParagraph(headerPar, tableContent.Headers[i].CellContent.Markdown);
                //cell.Style = "TableHeader";
            }




            // Add rows
            foreach (var rowCells in tableContent.Rows)
            {
                var tableRow = table.AddRow();
                for (int i = 0; i < rowCells.Count; i++)
                {
                    var cell = tableRow.Cells[i];
                    // controleer of een override op de width is
                    var currentWidth = table.Columns[i].Width;
                    var newWidth = rowCells[i].Width;
                    if (newWidth > 0 && newWidth != currentWidth)
                    {
                        table.Columns[i].Width = newWidth;
                    }

                    if (!string.IsNullOrEmpty(rowCells[i].SvgImage))
                    {
                        try
                        {
                            var svgContent = rowCells[i].SvgImage;
                            var imgStream = SvgService.ConvertSvgToPngStream(svgContent, out double width, out double height);

                            if (imgStream != null)
                            {
                                Console.WriteLine($"SVG converted successfully. Width: {width}, Height: {height}");
                                var parWithSvgImage = cell.AddParagraph();
                                parWithSvgImage.Tag = rowCells[i].SvgImage; // write svg to Tag for HtmlCreator.
                                AddImageFromStream(parWithSvgImage, imgStream);
                            }
                            else
                            {
                                Console.WriteLine("Failed to convert SVG.");
                            }

                        }
                        catch (Exception ex)
                        {
                            cell.AddParagraph($"Error rendering SVG: {ex.Message}");
                        }

                        //var image = cell.AddImage(CreateSvgImage(row[i].SvgImage));
                        //image.Width = "2cm"; // Adjust size as needed
                        //image.LockAspectRatio = true;
                    }

                    if (!string.IsNullOrEmpty(rowCells[i].Markdown))
                    {
                        var par = cell.AddParagraph();
                        AddMarkdownToParagraph(par, rowCells[i].Markdown);
                    }
                }
            }
        }



        public static void AddImageFromStream<T>(T target, Stream imageStream) where T : DocumentObject
        {
            // Convert the image stream to a Base64 string
            string base64Image = ConvertStreamToBase64(imageStream);
            string fileName = $"base64:{base64Image}";

            if (target is Paragraph paragraph)
            {
                // Add image to Paragraph
                var image = paragraph.AddImage(fileName);
                image.LockAspectRatio = true; // Maintain the aspect ratio
                image.Width = "9cm";          // Adjust size as needed
            }
            else if (target is Cell tableCell)
            {
                // Add image to TableCell
                var image = tableCell.AddImage(fileName);
                image.LockAspectRatio = true; // Maintain the aspect ratio
                image.Width = "9cm";          // Adjust size as needed
            }
            else if (target is Section section)
            {
                // Add image to Section
                var image = section.AddImage(fileName);
                image.LockAspectRatio = true;
                image.Width = "9cm";
            }
            else
            {
                throw new InvalidOperationException("Target must be a Section, Paragraph or TableCell.");
            }
        }

        public static string ConvertStreamToBase64(Stream stream)
        {
            stream.Position = 0; // Reset the stream to the start
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }



        private static void AddHeading(Section section, HeadingContent heading, List<BookmarkContent> bookmarks)
        {
            var paragraph = section.AddParagraph(heading.Text, heading.Style);

            if (heading.AddToTOC)
            {
                var bookmarkName = Guid.NewGuid().ToString();
                paragraph.AddBookmark(bookmarkName);

                // Determine level from the heading style
                //int level = heading.Style == "Heading1" ? 1 : 2;

                bookmarks.Add(new() { Title = heading.Text, BookmarkName = bookmarkName, Level = heading.Level });
            }
        }




        /// <summary>
        /// Writes Metadata to the document.
        /// </summary>
        /// <param name="document">a MigraDoc document</param>
        /// <param name="content">the Data content</param>
        public static void SetDocumentInfo(Document document, DocumentContent content)
        {
            document.Info = content.DocumentInfo;
        }


        /// <summary>
        /// Update the section with the Table of Content with bookmarks
        /// </summary>
        /// <param name="section">Section with TOC</param>
        /// <param name="bookmarks">List of bookmarks</param>
        private static void UpdateTableOfContent(Section section, List<BookmarkContent> bookmarks)
        {
            foreach (var bookmark in bookmarks)
            {
                var paragraph = section.AddParagraph();
                paragraph.Style = "TOC";

                // Add the clickable link to the bookmark
                var hyperlink = paragraph.AddHyperlink(bookmark.BookmarkName, HyperlinkType.Bookmark);
                hyperlink.AddFormattedText(bookmark.Title);
                paragraph.AddTab();
                paragraph.AddPageRefField(bookmark.BookmarkName);

                // indent heading2 and up.
                paragraph.Format.LeftIndent = Unit.FromMillimeter((bookmark.Level - 1) * 4);
            }
        }

        /// <summary>
        /// Set styling for the document
        /// </summary>
        /// <param name="document">a MigraDoc document</param>
        /// <param name="content">the data content</param>
        private static void DefineStyles(Document document, DocumentContent content)
        {
            var baseStyle = document.Styles["Normal"];
            baseStyle.Font = content.Font;
            baseStyle.Font.Bold = false; // explicitly set to false (for rtf export!)

            // Table of content style
            var tocStyle = document.Styles.AddStyle("TOC", "Normal");
            //tocStyle.Font.Size = 12;
            tocStyle.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(18), TabAlignment.Right, TabLeader.MiddleDot);

            // h1
            var heading1 = document.Styles.AddStyle("Heading1", "Normal");
            heading1.Font.Size = 1.5 * content.Font.Size;
            heading1.Font.Bold = !true;
            heading1.ParagraphFormat.PageBreakBefore = true;
            heading1.ParagraphFormat.SpaceAfter = "3mm";
            var kop1 = document.Styles.AddStyle("Kop 1", "Normal");
            kop1.Font.Size = 1.5 * content.Font.Size;
            kop1.Font.Bold = !true;
            kop1.ParagraphFormat.PageBreakBefore = true;
            kop1.ParagraphFormat.SpaceAfter = "3mm";

            // h2
            var heading2 = document.Styles.AddStyle("Heading2", "Normal");
            heading2.Font.Size = 1.25 * content.Font.Size;
            heading2.Font.Bold = !true;
            heading2.ParagraphFormat.SpaceBefore = "2mm";
            heading2.ParagraphFormat.SpaceAfter = "2mm";
            var kop2 = document.Styles.AddStyle("Kop 2", "Normal");
            kop2.Font.Size = 1.25 * content.Font.Size;
            kop2.Font.Bold = !true;
            kop2.ParagraphFormat.SpaceBefore = "2mm";
            kop2.ParagraphFormat.SpaceAfter = "2mm";


            // h3
            var heading3 = document.Styles.AddStyle("Heading3", "Normal");
            heading3.Font.Size = 1.00 * content.Font.Size;
            heading3.Font.Bold = !true;
            heading3.ParagraphFormat.SpaceBefore = "1mm";
            heading3.ParagraphFormat.SpaceAfter = "1mm";
            var kop3 = document.Styles.AddStyle("Kop 3", "Normal");
            kop3.Font.Size = 1.00 * content.Font.Size;
            kop3.Font.Bold = !true;
            kop3.ParagraphFormat.SpaceBefore = "1mm";
            kop3.ParagraphFormat.SpaceAfter = "1mm";

            // table heading
            var tableHeading = document.Styles.AddStyle("TableHeading", "Normal");
            //tableHeading.Font.Size = 12;
            tableHeading.Font.Italic = !true;




            // Title style for the cover page
            var title = document.Styles.AddStyle("Title", "Normal");
            title.Font.Size = 38;
            title.Font.Bold = !true;
            title.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Title style for the cover page
            var subTitle = document.Styles.AddStyle("Subtitle", "Normal");
            subTitle.Font.Size = 24;
            subTitle.Font.Italic = !true;
            subTitle.Font.Bold = !true;
            subTitle.ParagraphFormat.Alignment = ParagraphAlignment.Center;


            // Header style
            var headerStyle = document.Styles.AddStyle("Header", "Normal");
            headerStyle.Font.Size = 0.75 * content.Font.Size;
            headerStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            headerStyle.Font.Color = content.HeaderColor;

            // Footer style
            var footerStyle = document.Styles.AddStyle("Footer", "Normal");
            footerStyle.Font.Size = 0.75 * content.Font.Size;
            footerStyle.Font.Color = content.FooterColor;
            footerStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;


        }

        /// <summary>
        /// Defines and Adds a Header and Footer to a Document
        /// </summary>
        /// <param name="section">Section to apply on.</param>
        /// <param name="content">Data content</param>
        private static void DefineHeaderAndFooter(Section section, DocumentContent content)
        {
            ArgumentNullException.ThrowIfNull(section);

            section.PageSetup = GetPageSetupForDocument(content).Clone();


            var header = section.Headers.Primary;


            // Add a table for the header
            var headerTable = header.AddTable();
            headerTable.Borders.Visible = false;
            headerTable.Borders.Bottom.Visible = true;
            headerTable.AddColumn(Unit.FromCentimeter(6));
            headerTable.AddColumn(Unit.FromCentimeter(6));
            headerTable.AddColumn(Unit.FromCentimeter(6));

            //headerTable.AddColumn(Unit.FromCentimeter(18)); // Full page width column
            headerTable.TopPadding = Unit.FromMillimeter(2);
            headerTable.BottomPadding = Unit.FromMillimeter(1);
            var headerRow = headerTable.AddRow();


            // 
            int headerIndex = 1;
            if (content.PageMarginSetting.HasValue)
            {
                switch (content.PageMarginSetting.Value)
                {
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.Gecentreerd:
                        headerIndex = 1;
                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.MargeLinks_PaginaNummerRechts:
                        headerIndex = 1;
                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.EvenOnevenGespiegeld:
                        headerIndex = 1;
                        break;
                }
            }

            // Add document title in header
            var par = headerRow.Cells[0].AddParagraph(); AddMarkdownToParagraph(par, content.PageHeader.Text1); par.Format.Alignment = ParagraphAlignment.Left;
            par = headerRow.Cells[1].AddParagraph(); AddMarkdownToParagraph(par, content.PageHeader.Text2); par.Format.Alignment = ParagraphAlignment.Center;
            par = headerRow.Cells[2].AddParagraph(); AddMarkdownToParagraph(par, content.PageHeader.Text3); par.Format.Alignment = ParagraphAlignment.Right;


            headerRow.Shading.Color = content.HeaderBackgroundColor;
            headerRow.Style = "Header";
            //headerRow.Cells[0].Shading.Color = content.HeaderBackgroundColor;
            headerRow.Borders.Visible = false;
            headerRow.Borders.Bottom.Visible = true;
            headerRow.Borders.Bottom.Color = content.HeaderLineColor;

            // Add company logo as SVG in header
            if (!string.IsNullOrEmpty(content.PageHeader.SvgLogo))
            {
                var stream = SvgService.ConvertSvgToPngStream(content.PageHeader.SvgLogo, out _, out _);
                if (stream != null)
                {

                    var cell = headerRow.Cells[1];
                    try
                    {
                        AddImageFromStream(cell, stream);
                    }
                    catch (Exception ex)
                    {
                        cell.AddParagraph(ex.Message);
                    }


                    //logo.LockAspectRatio = true;
                    //logo. = Unit.FromCentimeter(2);
                    // todo wellicht hoogte breedte op kunnen geven in AddImageFromStream of in ConvertSvgToPngStream... uitzoeken
                }
            }
            //headerRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;

            // even same settings, so clone
            section.Headers.EvenPage = header.Clone();




            // PRIMARY
            var footer1 = section.Footers.Primary;


            int colIndex1 = 1;
            int colIndex11 = 0;
            int colIndex12 = 2;

            int colIndex2 = 1;
            int colIndex21 = 0;
            int colIndex22 = 2;

            ParagraphAlignment alignment1 = ParagraphAlignment.Center;
            ParagraphAlignment alignment11 = ParagraphAlignment.Center;
            ParagraphAlignment alignment12 = ParagraphAlignment.Center;

            ParagraphAlignment alignment2 = ParagraphAlignment.Center;
            ParagraphAlignment alignment21 = ParagraphAlignment.Center;
            ParagraphAlignment alignment22 = ParagraphAlignment.Center;

            if (content.PageMarginSetting.HasValue)
            {
                switch (content.PageMarginSetting.Value)
                {
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.Gecentreerd:
                        colIndex1 = 1;
                        colIndex11 = 0;
                        colIndex12 = 2;

                        colIndex2 = 1;
                        colIndex21 = 0;
                        colIndex22 = 2;

                        alignment1 = ParagraphAlignment.Center;
                        alignment11 = ParagraphAlignment.Left;
                        alignment12 = ParagraphAlignment.Right;

                        alignment2 = ParagraphAlignment.Center;
                        alignment21 = ParagraphAlignment.Left;
                        alignment22 = ParagraphAlignment.Right;


                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.MargeLinks_PaginaNummerRechts:
                        colIndex1 = 2;
                        alignment1 = ParagraphAlignment.Right;

                        colIndex11 = 0;
                        alignment11 = ParagraphAlignment.Left;

                        colIndex12 = 1;
                        alignment12 = ParagraphAlignment.Center;

                        colIndex2 = 2;
                        alignment2 = ParagraphAlignment.Right;

                        colIndex21 = 0;
                        alignment21 = ParagraphAlignment.Left;

                        colIndex22 = 1;
                        alignment22 = ParagraphAlignment.Center;



                        break;
                    case DocumentContent.PageMarginAndPageNumberSettingsEnum.EvenOnevenGespiegeld:
                        colIndex1 = 0;
                        alignment1 = ParagraphAlignment.Left;

                        colIndex11 = 2;
                        alignment11 = ParagraphAlignment.Right;

                        colIndex12 = 1;
                        alignment12 = ParagraphAlignment.Center;

                        colIndex2 = 2;
                        alignment2 = ParagraphAlignment.Right;

                        colIndex21 = 0;
                        alignment21 = ParagraphAlignment.Left;

                        colIndex22 = 1;
                        alignment22 = ParagraphAlignment.Center;
                        break;
                }
            }





            // Add a table for the footer
            var footerTable = footer1.AddTable();
            footerTable.Borders.Visible = false;
            footerTable.TopPadding = Unit.FromMillimeter(1);
            footerTable.BottomPadding = Unit.FromMillimeter(2);
            footerTable.AddColumn(Unit.FromCentimeter(6));
            footerTable.AddColumn(Unit.FromCentimeter(6));
            footerTable.AddColumn(Unit.FromCentimeter(6));
            var footerRow = footerTable.AddRow();
            footerRow.Style = "Footer";
            footerRow.Borders.Visible = false;
            footerRow.Borders.Width = 0.1;
            footerRow.Borders.Top.Visible = true;
            footerRow.Borders.Top.Color = content.FooterLineColor;

            // Add a background color for the footer
            var footerCell = footerRow.Cells[colIndex1];
            footerCell.Style = "Footer";
            footerCell.Shading.Color = content.FooterBackgroundColor;

            //footerRow.Format.Shading.Color = content.AccentColor;

            footerRow.Cells[colIndex1].AddParagraph("Pag. ").AddPageField();
            footerRow.Cells[colIndex1].Style = "Footer";
            footerRow.Cells[colIndex1].Format.Alignment = alignment1;


            par = footerRow.Cells[colIndex11].AddParagraph(); AddMarkdownToParagraph(par, content.PageFooter.Text1); par.Format.Alignment = alignment11;
            par = footerRow.Cells[colIndex12].AddParagraph(); AddMarkdownToParagraph(par, content.PageFooter.Text2); par.Format.Alignment = alignment12;



            // even page
            var footer2 = section.Footers.EvenPage;
            var f2t = footer2.AddTable();
            f2t.Borders.Visible = false;
            f2t.TopPadding = Unit.FromMillimeter(1);
            f2t.BottomPadding = Unit.FromMillimeter(2);
            f2t.AddColumn(Unit.FromCentimeter(6));
            f2t.AddColumn(Unit.FromCentimeter(6));
            f2t.AddColumn(Unit.FromCentimeter(6));
            var f2r = f2t.AddRow();
            f2r.Borders.Top.Visible = true;
            f2r.Borders.Top.Color = content.FooterLineColor;
            f2r.Style = "Footer";

            // Add a background color for the footer
            f2r.Cells[colIndex2].AddParagraph("Pag. ").AddPageField();
            f2r.Cells[colIndex2].Style = "Footer";
            f2r.Cells[colIndex2].Shading.Color = content.FooterBackgroundColor;
            f2r.Cells[colIndex2].Format.Alignment = alignment2;

            par = f2r.Cells[colIndex21].AddParagraph(); AddMarkdownToParagraph(par, content.PageFooter.Text1); par.Format.Alignment = alignment21;
            par = f2r.Cells[colIndex22].AddParagraph(); AddMarkdownToParagraph(par, content.PageFooter.Text2); par.Format.Alignment = alignment22;





        }





        // Helper function to replace Greek letters in Markdown
        public static string ReplaceGreekLetters(string markdown)
        {
            foreach (var (key, value) in GreekLetters)
            {
                markdown = markdown.Replace($"|{key}|", value);
                //markdown = markdown.Replace($"\\{key}\\", value);
            }
            return markdown;
        }


        private static void AddRevisionPage(Document document, RevisionContent content)
        {
            var section = document.AddSection();
            var table = section.AddTable();
            //var row = table.AddRow();

            //if (content.ColumnNames.TryGetValue("Name", out string? colNameHeaderTitle))
            //{
            //    table.AddColumn(Unit.FromMillimeter(15));
            //    row.Cells[table.Columns.Count - 1].AddParagraph(colNameHeaderTitle);
            //}
            //if (content.ColumnNames.TryGetValue("Date", out string? colDateHeaderTitle))
            //{
            //    table.AddColumn(Unit.FromMillimeter(30));
            //    row.Cells[table.Columns.Count - 1].AddParagraph(colDateHeaderTitle);
            //}
            //if (content.ColumnNames.TryGetValue("Description", out string? colDescriptionHeaderTitle))
            //{
            //    table.AddColumn(Unit.FromMillimeter(70));
            //    row.Cells[table.Columns.Count - 1].AddParagraph(colDescriptionHeaderTitle);
            //}







        }

        /// <summary>
        /// Adds a cover page to a document
        /// </summary>
        /// <param name="document">a MigraDoc Document</param>
        /// <param name="coverPage">Data content</param>
        private static void AddCoverPage(Document document, CoverPageContent coverPage)
        {
            var section = document.AddSection();
            section.Tag = "Voorblad";

            //document.Styles.
            //section.PageSetup.BackgroundColor = coverPage.BackgroundColor;
            // setup for page size and margin
            section.PageSetup = CoverPageSetup.Clone();


            if (!string.IsNullOrEmpty(coverPage.CompanyLogoPath))
            {
                var logo = section.Headers.Primary.AddImage(coverPage.CompanyLogoPath);
                logo.Width = "5cm";
                logo.LockAspectRatio = true;
            }

            section.AddParagraph(coverPage.Title ?? "", "Title");
            section.AddParagraph(coverPage.Subtitle ?? "", "Subtitle");
            section.AddParagraph($"Project Number: {coverPage.ProjectNumber}", "Normal").Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph(coverPage.CompanyName, "Normal").Format.Alignment = ParagraphAlignment.Center;
        }


        public static void AddMarkdownToParagraph(Paragraph paragraph, string? markdown, bool trim = false)
        {
            if (markdown == null) return;
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            // Trim whitespace
            if (trim)
                markdown = markdown.Trim();

            // Determine if the markdown is a heading
            if (markdown.StartsWith("# "))
            {
                // Heading1
                paragraph.AddFormattedText(markdown.Substring(2).Trim(), "Kop 1");

            }
            else if (markdown.StartsWith("## "))
            {
                // Heading2
                paragraph.AddFormattedText(markdown.Substring(3).Trim(), "Kop 2");
            }
            else if (markdown.StartsWith("### "))
            {
                // Heading3
                paragraph.AddFormattedText(markdown.Substring(4).Trim(), "Kop 3");
            }
            else
            {
                // Regular text (non-heading)
                AddStyledTextToParagraph(paragraph, markdown);
            }
        }




        private static void AddStyledTextToParagraph(Paragraph paragraph, string markdown)
        {
            // Parse for backticks first to handle raw text
            var rawSegments = markdown.Split('`');
            for (int i = 0; i < rawSegments.Length; i++)
            {
                if (i % 2 == 1) // Odd indices are raw text (inside backticks)
                {
                    paragraph.AddText(rawSegments[i]); // Add raw text without further parsing
                }
                else
                {
                    // Replace Greek letter placeholders
                    string processedText = ReplaceGreekLetters(rawSegments[i]);
                    // Parse and apply Markdown styles (bold, italic, underline, etc.) to non-raw segments
                    ApplyMarkdownStylesToParagraph(paragraph, processedText);
                }
            }
        }



        private static void ApplyMarkdownStylesToParagraph(Paragraph paragraph, string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            // HTML entities & <br />
            markdown = markdown.Replace("<br />", "\n");
            markdown = markdown.Replace("<br>", "\n");
            markdown = markdown.Replace("×", "x");   // want × is niet ondersteund in rtf
            markdown = markdown.Replace("³", "^3^"); // want ³ is niet ondersteund in rtf
            markdown = markdown.Replace("²", "^2^"); // want ² is niet ondersteund in rtf
            markdown = markdown.Replace("¹", "^1^"); // want ¹ is niet ondersteund in rtf
            markdown = markdown.Replace("‰", "^0^/~00~"); // want is niet ondersteund in rtf

            // Combine both Markdown & basic HTML tags into tokens
            var regex = new Regex(
                @"(?<bold>\*\*(.*?)\*\*|<b>(.*?)</b>)|" +
                @"(?<italic>\*(.*?)\*|<i>(.*?)</i>)|" +
                @"(?<underline>__(.*?)__|<u>(.*?)</u>)|" +
                @"(?<strike>~~(.*?)~~)|" +
                @"(?<sup>\^(.*?)\^|<sup>(.*?)</sup>)|" +
                @"(?<sub>~(.*?)~|<sub>(.*?)</sub>)|" +
                @"(?<color>\{(.*?):(.*?)\})|" +
                @"(?<br>\n)|" +
                @"(?<text>[^*^~_<>{}\n]+)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );

            foreach (Match match in regex.Matches(markdown))
            {
                if (match.Groups["bold"].Success)
                {
                    var content = StripTags(match.Value, "**", "<b>", "</b>");
                    paragraph.AddFormattedText(content, TextFormat.Bold);
                }
                else if (match.Groups["italic"].Success)
                {
                    var content = StripTags(match.Value, "*", "<i>", "</i>");
                    paragraph.AddFormattedText(content, TextFormat.Italic);
                }
                else if (match.Groups["underline"].Success)
                {
                    var content = StripTags(match.Value, "__", "<u>", "</u>");
                    var text = paragraph.AddFormattedText(content);
                    text.Underline = Underline.Single;
                }
                else if (match.Groups["strike"].Success)
                {
                    var content = StripTags(match.Value, "~~");
                    var text = paragraph.AddFormattedText(content);
                    text.Color = Colors.Red; // MigraDoc heeft geen strikeout, dus markeer visueel
                }
                else if (match.Groups["sup"].Success)
                {
                    var content = StripTags(match.Value, "^", "<sup>", "</sup>");
                    var text = paragraph.AddFormattedText(content);
                    text.Superscript = true;
                }
                else if (match.Groups["sub"].Success)
                {
                    var content = StripTags(match.Value, "~", "<sub>", "</sub>");
                    var text = paragraph.AddFormattedText(content);
                    text.Subscript = true;
                }
                else if (match.Groups["color"].Success)
                {
                    var parts = Regex.Match(match.Value, @"\{(.*?):(.*?)\}").Groups;
                    var color = parts[1].Value.Trim();
                    var content = parts[2].Value.Trim();
                    var text = paragraph.AddFormattedText(content);
                    text.Color = Color.Parse(color);
                }
                else if (match.Groups["br"].Success)
                {
                    paragraph.AddLineBreak();
                }
                else if (match.Groups["text"].Success)
                {
                    paragraph.AddText(match.Value);
                }
            }
        }

        private static string StripTags(string input, string markdown = "", string htmlOpen = "", string htmlClose = "")
        {
            return input.Replace(markdown, "")
                        .Replace(htmlOpen, "", StringComparison.OrdinalIgnoreCase)
                        .Replace(htmlClose, "", StringComparison.OrdinalIgnoreCase);
        }



        private static void ApplyMarkdownStylesToParagraphBAK(Paragraph paragraph, string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return;
            }

            // Regex to detect various Markdown patterns
            var matches = Regex.Matches(markdown, @"(\*\*.*?\*\*|\*.*?\*|__.*?__|~~.*?~~|\^.*?\^|~.*?~|\{.*?:.*?\}|[^_\*\^\{\}~]+)");
            foreach (Match match in matches)
            {
                var token = match.Value;

                if (token.StartsWith("**") && token.EndsWith("**")) // Bold
                {
                    paragraph.AddFormattedText(token.Trim('*'), TextFormat.Bold);
                }
                else if (token.StartsWith("*") && token.EndsWith("*")) // Italic
                {
                    paragraph.AddFormattedText(token.Trim('*'), TextFormat.Italic);
                }
                else if (token.StartsWith("__") && token.EndsWith("__")) // Underline
                {
                    var formattedText = paragraph.AddFormattedText(token.Trim('_'));
                    formattedText.Underline = Underline.Single;
                }
                else if (token.StartsWith("~~") && token.EndsWith("~~")) // Strikethrough
                {
                    var formattedText = paragraph.AddFormattedText(token.Trim('~'));
                    formattedText.Color = Colors.Red;
                }
                else if (token.StartsWith("^") && token.EndsWith("^")) // Superscript
                {
                    var formattedText = paragraph.AddFormattedText(token.Trim('^'));
                    formattedText.Superscript = true;
                }
                else if (token.StartsWith("~") && token.EndsWith("~")) // Subscript
                {
                    var formattedText = paragraph.AddFormattedText(token.Trim('~'));
                    formattedText.Subscript = true;
                }
                else if (token.StartsWith("{") && token.Contains(":") && token.EndsWith("}")) // Color
                {
                    var parts = token.Trim('{', '}').Split(':');
                    var color = parts[0];
                    var text = parts[1];
                    var formattedText = paragraph.AddFormattedText(text);
                    formattedText.Color = Color.Parse(color);
                }
                else // Plain text
                {
                    paragraph.AddText(token);
                }
            }
        }



        /// <summary>
        /// Adds a emtpy Section for the Table Of Content to the document. This section is inserted into the document so it can be filled with a TOC after all the Headings with Bookmarks are inserted.
        /// </summary>
        /// <param name="document">a MigraDoc Document</param>
        /// <param name="tocSection">Section with TOC</param>
        private static void AddTableOfContents(Document document, out Section tocSection)
        {
            // Create a TOC section
            tocSection = document.AddSection();
            tocSection.Tag = "TOC";

            // Add a title for the TOC
            var titleParagraph = tocSection.AddParagraph("Inhoudsopgave", "Heading1");
            titleParagraph.Format.PageBreakBefore = false; // no page break for toc heading

        }






    }



}
