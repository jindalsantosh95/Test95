using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class SqlHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, SqlConnection> cache = new();

        private static SqlConnection GetOrCreateConnection(string connectionString)
        {
            connectionString = new SqlConnectionStringBuilder(connectionString).ToString();

            if (cache.ContainsKey(connectionString)) return cache[connectionString];

            var sqlConnection = new SqlConnection(connectionString);

            while (!cache.ContainsKey(connectionString) && !cache.TryAdd(connectionString, sqlConnection))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[connectionString];
        }

        private string connectionString;

        public SqlHealthCheck(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthCheckResult healthCheckResult;

            try
            {
                var connection = GetOrCreateConnection(connectionString);

                SqlCommand sqlCommand = new("SELECT 1", connection);

                connection.Open();

                if (sqlCommand.ExecuteReader().HasRows)
                    return HealthCheckResult.Healthy("SQL executed successfully.");
                else
                    return HealthCheckResult.Degraded("SQL executed successfully but no result found.");
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(connectionString), connectionString);

                healthCheckResult = new HealthCheckResult(context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy, ex.Message, ex);
            }

            return healthCheckResult;
        }
    }
}
