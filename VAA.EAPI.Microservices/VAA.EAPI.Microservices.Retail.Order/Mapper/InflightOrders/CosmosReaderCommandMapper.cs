using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders
{
    public class CosmosReaderCommandMapper
    {
        public static CosmosReaderRepoQuery MapToCosmosReaderCommandRequest(CollectionTypes collectionType, string sqlQuery, List<KeyValuePair<string, object>> queryParams, string databaseEndpoint, string databaseName)
        {
            string collectionName = string.Empty;

            switch (collectionType)
            {
                case CollectionTypes.WorkingProfile:
                    collectionName = ConfigReader.Get(Constants.Environment.WorkingProfileCollection);
                    break;
                case CollectionTypes.CustomerProfile:
                    collectionName = ConfigReader.Get(Constants.Environment.CustomerProfileCollection);
                    break;
                case CollectionTypes.OrdersCollection:
                    collectionName = ConfigReader.Get(Constants.Environment.OrdersCollectionName);
                    break;
                case CollectionTypes.FlightCollection:
                    collectionName = ConfigReader.Get(Constants.Environment.FlightCollectionCollection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new CosmosReaderRepoQuery()
            {
                CollectionName = collectionName,
                DatabaseEndpoint = databaseEndpoint,
                DatabaseName = databaseName,
                QueryParams = new List<KeyValuePair<string, object>>(queryParams),
                SqlQuery = sqlQuery
            };
        }
    }
}
