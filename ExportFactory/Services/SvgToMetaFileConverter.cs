using Svg;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Xml;

namespace ExportFactory.Services
{
    public static class SvgToMetaFileConverter
    {
        public static void ConvertSvgToMetafile(string svgXml, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(svgXml))
                throw new ArgumentException("SVG XML mag niet leeg zijn.", nameof(svgXml));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output pad mag niet leeg zijn.", nameof(outputPath));

            string extension = Path.GetExtension(outputPath).ToLowerInvariant();
            bool useEmf = extension == ".emf";

            // 1. Parse SVG XML naar SvgDocument
            SvgDocument svgDoc;
            try
            {
                svgDoc = SvgDocument.FromSvg<SvgDocument>(svgXml);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Kon SVG niet parsen.", ex);
            }

            float dpi = 96f;
            float widthPx = 1000;
            float heightPx = 1000;

            // haal afmetingen op (maar vertrouw ze niet)
            // niet in alle gevallen zijn ze correct
            // onderzoek wanneer svgDoc.Width/Height niet correct doorkomen
            // als bijvoorbeeld width:auto; height:auto; in style staat?
            // dan is het formaat afhankelijk van de container (die om de svg heen zit)
            // dus we moeten een redelijke fallback strategie hebben
            // wat we weten:

            // 1. Probeer uit SvgDocument zelf
            var dim = svgDoc.GetDimensions();
            widthPx = dim.Width;
            heightPx = dim.Height;

            // 2. Parse <svg> element uit XML en dan style..
            string? styleAttr = null;
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(svgXml);
            var svgNode = xmlDoc.DocumentElement;
            if (svgNode != null && svgNode.Name == "svg")
            {
                styleAttr = svgNode.GetAttribute("style");
            }

            // 3. Probeer style="width:..., height:..."
            if (!string.IsNullOrEmpty(styleAttr))
            {
                widthPx = ParseCssLength(styleAttr, "width", dpi, widthPx);
                heightPx = ParseCssLength(styleAttr, "height", dpi, heightPx);
            }
            // 4. Anders, gebruik ViewBox
            else if (svgDoc.ViewBox.Width > 0 && svgDoc.ViewBox.Height > 0)
            {
                widthPx = svgDoc.ViewBox.Width;
                heightPx = svgDoc.ViewBox.Height;
            }

            // 5. fallback default
            widthPx = widthPx > 0 ? widthPx : 1000;
            heightPx = heightPx > 0 ? heightPx : 1000;

            // 5. Maak tijdelijk GDI+ graphics context aan
            using var tempBitmap = new Bitmap(1, 1);
            using var g = Graphics.FromImage(tempBitmap);
            IntPtr hdc = g.GetHdc();

            try
            {
                var rect = new RectangleF(0, 0, widthPx, heightPx);

                using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                using var metafile = useEmf
                    ? new Metafile(stream, hdc, Rectangle.Ceiling(rect), MetafileFrameUnit.Pixel, EmfType.EmfPlusDual)
                    : new Metafile(stream, hdc, Rectangle.Ceiling(rect), MetafileFrameUnit.Pixel);

                using var gMeta = Graphics.FromImage(metafile);

                // Vector-rendering
                svgDoc.Draw(gMeta, new SizeF(widthPx, heightPx));
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        private static float ParseCssLength(string style, string property, float dpi, float fallback)
        {
            var regex = new Regex($@"{property}\s*:\s*(\d+(\.\d+)?)(px|cm|mm)", RegexOptions.IgnoreCase);
            var match = regex.Match(style);
            if (match.Success && float.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
            {
                var unit = match.Groups[3].Value.ToLower();
                return unit switch
                {
                    "px" => value,
                    "cm" => value / 2.54f * dpi,
                    "mm" => value / 25.4f * dpi,
                    _ => fallback
                };
            }
            return fallback;
        }
    }
}
