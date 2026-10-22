using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Domain.Models.CosmosGraph;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosGremlinAPI;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults;
using VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.InflightFetchOrders
{
    public class OrderSearchCommandHandler : IRequestHandler<OrderSearchCommand, Result<Response<List<FlightOrders>>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSearchCommandHandler> _logger;
        private readonly ICachedAirport _cachedAirport;
        private readonly ICachedCarrier _cachedCarrier;
        private readonly IValidatePayload _validatePayload;
        private readonly StringBuilder _logText = new StringBuilder();

        public OrderSearchCommandHandler(IMediator mediator
            , ICachedAirport cachedAirport
            , ICachedCarrier cachedCarrier
            , IValidatePayload validatePayload
            , ILogger<OrderSearchCommandHandler> logger)
        {
            _mediator = mediator;
            _cachedAirport = cachedAirport;
            _cachedCarrier = cachedCarrier;
            _validatePayload = validatePayload;
            _logger = logger;
        }

        public async Task<Result<Response<List<FlightOrders>>>> Handle(OrderSearchCommand request, CancellationToken cancellationToken)
        {
            _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler");
            Result<Response<List<FlightOrders>>> flightSearchResponse = new Result<Response<List<FlightOrders>>>();
            string guid = Guid.NewGuid().ToString();
            var validationErrors = _validatePayload.ValidateSearchOrderCommand(orderSearchCommand: request, guid);
            if (validationErrors.Count > 0)
            {
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler || ErrorId {guid} || Validation Error");
                _logger.LogInformation(_logText.ToString());
                return Result.Success(new Response<List<FlightOrders>>
                {
                    Errors = validationErrors,
                    Status = Status.BadRequest,
                    IgnoreNullValues = false
                });
            }

            try
            {
                var graphResponse = await _mediator.Send(new B2CGraphRepoQuery(request?.CustomerId), cancellationToken);
                string loyaltyNumber = ExtractFlyingClubNumber(graphResponse);
                bool isCustomerTypeLoyalty = !string.IsNullOrEmpty(loyaltyNumber);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Is Customer Type Loyalty: {isCustomerTypeLoyalty}");



                int upperBound = Convert.ToInt32(request.FutureDays);
                int lowerBound = -Convert.ToInt32(request.PastDays);
                List<FlightOrders> orderViewResponse = new();
                if (isCustomerTypeLoyalty)
                {
                    _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)}Customer Orders Get Flights Application handler | Fetchin flight info for Loyalty Customer from SCV OrdersCollection");
                    string sqlQuery = Constants.Application.OrdersCollQueryLoyaltyCustomers;
                    List<KeyValuePair<string, object>> queryParams = new();
                    queryParams.Add(new KeyValuePair<string, object>("LoyaltyNumber", loyaltyNumber));
                    string databaseEndpoint = ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseEndPoint);
                    string databaseName = ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseName);
                    var cosmosReaderRepoCommand = CosmosReaderCommandMapper.MapToCosmosReaderCommandRequest(CollectionTypes.OrdersCollection, sqlQuery, queryParams, databaseEndpoint, databaseName);
                    string scvData = Json.DeserializeObject<string>((JToken)(await _mediator.Send(cosmosReaderRepoCommand, cancellationToken)).FirstOrDefault());
                    if (!string.IsNullOrEmpty(scvData))
                    {
                        var scvOrdersCollectionRecords = Json.Deserialize<List<OrderCollectionResult>>(scvData);
                        if (scvOrdersCollectionRecords.Count > 0)
                        {
                            _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler | Total {scvOrdersCollectionRecords.Count} flights found from SCV");
                            var airportData = _cachedAirport.GetCachedAirport();
                            var carrierData = _cachedCarrier.GetCachedCarrier();
                            orderViewResponse = ScvResponseMapper.MapScvOrderResponse(scvOrdersCollectionRecords, upperBound, lowerBound, airportData, carrierData);
                            _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler | Total {orderViewResponse.Count} flights mapped from SCV to Retail Ordert API response");
                        }
                    }
                }
                if (ConfigReader.Get(Constants.Environment.PNRValidationFeatureFlag) == "Y")
                {

                    /// Read Cosmos Graph DB, if customer is loyalty member add to orderViewResponse
                    Customer customer = new() { CustomerId = request.CustomerId };
                    CosmosGremlinRepoCommand cmdRepo = new()
                    {
                        GraphService = "ReadGraph",
                        GraphTraversalItems = new Dictionary<string, GraphObject>()
                    };
                    cmdRepo.GraphTraversalItems.Add("Get Customer Flights", customer);

                    Response<string> orderGraphResponse = new();
                    _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler | Getting flight info for Customer from SCVplus Graph DB");
                    orderGraphResponse = await _mediator.Send(cmdRepo, cancellationToken);
                    var orderGraphResponseFlightDetails = CosmosGraphResultToOrderViewMapper.MapCosmosGraphResultToOrderView(orderGraphResponse.Value, upperBound, lowerBound);
                    _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler | Total {orderGraphResponseFlightDetails.Count} flights found from SCVplus Graph DB");
                    orderViewResponse.AddRange(orderGraphResponseFlightDetails);
                }
                orderViewResponse = orderViewResponse.GroupBy(x => x.OrderId.OrderId).Distinct().Select(a => a.First()).ToList();
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Get Flights Application handler | Total {orderViewResponse.Count} flights found that satisfy retail flight window criteria i.e. exists before {lowerBound} days and after {upperBound} days");
                if (orderViewResponse.Count > 0)
                {
                    flightSearchResponse = Result.Success(new Response<List<FlightOrders>>
                    {
                        Value = orderViewResponse,
                        Status = Status.OK,
                        IgnoreNullValues = false
                    });
                }
                else
                {

                    flightSearchResponse = Result.Success(new Response<List<FlightOrders>>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoflightsAssociationException, guid, Constants.Application.InflightOrdersExceptionFilePath),
                        Status = Status.BusinessException,
                        IgnoreNullValues = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Order API Get Flights Application handler || ErrorId {guid} ||  Exception: {ex.Message} StackTrace : {ex.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, guid, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                flightSearchResponse = Result.Failure<Response<List<FlightOrders>>>(Json.Serialize(instance: errorResponse, ignoreNullValues: false));
            }
            _logger.LogInformation(_logText.ToString());
            return flightSearchResponse;
        }

        private static string ExtractFlyingClubNumber(B2CGraphResponse graphResponse)
        {
            if (graphResponse != null && graphResponse.Value?.Count > 0)
            {
                return graphResponse.Value.FirstOrDefault()?.SignInNames?.FirstOrDefault(s => s?.Type == "username")?.Value?.ToString();
            }
            return string.Empty;
        }
    }
}
