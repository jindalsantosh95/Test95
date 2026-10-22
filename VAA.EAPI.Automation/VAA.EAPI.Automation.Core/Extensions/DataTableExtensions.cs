using System.Data;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class DataTableExtension
    {
        public static bool FromDelimitedFile(this DataTable source, string path, string delimiters, bool hasFieldsEnclosedInQuotes = false, bool hasHeaders = true) => source.FromDelimitedFile(path, delimiters, out Exception exception, hasFieldsEnclosedInQuotes, hasHeaders);
        public static bool FromDelimitedFile(this DataTable source, string path, string delimiters, out Exception exception, bool hasFieldsEnclosedInQuotes = false, bool hasHeaders = true)
        {
            if (source == null)
                throw new ArgumentNullException("source", "Cannot be of null or empty value.");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path", "Cannot be of null or empty value.");
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException(string.Empty, path);
            if (string.IsNullOrEmpty(delimiters))
                throw new ArgumentNullException("delimiters", "Cannot be of null or empty value.");

            exception = null;
            bool loaded = true;
            bool headersLoaded = false;

            source.TableName = Path.GetFileName(path);

            source.BeginLoadData();

            // detect maximum columns required by scanning the file, only way when the CSV data is jagged
            using (Microsoft.VisualBasic.FileIO.TextFieldParser textFieldParser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path) { HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes })
            {
                textFieldParser.SetDelimiters(delimiters.ToCharArray().Select(c => c.ToString()).ToArray());

                while (!textFieldParser.EndOfData)
                {
                    try
                    {
                        string[] values = textFieldParser.ReadFields();

                        if (hasHeaders)
                        {
                            for (int i = 0, count = values.Length; i < count; i++)
                                source.Columns.Add(new DataColumn(values[i]) { AllowDBNull = true });

                            break;
                        }
                        else if (values.Length > source.Columns.Count)
                            for (int i = source.Columns.Count, count = values.Length; i < count; i++)
                                source.Columns.Add(new DataColumn($"Column_{i}") { AllowDBNull = true });
                    }
                    catch { }
                }
            }

            // now we load the data
            using (Microsoft.VisualBasic.FileIO.TextFieldParser textFieldParser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path) { HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes })
            {
                textFieldParser.SetDelimiters(delimiters.ToCharArray().Select(c => c.ToString()).ToArray());

                while (!textFieldParser.EndOfData)
                {
                    try
                    {
                        object[] row = new object[source.Columns.Count];
                        object[] values = textFieldParser.ReadFields();

                        if (hasHeaders && !headersLoaded)
                        {
                            headersLoaded = true;
                            continue;
                        }

                        Array.Copy(values, row, values.Length);

                        source.Rows.Add(row);
                    }
                    catch (Microsoft.VisualBasic.FileIO.MalformedLineException ex)
                    {
                        exception = ex;

                        loaded = false;

                        break;
                    }
                }
            }

            source.EndLoadData();

            return loaded;
        }
    }
}
