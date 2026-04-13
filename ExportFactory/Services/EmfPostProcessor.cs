using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace ExportFactory.Services
{
    public static class EmfPostProcessor
    {
        // Deze regex matcht zowel {{EMF:...}} als geëscapeerde \{EMF:...\}
        private static readonly Regex MarkerRegex = new(@"\\?\{EMF:(.+?)\\?\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static void ReplaceEmfMarkersInFile(string rtfPath)
        {
            if (string.IsNullOrWhiteSpace(rtfPath) || !File.Exists(rtfPath))
                throw new FileNotFoundException("RTF-bestand niet gevonden.", rtfPath);

            string rtf = File.ReadAllText(rtfPath, Encoding.ASCII);


            rtf = ReplaceEmfMarkersInText(rtf);

            File.WriteAllText(rtfPath, rtf, Encoding.ASCII);

        }


        /// <summary>
        /// Splitst een hex-string in meerdere regels van een opgegeven lengte, voor RTF overzicht.
        /// </summary>
        /// <param name="hex">De hex-string van de EMF.</param>
        /// <param name="chunkSize">Aantal tekens per regel (standaard 128).</param>
        /// <returns>Geformatteerde hex-string met regelscheiding.</returns>
        public static string FormatHexForRtf(string hex, int chunkSize = 128)
        {
            if (string.IsNullOrEmpty(hex))
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += chunkSize)
            {
                int len = Math.Min(chunkSize, hex.Length - i);
                sb.AppendLine(hex.Substring(i, len));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Vervangt alle {EMF:...} placeholders in RTF door correcte RTF-shapes.
        /// Ondersteunt parameters: path, x, y, rel, wrap
        /// </summary>
        public static string ReplaceEmfMarkersInText(string rtfText)
        {
            if (string.IsNullOrEmpty(rtfText)) return rtfText;

            return Regex.Replace(rtfText, @"\\{EMF:(.*?)\\}", match =>
            {
                string paramText = match.Groups[1].Value;

                // Splitsen op ; en = 
                var parameters = paramText.Split(';')
                                          .Select(p => p.Split('='))
                                          .Where(p => p.Length == 2)
                                          .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

                if (!parameters.TryGetValue("path", out var emfPath))
                    return match.Value; // fallback als path ontbreekt

                // Standaardwaarden
                double xCm = ParseCm(parameters.GetValueOrDefault("x", "0cm"));
                double yCm = ParseCm(parameters.GetValueOrDefault("y", "0cm"));
                string rel = parameters.GetValueOrDefault("rel", "paragraph").ToLowerInvariant();
                string wrap = parameters.GetValueOrDefault("wrap", "square").ToLowerInvariant();
                bool behind = wrap == "behind";

                int relCode = rel switch
                {
                    "page" => 0,
                    "margin" => 1,
                    "column" => 2,
                    "edge" => 3, // paragraph (soms edge)
                    _ => 4 // fallback / andere
                };

                int wrapCode = wrap switch
                {
                    "none" => 1,
                    "square" => 2,
                    "behind" => 3,
                    "front" => 4,
                    _ => 2
                };

                int xTwips = (int)(xCm * 567);
                int yTwips = (int)(yCm * 567);
                using var img = Image.FromFile(emfPath);
                var wTwips = (int)(img.Width / img.HorizontalResolution * 1440);
                var hTwips = (int)(img.Height / img.VerticalResolution * 1440);

                var hex = BuildEmfHexData(emfPath);
                var formattedHex = EmfPostProcessor.FormatHexForRtf(hex);


                return $@"
                {{\pict\emfblip\picwgoal{wTwips}\pichgoal{hTwips}
                {formattedHex}}}
                ";
            }, RegexOptions.IgnoreCase);
        }




        private static double ParseCm(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            input = input.Trim().ToLowerInvariant();

            double factor = 1.0; // standaard cm
            double value = 0.0;
            var num = new string(input.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());
            if (!double.TryParse(num.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                return 0;

            if (input.EndsWith("mm"))
                factor = 0.1;
            else if (input.EndsWith("in") || input.EndsWith("\""))
                factor = 2.54;
            else if (input.EndsWith("px"))
                factor = 2.54 / 96.0; // 96 dpi aannemen
            else if (input.EndsWith("twip") || input.EndsWith("twips"))
                factor = 2.54 / 1440.0;

            return value * factor;
        }


        
        private static string BuildEmfHexData(string emfPath)
        {
            if (string.IsNullOrWhiteSpace(emfPath))
                throw new ArgumentException("EMF pad mag niet leeg zijn.", nameof(emfPath));

            if (!File.Exists(emfPath))
                throw new FileNotFoundException("EMF-bestand niet gevonden.", emfPath);

            if (Path.GetExtension(emfPath)?.ToLowerInvariant() != ".emf")
                throw new InvalidDataException("Bestand is geen EMF-bestand: " + emfPath);

            byte[] data;
            try
            {
                data = File.ReadAllBytes(emfPath);
            }
            catch (Exception ex)
            {
                throw new IOException("Fout bij lezen van EMF-bestand: " + emfPath, ex);
            }

            // Hex-conversie: kleine letters, geen streepjes
            var sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }



        private static string BuildEmfBlock(string emfPath)
        {
            using var img = Image.FromFile(emfPath);

            // RTF gebruikt TWIPS (1 inch = 1440 twips)
            float twipsPerInch = 1440f;
            float widthTwips = img.Width / img.HorizontalResolution * twipsPerInch;
            float heightTwips = img.Height / img.VerticalResolution * twipsPerInch;

            var emfBytes = File.ReadAllBytes(emfPath);
            var hex = BitConverter.ToString(emfBytes).Replace("-", "").ToLowerInvariant();

            // pict/goal attributen zorgen voor correcte schaal
            return $@"{{\pict\emfblip\picwgoal{(int)widthTwips}\pichgoal{(int)heightTwips}
{hex}}}";
        }
    }
}
