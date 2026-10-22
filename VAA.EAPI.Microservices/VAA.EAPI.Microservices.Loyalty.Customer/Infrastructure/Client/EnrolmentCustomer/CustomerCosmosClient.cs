using Microsoft.Azure.Cosmos;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Client.EnrolmentCustomer
{
    public class CustomerCosmosClient
    {
        private static readonly string _endpoint = ConfigReader.Get(Environment.SCVCustDBEndPoint);
        private static readonly string _authKey = ConfigReader.Get(Environment.SCVCustDBAuthKey);
        private static readonly object _clientLock = new object();
        private static CosmosClient _client;
        private CosmosClient CosmosClient
        {
            get
            {
                if (_client == null)
                {
                    lock (_clientLock)
                    {
                        if (_client == null)
                        {
                            if (ConfigReader.Get(Environment.CosmosConnectionMode).ToUpper() == "DIRECT")
                            {
                                _client = new CosmosClient(_endpoint, _authKey, new CosmosClientOptions()
                                {
                                    ConnectionMode = ConnectionMode.Direct
                                });
                            }
                            else
                            {
                                _client = new CosmosClient(_endpoint, _authKey, new CosmosClientOptions()
                                {
                                    ConnectionMode = ConnectionMode.Gateway
                                });
                            }
                        }
                    }
                }

                return _client;
            }
        }
        public CosmosClient GetCustomerDBCosmosClient()
        {
            return CosmosClient;
        }
    }
}
