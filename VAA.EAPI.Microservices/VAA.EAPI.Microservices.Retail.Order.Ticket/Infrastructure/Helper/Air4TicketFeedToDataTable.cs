using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper
{
    class Air4TicketFeedToDataTable<T> where T : new()
    {
        public static DataTable Convert(string dataString, string tableName, string delimiter)
        {
            var extractDataTable = GetTableStructure(tableName);
            //Open the string in a stream reader.
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(dataString)))
            {
                using (var reader = new StreamReader(memoryStream))
                {
                    //Read the rest of the data in the file.        
                    string allData = reader.ReadToEnd();
                    //Split off each row at the Carriage Return Line Feed
                    string[] rows = allData.Split("\r\n".ToCharArray());

                    foreach (string r in rows)
                    {
                        //Split the row at the delimiter.
                        if (r.Trim().Length > 0)
                        {
                            string[] items = r.Split(delimiter.ToCharArray());

                            if (rows.Length == 1)
                                return ConvertTableInfoFromArrayOfString(items, tableName);
                            else
                            {
                                foreach (var item in items)
                                {
                                    if (item.Equals(tableName))
                                    {
                                        DataRow extractDataRow = extractDataTable.NewRow();
                                        for (int i = 0; i < items.Length - 1; i++)
                                        {
                                            //+1 is to skip table Name
                                            extractDataRow[i] = items[i + 1];
                                        }
                                        extractDataTable.Rows.Add(extractDataRow);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return extractDataTable;
        }

        private static DataTable ConvertTableInfoFromArrayOfString(string[] stream, string tableName)
        {
            var extractDataTable = GetTableStructure(tableName);
            var rawtableInfo = stream
                                .Select((value, index) => (index, value))
                                .Where(x => x.value.Contains(tableName));

            foreach (var rowItem in rawtableInfo)
            {
                if (rowItem.value.EndsWith(tableName))
                {
                    //+1 is to skip table Name

                    var lastOfIndexCounter = stream.Skip(rowItem.index + 1)
                                            .Select((value, index) => (index, value))
                                            .FirstOrDefault(item => item.value.Contains(Constants.Application.TicketFeedTablesStartWith));

                    DataRow extractDataRow = extractDataTable.NewRow();
                    for (int j = 0; j <= lastOfIndexCounter.index; j++)
                    {
                        //+1 is to skip table Name
                        extractDataRow[j] = stream[rowItem.index + 1 + j].Contains(Constants.Application.TicketFeedTablesStartWith)
                                            ? stream[rowItem.index + 1 + j][..stream[rowItem.index + 1 + j].IndexOf(Constants.Application.TicketFeedTablesStartWith)]
                                            : stream[rowItem.index + 1 + j];
                    }

                    extractDataTable.Rows.Add(extractDataRow);
                }
            }
            return extractDataTable;
        }

        private static DataTable GetTableStructure(string tableName)
        {
            var dataTable = ConvertObjectToDataTable(new T(), tableName);
            dataTable.Rows.Clear();
            return dataTable;
        }

        private static DataTable ConvertObjectToDataTable(object obj, string tableName)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            DataTable extract = new(tableName);

            DataRow dr = extract.NewRow();
            extract.Rows.Add(dr);

            foreach (var f in obj.GetType().GetProperties())
            {
                f.GetValue(obj, null);
                extract.Columns.Add(f.Name, f.PropertyType);
                extract.Rows[0][f.Name] = f.GetValue(obj, null);
            }
            return extract;
        }
    }
}