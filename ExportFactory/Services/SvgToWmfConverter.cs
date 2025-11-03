using Svg;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExportFactory.Services
{
    [Obsolete("Gebruik SvgToMetafileConverter.ConvertSvgToMetafile")]
    public static class SvgToWmfConverter
    {

        /// <summary>
        /// Converteert SVG XML naar een EMF- of WMF-bestand op het opgegeven pad.
        /// </summary>
        /// <param name="svgXml">De SVG als XML-string</param>
        /// <param name="outputPath">Volledig pad waar het .emf- of .wmf-bestand opgeslagen moet worden</param>
        public static void ConvertSvgToMetafileBAK(string svgXml, string outputPath)
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


            // 2. Bepaal de gewenste tekenafmetingen
            int width = 1000;
            int height = 1000;


            if (svgDoc.Width.Type == SvgUnitType.Pixel && svgDoc.Width.Value > 0)
                width = (int)Math.Ceiling(svgDoc.Width.Value);


            if (svgDoc.Height.Type == SvgUnitType.Pixel && svgDoc.Height.Value > 0)
                height = (int)Math.Ceiling(svgDoc.Height.Value);


            // 3. Maak tijdelijk GDI+ graphics context aan
            using var tempBitmap = new Bitmap(1, 1);
            using var g = Graphics.FromImage(tempBitmap);
            IntPtr hdc = g.GetHdc();


            try
            {
                // 4. Schrijf naar EMF of WMF afhankelijk van de extensie
                using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                var rect = new Rectangle(0, 0, width, height);


                using var metafile = useEmf
                ? new Metafile(stream, hdc, rect, MetafileFrameUnit.Pixel, EmfType.EmfPlusDual)
                : new Metafile(stream, hdc, rect, MetafileFrameUnit.Pixel);


                using var gMeta = Graphics.FromImage(metafile);


                // 5. Render de SVG op het metafile
                svgDoc.Draw(gMeta, new SizeF(width, height));
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }



        /// <summary>
        /// Converteert SVG XML naar een WMF-bestand op het opgegeven pad.
        /// </summary>
        /// <param name="svgXml">De SVG als XML-string</param>
        /// <param name="outputWmfPath">Volledig pad waar het .wmf-bestand opgeslagen moet worden</param>
        public static void ConvertSvgToWmf(string svgXml, string outputWmfPath)
        {
            if (string.IsNullOrWhiteSpace(svgXml))
                throw new ArgumentException("SVG XML mag niet leeg zijn.", nameof(svgXml));

            if (string.IsNullOrWhiteSpace(outputWmfPath))
                throw new ArgumentException("Output pad mag niet leeg zijn.", nameof(outputWmfPath));

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

            // 2. Bepaal de gewenste tekenafmetingen
            int width = 1000;
            int height = 1000;

            if (svgDoc.Width.Type == SvgUnitType.Pixel && svgDoc.Width.Value > 0)
                width = (int)Math.Ceiling(svgDoc.Width.Value);

            if (svgDoc.Height.Type == SvgUnitType.Pixel && svgDoc.Height.Value > 0)
                height = (int)Math.Ceiling(svgDoc.Height.Value);

            // 3. Maak tijdelijk GDI+ graphics context aan
            using var tempBitmap = new Bitmap(1, 1);
            using var g = Graphics.FromImage(tempBitmap);
            IntPtr hdc = g.GetHdc();

            try
            {
                // 4. Schrijf naar WMF
                using var stream = new FileStream(outputWmfPath, FileMode.Create, FileAccess.Write);
                using var metafile = new Metafile(stream, hdc, new Rectangle(0, 0, width, height), MetafileFrameUnit.Pixel);
                using var gMeta = Graphics.FromImage(metafile);

                // 5. Render de SVG op het metafile
                svgDoc.Draw(gMeta, new SizeF(width, height));
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }
    }
}
