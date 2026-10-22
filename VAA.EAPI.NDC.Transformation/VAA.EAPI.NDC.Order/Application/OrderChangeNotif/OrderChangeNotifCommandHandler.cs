using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using FlightSegment = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public class OrderChangeNotifCommandHandler : IOrderChangeNotifCommand
    {
        private readonly ILogger<OrderChangeNotifCommandHandler> _logger;
        private readonly IOrderNotificationMSCommand _orderNotificationMSCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSAgencyOCNDetailsQueryHandler _agencyOCNDetailsQueryHandler;
        private readonly OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly IInRuleService _inRuleService;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderChangeNotifCommandHandler(ILogger<OrderChangeNotifCommandHandler> logger, IOrderNotificationMSCommand orderNotificationMSCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSAgencyOCNDetailsQueryHandler agencyOCNDetailsQueryHandler, OrderSearchMSCommandHandler orderSearchMSCommandHandler, ValidationMSCommandHandler validationMSCommandHandler, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, IInRuleService inRuleService, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderNotificationMSCommand = orderNotificationMSCommand ?? throw new ArgumentNullException(nameof(orderNotificationMSCommand));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler;
            _orderDataMSQueryHandler = orderDataMSQueryHandler;
            _agencyOCNDetailsQueryHandler = agencyOCNDetailsQueryHandler;
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler;
            _validationMSCommandHandler = validationMSCommandHandler;
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _inRuleService = inRuleService ?? throw new ArgumentNullException(nameof(inRuleService));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Response<string>>> Handle(OrderChangeNotifRQ request, CancellationToken cancellationToken)
        {
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeNotifEventSubject
            };

            try
            {
                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);

                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("RequestType", "OCN");
                analyticsEventCommand.Track("PNR", request.Locator);

                analyticsEventCommand.Track("OrderID", $"VS932{request.Locator}{DateTime.UtcNow:yy}");
                bool itineraryChanged = false;
                var scheduleChanged = false;

                if (request.ChangeIndicators?.Indicator != null && request.ChangeIndicators.Indicator.Any(x => x.Name == "Itinerary" && x.HasChanged == "Y"))
                {
                    itineraryChanged = true;

                    var orderRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"VS932{request.Locator}{DateTime.UtcNow:yy}" }, cancellationToken);

                    if (orderRS?.OrderDocument is null) throw new InvalidOperationException("Unable to locate order document");

                    var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(orderRS.OrderDocument.Parties, orderRS.OrderDocument.Parties.CorporateAccount, false, true);
                    var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest }, cancellationToken);

                    if (validationMSResponse?.Error is not null)
                    {
                        var exception = new InvalidOperationException($"Validation failed: {string.Join(Environment.NewLine, validationMSResponse.Error.Select(x => $"{x.Code}: {x.Message}"))}");

                        exception.Data.Add("Errors", validationMSResponse.Error);

                        throw exception;
                    }

                    var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument, true);
                    var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new OrderSearchMSCommand(getReservationReq), cancellationToken);

                    var latestOrderHistory = getReservationOrderDoc?.OrderHistory?.FirstOrDefault();
                    var latestChangeType = latestOrderHistory?.ChangeType;
                    var isCancelled = latestChangeType == ChangeTypeEnum.DocumentVoided || latestChangeType == ChangeTypeEnum.DocumentRefunded;
                    if (isCancelled)
                    {
                        if (validationMSResponse.ValidateResponse.OCNFlag)
                        {
                            var agencyOCNDetails = await _agencyOCNDetailsQueryHandler.Handle(new ValidationMSAgencyOCNDetailsQuery()
                            {
                                AgencyId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)?.AgencyID,
                                AggregatorId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Distributor)?.AgencyID,
                                Environment = ConfigReader.Get(Constants.Environment.AgentRegistrationEnvironment)
                            }, cancellationToken);

                            if (agencyOCNDetails != null)
                            {
                                orderRS.OrderDocument.Version = orderRS.OrderDocument.Version + 1;
                                orderRS.OrderDocument.ChangeType = latestOrderHistory?.ChangeType;
                                orderRS.OrderDocument.ChangeDateTime = new() { UtcDateTime = MapUtcDateTime(request.EventTimeStamp.Value).ToString(Constants.Application.ZuluDateTimeFormat) };

                                var agentResponse = await _orderNotificationMSCommand.Handle(new() { OrderDocument = orderRS?.OrderDocument, AgentEndpointDetails = agencyOCNDetails.Select(x => { return new Infrastructure.Model.OrderNotificationMS.AgencyOCNDetails() { AgencyDetails = x.AgencyDetails, EndpointDetails = x.EndpointDetails }; }) });

                                if (agentResponse.OrderNotificationAgentResponse != null)
                                {
                                    foreach (var (x, index) in agentResponse.OrderNotificationAgentResponse.AgentResponse.Select((x, index) => (x, index)))
                                    {
                                        int i = index;
                                        i++;
                                        analyticsEventCommand.Track($"Agent{i}", x.AgentId);
                                        analyticsEventCommand.Track($"Agent{i}AckStatus", x.AcknowledgementRcvd ? "Y" : "N");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var isHoldBookingIssuance = getReservationOrderDoc != null && getReservationOrderDoc.OrderHistory?.FirstOrDefault()?.ChangeType == ChangeTypeEnum.DocumentIssuance;
                        var isTicketExchanged = getReservationOrderDoc != null && getReservationOrderDoc.OrderHistory?.FirstOrDefault()?.ChangeType == ChangeTypeEnum.DocumentExchanged;

                        OrderDocument updatedOrderDoc = new();

                        if (getReservationOrderDoc.Errors?.Any() != true)
                        {
                            if (getReservationOrderDoc.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator == true))
                            {
                                scheduleChanged = true;

                                OrderDocumentMapper.UpdateOrderItemId(getReservationOrderDoc, orderRS.OrderDocument, isDisrupted: true);
                            }

                            if (scheduleChanged || isHoldBookingIssuance || isTicketExchanged)
                            {
                                updatedOrderDoc = OrderDocumentMapper.UpdateOrderDocumentHistory(orderRS.OrderDocument, getReservationOrderDoc, MapUtcDateTime(request.EventTimeStamp.Value).ToString(Constants.Application.ZuluDateTimeFormat));

                                var firstHistory = updatedOrderDoc.OrderHistory?.FirstOrDefault();
                                if (isTicketExchanged && firstHistory != null && orderRS.OrderDocument.Version > 1)
                                {
                                    firstHistory.Version = (orderRS.OrderDocument.Version - 1).ToString();
                                }

                                OrderDocument updatedHistoryDoc = null;

                                if (scheduleChanged)
                                {
                                    updatedOrderDoc.Version = orderRS.OrderDocument.Version + 1;
                                    updatedOrderDoc.ChangeType = latestOrderHistory?.ChangeType;
                                    updatedOrderDoc.ChangeDateTime = new() { UtcDateTime = MapUtcDateTime(request.EventTimeStamp.Value).ToString(Constants.Application.ZuluDateTimeFormat) };

                                    if (ConfigReader.Try(Constants.Environment.InRuleEnabled, false))
                                    {
                                        await ApplyInRuleAdviceAsync(latestOrderHistory, orderRS.OrderDocument, updatedOrderDoc, cancellationToken);
                                    }
                                }

                                if (validationMSResponse.ValidateResponse.OCNFlag)
                                {
                                    var agencyOCNDetails = await _agencyOCNDetailsQueryHandler.Handle(new ValidationMSAgencyOCNDetailsQuery()
                                    {
                                        AgencyId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)?.AgencyID,
                                        AggregatorId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Distributor)?.AgencyID,
                                        Environment = ConfigReader.Get(Constants.Environment.AgentRegistrationEnvironment)
                                    }, cancellationToken);

                                    if (agencyOCNDetails != null)
                                    {
                                        var agentResponse = await _orderNotificationMSCommand.Handle(new() { OrderDocument = updatedOrderDoc, AgentEndpointDetails = agencyOCNDetails.Select(x => { return new Infrastructure.Model.OrderNotificationMS.AgencyOCNDetails() { AgencyDetails = x.AgencyDetails, EndpointDetails = x.EndpointDetails }; }) });

                                        if (agentResponse.OrderNotificationAgentResponse != null)
                                        {
                                            foreach (var (x, index) in agentResponse.OrderNotificationAgentResponse.AgentResponse.Select((x, index) => (x, index)))
                                            {
                                                int i = index;
                                                i++;
                                                analyticsEventCommand.Track($"Agent{i}", x.AgentId);
                                                analyticsEventCommand.Track($"Agent{i}AckStatus", x.AcknowledgementRcvd ? "Y" : "N");
                                            }
                                        }
                                    }
                                }

                                if (scheduleChanged)
                                {
                                    var historyOrderDoc = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"VS932{request.Locator}{DateTime.UtcNow:yy}-History" }, cancellationToken);

                                    updatedHistoryDoc = OrderDocumentMapper.UpdateOrderHistoryDocument(orderRS.OrderDocument, historyOrderDoc?.OrderDocument, getReservationOrderDoc);

                                    updatedOrderDoc.OrderHistory = null;

                                    await RetainDetailsFromOldOrderDoc(orderRS, updatedOrderDoc);

                                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = updatedOrderDoc };

                                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                                    var publishOrderHistoryDocRequest = new UpdateOrderRequest { OrderDocument = updatedHistoryDoc };

                                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderHistoryDocRequest), cancellationToken);
                                }
                            }

                        }
                        else
                        {
                            analyticsEventCommand.Track("ServiceName", "NDCOrderSearchMSRS");
                            analyticsEventCommand.Track("ErrorId", getReservationOrderDoc.Errors.First().Id);
                            analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                        }
                    }
                }

                analyticsEventCommand.Track("ItineraryChanged", itineraryChanged ? "Y" : "N");
                analyticsEventCommand.Track("ScheduleChanged", scheduleChanged ? "Y" : "N");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessages = string.Empty;

                if (ex.Data.Contains("Errors"))
                {
                    IEnumerable<ErrorDetails> errors = (IEnumerable<ErrorDetails>)ex.Data["Errors"];

                    errorMessages = $", Errors{string.Join(Environment.NewLine, errors.Select(x => $"{x.Code}: {x.Message}"))}";
                }

                _logger.LogError(ex, $"Error occurred - {ex.Message}{errorMessages}");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }

            return Result.Success(new Response<string>() { Status = Status.Accepted });
        }

        private async Task RetainDetailsFromOldOrderDoc(OrderRS orderRS, OrderDocument getReservationOrderDoc)
        {
            if (orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts?.CountryDailingCode != null)
            {
                getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
            }

            if (orderRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderRS.OrderDocument.FareTypeRequest;

            foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
            {
                getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
            }

            foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails)
            {
                var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                if (getReservationOrderDocCustomer != null)
                {
                    getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                }
            }

            if (orderRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
            {
                OrderDocumentMapper.MapOrderItems(orderRS.OrderDocument, getReservationOrderDoc);
            }

            foreach (var orderItem in orderRS.OrderDocument.OrderItems.Where(a => a.OCNTimestamp != null))
            {
                getReservationOrderDoc.OrderItems.First(x => x.OrderItemId == orderItem.OrderItemId).OCNTimestamp = orderItem.OCNTimestamp;
            }

            getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderRS.OrderDocument, getReservationOrderDoc);
            IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

            if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
            {
                var brandMsReq = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc);
                brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, CancellationToken.None);

            }
            else
            {
                var brandMsReq = BrandMSRequestMapper.MapBrandMSOrderRequest(getReservationOrderDoc);
                brandResponse = await _brandMSCommand.Handle(brandMsReq, CancellationToken.None);
            }

            getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);

            bool isCashPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
            bool isEasyPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

            if (isCashPayment || isEasyPayment)
            {
                foreach (var orderItem in getReservationOrderDoc.OrderItems)
                {
                    orderItem.PaymentDetails = orderRS.OrderDocument.OrderItems.First().PaymentDetails;
                }
            }
        }

        internal async Task ApplyInRuleAdviceAsync(
            VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory latestOrderHistory,
            OrderDocument originalOrderDocument,
            OrderDocument updatedOrderDocument,
            CancellationToken cancellationToken)
        {
            if (!ShouldInvokeInRule(latestOrderHistory, originalOrderDocument))
            {
                return;
            }

            var correlationId = AsyncContext.Current?.CorrelationId is Guid correlation && correlation != Guid.Empty
                ? correlation.ToString()
                : Guid.NewGuid().ToString();
            var inRuleRequest = CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, correlationId, latestOrderHistory);

            DisruptionResponse inRuleResponse = null;

            try
            {
                inRuleResponse = await _inRuleService.GetDisruptionResponseAsync(inRuleRequest, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "InRule disruption evaluation failed for correlation {CorrelationId}. Continuing schedule-change processing without seller follow-up action.",
                    correlationId);
            }

            if (IsValidInRuleResponse(updatedOrderDocument, inRuleRequest, inRuleResponse))
            {
                MapInRuleSellerFollowUpActions(updatedOrderDocument, inRuleResponse);
            }
            else
            {
                _logger.LogWarning("Ignoring invalid InRule disruption response for correlation {CorrelationId}.", correlationId);
            }
        }

        internal static bool ShouldInvokeInRule(
            VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory latestOrderHistory,
            OrderDocument orderDocument) =>
            latestOrderHistory?.ChangeType is ChangeTypeEnum.FlightDateTimeChange or
                ChangeTypeEnum.FlightCancellation &&
            orderDocument?.OrderItems?.Any() == true;

        internal static DisruptionRequest CreateInRuleRequest(
            OrderDocument originalOrderDocument,
            OrderDocument updatedOrderDocument,
            string correlationId,
            VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory latestOrderHistory = null) =>
            new()
            {
                Event = new Event
                {
                    EventId = $"OCN-{correlationId}",
                    EventType = "SCHEDULE_CHANGE",
                    EventVersion = "1",
                    Source = "SABRE_ENS",
                    OccurredAt = DateTime.TryParse(
                        updatedOrderDocument?.ChangeDateTime?.UtcDateTime,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var occurredAt)
                        ? occurredAt
                        : null,
                    CorrelationId = correlationId
                },
                Order = latestOrderHistory is null
                    ?
                    [
                        updatedOrderDocument is null
                            ? new OrderReference()
                            : (OrderReference)updatedOrderDocument
                    ]
                    :
                    [
                        CreateHistoricalOrderReference(originalOrderDocument, latestOrderHistory),
                        updatedOrderDocument is null
                            ? new OrderReference()
                            : (OrderReference)updatedOrderDocument
                    ]
            };

        private static OrderReference CreateHistoricalOrderReference(
            OrderDocument originalOrderDocument,
            VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory latestOrderHistory)
        {
            var historicalOrder = originalOrderDocument is null
                ? new OrderReference()
                : (OrderReference)originalOrderDocument;

            if (latestOrderHistory is null)
            {
                return historicalOrder;
            }

            historicalOrder.DataList = latestOrderHistory.DataList is null
                ? historicalOrder.DataList
                : (VAA.EAPI.NDC.Order.Infrastructure.Model.InRule.DataList)latestOrderHistory.DataList;
            historicalOrder.OrderItems = latestOrderHistory.OrderItems?
                .Where(orderItem => orderItem is not null)
                .Select(orderItem => (VAA.EAPI.NDC.Order.Infrastructure.Model.InRule.OrderItem)orderItem)
                ?? historicalOrder.OrderItems;
            historicalOrder.TimeLimits = latestOrderHistory.TimeLimits ?? historicalOrder.TimeLimits;
            historicalOrder.OrderStatus = latestOrderHistory.OrderStatus ?? historicalOrder.OrderStatus;
            historicalOrder.ChangeType = latestOrderHistory.ChangeType?.ToString() ?? historicalOrder.ChangeType;
            historicalOrder.ChangeDateTime = string.IsNullOrWhiteSpace(latestOrderHistory.ChangeDateTime)
                ? historicalOrder.ChangeDateTime
                : new UtcAndLocalDateTime { UtcDateTime = latestOrderHistory.ChangeDateTime };
            historicalOrder.IsSplit = latestOrderHistory.IsSplit;

            if (int.TryParse(latestOrderHistory.Version, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version))
            {
                historicalOrder.Version = version;
            }

            return historicalOrder;
        }

        internal static bool IsValidInRuleResponse(
            OrderDocument orderDocument,
            DisruptionRequest request,
            DisruptionResponse response)
        {
            return response?.Event?.EventId == request?.Event?.EventId &&
                   response.Event.EventType == request.Event.EventType &&
                   response.Event.EventVersion == request.Event.EventVersion &&
                   response.Event.Source == request.Event.Source &&
                   response.Event.CorrelationId == request.CorrelationId &&
                   response.InRuleResponse?.CorrelationId == request.CorrelationId &&
                   TryGetInRuleSellerFollowUpActions(response, out _);
        }

        internal static void MapInRuleSellerFollowUpActions(OrderDocument orderDocument, DisruptionResponse response)
        {
            if (!TryGetInRuleSellerFollowUpActions(response, out var sellerFollowUpActions))
            {
                return;
            }

            foreach (var orderItem in orderDocument?.OrderItems ?? Enumerable.Empty<OrderItems>())
            {
                if (orderItem is not null)
                {
                    orderItem.SellerFollowUpAction = sellerFollowUpActions.Select(action => action.ToString()).ToArray();
                }
            }
        }

        private static bool TryGetInRuleSellerFollowUpActions(
            DisruptionResponse response,
            out SellerFollowUpAction[] sellerFollowUpActions)
        {
            sellerFollowUpActions = [];
            var actionValues = response?.InRuleResponse?.Value?
                .Split(',', StringSplitOptions.TrimEntries);

            if (actionValues?.Length is not > 0)
            {
                return false;
            }

            var parsedActions = new List<SellerFollowUpAction>(actionValues.Length);

            foreach (var actionValue in actionValues)
            {
                if (!Enum.TryParse<SellerFollowUpAction>(actionValue, true, out var action) ||
                    !Enum.IsDefined(action) ||
                    !Enum.GetNames<SellerFollowUpAction>().Contains(actionValue, StringComparer.OrdinalIgnoreCase) ||
                    parsedActions.Contains(action))
                {
                    return false;
                }

                parsedActions.Add(action);
            }

            sellerFollowUpActions = parsedActions.ToArray();
            return true;
        }

        private static DateTime MapUtcDateTime(string creationDateTimeStamp)
        {
            var cstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(creationDateTimeStamp), cstTimeZone);
        }
    }
}
