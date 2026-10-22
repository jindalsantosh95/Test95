using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderManagementMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;
using PaxSegmentRefListType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaxSegmentRefListType;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeRefundCommandHandler : IRequestHandler<OrderChangeRefundCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeRefundCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly PaymentIFGMSOrderRefundCommandHandler _paymentIFGMSOrderRefundCommand;
        private readonly PaymentIFGMSExchangeCommandHandler _paymentIFGMSExchangeCommand;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly IOrderChangeReshopHoldBookingCommand _orderChangeReshopHoldBookingCommand;

        public OrderChangeRefundCommandHandler(IMediator mediator, ILogger<OrderChangeRefundCommandHandler> logger, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, PaymentIFGMSOrderRefundCommandHandler paymentIFGMSOrderRefundCommand, PaymentIFGMSExchangeCommandHandler paymentIFGMSExchangeCommand, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ICurrencyConversionQuery currencyConversionQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, IOrderChangeReshopHoldBookingCommand orderChangeReshopHoldBookingCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _paymentIFGMSOrderRefundCommand = paymentIFGMSOrderRefundCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderRefundCommand));
            _paymentIFGMSExchangeCommand = paymentIFGMSExchangeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSExchangeCommand));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _orderChangeReshopHoldBookingCommand = orderChangeReshopHoldBookingCommand ?? throw new ArgumentNullException(nameof(orderChangeReshopHoldBookingCommand));
        }

        public async Task<Response<IATA_OrderViewRS>> Handle(OrderChangeRefundCommand request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            var nDCWarnings = new List<NDCWarning>();
            try
            {
                analyticsEventCommand.Track("PNR", request.OrderChangeRQ.Request?.BookingRef?.FirstOrDefault()?.BookingID);

                analyticsEventCommand.Track("OrderID", request.OrderChangeRQ.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_Refund");

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
                    analyticsEventCommand.Track("ServiceName", "OrderChangeRefundRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }
                string offerID = (request.OrderChangeRQ.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;
                var offerDoc = await _mediator.Send((OfferMSQuery)offerID, cancellationToken);

                bool isReshopHoldBooking = (offerDoc != null && offerDoc.DocType == Constants.Application.OrderChangeReshopHoldBooking);

                if (isReshopHoldBooking)
                {
                    var result = await _orderChangeReshopHoldBookingCommand.Handle(request.OrderChangeRQ, request.SubscriptionKey, cancellationToken);
                    return result.Value;
                }
                else
                {
                    var dbErrors = OrderChangeValidations.ValidateDbOffer(offerDoc, offerID, _hostEnvironment);

                    if (dbErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OfferStoreValidation");
                        analyticsEventCommand.Track("ErrorId", dbErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(dbErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(dbErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    var evenExchangeValidation = OrderChangeValidations.ValidatePaymentFunctions<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, offerDoc);
                    if (evenExchangeValidation?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenExchangePaymentValidation");
                        analyticsEventCommand.Track("ErrorId", evenExchangeValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(evenExchangeValidation.ResponseMessages, analyticsEventCommand);

                        return ErrorMapper.GetErrorDetails(evenExchangeValidation.ResponseMessages);
                    }

                    var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, offerDoc?.Parties?.CorporateAccount, true);
                    var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);
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

                    var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, validationMSResponse, offerDoc, isCashPayment: false, isEvenExchange: false, isUnEvenExchangeRefund: true);

                    if (offerDocValidationErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OfferDocRequestValidation");
                        analyticsEventCommand.Track("ErrorId", offerDocValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(offerDocValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(offerDocValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    analyticsEventCommand.Track("offerID", offerID);
                    analyticsEventCommand.Track("PosCity", offerDoc.Pos?.CityCode);

                    var serviceListOfferId = ((AcceptSelectedQuotedOfferListType)request.OrderChangeRQ?.Request?.ChangeOrderChoice?.Item)?.SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem != null && !((PaxSegmentRefListType)x.SelectedALaCarteOfferItem.OfferFlightAssociations.Item).PaxSegmentRefID.IsNullOrEmpty()));

                    Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse = new();

                    if (serviceListOfferId != null)
                    {
                        flightServiceListResponse = await _mediator.Send((FlightServiceOfferDocQuery)serviceListOfferId.OfferRefID, cancellationToken);
                        var flightServiceValidationErrors = OrderChangeValidations.ValidateFlightServiceList<IATA_OrderViewRS>(request.OrderChangeRQ, flightServiceListResponse, _hostEnvironment);

                        if (flightServiceValidationErrors != null)
                        {
                            analyticsEventCommand.Track("ServiceName" , "FlightServiceMS");
                            analyticsEventCommand.Track("ErrorId", flightServiceValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(flightServiceValidationErrors?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(flightServiceValidationErrors?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    var orderChangeRQErrors = OrderChangeValidations.ValidateEvenOrderChangeRQ<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (orderChangeRQErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeRequestValidation");
                        analyticsEventCommand.Track("ErrorId", orderChangeRQErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(orderChangeRQErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderChangeRQErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    var offerDocErros = OrderChangeValidations.ValidateEvenOrderChangeRQAgainstOfferDoc<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (offerDocErros?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeOfferDocValidation");
                        analyticsEventCommand.Track("ErrorId", offerDocErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(offerDocErros?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(offerDocErros?.ResponseMessages);
                        return errorDetails;
                    }

                    var orderDataRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                    if (orderDataRS is null) { analyticsEventCommand.Track("ServiceName", "OrderDataMSFailed"); }

                    var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderDataRS, _hostEnvironment);

                    if (orderDocErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderDataMSValidation");
                        analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsResponseListPTC = new();
                    bool isPartialFlown = orderDataRS.OrderDocument.DataList.SegmentDetails.Any(x => x.Status?.DeliveryStatusCode == "Delivered");

                    if (offerDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                    {
                        var tasks = offerDoc.Offers.Select(async offer =>
                        {
                            var brandMSRQ = BrandMSRequestMapper.MapBrandMSPTCRequest(offer, offerDoc, isPartialFlown);
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
                            var brandMSRQ = BrandMSRequestMapper.MapBrandMSRequest(offer, offerDoc, isPartialFlown);
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
                            BrandMSResponseMapper.UpdateOfferDoc(brandResp.Value, brandResp.Key, offerDoc, ref penaltyIndex, ref baggageCounter, ref serviceCounter, true);
                        }
                    }
                    
                    if (orderDataRS.OrderDocument.DataList.SegmentDetails.Any(seg => seg.Status.DeliveryStatusCode == "Delivered"))
                    {
                        OrderDocumentMapper.MapFlownSegmentOrderChangePenalty(orderDataRS.OrderDocument, offerDoc);
                    }

                    var paymentStatusValidationErrors = OrderChangeValidations.ValidatePaymentStatus(orderDataRS, _hostEnvironment);

                    if (paymentStatusValidationErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "PaymentStatusValidation");
                        analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    if (!isNonUsAgent)
                    {
                        var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);
                        OrderDocumentMapper.CurrencyConversion(offerDoc, currencyExchangeValue);
                    }

                    bool isCashPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                    bool isEasyPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                    if (isCashPayment)
                    {
                        var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request.OrderChangeRQ, validationMSResponse, _hostEnvironment);

                        if (cashPaymentValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "CashFopValidation");
                            analyticsEventCommand.Track("ErrorId", cashPaymentValidation?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(cashPaymentValidation?.ResponseMessages, analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(cashPaymentValidation?.ResponseMessages);
                        }
                    }
                    if (isEasyPayment)
                    {
                        var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(isEasyPayment, validationMSResponse, _hostEnvironment);

                        if (easyFopValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "ValidateEasyFop");
                            analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                            return errorDetails;
                        }
                    }
                    else
                    {
                        var paymentRq = request.OrderChangeRQ.Request.PaymentFunctions?.First().PaymentProcessingDetails;
                        var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq?.PaymentMethod?.Item);
                        var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

                        analyticsEventCommand.Track("PaymentTypeCode", request?.OrderChangeRQ?.Request?.PaymentFunctions?.First().PaymentMethodCriteria?.PaymentTypeCode);

                        analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                        analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');
                    }

                    var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request.OrderChangeRQ, orderDataRS, offerDoc);
                    var orderManagementMSRS = await _mediator.Send(orderManagementMSRequest, cancellationToken);

                    if (orderManagementMSRS.error is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                        var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSRS, _hostEnvironment);
                        analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                        return errorDetails;
                    }

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderManagementMSRS.orderDocument, _hostEnvironment, nDCWarnings));

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateRefundDetails(offerDoc, _hostEnvironment));

                    orderManagementMSRS.orderDocument.BookingReference.SplitRLOC = orderDataRS.OrderDocument.BookingReference?.SplitRLOC;

                    OrderDocumentMapper.UpdateOrderItemId(orderManagementMSRS.orderDocument, orderDataRS.OrderDocument);

                    bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

                    PaymentRS paymentDocumentResponse = null;

                    var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);

                    if (airportDetails?.Any() != true)
                    {
                        analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    if (isCashPayment && isNonUsAgent)
                    {
                        var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, null, true, false,false);
                        var paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                        if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                            var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        var paymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                        var latestPaymentDetails = paymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault();

                        if (latestPaymentDetails != null)
                        {
                            latestPaymentDetails.TrackId = paymentIFGOrderExchangeRS.PaymentDocument?.TrackId;
                        }
                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                        }

                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = null;
                        }

                    }

                    else if (isEasyPayment && isNonUsAgent)
                    {
                        var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(true, validationMSResponse, _hostEnvironment);

                        if (easyFopValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "EasyFopValidation");
                            analyticsEventCommand.Track("ErrorId", easyFopValidation?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                            return errorDetails;
                        }

                        var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, null, true, false, true);
                        var paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                        if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                            var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        var paymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                        var latestPaymentDetails = paymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault();

                        if (latestPaymentDetails != null)
                        {
                            latestPaymentDetails.TrackId = paymentIFGOrderExchangeRS.PaymentDocument?.TrackId;
                        }

                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                        }

                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = null;
                        }

                    }
                    else if (!isEasyPayment && !isCashPayment)
                    {
                        if (isIFGIntegrated)
                        {
                            var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, null, true, true,false);
                            var paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                            if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                                var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);

                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            }

                            var paymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                            var latestPaymentDetails = paymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault();

                            if (latestPaymentDetails != null)
                            {
                                latestPaymentDetails.TrackId = paymentIFGOrderExchangeRS.PaymentDocument?.TrackId;
                            }

                        }

                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = orderDataRS.OrderDocument.OrderItems.First().PaymentDetails;
                        }

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                        foreach (var orderItem in orderManagementMSRS.orderDocument.OrderItems)
                        {
                            orderItem.PaymentDetails = null;
                        }

                    }
                    try
                    {
                        UpdateGenderAndDOB(request.OrderChangeRQ, orderManagementMSRS.orderDocument);

                        var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, orderManagementMSRS.orderDocument, null);

                        var gstValidation = OrderChangeValidations.ValidateGSTDetailsAgainstOrderDoc(orderManagementMSRS.orderDocument, request.OrderChangeRQ, _hostEnvironment);
                        if (gstValidation.Any())
                        {
                            isGSTValidationFailed = true;
                            nDCWarnings.AddRange(gstValidation);
                        }

                        var gstWarnings = OrderChangeValidations.ValidateGSTDetails<NDCWarning>(request.OrderChangeRQ, _hostEnvironment);
                        if (gstWarnings.Any())
                        {
                            isGSTValidationFailed = true;
                            nDCWarnings.AddRange(gstWarnings);
                        }

                        nDCWarnings.AddRange(OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, orderManagementMSRS.orderDocument));

                        //Update APIS , SSR and GST
                        if (request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.IdentityDoc != null) || (!isGSTValidationFailed && request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.Employer != null)) || flightServiceListResponse?.ServiceListResponse != null || (request.OrderChangeRQ.Request.DataLists.ContactInfoList != null && request.OrderChangeRQ.Request.DataLists.ContactInfoList.Any(x => x.PostalAddress != null)))
                        {
                            var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequest(request.OrderChangeRQ, orderManagementMSRS.orderDocument, flightServiceListResponse, isGSTValidationFailed);
                            var orderChangeAPISResponse = await _mediator.Send(orderManagementAPISRequest, cancellationToken);

                            if (orderChangeAPISResponse != null)
                            {
                                OrderDocumentMapper.UpdateSSRAPISGSTToOrderDoc(orderManagementMSRS.orderDocument, orderChangeAPISResponse.orderDocument);

                                nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderChangeAPISResponse.orderDocument, _hostEnvironment, nDCWarnings));

                                orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                                if (orderChangeAPISResponse.orderDocument?.Warnings?.Any() == true)
                                {
                                    orderManagementMSRS.orderDocument.Warnings = orderChangeAPISResponse?.orderDocument?.Warnings;
                                }

                                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                            }
                            else
                            {
                                analyticsEventCommand.Track("ServiceName", "orderChangeAPISResponse");
                                analyticsEventCommand.Track("ErrorId", orderChangeAPISResponse?.error?.First().Id);
                                nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                            }
                        }
                    }
                    catch
                    {
                        nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                    }


                    OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSRS.orderDocument);

                    var segmentCount = orderManagementMSRS.orderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                    analyticsEventCommand.Track("SegmentCount", segmentCount);

                    var refundDifferenceAmount = orderManagementMSRS?.orderDocument?.OrderItems?
                    .Where(x => x?.RefundPrice != null)
                    .Sum(x =>
                    {
                        var beneficiariesCount = x?.Services?.Where(s => s?.ServiceBeneficiaries != null)?.SelectMany(s => s?.ServiceBeneficiaries)?.Distinct()?.Count();
                        var refundAmountPerPax = x?.RefundPrice?.Sum(r => Convert.ToDecimal(r?.Amount?.MonetaryAmount));
                        return refundAmountPerPax * beneficiariesCount;
                    });

                    analyticsEventCommand.Track("RefundDifferenceAmount", refundDifferenceAmount);

                    string approvalCode = orderDataRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;

                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.OrderChange, orderManagementMSRS.orderDocument);
                    _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                    var orderchangeResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSRS.orderDocument, nDCWarnings, false, true, false, false, null, null, isCashPayment, isEasyPayment, approvalCode);

                    LogOrderDetails(orderchangeResponse, orderManagementMSRS.orderDocument, analyticsEventCommand);

                    return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };


                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeRefundCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Change Refund Command Handler");
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

                foreach (var (payment, index) in orderRS?.PaymentFunctions?.Select((payment, index) => (payment, index)))
                {
                    var paymentSummary = payment?.PaymentProcessingSummary;
                    var paymentMethod = paymentSummary?.PaymentProcessingSummaryPaymentMethod?.Item;
                    var cardPayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.ReceiptPaymentCardType;
                    var offlinePayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.OfflinePaymentInstructionReceiptType;

                    analyticsEventCommand.Track($"TransactionId{index + 1}", payment?.PaymentProcessingSummary?.PaymentID);
                    analyticsEventCommand.Track($"TransactionAmount{index + 1}", payment?.PaymentProcessingSummary?.Amount?.Value);
                    analyticsEventCommand.Track($"TransactionCurrencyCode{index + 1}", payment?.PaymentProcessingSummary?.Amount?.CurCode);
                    analyticsEventCommand.Track($"TransactionCardType{index + 1}", cardPayment?.CardBrandCode ?? offlinePayment?.PaymentBrandCode);
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
                    var priceClassRefId = orderDocument.OrderItems.First().Services?.FirstOrDefault(x => segRefIds.Contains(x?.SegmentRefIds?.FirstOrDefault()))?.PriceClassRefIds?.FirstOrDefault();

                    analyticsEventCommand.Track($"O&D{index + 1}Cabin", orderDocument.DataList?.PriceClass?.FirstOrDefault(x => x?.Id == priceClassRefId)?.CabinType?.CabinName);
                    analyticsEventCommand.Track($"O&D{index + 1}FareBrand", orderDocument.DataList?.PriceClass?.FirstOrDefault(x => x?.Id == priceClassRefId)?.ProductName);
                }

                var paidSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0);

                if (paidSeatCount > 0)
                {
                    var paidSeatAmount = orderDocument.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.Sum(x => Convert.ToDecimal(x.PriceDetails.First(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));
                }

                var freeSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) == 0);

                decimal totalAmount = decimal.Zero;
                decimal baseAmount = decimal.Zero;

                foreach (var orderItem in orderResponse.Order.First().OrderItem)
                {
                    totalAmount += Convert.ToDecimal(orderItem.Price.TotalAmount.Value);
                    baseAmount += Convert.ToDecimal(orderItem.Price.BaseAmount?.Value);
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

        public void UpdateGenderAndDOB(IATA_OrderChangeRQ request, OrderDocument orderDocument)
        {
            foreach (var pax in request.Request.DataLists.PaxList)
            {
                var orderDocCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == pax.PaxID);

                if (pax.Individual == null)
                {
                    pax.Individual = new()
                    {
                        GivenName = [orderDocCust.FirstName],
                        Surname = orderDocCust.LastName,
                        Birthdate = Convert.ToDateTime(orderDocCust.DateOfBirth),
                        BirthdateSpecified = true,
                        GenderCode = Enum.Parse<Domain.OrderChangeRQ.GenderCodeType>(orderDocCust.Gender),
                        GenderCodeSpecified = true,
                        TitleName = orderDocCust.Title
                    };
                }
                else
                {
                    pax.Individual.Birthdate = Convert.ToDateTime(orderDocCust.DateOfBirth);
                    pax.Individual.BirthdateSpecified = true;
                    pax.Individual.GenderCode = Enum.Parse<Domain.OrderChangeRQ.GenderCodeType>(orderDocCust.Gender);
                    pax.Individual.GenderCodeSpecified = true;
                }
            }
        }
    }
}
