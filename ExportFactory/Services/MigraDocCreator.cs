using ExportFactory.MigraDocContentModels;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;

//using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace ExportFactory.Services
{
    public class MigraDocCreator
    {
        /// <summary>
        /// Create a document.
        /// </summary>
        /// <param name="content">DocumentContent for creating the document</param>
        /// <returns>A Migradoc Document</returns>
        public static Document GenerateDocument(DocumentContent content)
        {
            var document = new Document();
            SetProjectInfo(document, content);
            DefineStyles(document, content);

            // Add cover page
            if (content.CoverPage != null)
            {
                AddCoverPage(document, content.CoverPage);
            }

            // Add header and footer
            DefineHeaderAndFooter(document.AddSection(), content);

            // empty Tabel of Contents (TOC)
            AddTableOfContents(document, out Section tocSection);

            // save bookmarks to be used later in the TOC
            var bookmarks = new List<BookmarkContent>();

            // Add sections (iterate through all contents)
            foreach (var sectionContent in content.Sections)
            {
                AddSection(document, sectionContent, bookmarks);
            }

            // Bookmarks bijwerken (with saved bookmarks)
            UpdateTableOfContent(tocSection, bookmarks);

            return document;
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


        /// <summary>
        /// Adds a Section to a Document. Bookmarks are tracked/updated.
        /// </summary>
        /// <param name="document">a MigraDoc Document</param>
        /// <param name="sectionContent">Data content for this Section</param>
        /// <param name="bookmarks">List of bookmarks</param>
        private static void AddSection(Document document, SectionContent sectionContent, List<BookmarkContent> bookmarks)
        {
            var section = document.AddSection();

            // Sort elements by order
            var sortedElements = sectionContent.Elements.OrderBy(e => e.Order);

            foreach (var element in sortedElements)
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
                        var target = document.AddSection();




                        //frame.Left = "4cm"; // todo uitlijnen
                        AddTable(target, tableContent);
                        break;
                }
            }
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
            foreach (var row in tableContent.Rows)
            {
                var tableRow = table.AddRow();
                for (int i = 0; i < row.Count; i++)
                {
                    var cell = tableRow.Cells[i];
                    if (!string.IsNullOrEmpty(row[i].SvgImage))
                    {
                        try
                        {
                            var svgContent = row[i].SvgImage;
                            var imgStream = SvgService.ConvertSvgToPngStream(svgContent, out double width, out double height);

                            if (imgStream != null)
                            {
                                Console.WriteLine($"SVG converted successfully. Width: {width}, Height: {height}");
                                AddImageFromStream(cell, imgStream);
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

                    if (!string.IsNullOrEmpty(row[i].Markdown))
                    {
                        var par = cell.AddParagraph();
                        AddMarkdownToParagraph(par, row[i].Markdown);
                    }
                }
            }
        }



        public static void AddImageFromStream<T>(T target, Stream imageStream) where T : DocumentObject
        {
            // Convert the image stream to a Base64 string
            string base64Image = ConvertStreamToBase64(imageStream);

            if (target is Paragraph paragraph)
            {
                // Add image to Paragraph
                var image = paragraph.AddImage($"base64:{base64Image}");
                image.LockAspectRatio = true; // Maintain the aspect ratio
                image.Width = "9cm";          // Adjust size as needed
            }
            else if (target is Cell tableCell)
            {
                // Add image to TableCell
                var image = tableCell.AddImage($"base64:{base64Image}");
                image.LockAspectRatio = true; // Maintain the aspect ratio
                image.Width = "9cm";          // Adjust size as needed
            }
            else if (target is Section section)
            {
                // Add image to Section
                var image = section.AddImage($"base64:{base64Image}");
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
        /// Writes Info as Metadata to the document
        /// </summary>
        /// <param name="document">a MigraDoc document</param>
        /// <param name="content">the Data content</param>
        public static void SetProjectInfo(Document document, DocumentContent content)
        {
            document.Info.Author = content.CoverPage.CompanyName;
            document.Info.Title = content.CoverPage.Title;
            document.Info.Subject = content.CoverPage.Subtitle;
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
            tocStyle.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(17), TabAlignment.Right, TabLeader.MiddleDot);

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
            tableHeading.Font.Italic = true;




            // Title style for the cover page
            var title = document.Styles.AddStyle("Title", "Normal");
            title.Font.Size = 38;
            title.Font.Bold = !true;
            title.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Title style for the cover page
            var subTitle = document.Styles.AddStyle("Subtitle", "Normal");
            subTitle.Font.Size = 24;
            subTitle.Font.Italic = true;
            subTitle.Font.Bold = !true;
            subTitle.ParagraphFormat.Alignment = ParagraphAlignment.Center;


            // Header style
            var headerStyle = document.Styles.AddStyle("Header", "Normal");
            //headerStyle.Font.Size = 12;
            //headerStyle.Font.Bold = true;
            headerStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            headerStyle.Font.Color = content.HeaderColor;

            // Footer style
            var footerStyle = document.Styles.AddStyle("Footer", "Normal");
            footerStyle.Font.Size = 1.5 * content.Font.Size;
            footerStyle.Font.Color = content.FooterColor;
            footerStyle.Font.Bold = !true;
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
            section.PageSetup.HeaderDistance = 0;
            section.PageSetup.OddAndEvenPagesHeaderFooter = true;
            var header = section.Headers.Primary;

            //header.Format.LeftIndent = -Unit.FromMillimeter(15);

            // gebruik een textArea
            //var headerFrame = header.AddTextFrame();
            //headerFrame.Left = Unit.FromMillimeter(-15);

            // Add a table for the header
            var headerTable = header.AddTable();
            headerTable.Borders.Visible = false;
            headerTable.Borders.Bottom.Visible = true;
            headerTable.AddColumn(Unit.FromCentimeter(18)); // Full page width column
            headerTable.TopPadding = Unit.FromMillimeter(2);
            headerTable.BottomPadding = Unit.FromMillimeter(2);
            var headerRow = headerTable.AddRow();


            // Add document title in header
            var titleParagraph = headerRow.Cells[0].AddParagraph(content.PageHeader.Text);
            titleParagraph.Style = "Header";

            headerRow.Cells[0].Shading.Color = content.HeaderBackgroundColor;


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

            section.Headers.EvenPage = header.Clone();




            // Configure the footer to extend across the full page
            section.PageSetup.FooterDistance = 0; // Align footer at the bottom of the page
            var footer1 = section.Footers.Primary;

            // Add a table for the footer
            var footerTable = footer1.AddTable();
            footerTable.Borders.Visible = false;
            footerTable.TopPadding = Unit.FromMillimeter(2);
            footerTable.BottomPadding = Unit.FromMillimeter(4);
            footerTable.AddColumn(Unit.FromCentimeter(8));
            footerTable.AddColumn(Unit.FromCentimeter(2));
            footerTable.AddColumn(Unit.FromCentimeter(8));
            var footerRow = footerTable.AddRow();

            // Add a background color for the footer
            var footerCell = footerRow.Cells[1];
            footerCell.Style = "Footer";
            footerCell.Shading.Color = content.FooterBackgroundColor;
            //footerRow.Format.Shading.Color = content.AccentColor;


            // Add page number centered in the footer
            var pageNumberParagraph = footerRow.Cells[1].AddParagraph("");
            pageNumberParagraph.Style = "Footer";
            pageNumberParagraph.AddPageField();
            pageNumberParagraph.Format.Alignment = ParagraphAlignment.Center;


            var footer2 = section.Footers.EvenPage;
            var f2t = footer2.AddTable();
            f2t.Borders.Visible = false;
            f2t.TopPadding = Unit.FromMillimeter(2);
            f2t.BottomPadding = Unit.FromMillimeter(4);
            f2t.AddColumn(Unit.FromCentimeter(8));
            f2t.AddColumn(Unit.FromCentimeter(2));
            f2t.AddColumn(Unit.FromCentimeter(8));
            var f2r = f2t.AddRow();

            // Add a background color for the footer
            f2r.Cells[2].AddParagraph().AddPageField();
            f2r.Cells[2].Style = "Footer";
            f2r.Cells[2].Shading.Color = content.FooterBackgroundColor;






            // Set padding for full-width header/footer alignment
            section.PageSetup.LeftMargin = Unit.FromMillimeter(15);
            section.PageSetup.RightMargin = Unit.FromMillimeter(15);
            section.PageSetup.TopMargin = Unit.FromMillimeter(15);
            section.PageSetup.BottomMargin = Unit.FromMillimeter(15);
        }






        // Helper function to replace Greek letters in Markdown
        private static string ReplaceGreekLetters(string markdown)
        {
            foreach (var (key, value) in GreekLetters)
            {
                markdown = markdown.Replace($"|{key}|", value);
                //markdown = markdown.Replace($"\\{key}\\", value);
            }
            return markdown;
        }


        /// <summary>
        /// Adds a cover page to a document
        /// </summary>
        /// <param name="document">a MigraDoc Document</param>
        /// <param name="coverPage">Data content</param>
        private static void AddCoverPage(Document document, CoverPageContent coverPage)
        {
            var section = document.AddSection();

            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1);

            if (!string.IsNullOrEmpty(coverPage.CompanyLogoPath))
            {
                var logo = section.Headers.Primary.AddImage(coverPage.CompanyLogoPath);
                logo.Width = "5cm";
                logo.LockAspectRatio = true;
            }

            section.AddParagraph(coverPage.Title, "Title");
            section.AddParagraph(coverPage.Subtitle, "Subtitle");
            section.AddParagraph($"Project Number: {coverPage.ProjectNumber}", "Normal").Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph(coverPage.CompanyName, "Normal").Format.Alignment = ParagraphAlignment.Center;
        }


        private static void AddMarkdownToParagraph(Paragraph paragraph, string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            // Trim whitespace
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
            tocSection = document.LastSection;

            // Add a title for the TOC
            var titleParagraph = tocSection.AddParagraph("Inhoudsopgave", "Heading1");
            titleParagraph.Format.PageBreakBefore = false; // no page break for toc heading

        }






    }



}
