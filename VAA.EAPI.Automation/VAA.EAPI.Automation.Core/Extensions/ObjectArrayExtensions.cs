using System.Data;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class ObjectArrayExtensions
    {
        public static DataTable ConvertToDataTable(this object[] source, string name, params string[] columnNames)
        {
            if (source == null)
                ArgumentNullException.ThrowIfNull(source, nameof(source));
            if (string.IsNullOrWhiteSpace(name))
                ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (columnNames == null)
                ArgumentNullException.ThrowIfNull(columnNames, nameof(columnNames));
            if (!columnNames.Any())
                throw new ArgumentOutOfRangeException(nameof(columnNames), "Must contain at least 1 element.");

            int max = source.Max(x => ((object[])x).Length);

            if (max > columnNames.Length)
                throw new ArgumentOutOfRangeException(nameof(columnNames), $"Expecting {max} column names.");

            DataTable dataTable = new DataTable(name);

            foreach (string columnName in columnNames)
                dataTable.Columns.Add(columnName, typeof(string));

            foreach (object[] values in source)
                dataTable.LoadDataRow(values, true);

            return dataTable;
        }
    }
}
