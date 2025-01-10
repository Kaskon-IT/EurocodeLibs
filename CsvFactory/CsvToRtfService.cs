using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace CsvFactory
{


    public class CsvToRtfService
    {
        /// <summary>
        /// Creates an RTF table from CSV content and saves it to a file.
        /// </summary>
        /// <param name="csvContent">The CSV content as a string.</param>
        /// <param name="outputPath">The file path to save the RTF.</param>
        /// <param name="delimiter">The character used to separate values in the CSV.</param>
        public void CreateRtfFromCsv(string csvContent, string outputPath, char delimiter = ',')
        {
            var lines = csvContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) throw new ArgumentException("CSV content must have at least one line.");

            var headers = lines[0].Split(delimiter);
            var document = new Document();
            var section = document.AddSection();

            // Add table to document
            var table = section.AddTable();
            table.Borders.Width = 0.75;

            // Define columns
            foreach (var header in headers)
            {
                var column = table.AddColumn(Unit.FromCentimeter(3));
                column.Format.Alignment = ParagraphAlignment.Center;
            }

            // Add header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells[i].AddParagraph(headers[i]);
                headerRow.Cells[i].Format.Font.Bold = true;
            }

            // Add data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var row = table.AddRow();
                var values = lines[i].Split(delimiter);
                for (int j = 0; j < headers.Length; j++)
                {
                    var cellText = j < values.Length ? values[j] : string.Empty;

                    // Check for subscript and superscript notation (e.g., "H_2~O" or "x^2~") and apply formatting
                    var paragraph = row.Cells[j].AddParagraph();
                    foreach (var part in SplitForSpecialFormatting(cellText))
                    {
                        var formattedText = paragraph.AddFormattedText(part.Text);
                        if (part.IsSubscript)
                        {
                            formattedText.Subscript = true;
                        }
                        else if (part.IsSuperscript)
                        {
                            formattedText.Superscript = true;
                        }
                    }
                }
            }

            // Render and save to file
            var renderer = new PdfDocumentRenderer(true)
            {
                Document = document
            };
            renderer.RenderDocument();

            // Save as RTF
            //var rffRenderer = new MigraDocCore.Rendering.DocumentRenderer(document);
            //rffRenderer.
            //var rtfRenderer = new RtfDocumentRenderer();
            //rtfRenderer.Render(document, outputPath, null);
        }

        /// <summary>
        /// Splits a string into parts for normal, subscript, and superscript formatting.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>A list of text parts with formatting metadata.</returns>
        private static List<TextPart> SplitForSpecialFormatting(string text)
        {
            var parts = new List<TextPart>();
            var current = string.Empty;
            var formatting = FormattingType.Normal;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '_') // Start subscript
                {
                    if (current != string.Empty)
                    {
                        parts.Add(new TextPart(current, formatting));
                        current = string.Empty;
                    }
                    formatting = FormattingType.Subscript;
                }
                else if (c == '^') // Start superscript
                {
                    if (current != string.Empty)
                    {
                        parts.Add(new TextPart(current, formatting));
                        current = string.Empty;
                    }
                    formatting = FormattingType.Superscript;
                }
                else if (c == '~') // End subscript/superscript
                {
                    if (current != string.Empty)
                    {
                        parts.Add(new TextPart(current, formatting));
                        current = string.Empty;
                    }
                    formatting = FormattingType.Normal;
                }
                else
                {
                    current += c;
                }
            }

            if (current != string.Empty)
            {
                parts.Add(new TextPart(current, formatting));
            }

            return parts;
        }

        /// <summary>
        /// Represents a part of text with special formatting metadata.
        /// </summary>
        private class TextPart
        {
            public string Text { get; }
            public bool IsSubscript => Formatting == FormattingType.Subscript;
            public bool IsSuperscript => Formatting == FormattingType.Superscript;
            public FormattingType Formatting { get; }

            public TextPart(string text, FormattingType formatting)
            {
                Text = text;
                Formatting = formatting;
            }
        }

        private enum FormattingType
        {
            Normal,
            Subscript,
            Superscript
        }
    }

}
