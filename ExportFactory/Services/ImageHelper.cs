namespace ExportFactory.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;


    /// <summary>
    /// Eigen klasse voor het beheren van tijdelijke bestanden.
    /// Dit zorgt voor een efficiënte manier om tijdelijke bestanden te creëren en op te ruimen.
    /// Gemaakt om te worden gebruikt in combinatie met MigraDoc voor documentgeneratie.
    /// MigraDoc PDF export work-around: 
    /// </summary>
    public sealed class CustomTempFileCollectionXX : IDisposable
    {
        private readonly List<string> _files = new();

        /// <summary>
        /// Maakt een tijdelijke kopie van een bestaand bestand.
        /// Het pad naar de tempkopie wordt teruggegeven.
        /// </summary>
        public string AddFile(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Bestand niet gevonden.", sourcePath);

            var fileName = Path.GetFileName(sourcePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"migradoc_{Guid.NewGuid()}_{fileName}");

            File.Copy(sourcePath, tempPath, overwrite: true);

            _files.Add(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Ruimt alle tijdelijke bestanden op.
        /// </summary>
        public void Dispose()
        {
            foreach (var file in _files)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch
                {
                    // Cleanup errors negeren
                }
            }

            _files.Clear();
        }
    }

}
