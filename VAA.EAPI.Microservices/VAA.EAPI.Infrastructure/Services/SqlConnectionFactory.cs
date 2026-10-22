using Microsoft.Data.SqlClient;
using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Infrastructure.Services
{
    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        public SqlConnection CreateConnection(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            var connection = new SqlConnection(connectionString);
            SqlConnectionHelper.ConfigureManagedIdentitySqlConnection(connection);

            return connection;
        }
    }
}
