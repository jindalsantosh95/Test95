using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using System;
using System.Net.WebSockets;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Client
{
    public static class CosmosGremlinClient
    {
        /// <summary>
        /// Object for locking thread
        /// </summary>
        private static readonly object clientLock = new object();
        /// <summary>
        /// GremlinClient instance
        /// </summary>
        private static GremlinClient client;
        /// <summary>
        /// Initializes Gremlin Client
        /// </summary>
        private static GremlinClient GremlinClient
        {
            get
            {
                if (client == null)
                    lock (clientLock)
                        if (client == null)
                        {
                            var containerLink = "/dbs/" + DatabaseName + "/colls/" + Container;
                            var gremlinServer = new GremlinServer(HostEndPoint, Port, EnableSSL, containerLink, Key);
                            var messageSerializer = new GraphSON2MessageSerializer(new GraphSON2Reader(), new GraphSON2Writer());
                            var connectionPoolSettings = new ConnectionPoolSettings()
                            {
                                MaxInProcessPerConnection = 10,
                                PoolSize = 30
                            };
                            var webSocketConfiguration = new Action<ClientWebSocketOptions>(options => options.KeepAliveInterval = TimeSpan.FromSeconds(10));

                            client = new GremlinClient(gremlinServer, messageSerializer, connectionPoolSettings, webSocketConfiguration);
                        }

                return client;
            }
        }

        public static string DatabaseName { get; private set; }
        public static string Container { get; private set; }
        public static string HostEndPoint { get; private set; }
        private static bool EnableSSL
        {
            get => ConfigReader.Try(Constants.Constants.CosmosGraphEnableSSL, true);
        }

        private static int Port
        {
            get => ConfigReader.Try(Constants.Constants.CosmosGraphPort, 443);
        }

        public static string Key { get; private set; }

        /// <summary>
        /// To create a CosmosClient Singleton
        /// </summary>
        /// <returns>CosmosClient</returns>
        public static GremlinClient GetGremlinClient(string database, string container, string host, string key)
        {
            DatabaseName = database;
            Container = container;
            HostEndPoint = host;
            Key = key;

            return GremlinClient;
        }
    }
}
