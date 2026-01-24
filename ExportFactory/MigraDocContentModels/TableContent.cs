namespace ExportFactory.MigraDocContentModels
{
    public class TableContent : SectionElement
    {
        public string Title { get; set; } = "";
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
        public List<TableCellHeaderContent> Headers { get; set; } = new List<TableCellHeaderContent>();
        public List<double> ColumnWidths { get; set; } = new List<double>();
        public List<List<TableCellContent>> Rows { get; set; } = new List<List<TableCellContent>>();
        
        /// <summary>
        /// Geeft de optie om de headers niet weer te geven.
        /// </summary>
        public bool HideHeaders { get; set; } = false; // default false

        /// <summary>
        /// Bepaald of het een normale tabel is of een draaitabel (pivot table).
        /// Bij een draaitabel krijgt de table de 'row-head' CSS klasse erbij.
        /// Bij een normale table 'ec-table' CSS klasse.
        /// </summary>
        public bool IsPivotTable { get; set; } = false;

        /// <summary>
        /// Bepaald of alleeen de layout van de tabel wordt aangehouden, dus zonder styling.
        /// Er wordt wel een gestippelde cell-border getoond om de cellen aan te geven.
        /// Maar deze wordt NIET geprint.
        /// </summary>
        public bool LayoutOnly { get; set; } = false;

        public TableContent Clone()
        {
            return new TableContent
            {
                Title = this.Title,
                Alignment = this.Alignment,
                HideHeaders = this.HideHeaders,
                ColumnWidths = new List<double>(this.ColumnWidths),
                Headers = this.Headers.Select(h => h.Clone()).ToList(),
                Rows = this.Rows.Select(row => row.Select(cell => cell.Clone()).ToList()).ToList()
            };
        }

    }



}
