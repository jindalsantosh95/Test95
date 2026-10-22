using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
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

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeReshopConfirmHoldBookingCommandHandler : IOrderChangeReshopConfirmHoldBookingCommand
    {
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OfferMSQueryHandler _offerMSQueryHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<OrderChangeReshopConfirmHoldBookingCommandHandler> _logger;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly SabreOrderManagementMSCommandHandler _sabreOrderManagementMSCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly ReferenceMSCurrencyQueryHandler _referenceMSCurrencyQueryHandler;
        private readonly PaymentMSAuthorizeCommandHandler _paymentMSAuthorizeCommandHandler;
        private readonly PaymentMSRefundCommandHandler _paymentMSRefundCommandHandler;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PaymentMSCaptureCommandHandler _paymentMSCaptureCommandHandler;
        private readonly ITransactionCommand _transactionCommand;
        private readonly ManageCustomerRepoCommandHandler _manageCustomerRepoCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;

        public OrderChangeReshopConfirmHoldBookingCommandHandler(ValidationMSCommandHandler validationMSCommandHandler, OfferMSQueryHandler offerMSQueryHandler, IHostEnvironment hostEnvironment, ILogger<OrderChangeReshopConfirmHoldBookingCommandHandler> logger, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, OrderDataMSQueryHandler orderDataMSQueryHandler, ICurrencyConversionQuery currencyConversionQueryHandler, SabreOrderManagementMSCommandHandler sabreOrderManagementMSCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ICheckAgentStatusQuery checkAgentStatusQuery, IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, AirportDetailsQueryHandler airportDetailsQueryHandler, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, OrderSearchMSCommandHandler orderSearchMSCommandHandler, ReferenceMSCurrencyQueryHandler referenceMSCurrencyQueryHandler, PaymentMSAuthorizeCommandHandler paymentMSAuthorizeCommandHandler, PaymentMSRefundCommandHandler paymentMSRefundCommandHandler, PaymentIFGMSOrderCommandHandler paymentIFGMSOrderCommandHandler, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PaymentMSCaptureCommandHandler paymentMSCaptureCommandHandler, ITransactionCommand transactionCommand, ManageCustomerRepoCommandHandler manageCustomerRepoCommandHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler)
        {
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _offerMSQueryHandler = offerMSQueryHandler ?? throw new ArgumentNullException(nameof(offerMSQueryHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _sabreOrderManagementMSCommandHandler = sabreOrderManagementMSCommandHandler ?? throw new ArgumentNullException(nameof(sabreOrderManagementMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _airportDetailsQueryHandler = airportDetailsQueryHandler ?? throw new ArgumentNullException(nameof(airportDetailsQueryHandler));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _referenceMSCurrencyQueryHandler = referenceMSCurrencyQueryHandler ?? throw new ArgumentNullException(nameof(referenceMSCurrencyQueryHandler));
            _paymentMSAuthorizeCommandHandler = paymentMSAuthorizeCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSAuthorizeCommandHandler));
            _paymentMSRefundCommandHandler = paymentMSRefundCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSRefundCommandHandler));
            _paymentIFGMSOrderCommandHandler = paymentIFGMSOrderCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderCommandHandler));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _paymentMSCaptureCommandHandler = paymentMSCaptureCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSCaptureCommandHandler));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _manageCustomerRepoCommandHandler = manageCustomerRepoCommandHandler ?? throw new ArgumentNullException(nameof(manageCustomerRepoCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, string subscriptionKey, CancellationToken cancellationToken)
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
                string offerID = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType)
                    .SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;
                var offerDoc = await _offerMSQueryHandler.Handle((OfferMSQuery)offerID, cancellationToken);

                var dbErrors = OrderChangeValidations.ValidateDbOffer(offerDoc, offerID, _hostEnvironment);

                if (dbErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferStoreValidation");
                    analyticsEventCommand.Track("ErrorId", dbErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(dbErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(dbErrors?.ResponseMessages);
                    return errorDetails;
                }

                var amountValidationErrors = OrderChangeValidations.ValidateChangeReshopHoldBooking<IATA_OrderViewRS>(request, offerDoc, _hostEnvironment);
                if (amountValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "AmountValidationErrors");
                    analyticsEventCommand.Track("ErrorId", amountValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(amountValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(amountValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, offerDoc?.Parties?.CorporateAccount, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, cancellationToken);
                bool isNonUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode != "US";
                bool isTidsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.RequestAgencyIdType == "TIDS";

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

                var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request, _hostEnvironment, validationMSResponse, offerDoc);

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

                var orderChangeRQErrors = OrderChangeValidations.ValidateEvenOrderChangeRQ<IATA_OrderViewRS>(request, offerDoc, _hostEnvironment);

                if (orderChangeRQErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "EvenOrderChangeRQValidation");
                    analyticsEventCommand.Track("ErrorId", orderChangeRQErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderChangeRQErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderChangeRQErrors?.ResponseMessages);
                    return errorDetails;
                }

                var OfferDocErros = OrderChangeValidations.ValidateEvenOrderChangeRQAgainstOfferDoc<IATA_OrderViewRS>(request, offerDoc, _hostEnvironment);

                if (OfferDocErros?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "EvenOrderChangeOfferDocValidation");
                    analyticsEventCommand.Track("ErrorId", OfferDocErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(OfferDocErros?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(OfferDocErros?.ResponseMessages);
                    return errorDetails;
                }

                ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsResponseListPTC = new();

                if (offerDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var tasks = offerDoc.Offers.Select(async offer =>
                    {
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSPTCRequest(offer, offerDoc);
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
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSRequest(offer, offerDoc);
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

                var paymentStatusValidationErrors = OrderChangeValidations.ValidatePaymentStatus(orderDataRS, _hostEnvironment);

                if (paymentStatusValidationErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentStatusValidation");
                    analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var formOfPaymentValidationErrors = OrderChangeValidations.ValidateOrderChangeFormOfPayment<IATA_OrderViewRS>(request, orderDataRS, _hostEnvironment, isPostSaleSeat: false);

                if (formOfPaymentValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "FormOfPaymentValidation");
                    analyticsEventCommand.Track("ErrorId", formOfPaymentValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(formOfPaymentValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(formOfPaymentValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

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

                bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));
                var paymentRq = request.Request.PaymentFunctions?.First().PaymentProcessingDetails;
                bool isCashPayment = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode == "CA";
                bool isEasyPay = request?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is PaymentProcessingIATA_EasyPayType;

                if (isCashPayment)
                {
                    var cashPaymentTypeCode = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode;

                    if (cashPaymentTypeCode != null)
                    {
                        var cashPaymentValidation = AgencyValidation.ValidateOrderChangeFOPCash<IATA_OrderViewRS>(request, validationMSResponse, _hostEnvironment);

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

                // calling IFG Agent LookUp API to check agent permission
                if (isNonUsAgent && !isTidsAgent)
                {
                    IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = null;
                    ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSResponse);

                    var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                    var validateIFGAgentsPermission = OrderChangeValidations.ValidateIFGAgentsPermission(request, ifgAgentFopLookUpResponse, _hostEnvironment);
                    if (validateIFGAgentsPermission is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "IFGAgentFopLookUp Validation");
                        analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(validateIFGAgentsPermission?.ResponseMessages);
                        return errorDetails;
                    }

                }

                var currencyRS = await _referenceMSCurrencyQueryHandler.Handle(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);

                if (string.IsNullOrWhiteSpace(currencyRS))
                {
                    analyticsEventCommand.Track("ServiceName", "ReferenceMSCurrencyFailed");
                }

                IEnumerable<AirportDetail> airportDetails = null;
                airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);

                if (airportDetails?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }


                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDataRS.OrderDocument);
                var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                getReservationOrderDoc.PointOfSale = orderDataRS.OrderDocument.PointOfSale;

                if (orderDataRS.OrderDocument.DataList.CustomerDetails.First().Contacts?.CountryDailingCode != null)
                {
                    if (getReservationOrderDoc.DataList.CustomerDetails.First().Contacts != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDataRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }
                }

                var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);

                PaymentMSAuthorizeCommand paymentMSAuthorizeRequest = null;
                PaymentRS paymentMSAuthorizeRS = null;
                PaymentIFGMSAuthorizeCommand paymentIFGAuthorizeRQ = null;
                PaymentRS paymentIFGAuthorizeRS = null;
                PaymentIFGMSOrderCommand paymentIFGOrderRQ = null;
                PaymentRS paymentIFGOrderRS = null;
                PaymentRS paymentCaptureRS = null;
                string approvalCode = null;
                TransactionServiceRequest transactionRequest = null;
                TransactionServiceResponse transactionresponse = null;

               if (isCashPayment && isNonUsAgent)
                {       
                    paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request, offerDoc, airportDetails, getReservationOrderDoc, false, false, true);
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
                    paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request, offerDoc, airportDetails, getReservationOrderDoc, false, true, true);
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

                else if (!isCashPayment && !isEasyPay)
                {
                    var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq?.PaymentMethod?.Item);
                    var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

                    analyticsEventCommand.Track("PaymentTypeCode", request?.Request?.PaymentFunctions?.First().PaymentMethodCriteria?.PaymentTypeCode);

                    analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                    analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');

                    if (isIFGIntegrated && isNonUsAgent)
                    {           
                        paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeExchangeIFGAuthorizeRequest(request, offerDoc, airportDetails, getReservationOrderDoc, true, false, true);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        if (paymentIFGAuthorizeRS.PaymentDocument.HttpStatus != 200)
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGAuthorizeRS");
                            analyticsEventCommand.Track("StatusCode", paymentIFGAuthorizeRS.PaymentDocument.HttpStatus);
                            analyticsEventCommand.Track("ErrorId", paymentIFGAuthorizeRS.Errors?.FirstOrDefault()?.Id);

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId }, cancellationToken);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }
                    }
                    else
                    {
                        paymentMSAuthorizeRequest = PaymentMSRequestMapper.MapOrderChangeAuthorizeRequest(getReservationOrderDoc, request, currencies, offerDoc, airportDetails);
                        paymentMSAuthorizeRS = await _paymentMSAuthorizeCommandHandler.Handle(paymentMSAuthorizeRequest, cancellationToken);

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

                var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request, orderDataRS, offerDoc, null, false, true);
                var orderManagementMSResponse = await _sabreOrderManagementMSCommandHandler.Handle(orderManagementMSRequest, cancellationToken);

                if (orderManagementMSResponse.error is not null)
                {
                    if (isCashPayment || isEasyPay || (isIFGIntegrated && !isEasyPay && !isCashPayment && isNonUsAgent))
                    {
                        await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS?.PaymentDocument?.TrackId }, cancellationToken);
                        analyticsEventCommand.Track("OrderCancelStatus", true);

                    }
                    else
                    {
                        var paymentMSRefundRequest = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderDataRS, request, orderManagementMSResponse?.orderDocument?.OrderItems.First().AccountableDocumentRefs, paymentMSAuthorizeRS, paymentMSAuthorizeRequest);
                        var paymentMSRefundRS = await _paymentMSRefundCommandHandler.Handle(paymentMSRefundRequest, cancellationToken);
                        analyticsEventCommand.Track("OrderRefundStatus", true);

                    }

                    var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                    analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                    return errorDetails;
                }

                if (isCashPayment && isNonUsAgent)
                {
                    paymentIFGOrderRQ = PaymentMSRequestMapper.MapIFGOrderChangeReshopHoldBooking(paymentIFGAuthorizeRQ, paymentIFGAuthorizeRS, orderManagementMSResponse.orderDocument, offerDoc, true);
                    paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

                    if (paymentIFGOrderRS?.Errors != null && paymentIFGOrderRS.Errors.Any())
                    {
                        analyticsEventCommand.Track("ServiceName", "PaymentIFGOrder");
                        var message = paymentIFGOrderRS?.Errors?.FirstOrDefault()?.Message;
                        var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                        analyticsEventCommand.Track("ErrorId", errorCode);

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        return errorDetails;
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGOrderRS, paymentIFGAuthorizeRQ, orderManagementMSResponse.orderDocument, Constants.Application.PaymentDocType);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSResponse.orderDocument, paymentIFGOrderRS, paymentDocumentResponse.PaymentDocument);

                }
                else if (isCashPayment && !isNonUsAgent)
                {
                    transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSResponse.orderDocument, paymentCaptureRS, null, Constants.ProductType.Ticket, true);

                    transactionresponse = await _transactionCommand.Handle(transactionRequest);

                    if (transactionresponse.ErrorDetails != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "transactionresponse");
                        analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First().Id);
                        nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(null, null, orderManagementMSResponse.orderDocument, Constants.Application.PaymentDocType, false, false, false, transactionRequest, transactionresponse);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSResponse.orderDocument, null, paymentDocumentResponse.PaymentDocument, false, null, null, false, transactionresponse);

                }
                else if (isEasyPay && isNonUsAgent)
                {
                    paymentIFGOrderRQ = PaymentMSRequestMapper.MapIFGOrderChangeReshopHoldBooking(paymentIFGAuthorizeRQ, paymentIFGAuthorizeRS, orderManagementMSResponse.orderDocument, offerDoc, true);
                    paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

                    var easyPayCardNumber = (request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item as PaymentProcessingIATA_EasyPayType).AccountNumber;

                    if (paymentIFGOrderRS?.Errors != null && paymentIFGOrderRS.Errors.Any())
                    {
                        analyticsEventCommand.Track("ServiceName", "PaymentIFGOrder");
                        var message = paymentIFGOrderRS?.Errors?.FirstOrDefault()?.Message;
                        var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                        analyticsEventCommand.Track("ErrorId", errorCode);

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        return errorDetails;
                    }

                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGOrderRS, paymentIFGAuthorizeRQ, orderManagementMSResponse.orderDocument, Constants.Application.PaymentDocType, false, false, true, null, null, easyPayCardNumber);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSResponse.orderDocument, paymentIFGOrderRS, paymentDocumentResponse.PaymentDocument, true, null, paymentIFGOrderRS);

                }
                else if (!isCashPayment && !isEasyPay)
                {
                    if (isIFGIntegrated && isNonUsAgent)
                    {
                        paymentIFGOrderRQ = PaymentMSRequestMapper.MapIFGOrderChangeReshopHoldBooking(paymentIFGAuthorizeRQ, paymentIFGAuthorizeRS, orderManagementMSResponse.orderDocument, offerDoc, true);
                        paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

                        if (paymentIFGOrderRS?.Errors != null && paymentIFGOrderRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentIFGOrder");
                            var message = paymentIFGOrderRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            return errorDetails;
                        }


                        var paymentDocs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery()
                        {
                            OrderID = orderManagementMSResponse.orderDocument.OrderNumber.OrderId
                        }, cancellationToken);

                        var ticketPaymentDoc = paymentDocs.First(x => x.Id == paymentIFGAuthorizeRS?.PaymentDocument?.TransactionId);

                        OrderDocumentMapper.UpdateCCPaymentDetails(orderManagementMSResponse.orderDocument, ticketPaymentDoc, paymentIFGAuthorizeRS?.PaymentDocument?.TrackId);

                    }
                    else
                    {
                        var paymentMSCaptureRequest = PaymentMSRequestMapper.MapOrderChangeCaptureRequest(paymentMSAuthorizeRequest, orderManagementMSResponse.orderDocument, paymentMSAuthorizeRS, offerDoc);
                        paymentCaptureRS = await _paymentMSCaptureCommandHandler.Handle(paymentMSCaptureRequest, cancellationToken);

                        if (paymentCaptureRS.Errors is not null)
                        {
                            var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentCaptureRS, _hostEnvironment);

                            var paymentMSRefundRequest = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderDataRS, request, orderManagementMSResponse?.orderDocument.OrderItems.First().AccountableDocumentRefs, paymentMSAuthorizeRS, paymentMSAuthorizeRequest);
                            var paymentMSRefundRS = await _paymentMSRefundCommandHandler.Handle(paymentMSRefundRequest, cancellationToken);

                            analyticsEventCommand.Track("ServiceName", "PaymentMSCapture");
                            analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);
                            return errorDetails;
                        }

                        OrderDocumentMapper.UpdateFopPaymentDetails(orderManagementMSResponse.orderDocument, null, null, false, paymentCaptureRS);

                        if (!isNonUsAgent)
                        {
                            transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderManagementMSResponse.orderDocument, paymentCaptureRS, null, Constants.ProductType.Ticket);

                            transactionresponse = await _transactionCommand.Handle(transactionRequest);

                            if (transactionresponse.ErrorDetails != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "transactionresponse");
                                analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First().Id);
                                nDCWarnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                            }

                            var paymentDetails = orderManagementMSResponse.orderDocument.OrderItems.Where(oi => oi.PaymentDetails != null).SelectMany(oi => oi.PaymentDetails);
                            foreach (var pd in paymentDetails)
                            {
                                pd.TrackId = transactionresponse.RequestId;
                                pd.PaymentProcessingStatus = (transactionresponse.ErrorDetails == null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING;
                            }
                        }

                    }

                }

                OrderDocumentMapper.UpdateOrderItemId(orderManagementMSResponse.orderDocument, orderDataRS.OrderDocument);

                orderManagementMSResponse.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSResponse.orderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument);

                try
                {
                    airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);

                    UpdateGenderAndDOB(request, orderManagementMSResponse.orderDocument);

                    var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, orderManagementMSResponse.orderDocument, null);

                    var gstValidation = OrderChangeValidations.ValidateGSTDetailsAgainstOrderDoc(orderManagementMSResponse.orderDocument, request, _hostEnvironment);
                    if (gstValidation.Any())
                    {
                        isGSTValidationFailed = true;
                        nDCWarnings.AddRange(gstValidation);
                    }

                    var gstWarnings = OrderChangeValidations.ValidateGSTDetails<NDCWarning>(request, _hostEnvironment);
                    if (gstWarnings.Any())
                    {
                        isGSTValidationFailed = true;
                        nDCWarnings.AddRange(gstWarnings);
                    }

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request, _hostEnvironment, orderManagementMSResponse.orderDocument));

                    //Update APIS , SSR and GST
                    if (request.Request.DataLists.PaxList.Any(x => x.IdentityDoc != null) || (!isGSTValidationFailed && request.Request.DataLists.PaxList.Any(x => x.Employer != null)) || (request.Request.DataLists.ContactInfoList != null && request.Request.DataLists.ContactInfoList.Any(x => x.PostalAddress != null)))
                    {
                        var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequest(request, orderManagementMSResponse.orderDocument, null, isGSTValidationFailed);
                        var orderChangeAPISResponse = await _manageCustomerRepoCommandHandler.Handle(orderManagementAPISRequest, cancellationToken);

                        if (orderChangeAPISResponse != null)
                        {
                            OrderDocumentMapper.UpdateSSRAPISGSTToOrderDoc(orderManagementMSResponse.orderDocument, orderChangeAPISResponse.orderDocument);

                            nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderChangeAPISResponse.orderDocument, _hostEnvironment, nDCWarnings));

                            orderManagementMSResponse.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                            if (orderChangeAPISResponse.orderDocument?.Warnings?.Any() == true)
                            {
                                orderManagementMSResponse.orderDocument.Warnings = orderChangeAPISResponse?.orderDocument?.Warnings;
                            }

                            var publishOrderDocReq = new UpdateOrderRequest { OrderDocument = orderManagementMSResponse.orderDocument };
                            await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocReq), cancellationToken);

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

                approvalCode = paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode;

                var orderchangeResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSResponse.orderDocument, nDCWarnings, false, true, false, false, null, null, isCashPayment, isEasyPay, approvalCode,false,true);

                LogOrderDetails(orderchangeResponse, orderManagementMSResponse.orderDocument, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeReshopConfirmHoldBookingCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Order Change Reshop Confirm Hold Booking Command Handler");
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }

            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
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
        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                analyticsEventCommand.Track("PNR", orderDocument?.BookingReference?.RLOC);

                analyticsEventCommand.Track("OrderId", orderDocument?.OrderNumber?.OrderId);

                DateTime? dateOfResponse = DateTime.UtcNow;

                if (orderRS?.PaymentFunctions != null)
                {
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

                decimal totalAmount = decimal.Zero;

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
                        analyticsEventCommand.Track($"WarningMessage{index + 1}", warning.DescText);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }
    }
}
