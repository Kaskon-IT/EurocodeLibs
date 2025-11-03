namespace ExportFactory.Shared
{
    [Flags]
    public enum ColumnPart
    {
        None = 0,
        Header = 1 << 0,
        Symbol = 1 << 1,
        Value = 1 << 2,
        Article = 1 << 3,
        Formula = 1 << 4
    }
}
