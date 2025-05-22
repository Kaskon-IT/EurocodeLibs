
using MigraDoc.DocumentObjectModel;
namespace ExportFactory.MigraDocContentModels
{
    public class TableCellHeaderContent
    {
        public TableCellContent CellContent { get; set; } = new(); // default cell content
        public Unit Width { get; set; } = Unit.FromMillimeter(30); // default width


        public TableCellHeaderContent Clone()
        {
            return new TableCellHeaderContent
            {
                CellContent = this.CellContent.Clone(),
                Width = this.Width
            };
        }

    }






}
