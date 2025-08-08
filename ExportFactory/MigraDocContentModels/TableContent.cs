namespace ExportFactory.MigraDocContentModels
{
    public class TableContent : SectionElement
    {
        public string Title { get; set; } = "";
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
        public List<TableCellHeaderContent> Headers { get; set; } = new List<TableCellHeaderContent>();
        public List<double> ColumnWidths { get; set; } = new List<double>();
        public List<List<TableCellContent>> Rows { get; set; } = new List<List<TableCellContent>>();
        public bool HideHeaders { get; set; } = false; // default false


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
