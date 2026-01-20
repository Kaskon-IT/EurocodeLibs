namespace ExportFactory.Shared
{
    public class Formula
    {
        public Formula()
        {

        }

        public Formula(string name, string staticValue, string? dynamicValue)
        {
            Name = name;
            StaticValue = staticValue;
            DynamicValue = dynamicValue;
        }


        public List<string> Lines { get; set; } = [];
        public string BuildFormula()
        {
            if (Lines == null || Lines.Count == 0)
            {
                // Standaard
                Lines.Add(StaticValue);
                if (DynamicValue != null)
                {
                    Lines.Add(DynamicValue);
                }
            }

            var joined = string.Join(" \\\\ ", Lines);

            return $"\\begin{{alignedat}} {joined} \\end{{alignedat}}";
        }

        /// <summary>
        /// Statische LaTex string
        /// </summary>
        public string StaticValue { get; set; } = "";

        /// <summary>
        /// Dynamische LaTex string (voor ingevulde waarde)
        /// </summary>
        public string? DynamicValue { get; set; } = null;

        /// <summary>
        /// Naam bijvoorbeeld (B.3)
        /// </summary>
        public string Name { get; set; } = "";


        public string GetValue(bool removeSymbol = false)
        {
            int aantalKarakters = DynamicValue?.Length ?? 0 + StaticValue.Length;
            int aantalKaraktersVoorLineBreak = 120;

            var part1 = FormulaSanitizer.Sanitize(StaticValue);
            var part2 = "";
            if (DynamicValue != null)
            {
                part2 = FormulaSanitizer.Sanitize(DynamicValue);

                if (removeSymbol || aantalKarakters < aantalKaraktersVoorLineBreak) // kleiner dan dit aantal sowieso in 1 lijn
                {
                    var index = part2.IndexOf('=');
                    string result = index >= 0 ? part2.Substring(index) : string.Empty;
                    part2 = result;
                }
            }

            if (aantalKarakters < aantalKaraktersVoorLineBreak)
                return $"{part1} {part2}";
            else
                return $"{part1}\\\\{part2}";
        }

        public static class FormulaSanitizer
        {
            public static string Sanitize(string tex)
            {
                if (string.IsNullOrEmpty(tex))
                    return tex;

                return tex
                    .Replace("‰", @"\text{\textperthousand}")
                    .Replace("–", @"\text{--}")
                    .Replace("Ø", @"\text{Ø}");
            }
        }

    }

}
