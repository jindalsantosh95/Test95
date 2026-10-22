using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
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
    public class OrderChangeDisruptionCommandHandler : IOrderChangeDisruptionCommand
    {
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly SabreOrderManagementMSCommandHandler _sabreOrderManagementMSCommandHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ILogger<OrderChangeDisruptionCommandHandler> _logger;

        public OrderChangeDisruptionCommandHandler(OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSCommandHandler validationMSCommandHandler, Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler orderSearchMSCommandHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, SabreOrderManagementMSCommandHandler sabreOrderManagementMSCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<OrderChangeDisruptionCommandHandler> logger)
        {
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _sabreOrderManagementMSCommandHandler = sabreOrderManagementMSCommandHandler ?? throw new ArgumentNullException(nameof(sabreOrderManagementMSCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
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

            analyticsEventCommand.Track("OrderID", request.Request?.Order?.OrderID);
            analyticsEventCommand.Track("RequestType", "OrderChange_Disruption");

            try
            {
                var distributionChain = request.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
               
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeAmendApis<IATA_OrderViewRS>(request, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderChangeDisruptionRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var freePaymentFunctionErrors = OrderChangeValidations.ValidateDisruptionPaymentFunctions<IATA_OrderViewRS>(request, _hostEnvironment);

                if (freePaymentFunctionErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "DisruptionPaymentFunctionsValidation");
                    analyticsEventCommand.Track("ErrorId", freePaymentFunctionErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(freePaymentFunctionErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(freePaymentFunctionErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderDataRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.Request?.Order?.OrderID ?? $"VS932{request.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderDataRS, _hostEnvironment);

                if (orderDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderDataMSValidation");
                    analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors?.ResponseMessages);
                    return errorDetails;
                }

                var acceptOrderItemIdsValidationErrors = OrderChangeValidations.ValidateAcceptSellerFollowUpAction<IATA_OrderViewRS>(orderDataRS.OrderDocument, request, _hostEnvironment);

                if (acceptOrderItemIdsValidationErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "AcceptSellerFollowUpActionValidation");
                    analyticsEventCommand.Track("ErrorId", acceptOrderItemIdsValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(acceptOrderItemIdsValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(acceptOrderItemIdsValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, null, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, new CancellationToken());

                if (validationMSResponse.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderMgmtMSReq = OrderChangeRequestMapper.MapOrderMgmtReqForDisruption(orderDataRS.OrderDocument);
                var orderMgmtMSRes = await _sabreOrderManagementMSCommandHandler.Handle(orderMgmtMSReq, cancellationToken);

                if (orderMgmtMSRes.error is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                bool isCashPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPayPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;
                bool isHoldBooking = orderMgmtMSRes.orderDocument.OrderItems.Any() &&
                                     orderMgmtMSRes.orderDocument.OrderItems.All(orderItem => orderItem.AccountableDocumentRefs?.Any() != true);

                await RetainOldOrderDocDetails(orderDataRS, orderMgmtMSRes.orderDocument, isCashPayment, isEasyPayPayment, isHoldBooking);

                var publishOrderMgmtDocRequest = new UpdateOrderRequest { OrderDocument = orderMgmtMSRes.orderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderMgmtDocRequest), cancellationToken);

                var orderResponse = OrderResponseMapper.MapOrderResponse(orderMgmtMSRes.orderDocument, null, isHoldBooking, false, false, false, null, null, isCashPayment, isEasyPayPayment);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change disruption processing was cancelled.");
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in {HandlerName}.", nameof(OrderChangeDisruptionCommandHandler));
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private async Task RetainOldOrderDocDetails(OrderRS oldOrderDoc, OrderDocument orderMgmtRes, bool isCashPayment, bool isEasyPayPayment, bool isHoldBooking)
        {
            if (isHoldBooking)
            {
                OrderDocumentMapper.UpdateOrderItemId(orderMgmtRes, oldOrderDoc.OrderDocument, isDisrupted: true);
            }
            else if (oldOrderDoc.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
            {
                OrderDocumentMapper.MapOrderItems(oldOrderDoc.OrderDocument, orderMgmtRes);
            }

            orderMgmtRes.DataList.BaggageAllowances = oldOrderDoc.OrderDocument.DataList.BaggageAllowances;
            orderMgmtRes.DataList.ServiceDefinitions = oldOrderDoc.OrderDocument.DataList.ServiceDefinitions;
            orderMgmtRes.DataList.Penalty = oldOrderDoc.OrderDocument.DataList.Penalty;
            orderMgmtRes.PointOfSale = oldOrderDoc.OrderDocument.PointOfSale;
            orderMgmtRes.OrderRemarks = oldOrderDoc.OrderDocument.OrderRemarks;
            orderMgmtRes.ChangeType = oldOrderDoc.OrderDocument.ChangeType;
            orderMgmtRes.ChangeDateTime = oldOrderDoc.OrderDocument.ChangeDateTime;
            orderMgmtRes.Version = oldOrderDoc.OrderDocument.Version;

            if (oldOrderDoc.OrderDocument.ChangeType == ChangeTypeEnum.FlightCancellation ||
                (isHoldBooking && oldOrderDoc.OrderDocument.ChangeType == ChangeTypeEnum.FlightDateTimeChange))
            {
                orderMgmtRes = BrandMSRequestMapper.UpdateFares(oldOrderDoc.OrderDocument, orderMgmtRes);
                IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                if (orderMgmtRes.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var brandMsReq = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(orderMgmtRes);
                    brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, CancellationToken.None);

                }
                else
                {
                    var brandMsReq = BrandMSRequestMapper.MapBrandMSOrderRequest(orderMgmtRes);
                    brandResponse = await _brandMSCommand.Handle(brandMsReq, CancellationToken.None);
                }

                orderMgmtRes = BrandMSResponseMapper.MapFareComponents(orderMgmtRes, brandResponse, oldOrderDoc.OrderDocument);
            }

            foreach (var orderItem in orderMgmtRes.OrderItems)
            {
                orderItem.PaymentDetails = oldOrderDoc.OrderDocument.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId)?.PaymentDetails;
            }

            OrderDocumentMapper.UpdateRetainedJourneySeatOrderItems(orderMgmtRes, oldOrderDoc.OrderDocument);
            OrderDocumentMapper.UpdateOTPaymentFunctions(orderMgmtRes, oldOrderDoc.OrderDocument, isCashPayment, isEasyPayPayment);
        }

        private void LogErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                if (responseMessage?.Errors?.Any() != true)
                {
                    _logger.LogWarning("LogErrorDetails called with null or empty ResponseMessage.Errors");
                    return;
                }

                var resultCode = ErrorCodeMapper.MapHttpStatusCode(responseMessage);

                analyticsEventCommand.Track("StatusCode", resultCode);

                foreach (var (error, index) in responseMessage.Errors.Select((e, i) => (e, i)))
                {
                    analyticsEventCommand.Track($"ErrorCode{index + 1}", error.Code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }
    }
}


