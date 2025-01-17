using ExportFactory.MigraDocContentModels;
using MigraDoc.DocumentObjectModel;
using System.Text.RegularExpressions;

namespace ExportFactory.Services
{
    public class MigraDocCreator
    {

        private static string CreateSvgImage(string svgContent)
        {
            // Save SVG content to a temporary file and return the path
            var tempPath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempPath, svgContent);
            return tempPath;
        }

        private static void AddTable(Section section, TableContent tableContent)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.75;

            // Define columns
            foreach (var header in tableContent.Headers)
            {
                table.AddColumn(Unit.FromCentimeter(4)); // Adjust column width as needed
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
                cell.AddParagraph(tableContent.Headers[i]);
                cell.Style = "TableHeader";
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
                        var image = cell.AddImage(CreateSvgImage(row[i].SvgImage));
                        image.Width = "2cm"; // Adjust size as needed
                        image.LockAspectRatio = true;
                    }

                    if (!string.IsNullOrEmpty(row[i].Markdown))
                    {
                        var par = cell.AddParagraph();
                        AddMarkdownToParagraph(par, row[i].Markdown);
                    }
                }
            }
        }

        private static void AddHeading(Section section, HeadingContent heading, List<BookmarkContent> bookmarks)
        {
            var paragraph = section.AddParagraph(heading.Text, heading.Style);

            if (heading.AddToTOC)
            {
                var bookmarkName = Guid.NewGuid().ToString();
                paragraph.AddBookmark(bookmarkName);

                // Determine level from the heading style
                int level = heading.Style == "Heading1" ? 1 : 2;

                bookmarks.Add(new() { Title = heading.Text, BookmarkName = bookmarkName, Level = level });
            }
        }


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
                        var par = section.AddParagraph();
                        AddMarkdownToParagraph(par, paragraphContent.Markdown);
                        break;

                    case TableContent tableContent:
                        AddTable(section, tableContent);
                        break;
                }
            }
        }


        public static Document GenerateDocument(DocumentContent content)
        {
            var document = new Document();
            DefineStyles(document);

            // Add cover page
            if (content.CoverPage != null)
            {
                AddCoverPage(document, content.CoverPage);
            }

            // Add Table of Contents (TOC)
            if (content.TableOfContents != null)
            {
                AddTableOfContents(document, content.TableOfContents);
            }

            // Add sections
            foreach (var sectionContent in content.Sections)
            {
                var bookmarks = new List<BookmarkContent>();
                AddSection(document, sectionContent, bookmarks);
            }

            return document;
        }

        private static void DefineStyles(Document document, List<DefineStylesContent> customStyles = null)
        {
            var baseStyle = document.Styles["Normal"];
            baseStyle.Font.Name = "Arial Narrow";
            baseStyle.Font.Size = 10;

            var heading1 = document.Styles.AddStyle("Heading1", "Normal");
            heading1.Font.Size = 18;
            heading1.Font.Bold = true;
            heading1.ParagraphFormat.SpaceAfter = "0.5cm";
            heading1.ParagraphFormat.PageBreakBefore = true;

            var heading2 = document.Styles.AddStyle("Heading2", "Normal");
            heading2.Font.Size = 14;
            heading2.Font.Bold = true;

            var heading3 = document.Styles.AddStyle("Heading3", "Normal");
            heading3.Font.Size = 12;
            heading3.Font.Bold = true;

            var tableHeader = document.Styles.AddStyle("TableHeader", "Normal");
            tableHeader.Font.Bold = true;
            tableHeader.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            tableHeader.ParagraphFormat.Shading.Color = Colors.LightGray;

            // Apply custom styles if provided
            if (customStyles != null)
            {
                foreach (var style in customStyles)
                {
                    var newStyle = document.Styles.AddStyle(style.Name, "Normal");
                    newStyle.Font.Name = style.FontName;
                    newStyle.Font.Size = style.FontSize;
                    newStyle.Font.Bold = style.IsBold;
                    newStyle.Font.Italic = style.IsItalic;
                    newStyle.Font.Color = Colors.Black; // todo parser for color
                    newStyle.ParagraphFormat.SpaceBefore = Unit.FromPoint(style.SpaceBefore);
                    newStyle.ParagraphFormat.SpaceAfter = Unit.FromPoint(style.SpaceAfter);
                }
            }
        }


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

            section.AddParagraph(coverPage.Title, "Heading1").Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph(coverPage.Subtitle, "Heading2").Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph($"Project Number: {coverPage.ProjectNumber}", "Normal").Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph(coverPage.CompanyName, "Normal").Format.Alignment = ParagraphAlignment.Center;
        }

        private static void AddMarkdownToParagraph(Paragraph paragraph, string markdown)
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



        private static void AddTableOfContents(Document document, TableOfContentsContent tocContent)
        {
            var section = document.AddSection();
            section.AddParagraph(tocContent.Title, "Heading1");
            section.AddParagraph("[Place TOC here - auto-generate if needed]");
        }

        private static void AddTableToSection(Section section, TableContent tableContent)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;

            // Add columns
            foreach (var columnWidth in tableContent.ColumnWidths)
            {
                var column = table.AddColumn(Unit.FromCentimeter(columnWidth));
                column.Format.Alignment = ParagraphAlignment.Center;
            }

            // Add rows and cells
            foreach (var rowContent in tableContent.Rows)
            {
                var row = table.AddRow();
                for (int i = 0; i < rowContent.Count; i++)
                {
                    var cell = row.Cells[i];
                    var cellContent = rowContent[i];

                    // Apply Markdown formatting to the cell content
                    var paragraph = cell.AddParagraph();
                    AddMarkdownToParagraph(paragraph, cellContent.Markdown);

                    // Apply col/row spans
                    cell.MergeRight = cellContent.ColSpan - 1;
                    cell.MergeDown = cellContent.RowSpan - 1;
                }
            }
        }




    }



}
