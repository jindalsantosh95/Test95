using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate
{
    public class CosmosReaderServiceQueryMapper
    {
        public static CosmosReaderServiceQuery Mapper(string query, List<KeyValuePair<string, object>> keyValues)
        {
            return new CosmosReaderServiceQuery()
            {
                CollectionName = ConfigReader.Get(Environment.OrderCollection),
                DatabaseEndpoint = ConfigReader.Get(Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Environment.DatabaseName),
                QueryParams = keyValues,
                SqlQuery = query
            };
        }
    }
}
