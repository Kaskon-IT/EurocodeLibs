using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace ExportFactory.MigraDocContentModels
{



    public enum AutoColumnSizeOption
    {
        NotSet,
        None,
        ColumnHeader,
        AllCellsExceptHeader,
        AllCells,
        Fill
    }

    public enum WordTableStyle
    {
        None,
        GridTable1Light,
        GridTable1Dark,
        ListTable1Light,
        ListTable1Dark,
        BandedTable1Light,
        BandedTable1Dark,
        // Add more styles as needed to match Word presets
    }


    public class ColumnStyleSettings
    {
        public AutoColumnSizeOption AutoSize { get; set; } = AutoColumnSizeOption.None; // default
        public ParagraphAlignment Alignment { get; set; } = ParagraphAlignment.Center; // default

    }


    public class TableStyleContent
    {
        /// <summary>
        /// Determines the auto column size behavior for the table.
        /// </summary>
        public AutoColumnSizeOption AutoColumnSize { get; set; } = AutoColumnSizeOption.NotSet;

        /// <summary>
        /// Sets the table style, mimicking Word table style presets.
        /// </summary>
        public WordTableStyle TableStyle { get; set; } = WordTableStyle.None;

        /// <summary>
        /// Table header background color.
        /// </summary>
        public Color HeaderBackgroundColor { get; set; } = Colors.LightGray;

        /// <summary>
        /// Table border color.
        /// </summary>
        public Color BorderColor { get; set; } = Colors.Black;

        /// <summary>
        /// Table border width.
        /// </summary>
        public Unit BorderWidth { get; set; } = 0.5;

        /// <summary>
        /// Table cell padding.
        /// </summary>
        public Unit CellPadding { get; set; } = 2;

        /// <summary>
        /// Default font for table text.
        /// </summary>
        public string FontName { get; set; } = "Arial";

        /// <summary>
        /// Default font size for table text.
        /// </summary>
        public Unit FontSize { get; set; } = 9;

        /// <summary>
        /// Whether the first row should be styled as a header.
        /// </summary>
        public bool FirstRowHeader { get; set; } = true;

        /// <summary>
        /// Whether the last row should be styled as a header.
        /// </summary>
        public bool LastRowHeader { get; set; } = false;

        /// <summary>
        /// Whether the first column should be styled as a header.
        /// </summary>
        public bool FirstColumnHeader { get; set; } = false;

        /// <summary>
        /// Whether the last column should be styled as a header.
        /// </summary>
        public bool LastColumnHeader { get; set; } = false;

        /// <summary>
        /// Apply styles to a table.
        /// </summary>
        public void ApplyToTable(Table table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            // Set table border
            table.Borders.Color = BorderColor;
            table.Borders.Width = BorderWidth;

            // Apply padding to all cells
            foreach (Row row in table.Rows)
            {
                foreach (Cell cell in row.Cells)
                {
                    cell.Format.LeftIndent = CellPadding;
                    cell.Format.RightIndent = CellPadding;
                    cell.Format.Font.Name = FontName;
                    cell.Format.Font.Size = FontSize;
                }
            }

            // Apply header styles
            if (FirstRowHeader && table.Rows.Count > 0)
            {
                StyleRow(table.Rows[0]);
            }

            if (LastRowHeader && table.Rows.Count > 1)
            {
                StyleRow(table.Rows[table.Rows.Count - 1]);
            }

            if (FirstColumnHeader && table.Columns.Count > 0)
            {
                foreach (Row row in table.Rows)
                {
                    StyleCell(row.Cells[0]);
                }
            }

            if (LastColumnHeader && table.Columns.Count > 1)
            {
                foreach (Row row in table.Rows)
                {
                    StyleCell(row.Cells[row.Cells.Count - 1]);
                }
            }
        }

        /// <summary>
        /// Apply header styles to a row.
        /// </summary>
        private void StyleRow(Row row)
        {
            foreach (Cell cell in row.Cells)
            {
                StyleCell(cell);
            }
        }

        /// <summary>
        /// Apply header styles to a cell.
        /// </summary>
        private void StyleCell(Cell cell)
        {
            cell.Shading.Color = HeaderBackgroundColor;
            cell.Format.Font.Name = FontName;
            cell.Format.Font.Size = FontSize;
            cell.Format.Font.Bold = true;
        }
    }

}
