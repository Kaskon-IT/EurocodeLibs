using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using SkiaSharp;
using Svg.Skia;
using System.Text;
using System.Text.RegularExpressions;

namespace ExportFactory.Services
{

    public class SvgImageCache
    {
        private readonly List<string> _tempFiles = new();

        /// <summary>
        /// Voegt een SVG-string toe aan een MigraDoc Paragraph als Image.
        /// </summary>
        /// <param name="paragraph">Doel-paragraph</param>
        /// <param name="svgXml">SVG XML string</param>
        /// <param name="widthCm">Gewenste breedte in cm (hoogte schaalt automatisch mee)</param>
        /// <param name="maxHeightCm">Optioneel: maximale hoogte in cm (default = 0 = onbeperkt)</param>
        /// <param name="center">Centreren in de paragraph</param>
        /// <returns>Het toegevoegde MigraDoc Image</returns>
        public Image AddSvg(Paragraph paragraph, string svgXml, double widthCm, double maxHeightCm = 0, bool center = true)
        {
            // ---- 1. Parse viewBox
            var viewBox = ParseViewBox(svgXml);
            double aspect = viewBox.width / viewBox.height;

            // ---- 2. Rendergrootte in pixels (hoge resolutie)
            int renderWidthPx = 1200;
            int renderHeightPx = (int)(renderWidthPx / aspect);
            double scale = renderHeightPx / viewBox.width;

            // ---- 3. Render SVG naar PNG-bytes
            byte[] pngBytes = RenderSvgToPng(svgXml, renderWidthPx, renderHeightPx, scale);

            // ---- 4. Temp bestand
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(fileName, pngBytes);
            _tempFiles.Add(fileName);

            // ---- 5. MigraDoc Image
            var image = paragraph.AddImage(fileName);
            image.LockAspectRatio = true;
            image.Width = Unit.FromCentimeter(widthCm);

            if (maxHeightCm > 0 && image.Height.Centimeter > maxHeightCm)
            {
                image.Height = Unit.FromCentimeter(maxHeightCm);
            }

            if (center)
            {
                paragraph.Format.Alignment = ParagraphAlignment.Center;
            }

            return image;
        }

        /// <summary>
        /// Ruimt alle tijdelijke bestanden op. Aanroepen NA het wegschrijven van het document!
        /// </summary>
        public void Cleanup()
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file)) File.Delete(file);
                }
                catch
                {
                    // negeren
                }
            }
            _tempFiles.Clear();
        }

        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------

        private (double x, double y, double width, double height) ParseViewBox(string svgXml)
        {
            var m = Regex.Match(svgXml, @"viewBox\s*=\s*""([\d\.\-]+)\s+([\d\.\-]+)\s+([\d\.\-]+)\s+([\d\.\-]+)""");
            if (!m.Success)
                throw new Exception("SVG heeft geen geldige viewBox");

            double x = double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            double y = double.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
            double w = double.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
            double h = double.Parse(m.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture);
            return (x, y, w, h);
        }

        private byte[] RenderSvgToPng(string svgXml, int widthPx, int heightPx, double scale)
        {
            using var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(svgXml));
            var svg = new SKSvg();
            svg.Load(svgStream);

            using var bitmap = new SKBitmap(widthPx, heightPx);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            //float scaleX = widthPx / svg.Picture.CullRect.Width;
            //float scaleY = heightPx / svg.Picture.CullRect.Height;
            canvas.Scale((float)scale);
            canvas.DrawPicture(svg.Picture);
            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }



}
