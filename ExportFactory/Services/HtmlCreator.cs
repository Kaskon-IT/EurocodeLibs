namespace ExportFactory.Services
{
    using CommonLibrary;
    using CommonLibrary.Models;
    using ExportFactory.Shared;
    using MigraDoc.DocumentObjectModel;
    using MigraDoc.DocumentObjectModel.Fields;
    using MigraDoc.DocumentObjectModel.Tables;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;

    class TextStyle
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Sub { get; set; }
        public bool Sup { get; set; }
        public bool Underline { get; set; }
        public Color Color { get; set; } = Colors.Black;

        public string Wrap(string input)
        {
            var result = input;

            if (Sup) result = $"<sup>{result}</sup>";
            if (Sub) result = $"<sub>{result}</sub>";
            if (Italic) result = $"<i>{result}</i>";
            if (Bold) result = $"<b>{result}</b>";
            if (Underline) result = $"<u>{result}</u>";

            string htmlColor = $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

            // Voeg alleen <span> toe als het NIET zwart is
            if (htmlColor != "#000000")
            {
                result = $"<span style=\"color:{htmlColor};\">{result}</span>";
            }

            return result;
        }

        public override bool Equals(object? obj)
        {
            return obj is TextStyle other &&
                   Bold == other.Bold &&
                   Italic == other.Italic &&
                   Sub == other.Sub &&
                   Sup == other.Sup &&
                   Underline == other.Underline &&
                   Color == other.Color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Bold, Italic, Sub, Sup, Underline, Color);
        }
    }



    public class HtmlCreator
    {

        public static string MarkdownToHtml(string? markdown)
        {
            string html = "";

            if (!string.IsNullOrEmpty(markdown))
            {
                // |greek| 
                markdown = MigraDocCreator.ReplaceGreekLetters(markdown);

                // Kleuren: {red:tekst} -> <span style="color:red">tekst</span>
                markdown = Regex.Replace(markdown, @"\{(\w+):(.+?)\}", "<span style=\"color:$1\">$2</span>");


                // Underline: __text__ -> <u>text</u>
                markdown = Regex.Replace(markdown, @"__(.*?)__", "<u>$1</u>");

                // Strikethrough: ~~text~~ -> <s>text</s>
                markdown = Regex.Replace(markdown, @"~~(.*?)~~", "<s>$1</s>");

                // Subscript: ~sub~ -> <sub>sub</sub> 
                markdown = Regex.Replace(markdown, @"~(.*?)~", "<sub>$1</sub>");

                // Superscript: ^super^ -> <sup><text</sup>
                markdown = Regex.Replace(markdown, @"\^(.*?)\^", "<sup>$1</sup>");

                // Bold: **text** -> <strong>text</strong>
                markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

                // Italic: *text* -> <em>text</em>
                markdown = Regex.Replace(markdown, @"\*(.*?)\*", "<i>$1</i>");

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
                    //htmlBuilder.AppendLine(@"<div class='page-header'>");

                    //var header = section.Headers.Primary;
                    //string hdr1 = "";
                    //string hdr2 = "";
                    //string hdr3 = "";

                    //if (header?.Elements != null && header?.Elements.Count > 0)
                    //{
                    //    var hdrFod = header?.Elements[0];
                    //    if (hdrFod is Table table)
                    //    {
                    //        //if (table.Rows[0].Cells[0].Elements.First is string str)
                    //        //hdr1 = cell0?.ToString();
                    //    }
                    //}

                    //htmlBuilder.AppendLine(@$"<span class='left'>LINKS{hdr1}</span>");
                    //htmlBuilder.AppendLine(@$"<span class='middle'>MIDDEN</span>");
                    //htmlBuilder.AppendLine(@$"<span class='right'>RECHTS</span>");
                    //htmlBuilder.AppendLine("@</div>");

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
                        bool layoutOnly = tableTag != null && tableTag.Contains("layout-only");

                        List<string> cssClasses = ["ec-table"];
                        string cssTable = "ec-table";
                       
                        if (isPivotTable)
                        {
                            cssTable += " row-head";
                            cssClasses.Add("row-head");
                        }
                        if (!hideHeader)
                        {
                            cssClasses.Add("col-head");
                        }
                        if (layoutOnly)
                        {
                            cssClasses.Add("layout-only"); 
                        }

                        if (table.Borders.Visible == false)
                        {
                            //cssTable += " layout-only"; // alleen 
                        }



                        // start <table>
                        htmlBuilder.AppendLine($"<div class='table-wrap {(layoutOnly? "layout-only": "")}'>");

                        htmlBuilder.AppendLine($"<table class='{string.Join(" ", cssClasses)}'>");

                        htmlBuilder.AppendLine("<colgroup>");
                        foreach (var col in table.Columns)
                        {
                            if (col is MigraDoc.DocumentObjectModel.Tables.Column mdc)
                            {
                                var perc = mdc.Width.Millimeter / 182.0;
                                htmlBuilder.AppendLine($"<col style='width:{(perc*100):0}%'>");
                            }
                        }
                        htmlBuilder.AppendLine("</colgroup>");

                        htmlBuilder.AppendLine("<tbody>");  // NB. geen thead of tfoot (maar met CSS)

                        //if (isPivotTable || hideHeader )
                        //{
                        //    htmlBuilder.AppendLine("<tbody>");  // geen header bij pivot en als verborgen
                        //}
                        //else
                        //{
                        //    //htmlBuilder.AppendLine("<thead class='ec-table-head'>");
                        //    htmlBuilder.AppendLine("<tbody>"); // geen aparte thead meer
                        //}


                        var rowIndex = 0;

                        // Check if first row is 'empty'
                        if (table.Tag is string tag)
                        {
                            //if (tag.Contains(""))
                        }


                        foreach (Row row in table.Rows)
                        {
                            var cellIndex = 0;

                            var sourceObj = row.Tag;
                            if (sourceObj != null) 
                            {
                                if (sourceObj is BaseEurocodeContext ctx)
                                {
                                    var test = ctx; // even debuggen
                                    //if (ctx is )
                                }
                            }


                            if (isPivotTable)
                            {
                                //htmlBuilder.AppendLine("<tr class='ec-table-row ec-table-row-pivot'>");
                                htmlBuilder.AppendLine("<tr>");

                            }
                            else
                            {
                                htmlBuilder.AppendLine("<tr>");
                            }

                            string rowspan = string.Empty;
                            string colspan = string.Empty;
                            string style = string.Empty;

                            foreach (Cell cell in row.Cells)
                            {
                                Paragraph? cellPar = null;

                                // kijk voor spans
                                if (cell != null)
                                {
                                    if (cell.MergeDown > 1)
                                        rowspan = $" rowspan={cell.MergeDown}";
                                    if (cell.MergeRight > 1)
                                        colspan = $" cospan={cell.MergeRight}";

                                }



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

                                var title = "";
                                if (cell.Tag is Formula formula)
                                {
                                    title = $"data-tex='{formula.GetValue()}' data-caption='{formula.Name}'";
                                    htmlClass += " has-formula";
                                }

                                


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
                                        title = $"title='{cellPar}'";

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

                                // alleen voor de 1e rij en de tweede rij, controleer of width is opgegeven
                                if (rowIndex <= 1)
                                {
                                    var thisColumn = table.Columns[cellIndex];


                                    if (thisColumn != null && !thisColumn.Width.IsNull)
                                    {
                                        //
                                        var width = thisColumn.Width.Millimeter.ToString("0mm"); // let op geen spatie
                                        style = $"style = 'min-width: {width};'";
                                    }
                                }

                                string cellText = "?";
                                if (cellPar != null)
                                    cellText = ProcessParagraph(cellPar);

                                // even lelijk, maar werkt
                                var tagComplete = string.Join(" ", htmlTag, $"class='{htmlClass}'", title, colspan, rowspan);

                                //htmlBuilder.AppendLine($"<{htmlTag} {title} {style} class='{htmlClass}' {colspan} {rowspan}>" + $"{cellText}" + $"</{htmlTag}>");
                                htmlBuilder.AppendLine($"<{tagComplete}>" + $"{cellText}" + $"</{htmlTag}>");




                                cellIndex++;
                            }
                            htmlBuilder.AppendLine("</tr>");


                            if (rowIndex == 0 && !isPivotTable && !hideHeader)
                            {
                                //htmlBuilder.AppendLine("</thead>"); // niet bij pivottable of verbogen headers
                                //htmlBuilder.AppendLine("</thead>"); // niet bij pivottable of verbogen headers

                            }

                            rowIndex++;
                        }

                        // apply closing tags  
                        if (isPivotTable)
                        {
                            htmlBuilder.AppendLine("</tbody>");
                        }
                        htmlBuilder.AppendLine("</table>"); 
                        htmlBuilder.AppendLine("</div>"); // close div wrapper

                    } // end if table
                }

                // Close the accordion div
                if (accordionIsOpen)
                {
                    htmlBuilder.Append("</div>");
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


        public static string ProcessParagraph(Paragraph? paragraph)
        {
            if (paragraph == null)
                return "";

            var sb = new StringBuilder();

            if (paragraph.Tag is string tagString && tagString.StartsWith("<svg"))
            {

                //sb.AppendLine("<div class=\"svg-container\">");
                sb.AppendLine(tagString);
                //sb.AppendLine("</div>");
                return sb.ToString();
            }

            var currentText = new StringBuilder();
            var currentStyle = new TextStyle();

            void FlushCurrentText()
            {
                if (currentText.Length == 0) return;
                sb.Append(currentStyle.Wrap(currentText.ToString()));
                currentText.Clear();
            }

            foreach (var inline in paragraph.Elements)
            {
                switch (inline)
                {
                    case Character ch when ch.SymbolName == SymbolName.Tab:
                        FlushCurrentText();
                        sb.Append("<span class=\"tab-simulated\"></span>");
                        break;

                    case Character ch when ch.SymbolName == SymbolName.LineBreak:
                        currentText.Append("<br />");
                        break;

                    case Text t:
                        currentText.Append(t.Content);
                        break;

                    case FormattedText ft:
                        foreach (var el in ft.Elements)
                        {
                            if (el is Text ftText)
                            {
                                var newStyle = new TextStyle
                                {
                                    Bold = ft.Bold,
                                    Italic = ft.Italic,
                                    Sub = ft.Subscript,
                                    Sup = ft.Superscript,
                                    Underline = ft.Underline != Underline.None,
                                    Color = ft.Color
                                };

                                if (!currentStyle.Equals(newStyle))
                                {
                                    FlushCurrentText();
                                    currentStyle = newStyle;
                                }

                                currentText.Append(ftText.Content);
                            }
                        }
                        break;
                }
            }

            FlushCurrentText();

            return sb.ToString();
        }



        public static string ProcessParagraphVERSIE2(Paragraph? paragraph)
        {
            if (paragraph == null)
                return "";

            StringBuilder sb = new StringBuilder();

            if (paragraph.Tag != null &&
                paragraph.Tag.ToString().StartsWith("<svg"))
            {
                sb.AppendLine(paragraph.Tag.ToString());
            }
            else
            {
                string currentText = string.Empty;

                foreach (var inline in paragraph.Elements)
                {
                    switch (inline)
                    {
                        case FormattedText ft:
                            var contentBuilder = new StringBuilder();
                            foreach (var ftElement in ft.Elements)
                            {
                                if (ftElement is Text ftText)
                                {
                                    contentBuilder.Append(ftText.Content);
                                }
                            }

                            string content = contentBuilder.ToString();

                            if (string.IsNullOrEmpty(content))
                                break;

                            if (ft.Bold)
                                content = $"<b>{content}</b>";
                            if (ft.Italic)
                                content = $"<i>{content}</i>";
                            if (ft.Subscript)
                                content = $"<sub>{content}</sub>";
                            if (ft.Superscript)
                                content = $"<sup>{content}</sup>";
                            if (ft.Color != Colors.Black)
                                content = $"<span style='color:{ft.Color.ToString().ToLowerInvariant()};'>{content}</span>";

                            currentText += content;
                            break;

                        case Text text:
                            currentText += text.Content;
                            break;

                        case Character character:
                            if (character.SymbolName == SymbolName.Tab)
                            {
                                if (!string.IsNullOrEmpty(currentText))
                                {
                                    sb.Append($"<span class=\"tab-simulated\">{currentText}</span>");
                                    currentText = string.Empty;
                                }
                            }
                            else if (character.SymbolName == SymbolName.LineBreak)
                            {
                                currentText += "<br />";
                            }
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(currentText))
                {
                    sb.Append(currentText);
                }
            }

            return sb.ToString();
        }



        // Methode om een paragraaf te verwerken en te converteren naar HTML
        public static string ProcessParagraphDELETE(Paragraph? paragraph)
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

                        if (ft.Color != Colors.Black)
                        {
                            content = "<span style='color:red;'>" + content + "</span>";
                        }



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



