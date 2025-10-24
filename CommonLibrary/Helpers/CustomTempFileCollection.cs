namespace CommonLibrary.Helpers
{
    public sealed class CustomTempFileCollection
    {
        private readonly List<string> _files = new();

        public string? AddFile(string? sourcePath)
        {
            // Als pad leeg of null is → overslaan met melding
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                Console.WriteLine("[CustomTempFileCollection] Geen pad opgegeven, bestand wordt overgeslagen.");
                return null;
            }

            // Als bestand niet bestaat → overslaan met melding
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"[CustomTempFileCollection] Bestand niet gevonden: {sourcePath}");
                return null;
            }

            try
            {
                var fileName = Path.GetFileName(sourcePath);
                var tempPath = Path.Combine(Path.GetTempPath(), $"migraDoc_{Guid.NewGuid()}_{fileName}");
                File.Copy(sourcePath, tempPath, overwrite: true);
                _files.Add(tempPath);
                return tempPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomTempFileCollection] Fout bij kopiëren van bestand '{sourcePath}': {ex.Message}");
                return null;
            }
        }


        // Optioneel: alle paden opvragen (bijvoorbeeld voor debug / logging)
        public IReadOnlyList<string> Files => _files.AsReadOnly();
    }

}
