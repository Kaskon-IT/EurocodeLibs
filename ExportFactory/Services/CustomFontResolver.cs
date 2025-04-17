namespace ExportFactory.Services
{
    using PdfSharp.Fonts;




    public class CustomFontResolver : IFontResolver
    {
        private readonly Dictionary<string, byte[]> _fontData = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _faceNameToKey = new(StringComparer.OrdinalIgnoreCase);

        public CustomFontResolver()
        {
            var assembly = typeof(CustomFontResolver).Assembly;
            var allResources = assembly.GetManifestResourceNames();

            foreach (var resource in allResources)
            {
                if (resource.StartsWith("ExportFactory.Resources.Fonts.", StringComparison.OrdinalIgnoreCase)
         && resource.EndsWith(".TTF", StringComparison.OrdinalIgnoreCase))
                {
                    var resourceNameParts = resource.Split('.');
                    var fontKey = resourceNameParts[^2].ToLowerInvariant(); // bv. "arial"

                    using var stream = assembly.GetManifestResourceStream(resource);
                    if (stream == null)
                    {
                        Console.WriteLine($"❌ Geen stream voor resource: {resource}");
                        continue;
                    }

                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    _fontData[fontKey] = ms.ToArray();

                    Console.WriteLine($"✅ Font geladen: {fontKey} vanuit resource: {resource}");

                    _faceNameToKey[fontKey] = fontKey;
                }
            }

            // Voeg handmatige alias toe voor veelgebruikte fontnamen
            AddAlias("arial", "arial");
            AddAlias("calibri", "calibri");
            AddAlias("calibri light", "calibril");
            AddAlias("verdana", "verdana");
            AddAlias("consolas", "consolas");
            AddAlias("roboto", "roboto-regular"); // bijvoorbeeld
            AddAlias("inconsolata", "inconsolata-regular");
            AddAlias("latin modern math", "latinmodern-math");
            AddAlias("stix math", "stix-math");
        }

        private void AddAlias(string faceName, string fontKey)
        {
            if (_fontData.ContainsKey(fontKey))
            {
                _faceNameToKey[faceName.ToLowerInvariant()] = fontKey;
            }
        }

        public byte[] GetFont(string faceName)
        {
            if (_fontData.TryGetValue(faceName.ToLowerInvariant(), out var data))
                return data;

            throw new InvalidOperationException($"Font '{faceName}' not registered.");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (_faceNameToKey.TryGetValue(familyName.ToLowerInvariant(), out var key))
                return new FontResolverInfo(key);

            // Fallback op Arial
            return new FontResolverInfo("arial");
        }
    }


    public static class ResourceDebugger
    {
        public static void LogAllEmbeddedResources()
        {
            var assembly = typeof(CustomFontResolver).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();

            Console.WriteLine("=== Embedded Resources in ExportFactory ===");
            foreach (var name in resourceNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine("=== End of List ===");
        }
    }


}
