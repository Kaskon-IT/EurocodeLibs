using SkiaSharp;
using Svg.Skia;
using System.Text;







public static class SvgTest
{




    public const string demoSvgXml = "<svg xmlns=\"http://www.w3.org/2000/svg\" preserveAspectRatio=\"xMidYMid meet\" viewBox=\"-475.17705735660854 -3385.1770573566087 4370.354114713217 3470.2124688279305\" style=\"width:auto; height:auto;border:1px solid #ddd; border-radius:4px; background:#fafafa; padding:4px; box-sizing:border-box;\"> <marker id=\"circle-cross-marker\" viewBox=\"0 0 16 16\" markerWidth=\"66.681\" markerHeight=\"66.681\" refX=\"8\" refY=\"8\" markerUnits=\"userSpaceOnUse\" orient=\"auto\"> <circle cx=\"8\" cy=\"8\" r=\"3\" stroke=\"black\" fill=\"black\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"2\" y1=\"8\" x2=\"14\" y2=\"8\" stroke=\"black\" stroke-width=\"1\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"8\" y1=\"2\" x2=\"8\" y2=\"14\" stroke=\"black\" stroke-width=\"1\" vector-effect=\"non-scaling-stroke\"/> </marker> <path d=\"M 0 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l -50 -135 l 0 -50 l 270 0 l 0 100 l -100 0 L 3420 -2719.121 L 186.451 0 Z\" stroke=\"black\" stroke-width=\"2\" fill=\"lightgray\" opacity=\"1\" stroke-dasharray=\"\" stroke-linejoin=\"miter\" stroke-linecap=\"butt\" fill-rule=\"nonzero\" vector-effect=\"non-scaling-stroke\" /> <line x1=\"-350.062\" y1=\"0\" x2=\"-350.062\" y2=\"-2960\" stroke=\"black\" stroke-width=\"1\" marker-start=\"url(#circle-cross-marker)\" marker-end=\"url(#circle-cross-marker)\" vector-effect=\"non-scaling-stroke\" /> <text x=\"-350.062\" y=\"-1491.113\" text-anchor=\"middle\" font-size=\"66.681\" font-family=\"Arial\" transform=\"rotate(-90,-350.062,-1480)\"> 16×185 = 2960 </text> <line x1=\"0\" y1=\"0\" x2=\"-350.062\" y2=\"0\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"3520\" y1=\"-2960\" x2=\"-350.062\" y2=\"-2960\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"0\" y1=\"-3260.062\" x2=\"3520\" y2=\"-3260.062\" stroke=\"black\" stroke-width=\"1\" marker-start=\"url(#circle-cross-marker)\" marker-end=\"url(#circle-cross-marker)\" vector-effect=\"non-scaling-stroke\" /> <text x=\"1760\" y=\"-3271.176\" text-anchor=\"middle\" font-size=\"66.681\" font-family=\"Arial\" transform=\"rotate(0,1760,-3260.062)\"> 3520 </text> <line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"-3260.062\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"3520\" y1=\"-2960\" x2=\"3520\" y2=\"-3260.062\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"-193.12\" y1=\"-229.656\" x2=\"3326.88\" y2=\"-3189.656\" stroke=\"black\" stroke-width=\"1\" marker-start=\"url(#circle-cross-marker)\" marker-end=\"url(#circle-cross-marker)\" vector-effect=\"non-scaling-stroke\" /> <text x=\"1566.88\" y=\"-1720.77\" text-anchor=\"middle\" font-size=\"66.681\" font-family=\"Arial\" transform=\"rotate(-40.061,1566.88,-1709.656)\"> 4599 </text> <line x1=\"0\" y1=\"0\" x2=\"-193.12\" y2=\"-229.656\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"3520\" y1=\"-2960\" x2=\"3326.88\" y2=\"-3189.656\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"449.656\" y1=\"-378.12\" x2=\"526.888\" y2=\"-286.277\" stroke=\"black\" stroke-width=\"1\" marker-start=\"url(#circle-cross-marker)\" marker-end=\"url(#circle-cross-marker)\" vector-effect=\"non-scaling-stroke\" /> <text x=\"488.272\" y=\"-343.312\" text-anchor=\"middle\" font-size=\"66.681\" font-family=\"Arial\" transform=\"rotate(49.939,488.272,-332.198)\"> 120 </text> <line x1=\"220\" y1=\"-185\" x2=\"449.656\" y2=\"-378.12\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <line x1=\"297.232\" y1=\"-93.157\" x2=\"526.888\" y2=\"-286.277\" stroke=\"black\" stroke-width=\"0.5\" stroke-dasharray=\"2,2\" vector-effect=\"non-scaling-stroke\"/> <text x=\"1870\" y=\"-1572.5\" text-anchor=\"middle\" font-size=\"66.681\" font-family=\"Arial\" fill=\"black\" transform=\"rotate(-40.061,1870,-1572.5)\"> <tspan x=\"1870\" dy=\"0\">wapening </tspan> <tspan x=\"1870\" dy=\"80.017\">12-150 (754 mm²)</tspan> </text> </svg>";




    private static SKRect ParseViewBox(string svgXml)
    {
        // zoek viewBox attribuut
        var match = System.Text.RegularExpressions.Regex.Match(
            svgXml, @"viewBox\s*=\s*""([^""]+)""");

        if (!match.Success)
            throw new InvalidOperationException("Geen viewBox gevonden in SVG");

        var parts = match.Groups[1].Value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                                        .ToArray();
        if (parts.Length != 4)
            throw new InvalidOperationException("Ongeldige viewBox");

        return new SKRect(parts[0], parts[1], parts[0] + parts[2], parts[1] + parts[3]);
    }

    public static void SaveSvgStaticTest(string svgXml)
    {
        // laad SVG vanuit string
        var svg = new SKSvg();
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(svgXml));
        svg.Load(ms);

        if (svg.Picture == null)
        {
            throw new Exception("Geen SVG picture geladen!");
        }

        // vaste canvas
        int width = 1200;
        int height = 1200;
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        // VASTE translate + scale (hardcoded)
        // probeer gewoon het svg ergens in het midden te zetten
        float scale = 0.1f;  // kleine scale zodat het past
        float offsetX = 600; // midden canvas
        float offsetY = 600; // midden canvas

        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        // teken SVG
        canvas.DrawPicture(svg.Picture);
        canvas.Flush();

        // opslaan als PNG
        using var img = SKImage.FromBitmap(bitmap);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.OpenWrite(@"C:\Temp\test_static.png");
        data.SaveTo(fs);

        Console.WriteLine("Static PNG gemaakt: C:\\Temp\\test_static.png");
    }

    public static void SaveSvgAsPng(string svgXml, string outputPath, int width, int height)
    {
        var svg = new SKSvg();

        // svgXml -> stream
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(svgXml));
        svg.Load(ms); // ✅ juiste overload


        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        // Als er een viewBox is, respecteer die
        var vb = ParseViewBox(svgXml);
        float scaleX = width / vb.Width;
        float scaleY = height / vb.Height;
        float scale = Math.Min(scaleX, scaleY);

        // centreren
        float dx = (width - vb.Width * scale) / 2f;
        float dy = (height - vb.Height * scale) / 2f;

        canvas.Translate(dx - vb.Left * scale, dy - vb.Top * scale);
        canvas.Scale(scale, scale);
        canvas.DrawPicture(svg.Picture);

        using var img = SKImage.FromBitmap(bitmap);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.OpenWrite(outputPath);
        data.SaveTo(fs);
    }
}
