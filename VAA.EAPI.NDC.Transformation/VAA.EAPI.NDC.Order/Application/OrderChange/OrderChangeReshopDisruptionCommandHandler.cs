using CSharpFunctionalExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderManagementMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeReshopDisruptionCommandHandler : IOrderChangeReshopDisruptionCommand
    {
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly SabreOrderManagementMSCommandHandler _sabreOrderManagementMSCommandHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ILogger<OrderChangeReshopDisruptionCommandHandler> _logger;
        private readonly OfferMSQueryHandler _offerMSQueryHandler;

        public OrderChangeReshopDisruptionCommandHandler( OrderDataMSQueryHandler orderDataMSQueryHandler,ValidationMSCommandHandler validationMSCommandHandler, SabreOrderManagementMSCommandHandler sabreOrderManagementMSCommandHandler,  PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, IHostEnvironment hostEnvironment, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<OrderChangeReshopDisruptionCommandHandler> logger, OfferMSQueryHandler offerMSQueryHandler , IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand)
        {
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _offerMSQueryHandler = offerMSQueryHandler ?? throw new ArgumentNullException(nameof(offerMSQueryHandler));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _sabreOrderManagementMSCommandHandler = sabreOrderManagementMSCommandHandler ?? throw new ArgumentNullException(nameof(sabreOrderManagementMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
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
            analyticsEventCommand.Track("RequestType", "OrderChangeReshop_Disruption");

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

                string offerID = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;

                var offerDoc = await _offerMSQueryHandler.Handle((OfferMSQuery)offerID, cancellationToken);

                analyticsEventCommand.Track("OfferId", offerID);
                analyticsEventCommand.Track("PosCity", offerDoc?.Pos?.CityCode);

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, null, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, cancellationToken);

                if (validationMSResponse.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestOrgName);

                if (validationMSResponse?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }

                analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode);

                var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request, _hostEnvironment, validationMSResponse, offerDoc, isCashPayment: false);

                if (offerDocValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferDocRequestValidation");
                    analyticsEventCommand.Track("ErrorId", offerDocValidationErrors.ResponseMessages?.Errors?.First()?.Id);

                    LogErrorDetails(offerDocValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(offerDocValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsResponseListPTC = new();

                if (offerDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var tasks = offerDoc.Offers.Select(async offer =>
                    {
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSPTCRequest(offer, offerDoc, false);
                        var brandResponsePTC = await _brandMSPTCCommand.Handle(brandMSRQ, cancellationToken);
                        if (!brandMsResponseListPTC.ContainsKey(offer))
                            brandMsResponseListPTC.TryAdd(offer, brandResponsePTC);
                    });
                    await Task.WhenAll(tasks);

                    if (brandMsResponseListPTC.Values.SelectMany(x => x ?? Enumerable.Empty<Infrastructure.Model.BrandMS.BrandRS>()).Any(x => x?.Errors?.Any() == true))
                    {
                        analyticsEventCommand.Track("AncillaryStatus", false);
                    }
                }
                else
                {
                    var tasks = offerDoc.Offers.Select(async offer =>
                    {
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSRequest(offer, offerDoc, false);
                        var brandResponse = await _brandMSCommand.Handle(brandMSRQ, cancellationToken);
                        if (!brandMsResponseListPTC.ContainsKey(offer))
                            brandMsResponseListPTC.TryAdd(offer, brandResponse);
                    });
                    await Task.WhenAll(tasks);

                    if (brandMsResponseListPTC.Values.SelectMany(x => x ?? Enumerable.Empty<Infrastructure.Model.BrandMS.BrandRS>()).Any(x => x?.Errors?.Any() == true))
                    {
                        analyticsEventCommand.Track("AncillaryStatus", false);
                    }
                }

                int penaltyIndex = 1;
                int baggageCounter = 1;
                int serviceCounter = 1;

                if (brandMsResponseListPTC.Any())
                {
                    foreach (var brandResp in brandMsResponseListPTC)
                    {
                        offerDoc = BrandMSResponseMapper.UpdateOfferDoc(brandResp.Value, brandResp.Key, offerDoc, ref penaltyIndex, ref baggageCounter, ref serviceCounter, true);
                    }
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

                var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request, orderDataRS, offerDoc, isReshopDisruption: true);
                var orderManagementMSResponse = await _sabreOrderManagementMSCommandHandler.Handle(orderManagementMSRequest, cancellationToken);

                if (orderManagementMSResponse.error is not null)
                {
                    var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                OrderDocumentMapper.UpdateOrderItemId(orderManagementMSResponse.orderDocument, orderDataRS.OrderDocument);

                bool isCashPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPayPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                UpdateFreePaymentDetails(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument, isCashPayment, isEasyPayPayment);
                RetainPaymentDetails(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument);
                OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument);

                orderManagementMSResponse.orderDocument.Version = orderDataRS.OrderDocument.Version + 1;

                var publishOrderMgmtDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSResponse.orderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderMgmtDocRequest), cancellationToken);

                RemoveRetainedPaymentDetails(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument);

                var orderResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSResponse.orderDocument, null, false, true, false, false, null, null, isCashPayment, isEasyPayPayment, isReshopDisruption: true);

                return new Response<IATA_OrderViewRS> { Status = Status.OK , Value = orderResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeReshopDisruptionCommandHandler));
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in {HandlerName}.", nameof(OrderChangeReshopDisruptionCommandHandler));
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private static void UpdateFreePaymentDetails(OrderDocument oldOrderDocument, OrderDocument updatedOrderDocument, bool isCashPayment, bool isEasyPayPayment)
        {
            foreach (var orderItem in updatedOrderDocument.OrderItems)
            {
                var isSeatOrderItem = orderItem.Services?.Any(service => service?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum) == true;
                if (isSeatOrderItem)
                {
                    continue;
                }

                var existingOrderItemId = orderItem.ExistingOrderItemId ?? orderItem.OrderItemId;
                var latestSequenceId = oldOrderDocument.OrderItems .FirstOrDefault(item => item.OrderItemId == existingOrderItemId)?.PaymentDetails? .Select(payment => int.TryParse(payment.SequenceId, out var sequenceId) ? sequenceId : 0) .DefaultIfEmpty() .Max() ?? 0;

                var paymentDetails = orderItem.PaymentDetails?.ToList();
                if (paymentDetails is null)
                {
                    continue;
                }

                orderItem.PaymentDetails = paymentDetails;

                foreach (var freePaymentDetail in paymentDetails.Where(payment => payment.Amount?.MonetaryAmount == "0.00"))
                {
                    freePaymentDetail.PaxRefIds = updatedOrderDocument.DataList.CustomerDetails.Select(customer => customer.CustId);
                    freePaymentDetail.SequenceId = Convert.ToString(latestSequenceId + 1);
                    freePaymentDetail.FormOfPayments = isEasyPayPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : isCashPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum: EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum;
                }
            }
        }

        private static void RetainPaymentDetails(OrderDocument oldOrderDocument, OrderDocument updatedOrderDocument)
        {
            foreach (var orderItem in updatedOrderDocument.OrderItems)
            {
                var isSeatOrderItem = orderItem.Services?.Any(service => service?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum) == true;
                if (isSeatOrderItem)
                {
                    continue;
                }

                var existingOrderItemId = orderItem.ExistingOrderItemId ?? orderItem.OrderItemId;
                var existingPaymentDetails = oldOrderDocument.OrderItems.FirstOrDefault(item => item.OrderItemId == existingOrderItemId)?.PaymentDetails ?? Enumerable.Empty<EAPI.Domain.Virgin.Models.Payment.PaymentDetails>();
                var freePaymentDetails = orderItem.PaymentDetails? .Where(payment => payment.Amount?.MonetaryAmount == "0.00") ?? Enumerable.Empty<EAPI.Domain.Virgin.Models.Payment.PaymentDetails>();

                orderItem.PaymentDetails = existingPaymentDetails.Concat(freePaymentDetails).ToList();
            }
        }

        private static void RemoveRetainedPaymentDetails(OrderDocument oldOrderDocument, OrderDocument updatedOrderDocument)
        {
            foreach (var orderItem in updatedOrderDocument.OrderItems)
            {
                var existingOrderItemId = orderItem.ExistingOrderItemId ?? orderItem.OrderItemId;
                var oldPaymentDetails = oldOrderDocument.OrderItems .FirstOrDefault(item => item.OrderItemId == existingOrderItemId)? .PaymentDetails? .ToList();

                if (oldPaymentDetails?.Any() != true || orderItem.PaymentDetails == null)
                {
                    continue;
                }

                orderItem.PaymentDetails = orderItem.PaymentDetails.Where(payment => !oldPaymentDetails.Any(oldPayment => ReferenceEquals(oldPayment, payment))) .ToList();
            }
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
