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

            var part2 = "";
            if (DynamicValue != null)
            {
                part2 = DynamicValue;

                if (removeSymbol)
                {
                    var index = DynamicValue.IndexOf('=');
                    string result = index >= 0 ? DynamicValue.Substring(index) : string.Empty;
                    part2 = result;
                }


            }
            return $"{StaticValue}\\\\{part2}";

        }

    }

}
