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
using VAA.EAPI.Common.Serialisation;
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
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
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
    public class OrderChangeAddColCommandHandler : IRequestHandler<OrderChangeAddColCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeAddColCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSExchangeCommandHandler _paymentIFGMSExchangeCommand;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly ITransactionCommand _transactionCommand;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly IARCOrderDataMSQuery _orderDataARCQuery;
        private readonly IOrderChangeReshopConfirmHoldBookingCommand _orderChangeReshopConfirmHoldBookingCommand;

        public OrderChangeAddColCommandHandler(IMediator mediator, ILogger<OrderChangeAddColCommandHandler> logger, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSExchangeCommandHandler paymentIFGMSExchangeCommand, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ICurrencyConversionQuery currencyConversionQueryHandler, ICheckAgentStatusQuery checkAgentStatusQuery, ITransactionCommand transactionCommand, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, IARCOrderDataMSQuery orderDataARCQuery, IOrderChangeReshopConfirmHoldBookingCommand orderChangeReshopConfirmHoldBookingCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSExchangeCommand = paymentIFGMSExchangeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSExchangeCommand));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _orderDataARCQuery = orderDataARCQuery ?? throw new ArgumentNullException(nameof(orderDataARCQuery));
            _orderChangeReshopConfirmHoldBookingCommand = orderChangeReshopConfirmHoldBookingCommand ?? throw new ArgumentNullException(nameof(orderChangeReshopConfirmHoldBookingCommand));
        }

        public async Task<Response<IATA_OrderViewRS>> Handle(OrderChangeAddColCommand request, CancellationToken cancellationToken)
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

                analyticsEventCommand.Track("RequestType", "OrderChange_AddCollect");
                var paymentReq = request.OrderChangeRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
                var paymentMethodData = (paymentReq?.PaymentMethod?.Item) as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType;
                var txn_Type = paymentMethodData?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();
                bool motoPayment = txn_Type == "MO" || txn_Type == "TO" ? true : false;

                var distributionChain = request.OrderChangeRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeRequest<IATA_OrderViewRS>(request.OrderChangeRQ, motoPayment, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOrderChangeRequest");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                string offerID = (request.OrderChangeRQ.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;
                var offerDoc = await _mediator.Send((OfferMSQuery)offerID, new CancellationToken());

                var paymentFunctionErrors = offerDoc?.DocType == Constants.Application.OrderReshopDisruption
                    ? OrderChangeValidations.ValidateDisruptionPaymentFunctions<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment)
                    : null;

                if (paymentFunctionErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "DisruptionPaymentFunctionsValidation");
                    analyticsEventCommand.Track("ErrorId", paymentFunctionErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(paymentFunctionErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(paymentFunctionErrors?.ResponseMessages);
                    return errorDetails;
                }

                bool isReshopHoldBooking = (offerDoc != null && offerDoc.DocType == Constants.Application.OrderChangeReshopHoldBooking);

                if (isReshopHoldBooking)
                {
                    var result = await _orderChangeReshopConfirmHoldBookingCommand.Handle(request.OrderChangeRQ, request.SubscriptionKey, cancellationToken);
                    return result.Value;
                }
                else
                {
                    analyticsEventCommand.Track("offerID", offerID);
                    analyticsEventCommand.Track("PosCity", offerDoc.Pos?.CityCode);

                    var dbErrors = OrderChangeValidations.ValidateDbOffer(offerDoc, offerID, _hostEnvironment);

                    if (dbErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ValidateDbOffer");
                        analyticsEventCommand.Track("ErrorId", dbErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(dbErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(dbErrors?.ResponseMessages);
                        return errorDetails;
                    }


                    var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, offerDoc?.Parties?.CorporateAccount, true);
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

                    var paymentRq = request.OrderChangeRQ.Request.PaymentFunctions?.First().PaymentProcessingDetails;
                    bool isCashPayment = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode == "CA";
                    bool isEasyPay = request.OrderChangeRQ?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is PaymentProcessingIATA_EasyPayType;

                    var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, validationMSResponse, offerDoc, isCashPayment || isEasyPay, isEvenExchange: false, isUnEvenExchangeRefund: false, isUnEvenExchangeAddCollect: true);

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

                    Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse = new();

                    var serviceListOfferId = ((AcceptSelectedQuotedOfferListType)request.OrderChangeRQ?.Request?.ChangeOrderChoice?.Item)?.SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem != null && !((PaxSegmentRefListType)x.SelectedALaCarteOfferItem.OfferFlightAssociations.Item).PaxSegmentRefID.IsNullOrEmpty()));

                    if (serviceListOfferId != null)
                    {
                        flightServiceListResponse = await _mediator.Send((FlightServiceOfferDocQuery)serviceListOfferId.OfferRefID, cancellationToken);
                        var flightServiceValidationErrors = OrderChangeValidations.ValidateFlightServiceList<IATA_OrderViewRS>(request.OrderChangeRQ, flightServiceListResponse, _hostEnvironment);

                        if (flightServiceValidationErrors != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "FlightServiceMS");
                            analyticsEventCommand.Track("ErrorId", flightServiceValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(flightServiceValidationErrors?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(flightServiceValidationErrors?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    var orderChangeRQErrors = OrderChangeValidations.ValidateEvenOrderChangeRQ<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (orderChangeRQErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeRQValidation");
                        analyticsEventCommand.Track("ErrorId", orderChangeRQErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(orderChangeRQErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderChangeRQErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    var OfferDocErros = OrderChangeValidations.ValidateEvenOrderChangeRQAgainstOfferDoc<IATA_OrderViewRS>(request.OrderChangeRQ, offerDoc, _hostEnvironment);

                    if (OfferDocErros?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EvenOrderChangeOfferDocValidation");
                        analyticsEventCommand.Track("ErrorId", OfferDocErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(OfferDocErros?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(OfferDocErros?.ResponseMessages);
                        return errorDetails;
                    }

                    var orderDataRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

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

                    var formOfPaymentValidationErrors = OrderChangeValidations.ValidateOrderChangeFormOfPayment<IATA_OrderViewRS>(request.OrderChangeRQ, orderDataRS, _hostEnvironment, isPostSaleSeat: false);

                    if (formOfPaymentValidationErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "FormOfPaymentValidation");
                        analyticsEventCommand.Track("ErrorId", formOfPaymentValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(formOfPaymentValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(formOfPaymentValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    //Add Collect
                    if (!isNonUsAgent)
                    {
                        var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);
                        OrderDocumentMapper.CurrencyConversion(offerDoc, currencyExchangeValue);

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

                    var currencyRS = await _mediator.Send(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);

                    if (string.IsNullOrWhiteSpace(currencyRS))
                    {
                        analyticsEventCommand.Track("ServiceName", "ReferenceMSCurrencyFailed");
                    }

                    var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);

                    PaymentMSAuthorizeCommand paymentMSAuthorizeRequest = null;
                    PaymentRS paymentMSAuthorizeRS = null;
                    PaymentRS paymentIFGAuthorizeRS = null;
                    PaymentIFGMSAuthorizeCommand paymentIFGAuthorizeRQ = null;
                    PaymentIFGMSExchangeCommand paymentIFGOrderExchangeRQ = null;
                    PaymentRS paymentIFGOrderExchangeRS = null;
                    string approvalCode = null;

                    var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDataRS.OrderDocument);
                    var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken) ?? throw new Exception($"SabreOrderDoc not found for orderId: {orderDataRS?.OrderDocument?.OrderNumber?.OrderId}");
                    getReservationOrderDoc.PointOfSale = orderDataRS.OrderDocument.PointOfSale;
                    if (orderDataRS.OrderDocument.DataList.CustomerDetails.First().Contacts?.CountryDailingCode != null)
                    {
                    if(getReservationOrderDoc.DataList.CustomerDetails.First().Contacts != null)
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDataRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                        }
                    }

                    bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

                    if (isCashPayment)
                    {
                        var cashPaymentTypeCode = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode;

                        if (cashPaymentTypeCode != null)
                        {
                            var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request.OrderChangeRQ, validationMSResponse, _hostEnvironment);

                            if (cashPaymentValidation?.ResponseMessages != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "ValidateOrderChangeFOPCash");
                                analyticsEventCommand.Track("ErrorId", cashPaymentValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(cashPaymentValidation?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(cashPaymentValidation?.ResponseMessages);
                                return errorDetails;
                            }
                        }
                    }

                    if (isEasyPay)
                    {
                        var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(isEasyPay, validationMSResponse, _hostEnvironment);

                        if (easyFopValidation?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "ValidateEasyFop");
                            analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);

                    if (airportDetails?.Any() != true)
                    {
                        analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    if (isCashPayment && isNonUsAgent)
                    {
                        paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request.OrderChangeRQ, offerDoc, airportDetails, getReservationOrderDoc);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGAuthorizeRS");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);
                            var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);

                            analyticsEventCommand.Track("OrderCancelStatus", true);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }
                    }

                    else if (isEasyPay && isNonUsAgent)
                    {
                        paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request.OrderChangeRQ, offerDoc, airportDetails, getReservationOrderDoc, false, true);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        approvalCode = paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode;

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGAuthorizeRS");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);
                            var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);

                            analyticsEventCommand.Track("OrderCancelStatus", true);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }
                    }
                    else if(!isCashPayment && !isEasyPay)
                    {
                        var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq?.PaymentMethod?.Item);
                        var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

                        analyticsEventCommand.Track("PaymentTypeCode", request?.OrderChangeRQ?.Request?.PaymentFunctions?.First().PaymentMethodCriteria?.PaymentTypeCode);

                        analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                        analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');

                        if (isIFGIntegrated && isNonUsAgent)
                        {
                            paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request.OrderChangeRQ, offerDoc, airportDetails, getReservationOrderDoc, true);
                            paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                            if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "PaymentIFGAuthorizeRS");
                                var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);
                                var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                                await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);

                                analyticsEventCommand.Track("OrderCancelStatus", true);

                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            }
                        }
                        else
                        {
                            paymentMSAuthorizeRequest = PaymentMSRequestMapper.MapOrderChangeAuthorizeRequest(getReservationOrderDoc, request.OrderChangeRQ, currencies, offerDoc , airportDetails);
                            paymentMSAuthorizeRS = await _mediator.Send(paymentMSAuthorizeRequest, cancellationToken);

                            if (paymentMSAuthorizeRS.Errors is not null)
                            {
                                var paymentMSAuthorizeErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSAuthorizeRS, _hostEnvironment);

                                analyticsEventCommand.Track("ServiceName", "PaymentMSAuthorize");
                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);
                                return errorDetails;
                            }
                        }

                    }

                    bool isTidsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.RequestAgencyIdType == "TIDS";

                    // calling IFG Agent LookUp API to check agent permission
                    if (isNonUsAgent && !isTidsAgent)
                    {
                        IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = null;
                        ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSResponse);

                        var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                        var validateIFGAgentsPermission = OrderChangeValidations.ValidateIFGAgentsPermission(request.OrderChangeRQ, ifgAgentFopLookUpResponse, _hostEnvironment);

                        if (validateIFGAgentsPermission is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "IFGAgentFopLookUp Validation");
                            analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(validateIFGAgentsPermission?.ResponseMessages);
                            return errorDetails;
                        }

                    }

                    var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request.OrderChangeRQ, orderDataRS, offerDoc, ((isCashPayment || isEasyPay) ||(!isCashPayment && !isEasyPay && isNonUsAgent)) ? paymentIFGAuthorizeRS?.PaymentDocument?.TrackId : paymentMSAuthorizeRS.PaymentDocument.TransactionId);
                    var orderManagementMSRS = await _mediator.Send(orderManagementMSRequest, cancellationToken);
                    if (orderManagementMSRS.error is not null)
                    {
                        // to modify after facade layer implementation
                        if (isCashPayment || isEasyPay || (isIFGIntegrated && !isEasyPay && !isCashPayment && isNonUsAgent))
                        {
                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS?.PaymentDocument?.TrackId }, cancellationToken);
                            analyticsEventCommand.Track("OrderCancelStatus", true);
                        }
                        else
                        {
                            var paymentMSRefundRequest = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderDataRS, request.OrderChangeRQ, orderManagementMSRS?.orderDocument?.OrderItems.First().AccountableDocumentRefs, paymentMSAuthorizeRS, paymentMSAuthorizeRequest);
                            var paymentMSRefundRS = await _mediator.Send(paymentMSRefundRequest, cancellationToken);
                            analyticsEventCommand.Track("OrderRefundStatus", true);
                        }


                        analyticsEventCommand.Track("Delta_PNR", orderManagementMSRS?.orderDocument?.BookingReference?.ExternalRecordLocator);

                        var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSRS, _hostEnvironment);

                        analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                        analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                        return errorDetails;
                    }

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderManagementMSRS.orderDocument, _hostEnvironment, nDCWarnings));

                    orderManagementMSRS.orderDocument.BookingReference.SplitRLOC = orderDataRS.OrderDocument.BookingReference?.SplitRLOC;

                    OrderDocumentMapper.UpdateOrderItemId(orderManagementMSRS.orderDocument, orderDataRS.OrderDocument);
                    PaymentRS paymentDocumentResponse = null;
                    PaymentTransactionDocument paymentTransactionDocument = null;
                    TransactionServiceRequest transactionRequest = null;
                    TransactionServiceResponse transactionresponse = null;
                    ARCDocumentResponse aRCDocumentResponse = null;

                    if (isCashPayment && isNonUsAgent)
                    {
                        paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, paymentIFGAuthorizeRS, false, false, false);
                        paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                        if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGMSOrderExchange");
                            var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderManagementMSRS.orderDocument, Constants.Application.PaymentDocType, true);
                        paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                            OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, orderDataRS);

                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    else if (isEasyPay && isNonUsAgent)
                    {
                        paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, paymentIFGAuthorizeRS, false, false, true);
                        paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                        if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGMSOrderExchange");
                            var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        var easyPayCardNumber = (request.OrderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item as PaymentProcessingIATA_EasyPayType).AccountNumber;
                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderManagementMSRS.orderDocument, Constants.Application.PaymentDocType, true, false, true, null, null, easyPayCardNumber);
                        paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                            OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, orderDataRS, null, true, null, null, false, false, null, paymentDocumentRequest.PaymentDetails.First().CardMethod);

                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    else if (isCashPayment && !isNonUsAgent)
                    {
                        aRCDocumentResponse = await _orderDataARCQuery.Handle($"{orderManagementMSRS.orderDocument.OrderNumber.OrderId}-ARC", cancellationToken);

                        if (aRCDocumentResponse.Errors != null)
                        {
                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }

                        transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSRS.orderDocument, null, null, Constants.ProductType.Ticket,true,false, false,null,false,null,false,aRCDocumentResponse.ARCDocument,true);

                        transactionresponse = await _transactionCommand.Handle(transactionRequest);

                        if (transactionresponse.ErrorDetails != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "transactionresponse");
                            analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.FirstOrDefault()?.Id);
                            nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                        }

                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderManagementMSRS.orderDocument, Constants.Application.PaymentDocType, true,false,false,transactionRequest ,transactionresponse);
                        paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, orderDataRS ,null ,false, null ,null,false,true, transactionresponse);

                        orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                    }

                    else if(!isCashPayment && !isEasyPay)
                    {
                        PaymentRS paymentCaptureRS = null;

                        if (isIFGIntegrated && isNonUsAgent)
                        {
                            paymentIFGOrderExchangeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGOrderExchangeRequest(request.OrderChangeRQ, offerDoc, airportDetails, orderManagementMSRS.orderDocument, orderDataRS.OrderDocument, paymentIFGAuthorizeRS, false, true, false);
                            paymentIFGOrderExchangeRS = await _paymentIFGMSExchangeCommand.Handle(paymentIFGOrderExchangeRQ, cancellationToken);

                            if (paymentIFGOrderExchangeRS?.Errors != null && paymentIFGOrderExchangeRS.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "PaymentIFGMSOrderExchange");
                                var message = paymentIFGOrderExchangeRS?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);

                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            }

                            var paymentDocs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery()
                            {
                                OrderID = orderManagementMSRS.orderDocument.OrderNumber.OrderId
                            }, cancellationToken);

                            if (paymentDocs != null)
                            {
                                paymentTransactionDocument = paymentDocs.FirstOrDefault(x => x.Id == paymentIFGAuthorizeRS.PaymentDocument.TransactionId);
                                OrderDocumentMapper.UpdateExchangeCCPaymentDetails(orderManagementMSRS.orderDocument, paymentTransactionDocument, paymentIFGAuthorizeRS.PaymentDocument.TrackId,orderDataRS.OrderDocument);
                            }

                            orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                            var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                            await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                        }

                        else
                        {
                            var paymentMSCaptureRequest = PaymentMSRequestMapper.MapOrderChangeCaptureRequest(paymentMSAuthorizeRequest, orderManagementMSRS.orderDocument, paymentMSAuthorizeRS, offerDoc);
                            paymentCaptureRS = await _mediator.Send(paymentMSCaptureRequest, cancellationToken);

                            if (paymentCaptureRS.Errors is not null)
                            {
                                var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentCaptureRS, _hostEnvironment);

                                var paymentMSRefundRequest = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderDataRS, request.OrderChangeRQ, orderManagementMSRS?.orderDocument.OrderItems.First().AccountableDocumentRefs, paymentMSAuthorizeRS, paymentMSAuthorizeRequest);
                                var paymentMSRefundRS = await _mediator.Send(paymentMSRefundRequest, cancellationToken);

                                analyticsEventCommand.Track("ServiceName", "PaymentMSCapture");
                                analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);
                                return errorDetails;
                            }

                            if (!isNonUsAgent)
                            {
                                aRCDocumentResponse = await _orderDataARCQuery.Handle($"{orderManagementMSRS.orderDocument.OrderNumber.OrderId}-ARC", cancellationToken);

                                if (aRCDocumentResponse.Errors != null)
                                {
                                    analyticsEventCommand.Track("ServiceName", "aRCDocumentResponse");
                                    analyticsEventCommand.Track("ErrorId", aRCDocumentResponse?.Errors?.FirstOrDefault()?.Id);

                                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                }

                            transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSRS.orderDocument, paymentCaptureRS, null, Constants.ProductType.Ticket, false, false, false,null,false,null,false, aRCDocumentResponse.ARCDocument ,true);

                                transactionresponse = await _transactionCommand.Handle(transactionRequest);

                                if (transactionresponse.ErrorDetails != null)
                                {
                                    analyticsEventCommand.Track("ServiceName", "transactionresponse");
                                    analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.FirstOrDefault()?.Id);
                                    nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                                }
                            }

                        OrderDocumentMapper.UpdateExchangeFOPPaymentDetails(orderManagementMSRS.orderDocument, paymentMSAuthorizeRS, null, orderDataRS, null, false, null, paymentCaptureRS ,true, false , transactionresponse);

                            orderManagementMSRS.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                            var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSRS.orderDocument };
                            await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentMSAuthorizeRS, null, false, paymentCaptureRS);

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
                        analyticsEventCommand.Track("GST Indicator", validationMSResponse.ValidateResponse.AgencyDetails.CountryCode.Is(Constants.Application.IndiaCountryCode) ? "Yes" : "No");

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
                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument);
                        }
                        else if (isEasyPay)
                        {
                            OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSRS.orderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, true);
                        }
                        else if (isIFGIntegrated)
                        {
                            OrderDocumentMapper.UpdateCCPaymentDetails(orderManagementMSRS.orderDocument, paymentTransactionDocument, paymentIFGAuthorizeRS.PaymentDocument.TrackId, true);
                        }
                    }

                    OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSRS.orderDocument);

                    var segmentCount = orderManagementMSRS.orderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                    analyticsEventCommand.Track("SegmentCount", segmentCount);

                    var addCollectDifferenceAmount = orderManagementMSRS.orderDocument.OrderItems
                    .Where(x => x.AddCollectPrice?.Amount?.MonetaryAmount != null)
                    .Sum(x =>
                    {
                        var beneficiariesCount = x?.Services?.Where(s => s?.ServiceBeneficiaries != null)?.SelectMany(s => s.ServiceBeneficiaries)?.Distinct()?.Count();
                        return Convert.ToDecimal(x.AddCollectPrice.Amount.MonetaryAmount) * beneficiariesCount;
                    });

                    analyticsEventCommand.Track("AddColDifferenceAmount", addCollectDifferenceAmount);

                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.OrderChange, orderManagementMSRS.orderDocument);
                    _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                    var orderchangeResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSRS.orderDocument, nDCWarnings, false, true, false, false, null, null, isCashPayment, isEasyPay, approvalCode);

                    LogOrderDetails(orderchangeResponse, orderManagementMSRS.orderDocument, analyticsEventCommand);

                    return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
                }

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeAddColCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Change Add_Col Command Handler");
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

                    analyticsEventCommand.Track($"O&D{index + 1}Cabin", orderDocument.DataList?.PriceClass?.FirstOrDefault(x => x.Id == priceClassRefId)?.CabinType?.CabinName);
                    analyticsEventCommand.Track($"O&D{index + 1}FareBrand", orderDocument.DataList?.PriceClass?.FirstOrDefault(x => x.Id == priceClassRefId)?.ProductName);
                }

                var paidSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0);

                if (paidSeatCount > 0)
                {
                    var paidSeatAmount = orderDocument.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.Sum(x => Convert.ToDecimal(x.PriceDetails.First(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));
                    analyticsEventCommand.Track("PaidSeatAmount", paidSeatAmount);
                }

                var freeSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) == 0);

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
