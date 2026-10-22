using Azure.Identity;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Common.Helpers;
using ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode;

namespace VAA.EAPI.Infrastructure.Client
{
    /// <summary>
    /// Azure Cosmos Client Singleton
    /// </summary>
    public static class CosmosSqlDBClient
    {
        /// <summary>
        /// Cosmos Db endpoint configuration
        /// </summary>
        private static string endpoint;
        /// <summary>
        /// Object for locking thread
        /// </summary>
        private static readonly object clientLock = new object();
        /// <summary>
        /// CosmosClient instance
        /// </summary>
        private static CosmosClient client;
        /// <summary>
        /// Initializes Cosmos Client
        /// </summary>
        private static CosmosClient CosmosClient
        {
            get
            {
                if (client == null)
                {
                    lock (clientLock)
                    {
                        if (client == null)
                        {
                            if (ConfigReader.Get("CosmosSqlDBConnectionMode").ToUpper() == "DIRECT")
                            {
                                client = new CosmosClient(endpoint, new DefaultAzureCredential(), new CosmosClientOptions
                                {
                                    ConnectionMode = ConnectionMode.Direct
                                });
                            }
                            else
                            {
                                client = new Microsoft.Azure.Cosmos.CosmosClient(endpoint, new DefaultAzureCredential(), new CosmosClientOptions
                                {
                                    ConnectionMode = ConnectionMode.Gateway
                                });
                            }
                        }
                    }
                }
                return client;
            }
        }
        /// <summary>
        /// To create a CosmosClient Singleton
        /// </summary>
        /// <returns>CosmosClient</returns>
        public static CosmosClient GetCosmosDBClient(string cosmosDbEndPoint)
        {
            endpoint = cosmosDbEndPoint;
            return CosmosClient;
        }
    }
}