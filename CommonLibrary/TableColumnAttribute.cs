using MigraDoc.DocumentObjectModel;

namespace CommonLibrary
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class TableColumnAttribute : Attribute
    {
        [Obsolete("vervang door de info uit deze attribute zelf te halen en niet extern")]
        public string? Key { get; init; }


        /// <summary>
        /// Korte label voor de kolomkop of voor het inputveld en dergelijke.
        /// </summary>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Langere omschrijving voor de property. Te gebruiken in tooltips en dergelijke.
        /// </summary>
        public string? Description { get; set; } = null;


        /// <summary>
        /// Optie om een eigen stringformat op te geven. 
        /// Indien leeg (null) gelaten wordt de globale standaard gebruikt voor getalnotatie.
        /// </summary>
        public string? StringFormat { get; set; } = null;


        /// <summary>
        /// Een symbool in Markdown notatie (kan ook LaTeX zijn)
        /// </summary>
        public string? Symbol { get; set; } = null;

        /// <summary>
        /// Mogelijke verwijzing naar een artikel in de norm
        /// </summary>
        public string? Article { get; set; } = null;

        //[Obsolete("use formula instead")]
        //public string? Formula { get; set; } = null;
        //[Obsolete("Use formula class instead")]
        //public string? DynamicFormulaProperty { get; set; } = null;

        /// <summary>
        /// De mogelijkheid om een eenheid op te geven die achter het getal komt.
        /// Standaard lege string (geen eenheid).
        /// </summary>
        public string? Unit { get; set; } = "";


        // niet nodig, Formules worden buiten deze attribute om afgehandeld
        // De regel is, dat een property met de naam <prop.Name>Formula gezocht wordt
        // bijvoorbeeld bij een property "ScheurwijdteBerekend" wordt gezocht naar "ScheurwijdteBerekendFormula"
        //public Formula? Vergelijking { get; set; } = null;


        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center;

        //[Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        //public WeergaveEnum Weergave { get; set; } = WeergaveEnum.AlleTabellen;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public bool Visible { get; set; } = true;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public double Width { get; set; } = 3.00;

        [Obsolete("Verplaatst, niet meer binnen TableColumnAttribute")]
        public int Order { get; set; } = -1;


        public TableColumnAttribute()
        {

        }

        public TableColumnAttribute(
            string? label = null,
            string? description = null,
            string? stringFormat = null,
            ParagraphAlignment alignment = ParagraphAlignment.Left,
            bool visible = true,
            //WeergaveEnum weergave = WeergaveEnum.AlleTabellen,
            double width = 2.00,
            int order = -1,
            string? key = null,
            string? symbol = null,
            string? article = null
            )
        {
            Key = key;
            Label = label;
            Description = description;
            StringFormat = stringFormat;
            Alignment = alignment;
            Visible = visible;
            //Weergave = weergave;
            Width = width;
            Order = order;
            Symbol = symbol;
            Article = article;
        }


    }
}
