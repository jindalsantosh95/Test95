using Microsoft.Azure.Cosmos;

namespace VAA.EAPI.Infrastructure.Client
{
    public interface ICosmosSQLClient
    {
        public CosmosClient GetCosmosClient(string cosmosEndPoint);
    }
}
