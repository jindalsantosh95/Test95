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
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderTicketMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
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
    public class OrderChangeHoldBookingCommandHandler : IRequestHandler<OrderChangeHoldBookingCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeHoldBookingCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ITransactionCommand _transactionCommand;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        public OrderChangeHoldBookingCommandHandler(IMediator mediator, ILogger<OrderChangeHoldBookingCommandHandler> logger, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSOrderCommandHandler paymentIFGMSOrderCommandHandler, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ITransactionCommand transactionCommand, ICheckAgentStatusQuery checkAgentStatusQuery, OrderDataMSQueryHandler orderDataMSQueryHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSOrderCommandHandler = paymentIFGMSOrderCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderCommandHandler));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Response<IATA_OrderViewRS>> Handle(OrderChangeHoldBookingCommand request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            try
            {
                List<NDCWarning> warnings = [];

                analyticsEventCommand.Track("PNR", request.OrderChangeRQ.Request?.BookingRef?.FirstOrDefault()?.BookingID);

                analyticsEventCommand.Track("OrderID", request.OrderChangeRQ.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_HoldBooking");

                var distributionChain = request.OrderChangeRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeHoldBookingRequest<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOrderChangeHoldBookingRequest");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);
                var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderRS, _hostEnvironment);

                if (orderDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderDataMSValidation");
                    analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, null, true);
                var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, new CancellationToken());
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

                if (!isNonUsAgent)
                {
                    var checkResponse = await _checkAgentStatusQuery.Handle(validationMSResponse.ValidateResponse.AgencyDetails.RequestAgencyId);
                    var checkResponseValidation = AgencyValidation.ValidateArcAgentStatus<IATA_OrderViewRS>(checkResponse, _hostEnvironment);

                    if (checkResponseValidation?.ResponseMessages is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ValidateArcAgentStatus");
                        analyticsEventCommand.Track("ErrorId", checkResponseValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(checkResponseValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(checkResponseValidation?.ResponseMessages);
                        return errorDetails;
                    }
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

                bool isCreditCardAccepted = false;
                var countryCode = validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode;
                var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

                if (currencyData != null)
                {
                    var currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                    var bspMarketPayment = await _mediator.Send(new BspMarketPaymentQuery(countryCode, currencyCode), cancellationToken);

                    isCreditCardAccepted = bspMarketPayment.IsCreditCardAccepted;
                }

                var paymentRq = request.OrderChangeRQ.Request.PaymentFunctions?.First().PaymentProcessingDetails;
                var cashPaymentTypeCode = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode;
                bool isEasyPay = request.OrderChangeRQ?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is PaymentProcessingIATA_EasyPayType;

                if (orderRS.OrderDocument.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode)
                {
                    var privatefaredFopValidation = OrderChangeValidations.ValidateOrderChangePrvateFareFOP<IATA_OrderViewRS>(request.OrderChangeRQ, orderRS.OrderDocument, _hostEnvironment);

                    if (privatefaredFopValidation != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "PrivateFareFopValidation");
                        analyticsEventCommand.Track("ErrorId", privatefaredFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(privatefaredFopValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(privatefaredFopValidation?.ResponseMessages);
                        return errorDetails;
                    }

                }

                if (cashPaymentTypeCode != null)
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
                }

                else if (isEasyPay)
                {
                    var easyFopValidation = AgencyValidation.ValidateEasyFop<IATA_OrderViewRS>(isEasyPay, validationMSResponse, _hostEnvironment);

                    if (easyFopValidation?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "EasyFopValidation");
                        analyticsEventCommand.Track("ErrorId", easyFopValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(easyFopValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(easyFopValidation?.ResponseMessages);
                        return errorDetails;
                    }
                }

                else
                {
                    var bspMarketValidationErrors = OrderChangeValidations.ValidateOrderChangeRequestFOP<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, isCreditCardAccepted);

                    if (bspMarketValidationErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "BspMarketFopValidation");
                        analyticsEventCommand.Track("ErrorId", bspMarketValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(bspMarketValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(bspMarketValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }
                }

                var serviceListOfferId = ((AcceptSelectedQuotedOfferListType)request.OrderChangeRQ?.Request?.ChangeOrderChoice?.Item)?.SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem != null && !((PaxSegmentRefListType)x.SelectedALaCarteOfferItem.OfferFlightAssociations.Item).PaxSegmentRefID.IsNullOrEmpty()));
                Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse = new Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS();

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

                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument);

                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                if (getReservationOrderDoc is not null)
                {
                    if (orderRS.OrderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }
                    foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                    }

                    if (orderRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderRS.OrderDocument.FareTypeRequest;

                    foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails)
                    {
                        var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (getReservationOrderDocCustomer != null)
                        {
                            getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                        }
                    }

                    getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderRS.OrderDocument, getReservationOrderDoc);
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

                    getReservationOrderDoc.ChangeType = orderRS.OrderDocument.ChangeType ?? getReservationOrderDoc.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderRS.OrderDocument.ChangeDateTime ?? getReservationOrderDoc.ChangeDateTime;

                    if (orderRS.OrderDocument.Version != null)
                    {
                        getReservationOrderDoc.Version = orderRS.OrderDocument.Version;
                    }

                    getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);

                    getReservationOrderDoc.Version = orderRS.OrderDocument.Version;
                    getReservationOrderDoc.ChangeType = orderRS.OrderDocument.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderRS.OrderDocument.ChangeDateTime;

                    if (!isNonUsAgent)
                    {
                        foreach (var orderItem in orderRS.OrderDocument.OrderItems)
                        {
                            var getReservationOrderDocOrderItem = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);
                            {
                                getReservationOrderDocOrderItem.PriceDetails = orderItem.PriceDetails;
                            }
                        }
                    }
                }
                else
                {
                    analyticsEventCommand.Track("ServiceName", "OrderSearchMS");
                    analyticsEventCommand.Track("ErrorId", getReservationOrderDoc?.Errors?.FirstOrDefault()?.Id);
                    throw new Exception($"SabreOrderDoc not found for orderId: {orderRS?.OrderDocument?.OrderNumber?.OrderId}");
                }

                var dobandgenderErrors = OrderChangeValidations.ValidateGenderAndDOBWithOrderRetrieve<IATA_OrderViewRS>(request.OrderChangeRQ, getReservationOrderDoc, _hostEnvironment);

                if (dobandgenderErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "DobGenderValidation");
                    analyticsEventCommand.Track("ErrorId", dobandgenderErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(dobandgenderErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(dobandgenderErrors?.ResponseMessages);
                    return errorDetails;
                }

                if (getReservationOrderDoc.DataList.CustomerDetails.Any(c => c.Gender.IsNullOrEmpty() || c.DateOfBirth.IsNullOrEmpty() || request.OrderChangeRQ.Request.DataLists.PaxList.Any(p => p.PaxID == c.CustId && (p.Individual != null && (p.Individual.BirthdateSpecified && p.Individual.Birthdate != Convert.ToDateTime(c.DateOfBirth) || (p.Individual.GenderCodeSpecified && p.Individual.GenderCode.ToString() != c.Gender))))))
                {
                    var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequestForGenderDOB(request.OrderChangeRQ, getReservationOrderDoc);
                    var orderChangeAPISResponse = await _mediator.Send(orderManagementAPISRequest, cancellationToken);

                    if (orderChangeAPISResponse != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails = getReservationOrderDoc.DataList.CustomerDetails.Select(x =>
                        {
                            var cust = orderChangeAPISResponse.orderDocument.DataList.CustomerDetails.First(c => c.FirstName.Equals(x.FirstName, StringComparison.OrdinalIgnoreCase) && c.LastName.Equals(x.LastName, StringComparison.OrdinalIgnoreCase));

                            x.DateOfBirth = cust.DateOfBirth;
                            x.Gender = cust.Gender;

                            return x;
                        });
                        await _mediator.Send(new OrderDataMSCommand() { OrderDocument = getReservationOrderDoc }, cancellationToken);
                    }
                    else
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                        analyticsEventCommand.Track("ErrorId", orderChangeAPISResponse?.error?.First()?.Id);

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        return errorDetails;
                    }
                }

                UpdateGenderAndDOB(request.OrderChangeRQ, getReservationOrderDoc);

                foreach (var (logApis, index) in request.OrderChangeRQ.Request.DataLists.PaxList.Select((logApis, index) => (logApis, index)))
                {
                    int i = index;
                    i++;
                    analyticsEventCommand.Track($"PaxID{i}", logApis.PaxID);
                    analyticsEventCommand.Track($"APIS Indicator{i}", (logApis.IdentityDoc != null || logApis.RedressCase != null) ? "Yes" : "No");

                    if (logApis.IdentityDoc != null)
                    {
                        analyticsEventCommand.Track($"Passport{i}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.PT) ? "Yes" : "No");
                        analyticsEventCommand.Track($"Visa{i}", logApis.IdentityDoc.Any(x => x.Visa != null) ? "Yes" : "No");
                        analyticsEventCommand.Track($"Greencard{i}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.GC) ? "Yes" : "No");
                        analyticsEventCommand.Track($"KnownTraveller{i}", logApis.IdentityDoc.Any(x => x.IdentityDocTypeCode == Constants.Application.CR) ? "Yes" : "No");
                        analyticsEventCommand.Track($"Redress{i}", logApis.RedressCase != null ? "Yes" : "No");
                    }
                    else if (logApis.RedressCase != null && logApis.IdentityDoc == null)
                    {
                        analyticsEventCommand.Track($"Redress{i}", "Yes");
                    }
                }
                analyticsEventCommand.Track("GST Indicator", validationMSResponse.ValidateResponse.AgencyDetails.CountryCode.Is(Constants.Application.IndiaCountryCode) ? "Yes" : "No");

                var currencyRS = await _mediator.Send(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);
                var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);
                bool isCashPayment = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode == "CA";

                bool isCardPayment = !isCashPayment && !isEasyPay && request.OrderChangeRQ.Request.PaymentFunctions != null;
                bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));


                string ticketPaymentTransactionId = null;
                string ticketTrackId = null;

                if (isCardPayment)
                {
                    var paymentMethodDetail = paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType;
                    var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

                    analyticsEventCommand.Track("PaymentTypeCode", request?.OrderChangeRQ?.Request?.PaymentFunctions?.First().PaymentMethodCriteria?.PaymentTypeCode);

                    analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                    analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');
                }

                OrderTicketRS orderTicketRS = null;
                string approvalCode = null;

                var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);
                if (airportDetails?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                if (request.OrderChangeRQ.Request.PaymentFunctions != null)
                {
                    PaymentMSAuthorizeCommand paymentMSRequest = null;
                    PaymentRS paymentIFGAuthorizeRS = null;
                    PaymentIFGMSAuthorizeCommand paymentIFGAuthorizeRQ = null;
                    PaymentRS paymentMSAuthorizeRS = null;
                    PaymentRS paymentMSCaptureRS = null;

                    var amount = request.OrderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.Value.ToString();

                    if (isCashPayment && isNonUsAgent)
                    {
                        paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeIFGAuthorizeRequest(request.OrderChangeRQ, getReservationOrderDoc, airportDetails, amount, Constants.ProductType.Ticket, false, false);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "paymentIFGMSAuthorize");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);
                            var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);
                            
                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            return errorDetails;
                        }
                    }

                    else if (isEasyPay && isNonUsAgent)
                    {
                        paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeIFGAuthorizeRequest(request.OrderChangeRQ, getReservationOrderDoc, airportDetails, amount, Constants.ProductType.Ticket, false, true);
                        paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        approvalCode = paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode;

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "paymentIFGMSAuthorize");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);
                            var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);
                            
                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                            return errorDetails;
                        }
                    }

                    else if (isCardPayment)
                    {
                        if (isIFGIntegrated && isNonUsAgent)
                        {
                            paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderChangeIFGAuthorizeRequest(request.OrderChangeRQ, getReservationOrderDoc, airportDetails, amount, Constants.ProductType.Ticket, true, false);

                            paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                            if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "paymentIFGMSAuthorize");
                                var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);
                                var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                                await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);
                                
                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                return errorDetails;
                            }

                            ticketPaymentTransactionId = paymentIFGAuthorizeRS.PaymentDocument.TransactionId;
                            ticketTrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId;
                        }
                        else
                        {

                            paymentMSRequest = PaymentMSRequestMapper.MapHoldBookingAuthorizeRequest(getReservationOrderDoc, request.OrderChangeRQ, airportDetails, amount, currencies, Constants.ProductType.Ticket);
                            paymentMSAuthorizeRS = await _mediator.Send(paymentMSRequest, cancellationToken);

                            if (paymentMSAuthorizeRS.Errors is not null)
                            {
                                var paymentMSAuthorizeErrors = Order.Validations.Common.PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSAuthorizeRS, _hostEnvironment);
                                analyticsEventCommand.Track("ServiceName", "paymentMSAuthorize");
                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);
                                return errorDetails;
                            }
                        }

                    }

                    // calling IFG Agent LookUp API to check agent permission

                    if (isNonUsAgent && !isTidsAgent)
                    {
                        IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = null;
                        ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSResponse);

                        var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                        var validateIFGAgentsPermission = OrderChangeValidations.ValidateIFGAgentsPermission(request.OrderChangeRQ, ifgAgentFopLookUpResponse, _hostEnvironment);

                        if (validateIFGAgentsPermission is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "iFGAgentFopLookUpValidation");
                            analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(validateIFGAgentsPermission?.ResponseMessages);
                            return errorDetails;
                        }
                    }

                    var orderTicketMSRequest = OrderChangeHeldRequestMapper.MapOrderTicketMSRequest(request.OrderChangeRQ, getReservationOrderDoc);
                    orderTicketRS = await _mediator.Send(orderTicketMSRequest, cancellationToken);

                    if (orderTicketRS.Errors is not null)
                    {
                        // to modify after facade layer implementation
                        if (isCashPayment || isIFGIntegrated)
                        {
                            await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId }, cancellationToken);
                        }
                        else
                        {
                            var orderCancelRequest = OrderCancelRequestMapper.MapOrderCancelRequest(orderRS.OrderDocument.BookingReference, false, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderChange, null, getReservationOrderDoc.Parties);
                            await _mediator.Send(orderCancelRequest, cancellationToken);

                            var paymentMSRefundRequest = PaymentMSRequestMapper.MapPaymentMSOrderChangeRefundRequest(orderRS, request.OrderChangeRQ, currencies, amount.ToString(), paymentMSAuthorizeRS, orderTicketRS?.OrderTicketDocument?.AccountableDocumentRef);
                            await _mediator.Send(paymentMSRefundRequest, cancellationToken);
                        }
                        analyticsEventCommand.Track("ServiceName", "OrderTicket");
                        analyticsEventCommand.Track("ErrorId", orderTicketRS?.Errors?.First()?.Id);

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        return errorDetails;
                    }

                    getReservationOrderDoc = OrderDocumentMapper.UpdateOrderDocument(getReservationOrderDoc, orderTicketRS.OrderTicketDocument, null, null, null);

                    getReservationOrderDoc.Version = orderRS.OrderDocument.Version + 1;
                    getReservationOrderDoc.ChangeType = ChangeTypeEnum.DocumentIssuance;
                    getReservationOrderDoc.ChangeDateTime = new UtcAndLocalDateTime() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };

                    if (isCashPayment && isNonUsAgent)
                    {
                        var paymentIFGOrderRQ = PaymentMSRequestMapper.MapOrderChangeHoldBookingIFGOrderRequest(paymentIFGAuthorizeRQ, orderTicketRS.OrderTicketDocument.AccountableDocumentRef, paymentIFGAuthorizeRS, orderRS.OrderDocument, Constants.ProductType.Ticket);
                        var paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

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

                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderRS.OrderDocument, Constants.Application.PaymentDocType);
                        var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                            OrderDocumentMapper.UpdateFopPaymentDetails(getReservationOrderDoc, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument);

                    }
                    else if (isEasyPay && isNonUsAgent)
                    {
                        var paymentIFGOrderRQ = PaymentMSRequestMapper.MapOrderChangeHoldBookingIFGOrderRequest(paymentIFGAuthorizeRQ, orderTicketRS.OrderTicketDocument.AccountableDocumentRef, paymentIFGAuthorizeRS, orderRS.OrderDocument, Constants.ProductType.Ticket);
                        var paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

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

                        var easyPayCardNumber = (request.OrderChangeRQ.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item as PaymentProcessingIATA_EasyPayType).AccountNumber;
                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderRS.OrderDocument, Constants.Application.PaymentDocType, false, false, true, null, null, easyPayCardNumber);
                        var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                            OrderDocumentMapper.UpdateFopPaymentDetails(getReservationOrderDoc, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, true, null, null, false, null, paymentDocumentRequest.PaymentDetails.First().CardMethod);


                    }
                    else if (isCardPayment)
                    {
                        //need to remove the if condition after ifg integration is completed
                        if (isIFGIntegrated && isNonUsAgent)
                        {
                            var paymentIFGCaptureRQ = PaymentMSRequestMapper.MapOrderChangeHoldBookingIFGOrderRequest(paymentIFGAuthorizeRQ, orderTicketRS?.OrderTicketDocument?.AccountableDocumentRef, paymentIFGAuthorizeRS, orderRS.OrderDocument, Constants.ProductType.Ticket);
                            var paymentIFGCaptureRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGCaptureRQ, cancellationToken);

                            if (paymentIFGCaptureRS?.Errors != null && paymentIFGCaptureRS.Errors.Any())
                            {
                                analyticsEventCommand.Track("ServiceName", "paymentIFGCapture");
                                var message = paymentIFGCaptureRS?.Errors?.FirstOrDefault()?.Message;
                                var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                                analyticsEventCommand.Track("ErrorId", errorCode);
                                
                                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                return errorDetails;
                            }
                        }
                        else
                        {
                            var paymentMSCaptureRequest = PaymentMSRequestMapper.MapHoldBookingCaptureRequest(paymentMSRequest, paymentMSAuthorizeRS, getReservationOrderDoc, orderTicketRS.OrderTicketDocument.AccountableDocumentRef, Constants.ProductType.Ticket);
                            paymentMSCaptureRS = await _mediator.Send(paymentMSCaptureRequest, cancellationToken);

                            if (paymentMSCaptureRS.Errors == null)
                            {
                                OrderDocumentMapper.UpdateHoldBookingPaymentDetails(getReservationOrderDoc, paymentMSRequest, paymentMSAuthorizeRS, paymentMSCaptureRS);
                            }
                            else
                            {
                                analyticsEventCommand.Track("ServiceName", "PaymentMSCapture");
                                analyticsEventCommand.Track("ErrorId", paymentMSCaptureRS?.Errors?.FirstOrDefault()?.Id);

                                getReservationOrderDoc.ChangeType = ChangeTypeEnum.PaymentFailure;
                                foreach (var item in getReservationOrderDoc.OrderItems) { item.AccountableDocumentRefs = null; }

                                OrderDocumentMapper.UpdateHoldBookingPaymentDetails(getReservationOrderDoc, paymentMSRequest, paymentMSAuthorizeRS, null);

                                var publishPaymentFailureOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishPaymentFailureOrderDocRequest), cancellationToken);

                                var paymentMSCaptureErrors = Order.Validations.Common.PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSCaptureRS, _hostEnvironment);

                                var orderCancelRequest = OrderCancelRequestMapper.MapOrderCancelRequest(orderRS.OrderDocument.BookingReference, false, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderChange, null, getReservationOrderDoc.Parties);
                                await _mediator.Send(orderCancelRequest, cancellationToken);

                                var paymentMSRefundRequest = PaymentMSRequestMapper.MapPaymentMSOrderChangeRefundRequest(orderRS, request.OrderChangeRQ, currencies, amount.ToString(), paymentMSAuthorizeRS, orderTicketRS.OrderTicketDocument.AccountableDocumentRef);

                                await _mediator.Send(paymentMSRefundRequest, cancellationToken);

                                analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);
                                return errorDetails;
                            }

                        }

                        if (!isNonUsAgent)
                        {
                            var cardTransactionRequest = TransactionRequestMapper.MapToTransactionRequest(getReservationOrderDoc, paymentMSCaptureRS, orderTicketRS?.OrderTicketDocument?.AccountableDocumentRef, Constants.ProductType.Ticket);

                            var transactionresponse = await _transactionCommand.Handle(cardTransactionRequest);

                            if (transactionresponse.ErrorDetails != null)
                            {
                                analyticsEventCommand.Track("ServiceName", "transactionresponse");
                                analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First().Id);
                                warnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                            }

                            getReservationOrderDoc.OrderItems.Where(oi => oi.PaymentDetails != null).SelectMany(oi => oi.PaymentDetails).Select(pd =>
                            {
                                pd.TrackId = transactionresponse.RequestId;
                                pd.PaymentProcessingStatus = (transactionresponse.ErrorDetails == null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING;

                                return pd;
                            });
                        }

                    }
                    else if (isCashPayment && !isNonUsAgent)
                    {
                        var cashTransactionRequest = TransactionRequestMapper.MapToTransactionRequest(getReservationOrderDoc, paymentMSCaptureRS, orderTicketRS?.OrderTicketDocument?.AccountableDocumentRef, Constants.ProductType.Ticket, true);

                        var transactionresponse = await _transactionCommand.Handle(cashTransactionRequest);

                        if (transactionresponse.ErrorDetails != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "transactionresponse");
                            analyticsEventCommand.Track("ErrorId", transactionresponse?.ErrorDetails?.First().Id);
                            warnings.AddRange(OrderChangeValidations.PaymentReportingFailureWarning(_hostEnvironment));
                        }

                        var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderRS.OrderDocument, Constants.Application.PaymentDocType, false, false, false, cashTransactionRequest, transactionresponse);
                        var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                        if (paymentDocumentResponse.PaymentDocument != null)
                            OrderDocumentMapper.UpdateFopPaymentDetails(getReservationOrderDoc, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, false, null, null, false, transactionresponse);

                    }
                }

                try
                {
                    var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, getReservationOrderDoc, null);

                    var gstWarnings = OrderChangeValidations.ValidateGSTDetails<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                    if (gstWarnings.Any())
                    {
                        warnings.AddRange(gstWarnings);
                        isGSTValidationFailed = true;
                    }

                    var apisWarnings = OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, getReservationOrderDoc);
                    if (apisWarnings.Any()) warnings.AddRange(apisWarnings);

                    //Update APIS , SSR and GST
                    if (orderRS?.OrderDocument.OrderStatus != nameof(OrderStatusEnum.Unticketed) && (request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.IdentityDoc != null) || (!isGSTValidationFailed && request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.Employer != null)) || flightServiceListResponse?.ServiceListResponse != null) || (request.OrderChangeRQ.Request.DataLists.ContactInfoList != null && request.OrderChangeRQ.Request.DataLists.ContactInfoList.Any(x => x.PostalAddress != null)))
                    {
                        var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequest(request.OrderChangeRQ, getReservationOrderDoc, flightServiceListResponse, isGSTValidationFailed);
                        var orderChangeAPISResponse = await _mediator.Send(orderManagementAPISRequest, cancellationToken);

                        if (orderChangeAPISResponse?.orderDocument != null)
                        {
                            if (orderChangeAPISResponse.orderDocument.Warnings is not null)
                            {
                                var orderChangeWarnings = orderChangeAPISResponse.orderDocument.Warnings;
                                var ssrInstructions = orderManagementAPISRequest.ManageCustomerRequest.OrderInstructions
                                    .Where(oi => oi.ItemText?.Contains(Constants.Application.FlightService) == true)
                                    .ToList();
                                var ssrWarnings = orderChangeWarnings.Where(w => w.Code == "186").ToList();

                                bool allMatched = ssrInstructions.All(oi =>
                                        ssrWarnings.Any(a =>
                                            a.Details == oi.ItemCode &&
                                            a.Message.Contains(oi.ItemText.Split("/")[1]) &&
                                            a.Message.Contains(oi.ItemText.Split("/")[2])
                                        ));

                                if (ssrInstructions.Any()
                                    && ssrInstructions.Count == ssrWarnings.Count
                                    && allMatched)
                                {
                                    var orderManagementMS = OrderManagementMSResponseValidation.ChangeWarningsToError<IATA_OrderViewRS>(orderChangeAPISResponse, _hostEnvironment);

                                    analyticsEventCommand.Track("ServiceName", "orderManagementMS");

                                    analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                    LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                                    var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                                    return errorDetails;
                                }

                                warnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderChangeAPISResponse.orderDocument, _hostEnvironment, warnings));
                            }

                            OrderDocumentMapper.UpdateSSRAPISGSTToOrderDoc(getReservationOrderDoc, orderChangeAPISResponse.orderDocument);

                            if (orderChangeAPISResponse.orderDocument.Warnings?.Any() == true)
                            {
                                getReservationOrderDoc.Warnings = orderChangeAPISResponse?.orderDocument?.Warnings;
                            }
                            if (request.OrderChangeRQ.Request.PaymentFunctions == null && orderChangeAPISResponse.orderDocument.Warnings?.Any(x => x.Code == "182") == true)
                            {
                                getReservationOrderDoc.ChangeType = ChangeTypeEnum.SpecialServiceRequestAdded;
                                getReservationOrderDoc.Version = getReservationOrderDoc.Version + 1;
                                getReservationOrderDoc.ChangeDateTime = orderChangeAPISResponse.orderDocument.ChangeDateTime;
                                getReservationOrderDoc.IsPostSaleSSR = true;

                                var historyDoc = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"{getReservationOrderDoc.OrderNumber.OrderId}-History" }, cancellationToken);

                                var updateHistoryDocument = OrderDocumentMapper.UpdateOrderHistoryWithChanges(orderRS.OrderDocument, historyDoc.OrderDocument);
                                var publishHistoryDoc = new UpdateOrderRequest { OrderDocument = updateHistoryDocument };

                                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishHistoryDoc), cancellationToken);

                            }

                            await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(new UpdateOrderRequest { OrderDocument = getReservationOrderDoc }), cancellationToken);
                        }
                        else if (request.OrderChangeRQ.Request.PaymentFunctions == null && orderChangeAPISResponse.error != null)
                        {
                            var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderChangeAPISResponse, _hostEnvironment);

                            analyticsEventCommand.Track("ServiceName", "orderManagementMS");

                            analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                            return errorDetails;
                        }
                        else
                        {
                            warnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                        }
                    }

                }
                catch
                {
                    warnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                }

                if (isIFGIntegrated && isCardPayment && isNonUsAgent)
                {
                    var paymentDocs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery()
                    {
                        OrderID = getReservationOrderDoc.OrderNumber.OrderId
                    }, cancellationToken);

                    var ticketPaymentDoc = paymentDocs.First(x => x.Id == ticketPaymentTransactionId);

                    OrderDocumentMapper.UpdateCCPaymentDetails(getReservationOrderDoc, ticketPaymentDoc, ticketTrackId);
                }

                var segmentCount =  getReservationOrderDoc?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                analyticsEventCommand.Track("SegmentCount", segmentCount);

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                var orderviewResponse = OrderResponseMapper.MapOrderResponse(getReservationOrderDoc, warnings, false, false, false, false, null, null, isCashPayment, isEasyPay, approvalCode);

                var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(RequestType.OrderChange, getReservationOrderDoc);
                _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                LogOrderDetails(orderviewResponse, getReservationOrderDoc, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderviewResponse };

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeHoldBookingCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Change HoldBooking Command Handler");
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
                    if (!pax.Individual.BirthdateSpecified)
                    {
                        pax.Individual.Birthdate = Convert.ToDateTime(orderDocCust.DateOfBirth);
                        pax.Individual.BirthdateSpecified = true;
                    }
                    else orderDocCust.DateOfBirth = pax.Individual.Birthdate.ToString("yyyy-MM-dd");

                    if (!pax.Individual.GenderCodeSpecified)
                    {
                        pax.Individual.GenderCode = Enum.Parse<Domain.OrderChangeRQ.GenderCodeType>(orderDocCust.Gender);
                        pax.Individual.GenderCodeSpecified = true;
                    }
                    else orderDocCust.Gender = pax.Individual.GenderCode.ToString();
                }
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
