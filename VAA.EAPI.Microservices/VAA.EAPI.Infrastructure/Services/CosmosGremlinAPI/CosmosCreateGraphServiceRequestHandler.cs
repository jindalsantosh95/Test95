using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Client;

namespace VAA.EAPI.Infrastructure.Services.CosmosGremlinAPI
{
    public class CosmosCreateGraphServiceRequestHandler : IRequestHandler<CosmosCreateGraphServiceRequest, IDictionary<string, object>>
    {
        private readonly ILogger<CosmosCreateGraphServiceRequestHandler> _logger;
        private string gremlinTransactionId = "";

        public CosmosCreateGraphServiceRequestHandler(ILogger<CosmosCreateGraphServiceRequestHandler> logger)
        {
            _logger = logger;
        }

        public async Task<IDictionary<string, object>> Handle(CosmosCreateGraphServiceRequest request, CancellationToken cancellationToken)
        {
            IDictionary<string, object> resultList = new Dictionary<string, object>();
            gremlinTransactionId = Guid.NewGuid().ToString();

            _logger.LogInformation("Order API Associate flights || CreateGraphServiceRequestHandler ");

            try
            {
                using (var gremlinClient = CosmosGremlinClient.GetGremlinClient(request.DatabaseName,
                                                                                        request.Container,
                                                                                        request.HostEndPoint,
                                                                                        request.Key))
                {

                    foreach (var query in request.GremlinQueries)
                    {
                        var previousQuery = resultList.LastOrDefault<KeyValuePair<string, object>>();
                        if (previousQuery.Key?.StartsWith("Check") is true && previousQuery.Value?.ToString() != "0")
                        {
                            resultList.Add(query.Key, null);
                            _logger.LogInformation(String.Format("Gremlin Create Graph TransactionID {0}. Skipping this query: {1}: {2}", gremlinTransactionId, query.Key, query.Value));
                            continue;
                        }

                        _logger.LogInformation(String.Format("Gremlin Create Graph TransactionID {0}. Running this query: {1}: {2}", gremlinTransactionId, query.Key, query.Value));

                        // Create async task to execute the Gremlin query.
                        var resultSet = await SubmitRequest(gremlinClient, query);

                        if (resultSet.Count > 0)
                        {
                            _logger.LogInformation(String.Format("Gremlin Create Graph TransactionID {0}. Successfully Ran this query: {1}: {2} Query output {3}", gremlinTransactionId, query.Key, query.Value, Json.Serialize(resultSet)));

                            // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                            resultList.Add(query.Key, Json.Serialize(resultSet.First()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gremlin Transaction Id: {0} Failed with Exception: {1}", gremlinTransactionId, ex);
            }
            return resultList;
        }

        private Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, KeyValuePair<string, string> query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query.Value);
            }
            catch (ResponseException)
            {
                _logger.LogInformation(String.Format("Gremlin Create Graph TransactionID {0}. Failure query: {1}: {2} Failed Response", gremlinTransactionId, query.Key, query.Value));

                throw;
            }
        }

    }
}
