using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper
{
    public class Air4TicketFeedToEntities<T> where T : new()
    {
        public static List<T> Convert(string dataString, string tableName, string delimiter)
        {
            var extractDataTable = new List<T>();
            using (StringReader stringReader = new StringReader(dataString))
            {
                using (TextFieldParser parser = new TextFieldParser(stringReader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiter);

                    var properties = typeof(T).GetProperties();

                    while (!parser.EndOfData)
                    {
                        string[] items = parser.ReadFields();

                        foreach (var item in items)
                        {
                            if (item.Equals(tableName))
                            {
                                T extractDataRow = new T();
                                for (int i = 0; i < items.Length - 1; i++)
                                {
                                    try
                                    {
                                        if (i >= properties.Count())
                                        {
                                            break;
                                        }
                                        PropertyInfo prop = properties[i];

                                        if (prop != null && prop.CanWrite)
                                        {
                                            prop.SetValue(extractDataRow, items[i + 1]);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw;
                                    }
                                }
                                extractDataTable.Add(extractDataRow);
                            }
                        }
                    }
                }
            }
            return extractDataTable;
        }
    }
}
