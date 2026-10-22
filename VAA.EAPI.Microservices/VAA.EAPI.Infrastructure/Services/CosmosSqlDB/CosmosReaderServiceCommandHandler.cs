using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDB
{
    [Obsolete("Use VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2")]
    public class CosmosReaderServiceCommandHandler : IRequestHandler<CosmosReaderServiceCommand, IEnumerable<object>>
    {
        private readonly ILogger<CosmosReaderServiceCommandHandler> _logger;
        private readonly CosmosClient _cosmosSQLClient;

        public CosmosReaderServiceCommandHandler(CosmosClient cosmosSQLClient, ILogger<CosmosReaderServiceCommandHandler> logger)
        {
            _cosmosSQLClient = cosmosSQLClient ?? throw new ArgumentNullException(nameof(cosmosSQLClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<object>> Handle(CosmosReaderServiceCommand cosmosReaderServiceCommand, CancellationToken cancellationToken)
        {
            //CosmosClient client = CosmosSqlDBClient.GetCosmosDBClient(cosmosReaderServiceCommand.DatabaseEndpoint);
            //CosmosClient client = _cosmosSQLClient.GetCosmosClient(cosmosReaderServiceCommand.DatabaseEndpoint);
            FeedResponse<object> currentResultSet = null;
            string databaseId = cosmosReaderServiceCommand.DatabaseName;
            string collectionId = cosmosReaderServiceCommand.CollectionName;
            string sqlQueryText = cosmosReaderServiceCommand.SqlQuery;
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);


            cosmosReaderServiceCommand.QueryParams.ForEach(p =>
            {
                queryDefinition.WithParameter($"@{p.Key}", p.Value);
            });

            FeedIterator<object> queryResultSetIterator = _cosmosSQLClient.GetContainer(databaseId, collectionId).GetItemQueryIterator<object>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                try
                {
                    currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    if (currentResultSet.Resource.Any())
                    {
                        _logger.LogInformation("Data fetched successfully from Cosmos DB");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Data fetching error from Cosmos DB {ex.Message}");
                    throw;
                }
            }

            return currentResultSet;
        }
    }
}
