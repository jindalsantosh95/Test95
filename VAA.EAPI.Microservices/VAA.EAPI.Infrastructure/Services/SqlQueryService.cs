using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Infrastructure.Services
{
    public sealed class SqlQueryService(ISqlConnectionFactory sqlConnectionFactory) : ISqlQueryService
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));

        public async Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string query, object parameters, CommandType commandType, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);

            await using var connection = _sqlConnectionFactory.CreateConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.QueryAsync<T>(new CommandDefinition(
                query,
                parameters,
                commandType: commandType,
                cancellationToken: cancellationToken));
        }
    }
}
