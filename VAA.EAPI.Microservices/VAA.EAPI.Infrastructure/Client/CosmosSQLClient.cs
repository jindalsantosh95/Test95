using Azure.Identity;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Client
{
    public class CosmosSQLClient : ICosmosSQLClient
    {
        public CosmosClient GetCosmosClient(string cosmosEndPoint)
        {
            var cosmosClient = new CosmosClient(cosmosEndPoint, new DefaultAzureCredential(), new CosmosClientOptions
            {

                ConnectionMode = ConfigReader.Get("CosmosSqlDBConnectionMode").ToUpper() == "DIRECT" ? ConnectionMode.Direct : ConnectionMode.Gateway
            });
            return cosmosClient;
        }
    }
}
