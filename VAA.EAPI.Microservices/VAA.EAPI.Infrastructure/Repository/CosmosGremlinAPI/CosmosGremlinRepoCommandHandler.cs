using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Entities.Enum;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services.CosmosGremlinAPI;

namespace VAA.EAPI.Infrastructure.Repository.CosmosGremlinAPI
{
    public class CosmosGremlinRepoCommandHandler : IRequestHandler<CosmosGremlinRepoCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CosmosGremlinRepoCommandHandler> _logger;

        public CosmosGremlinRepoCommandHandler(IMediator mediator, ILogger<CosmosGremlinRepoCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(CosmosGremlinRepoCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order API Associate flights || GremlinRepoCommandHandler ");
            try
            {
                string database = ConfigReader.Get(Constants.Constants.CosmosGraphDatabaseName);
                string host = ConfigReader.Get(Constants.Constants.CosmosGraphHostEndPoint);
                string key = ConfigReader.Get(Constants.Constants.CosmosGraphKey);
                string container = ConfigReader.Get(Constants.Constants.CosmosGraphDBContainer);
                IDictionary<string, string> gremlinQueries = new Dictionary<string, string>();

                if (command.GraphService == "CreateGraph")
                {

                    CosmosCreateGraphServiceRequest cosmosCreateGraphServiceRequest = new CosmosCreateGraphServiceRequest()
                    {
                        DatabaseName = database,
                        Container = container,
                        HostEndPoint = host,
                        Key = key
                    };

                    foreach (var traversalItem in command.GraphTraversalItems)
                    {
                        GraphOperation graphOperation;
                        if (traversalItem.Value.Type == "vertex")
                        {
                            //Check if Vertex exists                           
                            graphOperation = GraphOperation.CheckVertex;
                            gremlinQueries.Add("Check " + traversalItem.Key.Substring(14), GremlinQueryBuilder.GetGremlinQuery(traversalItem.Value, graphOperation));

                            //Create Vertex Query
                            graphOperation = GraphOperation.CreateVertex;
                            gremlinQueries.Add(traversalItem.Key, GremlinQueryBuilder.GetGremlinQuery(traversalItem.Value, graphOperation));

                        }
                        else if (traversalItem.Value.Type == "edge")
                        {
                            //Check if Edge exists                            
                            if (!traversalItem.Key.Contains("Reverse"))
                                graphOperation = GraphOperation.CheckEdge;
                            else
                                graphOperation = GraphOperation.CheckEdgeReverse;
                            gremlinQueries.Add("Check " + traversalItem.Key.Substring(7), GremlinQueryBuilder.GetGremlinQuery(traversalItem.Value, graphOperation));

                            //Create Edge Query
                            if (!traversalItem.Key.Contains("Reverse"))
                                graphOperation = GraphOperation.CreateEdge;
                            else
                                graphOperation = GraphOperation.CreateEdgeReverse;
                            gremlinQueries.Add(traversalItem.Key, GremlinQueryBuilder.GetGremlinQuery(traversalItem.Value, graphOperation));
                        }

                    }
                    cosmosCreateGraphServiceRequest.GremlinQueries = gremlinQueries;

                    StringBuilder sb = new StringBuilder();
                    foreach (var query in gremlinQueries)
                        sb.Append(query.Key + "-" + query.Value + "\n");
                    var stquery = sb.ToString();
                    _logger.LogInformation($"Gremlin quries : {Json.Serialize(gremlinQueries)}");

                    var response = await _mediator.Send(cosmosCreateGraphServiceRequest, cancellationToken);

                    if (gremlinQueries.Count == response.Count)
                        response.Add("Create Graph Result", "Cosmos Graph Created Successfully");
                    else
                        response.Add("Create Graph Result", "Cosmos Graph Not Created");
                    return new Response<string>() { Status = Status.OK, Value = $"Gremlin quries : {Json.Serialize(gremlinQueries)}, Query Result : {Json.Serialize(response)}" };

                }
                else if (command.GraphService == "ReadGraph")
                {
                    CosmosGetGraphServiceRequest cosmosGetGraphServiceRequest = new CosmosGetGraphServiceRequest()
                    {
                        DatabaseName = database,
                        Container = container,
                        HostEndPoint = host,
                        Key = key
                    };

                    foreach (var traversalItem in command.GraphTraversalItems)
                    {
                        cosmosGetGraphServiceRequest.GraphQuery = GremlinQueryBuilder.GetCustomerFlightsGraphQuery(traversalItem.Value);
                    }

                    var response = await _mediator.Send(cosmosGetGraphServiceRequest, cancellationToken);
                    if (response == "")
                        return new Response<string>() { Status = Status.BusinessException, Value = "Error Occured while getting graph structure. Please check logs" };
                    else
                        return new Response<string>() { Status = Status.OK, Value = response };
                }
                else
                    throw new NotImplementedException("Graph Service Not Supported");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<string>() { Status = Status.Error, Value = "Error Occured while accessing graph. Please check logs" };
            }

        }
    }
}
