using System.Text.Json.Serialization;

namespace ExportFactory.MigraDocContentModels
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(HeadingContent), "headingContent")]
    [JsonDerivedType(typeof(TableContent), "tableContent")]
    [JsonDerivedType(typeof(SectionContent), "sectionContent")]
    [JsonDerivedType(typeof(ParagraphContent), "paragraphContent")]
    [JsonDerivedType(typeof(MigraDocElement), "migraDocElement")]
    [JsonDerivedType(typeof(MigraDocTable), "migraDocTable")]

    public abstract class SectionElement
    {
        public int Order { get; set; }
    }

    public class HeadingContent : SectionElement
    {
        public HeadingContent() { }


        public HeadingContent(string text, string style)
        {
            Text = text;
            Style = style;

        }


        public HeadingContent(string text, int level)
        {
            Text = text;
            Style = $"Kop {level}";
            AddToTOC = (level == 1 || level == 2);
            Level = level;
        }


        public HeadingContent(string text, string style, bool addToTOC, int level)
        {
            Text = text;
            Style = style;
            AddToTOC = addToTOC;
            Level = level;
        }

        public string Text { get; set; } = "Untitled Heading";
        public string Style { get; set; } = "Normal";
        public bool AddToTOC { get; set; } = true; // optional set to false to skip from TOC
        public int Level { get; set; } = 1; // default h1



    }

    //public class TableContent<T> : SectionElement
    //{
    //     public List<T> TableData { get; set; } = [];
    // }

    //public class TableContent2


    public enum TableAlignment
    {
        Left,
        Center
    }



}
