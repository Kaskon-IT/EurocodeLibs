namespace Eurocode.Blazor.Demo.Shared.SampleData
{
    /// <summary>
    /// Represents metadata information for a document.
    /// </summary>
    public class DocumentInfo
    {
        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author of the document.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subject of the document.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the keywords associated with the document.
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// Gets or sets the producer of the document (e.g., the software that created it).
        /// </summary>
        public string Producer { get; set; } = "ExportFactory";

        /// <summary>
        /// Gets or sets the creation date of the document.
        /// </summary>
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the modification date of the document.
        /// </summary>
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the version of the document standard.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Converts the keywords list to a semicolon-separated string.
        /// </summary>
        /// <returns>A string of semicolon-separated keywords.</returns>
        public string GetKeywordsAsString()
        {
            return string.Join("; ", Keywords);
        }

        /// <summary>
        /// Populates the keywords list from a semicolon-separated string.
        /// </summary>
        /// <param name="keywords">A string of semicolon-separated keywords.</param>
        public void SetKeywordsFromString(string keywords)
        {
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                Keywords = new List<string>(keywords.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// Displays a summary of the document info.
        /// </summary>
        /// <returns>A formatted string with document metadata.</returns>
        public override string ToString()
        {
            return $"Title: {Title}\n" +
                   $"Author: {Author}\n" +
                   $"Subject: {Subject}\n" +
                   $"Keywords: {GetKeywordsAsString()}\n" +
                   $"Producer: {Producer}\n" +
                   $"Created: {CreationDate}\n" +
                   $"Modified: {ModificationDate}\n" +
                   $"Version: {Version}";
        }
    }
}
