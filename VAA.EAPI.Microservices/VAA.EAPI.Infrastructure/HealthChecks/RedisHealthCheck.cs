using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class RedisHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> cache = new();

        private static ConnectionMultiplexer GetOrCreateConnection(string connectionString)
        {
            if (cache.ContainsKey(connectionString)) return cache[connectionString];

            var connection = ConnectionMultiplexer.Connect(connectionString);

            while (!cache.ContainsKey(connectionString) && !cache.TryAdd(connectionString, connection))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[connectionString];
        }

        private readonly string connectionString;

        public RedisHealthCheck(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = GetOrCreateConnection(connectionString);
                var db = connection.GetDatabase();

                await db.PingAsync();

                return HealthCheckResult.Healthy("A healthy result.");
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(connectionString), connectionString);
                return new HealthCheckResult(context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy, ex.Message, ex);
            }
        }
    }
}
