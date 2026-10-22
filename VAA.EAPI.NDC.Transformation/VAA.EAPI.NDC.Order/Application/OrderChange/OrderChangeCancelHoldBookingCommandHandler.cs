using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeCancelHoldBookingCommandHandler : IOrderChangeCancelHoldBookingCommand
    {
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OrderCancelCommandHandler _orderCancelCommandHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ILogger<OrderChangeCancelHoldBookingCommandHandler> _logger;

        public OrderChangeCancelHoldBookingCommandHandler(
            OrderDataMSQueryHandler orderDataMSQueryHandler,
            OrderSearchMSCommandHandler orderSearchMSCommandHandler,
            ValidationMSCommandHandler validationMSCommandHandler,
            OrderCancelCommandHandler orderCancelCommandHandler,
            IHostEnvironment hostEnvironment,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler,
            PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler,
            ILogger<OrderChangeCancelHoldBookingCommandHandler> logger)
        {
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderCancelCommandHandler = orderCancelCommandHandler ?? throw new ArgumentNullException(nameof(orderCancelCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            analyticsEventCommand.Track("OrderID", request?.Request?.Order?.OrderID);
            analyticsEventCommand.Track("RequestType", "OrderChange_CancelHoldBooking");

            try
            {
                var distributionChain = request?.DistributionChain;
                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor.ParticipatingOrg.OrgID);
                }

                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeAmendApis<IATA_OrderViewRS>(request, _hostEnvironment);
                if (requestValidationErrors?.ResponseMessages is not null)
                {
                    return MapValidationError(requestValidationErrors.ResponseMessages, "CancelHoldBookingRequestValidation", analyticsEventCommand);
                }

                var cancelHoldBookingRequestValidation = OrderChangeValidations.ValidateCancelHoldBookingRequest<IATA_OrderViewRS>(request, _hostEnvironment);
                if (cancelHoldBookingRequestValidation?.ResponseMessages is not null)
                {
                    return MapValidationError(cancelHoldBookingRequestValidation.ResponseMessages, "CancelHoldBookingRequestValidation", analyticsEventCommand);
                }

                var orderDataResponse = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery { OrderID = request.Request.Order.OrderID }, cancellationToken);

                var orderDocumentErrors = OrderChangeValidations.ValidateDbOrder(orderDataResponse, _hostEnvironment);
                if (orderDocumentErrors?.ResponseMessages is not null)
                {
                    return MapValidationError(orderDocumentErrors.ResponseMessages, "OrderDataMSValidation", analyticsEventCommand);
                }

                var paxListValidation = OrderChangeValidations.ValidateCancelHoldBookingPaxList<IATA_OrderViewRS>(request, orderDataResponse.OrderDocument, _hostEnvironment);
                if (paxListValidation?.ResponseMessages is not null)
                {
                    return MapValidationError(paxListValidation.ResponseMessages, "CancelHoldBookingPaxListValidation", analyticsEventCommand);
                }

                var holdBookingValidation = OrderChangeValidations.ValidateCancelHoldBooking<IATA_OrderViewRS>(orderDataResponse.OrderDocument, _hostEnvironment);
                if (holdBookingValidation?.ResponseMessages is not null)
                {
                    return MapValidationError(holdBookingValidation.ResponseMessages, "CancelHoldBookingValidation", analyticsEventCommand);
                }

                analyticsEventCommand.Track("PNR", orderDataResponse.OrderDocument.BookingReference?.RLOC);
                analyticsEventCommand.Track("Delta_PNR", orderDataResponse.OrderDocument.BookingReference?.ExternalRecordLocator);

                var validationRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, null, true);
                var validationResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand { AgencyValidateRequest = validationRequest, AgentSubscriptionKey = subscriptionKey }, cancellationToken);

                if (validationResponse?.Error is not null)
                {
                    var agencyErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationResponse, _hostEnvironment);
                    return MapValidationError(agencyErrors.ResponseMessages, "ValidationMS", analyticsEventCommand);
                }

                var validateAgent = OrderChangeValidations.ValidateCancelHoldBookingOwnership<IATA_OrderViewRS>(orderDataResponse.OrderDocument, validationResponse, _hostEnvironment);
                if (validateAgent?.ResponseMessages is not null)
                {
                    return MapValidationError(validateAgent.ResponseMessages, "OrderOwnershipValidation", analyticsEventCommand);
                }

                bool isCancelDisruption = orderDataResponse.OrderDocument.OrderItems.Any(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Cancel"))) && orderDataResponse.OrderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator);

                var disruptionValidation = OrderChangeValidations.ValidateCancelSellerFollowUpAction<ErrorType>(orderDataResponse.OrderDocument, _hostEnvironment);
                if (disruptionValidation?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "CancelSellerFollowUpActionValidation");
                    analyticsEventCommand.Track("ErrorId", disruptionValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(disruptionValidation.ResponseMessages, analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(disruptionValidation.ResponseMessages);
                }

                bool isCancelledAndRebooked = isCancelDisruption && orderDataResponse.OrderDocument.DataList.SegmentDetails.Any(segment => segment.Status?.Code.Is("UN") == true) && orderDataResponse.OrderDocument.DataList.SegmentDetails.Any(segment => segment.Status?.Code.Is("TK") == true);

                Infrastructure.Model.OrderMS.OrderDocument getReservationOrderDoc = null;
                if (isCancelledAndRebooked)
                {
                    var getReservationRequest = OrderSearchMSRequestMapper.Mapper(orderDataResponse.OrderDocument, false, true);
                    getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new OrderSearchMSCommand(getReservationRequest), cancellationToken);

                    if (getReservationOrderDoc?.Errors?.Any() == true || getReservationOrderDoc?.DataList is null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderSearchMS");
                        return MapTechnicalError(analyticsEventCommand);
                    }

                    if (orderDataResponse.OrderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode is not null
                        && getReservationOrderDoc.DataList.CustomerDetails?.FirstOrDefault()?.Contacts is not null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDataResponse.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }

                    orderDataResponse.OrderDocument.DataList.CustomerDetails = getReservationOrderDoc.DataList.CustomerDetails;
                }

                var cancelRequest = OrderCancelRequestMapper.MapCancelHoldBookingRequest(orderDataResponse.OrderDocument, isCancelDisruption);
                var isCancelled = await _orderCancelCommandHandler.Handle(cancelRequest, cancellationToken);

                if (!isCancelled)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                    return MapTechnicalError(analyticsEventCommand);
                }

                if (isCancelledAndRebooked)
                {
                    OrderDocumentMapper.ReconcileCancelHoldBookingRebookedOrderDocument(orderDataResponse.OrderDocument, getReservationOrderDoc);
                }

                OrderDocumentMapper.UpdateCancelHoldBookingOrderDocument(orderDataResponse.OrderDocument);
                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderDataResponse.OrderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                OrderDocumentMapper.UpdateCancelHoldBookingPriceDetails(orderDataResponse.OrderDocument);
                var orderResponse = OrderResponseMapper.MapOrderResponse(orderDataResponse.OrderDocument, null, true, false, true, false);

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);
                return new Response<IATA_OrderViewRS> { Status = Status.OK, Value = orderResponse };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred in Order Change Cancel Hold Booking Command Handler");
                return MapTechnicalError(analyticsEventCommand);
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private static Result<Response<IATA_OrderViewRS>> MapValidationError(ResponseMessage responseMessage, string serviceName, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            analyticsEventCommand.Track("ServiceName", serviceName);
            analyticsEventCommand.Track("ErrorId", responseMessage.Errors?.FirstOrDefault()?.Id);
            LogErrorDetails(responseMessage, analyticsEventCommand);
            return ErrorMapper.GetErrorDetails(responseMessage);
        }

        private static Result<Response<IATA_OrderViewRS>> MapTechnicalError(TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var error = DefaultError.DefaultErrorMesaage();
            LogErrorDetails(error, analyticsEventCommand);
            return ErrorMapper.GetErrorDetails(error);
        }

        private static void LogErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var statusCode = ErrorCodeMapper.MapHttpStatusCode(responseMessage);
            analyticsEventCommand.Track("StatusCode", statusCode);

            foreach (var (error, index) in responseMessage.Errors.Select((item, index) => (item, index)))
            {
                analyticsEventCommand.Track($"ErrorCode{index + 1}", error.Code);
            }
        }
    }
}
