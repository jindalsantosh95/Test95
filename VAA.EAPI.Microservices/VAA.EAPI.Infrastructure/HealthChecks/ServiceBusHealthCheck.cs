using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class ServiceBusHealthCheck : IHealthCheck
    {
        private static ServiceBusClientOptions DefaultServiceBusClientOptions = new()
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        };

        private static readonly ConcurrentDictionary<string, ServiceBusClient> cache = new();

        private static ServiceBusClient GetOrCreateClient(string connectionString, ServiceBusClientOptions serviceBusClientOptions = null)
        {
            if (cache.ContainsKey(connectionString)) return cache[connectionString];

            var serviceBusClient = new ServiceBusClient(connectionString, serviceBusClientOptions ?? DefaultServiceBusClientOptions);

            while (!cache.ContainsKey(connectionString) && !cache.TryAdd(connectionString, serviceBusClient))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[connectionString];
        }

        private string connectionString;

        public ServiceBusHealthCheck(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthCheckResult healthCheckResult;

            try
            {
                await using var serviceBusClient = new ServiceBusClient(connectionString);

                return HealthCheckResult.Healthy("A healthy result.");
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
