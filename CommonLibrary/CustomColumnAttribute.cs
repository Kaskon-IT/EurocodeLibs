using MigraDoc.DocumentObjectModel;

namespace CommonLibrary
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomColumnAttribute : Attribute
    {
        /// <summary>
        /// Name of the column header
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// String formatting ("E2", etcetera)
        /// </summary>
        public string? Format { get; set; } = null;
        /// <summary>
        /// Width in centimeters
        /// </summary>
        public double Width { get; set; } = 3;

        /// <summary>
        /// Alignment for column
        /// </summary>
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;

        public int ColumnPosition { get; set; } = -1;

        /// <summary>
        /// Set column visibility
        /// </summary>
        public bool Visible { get; set; }

        public CustomColumnAttribute(string headerName)
        {
            HeaderName = headerName;
        }

        public CustomColumnAttribute(string headerName, string? format = null, double width = 3, ParagraphAlignment alignment = ParagraphAlignment.Center, int columnPosition = -1, bool visible = true)
        {
            HeaderName = headerName;
            Format = format;
            Width = width;
            Alignment = alignment;
            ColumnPosition = columnPosition;
            Visible = visible;
        }
    }
}
