using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2
{
    public class CosmosReaderServiceQuery : CosmosServiceBase, IRequest<List<object>>
    {
        public CosmosReaderServiceQuery()
        {
        }
        public CosmosReaderServiceQuery(string collectionName, string dataBaseEndPoint, string dataBaseName, List<KeyValuePair<string, object>> parameters, string sqlQuery)
        {
            CollectionName = collectionName;
            DatabaseEndpoint = dataBaseEndPoint;
            DatabaseName = dataBaseName;
            QueryParams = parameters;
            SqlQuery = sqlQuery;
        }

        public string SqlQuery { get; set; }
        public List<KeyValuePair<string, object>> QueryParams { get; set; }
    }
    public class CosmosReaderServiceQueryHandler : IRequestHandler<CosmosReaderServiceQuery, List<object>>
    {
        private readonly ILogger<CosmosReaderServiceQueryHandler> _logger;
        private readonly ICosmosClientFactory _cosmosFactory;

        public CosmosReaderServiceQueryHandler(ILogger<CosmosReaderServiceQueryHandler> logger, ICosmosClientFactory cosmosFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosFactory = cosmosFactory ?? throw new ArgumentNullException(nameof(cosmosFactory));
        }
        public async Task<List<object>> Handle(CosmosReaderServiceQuery cosmosReaderServiceCommand, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            try
            {
                List<object> docs = new();

                CosmosClientOptions clientOptions = _cosmosFactory.GetDefaultCosmosClientOptions();

                CosmosClient cosmosClient = _cosmosFactory.GetClient(cosmosReaderServiceCommand.DatabaseEndpoint, clientOptions);

                QueryDefinition queryDefinition = new(cosmosReaderServiceCommand.SqlQuery);

                foreach (var p in cosmosReaderServiceCommand.QueryParams)
                    queryDefinition.WithParameter(p.Key.StartsWith('@') ? p.Key : $"@{p.Key}", p.Value);

                QueryRequestOptions queryRequestOptions = AsyncContext.Current.DiagnosticsEnabled ? new() { PopulateIndexMetrics = true } : default;

                FeedIterator<object> queryResultSetIterator = cosmosClient.GetContainer(cosmosReaderServiceCommand.DatabaseName, cosmosReaderServiceCommand.CollectionName).GetItemQueryIterator<object>(queryDefinition, null, queryRequestOptions);

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync(cancellationToken);

                    trace.Diagnostics.LogInformation($"{{0}}> IndexMetrics: {currentResultSet.IndexMetrics?.ToString().MakeInterpolationSafe()}");
                    trace.Diagnostics.LogInformation($"{{0}}> Diagnostics: {currentResultSet.Diagnostics?.ToString().MakeInterpolationSafe()}");

                    if (currentResultSet.Resource.Any())
                        docs.AddRange(currentResultSet);
                }

                return docs;
            }
            catch (CosmosException ex)
            {
                trace.Log.LogError(ex, $"{{0}}> {ex.GetType().Name}: {ex.Message.MakeInterpolationSafe()}");

                trace.Diagnostics.LogInformation($"{{0}}> Diagnostics: {ex.Diagnostics?.ToString().MakeInterpolationSafe()}");

                throw;
            }
            catch (Exception ex)
            {
                trace.Log.LogError(ex, $"{{0}}> {ex.GetType().Name}: {ex.Message.MakeInterpolationSafe()}");

                throw;
            }
        }
    }
}
