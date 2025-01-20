using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

//using MigraDoc.DocumentObjectModel.Tables;

using PdfSharp.Drawing;

namespace ExportFactory.Services
{
    public class PdfSharpHelper
    {
        private static void AddImageFromStreamToPdf(dynamic target, Stream imageStream, double widthCm, double heightCm)
        {
            ArgumentNullException.ThrowIfNull(imageStream);

            // Convert cm to points (1 cm = 28.35 points)
            double widthPoints = widthCm * 28.35;
            double heightPoints = heightCm * 28.35;

            // Use PDFSharp to handle the image rendering
            using var image = XImage.FromStream(imageStream);

            // Check if the target is a paragraph or table cell
            if (target is Paragraph paragraph)
            {
                var documentRenderer = new MigraDoc.Rendering.DocumentRenderer(paragraph.Document);
                var gfx = XGraphics.CreateMeasureContext(new XSize(widthPoints, heightPoints), XGraphicsUnit.Point, XPageDirection.Downwards);

                gfx.DrawImage(image, 0, 0, widthPoints, heightPoints);
            }
            else if (target is Cell cell)
            {
                // Render directly to the table cell
                var documentRenderer = new MigraDoc.Rendering.DocumentRenderer(cell.Document);
                var gfx = XGraphics.CreateMeasureContext(new XSize(widthPoints, heightPoints), XGraphicsUnit.Point, XPageDirection.Downwards);

                gfx.DrawImage(image, 0, 0, widthPoints, heightPoints);
            }
        }


    }





}
