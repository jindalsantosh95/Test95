using System.Collections.Concurrent;
using System.Data;
using Microsoft.Data.SqlClient;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class SqlHelper
    {
        private static readonly ConcurrentDictionary<string, SqlConnection> _sqlConnections = [];

        public static SqlConnection Connect(string connectionString)
        {
            Microsoft.Data.SqlClient.SqlConnection connection = _sqlConnections.GetOrAdd(connectionString, value =>
            {
                Microsoft.Data.SqlClient.SqlConnection sqlConnection = new SqlConnection(value);

                sqlConnection.Open();

                return sqlConnection;
            });

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public static object ExecuteScalar(string query, string connectionString)
        {
            using SqlCommand sqlCommand = new()
            {
                Connection = Connect(connectionString),
                CommandText = query
            };

            return sqlCommand.ExecuteScalar();
        }

        public static object[][] ExecuteReader(string query, string connectionString)
        {
            using SqlCommand sqlCommand = new()
            {
                Connection = Connect(connectionString),
                CommandText = query
            };

            // do not dispose of the reader as this will close the connection
            Microsoft.Data.SqlClient.SqlDataReader dataReader = sqlCommand.ExecuteReader();

            using DataTable dataTable = new DataTable();

            dataTable.Load(dataReader);

            return dataTable.Rows.OfType<DataRow>().Select(x => x.ItemArray).ToArray();
        }

        public static List<T> ExecuteReaderAsList<T>(string query, string connectionString)
        {
            List<T> results = [];

            using SqlCommand sqlCommand = new(query, Connect(connectionString))
            {
                CommandTimeout = 120
            };

            // do not dispose of the reader as this will close the connection
            Microsoft.Data.SqlClient.SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            while (sqlDataReader.Read())
                if (!sqlDataReader.IsDBNull(0))
                    results.Add((T)Convert.ChangeType(sqlDataReader[0], typeof(T)));

            return results;
        }
    }
}
