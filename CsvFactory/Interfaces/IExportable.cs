using MigraDoc.DocumentObjectModel;

namespace CsvFactory.Interfaces
{
    /// <summary>
    /// Generic interface for export of an object.
    /// </summary>
    public interface IExportable<T>
    {
        /// <summary>
        /// Exports object of type T to specified format.
        /// </summary>
        /// <returns></returns>
        T Export();
    }

    /// <summary>
    /// Interface for export to CSV (Comma Seperated Value) 
    /// </summary>
    public interface IExportableCsv
    {
        string Export();
    }

    /// <summary>
    /// Interface for export to RTF (Rich Text Format)
    /// </summary>
    public interface IExportableRtf
    {
        string Export();
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
    }

    public interface ISvgExportable
    {
        string Export();
    }

}
