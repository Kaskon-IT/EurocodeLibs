namespace ExportFactory.Interfaces.Export
{
    public class TableData<T> where T : class
    {
        public List<T> Rows { get; set; } = [];
    }
}