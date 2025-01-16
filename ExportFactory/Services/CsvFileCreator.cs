namespace ExportFactory.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;


    public class CsvFileCreator(char delimiter = '\t')
    {
        private Dictionary<string, string> ColumnHeaders { get; set; } = [];
        private readonly List<Dictionary<string, string>> _rows = [];
        private readonly char _delimiter = delimiter;


        public void SetColumnHeaders(Dictionary<string, string> headers)
        {
            ColumnHeaders = headers;
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

        public StringWriter GetCsvStringWriter()
        {
            StringWriter sw = new StringWriter();
            foreach (var row in _rows)
            {
                List<string> rowValues = [];
                foreach (var header in ColumnHeaders)
                {
                    if (row.TryGetValue(header.Key, out string? value))
                    {
                        rowValues.Add(EscapeForCsv(value));
                    }
                    else
                    {
                        rowValues.Add(string.Empty);
                    }
                }
                sw.WriteLine(string.Join(_delimiter, rowValues));
            }


            return sw;
        }

        public StringBuilder GetCsvStringBuilder()
        {
            StringBuilder sb = new();
            foreach (var row in _rows)
            {
                List<string> rowValues = [];
                foreach (var header in ColumnHeaders)
                {
                    if (row.TryGetValue(header.Key, out string? value))
                    {
                        rowValues.Add(EscapeForCsv(value));
                    }
                    else
                    {
                        rowValues.Add(string.Empty);
                    }
                }
                sb.AppendLine(string.Join(_delimiter, rowValues));
            }
            return sb;
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
