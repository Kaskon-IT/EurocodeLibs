using SkiaSharp;
//using Svg.Skia;



namespace ExportFactory.Services
{


    public class SvgService
    {

        [Obsolete("Vervangen door Iets anders")]
        public static Stream ConvertSvgToPngStream(string svgContent, out double width, out double height)
        {
            // even bewaren, werkt misschien toch wel. 
            // althans voor svg met alleen positieve coordinaten.
            // voor nu even null teruggeven
            // beter als we geen SkiaSharp en Svg.Skia hoeven te gebruiken
            // volgens mij 
            // zijn we dichtbij een solide oplossing via EMF
            // eigen code zonder DOM elementen te gebruiken


            width = 0;
            height = 0;

            // bewaren totdat eigen code 100% zeker werkt.
            return null;


            try
            {
                // Load the SVG content
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
                //var svg = new SKSvg();
                //var svgPicture = svg.Load(stream);

                //if (svgPicture == null)
                //{
                //    throw new Exception("Invalid SVG content.");
                //}

                // Get the size of the SVG
                //var svgBounds = svgPicture.CullRect;
                //width = svgBounds.Width;
                //height = svgBounds.Height;

                // Render the SVG to a bitmap
                var bitmap = new SKBitmap((int)width, (int)height);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.Transparent);
                //canvas.DrawPicture(svgPicture);
                canvas.Flush();

                // Save the bitmap to a stream
                var outputStream = new MemoryStream();
                bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outputStream);
                outputStream.Position = 0;

                return outputStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing SVG: {ex.Message}");
                return null;
            }
        }


    }
}
