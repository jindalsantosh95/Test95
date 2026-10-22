using Microsoft.Azure.Cosmos;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2
{
    public interface ICosmosClientFactory
    {
        CosmosClientOptions GetDefaultCosmosClientOptions();
        CosmosClient GetClient(string Endpoint, CosmosClientOptions options = null);
    }
}
