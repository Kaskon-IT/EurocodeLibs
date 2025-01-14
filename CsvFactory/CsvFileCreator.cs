namespace CsvFactory
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class CsvFileCreator(char delimiter = '\t')
    {
        private readonly List<string> _columns = [];
        private readonly List<Dictionary<string, string>> _rows = [];
        private readonly char _delimiter = delimiter;

        /// <summary>
        /// Adds a column to the CSV file.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        public void AddColumn(string columnName)
        {
            if (!_columns.Contains(columnName))
            {
                _columns.Add(columnName);
            }
        }

        public void AddColumns(List<string> columnNames)
        {
            foreach (string columnName in columnNames)
            {
                AddColumn(columnName);
            }
        }

        /// <summary>
        /// Adds a row of data to the CSV file.
        /// </summary>
        /// <param name="rowData">A dictionary representing the row data.</param>
        public void AddRow(Dictionary<string, string> rowData)
        {
            _rows.Add(rowData);
        }

        /// <summary>
        /// Adds a list of rowdata's to the CSV file.
        /// </summary>
        /// <param name="rowDatas">A list of dictionary representing the row data foreach row</param>
        public void AddRows(List<Dictionary<string, string>> rowDatas)
        {
            foreach (var rowData in rowDatas)
            {
                _rows.Add(rowData);
            }
        }

        public StringBuilder GetCsvStringBuilder()
        {
            StringBuilder csvContent = new();

            // Write header row
            csvContent.AppendLine(string.Join(_delimiter, _columns));

            // Write data rows
            foreach (var row in _rows)
            {
                List<string> rowValues = [];
                foreach (var column in _columns)
                {
                    if (row.TryGetValue(column, out string? value))
                    {
                        rowValues.Add(EscapeForCsv(value));
                    }
                    else
                    {
                        rowValues.Add(string.Empty);
                    }
                }
                csvContent.AppendLine(string.Join(_delimiter, rowValues));
            }
            return csvContent;
        }


        /// <summary>
        /// Saves the CSV file to the specified path.
        /// </summary>
        /// <param name="filePath">The path where the CSV file will be saved.</param>
        public void SaveToFile(string filePath)
        {
            var sb = GetCsvStringBuilder();

            // Save to file
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Escapes a value for inclusion in a CSV file.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        private string EscapeForCsv(string value)
        {
            if (value.Contains(_delimiter) || value.Contains('"') || value.Contains('\n'))
            {
                value = '"' + value.Replace("\"", "\"\"") + '"';
            }
            return value;
            // todo aanvullen zodat er ook linebreaks toegepast kunnen worden.
            // controleer met MAC en Windows
            // bijvoorbeeld
            // ----------------------------
            // |   M    |  col2  |  col3  |
            // | [kNm]  |        |        |
            // ----------------------------

        }
    }

}
