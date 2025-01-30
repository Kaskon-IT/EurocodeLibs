using CommonLibrary;
using MigraDoc.DocumentObjectModel;

namespace ExportFactory.MigraDocContentModels
{



    public class ExampleDocumentContent
    {
        const string _svgDemo = @"<svg class=""main-svg"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""2558"" height=""807"" style="""" viewBox=""0 0 2558 807""><rect x=""0"" y=""0"" width=""2558"" height=""807"" style=""fill: rgb(255, 255, 255); fill-opacity: 1;""/><defs id=""defs-8addc0""><g class=""clips""><clipPath id=""clip8addc0xyplot"" class=""plotclip""><rect width=""2398"" height=""627""/></clipPath><clipPath class=""axesclip"" id=""clip8addc0x""><rect x=""80"" y=""0"" width=""2398"" height=""807""/></clipPath><clipPath class=""axesclip"" id=""clip8addc0y""><rect x=""0"" y=""100"" width=""2558"" height=""627""/></clipPath><clipPath class=""axesclip"" id=""clip8addc0xy""><rect x=""80"" y=""100"" width=""2398"" height=""627""/></clipPath></g><g class=""gradients""/></defs><g class=""bglayer""/><g class=""layer-below""><g class=""imagelayer""/><g class=""shapelayer""/></g><g class=""cartesianlayer""><g class=""subplot xy""><g class=""layer-subplot""><g class=""shapelayer""/><g class=""imagelayer""/></g><g class=""gridlayer""><g class=""x""><path class=""xgrid crisp"" transform=""translate(472.49,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(741.33,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(1010.16,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(1279,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(1547.84,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(1816.68,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(2085.51,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""xgrid crisp"" transform=""translate(2354.35,0)"" d=""M0,100v627"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/></g><g class=""y""><path class=""ygrid crisp"" transform=""translate(0,691.9)"" d=""M80,0h2398"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""ygrid crisp"" transform=""translate(0,552.7)"" d=""M80,0h2398"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""ygrid crisp"" transform=""translate(0,413.5)"" d=""M80,0h2398"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""ygrid crisp"" transform=""translate(0,274.3)"" d=""M80,0h2398"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/><path class=""ygrid crisp"" transform=""translate(0,135.1)"" d=""M80,0h2398"" style=""stroke: rgb(238, 238, 238); stroke-opacity: 1; stroke-width: 1px;""/></g></g><g class=""zerolinelayer""><path class=""xzl zl crisp"" transform=""translate(203.65,0)"" d=""M0,100v627"" style=""stroke: rgb(68, 68, 68); stroke-opacity: 1; stroke-width: 1px;""/></g><path class=""xlines-below""/><path class=""ylines-below""/><g class=""overlines-below""/><g class=""xaxislayer-below""/><g class=""yaxislayer-below""/><g class=""overaxes-below""/><g class=""plot"" transform=""translate(80,100)"" clip-path=""url('#clip8addc0xyplot')""><g class=""scatterlayer mlayer""><g class=""trace scatter trace27eea3"" style=""stroke-miterlimit: 2; opacity: 1;""><g class=""fills""/><g class=""errorbars""/><g class=""lines""><path class=""js-line"" d=""M123.65,591.9Q482.1,313.5 661.33,313.5C840.55,313.5 1019.78,591.9 1199,591.9C1378.23,591.9 1557.45,406.3 1736.68,313.5Q1915.9,220.7 2274.35,35.1"" style=""vector-effect: non-scaling-stroke; fill: none; stroke: rgb(31, 119, 180); stroke-opacity: 1; stroke-width: 2px; opacity: 1;""/></g><g class=""points""><path class=""point"" transform=""translate(123.65,591.9)"" d=""M3,0A3,3 0 1,1 0,-3A3,3 0 0,1 3,0Z"" style=""opacity: 1; stroke-width: 0px; fill: rgb(31, 119, 180); fill-opacity: 1;""/><path class=""point"" transform=""translate(661.33,313.5)"" d=""M3,0A3,3 0 1,1 0,-3A3,3 0 0,1 3,0Z"" style=""opacity: 1; stroke-width: 0px; fill: rgb(31, 119, 180); fill-opacity: 1;""/><path class=""point"" transform=""translate(1199,591.9)"" d=""M3,0A3,3 0 1,1 0,-3A3,3 0 0,1 3,0Z"" style=""opacity: 1; stroke-width: 0px; fill: rgb(31, 119, 180); fill-opacity: 1;""/><path class=""point"" transform=""translate(1736.68,313.5)"" d=""M3,0A3,3 0 1,1 0,-3A3,3 0 0,1 3,0Z"" style=""opacity: 1; stroke-width: 0px; fill: rgb(31, 119, 180); fill-opacity: 1;""/><path class=""point"" transform=""translate(2274.35,35.1)"" d=""M3,0A3,3 0 1,1 0,-3A3,3 0 0,1 3,0Z"" style=""opacity: 1; stroke-width: 0px; fill: rgb(31, 119, 180); fill-opacity: 1;""/></g><g class=""text""/></g></g></g><g class=""overplot""/><path class=""xlines-above crisp"" d=""M0,0"" style=""fill: none;""/><path class=""ylines-above crisp"" d=""M0,0"" style=""fill: none;""/><g class=""overlines-above""/><g class=""xaxislayer-above""><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(203.65,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">0</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(472.49,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">0.5</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(741.33,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">1</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(1010.16,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">1.5</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(1279,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">2</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(1547.84,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">2.5</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(1816.68,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">3</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(2085.51,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">3.5</text></g><g class=""xtick""><text text-anchor=""middle"" x=""0"" y=""740"" transform=""translate(2354.35,0)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">4</text></g></g><g class=""yaxislayer-above""><g class=""ytick""><text text-anchor=""end"" x=""79"" y=""4.199999999999999"" transform=""translate(0,691.9)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">1</text></g><g class=""ytick""><text text-anchor=""end"" x=""79"" y=""4.199999999999999"" transform=""translate(0,552.7)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">1.5</text></g><g class=""ytick""><text text-anchor=""end"" x=""79"" y=""4.199999999999999"" transform=""translate(0,413.5)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">2</text></g><g class=""ytick""><text text-anchor=""end"" x=""79"" y=""4.199999999999999"" transform=""translate(0,274.3)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">2.5</text></g><g class=""ytick""><text text-anchor=""end"" x=""79"" y=""4.199999999999999"" transform=""translate(0,135.1)"" style=""font-family: 'Open Sans', verdana, arial, sans-serif; font-size: 12px; fill: rgb(68, 68, 68); fill-opacity: 1; white-space: pre;"">3</text></g></g><g class=""overaxes-above""/></g></g><g class=""polarlayer""/><g class=""ternarylayer""/><g class=""geolayer""/><g class=""funnelarealayer""/><g class=""pielayer""/><g class=""treemaplayer""/><g class=""sunburstlayer""/><g class=""glimages""/><defs id=""topdefs-8addc0""><g class=""clips""/></defs><g class=""layer-above""><g class=""imagelayer""/><g class=""shapelayer""/></g><g class=""infolayer""><g class=""g-gtitle""/><g class=""g-xtitle""/><g class=""g-ytitle""/></g></svg>";
        const string _par1 = "Dit is een paragraaf. **Kop1** begint altijd op een nieuwe pagina en komt in de inhoudsopgave. Er wordt geen lege ruimte tussen twee paragrafen gelaten. \r\n" +
            "Een paragraaf heeft standaard een kleine regelafstand. \r\n" +
            "In de tekst wordt *Markdown* (MD) ondersteund voor: \r\n" +
            "- E~subscript~ \r\n" +
            "- E^superscript^\r\n" +
            "- *italic*\r\n" +
            "- griekse letters zoals |alpha|, |beta|, |gamma|, |delta|, etcetera. \r\n" +
            "- combinaties van hierboven, bijvoorbeeld *|xi|*~Nederland~ \r\n";

        const string _par2 = "Hoofdstukken met **Kop2** komen ook in de inhoudsopgave. Er wordt automatisch lege ruimte boven en onder de koptekst gelaten.";
        const string _par3 = "Tabellen krijgen mogelijk een *Titel*. Kolommen kunnen worden worden uitgelijnd: \r\n" +
            "- links\r\n" +
            "- midden\r\n" +
            "- of rechts.\r\n" +
            "De eerste en/of laatste rij mag een *koptekst* bevatten.\r\n" +
            "De eerste en/of laatste kolom mag een *koptekst* bevatten.\r\n" +
            "Keuze voor automatische kolombreedte zijn:\r\n" +
            "- None\r\n" +
            "- ColumnHeaders\r\n" +
            "- AllCellsExceptHeaders\r\n" +
            "- AllCells\r\n" +
            "- Fill (UNDER CONSTRUCTION)\r\n" +
            "NB. Tabellen kunnen niet op meerdere pagina's staan.\r\n" +
            "NB. Ondersteuning voor automatisch opsplitsen van brede tabellen volgt.\r\n";

        const string _loremIpsumMarkdown = "**Lorem Ipsum** is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        const string _loremIpsumPlainText = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        const string _svgExample1 = @"<svg xmlns='http://www.w3.org/2000/svg' width='200' height='100'> <rect width='200' height='100' style='fill:blue;stroke-width:3;stroke:rgb(0,0,0)' /> </svg>";




        public class DemoDataClass : BaseDataClass
        {
            [CustomColumn(headerName: "*M*~Ed~", alignment: ParagraphAlignment.Right, format: "0.00 kNm")]
            public double Med { get; set; } = 48.3;

            [CustomColumn(headerName: "*M*~freq~", alignment: ParagraphAlignment.Right, format: "0.00 kNm")]
            public double Mfreq { get; set; } = 30.98123554245;

        }

        public class BaseDataClass
        {
            [CustomColumn(headerName: "Name", alignment: ParagraphAlignment.Left)]
            public string Name { get; set; } = "";


        }



        public static DocumentContent GetExampleDocumentContent(string fontFamily, int fontSize, DocumentContent.PageMarginAndPageNumberSettingsEnum? pageMarginSetting)
        {
            Font font = new Font(fontFamily, Unit.FromPoint(fontSize));
            font.Color = Colors.Black;

            string title = "Sample Project";
            string subtitle = "Toont de mogelijkheden van de export library";
            string docNaam = "ber-01";
            string onderdeel = "Trappen en bordessen";

            return new DocumentContent
            {
                HeaderLineColor = Colors.LightGray,
                HeaderColor = Colors.Gray,
                FooterLineColor = Colors.LightGray,
                FooterColor = Colors.Gray,


                PageMarginSetting = pageMarginSetting,
                Font = font,
                PageHeader = new PageHeaderContent()
                {

                    Text1 = title,
                    Text2 = docNaam,
                    Text3 = onderdeel,//SvgLogo = _svgKaskon,
                },
                PageFooter = new PageFooterContent()
                {
                    Text1 = title,
                    Text2 = docNaam,
                },

                CoverPage = new CoverPageContent()
                {
                    CompanyName = "Kaskon",
                    ProjectNumber = "1234",
                    Title = title,
                    Subtitle = subtitle,

                },

                TableOfContents = new TableOfContentsContent()
                {
                    Title = "Inhoudsopgave",

                },

                Sections =
                [
                    new SectionContent
                    {
                        Elements = new List<SectionElement>
                        {
                            new HeadingContent {
                                Style = "Kop 1",
                                Text = "Hoofdstuk 1",
                                AddToTOC = true,
                                Level = 1,
                                Order = 0,
                            },

                            new ParagraphContent{
                                Markdown = _par1,
                                Style = "",
                            },

                             new HeadingContent {
                                Style = "Kop 2",
                                Text = "Hoofdstuk 1.1",
                                AddToTOC = true,
                                Level = 2,
                                Order = 0,
                            },

                            new ParagraphContent{
                                Markdown = _par2,
                                Style = "",
                            },
                             new HeadingContent {
                                Style = "Kop 3",
                                Text = "Hoofdstuk 1.1.1",
                                AddToTOC = !true,
                                Level = 3,
                                Order = 0,
                            },

                            new ParagraphContent{
                                Markdown = _par3,
                                Style = "",
                            },


                            new TableModel<DemoDataClass>{
                                Data =
                                [
                                    new DemoDataClass(){ Med = 240, Mfreq = 200},
                                    new DemoDataClass(){ },
                                    new DemoDataClass(){ },
                                ],

                            },

                            new TableContent
                            {
                                Title = "My table title",
                                Alignment = TableAlignment.Left,
                                Order = 0,
                                //ColumnWidths = [5, 10, 10, 10, 10, 10],
                                Headers =
                                [
                                    new TableCellHeaderContent()
                                    {
                                        Width = Unit.FromCentimeter(4),
                                        CellContent = new TableCellContent()
                                        {
                                            Markdown = "Markdown pattern",
                                            Style = new ColumnStyleSettings()
                                            {
                                                AutoSize = AutoColumnSizeOption.ColumnHeader,
                                                Alignment = ParagraphAlignment.Right,
                                            },
                                        }
                                    },
                                    new TableCellHeaderContent()
                                    {
                                        Width = Unit.FromCentimeter(10),
                                        CellContent = new TableCellContent() { Markdown = "Translates to" },
                                    },
                                    new TableCellHeaderContent()
                                    {
                                        Width = Unit.FromCentimeter(10),
                                        CellContent = new TableCellContent() { Markdown = "Demo" },
                                    },
                                    new TableCellHeaderContent()
                                    {
                                        Width = Unit.FromCentimeter(10),
                                        CellContent = new TableCellContent() { Markdown = "Demo" },
                                    },


                                ],
                                Rows =
                                [

                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "`**Bold**`" },
                                        new TableCellContent { Markdown = "**Bold**" }
                                    ],
                                     [
                                        new TableCellContent { Markdown = "`*Italic*`" },
                                        new TableCellContent { Markdown = "*Italic*" }
                                    ],
                                     [
                                        new TableCellContent { Markdown = "```|xi|~nl~```" },
                                        new TableCellContent { Markdown = "|xi|~nl~" }
                                    ],

                                     [
                                        new TableCellContent { Markdown = "```*|xi|*~nl~```" },
                                        new TableCellContent { Markdown = "*|xi|*~nl~" }
                                    ],

                                    [
                                        new TableCellContent { Markdown = "`H~2~O`" },
                                        new TableCellContent { Markdown = "H~2~O" }
                                    ],
                                    [
                                        new TableCellContent { Markdown = "```x^3.14^```" },
                                        new TableCellContent { Markdown = "x^3.14^" }
                                    ],
                                    [   new TableCellContent("```~~rode tekst~~````"),
                                        new TableCellContent("~~rode tekst~~")
                                    ],
                                    [   new TableCellContent("```M~subscript~```"),
                                        new TableCellContent("M~subscript~"),
                                    ],
                                    [   new TableCellContent("``|alpha|```"),
                                        new TableCellContent("|alpha|"),
                                    ],
                                     [   new TableCellContent("```*|beta|*~sub~```"),
                                        new TableCellContent("*|beta|*~sub~"),
                                    ],


                                      [   new TableCellContent("`<svg></svg>`"),
                                        new TableCellContent(){SvgImage = _svgDemo},
                                    ],

                                ]
                            },

                            new HeadingContent{
                                Style = "Kop 1",
                                Text = "Hoofdstuk 2"
                            },

                            new ParagraphContent{
                                Markdown = "bla bla bla"
                            },

                            new HeadingContent{
                                Style = "Kop 1",
                                Text = "Hoofdstuk 3"
                            },

                            new ParagraphContent{
                                Markdown = "bla bla bla"
                            },

                            new HeadingContent{
                                Style = "Kop 1",
                                Text = "Hoofdstuk 4"
                            },

                            new ParagraphContent{
                                Markdown = "bla bla bla"
                            },


                        }
                    }
                ]
            };

        }

    }




}
