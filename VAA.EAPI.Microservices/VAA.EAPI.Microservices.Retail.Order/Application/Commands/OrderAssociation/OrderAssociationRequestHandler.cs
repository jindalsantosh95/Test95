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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Exceptions.CustomExceptions.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation
{
    public class OrderAssociationRequestHandler : IRequestHandler<OrderAssociationRequest, Result<Response<List<FlightOrders>>>>
    {
        private readonly IMediator _mediator;
        private readonly ICachedAirport _cachedAirport;
        private readonly ICachedCarrier _cachedCarrier;
        private readonly ILogger<OrderAssociationRequestHandler> _logger;
        private readonly IValidatePayloadAssociateFlights _validatePayload;
        private readonly StringBuilder _logText = new StringBuilder();

        public OrderAssociationRequestHandler(IMediator mediator
            , ICachedAirport cachedAirport
            , ICachedCarrier cachedCarrier
            , IValidatePayloadAssociateFlights validatePayload
            , ILogger<OrderAssociationRequestHandler> logger)
        {
            _mediator = mediator;
            _cachedAirport = cachedAirport;
            _cachedCarrier = cachedCarrier;
            _validatePayload = validatePayload;
            _logger = logger;
        }

        public async Task<Result<Response<List<FlightOrders>>>> Handle(OrderAssociationRequest request, CancellationToken cancellationToken)
        {
            _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler");
            Result<Response<List<FlightOrders>>> flightAssociationResponse = new();
            string guid = Guid.NewGuid().ToString();

            var validationErrors = _validatePayload.ValidateOrderAssociationRequestCommand(request, guid);
            if (validationErrors.Count > 0)
            {
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler || ErrorId {guid} || Validation Error");
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
                string databaseEndpoint = ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseEndPoint);
                string databaseName = ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseName);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Fetching flight data from SCV FlightsCollection- Start");
                var scvPNRResults = await GetOrderId(request.PNRNumber, databaseEndpoint, databaseName);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Fetching flight data from SCV FlightsCollection- End");
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Flights data fetched from SCV FlightsCollection {scvPNRResults}");
                if (string.IsNullOrEmpty(scvPNRResults))
                {
                    throw new InflightOrdersException(ErrorCodes.NoFlightdataException);

                }
                var pnrData = Json.Deserialize<List<FlightCollectionResult>>(scvPNRResults);

                List<OrderCollectionResult> orderCollectionResults = new();
                List<OrderCollectionResult> orderCollectionData = new();
                var orderIDList = pnrData.Select(x => x.PNRReference).Distinct().ToArray();

                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Fetching flight data from SCV OrdersCollections- Start");
                string sqlQuery = Constants.Application.OrdersCollectionSqlQuery;
                List<KeyValuePair<string, object>> queryParams = new();
                queryParams.Add(new KeyValuePair<string, object>("OrderIDList", orderIDList));
                queryParams.Add(new KeyValuePair<string, object>("firstName", request.FirstName));
                queryParams.Add(new KeyValuePair<string, object>("lastName", request.LastName));

                CosmosReaderRepoQuery cosmosReaderRepoCommand = CosmosReaderCommandMapper.MapToCosmosReaderCommandRequest(CollectionTypes.OrdersCollection, sqlQuery, queryParams, databaseEndpoint, databaseName);

                List<object> items = await _mediator.Send(cosmosReaderRepoCommand);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Fetching flight data from SCV OrdersCollections- End");
                items.ForEach(x =>
                {
                    orderCollectionResults.Add(x as OrderCollectionResult);
                });
                if (orderCollectionResults.Count() > 0)
                {
                    _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Total {orderCollectionResults.Count} flights found in SCV OrdersCollection");
                    orderCollectionData.AddRange(orderCollectionResults);
                }
                var airportData = _cachedAirport.GetCachedAirport();
                var carrierData = _cachedCarrier.GetCachedCarrier();
                var orderViewResponse = ScvResponseMapper.MapScvOrderResponse(orderCollectionData, airportData, carrierData);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Response Mapped successfully from SCV to Order association API");
                OrderAssociationEventCommand orderAssociationEvent = new()
                {
                    cloudEvent = new EAPI.Domain.Events.CloudEvent()
                    {

                        Id = Guid.NewGuid().ToString(),
                        Source = Constants.Application.OrderAssocaitionEventHandlerTopicName, //"orderassocaitionrequesteventmessage",
                        Subject = Constants.Application.OrderAssociationEventSubject,
                        Time = DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat),
                        Type = "OrderAssociation",
                        Data = new OrderAssociationRequestData()
                        {
                            CustomerId = request.CustomerId,
                            Orders = orderCollectionData
                        }
                    }
                };
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Message to be sent in Servive Bus Topic {Json.Serialize(orderAssociationEvent)}");
                string scvString = Json.Serialize(orderAssociationEvent.cloudEvent.Data);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Publishing event in to Azure ServiceBus Topic");
                var commandOutput = await _mediator.Send(orderAssociationEvent);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | Flights association message sent to Topic: {Constants.Application.OrderAssocaitionEventHandlerTopicName}");

                if (orderViewResponse != null && orderViewResponse.Count > 0)
                {
                    flightAssociationResponse = Result.Success(new Response<List<FlightOrders>>
                    {
                        Value = orderViewResponse,
                        Status = Status.OK,
                        IgnoreNullValues = false
                    });
                }
                else
                {
                    _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler | No flights associated with the customer");
                    flightAssociationResponse = Result.Success(new Response<List<FlightOrders>>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoflightsAssociationException, guid, Constants.Application.InflightOrdersExceptionFilePath),
                        Status = Status.BusinessException,
                        IgnoreNullValues = false
                    });
                }
                if (commandOutput.Value != null)
                {
                    flightAssociationResponse.Value.HeaderParameters = commandOutput.Value.HeaderParameters;
                }
            }
            catch (InflightOrdersException iex)
            {
                var errorResponse = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(iex.Message), guid, Constants.Application.InflightOrdersExceptionFilePath);
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler || ErrorId {guid} || Business exception from Inflight Orders Association");
                flightAssociationResponse = Result.Success(new Response<List<FlightOrders>>
                {
                    Errors = errorResponse,
                    Status = Status.BusinessException,
                    IgnoreNullValues = false
                });
            }
            catch (Exception ex)
            {
                _logText.AppendLine($"{DateTime.UtcNow.ToString(Constants.Application._logApplicationInsightTimeFormat)} Customer Orders Association Application handler || ErrorId {guid} ||  Exception: {ex.Message} StackTrace : {ex.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, guid, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                flightAssociationResponse = Result.Failure<Response<List<FlightOrders>>>(Json.Serialize(
                    instance: errorResponse, ignoreNullValues: false));
            }
            _logger.LogInformation(_logText.ToString());
            return flightAssociationResponse;
        }

        private async Task<string> GetOrderId(string pnrNumber, string databaseEndpoint, string databaseName)
        {
            string sqlQuery = Constants.Application.FlightCollectionSqlQuery;
            List<KeyValuePair<string, object>> queryParams = new()
            {
                new KeyValuePair<string, object>("PNRNumber", pnrNumber),
                new KeyValuePair<string, object>("DocType", "FlightOrders")
            };
            CosmosReaderRepoQuery cosmosReaderRepoCommand = CosmosReaderCommandMapper.MapToCosmosReaderCommandRequest(CollectionTypes.FlightCollection, sqlQuery, queryParams, databaseEndpoint, databaseName);
            List<object> items = await _mediator.Send(cosmosReaderRepoCommand);
            return Json.DeserializeObject<string>((JToken)items.FirstOrDefault());

        }
    }
}
