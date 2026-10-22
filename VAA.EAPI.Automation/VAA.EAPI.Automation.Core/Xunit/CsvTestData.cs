using System.Collections;
using System.Data;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class CsvTestData : IEnumerable<object[]>
    {
        private List<object[]> data = new();

        public CsvTestData(string path, string delimiters, bool hasHeaders, bool hasFieldsEnclosedInQuotes, string groupBy)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("File not found.", path);

            using (DataTable dataTable = new DataTable())
            {
                if (!dataTable.FromDelimitedFile(path, delimiters, hasFieldsEnclosedInQuotes, hasHeaders)) throw new FileLoadException("Invalid delimited file.", path);

                IEnumerable<DataRow> dataRows = dataTable.Rows.OfType<DataRow>();

                if (string.IsNullOrWhiteSpace(groupBy))
                    foreach (DataRow dataRow in dataRows)
                        data.Add(dataRow.ItemArray);
                else
                {
                    IEnumerable<object[]> groups = dataRows.GroupBy(r => r[groupBy])
                                                           .Select(g => new object[] { g.Key, g.Select(x => x.ItemArray).ToArray() });

                    foreach (object[] group in groups)
                        data.Add(group);
                }
            }
        }

        public IEnumerator<object[]> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
