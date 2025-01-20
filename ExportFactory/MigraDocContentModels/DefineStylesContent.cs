namespace ExportFactory.MigraDocContentModels
{
    public class DefineStylesContent
    {
        public string Name { get; set; } // Style name
        public string FontName { get; set; } = "Arial Narrow"; // Font
        public double FontSize { get; set; } = 10; // Font size
        public bool IsBold { get; set; } = false; // Bold
        public bool IsItalic { get; set; } = false; // Italic
        public string TextColor { get; set; } = "Black"; // Text color
        public double SpaceBefore { get; set; } = 0; // Space before paragraph
        public double SpaceAfter { get; set; } = 0; // Space after paragraph
    }






}
