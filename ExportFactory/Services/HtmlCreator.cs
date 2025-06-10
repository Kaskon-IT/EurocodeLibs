namespace ExportFactory.Services
{
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Fields;
    using MigraDoc.DocumentObjectModel.Tables;
    using System.Text;
    using System.Text.RegularExpressions;

    public class HtmlCreator
    {

        public static string MarkdownToHtml(string? markdown)
        {
            string html = "";

            if (!string.IsNullOrEmpty(markdown))
            {
                // |greek| 
                markdown = MigraDocCreator.ReplaceGreekLetters(markdown);

                // Subscript: ~sub~ -> <sub>sub</sub> 
                markdown = Regex.Replace(markdown, @"~(.*?)~", "<sub>$1</sub>");

                // Superscript: ^super^ -> <sup><text</sup>
                markdown = Regex.Replace(markdown, @"\^(.*?)\^", "<sup>$1</sup>");

                // Bold: **text** -> <strong>text</strong>
                markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

                // Italic: *text* -> <em>text</em>
                markdown = Regex.Replace(markdown, @"\*(.*?)\*", "<em>$1</em>");

                // Underline: __text__ -> <u>text</u>
                markdown = Regex.Replace(markdown, @"__(.*?)__", "<u>$1</u>");

                // Strikethrough: ~~text~~ -> <s>text</s>
                markdown = Regex.Replace(markdown, @"~~(.*?)~~", "<s>$1</s>");

                // Line breaks: dubbele nieuwe regel -> <br/>
                markdown = Regex.Replace(markdown, @"\n\s*\n", "<br/>");


                html = markdown;

            }


            return html;
        }


        // Methode om HTML te genereren van een MigraDoc Document
        public static string GenerateHtmlFromDocument(Document document, bool centered = !true)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            // Extract font style from the first section (this can be modified to extract from styles, paragraphs, etc.)
            var fontStyleString = ExtractFontStyles(document, out string fontFamily);

            // Begin van de HTML
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");

            // Voeg een stijl toe aan de pagina (CSS)
            //htmlBuilder.AppendLine("<style>");
            //htmlBuilder.AppendLine("</style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<script>function scrollToToc() { document.getElementById('toc-embvg01f').scrollIntoView({ behavior: 'smooth' });  }</script>");

            //htmlBuilder.AppendLine("<script>\r\n    // Selecteer alle knoppen met de klasse \"accordion\"\r\n    var acc = document.getElementsByClassName(\"ec-accordion\");\r\n\r\n    // Voeg een klik-gebeurtenis toe aan elke knop\r\n    for (var i = 0; i < acc.length; i++) {\r\n        acc[i].addEventListener(\"click\", function() {\r\n            // Toon of verberg de inhoud\r\n            this.classList.toggle(\"active\");\r\n            var panel = this.nextElementSibling;\r\n            if (panel.style.display === \"block\") {\r\n                panel.style.display = \"none\";\r\n            } else {\r\n                panel.style.display = \"block\";\r\n            }\r\n        });\r\n    }\r\n</script>");

            htmlBuilder.AppendLine("<body>");

            // Apply centered style if the 'centered' parameter is true
            string containerStyle = centered ? "text-align: center; margin: 0 auto; max-width: 680px;" : "text-align: left; max-width: 680px;";

            // Wrap the entire content in a container with the appropriate style
            htmlBuilder.Append($"<div class='preview-container'>");

            // TOC
            //htmlBuilder.Append(GenerateHtmlFromBookmarks(document));

            // Loop door de secties en paragrafen in het document
            bool inhoudsopgaveIsVerwerkt = false;
            bool accordionIsOpen = false;
            foreach (Section section in document.Sections)
            {
                // onderzoek: maak section inklapbaar (accordion)
                // <button class="accordion">Hoofdstuk 1</button>
                // <div class="panel">
                // <p>Dit is de inhoud van hoofdstuk 1. Hier kun je tekst plaatsen die je wilt inklappen.</p>
                //</div>
                accordionIsOpen = false;
                if (section.Tag != null)
                {
                    accordionIsOpen = true;
                    var guid = Guid.NewGuid();
                    htmlBuilder.AppendLine($"<button class='ec-accordion' id='{guid}' onclick='toggleAccordion(\"{guid}\")'>{section.Tag}</button>");
                    htmlBuilder.AppendLine($"<div class='ec-panel' id='pnl{guid}'>");

                }

                foreach (var element in section.Elements)
                {
                    string bookmarkId = "";

                    // Als het een paragraaf is
                    if (element is Paragraph paragraph)
                    {
                        if (paragraph.Elements.LastObject is BookmarkField bookmarkField)
                        {
                            bookmarkId = $"id='{bookmarkField.Name}'";
                        }

                        switch (paragraph.Style)
                        {
                            case "Title":
                                htmlBuilder.AppendLine("<h1 class='document-title'>" + ProcessParagraph(paragraph) + "</h1>");
                                break;
                            case "Heading1":
                            case "Heading 1":
                            case "Kop 1":
                                if (!inhoudsopgaveIsVerwerkt &&
                                    paragraph.Elements.First != null &&
                                    paragraph.Elements.First is Text text &&
                                    text != null &&
                                    text.Content != null &&
                                    text.Content.Equals("inhoudsopgave", StringComparison.CurrentCultureIgnoreCase))
                                {

                                    // TOC hyperlinks

                                    // The button that will scroll to the TOC
                                    htmlBuilder.Append("<div id='toc-embvg01f'>");
                                    htmlBuilder.AppendLine($"<h1 class='kop1'>" + ProcessParagraph(paragraph) + "</h1>");
                                    htmlBuilder.Append("<a class='to-toc-btn' onclick='scrollToToc()'>Naar Inhoudsopgave</a>");
                                    htmlBuilder.Append(GenerateHtmlFromBookmarks(document));
                                    htmlBuilder.Append("</div>");




                                    // vanaf nu niet meer in deze if-statement, geef aan dat toc is verwerkt.
                                    inhoudsopgaveIsVerwerkt = true;
                                }
                                else
                                {
                                    if (accordionIsOpen)
                                    {
                                        // sluit
                                        htmlBuilder.AppendLine("</div>");
                                    }

                                    // start nieuwe
                                    accordionIsOpen = true;
                                    var guid = Guid.NewGuid();
                                    htmlBuilder.AppendLine($"<button class='ec-accordion' id='{guid}' onclick='toggleAccordion(\"{guid}\")'>{ProcessParagraph(paragraph)}</button>");
                                    htmlBuilder.AppendLine($"<div class='ec-panel' id='pnl{guid}'>");
                                    htmlBuilder.AppendLine($"<h1 {bookmarkId} class='kop1'>" + ProcessParagraph(paragraph) + "</h1>");
                                }

                                break;

                            case "Subtitle":
                                htmlBuilder.AppendLine("<h2 class='document-subtitle'>" + ProcessParagraph(paragraph) + "</h2>");
                                break;
                            case "Heading2":
                            case "Heading 2":
                            case "Kop 2":
                                htmlBuilder.AppendLine($"<h2 {bookmarkId} class='kop2'>" + ProcessParagraph(paragraph) + "</h2>");
                                break;

                            case "Kop 3":
                            case "Heading3":
                                htmlBuilder.AppendLine($"<h3>{ProcessParagraph(paragraph)}</h3>");
                                break;

                            case "Kop 4":
                            case "Heading4":
                                htmlBuilder.AppendLine($"<h4>{ProcessParagraph(paragraph)}</h4>");
                                break;


                            case "TableHeading":

                                // maak een button voor inklapbaar
                                //htmlBuilder.AppendLine

                                htmlBuilder.AppendLine($"<h6 class='ec-table-heading'>{ProcessParagraph(paragraph)}</h6>");
                                break;

                            case "Mono":
                                htmlBuilder.AppendLine("<p class='ec-par mono'>" + ProcessParagraph(paragraph) + "</p>");
                                break;

                            default:
                                htmlBuilder.AppendLine("<p class='ec-par'>" + ProcessParagraph(paragraph) + "</p>");
                                break;
                        }


                    }
                    // Als het een tabel is
                    else if (element is Table table)
                    {
                        string? tableTag = "";
                        if (table.Tag != null)
                            tableTag = table.Tag.ToString();

                        bool isPivotTable = tableTag != null && tableTag.Contains("pivot");
                        bool hideHeader = tableTag != null && tableTag.Contains("hideheader");


                        string cssTable = "ec-table";
                        if (isPivotTable)
                            cssTable += "-pivot";



                        // start <table>
                        htmlBuilder.AppendLine($"<table class='{cssTable}'>");
                        if (isPivotTable)
                        {
                            htmlBuilder.AppendLine("<tbody>");
                        }
                        else
                        {
                            htmlBuilder.AppendLine("<thead class='ec-table-head'>");
                        }

                        var rowIndex = 0;

                        // Check if first row is 'empty'
                        if (table.Tag is string tag)
                        {
                            //if (tag.Contains(""))
                        }


                        foreach (Row row in table.Rows)
                        {
                            var cellIndex = 0;


                            if (isPivotTable)
                            {
                                htmlBuilder.AppendLine("<tr class='ec-table-row ec-table-row-pivot'>");
                            }
                            else
                            {
                                htmlBuilder.AppendLine("<tr class='ec-table-row'>");
                            }


                            foreach (Cell cell in row.Cells)
                            {
                                Paragraph? cellPar = null;

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


                                var htmlClass = "ec-td";



                                if (isPivotTable)
                                    htmlClass = "ec-td-pivot";

                                // check for th
                                if ((rowIndex == 0 && !isPivotTable && !hideHeader) ||
                                    (isPivotTable && cellIndex <= 2))
                                {
                                    htmlTag = "th"; // <th> first row (normal, header visible) or first columns (pivot tabel)
                                    htmlClass = "ec-th";
                                    if (isPivotTable)
                                    {
                                        htmlClass = "ec-th-pivot";
                                    }

                                    if (hideHeader)
                                    {
                                        htmlClass += " hidden";
                                    }



                                    if (cellPar != null)
                                    {

                                        if (cellPar.Tag != null)
                                        {
                                            switch (cellPar.Tag)
                                            {
                                                case "symbol": htmlClass += " ec-symbol"; break;
                                                case "description": htmlClass += " ec-description"; break;
                                                case "article": htmlClass += " ec-article"; break;
                                            }
                                        }
                                    }

                                }


                                // alignment uitlezen van column (en niet van cell, deze wordt genegeerd door MigraDoc)
                                var column = table.Columns[cellIndex];


                                if (column != null)
                                {
                                    switch (column.Format.Alignment)
                                    {
                                        case ParagraphAlignment.Left: htmlClass += " text-left"; break;
                                        case ParagraphAlignment.Center: htmlClass += " text-center"; break;
                                        case ParagraphAlignment.Right: htmlClass += " text-right"; break;
                                        case ParagraphAlignment.Justify: htmlClass += " text-justify"; break;
                                    }

                                }

                                // alleen voor de 1e rij, controleer of width is opgegeven
                                if (rowIndex == 1)
                                {
                                    var thisColumn = table.Columns[cellIndex];


                                    if (thisColumn != null && !thisColumn.Width.IsNull)
                                    {
                                        //
                                        var width = thisColumn.Width.Centimeter.ToString("0"); // alleen hele cm ondersteund!
                                        htmlClass += $" ec-width-{width}";
                                    }
                                }

                                if (cellPar != null)
                                    htmlBuilder.AppendLine($"<{htmlTag} class='{htmlClass}'>" + ProcessParagraph(cellPar) + $"</{htmlTag}>");

                                cellIndex++;
                            }
                            htmlBuilder.AppendLine("</tr>");


                            if (rowIndex == 0 && !isPivotTable)
                            {
                                htmlBuilder.AppendLine("</thead>");
                            }

                            rowIndex++;
                        }

                        // apply closing tags  
                        if (isPivotTable)
                        {
                            htmlBuilder.AppendLine("</tbody>");
                        }
                        htmlBuilder.AppendLine("</table>");
                    } // end if table
                }

                // Close the accordion div
                if (accordionIsOpen)
                {
                    htmlBuilder.Append("</div>");
                }


            }

            // Nog 1 div voor blanco deel onder laatste element voor leesbaarheid
            //htmlBuilder.AppendLine("<div style='height:1cm;'></div>");


            // Close the container div
            htmlBuilder.Append("</div>");

            // Nog een div voor blanco ruimte onder laatste element
            //htmlBuilder.AppendLine("<div style='height:5cm;'></div>");


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
            //int index = 0;
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
                                            htmlContent.Append($"<a href='#{hyperlink.Name}'>{txtValue}</a><br/>");

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
            if (paragraph == null)
                return "";

            StringBuilder sb = new StringBuilder();

            // test <svg>
            if (paragraph.Tag != null &&
                paragraph.Tag.ToString().StartsWith("<svg"))
            {
                sb.AppendLine(paragraph.Tag.ToString());
            }
            else
            {
                // aanvulling splits elementen by tabs (indien aanwezig)
                string currentText = string.Empty;


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

                        //sb.Append(content);
                        currentText += content;

                    }
                    if (inline is Text text)
                    {
                        //sb.Append(text.Content);
                        currentText += text.Content;
                    }
                    if (inline is Character character)
                    {
                        if (character.SymbolName == SymbolName.Tab)
                        {
                            // Als het een tab is, voeg de huidige verzamelde tekst toe en voeg een <span> voor de tab toe
                            if (!string.IsNullOrEmpty(currentText))
                            {
                                sb.Append($"<span class=\"tab-simulated\">{currentText}</span>");
                                currentText = string.Empty; // Reset de tekst na een tab
                            }


                        }

                        if (character.SymbolName == SymbolName.LineBreak)
                        {
                            // Als het een regelonderbreking is, voeg de huidige verzamelde tekst toe en voeg een <br> toe
                            currentText += "<br />";
                        }

                    }

                }
                // en de rest
                if (!string.IsNullOrEmpty(currentText))
                    sb.Append(currentText);


            }




            return sb.ToString();
        }
    }




}



