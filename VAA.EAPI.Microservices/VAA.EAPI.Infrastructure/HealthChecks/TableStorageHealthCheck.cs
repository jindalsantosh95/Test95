using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class TableStorageHealthCheck : IHealthCheck
    {
        private readonly string connectionString;
        private readonly IEnumerable<string> tableNames;

        public TableStorageHealthCheck(string connectionString, IEnumerable<string> tableNames)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ArgumentNullException.ThrowIfNull(tableNames);

            this.connectionString = connectionString;
            this.tableNames = tableNames;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

                var failures = new List<string>();

                foreach (var tableName in tableNames)
                {
                    try
                    {
                        var table = tableClient.GetTableReference(tableName);
                        bool exists = await table.ExistsAsync(cancellationToken);

                        if (!exists)
                            failures.Add($"Table '{tableName}' does not exist.");
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"Table '{tableName}': {ex.Message}");
                    }
                }

                if (failures.Count == 0)
                    return HealthCheckResult.Healthy("A healthy result.");

                return new HealthCheckResult(
                    context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy,
                    "One or more Azure Tables are inaccessible.",
                    exception: null,
                    data: new Dictionary<string, object>
                    {
                    { "Failures", failures }
                });
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy,
                    "Failed to connect to Azure Table Storage.",
                    exception: ex);
            }
        }
    }
}
