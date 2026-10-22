using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class CosmosHealthCheck : IHealthCheck
    {
        private static readonly CosmosClientOptions DefaultCosmosClientOptions = new()
        {
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = new TimeSpan(0, 30, 0),
        };

        private static readonly ConcurrentDictionary<string, CosmosClient> cache = new();

        private static CosmosClient GetOrCreateClientByConnectionString(string connectionString, CosmosClientOptions clientOptions = null) => GetOrCreateClient(new CosmosClient(connectionString, clientOptions ?? DefaultCosmosClientOptions));
        private static CosmosClient GetOrCreateClientByAccountEndpoint(string accountEndpoint, CosmosClientOptions clientOptions = null) => GetOrCreateClient(new CosmosClient(accountEndpoint, new DefaultAzureCredential(new DefaultAzureCredentialOptions()), clientOptions ?? DefaultCosmosClientOptions));
        private static CosmosClient GetOrCreateClientByAccountEndpoint(string accountEndpoint, string authKeyOrResourceToken, CosmosClientOptions clientOptions = null) => GetOrCreateClient(new CosmosClient(accountEndpoint, authKeyOrResourceToken, clientOptions ?? DefaultCosmosClientOptions));
        private static CosmosClient GetOrCreateClient(CosmosClient cosmosClient)
        {
            var accountEndpoint = cosmosClient.Endpoint.ToString().Trim('/').ToLower();

            if (cache.ContainsKey(accountEndpoint)) return cache[accountEndpoint];

            while (!cache.ContainsKey(accountEndpoint) && !cache.TryAdd(accountEndpoint, cosmosClient))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[accountEndpoint];
        }

        private readonly string connectionString;
        private readonly string accountEndpoint;
        private readonly string authKeyOrResourceToken;
        private readonly string databaseId;
        private readonly string containerId;

        private CosmosHealthCheck(string databaseId, string containerId)
        {
            if (databaseId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(databaseId));
            if (containerId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerId));

            this.databaseId = databaseId;
            this.containerId = containerId;
        }
        public CosmosHealthCheck(string connectionStringOrAccountEndpoint, string databaseId, string containerId) : this(databaseId, containerId)
        {
            if (connectionStringOrAccountEndpoint.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionStringOrAccountEndpoint));

            if (Uri.IsWellFormedUriString(connectionStringOrAccountEndpoint, UriKind.Absolute))
            {
                if (!connectionStringOrAccountEndpoint.ToLower().Contains("http"))
                    connectionStringOrAccountEndpoint = $"https://{connectionStringOrAccountEndpoint}";

                this.accountEndpoint = connectionStringOrAccountEndpoint;
            }
            else
                this.connectionString = connectionStringOrAccountEndpoint;
        }
        public CosmosHealthCheck(string connectionStringOrAccountEndpoint, string databaseId, string containerId, string authKeyOrResourceToken) : this(connectionStringOrAccountEndpoint, databaseId, containerId)
        {
            if (string.IsNullOrWhiteSpace(authKeyOrResourceToken)) throw new ArgumentNullException(nameof(authKeyOrResourceToken));

            this.authKeyOrResourceToken = authKeyOrResourceToken;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthCheckResult healthCheckResult;

            try
            {
                var useConnectionString = !string.IsNullOrWhiteSpace(connectionString);
                var useEndpoint = !string.IsNullOrWhiteSpace(accountEndpoint) && Uri.IsWellFormedUriString(accountEndpoint, UriKind.Absolute);
                var useAuthKeyOrResourceToken = !string.IsNullOrWhiteSpace(authKeyOrResourceToken);

                var client = useConnectionString ? GetOrCreateClientByConnectionString(connectionString) : useEndpoint && useAuthKeyOrResourceToken ? GetOrCreateClientByAccountEndpoint(accountEndpoint, authKeyOrResourceToken) : GetOrCreateClientByAccountEndpoint(accountEndpoint);

                var readContainer = client.GetContainer(databaseId, containerId);

                var query = $"SELECT TOP 1 * FROM {containerId} c";

                using (FeedIterator feedIterator = readContainer.GetItemQueryStreamIterator(new QueryDefinition(query)))
                    while (feedIterator.HasMoreResults)
                        await feedIterator.ReadNextAsync(cancellationToken); // we need to call as we want to test the connectivity of the cosmos db.

                return HealthCheckResult.Healthy("A healthy result.");
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(accountEndpoint), accountEndpoint);
                ex.Data.Add(nameof(databaseId), databaseId);
                ex.Data.Add(nameof(containerId), containerId);

                healthCheckResult = new HealthCheckResult(context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy, ex.Message, ex);
            }

            return healthCheckResult;
        }
    }
}
