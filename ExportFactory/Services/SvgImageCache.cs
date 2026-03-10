using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
//using Svg.Skia;
using System.Text.RegularExpressions;

namespace ExportFactory.Services
{



    public class SvgImageCache
    {
        private readonly List<string> _tempFiles = [];
        //private readonly SvgExportInterop _svgInterop;

        //public SvgImageCache(IJSRuntime jsRuntime)
        //{
        //    _svgInterop = new SvgExportInterop(jsRuntime);
        //}


        public Image AddSvgFromBytes(Paragraph paragraph, byte[] pngBytes, double widthCm, double maxHeightCm = 0, bool center = true)
        {
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(fileName, pngBytes);
            _tempFiles.Add(fileName);

            var image = paragraph.AddImage(fileName);
            image.LockAspectRatio = true;
            image.Width = Unit.FromCentimeter(widthCm);

            if (maxHeightCm > 0 && image.Height.Centimeter > maxHeightCm)
                image.Height = Unit.FromCentimeter(maxHeightCm);

            if (center)
                paragraph.Format.Alignment = ParagraphAlignment.Center;

            return image;
        }

        public Image AddPngBytesToSection(Section section, byte[] pngBytes, double widthCm, double maxHeightCm = 0)
        {
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(fileName, pngBytes);
            _tempFiles.Add(fileName);

            var image = section.AddImage(fileName);
            image.LockAspectRatio = true;
            image.Width = Unit.FromCentimeter(widthCm);
            image.Left = $"{18 - widthCm}cm";
            image.Top = "0cm";
            image.WrapFormat.Style = WrapStyle.None;
            image.RelativeVertical = RelativeVertical.Margin;
            image.RelativeHorizontal = RelativeHorizontal.Margin;


            if (maxHeightCm > 0 && image.Height.Centimeter > maxHeightCm)
                image.Height = Unit.FromCentimeter(maxHeightCm);




            return image;
        }





        /// <summary>
        /// Ruimt alle tijdelijke bestanden op.
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
    }





}
