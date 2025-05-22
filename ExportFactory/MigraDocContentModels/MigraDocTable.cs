namespace ExportFactory.MigraDocContentModels
{
    /// <summary>
    /// Gebruik dit om direct een MigraDoc.Table in de documentContent te plaatsen.
    /// </summary>
    public class MigraDocTable : SectionElement
    {
        public MigraDoc.DocumentObjectModel.Tables.Table Table { get; set; } = new();
    }



}
