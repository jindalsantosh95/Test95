using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2
{
    public sealed class CosmosClientFactory : ICosmosClientFactory
    {
        public static ConnectionMode ConnectionMode => ConfigReader.Try(Constants.Constants.CosmosSqlDBConnectionMode).Parse(ConnectionMode.Gateway);

        private static readonly ConcurrentDictionary<string, CosmosClient> clientEndPoints = new();

        public CosmosClient GetClient(string endpoint, CosmosClientOptions options = null)
        {
            if (clientEndPoints.ContainsKey(endpoint)) return clientEndPoints[endpoint];

            clientEndPoints.TryAdd(endpoint, new CosmosClient(endpoint, new DefaultAzureCredential(), options ?? GetDefaultCosmosClientOptions()));

            return clientEndPoints[endpoint];
        }

        public CosmosClientOptions GetDefaultCosmosClientOptions() => new() { ConnectionMode = ConnectionMode, SerializerOptions = new CosmosSerializationOptions { IgnoreNullValues = true } };
    }
}
