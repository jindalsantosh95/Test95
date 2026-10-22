using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using VAA.EAPI.NDC.Order.Validations.OrderCreate;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeCancelCommandHandler : IRequestHandler<OrderChangeCancelCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeCancelCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly PaymentIFGMSOrderRefundCommandHandler _paymentIFGMSOrderRefundCommand;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly ITransactionCommand _transactionCommand;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderChangeCancelCommandHandler(IMediator mediator, ILogger<OrderChangeCancelCommandHandler> logger, IHostEnvironment hostEnvironment, PaymentIFGMSOrderRefundCommandHandler paymentIFGMSOrderRefundCommandHandler, AirportDetailsQueryHandler airportDetailsQueryHandler, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ICurrencyConversionQuery currencyConversionQueryHandler, ICheckAgentStatusQuery checkAgentStatusQuery, ITransactionCommand transactionCommand, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, OrderDataMSQueryHandler orderDataMSQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _paymentIFGMSOrderRefundCommand = paymentIFGMSOrderRefundCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderRefundCommandHandler));
            _airportDetailsQueryHandler = airportDetailsQueryHandler ?? throw new ArgumentNullException(nameof(airportDetailsQueryHandler));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
        }

        public async Task<Response<IATA_OrderViewRS>> Handle(OrderChangeCancelCommand request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var nDCWarnings = new List<NDCWarning>();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            try
            {
                analyticsEventCommand.Track("OrderID", request.OrderChangeRQ.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_Cancel");

                var distributionChain = request.OrderChangeRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeRequest<IATA_OrderViewRS>(request.OrderChangeRQ, false, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderChangeCancelRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }
                var orderRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);
                var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderRS, _hostEnvironment);

                analyticsEventCommand.Track("PNR", orderRS?.OrderDocument?.BookingReference?.RLOC);
                analyticsEventCommand.Track("Delta_PNR", orderRS?.OrderDocument?.BookingReference?.ExternalRecordLocator);

                if (orderDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateDbOrder"); 
                    analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors?.ResponseMessages);
                    return errorDetails;
                }

                //Validating Disruption for cancel seller-up action
                var disruptionValidation = OrderChangeValidations.ValidateSellerFollowUpAction<IATA_OrderViewRS>(orderRS.OrderDocument, _hostEnvironment);

                if (disruptionValidation?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "SellerFollowUpActionValidation");
                    analyticsEventCommand.Track("ErrorId", disruptionValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                   
                    LogErrorDetails(disruptionValidation?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(disruptionValidation?.ResponseMessages);
                    return errorDetails;
                }
                bool isCancelDisruption = orderRS.OrderDocument.OrderItems.Any(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any( x => x.Is("Cancel"))) && orderRS.OrderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator);

                var paymentStatusValidationErrors = OrderChangeValidations.ValidatePaymentStatus(orderRS, _hostEnvironment);

                if (paymentStatusValidationErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentStatusValidation");
                    analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, null, true);
                var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, new CancellationToken());
                bool isNonUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode != "US";

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

                var offerID = ((AcceptCancelOfferType)request.OrderChangeRQ.Request.ChangeOrderChoice.Item).OfferID;
                var offerDocument = await _mediator.Send((CancelReshopOfferMSQuery)offerID, new CancellationToken());

                analyticsEventCommand.Track("offerID", offerID);
                analyticsEventCommand.Track("PosCity", offerDocument.Pos?.CityCode);

                if (offerDocument is null)
                {
                    analyticsEventCommand.Track("ServiceName", "CancelReshopOfferMS");
                    analyticsEventCommand.Track("ErrorId", DefaultError.DefaultErrorMesaage()?.Errors?.First()?.Id);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    return errorDetails;
                }

                bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

                if (!isNonUsAgent)
                {
                    var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);
                    OrderDocumentMapper.CancelFlowCurrencyConversion(offerDocument, currencyExchangeValue);

                    var checkResponse = await _checkAgentStatusQuery.Handle(validationMSResponse.ValidateResponse.AgencyDetails.RequestAgencyId);

                    var checkResponseValidation = AgencyValidation.ValidateArcAgentStatus<IATA_OrderViewRS>(checkResponse, _hostEnvironment);

                    if (checkResponseValidation?.ResponseMessages is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ArcAgentStatusValidation");
                        analyticsEventCommand.Track("ErrorId", checkResponseValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(checkResponseValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(checkResponseValidation?.ResponseMessages);
                        return errorDetails;
                    }
                }

                bool isCancelledAndRebooked = isCancelDisruption && orderRS.OrderDocument.DataList.SegmentDetails.Any(seg => seg.Status.Code.Is("UN")) && orderRS.OrderDocument.DataList.SegmentDetails.Any(seg => seg.Status.Code.Is("TK"));
                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument, false, isCancelledAndRebooked);
                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                if (getReservationOrderDoc is not null)
                {
                    if (orderRS.OrderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }
                    orderRS.OrderDocument.DataList.CustomerDetails = getReservationOrderDoc.DataList.CustomerDetails;
                }
                else
                {
                    analyticsEventCommand.Track("ServiceName", "OrderSearchMS");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                var cancelPnrRequest = OrderCancelRequestMapper.MapOrderCancelRequest(orderRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCancel, offerDocument, null , isCancelDisruption, orderRS.OrderDocument);
                var cancelPnrResponse = await _mediator.Send(cancelPnrRequest, cancellationToken);

                analyticsEventCommand.Track("IsRFC", cancelPnrRequest?.OrderChangeRequest?.IsRFC);

                if (!cancelPnrResponse)
                {
                    analyticsEventCommand.Track("ServiceName", "CancelPnrResponse");
                    
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    return errorDetails;
                }

                if (isCancelledAndRebooked)
                {
                    if (orderRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
                    {
                        OrderDocumentMapper.MapOrderItems(orderRS.OrderDocument, getReservationOrderDoc);
                    }

                    foreach (var orderItem in orderRS.OrderDocument.OrderItems)
                    {
                        var getReservationOrderDocOrderItem = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);

                        if(getReservationOrderDocOrderItem != null )
                        {
                            orderItem.Services = getReservationOrderDocOrderItem.Services;
                            orderItem.AccountableDocumentRefs = getReservationOrderDocOrderItem.AccountableDocumentRefs;
                        }
                    }
                }

                OrderDocumentMapper.UpdateCancelFlowOrderDocument(orderRS.OrderDocument, offerDocument, getReservationOrderDoc, isCancelledAndRebooked, isCancelDisruption);

                bool isCashPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;
                PaymentRS paymentIFGMSRefundResponse = null;
                IEnumerable<AirportDetail> airportDetails = null;
                Dictionary<string, string> refundTransactions = new();


                if (isCashPayment && isNonUsAgent)
                {
                    var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request.OrderChangeRQ, validationMSResponse, _hostEnvironment);

                    if (cashPaymentValidation?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "CashFopValidation");
                        analyticsEventCommand.Track("ErrorId", cashPaymentValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(cashPaymentValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(cashPaymentValidation?.ResponseMessages);
                        return errorDetails;
                    }

                    airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);
                    
                    if (airportDetails?.Any() != true)
                    {
                        analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    var paymentIFGMSRefundRequest = PaymentMSRequestMapper.MapOrderCancelIFGRefundRequest(offerDocument, orderRS.OrderDocument, airportDetails, false);
                    paymentIFGMSRefundResponse = await _paymentIFGMSOrderRefundCommand.Handle(paymentIFGMSRefundRequest, cancellationToken);

                    if (paymentIFGMSRefundResponse?.Errors != null && paymentIFGMSRefundResponse.Errors.Any())
                    {
                        analyticsEventCommand.Track("ServiceName", "paymentIFGMSRefundResponse");
                        var message = paymentIFGMSRefundResponse?.Errors?.FirstOrDefault()?.Message;
                        var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                        analyticsEventCommand.Track("ErrorId", errorCode);
                        
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        return errorDetails;
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(null, null, orderRS.OrderDocument, Constants.Application.RefundPaymentDocType, false, true);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateRefundFOPPaymentDetails(orderRS.OrderDocument, paymentDocumentResponse.PaymentDocument);
                    else analyticsEventCommand.Track("paymentDocumentResponse", paymentDocumentResponse?.Errors?.FirstOrDefault()?.Message);

                }

                else if (isEasyPayment && isNonUsAgent)
                {
                    var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(true, validationMSResponse, _hostEnvironment);

                    if (easyFopValidation?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EasyFopValidation");
                        analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                        
                        LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                        return errorDetails;
                    }

                    airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);

                    var paymentIFGMSRefundRequest = PaymentMSRequestMapper.MapOrderCancelIFGRefundRequest(offerDocument, orderRS.OrderDocument, airportDetails, true);
                    paymentIFGMSRefundResponse = await _paymentIFGMSOrderRefundCommand.Handle(paymentIFGMSRefundRequest, cancellationToken);

                    if (paymentIFGMSRefundResponse?.Errors != null && paymentIFGMSRefundResponse.Errors.Any())
                    {
                        analyticsEventCommand.Track("ServiceName", "paymentIFGMSRefundResponse");
                        var message = paymentIFGMSRefundResponse?.Errors?.FirstOrDefault()?.Message;
                        var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                        analyticsEventCommand.Track("ErrorId", errorCode);
                        
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(null, null, orderRS.OrderDocument, Constants.Application.RefundPaymentDocType, false, true, true);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateRefundFOPPaymentDetails(orderRS.OrderDocument, paymentDocumentResponse.PaymentDocument, null, true, null, null, null, paymentDocumentRequest.PaymentDetails.First().ApprovalCode, paymentDocumentRequest.PaymentDetails.First().CardMethod.CardNumber);
                    else analyticsEventCommand.Track("PaymentDocument", paymentDocumentResponse?.Errors?.FirstOrDefault()?.Message);
                }

                else if(!isCashPayment && !isEasyPayment)
                {
                    var toatlRefundAmount = offerDocument.Offers.First().OfferItems?.Select(a =>
                                            (Math.Abs(Convert.ToDecimal(a.OfferitemPricing.First().SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount)
                                             + Convert.ToDecimal(offerDocument.DataList?.Penalty?.FirstOrDefault(p => p.Id == a.Services?.First().PenaltyRef?.CancellationPenaltyRefIds?.FirstOrDefault())?.MaxPrice?.Amount?.MonetaryAmount))) * a.Services?.First()?.ServiceBeneficiaries?.Count()).Sum();

                    analyticsEventCommand.Track("toatlRefundAmount", toatlRefundAmount);

                    var rlocs = new[] { orderRS.OrderDocument.BookingReference?.RLOC, orderRS.OrderDocument.BookingReference?.SplitRLOC?.ParentRLOC }.Where(rloc => !string.IsNullOrEmpty(rloc)).Concat(orderRS.OrderDocument.BookingReference?.SplitRLOC?.ChildRLOCDetails?.Select(c => c.ChildRLOC).Where(rloc => !string.IsNullOrEmpty(rloc)) ?? Enumerable.Empty<string>()).Distinct();
                    var paymentDocuments = new List<PaymentTransactionDocument>();

                    foreach (var rloc in rlocs)
                    {
                        var orderId = $"VS932{rloc}{DateTime.UtcNow:yy}";
                        var docs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery { OrderID = orderId }, cancellationToken);

                        paymentDocuments.AddRange(docs);
                    }

                    var capturedPaymentDocs = paymentDocuments.Where(doc => doc.Status == "Payment Captured" && !doc.Id.Contains("Split") && !(doc.OrderItems?.Any(oi => oi.ProductType == Constants.ProductType.Seat) ?? false));
                   
                    decimal remainingRefundAmount = toatlRefundAmount ?? 0m;
                    PaymentRS refundResponse = null;
                    TransactionServiceResponse transactionServiceResponse = null;

                    foreach (var paymentDoc in capturedPaymentDocs.OrderBy(x => x.CreatedOn))
                    {
                        if (remainingRefundAmount <= 0)
                            break;

                        var transactionAmount = Convert.ToDecimal(paymentDoc.PaymentDetails.First().Amount.BalanceAmount?? paymentDoc.PaymentDetails.First().Amount.MonetaryAmount);
                        decimal refundAmountForTransaction = Math.Min(remainingRefundAmount, transactionAmount);

                        if (isIFGIntegrated && isNonUsAgent)
                        {
                            var orderHistoryDocument = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"{paymentDoc.OrderId}-History" }, cancellationToken);

                            if (orderHistoryDocument.Errors != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "OrderDataMSHistory");
                                analyticsEventCommand.Track("ErrorId", orderHistoryDocument.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                return errorDetails;
                            }

                            var matchingOrderHistories = orderHistoryDocument.OrderDocument.OrderHistory .Where(x => x.OrderItems.Any(oi => oi.PaymentDetails != null && oi.PaymentDetails.Any(pay => pay.PaymentReferenceId == paymentDoc.Id)));
                            var latestHistories = matchingOrderHistories.OrderBy(x => x.Version).First();

                            var paymentIFGMSRefundRequest = PaymentMSRequestMapper.MapOrderCancelCCIFGRefundRequest(offerDocument, orderRS.OrderDocument, latestHistories, paymentDoc);
                            paymentIFGMSRefundResponse = await _paymentIFGMSOrderRefundCommand.Handle(paymentIFGMSRefundRequest, cancellationToken);

                            if (paymentIFGMSRefundResponse?.Errors != null && paymentIFGMSRefundResponse.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "paymentIFGMSRefundResponse");
                                var message = paymentIFGMSRefundResponse?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);
                                
                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                return errorDetails;
                            }
                            refundTransactions.Add(paymentIFGMSRefundResponse.PaymentDocument.TransactionId, paymentIFGMSRefundResponse.PaymentDocument.TrackId );
                        }
                        else
                        {
                            var refundPaymentRequest = PaymentMSRequestMapper.MapOrderChangeCancelRefundRequest(orderRS.OrderDocument, offerDocument, refundAmountForTransaction, paymentDoc.Id, paymentDoc.OrderId);
                            refundResponse = await _mediator.Send(refundPaymentRequest, cancellationToken);

                            // transaction api for cc 
                            if (!isNonUsAgent)
                            {
                                var transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderRS.OrderDocument, refundResponse, null, Constants.ProductType.Ticket, false, false, offerDocument.IsRfc, null, !offerDocument.IsRfc);

                                transactionServiceResponse = await _transactionCommand.Handle(transactionRequest);

                                if (transactionServiceResponse.ErrorDetails != null)
                                {
                                    analyticsEventCommand.Track("ServiceName", "transactionServiceResponse");
                                    analyticsEventCommand.Track("ErrorId", transactionServiceResponse?.ErrorDetails?.FirstOrDefault()?.Id);
                                    nDCWarnings.AddRange(OrderCreateValidation.PaymentReportingFailureWarning(_hostEnvironment));
                                }
                            }

                            OrderDocumentMapper.UpdateRefundFOPPaymentDetails(orderRS.OrderDocument, null, null, false, null, refundResponse, transactionServiceResponse);

                        }

                        remainingRefundAmount -= refundAmountForTransaction;
                    }

                    
                }
                else if (isCashPayment && !isNonUsAgent)
                {
                    // transaction api for cash payment 
                    var transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderRS.OrderDocument, null, null, Constants.ProductType.Ticket, true, false, offerDocument.IsRfc, null, !offerDocument.IsRfc);
                    var transactionresponse = await _transactionCommand.Handle(transactionRequest);

                    if (transactionresponse.ErrorDetails != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "transactionresponse");
                        analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.FirstOrDefault()?.Id);

                        nDCWarnings.AddRange(OrderCreateValidation.PaymentReportingFailureWarning(_hostEnvironment));
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(null, null, orderRS.OrderDocument, Constants.Application.RefundPaymentDocType, false, true ,false ,transactionRequest,transactionresponse);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateRefundFOPPaymentDetails(orderRS.OrderDocument, paymentDocumentResponse.PaymentDocument,null, false , null, null,transactionresponse);
                }

                if (isIFGIntegrated && !isCashPayment && !isEasyPayment && isNonUsAgent)
                {
                    var paymentDocs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery()
                    {
                        OrderID = orderRS.OrderDocument.OrderNumber.OrderId
                    }, cancellationToken);

                    foreach (var doc in paymentDocs)
                    {
                        var trackId = refundTransactions.FirstOrDefault(x => x.Key == doc.Id).Value;
                        if (trackId != null)
                        {
                            var capturedDoc = paymentDocs.FirstOrDefault(pd => pd.Id == doc.Id.Split("-").First());

                            doc.OrderItems = capturedDoc?.OrderItems;
                            doc.PaymentDetails.First().CardMethod = capturedDoc?.PaymentDetails?.FirstOrDefault()?.CardMethod;
                            OrderDocumentMapper.UpdateCCPaymentDetails(orderRS.OrderDocument, doc, trackId);
                        };
                    }
                }

                var segmentCount = orderRS.OrderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                analyticsEventCommand.Track("SegmentCount" , segmentCount);

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderRS.OrderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                string approvalCode = orderRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;

                var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.OrderCancel, orderRS.OrderDocument);

                _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                var orderviewResponse = OrderResponseMapper.MapOrderResponse(orderRS.OrderDocument, nDCWarnings, false, false, true, false, null, null, isCashPayment, isEasyPayment, approvalCode);

                LogOrderDetails(orderviewResponse, orderRS.OrderDocument, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderviewResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeCancelCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Change Cancel Command Handler");
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
        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                analyticsEventCommand.Track("Delta_PNR", orderDocument?.BookingReference?.ExternalRecordLocator);

                DateTime? dateOfResponse = DateTime.UtcNow;

                foreach (var (payment, index) in orderRS?.PaymentFunctions?.Select((payment, index) => (payment, index)))
                {
                    var paymentSummary = payment?.PaymentProcessingSummary;
                    var paymentMethod = paymentSummary?.PaymentProcessingSummaryPaymentMethod?.Item;
                    var cardPayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.ReceiptPaymentCardType;
                    var offlinePayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.OfflinePaymentInstructionReceiptType;

                    analyticsEventCommand.Track($"TransactionId{index + 1}", payment?.PaymentProcessingSummary?.PaymentID);
                    analyticsEventCommand.Track($"TransactionAmount{index + 1}", payment?.PaymentProcessingSummary?.Amount?.Value);
                    analyticsEventCommand.Track($"TransactionCurrencyCode{index + 1}", payment?.PaymentProcessingSummary?.Amount?.CurCode);
                    analyticsEventCommand.Track($"TransactionCardType{index + 1}", cardPayment?.CardBrandCode is null ? offlinePayment?.PaymentBrandCode : null);
                }

                var orderResponse = (orderRS.Items as OrderViewResponseType[]).First();

                bool oneWay = orderResponse.DataLists.OriginDestList.Length == 1;
                bool roundTrip = !oneWay && orderResponse.DataLists.OriginDestList.Length == 2 && orderResponse.DataLists.OriginDestList.First().OriginCode == orderResponse.DataLists.OriginDestList.Last().DestCode && orderResponse.DataLists.OriginDestList.First().DestCode == orderResponse.DataLists.OriginDestList.Last().OriginCode;

                analyticsEventCommand.Track("OneWay", oneWay ? 'Y' : 'N');
                analyticsEventCommand.Track("RoundTrip", roundTrip ? 'Y' : 'N');
                analyticsEventCommand.Track("MultiCity", !oneWay && !roundTrip ? 'Y' : 'N');

                foreach (var (ond, index) in orderDocument.DataList.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
                {
                    analyticsEventCommand.Track($"OriginDestination{index + 1}", $"{ond.OriginDestination.OriginAirport}{ond.OriginDestination.DestinationAirport}");
                    analyticsEventCommand.Track($"DepartureDate{index + 1}", ond.OriginDestination.DepartureDate);
                }

                foreach (var (ptc, index) in orderResponse.DataLists.PaxList.GroupBy(x => x.PTC).Select((ptc, index) => (ptc, index)))
                {
                    analyticsEventCommand.Track($"PaxType{index + 1}", ptc.Key);
                    analyticsEventCommand.Track($"PaxCount{index + 1}", ptc.Count());
                }

                foreach (var (ond, index) in orderDocument.DataList.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
                {
                    var segRefIds = orderDocument.DataList.Journey.FirstOrDefault(x => x.JourneyId == ond.JourneyRefIds.FirstOrDefault()).FlightSegmentRefIds;
                    var priceClassRefId = orderDocument.OrderItems.First().Services.FirstOrDefault(x => segRefIds.Contains(x.SegmentRefIds.FirstOrDefault())).PriceClassRefIds.First();

                    analyticsEventCommand.Track($"O&D{index + 1}Cabin", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).CabinType.CabinName);
                    analyticsEventCommand.Track($"O&D{index + 1}FareBrand", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).ProductName);
                }

                var paidSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0);

                if (paidSeatCount > 0)
                {
                    var paidSeatAmount = orderDocument.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.Sum(x => Convert.ToDecimal(x.PriceDetails.First(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));
                }

                decimal totalAmount = decimal.Zero;
                decimal baseAmount = decimal.Zero;

                foreach (var orderItem in orderResponse.Order.First().OrderItem)
                {
                    totalAmount += Convert.ToDecimal(orderItem.Price?.TotalAmount.Value);
                    baseAmount += Convert.ToDecimal(orderItem.Price?.BaseAmount?.Value);
                }

                analyticsEventCommand.Track("TotalAmount", totalAmount);

                if (orderResponse.Warning?.Any() ?? false)
                {
                    foreach (var (warning, index) in orderResponse.Warning.Select((warning, index) => (warning, index)))
                    {
                        analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
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
