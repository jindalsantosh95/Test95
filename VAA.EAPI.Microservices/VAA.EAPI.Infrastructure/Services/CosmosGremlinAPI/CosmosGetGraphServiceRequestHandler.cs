using Gremlin.Net.Driver;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Client;

namespace VAA.EAPI.Infrastructure.Services.CosmosGremlinAPI
{
    class CosmosGetGraphServiceRequestHandler : IRequestHandler<CosmosGetGraphServiceRequest, string>
    {
        private readonly ILogger<CosmosGetGraphServiceRequestHandler> _logger;
        public CosmosGetGraphServiceRequestHandler(ILogger<CosmosGetGraphServiceRequestHandler> logger)
        {
            _logger = logger;
        }
        public async Task<string> Handle(CosmosGetGraphServiceRequest request, CancellationToken cancellationToken)
        {
            string resultResponse = string.Empty;
            _logger.LogInformation("Order API Associate flights || GetGraphServiceRequestHandler ");
            using (var gremlinClient = CosmosGremlinClient.GetGremlinClient(request.DatabaseName,
                                                                         request.Container,
                                                                         request.HostEndPoint,
                                                                         request.Key))
            {
                _logger.LogInformation(String.Format("Running this query: {0}", request.GraphQuery));

                // Create async task to execute the Gremlin query.
                var resultSet = await SubmitRequest(gremlinClient, request.GraphQuery);
                if (resultSet.Count > 0)
                {
                    // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                    resultResponse = Json.Serialize(resultSet.First());
                }
                return resultResponse;
            }
        }

        private static async Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, string query)
        {
            return await gremlinClient.SubmitAsync<dynamic>(query);
        }

    }
}
