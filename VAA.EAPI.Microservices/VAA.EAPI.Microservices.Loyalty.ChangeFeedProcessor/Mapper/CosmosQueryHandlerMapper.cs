using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Mapper
{
    public class CosmosQueryHandlerMapper
    {
        public static CosmosReaderServiceQuery Mapper(string collectionName, string dataBaseEndPoint, string dataBaseName, List<KeyValuePair<string, object>> parameters, string sqlQuery)
        {
            return new CosmosReaderServiceQuery()
            {
                CollectionName = collectionName,
                DatabaseEndpoint = dataBaseEndPoint,
                DatabaseName = dataBaseName,
                QueryParams = parameters,
                SqlQuery = sqlQuery
            };
        }

    }
}
