//using MigraDocCore.DocumentObjectModel;
//using MigraDocCore.Rendering;
//using RtfPipe;

using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using System.Text;
//using static System.Net.Mime.MediaTypeNames;

namespace CsvFactory
{


    public class CsvToRtfService
    {



        private string ApplyFormatting(string text, FormattingType formatting)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return formatting switch
            {
                FormattingType.Subscript => $"{{\\sub {text}}}",
                FormattingType.Superscript => $"{{\\super {text}}}",
                _ => text
            };
        }




        /// <summary>
        /// Get minimum width for a column, based on it's content
        /// </summary>
        /// <returns>Minimum width of a column</returns>
        public XUnit GetMinimumColumnWidth(string[] cellTexts, Font font)
        {
            XUnit minWidth = XUnit.FromMillimeter(10);
            string testedExtraTextForMargin = "00"; // geteste tekst die zorgt voor de juiste marge.NB. Let op! Rtf-export heeft net meer marge nodig dan PDF.
            TextMeasurement tm = new(font);
            foreach (var cellText in cellTexts)
            {
                XUnit textWidth = 0.00;
                Paragraph par = new Paragraph();
                ApplySubSuperscript(cellText, par);

                foreach (var element in par.Elements)
                {
                    if (element is FormattedText ft)
                    {
                        foreach (var subElement in ft.Elements)
                        {
                            if (subElement is Text txt)
                            {
                                textWidth += XUnit.FromMillimeter(tm.MeasureString(txt.Content + testedExtraTextForMargin).Width);
                            }
                        }
                    }
                    else if (element is Text txt)
                    {
                        textWidth += XUnit.FromMillimeter(tm.MeasureString(txt.Content + testedExtraTextForMargin).Width);
                    }

                    // geef max door als groter dan eerder gevonden.
                    if (textWidth.Millimeter > minWidth.Millimeter)
                    {
                        minWidth = textWidth;


                    }


                }


            }
            return minWidth;
        }

        public void ApplySubSuperscript(string cellText, Paragraph par)
        {
            var parts = SplitForSpecialFormatting(cellText);
            foreach (var part in parts)
            {
                var formattedText = par.AddFormattedText(part.Text);
                if (part.IsSubscript)
                {
                    formattedText.Subscript = true;
                }
                else if (part.IsSuperscript)
                {
                    formattedText.Superscript = true;
                }
                else if (parts.Count > 1)
                {
                    formattedText.Italic = true;
                }
            }
        }








        /// <summary>
        /// Creates an RTF table from CSV content and saves it to a file.
        /// </summary>
        /// <param name="csvContent">The CSV content as a string.</param>
        /// <param name="outputPath">The file path to save the RTF.</param>
        /// <param name="delimiter">The character used to separate values in the CSV.</param>
        public string? CreateRtfFromCsvUsingMigradoc(string csvContent, string outputPath, char delimiter = '\t')
        {
            string? rtfContent = null;
            var lines = csvContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) throw new ArgumentException("CSV content must have at least one line.");

            var headers = lines[0].Split(delimiter);

            var document = new Document();
            var section = document.AddSection();

            // set font 
            var font = new MigraDoc.DocumentObjectModel.Font("Calibri", "9pt");

            // Add table to document
            var table = section.AddTable();
            table.Borders.Width = 0.75;

            // Define columns
            for (int i = 0; i < headers.Length; i++)
            {
                // headers
                string[] colValues = new string[lines.Length];
                for (int j = 0; j < lines.Length; j++)
                {
                    //var rowData = lines[j].Split(delimiter)[i];
                    colValues[j] = lines[j].Split(delimiter)[i];
                }

                // split
                List<string> values = new List<string>();
                foreach (var value in colValues)
                {
                    values.AddRange(value.Split(" "));
                }
                var minSize = GetMinimumColumnWidth([.. values], font);
                var column = table.AddColumn(minSize.Millimeter);
                column.Format.Alignment = ParagraphAlignment.Center;
                column.Format.Font = font.Clone();
            }


            // Add header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            for (int i = 0; i < headers.Length; i++)
            {
                var cellText = i < headers.Length ? headers[i] : string.Empty;
                var headerPar = headerRow.Cells[i].AddParagraph();
                // Check for subscript and superscript notation (e.g., "H_2~O" or "x^2~") and apply formatting
                ApplySubSuperscript(cellText, headerPar);
                headerRow.Cells[i].Format.Font.Bold = !true;
            }

            // Add data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var row = table.AddRow();
                var values = lines[i].Split(delimiter);
                for (int j = 0; j < headers.Length; j++)
                {
                    var cellText = j < values.Length ? values[j] : string.Empty;
                    var paragraph = row.Cells[j].AddParagraph();
                    //paragraph.AddLineBreak();
                    // Check for subscript and superscript notation (e.g., "H_2~O" or "x^2~") and apply formatting
                    ApplySubSuperscript(cellText, paragraph);
                }
            }

            //Save the document as RTF
            var migraDocRtf = new MigraDoc.RtfRendering.RtfDocumentRenderer();
            migraDocRtf.Render(document, "output.rtf", null);

            using (var streamReader = new StreamReader(@"output.rtf", Encoding.UTF8))
            {
                rtfContent = streamReader.ReadToEnd();
            }


            //var rtfRenderer = new MigraDoc.RtfRendering
            //var renderer = new RtfDocumentRenderer();
            //renderer.Render(document, outputPath, null);


            // 
            //XPdfFontOptions options = new XPdfFontOptions(PdfSharp.Pdf.PdfFontEncoding.Unicode);

            // Render the document using PdfSharp
            var pdfRenderer = new PdfDocumentRenderer(true) // let op unicode moet op TRUE staan!
            {
                Document = document
            };
            pdfRenderer.RenderDocument();

            pdfRenderer.PdfDocument.Save(outputPath);


            return rtfContent;
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
