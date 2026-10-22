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
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
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
    public class OrderChangeEvenExchangeCommandHandler : IRequestHandler<OrderChangeEvenExchangeCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeEvenExchangeCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ManageCustomerRepoCommandHandler _manageCustomerRepoCommandHandler;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSExchangeCommandHandler _paymentIFGMSExchangeCommand;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly ITransactionCommand _transactionCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly IARCOrderDataMSQuery _orderDataARCQuery;
        private readonly IOrderChangeReshopDisruptionCommand _orderChangeReshopDisruptionCommand;

        public OrderChangeEvenExchangeCommandHandler(IMediator mediator, ILogger<OrderChangeEvenExchangeCommandHandler> logger, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, OrderDataMSQueryHandler orderDataMSQueryHandler, Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler orderSearchMSCommandHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ManageCustomerRepoCommandHandler manageCustomerRepoCommandHandler, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSExchangeCommandHandler paymentIFGMSExchangeCommand, ValidationMSCommandHandler validationMSCommandHandler, AirportDetailsQueryHandler airportDetailsQueryHandler, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, ICheckAgentStatusQuery checkAgentStatusQuery, ITransactionCommand transactionCommand, ICurrencyConversionQuery currencyConversionQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, IARCOrderDataMSQuery orderDataARCQuery, IOrderChangeReshopDisruptionCommand orderChangeReshopDisruptionCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _manageCustomerRepoCommandHandler = manageCustomerRepoCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSExchangeCommand = paymentIFGMSExchangeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSExchangeCommand));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _airportDetailsQueryHandler = airportDetailsQueryHandler ?? throw new ArgumentNullException(nameof(airportDetailsQueryHandler));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _orderDataARCQuery = orderDataARCQuery ?? throw new ArgumentNullException(nameof(orderDataARCQuery));
            _orderChangeReshopDisruptionCommand = orderChangeReshopDisruptionCommand ?? throw new ArgumentNullException(nameof(orderChangeReshopDisruptionCommand));
        }

        public async Task<Response<IATA_OrderViewRS>> Handle(OrderChangeEvenExchangeCommand request, CancellationToken cancellationToken)
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
                analyticsEventCommand.Track("PNR", request.OrderChangeRQ.Request?.BookingRef?.FirstOrDefault()?.BookingID);

                analyticsEventCommand.Track("OrderID", request.OrderChangeRQ.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_EvenExchange");

                string offerID = (request.OrderChangeRQ.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;
                var offerDoc = await _mediator.Send((OfferMSQuery)offerID, new CancellationToken());
                bool isNameChange = (offerDoc != null && offerDoc.DocType == Constants.Application.OrderReshopNameChange) ? true : false;
                bool isReshopDisruption = offerDoc?.DocType == Constants.Application.OrderReshopDisruption;
                if (isNameChange)
                {
                    analyticsEventCommand.Track("ChangeType", "NameCorrection");
                }
                var distributionChain = request.OrderChangeRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeRequest<IATA_OrderViewRS>(request.OrderChangeRQ, false, _hostEnvironment, isNameChange);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderChangeRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.First()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var paymentFunctions = request.OrderChangeRQ.Request.PaymentFunctions;
                var isDisruptedOrderChangeWithOTPayment = isReshopDisruption &&
                    paymentFunctions?.Any() == true &&
                    paymentFunctions.All(paymentFunction =>
                    {
                        var paymentProcessingDetails = paymentFunction.PaymentProcessingDetails;
                        return paymentProcessingDetails?.Amount?.Value == 0 &&
                            paymentProcessingDetails.PaymentMethod?.Item is OfflinePaymentType offlinePayment &&
                            offlinePayment.PaymentBrandCode == Constants.Application.OfflineBrandTypeCode &&
                            offlinePayment.PaymentTypeCode == Constants.Application.OfflinePaymentTypeCode;
                    });

                var paymentFunctionErrors = isReshopDisruption && !isDisruptedOrderChangeWithOTPayment
                    ? OrderChangeValidations.ValidateDisruptionPaymentFunctions<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment)
                    : OrderChangeValidations.ValidateOTPaymentFunctions<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                if (paymentFunctionErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentFunctionValidation");
                    analyticsEventCommand.Track("ErrorId", paymentFunctionErrors.ResponseMessages?.Errors?.First()?.Id);

                    LogErrorDetails(paymentFunctionErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(paymentFunctionErrors?.ResponseMessages);
                    return errorDetails;
                }

                var dbErrors = OrderChangeValidations.ValidateDbOffer(offerDoc, offerID, _hostEnvironment);

                if (dbErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "DBOfferValidation");
                    analyticsEventCommand.Track("ErrorId", dbErrors.ResponseMessages?.Errors?.First()?.Id);

                    LogErrorDetails(dbErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(dbErrors?.ResponseMessages);
                    return errorDetails;
                }

                if (isReshopDisruption && !isDisruptedOrderChangeWithOTPayment)
                {
                    var result = await _orderChangeReshopDisruptionCommand.Handle(request.OrderChangeRQ, request.SubscriptionKey, cancellationToken);
                    return result.Value;
                }

                if (offerDoc != null && offerDoc.DocType == Constants.Application.OrderReshopNameChange)
                {
                    var orderChangeResponse = await NameCorrectionHandler(request, offerDoc, analyticsEventCommand, cancellationToken);

                    if (orderChangeResponse is null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderChangeNameCorrectionFailed");
                        analyticsEventCommand.Track("ErrorId", "NameCorrectionHandlerResponseNull");

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    return orderChangeResponse;
                }
                else
                {
                    var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, offerDoc?.Parties?.CorporateAccount, true);
                    var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, new CancellationToken());
                    bool isNonUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode != "US";
                    bool isTidsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.RequestAgencyIdType == "TIDS";

                    if (validationMSResponse.Error != null)
                    {
                        var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);

                        analyticsEventCommand.Track("ServiceName", "ValidationMS");
                        analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.First()?.Id);

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

                    var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, validationMSResponse, offerDoc, isCashPayment: false, isEvenExchange: true);

                    if (offerDocValidationErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OfferDocRequestValidation");
                        analyticsEventCommand.Track("ErrorId", offerDocValidationErrors.ResponseMessages?.Errors?.First()?.Id);

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
                            analyticsEventCommand.Track("ServiceName", "FlightServiceMS");
                            analyticsEventCommand.Track("ErrorId", flightServiceValidationErrors.ResponseMessages?.Errors?.First()?.Id);

                            LogErrorDetails(flightServiceValidationErrors?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(flightServiceValidationErrors?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    var orderChangeRQErrors = OrderChangeValidations.ValidateEvenOrderChangeRQ<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (orderChangeRQErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeRequestValidation");
                        analyticsEventCommand.Track("ErrorId", orderChangeRQErrors.ResponseMessages?.Errors?.First()?.Id);

                        LogErrorDetails(orderChangeRQErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderChangeRQErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    var offerDocErros = OrderChangeValidations.ValidateEvenOrderChangeRQAgainstOfferDoc<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (offerDocErros?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeOfferDocValidation");
                        analyticsEventCommand.Track("ErrorId", offerDocErros.ResponseMessages?.Errors?.First()?.Id);

                        LogErrorDetails(offerDocErros?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(offerDocErros?.ResponseMessages);
                        return errorDetails;
                    }

                    var orderDataRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);
                    var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderDataRS, _hostEnvironment);

                    if (orderDocErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderDataMS");
                        analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.First()?.Id);

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
                            offerDoc = BrandMSResponseMapper.UpdateOfferDoc(brandResp.Value, brandResp.Key, offerDoc, ref penaltyIndex, ref baggageCounter, ref serviceCounter, true);
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
                        analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.First()?.Id);

                        LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

                    if (!isNonUsAgent)
                    {

                        var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);
                        OrderDocumentMapper.CurrencyConversion(offerDoc, currencyExchangeValue);

                        var checkResponse = await _checkAgentStatusQuery.Handle(validationMSResponse.ValidateResponse.AgencyDetails.RequestAgencyId);
                        var checkResponseValidation = AgencyValidation.ValidateArcAgentStatus<IATA_OrderViewRS>(checkResponse, _hostEnvironment);

                        if (checkResponseValidation?.ResponseMessages is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "ArcAgentStatusValidation");
                            analyticsEventCommand.Track("ErrorId", checkResponseValidation.ResponseMessages?.Errors?.First()?.Id);

                            LogErrorDetails(checkResponseValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(checkResponseValidation?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    bool isCashPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                    bool isEasyPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                    PaymentIFGMSAuthorizeCommand paymentIFGMSAuthorize = null;
                    PaymentRS paymentIFGAuthorizeRS = null;

                    var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);
                    if (airportDetails?.Any() != true)
                    {
                        analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    if (isCashPayment && isNonUsAgent)
                    {
                        var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request.OrderChangeRQ, validationMSResponse, _hostEnvironment);

                        if (cashPaymentValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "CashFopValidation");
                            analyticsEventCommand.Track("ErrorId", cashPaymentValidation.ResponseMessages?.Errors?.First()?.Id);

                            LogErrorDetails(cashPaymentValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(cashPaymentValidation?.ResponseMessages);
                            return errorDetails;
                        }

                        paymentIFGMSAuthorize = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderDataRS.OrderDocument);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGMSAuthorize, cancellationToken);

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGMSAuthorizeFailed");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);
                            var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }
                    }

                    if (isEasyPayment && isNonUsAgent)
                    {
                        var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(true, validationMSResponse, _hostEnvironment);

                        if (easyFopValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "EasyFopValidation");
                            analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.First()?.Id);

                            LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    if (isNonUsAgent && !isTidsAgent)
                    {
                        // calling IFG Agent LookUp API to check agent permission

                        IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = null;
                        ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSResponse);

                        var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                        var validateIFGAgentsPermission = OrderChangeValidations.ValidateIFGAgentsPermission(request.OrderChangeRQ, ifgAgentFopLookUpResponse, _hostEnvironment);

                        if (validateIFGAgentsPermission is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "iFGAgentFopLookUp Validation");
                            analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(validateIFGAgentsPermission?.ResponseMessages);
                            return errorDetails;
                        }

                    }

                    var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request.OrderChangeRQ, orderDataRS, offerDoc, isReshopDisruption: isReshopDisruption);
                    var orderManagementMSRS = await _mediator.Send(orderManagementMSRequest, cancellationToken);

                    if (orderManagementMSRS.error is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                        // need to modify after facade layer implementation
                        if (isCashPayment && isNonUsAgent)
                        {
                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId }, cancellationToken);
                        }
                        var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSRS, _hostEnvironment);

                        analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.First()?.Id);

                        LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                        return errorDetails;
                    }

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderManagementMSRS.orderDocument, _hostEnvironment, nDCWarnings));

                    orderManagementMSRS.orderDocument.BookingReference.SplitRLOC = orderDataRS.OrderDocument.BookingReference?.SplitRLOC;

                    OrderDocumentMapper.UpdateOrderItemId(orderManagementMSRS.orderDocument, orderDataRS.OrderDocument);

                    if (isReshopDisruption)
                    {
                        orderManagementMSRS.orderDocument.ChangeType = orderDataRS.OrderDocument.ChangeType ?? orderManagementMSRS.orderDocument.ChangeType;
                    }

                    TransactionServiceRequest transactionRequest = null;
                    TransactionServiceResponse transactionresponse = null;
                    ARCDocumentResponse aRCDocumentResponse = null;

                    if (isCashPayment && isNonUsAgent)
                    {
                        var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, paymentIFGAuthorizeRS, false, false, false);
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

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null, orderDataRS, null, false, null, null, false, true);
                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    if (isEasyPayment && isNonUsAgent)
                    {
                        var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, null, false, false, true);
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

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null, orderDataRS, null, isEasyPayment);
                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    if (isIFGIntegrated && !isCashPayment && !isEasyPayment && isNonUsAgent)
                    {
                        var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, null, false, true, false);
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

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, null, null, orderDataRS, paymentIFGOrderExchangeRS, false, null, null, true);
                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }
                    else if (isCashPayment && !isNonUsAgent)
                    {
                        aRCDocumentResponse = await _orderDataARCQuery.Handle($"{orderManagementMSRS.orderDocument.OrderNumber.OrderId}-ARC", cancellationToken);

                        if (aRCDocumentResponse.Errors != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "aRCDocumentResponse");
                            analyticsEventCommand.Track("ErrorId", aRCDocumentResponse?.Errors?.First()?.Id);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSRS.orderDocument, null, null, Constants.ProductType.Ticket, true, true,false,null,false,null,false,aRCDocumentResponse.ARCDocument);
                        transactionresponse = await _transactionCommand.Handle(transactionRequest);

                        if (transactionresponse.ErrorDetails != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "transactionresponse");
                            analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First()?.Id);
                            nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                        }

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null, orderDataRS, null,false,null,null,false ,true ,transactionresponse);
                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                    }
                    else if (!isCashPayment && !isEasyPayment)
                    {

                        if (!isNonUsAgent)
                        {

                            aRCDocumentResponse = await _orderDataARCQuery.Handle($"{orderManagementMSRS.orderDocument.OrderNumber.OrderId}-ARC", cancellationToken);

                            if (aRCDocumentResponse.Errors != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "aRCDocumentResponse");
                                analyticsEventCommand.Track("ErrorId", aRCDocumentResponse?.Errors?.First()?.Id);

                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            }

                            transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSRS.orderDocument, null, null, Constants.ProductType.Ticket,false,true,false,null,false,null,false,aRCDocumentResponse.ARCDocument);
                            transactionresponse = await _transactionCommand.Handle(transactionRequest);

                            if (transactionresponse.ErrorDetails != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "transactionresponse");
                                analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First()?.Id);
                                nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                            }
                        }

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, null, null, orderDataRS, null, false,null,null,true,false, transactionresponse);
                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                        OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, null, null, false);
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

                            foreach (var (warning, index) in gstValidation.Select((warning, index) => (warning, index)))
                            {
                                analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                            }
                        }

                        var gstWarnings = OrderChangeValidations.ValidateGSTDetails<NDCWarning>(request.OrderChangeRQ, _hostEnvironment);

                        if (gstWarnings.Any())
                        {
                            isGSTValidationFailed = true;
                            nDCWarnings.AddRange(gstWarnings);

                            foreach (var (warning, index) in gstWarnings.Select((warning, index) => (warning, index)))
                            {
                                analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                            }
                        }

                        nDCWarnings.AddRange(OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, orderManagementMSRS.orderDocument));

                        foreach (var (logApis, index) in request.OrderChangeRQ.Request.DataLists.PaxList.Select((logApis, index) => (logApis, index)))
                        {
                            analyticsEventCommand.Track($"PaxID{index + 1}", logApis.PaxID);
                            analyticsEventCommand.Track($"APIS Indicator{index + 1}", (logApis.IdentityDoc != null || logApis.RedressCase != null) ? "Yes" : "No");

                            if (logApis.IdentityDoc != null)
                            {
                                analyticsEventCommand.Track($"Passport{index + 1}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.PT) ? "Yes" : "No");
                                analyticsEventCommand.Track($"Visa{index + 1}", logApis.IdentityDoc.Any(x => x.Visa != null) ? "Yes" : "No");
                                analyticsEventCommand.Track($"Greencard{index + 1}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.GC) ? "Yes" : "No");
                                analyticsEventCommand.Track($"KnownTraveller{index + 1}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.CR) ? "Yes" : "No");
                                analyticsEventCommand.Track($"Redress{index + 1}", logApis.RedressCase != null ? "Yes" : "No");
                            }
                            else if (logApis.RedressCase != null && logApis.IdentityDoc == null)
                            {
                                analyticsEventCommand.Track($"Redress{index + 1}", "Yes");
                            }
                        }
                        analyticsEventCommand.Track($"GST Indicator", validationMSResponse.ValidateResponse.AgencyDetails.CountryCode.Is(Constants.Application.IndiaCountryCode) ? "Yes" : "No");

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
                                nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                            }
                        }
                    }
                    catch
                    {
                        nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                    }

                    if (isNonUsAgent)
                    {
                        if (isCashPayment)
                        {
                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null);
                        }
                        else if (isEasyPayment)
                        {
                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null, isEasyPayment);
                        }
                        else if (isIFGIntegrated)
                        {
                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, null, false, null, null, true);
                        }
                    }

                    OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSRS.orderDocument);

                    var segmentCount = orderManagementMSRS.orderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                    analyticsEventCommand.Track("SegmentCount", segmentCount);

                    string approvalCode = orderDataRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;

                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.EvenExchange, orderManagementMSRS.orderDocument);
                    _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                    var orderchangeResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSRS.orderDocument, nDCWarnings, false, true, false, false, null, null, isCashPayment, true, approvalCode, isReshopDisruption: isReshopDisruption);
                    LogOrderDetails(orderchangeResponse, orderManagementMSRS.orderDocument, analyticsEventCommand);

                    return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };

                }

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeEvenExchangeCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Order Change Query Handler");
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
                    totalAmount += Convert.ToDecimal(orderItem.Price.TotalAmount.Value);
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

        public async Task<Response<IATA_OrderViewRS>> NameCorrectionHandler(OrderChangeEvenExchangeCommand request, OfferResponse offerDoc, TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
        {
            var nDCWarnings = new List<NDCWarning>();

            var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, offerDoc?.Parties?.CorporateAccount, true);
            var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, new CancellationToken());

            if (validationMSResponse.Error != null)
            {
                var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);
                analyticsEventCommand.Track("ServiceName", "ValidationMS");
                analyticsEventCommand.Track("ErrorId", validationMSErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                return ErrorMapper.GetErrorDetails(validationMSErrors.ResponseMessages);
            }

            var orderDataRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

            var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderDataRS, _hostEnvironment);

            if (orderDocErrors?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "OrderDataMS");
                analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors.ResponseMessages);

                return errorDetails;
            }

            var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDataRS.OrderDocument);
            var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

            if (getReservationOrderDoc is not null)
            {
                var sourceCustomers = orderDataRS.OrderDocument.DataList.CustomerDetails;
                var targetCustomers = getReservationOrderDoc.DataList.CustomerDetails;

                if (sourceCustomers?.FirstOrDefault()?.Contacts?.CountryDailingCode is { } countryDailingCode && targetCustomers?.FirstOrDefault()?.Contacts is { } targetContacts)
                {
                    targetContacts.CountryDailingCode = countryDailingCode;
                }

                if (sourceCustomers is not null && targetCustomers is not null)
                {
                    foreach (var customer in sourceCustomers.Where(x => x.IsNameChanged))
                    {
                        var targetCustomer = targetCustomers.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (targetCustomer is not null)
                        {
                            targetCustomer.IsNameChanged = true;
                        }
                    }

                    foreach (var customer in sourceCustomers.Where(x => x.IsApisAdded))
                    {
                        var targetCustomer = targetCustomers.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (targetCustomer is not null)
                        {
                            targetCustomer.IsApisAdded = true;
                        }
                    }
                }

             
                if (orderDataRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderDataRS.OrderDocument.FareTypeRequest;

                foreach (var customer in orderDataRS.OrderDocument.DataList.CustomerDetails)
                {
                    var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails?.FirstOrDefault(x => x.CustId == customer.CustId);

                    if (getReservationOrderDocCustomer != null)
                    {
                        getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                    }
                }

                getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderDataRS.OrderDocument, getReservationOrderDoc);
                IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var brandMsReq = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc);
                    brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);

                }
                else
                {
                    var brandMsReq = BrandMSRequestMapper.MapBrandMSOrderRequest(getReservationOrderDoc);
                    brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                }

                getReservationOrderDoc.ChangeType = orderDataRS.OrderDocument.ChangeType ?? getReservationOrderDoc.ChangeType;
                getReservationOrderDoc.ChangeDateTime = orderDataRS.OrderDocument.ChangeDateTime ?? getReservationOrderDoc.ChangeDateTime;
                getReservationOrderDoc.Version = orderDataRS.OrderDocument.Version;
                getReservationOrderDoc.BookingReference = orderDataRS.OrderDocument.BookingReference;

                getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderDataRS.OrderDocument);

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
            }
            else
            {
                throw new Exception($"SabreOrderDoc not found for orderId: {orderDataRS?.OrderDocument?.OrderNumber?.OrderId}");
            }

            bool isUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode == "US";
            bool isCashPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
            bool isEasyPayment = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;
            bool isCardPayment = !isCashPayment && !isEasyPayment  && isUsAgent;
            bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

            PaymentRS paymentIFGOrderExchangeRS = null;
            TransactionServiceRequest transactionRequest = null;
            TransactionServiceResponse transactionServiceResponse = null;
            ARCDocumentResponse aRCDocumentResponse = null;

            var amount = request.OrderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value.ToString();
            var airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);

            if (isCashPayment)
            {
                var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request.OrderChangeRQ, validationMSResponse, _hostEnvironment);

                if (cashPaymentValidation?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "CashFopValidation");
                    analyticsEventCommand.Track("ErrorId", cashPaymentValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(cashPaymentValidation?.ResponseMessages, analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(cashPaymentValidation.ResponseMessages);
                }
            }

            if (isEasyPayment)
            {
                var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(true, validationMSResponse, _hostEnvironment);

                if (easyFopValidation?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "EasyFopValidation");
                    analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation.ResponseMessages);
                    return errorDetails;
                }

            }
            if (isUsAgent)
            {
                var checkResponse = await _checkAgentStatusQuery.Handle(validationMSResponse.ValidateResponse.AgencyDetails.RequestAgencyId);
                var checkResponseValidation = AgencyValidation.ValidateArcAgentStatus<IATA_OrderViewRS>(checkResponse, _hostEnvironment);

                if (checkResponseValidation?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "ArcAgentStatusValidation");
                    analyticsEventCommand.Track("ErrorId", checkResponseValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(checkResponseValidation?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(checkResponseValidation.ResponseMessages);
                    return errorDetails;
                }
            }

            var orderManagementNameCorrectionRequest = ManageCustomerRequestMapper.MapOrderChangeNameCorrectionRequest(request.OrderChangeRQ, getReservationOrderDoc, offerDoc);
            var orderManagementNameCorrectionResponse = await _manageCustomerRepoCommandHandler.Handle(orderManagementNameCorrectionRequest, cancellationToken);

            if (orderManagementNameCorrectionResponse is null)
            {
                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }

            OrderDocumentMapper.UpdateOrderDocumentNameCorrection(orderManagementNameCorrectionResponse.orderDocument, getReservationOrderDoc);

            if (isCashPayment && !isUsAgent)
            {
                var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeNameCorrectionIFGOrderExchangeRequest(request.OrderChangeRQ, airportDetails, getReservationOrderDoc, orderDataRS, false, false);
                paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                    var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                    var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                    analyticsEventCommand.Track("ErrorId", errorCode);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }
            }

            if (isEasyPayment && !isUsAgent)
            {
                var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeNameCorrectionIFGOrderExchangeRequest(request.OrderChangeRQ, airportDetails, getReservationOrderDoc, orderDataRS, true, false);
                paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                    var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                    var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                    analyticsEventCommand.Track("ErrorId", errorCode);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }
            }

            if (isCashPayment && isUsAgent)
            {
                aRCDocumentResponse = await _orderDataARCQuery.Handle($"{getReservationOrderDoc.OrderNumber.OrderId}-ARC", cancellationToken);

                if (aRCDocumentResponse.Errors != null)
                {
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                transactionRequest = TransactionRequestMapper.MapToTransactionRequest(getReservationOrderDoc, null, null, Constants.ProductType.Ticket, true, false, false, null, false, null, true,aRCDocumentResponse.ARCDocument);
                transactionServiceResponse = await _transactionCommand.Handle(transactionRequest);

                if (transactionServiceResponse.ErrorDetails != null)
                {
                    nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                }
            }

            if (isCardPayment)
            {
                aRCDocumentResponse = await _orderDataARCQuery.Handle($"{getReservationOrderDoc.OrderNumber.OrderId}-ARC", cancellationToken);

                if (aRCDocumentResponse.Errors != null)
                {
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                transactionRequest = TransactionRequestMapper.MapToTransactionRequest(getReservationOrderDoc, null, null, Constants.ProductType.Ticket, false, false, false, null, false, null, true,aRCDocumentResponse.ARCDocument);
                transactionServiceResponse = await _transactionCommand.Handle(transactionRequest);

                if (transactionServiceResponse.ErrorDetails != null)
                {
                    nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                }

            }

            if (isIFGIntegrated && !isCashPayment && !isEasyPayment && !isUsAgent)
            {
                var paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeNameCorrectionIFGOrderExchangeRequest(request.OrderChangeRQ, airportDetails, getReservationOrderDoc, orderDataRS, false, true);
                paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentIFGOrderExchange");
                    var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                    var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                    analyticsEventCommand.Track("ErrorId", errorCode);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }
            }

            if (isCashPayment || isEasyPayment || (isCashPayment && isUsAgent) || (isIFGIntegrated && !isUsAgent) || (isCardPayment))
            {
                OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(getReservationOrderDoc, null, null, orderDataRS, paymentIFGOrderExchangeRS, isEasyPayment, orderManagementNameCorrectionResponse.orderDocument, null, isCardPayment,isCashPayment ,transactionServiceResponse);

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
            }

            OrderDocumentMapper.UpdateOrderItemNameCorrection(orderManagementNameCorrectionResponse.orderDocument, getReservationOrderDoc);

            string approvalCode = orderDataRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;

            var orderchangeResponse = OrderResponseMapper.MapOrderResponse(getReservationOrderDoc, nDCWarnings, false, false, false, true, null, null, isCashPayment, isEasyPayment, approvalCode);

            LogOrderDetails(orderchangeResponse, getReservationOrderDoc, analyticsEventCommand);

            return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
        }
    }
}
