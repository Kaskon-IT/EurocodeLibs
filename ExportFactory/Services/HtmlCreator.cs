namespace ExportFactory.Services
{
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Tables;
    using System.Text;

    public class HtmlCreator
    {
        // Methode om HTML te genereren van een MigraDoc Document
        public static string GenerateHtmlFromDocument(Document document, bool centered = true)
        {
            StringBuilder htmlBuilder = new StringBuilder();



            // Extract font style from the first section (this can be modified to extract from styles, paragraphs, etc.)
            var fontStyleString = ExtractFontStyles(document, out string fontFamily);

            // Begin van de HTML
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");

            // Voeg een stijl toe aan de pagina (CSS)
            htmlBuilder.AppendLine("<style>");
            htmlBuilder.AppendLine($"body {{ font-family: {fontFamily}, sans-serif; line-height: 1.6; }}");
            htmlBuilder.AppendLine("h1 { font-size: 2.5em; }");
            htmlBuilder.AppendLine("h2 { font-size: 2em;  }");
            htmlBuilder.AppendLine("h3 { font-size: 1.75em; }");
            htmlBuilder.AppendLine("h4 { font-size: 1.5em;  }");
            htmlBuilder.AppendLine($"p {{ margin: 10px 0; font-size: 1.1em; font-family:{fontFamily}; line-height: 1.6; }}");
            htmlBuilder.AppendLine("b { font-weight: bold; }");
            htmlBuilder.AppendLine("i { font-style: italic; }");
            htmlBuilder.AppendLine("sub { font-size: 0.8em; vertical-align: sub; }");
            htmlBuilder.AppendLine("sup { font-size: 0.8em; vertical-align: super; }");
            htmlBuilder.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            //htmlBuilder.AppendLine("table, th, td { border: 1px solid #ddd; }");
            htmlBuilder.AppendLine("th, td { padding: 8px 12px; text-align: left; border-bottom: 1px solid #f4f4f4; }");
            //htmlBuilder.AppendLine("th { background-color: #f4f4f4; font-weight: bold; }");
            htmlBuilder.AppendLine("img { max-width: 100%; height: auto; display: block; margin: 20px 0; }");
            htmlBuilder.AppendLine("</style>");

            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");



            // Apply centered style if the 'centered' parameter is true
            string containerStyle = centered ? "text-align: center; margin: 0 auto; max-width: 680px;" : "text-align: left; max-width: 680px;";

            // Wrap the entire content in a container with the appropriate style
            htmlBuilder.Append($"<div style='{containerStyle} {fontStyleString}'>");

            // TOC
            //htmlBuilder.Append(GenerateHtmlFromBookmarks(document));



            // Loop door de secties en paragrafen in het document
            foreach (Section section in document.Sections)
            {
                foreach (var element in section.Elements)
                {


                    // Als het een paragraaf is
                    if (element is Paragraph paragraph)
                    {
                        switch (paragraph.Style)
                        {
                            case "Title":
                            case "Heading1":
                            case "Heading 1":
                            case "Kop 1":
                                htmlBuilder.AppendLine("<h1>" + ProcessParagraph(paragraph) + "</h1>");

                                if (paragraph.Elements.First != null && paragraph.Elements.First is Text text)
                                {
                                    if (text.Content.ToLower() == "inhoudsopgave")
                                    {
                                        // TOC hyperlinks
                                        htmlBuilder.Append(GenerateHtmlFromBookmarks(document));
                                    }
                                }

                                break;

                            case "Subtitle":
                            case "Heading2":
                            case "Heading 2":
                            case "Kop 2":
                                htmlBuilder.AppendLine("<h2>" + ProcessParagraph(paragraph) + "</h2>");
                                break;

                            default:
                                htmlBuilder.AppendLine("<p>" + ProcessParagraph(paragraph) + "</p>");
                                break;
                        }


                    }
                    // Als het een tabel is
                    else if (element is Table table)
                    {
                        htmlBuilder.AppendLine("<table class='fluent-data-grid grid'>");
                        var index = 0;
                        foreach (Row row in table.Rows)
                        {
                            if (index == 0)
                            {
                                htmlBuilder.AppendLine("<thead>");
                            }

                            htmlBuilder.AppendLine("<tr class='fluent-data-grid-row'>");
                            foreach (Cell cell in row.Cells)
                            {
                                Paragraph cellPar = null;

                                // Manually iterate through the elements to find the first Paragraph
                                foreach (var documentElement in cell.Elements)
                                {
                                    if (documentElement is Paragraph fod)
                                    {
                                        cellPar = fod;
                                        break; // Exit the loop once the first paragraph is found
                                    }
                                }

                                var htmlTag = "td"; // <td> by default;
                                if (index == 0)
                                {
                                    htmlTag = "th"; // <th> first row;
                                }

                                htmlBuilder.AppendLine($"<{htmlTag}>" + ProcessParagraph(cellPar) + $"</{htmlTag}>");
                            }
                            htmlBuilder.AppendLine("</tr>");

                            if (index == 0)
                            {
                                htmlBuilder.AppendLine("</thead>");
                            }



                            index++;


                        }
                        htmlBuilder.AppendLine("</table>");
                    }
                }
            }

            // Close the container div
            htmlBuilder.Append("</div>");


            // Einde van de HTML
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }


        // This function generates HTML for bookmarks in the document
        private static string GenerateHtmlFromBookmarks(Document document)
        {
            var htmlContent = new StringBuilder();

            // Loop through sections and their elements to find bookmarks
            foreach (var item in document.Sections)
            {
                if (item is Section section)
                {
                    if (section.Tag is not null and (object)"TOC")
                    {
                        foreach (var element in section.Elements)
                        {
                            if (element is Paragraph par)
                            {
                                foreach (var parE in par.Elements)
                                {
                                    if (parE is Hyperlink hyperlink)
                                    {
                                        if (hyperlink.Type == HyperlinkType.Bookmark)
                                        {
                                            string txtValue = "";
                                            foreach (var hyperlinkElement in hyperlink.Elements)
                                            {
                                                if (hyperlinkElement is FormattedText ft)
                                                {
                                                    foreach (var ftElement in ft.Elements)
                                                    {
                                                        if (ftElement is Text text)
                                                        {
                                                            txtValue += text.Content;
                                                        }
                                                    }
                                                }
                                            }
                                            htmlContent.Append($"<a id='{hyperlink.Name}' href='#{hyperlink.Name}'>{txtValue}</a><br/>");

                                        }
                                    }
                                }


                            }




                        }
                    }
                }


            }

            return htmlContent.ToString();
        }


        // Extract the font styles from the document (first section's paragraph, or other elements if needed)
        private static string ExtractFontStyles(Document document, out string fontFamily)
        {
            // Assuming we are extracting font from the first paragraph in the first section
            fontFamily = "Arial, sans-serif"; // Default font
            var fontSize = "12pt"; // Default font size
            var fontWeight = "normal"; // Default font weight
            var fontStyle = "normal"; // Default font style

            // Example: Get font styles from the first paragraph (you can modify this to work with styles)
            if (document.Sections.Count > 0 && document.Sections[0].Elements.Count > 0)
            {
                var firstParagraph = document.Sections[0].Elements[0] as Paragraph;
                if (firstParagraph != null)
                {
                    // Extract font settings from the first paragraph
                    fontFamily = firstParagraph.Format.Font.Name ?? fontFamily;
                    //fontSize = $"{firstParagraph.Format.Font.Size.Point}pt";
                    fontWeight = firstParagraph.Format.Font.Bold ? "bold" : "normal";
                    fontStyle = firstParagraph.Format.Font.Italic ? "italic" : "normal";
                }
            }

            // Return the style string to be applied to the HTML content
            return $"font-family: {fontFamily}; font-size: {fontSize}; font-weight: {fontWeight}; font-style: {fontStyle};";
        }


        // Methode om een paragraaf te verwerken en te converteren naar HTML
        public static string ProcessParagraph(Paragraph? paragraph)
        {
            if (paragraph == null) return "";
            StringBuilder sb = new StringBuilder();

            foreach (var inline in paragraph.Elements)
            {
                if (inline is FormattedText ft)
                {
                    string content = "";
                    foreach (var ftElement in ft.Elements)
                    {
                        if (ftElement is Text ftText)
                        {
                            content += ftText.Content;
                        }
                    }




                    // Verwerk opmaak
                    if (ft.Bold)
                        content = "<b>" + content + "</b>";
                    if (ft.Italic)
                        content = "<i>" + content + "</i>";
                    if (ft.Subscript)
                        content = "<sub>" + content + "</sub>";
                    if (ft.Superscript)
                        content = "<sup>" + content + "</sup>";

                    sb.Append(content);
                }
                else if (inline is Text text)
                {
                    sb.Append(text.Content);
                }
            }

            return sb.ToString();
        }
    }




}



