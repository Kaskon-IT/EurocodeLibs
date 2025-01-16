using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace ExportFactory.Interfaces.Export
{
    /// <summary>
    /// Interface for export to CSV (Comma Seperated Value) 
    /// </summary>
    public interface IExportableCsv
    {
        string Export(bool excludeHeaders);
        Task<string> ExportAsync(bool excludeHeaders);
    }

    /// <summary>
    /// Interface for export to RTF (Rich Text Format)
    /// </summary>
    public interface IExportableRtf
    {
        string Export();
        Task<string> ExportAsync();
    }

    /// <summary>
    /// Interface for export to MigraDoc. 
    /// </summary>
    public interface IExportableMigraDoc
    {
        /// <summary>
        /// Adds the object representation to a MigraDoc section.
        /// </summary>
        void AddToSection(Section section);

        Table ExportTable();
        Task<Table> ExportTableAsync();

    }

    public interface IExportableSvg
    {
        string Export();
    }



}
