namespace ExportFactory.Shared
{
    public interface IAttributesMapping
    {
        string? Article { get; set; }
        string? Description { get; set; }
        string Key { get; set; }
        string? Symbol { get; set; }
    }
}