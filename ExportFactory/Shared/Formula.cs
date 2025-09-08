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
    }

}
