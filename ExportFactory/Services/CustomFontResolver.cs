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


                    _faceNameToKey[fontKey] = fontKey;
                }
            }

            Console.WriteLine($"✅ Fonts geladen: {string.Join(", ", _fontData.Keys)}");


            // Voeg handmatige alias toe voor veelgebruikte fontnamen

            // Deze alias zijn handig voor compatibiliteit en gebruiksgemak
            // AddAlias("roboto", "roboto-regular");           // Regular
            // AddAlias("roboto#b", "roboto-bold");            // Bold
            // AddAlias("roboto#i", "roboto-italic");          // Italic
            // AddAlias("roboto#bi", "roboto-bolditalic");     // BoldItalic



            AddAlias("arial", "arial");
            AddAlias("arial#b", "arialbd");
            AddAlias("arial#i", "ariali");
            AddAlias("arial#bi", "arialbi");



            AddAlias("calibri", "calibri");
            AddAlias("calibri#b", "calibrib");
            AddAlias("calibri#i", "calibrili");


            AddAlias("verdana", "verdana");
            AddAlias("consolas", "consolas");

            AddAlias("roboto", "roboto-regular"); // bijvoorbeeld
            AddAlias("roboto#b", "roboto-bold"); // bijvoorbeeld
            AddAlias("roboto#i", "roboto-italic"); // bijvoorbeeld
            AddAlias("roboto#bi", "roboto-bolditalic"); // bijvoorbeeld

            AddAlias("notoemoji", "notoemoji-regular"); // bijvoorbeeld
            AddAlias("notoemoji#b", "notoemoji-bold"); // bijvoorbeeld

            AddAlias("notosans", "notosans-regular"); // bijvoorbeeld
            AddAlias("notosans#b", "notosans-bold"); // bijvoorbeeld
            AddAlias("notosans#i", "notosans-italic"); // bijvoorbeeld
            AddAlias("notosans#bi", "notosans-bolditalic"); // bijvoorbeeld

            AddAlias("segoe ui emoji", "seguiemj"); // bijvoorbeeld
            AddAlias("inconsolata", "inconsolata-regular");
            AddAlias("latin modern math", "latinmodern-math");
            AddAlias("stix math", "stix-math");
        }

        private void AddAlias(string faceName, string fontKey)
        {
            if (_fontData.ContainsKey(fontKey))
            {
                _faceNameToKey[faceName.ToLowerInvariant()] = fontKey.ToLowerInvariant();
            }
        }

        public byte[] GetFont(string faceName)
        {
            if (_fontData.TryGetValue(faceName.ToLowerInvariant(), out var data))
                return data;

            throw new InvalidOperationException($"Font '{faceName}' not registered.");
        }

        //public string? ResolveTypefaceREFERENTIE_VERWIJDREN(string familyName, bool isBold, bool isItalic)
        //{
        //    var key = familyName.ToLowerInvariant();
        //    if (isBold && isItalic)
        //        key += "#bi";
        //    else if (isBold)
        //        key += "#b";
        //    else if (isItalic)
        //        key += "#i";

        //    Console.WriteLine($"Resolving typeface: {key}");

        //    return _faceNameToKey.TryGetValue(key, out var fontKey) ? fontKey : null;
        //}

        //public FontResolverInfo ResolveTypefaceVERWIJDEREN(string familyName, bool isBold, bool isItalic)
        //{
        //    if (_faceNameToKey.TryGetValue(familyName.ToLowerInvariant(), out var key))
        //        return new FontResolverInfo(key);




        //    // Fallback op Arial
        //    return new FontResolverInfo("arial");
        //}

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var key = familyName.ToLowerInvariant();

            if (isBold && isItalic)
                key += "#bi";
            else if (isBold)
                key += "#b";
            else if (isItalic)
                key += "#i";

            if (_faceNameToKey.TryGetValue(key, out var fontKey))
                return new FontResolverInfo(fontKey);

            // Fallback: probeer regular
            if (_faceNameToKey.TryGetValue(familyName.ToLowerInvariant(), out var regularKey))
                return new FontResolverInfo(regularKey);

            // Final fallback: arial
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
