namespace CommonLibrary.Helpers
{
    public sealed class CustomTempFileCollection
    {
        private readonly List<string> _files = new();

        public string AddFile(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Bestand niet gevonden.", sourcePath);

            var fileName = Path.GetFileName(sourcePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"migraDoc_{Guid.NewGuid()}_{fileName}");
            File.Copy(sourcePath, tempPath, overwrite: true);
            _files.Add(tempPath);
            return tempPath;
        }

        // Optioneel: alle paden opvragen (bijvoorbeeld voor debug / logging)
        public IReadOnlyList<string> Files => _files.AsReadOnly();
    }

}
