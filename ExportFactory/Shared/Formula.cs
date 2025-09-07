namespace ExportFactory.Shared
{
    public class Formula
    {
        /// <summary>
        /// Statische LaTex string
        /// </summary>
        public string StaticValue { get; set; } = "";

        /// <summary>
        /// Dynamische LaTex string (voor ingevulde waarde)
        /// </summary>
        public string? DynamicValue { get; set; } = "";

        /// <summary>
        /// Naam bijvoorbeeld (B.3)
        /// </summary>
        public string Name { get; set; } = "";
    }

}
