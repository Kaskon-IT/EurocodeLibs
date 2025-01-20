using MigraDoc.DocumentObjectModel;
using SkiaSharp;
using Svg.Skia;
using System.Xml;

public static class DocumentHelper
{
    public static void AddImageFromStream(dynamic target, Stream imageStream, double widthCm, double heightCm)
    {
        if (target == null || imageStream == null)
            throw new ArgumentNullException("Target or image stream cannot be null.");

        // Convert the stream into a byte array
        using var memoryStream = new MemoryStream();
        imageStream.CopyTo(memoryStream);
        var imageData = memoryStream.ToArray();

        // Use a MigraDoc image source to handle the byte array

        var image = target.AddImage(memoryStream); // Dynamic allows adding to a cell or paragraph
        image.Width = Unit.FromCentimeter(widthCm);
        image.Height = Unit.FromCentimeter(heightCm);
    }



    public static void AddTableWithSvg(Document document, string svgContent)
    {
        // Create a section
        var section = document.AddSection();

        // Create a table
        var table = section.AddTable();
        table.Borders.Width = 0.75;

        // Define columns
        var column1 = table.AddColumn(Unit.FromCentimeter(6));
        column1.Format.Alignment = ParagraphAlignment.Center;

        var column2 = table.AddColumn(Unit.FromCentimeter(6));
        column2.Format.Alignment = ParagraphAlignment.Center;

        // Add a row
        var row = table.AddRow();

        // Cell with text
        var cell1 = row.Cells[0];
        cell1.AddParagraph("This is a table cell with text.");

        // Cell with SVG image
        var cell2 = row.Cells[1];
        try
        {
            var svgStream = ConvertSvgToImageStream(svgContent, out double width, out double height);
            if (svgStream != null)
            {

                try
                {
                    AddImageFromStream(cell2, svgStream, width / 100, height / 100);
                }
                catch (Exception ex)
                {
                    cell2.AddParagraph($"Error rendering image: {ex.Message}");
                }
                //AddImageFromStream(cell2, svgStream, width / 100, height / 100); // Convert pixels to cm
            }
            else
            {
                cell2.AddParagraph("Invalid SVG Content.");
            }
        }
        catch (Exception ex)
        {
            cell2.AddParagraph($"Error rendering SVG: {ex.Message}");
        }
    }


    public static Stream ConvertSvgToImageStream(string svgContent, out double width, out double height)
    {
        width = 0;
        height = 0;

        try
        {
            // Parse the SVG to extract width and height
            var svgDoc = new XmlDocument();
            svgDoc.LoadXml(svgContent);
            var svgNode = svgDoc.DocumentElement;

            if (svgNode == null || svgNode.Name != "svg")
                throw new Exception("Invalid SVG: Root element is not <svg>.");

            // Extract dimensions
            width = ExtractSvgDimension(svgNode, "width");
            height = ExtractSvgDimension(svgNode, "height");

            if (width <= 0 || height <= 0)
                throw new Exception("SVG width and height must be greater than 0.");

            // Render SVG to a bitmap
            using var svg = new SKSvg();
            try
            {
                svg.FromSvg(svgContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SVG content: {ex.Message}");
                // Handle the error (e.g., log, provide a fallback, or rethrow)
            }


            using var bitmap = new SKBitmap((int)width, (int)height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture);
            canvas.Flush();

            // Save the bitmap to a memory stream as PNG
            var memoryStream = new MemoryStream();
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(memoryStream);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting SVG to image: {ex.Message}");
            width = 0;
            height = 0;
            return null;
        }
    }


    public static Stream ConvertSvgToImageStreamBAK1(string svgContent, out double width, out double height)
    {
        width = 0;
        height = 0;

        try
        {
            // Parse the SVG for size
            var svgDoc = new XmlDocument();
            svgDoc.LoadXml(svgContent);
            var svgNode = svgDoc.DocumentElement;

            if (svgNode == null || svgNode.Name != "svg")
                throw new Exception("Invalid SVG: Root element is not <svg>.");

            // Extract width and height from the SVG
            width = ExtractSvgDimension(svgNode, "width");
            height = ExtractSvgDimension(svgNode, "height");

            if (width <= 0 || height <= 0)
                throw new Exception("SVG width and height must be greater than 0.");

            // Render SVG to a bitmap
            using var svg = new SKSvg();
            svg.Load(svgContent);

            using var bitmap = new SKBitmap((int)width, (int)height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture);
            canvas.Flush();

            // Save the bitmap to a memory stream as PNG
            var memoryStream = new MemoryStream();
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(memoryStream);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch
        {
            width = 0;
            height = 0;
            return null;
        }
    }

    private static double ExtractSvgDimension(XmlNode svgNode, string attribute)
    {
        if (svgNode.Attributes?[attribute] != null &&
            double.TryParse(svgNode.Attributes[attribute]?.Value.Replace("px", ""), out double dimension))
        {
            return dimension;
        }

        return 0; // Default to 0 if no valid dimension is found
    }
}

