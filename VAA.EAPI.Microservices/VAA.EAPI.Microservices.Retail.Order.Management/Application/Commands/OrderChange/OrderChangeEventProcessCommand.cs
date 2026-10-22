using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class OrderChangeEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {

    }
    public class OrderChangeEventSubscribeCommandHandler : IRequestHandler<OrderChangeEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeEventSubscribeCommandHandler> _logger;
        private readonly TableStorageService<ReferenceDataEntity> _tableStorageService;
        private readonly ICachedChannel _cachedChannel;
        private readonly IOfferSearchItems _OfferSearchItems;

        public OrderChangeEventSubscribeCommandHandler(IMediator mediator, ILogger<OrderChangeEventSubscribeCommandHandler> logger, ITableStorageService<ReferenceDataEntity> tableStorageService, ICachedChannel cachedChannel, IOfferSearchItems offerSearchItems)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = (TableStorageService<ReferenceDataEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReferenceDataTableName);
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
            _OfferSearchItems = offerSearchItems ?? throw new ArgumentNullException(nameof(offerSearchItems));
        }
        public async Task<Result<Response<string>>> Handle(OrderChangeEventProcessCommand request, CancellationToken cancellationToken)
        {
            Response<string> orderChangeResponse = new();
            List<string> repriceResponse = new();
            try
            {
                var orderChangeRequestData = Json.Deserialize<OrderChangeRequestData>(Convert.ToString(request.Data));
                var offerId = orderChangeRequestData.OrderAmendments.First().OrderByReference?.First().ItemsForAddition.OfferRefID;
                _logger.LogInformation("Offer Search Validation - Start");
                if (!await _OfferSearchItems.GetOfferSearchItemsById(offerId, orderChangeRequestData.ChannelId))
                {
                    _logger.LogWarning($"Offer Search Validation - Failed: {offerId}, ChannelId: {orderChangeRequestData.ChannelId}");
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOrderRefId), request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOrderRefId, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
                _logger.LogInformation($"Offer Search Validation - End. OfferId: {offerId}");

                var channelInfo = await _cachedChannel.GetChannelEntity(orderChangeRequestData.ChannelId);

                _logger.LogInformation($"OrderChange Event Subscribe Handler Start for Id: {request.Id}");
                var cancelSegments = SegmentV4RequestMapper.GetDeleteAllSegments(orderChangeRequestData);
                var rebookSegments = SegmentV4RequestMapper.GetInsertAllSegments(orderChangeRequestData);
                _logger.LogInformation($"Start order change for PNR: {orderChangeRequestData.Rloc}");

                var locationCode = orderChangeRequestData.Pos.CityCode;

                bool isCodeShareFlight = orderChangeRequestData.OrderAmendments.First().FlightItems.Any(x => !string.IsNullOrEmpty(x.Segments.OperatingCarrier) && x.Segments.OperatingCarrier != Constants.Application.VSCarrierCode);

                var sessionDetails = await AmendBooking(request, orderChangeRequestData, channelInfo, locationCode, cancelSegments: cancelSegments, rebookSegments: rebookSegments, isCodeShareFlight);


                //code to Reprice
                try
                {
                    foreach (var passenger in orderChangeRequestData.CustomerDetails)
                    {
                        _logger.LogDebug("Reprice call - Start");
                        repriceResponse = await _mediator.Send(new Air4TicketingProV3RepriceCommand { SessionId = sessionDetails.SessionId, TransactionId = request.Id, Location = locationCode, Trvallers = TravellersMapper.Mapper(sessionDetails.OrderDetails.Customers, passenger), ChannelInfo = channelInfo });
                        _logger.LogInformation($"Reprice Response: {repriceResponse.FirstOrDefault()}");
                    }

                    // Logic to check even exchange scenarion

                    if (repriceResponse.FirstOrDefault() == Constants.Application.EvenExchange)
                    {
                        _logger.LogDebug($"Perform revenue call- Start");
                        var performRevenueResponse = await _mediator.Send(new Air4TicketingProcessV3PerRevenueCommand { SessionId = sessionDetails.SessionId, TransactionId = request.Id, Location = locationCode, EmailAddress = orderChangeRequestData.CustomerDetails.FirstOrDefault().Contacts.Email, ChannelInfo = channelInfo });
                        _logger.LogDebug($"Perform revenue call- End");
                        var result = OrderChangeResponseMapper.ResponseMapper(performRevenueResponse, orderChangeRequestData);
                        await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderChangeEventSubject });

                        if (offerId != null)
                            await PublishEventToDeleteDoc(offerId);
                    }
                    else
                    {
                        //Code to cancel and rebook with original segments

                        if (isCodeShareFlight)
                        {
                            _logger.LogDebug($"Rollback to original booking call- Start");
                            await AmendBooking(request, orderChangeRequestData, channelInfo, locationCode, cancelSegments: rebookSegments, rebookSegments: cancelSegments, isCodeShareFlight);
                            _logger.LogDebug($"Rollback to original booking call- End");

                            _logger.LogWarning($"OrderChange Event Subscribe Command Handler ||UnevenExchange ||  ErrorId : {request.Id}");
                        }
                        await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.UnevenExchangeRollbackErrorCode), request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                        return Result.Failure<Response<string>>(ErrorCodes.UnevenExchangeRollbackErrorCode);

                    }
                }
                catch (Exception ex)
                {
                    //Code to cancel and rebook with original segments
                    if (isCodeShareFlight)
                    {
                        _logger.LogDebug($"Rollback to original booking call- Start");
                        await AmendBooking(request, orderChangeRequestData, channelInfo, locationCode, cancelSegments: rebookSegments, rebookSegments: cancelSegments, isCodeShareFlight);
                        _logger.LogDebug($"Rollback to original booking call- End");

                        _logger.LogError(ex, $"Error occured in OrderChange Event Subscribe Command Handler || ErrorId : {request.Id}");
                        if (ex.Message == ErrorCodes.DeltaRePriceErrorCode || ex.Message == ErrorCodes.DeltaErrorCode)
                        {
                            await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.RepriceRollbackErrorCode), request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                            return Result.Failure<Response<string>>(ex.Message);
                        }
                    }
                    throw;
                }

                _logger.LogDebug($" OrderChange Event Subscribe Handler - End");
                return orderChangeResponse;

            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Error occured in OrderChange Event Subscribe Command Handler || ErrorId : {request.Id}");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ex.Message, request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                return Result.Failure<Response<string>>(ex.Message);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, $"Error occured in OrderChange Event Subscribe Command Handler || ErrorId : {request.Id}");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(ex, request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                return Result.Failure<Response<string>>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured in OrderChange Event Subscribe Command Handler || ErrorId : {request.Id}");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderChangeEventSubject });
                return Result.Failure<Response<string>>(ex.Message);
            }
        }
        private async Task<PnrSessionDetails> AmendBooking(OrderChangeEventProcessCommand request, OrderChangeRequestData orderChangeRequestData, Channel channelInfo, string locationCode, IEnumerable<Segment> cancelSegments, IEnumerable<Segment> rebookSegments, bool isCodeShareFlight)
        {
            //SetupContext
            var sessionDetails = await SetupContext(orderChangeRequestData, request, locationCode, cancelSegments, channelInfo);
            //code to Cancel and Rebook  
            await CancelandRebook(orderChangeRequestData, sessionDetails, cancelSegments, rebookSegments, channelInfo);
            // code to add SSRs
            await AddSpecialServiceRequest(request, sessionDetails, locationCode, channelInfo);
            //code to Tear Down
            await ContextTearDown(isCodeShareFlight, request, sessionDetails, locationCode, channelInfo);
            return sessionDetails;
        }

        private async Task CancelandRebook(OrderChangeRequestData orderChangeRequestData, PnrSessionDetails sessionDetails, IEnumerable<Segment> cancelSegments, IEnumerable<Segment> rebookSegments, Channel channelInfo)
        {
            _logger.LogDebug($"Cancel and Rebook - Start");
            List<Segment> segments = new();
            if (sessionDetails.OrderDetails.Segments.Any(item => string.IsNullOrEmpty(item.DestinationAirportCode) && string.IsNullOrEmpty(item.MarketingFlightNumber)))
            {
                // ARNK Sector case
                cancelSegments = cancelSegments.Where(param => !string.IsNullOrEmpty(param.MarketingFlightNumber) && !string.IsNullOrEmpty(param.DestinationAirportCode));
                IEnumerable<Segment> reBookARNKSegments = rebookSegments.Where(param => !string.IsNullOrEmpty(param.MarketingFlightNumber) && !string.IsNullOrEmpty(param.DestinationAirportCode));
                foreach (var item in cancelSegments.Zip(reBookARNKSegments, Tuple.Create))
                {
                    await _mediator.Send(new SegmentV4CancelAndRebookCommand() { SessionId = sessionDetails.SessionId, CancelSegments = new List<Segment>() { item.Item1 }, RebookSegments = new List<Segment>() { item.Item2 }, NumberInParty = orderChangeRequestData.CustomerDetails.Count(x => x.Ptc != Constants.Application.InfantRequestPtc), ChannelInfo = channelInfo });
                }
            }
            else
            {
                // Normal scenario (Non-ARNK sector)
                segments = await _mediator.Send(new SegmentV4CancelAndRebookCommand() { SessionId = sessionDetails.SessionId, CancelSegments = cancelSegments.ToList(), RebookSegments = rebookSegments.ToList(), NumberInParty = orderChangeRequestData.CustomerDetails.Count(x => x.Ptc != Constants.Application.InfantRequestPtc), ChannelInfo = channelInfo });
            }
            _logger.LogDebug($"Cancel and Rebook - End.Segments count" + segments.Count);
        }

        private async Task AddSpecialServiceRequest(OrderChangeEventProcessCommand request, PnrSessionDetails sessionDetails, string locationCode, Channel channelInfo)
        {

            List<SSRsCodes> SSRsdetails = SSRsInformation.GetSSRsDetails();
            var specialServiceReqInfo = sessionDetails.OrderDetails.SpecialServiceRequestInfo is null ? null : sessionDetails.OrderDetails.SpecialServiceRequestInfo.Where(item1 => SSRsdetails.Any(item2 => item2.Code == item1.RequestCode));
            if (specialServiceReqInfo is not null && specialServiceReqInfo.Any())
            {
                foreach (var item in specialServiceReqInfo)
                {
                    var response = await _mediator.Send(new SpecialServiceRequestV4Command() { ChannelInfo = channelInfo, SessionId = sessionDetails.SessionId, Location = locationCode, TransactionId = request.Id, specialServiceRequestInfo = item });
                }
            }
        }
        private async Task<PnrSessionDetails> SetupContext(OrderChangeRequestData orderChangeRequestData, OrderChangeEventProcessCommand request, string locationCode, IEnumerable<Segment> cancelSegments, Channel channelInfo)
        {
            return await _mediator.Send(new Air4CreateSessionCommand { pnrNumber = orderChangeRequestData.Rloc, transactionId = request.Id, location = locationCode, CancelSegments = cancelSegments.ToList(), ChannelInfo = channelInfo });
        }
        private async Task ContextTearDown(bool isCodeShareFlight, OrderChangeEventProcessCommand request, PnrSessionDetails sessionDetails, string locationCode, Channel channelInfo)
        {
            if (isCodeShareFlight)
            {
                _logger.LogDebug("Teardown call - Start");
                var tearDownResponse = await _mediator.Send(new PNRCommonV6TearDownCommand { SessionId = sessionDetails.SessionId, TransactionId = request.Id, Location = locationCode, ChannelInfo = channelInfo });
                _logger.LogDebug("Teardown call - End");

            }
        }

        public async Task<string> GetLocationCode(OrderChangeRequestData orderChangeRequestData)
        {
            _logger.LogDebug($"Get Location code- Start");
            var response = await _tableStorageService.RetrieveAsync(Constants.Application.AirportLocation, orderChangeRequestData.OriginDestinations.FirstOrDefault().OriginAirportCode);
            if (response == null)
            {
                _logger.LogWarning("Location Code not found");
            }
            _logger.LogDebug($"Get Location Code- End");
            return response.LocationCode;
        }

        private async Task PublishEventToDeleteDoc(string Id)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = new CosmosDocument()
                {
                    Id = Id,
                    PartitionKey = Id
                },
                Id = Guid.NewGuid().ToString(),
                Type = ConfigReader.Get(Constants.Environment.DeleteDocEventType),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
            await _mediator.Publish(cloudEvent);
        }
    }
}