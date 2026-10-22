using CSharpFunctionalExtensions;
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
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Helper;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderCreate;
using VAA.EAPI.NDC.Order.Validations.Seat;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.DistributionChainRoleCodeType;
using PaxSegmentRefListType = VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaxSegmentRefListType;

namespace VAA.EAPI.NDC.Order.Application.OrderCreate
{
    public class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, Result<Response<IATA_OrderViewRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly ICheckAgentStatusQuery _checkAgentStatusQuery;
        private readonly ITransactionCommand _transactionCommand;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;

        public OrderCreateCommandHandler(IMediator mediator, ILogger<OrderCreateCommandHandler> logger, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSOrderCommandHandler paymentIFGMSOrderCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ICurrencyConversionQuery currencyConversionQueryHandler, ICheckAgentStatusQuery checkAgentStatusQuery, ITransactionCommand transactionCommand, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ICityCodeQuery cityCodeQuery)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSOrderCommandHandler = paymentIFGMSOrderCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _checkAgentStatusQuery = checkAgentStatusQuery ?? throw new ArgumentNullException(nameof(checkAgentStatusQuery));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(paymentMSRetrieveQueryHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _cityCodeQuery = cityCodeQuery ?? throw new ArgumentNullException(nameof(cityCodeQuery));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(OrderCreateCommand orderCreateCommand, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var nDCWarnings = new List<NDCWarning>();
            var transactionStatus = new List<TransactionIdStatus>();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderCreateEventSubject
            };

            try
            {
                bool isHoldBooking = orderCreateCommand?.OrderCreateRQ?.Request?.PaymentFunctions?.FirstOrDefault() == null;

                DateTime? dateOfRequest = DateTime.UtcNow;

                analyticsEventCommand.Track("BookingType", isHoldBooking ? "Hold" : "Instant");
                
                var paymentRq = orderCreateCommand.OrderCreateRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
                var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.PaymentProcessingPaymentCardType)paymentRq?.PaymentMethod?.Item);
                var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();
                bool motoPayment = transactionType == "MO" || transactionType == "TO" ? true : false;

                analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');

                var distributionChain = orderCreateCommand?.OrderCreateRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var orderCreateRQValidationErrors = OrderCreateValidation.OrderCreateRQValidation<IATA_OrderViewRS>(orderCreateCommand.OrderCreateRQ, isHoldBooking, motoPayment, true, _hostEnvironment);

                if (orderCreateRQValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderCreateRQValidation");
                    analyticsEventCommand.Track("ErrorId", orderCreateRQValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderCreateRQValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderCreateRQValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRQ = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderCreate(orderCreateCommand.OrderCreateRQ) }, null, true);
                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRQ, AgentSubscriptionKey = orderCreateCommand.SubscriptionKey }, cancellationToken);

                if (validationMSRS.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSRS, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                bool isUsAgent = validationMSRS.ValidateResponse?.AgencyDetails?.CountryCode == "US";
                bool isTidsAgent = validationMSRS.ValidateResponse?.AgencyDetails?.RequestAgencyIdType == "TIDS";

                analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestOrgName);
                if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }
                analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId);

                var offerId = (orderCreateCommand.OrderCreateRQ.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault()?.OfferRefID;
                analyticsEventCommand.Track("IATAOfferID", offerId);

                var offerDoc = await _mediator.Send((OfferMSQuery)offerId, cancellationToken);
                offerDoc = OfferDocMapper.UpdateOfferDocWithAgentType(offerDoc, validationMSRS);

                var offerDocValidationErrors = OrderCreateValidation.ValidateOfferOrder<IATA_OrderViewRS>(orderCreateCommand.OrderCreateRQ, offerDoc, validationMSRS, isHoldBooking, _hostEnvironment);

                if (offerDocValidationErrors is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferMS");
                    analyticsEventCommand.Track("ErrorId", offerDocValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                   
                    LogErrorDetails(offerDocValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(offerDocValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("PosCountry", offerDoc.Pos?.CountryCode);

                var contactValidationErrors = OrderCreateValidation.ValidateContactDetails<IATA_OrderViewRS>(orderCreateCommand.OrderCreateRQ, offerDoc, isHoldBooking, _hostEnvironment);

                if (contactValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateContactDetails");
                    analyticsEventCommand.Track("ErrorId", contactValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(contactValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(contactValidationErrors?.ResponseMessages);
                    return errorDetails;
                }


                var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);
             
                if (airportDetails?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);                  
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                var gstValidationFailed = OrderCreateValidation.ValidateGST(validationMSRS, offerDoc, airportDetails);

                var gstWarnings = OrderCreateValidation.ValidateGSTDetails(orderCreateCommand.OrderCreateRQ, _hostEnvironment);

                if (gstWarnings.Any())
                {
                    nDCWarnings.AddRange(gstWarnings);
                    gstValidationFailed = true;

                    foreach (var (warning, index) in gstWarnings.Select((warning, index) => (warning, index)))
                    {
                        analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                        analyticsEventCommand.Track($"WarningMessage{index + 1}", warning.TagText);
                    }
                }

                var apisWarnings = OrderCreateValidation.ValidateAPISDetails(orderCreateCommand.OrderCreateRQ, _hostEnvironment);

                if (apisWarnings.Any())
                {
                    nDCWarnings.AddRange(apisWarnings);

                    foreach (var (warning, index) in apisWarnings.Select((warning, index) => (warning, index)))
                    {
                        analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                        analyticsEventCommand.Track($"WarningMessage{index + 1}", warning.TagText);
                    }
                }

                #region Flight Services

                var serviceListOfferId = ((AcceptSelectedQuotedOfferListType)orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item)?
                   .SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null
                   && y.SelectedALaCarteOfferItem != null && !((PaxSegmentRefListType)y.SelectedALaCarteOfferItem.OfferFlightAssociations?.Item)
                   .PaxSegmentRefID.IsNullOrEmpty()));

                Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS flightServiceListRS = new Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS();

                if (serviceListOfferId != null)
                {
                    flightServiceListRS = await _mediator.Send((FlightServiceOfferDocQuery)serviceListOfferId.OfferRefID, cancellationToken);
                    var flightServiceValidationErrors = OrderCreateValidation.ValidateFlightServiceList<IATA_OrderViewRS>(orderCreateCommand.OrderCreateRQ, flightServiceListRS, _hostEnvironment);

                    if (flightServiceValidationErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "FlightService");
                        analyticsEventCommand.Track("ErrorId", flightServiceValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(flightServiceValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(flightServiceValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }
                }

                #endregion

                #region Seat Validation

                SeatRS seatOfferDoc = new();
                SeatFulfilmentResponse seatFulfilmentRS = null;

                if ((orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)))
                {
                    var seatmsOfferID = (orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).OfferRefID;
                    seatOfferDoc = await _mediator.Send(new SeatMSQuery() { OfferID = seatmsOfferID }, cancellationToken);
                    var seatValidationError = OrderCreateValidation.ValidateSeatOfferDetails<IATA_OrderViewRS>(orderCreateCommand.OrderCreateRQ, seatOfferDoc?.SeatAvailabilityResponse?.FlightSegmentSeatMap, _hostEnvironment);

                    if (seatValidationError != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatMS");
                        analyticsEventCommand.Track("ErrorId", seatValidationError?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(seatValidationError?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(seatValidationError?.ResponseMessages);
                        return errorDetails;
                    }
                }

                #endregion

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
                        BrandMSResponseMapper.UpdateOfferDoc(brandResp.Value, brandResp.Key, offerDoc, ref penaltyIndex, ref baggageCounter, ref serviceCounter);
                    }
                }
                if (isUsAgent)
                {
                    var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);

                    if (string.IsNullOrWhiteSpace(currencyExchangeValue))
                    {
                        analyticsEventCommand.Track("ServiceName", "CurrencyExchangeValueMissing");
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    OrderDocumentMapper.CurrencyConversion(offerDoc, currencyExchangeValue);

                    var checkResponse = await _checkAgentStatusQuery.Handle(validationMSRS.ValidateResponse.AgencyDetails.RequestAgencyId);

                    var checkResponseValidation = AgencyValidation.ValidateArcAgentStatus<IATA_OrderViewRS>(checkResponse, _hostEnvironment);

                    if (checkResponseValidation?.ResponseMessages is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "checkResponseValidationError");
                        analyticsEventCommand.Track("ErrorId", checkResponseValidation?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(checkResponseValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(checkResponseValidation?.ResponseMessages);
                        return errorDetails;
                    }
                }

                var orderMSRQ = OrderMSRequestMapper.MapOrderMSRequest(orderCreateCommand.OrderCreateRQ, offerDoc, !gstValidationFailed, flightServiceListRS);
                var orderMSRS = await _mediator.Send(orderMSRQ, cancellationToken);

                if (orderMSRS.Errors is not null)
                {
                    if (orderMSRS.Errors.Any(x => x.Code == "F002"))
                    {
                        var error = orderMSRS.Errors.FirstOrDefault(x => x.Code == "F002");
                        string rloc = error.DeveloperMessage?.Split("RecordLocator:").LastOrDefault()?.Trim();
                        BookingReference bookingReference = new BookingReference { RLOC = rloc };

                        var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(bookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties, false, null);
                        var orderCancelStatus = await _mediator.Send(orderCancelRQ, cancellationToken);

                        analyticsEventCommand.Track("OrderCancelStatus", orderCancelStatus);
                    }

                    var orderMSValidtionError = OrderMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderMSRS, _hostEnvironment);
                    analyticsEventCommand.Track("ServiceName", "OrderMS");
                    analyticsEventCommand.Track("ErrorId", orderMSValidtionError?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderMSValidtionError?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderMSValidtionError?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("PNR", orderMSRS?.OrderDocument?.BookingReference?.RLOC);
                analyticsEventCommand.Track("OrderID", orderMSRS?.OrderDocument?.OrderNumber?.OrderId);

                if (!isHoldBooking)
                {
                    var currencyRS = await _mediator.Send(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);

                    if (currencyRS is null) analyticsEventCommand.Track("ServiceName", "ReferenceCurrencyMS_Failure");

                    var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);

                    var amount = (orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Where(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null)).Sum(x => x?.TotalOfferPriceAmount?.Value);

                    analyticsEventCommand.Track("OrderId", orderMSRS.OrderDocument.OrderNumber.OrderId);
                    analyticsEventCommand.Track("PaymentTypeCode", "CC");

                    PaymentIFGMSAuthorizeCommand paymentIFGAuthorizeRQ = null;
                    PaymentRS paymentIFGAuthorizeRS = null;

                    PaymentMSAuthorizeCommand paymentMSAuthorizeRQ = null;
                    PaymentRS paymentMSAuthorizeRS = null;

                    string ticketPaymentTransactionId = null;
                    string ticketTrackId = null;

                    bool isIFGIntegrated = bool.Parse(ConfigReader.Get(Constants.Environment.isIFGIntegrated));

                    //need to remove the if condition after ifg integration is completed
                    if (isIFGIntegrated && !isUsAgent)
                    {
                            paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderCreateCCIFGAuthorizeRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, offerDoc, currencies, airportDetails, amount.ToString(), Constants.ProductType.Ticket);
                            paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                        if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                        {
                            var paymentMSAuthorizeErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentIFGAuthorizeRS, _hostEnvironment, true);
                            
                            analyticsEventCommand.Track("ServiceName", "PaymentMSAuthorize");
                            var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                            var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                            analyticsEventCommand.Track("ErrorId", errorCode);

                                LogErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);

                                var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                                await _mediator.Send(orderCancelRQ, cancellationToken);

                                analyticsEventCommand.Track("OrderCancelStatus", true);

                                return errorDetails;
                            }

                        ticketPaymentTransactionId = paymentIFGAuthorizeRS?.PaymentDocument?.TransactionId;
                        ticketTrackId = paymentIFGAuthorizeRS?.PaymentDocument?.TrackId;
                    }
                    else
                    {
                        //need to remove order payment ms call after ifg integration is completed
                      
                            paymentMSAuthorizeRQ = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, offerDoc, currencies, airportDetails, amount.ToString(), Constants.ProductType.Ticket);

                            paymentMSAuthorizeRS = await _mediator.Send(paymentMSAuthorizeRQ, cancellationToken);

                            if (paymentMSAuthorizeRS?.Errors?.Any() == true)
                            {
                                var paymentMSAuthorizeErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSAuthorizeRS, _hostEnvironment);
                                var responseMessage =paymentMSAuthorizeErrors?.ResponseMessages?? DefaultError.DefaultErrorMesaage();

                                analyticsEventCommand.Track("ServiceName", "PaymentMSAuthorize");
                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(responseMessage, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);

                                var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                                await _mediator.Send(orderCancelRQ, cancellationToken);

                                analyticsEventCommand.Track("OrderCancelStatus", true);

                                return errorDetails;
                            }
                    }

                    // calling IFG Agent LookUp API to check agent permission
                    if (isIFGIntegrated && !isUsAgent && !isTidsAgent)
                    {

                        IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSRS);

                        var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                        var validateIFGAgentsPermission = OrderCreateValidation.ValidateIFGAgentsPermission(orderCreateCommand.OrderCreateRQ, ifgAgentFopLookUpResponse, _hostEnvironment);

                        if (validateIFGAgentsPermission is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "IFGAgentFopLookUp Validation");
                            analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                            
                            LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(validateIFGAgentsPermission?.ResponseMessages);
                            
                            return errorDetails;
                        }
                    }

                    var orderTicketMSRQ = OrderTicketMSRequestMapper.MapOrderTicketMSRequest(orderCreateCommand.OrderCreateRQ, offerDoc, orderMSRS.OrderDocument, null);
                    var orderTicketMSRS = await _mediator.Send(orderTicketMSRQ, cancellationToken);

                    if (orderTicketMSRS.Errors is not null)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderTicketMS");
                        analyticsEventCommand.Track("ErrorId", orderTicketMSRS.Errors?.FirstOrDefault()?.Id);

                        var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                        await _mediator.Send(orderCancelRQ, cancellationToken);
                       
                        analyticsEventCommand.Track("OrderCancelStatus", true);

                        var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, currencies, amount.ToString(), paymentMSAuthorizeRS, orderTicketMSRS?.OrderTicketDocument?.AccountableDocumentRef);
                        await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                        
                        analyticsEventCommand.Track("PaymentMSRefundStatus", true);
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    OrderDocumentMapper.UpdateOrderDocument(orderMSRS.OrderDocument, orderTicketMSRS.OrderTicketDocument, null, null, offerDoc.DataList.PriceClass);

                    PaymentRS paymentMSCaptureRS = null;

                    if (isIFGIntegrated && !isUsAgent)
                    {
                            var paymentIFGCaptureRQ = PaymentMSRequestMapper.MapOrderCreateCCIFGOrderRequest(paymentIFGAuthorizeRQ, orderTicketMSRS?.OrderTicketDocument?.AccountableDocumentRef, paymentIFGAuthorizeRS, Constants.ProductType.Ticket, orderTicketMSRS?.OrderTicketDocument?.BookingReference, orderMSRS.OrderDocument, ticketTrackId);
                            var paymentIFGCaptureRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGCaptureRQ, cancellationToken);

                        if (paymentIFGCaptureRS?.Errors != null && paymentIFGCaptureRS.Errors.Any())
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentMSCapture");
                            var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentIFGCaptureRS, _hostEnvironment, true);

                                analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);
                                
                                var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                                await _mediator.Send(orderCancelRQ, cancellationToken);
                                
                                analyticsEventCommand.Track("OrderCancelStatus", true);

                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, currencies, amount.ToString(), paymentIFGAuthorizeRS, orderTicketMSRS.OrderTicketDocument.AccountableDocumentRef);
                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                                
                                analyticsEventCommand.Track("PaymentMSRefundStatus", true);
                                
                                return errorDetails;
                            }
                    }
                    else
                    {
                            var paymentMSCaptureRQ = PaymentMSRequestMapper.MapOrderCreateCaptureRequest(paymentMSAuthorizeRQ, orderTicketMSRS?.OrderTicketDocument?.AccountableDocumentRef, paymentMSAuthorizeRS, offerDoc, Constants.ProductType.Ticket, orderTicketMSRS?.OrderTicketDocument?.BookingReference, orderMSRS.OrderDocument);
                            paymentMSCaptureRS = await _mediator.Send(paymentMSCaptureRQ, cancellationToken);

                            if (paymentMSCaptureRS.Errors is not null)
                            {
                                analyticsEventCommand.Track("ServiceName", "PaymentMSCapture");
                                var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSCaptureRS, _hostEnvironment);

                                analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);

                                var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                                await _mediator.Send(orderCancelRQ, cancellationToken);
                                
                                analyticsEventCommand.Track("OrderCancelStatus", true);

                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, currencies, amount.ToString(), paymentMSAuthorizeRS, orderTicketMSRS.OrderTicketDocument.AccountableDocumentRef);
                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                                
                                analyticsEventCommand.Track("PaymentMSRefundStatus", true);
                                return errorDetails;
                            }
                    }

                    if (isIFGIntegrated && !isUsAgent)
                    {
                        // do nothing
                    }
                    else
                    {
                        OrderDocumentMapper.UpdateFopPaymentDetails(orderMSRS.OrderDocument, paymentMSAuthorizeRS, null, false, paymentMSCaptureRS);
                    }

                    TransactionServiceResponse transactionresponse = null;
                    if (isUsAgent)
                    {
                        var transactionRequest = TransactionRequestMapper.MapToTransactionRequest(orderMSRS.OrderDocument, paymentMSCaptureRS, orderTicketMSRS?.OrderTicketDocument?.AccountableDocumentRef, Constants.ProductType.Ticket, false, false, false);

                        transactionresponse = await _transactionCommand.Handle(transactionRequest);

                        if (transactionresponse.ErrorDetails != null)
                        {
                            nDCWarnings.AddRange(OrderCreateValidation.PaymentReportingFailureWarning(_hostEnvironment));
                        }

                        var paymentDetails = orderMSRS.OrderDocument.OrderItems.Where(oi => oi.PaymentDetails != null).SelectMany(oi => oi.PaymentDetails);
                        foreach (var pd in paymentDetails)
                        {
                            pd.TrackId = transactionresponse.RequestId;
                            pd.PaymentProcessingStatus = (transactionresponse.ErrorDetails == null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING;
                        }
                    }


                    #region Seat FulFillment

                    if ((orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)))
                    {
                        var seatDetails = GetSeatDetails(seatOfferDoc, (orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)), orderMSRS.OrderDocument);

                        var seatRetrieveRQ = RetrieveSeatMapRequestMapper.MapSeatAvailabilityMSRequestOrderDoc(orderMSRS.OrderDocument, orderCreateCommand.OrderCreateRQ, orderTicketMSRS.OrderTicketDocument.BookingReference.ExternalRecordLocator);
                        var seatRetrieveRS = await _mediator.Send(seatRetrieveRQ, cancellationToken);

                        if (seatRetrieveRS != null && seatRetrieveRS.Error == null && seatRetrieveRS.SeatAvailabilityResponse != null)
                        {
                            List<string> invalidSeats = [];
                            nDCWarnings.AddRange(SeatMapValidations.ValidateSeatDetails(seatRetrieveRS.SeatAvailabilityResponse, seatDetails, invalidSeats, orderCreateCommand.OrderCreateRQ.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode, (orderCreateCommand.OrderCreateRQ.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).TotalOfferPriceAmount.Value, _hostEnvironment));

                            nDCWarnings.AddRange(SeatMapValidations.ValidateInvalidSegments(invalidSeats, seatDetails, _hostEnvironment));

                            seatDetails = seatDetails.Where(x => !invalidSeats.Any(y => x.SeatNumber == y.Split("-").First() && x.AssociatedSegment.MarketingFlightNumber == y.Split("-").Last()));

                            if (seatDetails.Any())
                            {
                                string countryCode = orderMSRS.OrderDocument.PointOfSale.CountryCode;
                                var cityCodeData = await _cityCodeQuery.Handle(countryCode, cancellationToken);

                                bool isCityValid = cityCodeData != null && !string.IsNullOrEmpty(cityCodeData.NDCCityCodeLocation);
                                if (isCityValid)
                                {
                                    orderMSRS.OrderDocument.PointOfSale.CityCode = cityCodeData.CityCode;
                                    orderMSRS.OrderDocument.PointOfSale.NDCCityCodeLocation = cityCodeData.NDCCityCodeLocation;

                                    var seatFulfilmentRQ = SeatFulfilmentRequestMapper.MapSeatRequest(orderMSRS.OrderDocument, seatDetails, seatRetrieveRS.SeatAvailabilityResponse);

                                    var successfullyAuthSegs = new Dictionary<IEnumerable<string>, (PaymentMSAuthorizeCommand, Infrastructure.Model.PaymentMS.PaymentRS)>();
                                    var successfullyIFGSeatAuthSegs = new Dictionary<IEnumerable<string>, (PaymentIFGMSAuthorizeCommand, Infrastructure.Model.PaymentMS.PaymentRS)>();

                                    foreach (var item in orderMSRS.OrderDocument.DataList.Journey)
                                    {
                                        var segsInJour = item.FlightSegmentRefIds.Select(x => x.Replace("SEG", "").PadLeft(2, '0'));
                                        var jourAmount = seatFulfilmentRQ.SelectedSeatInfo.Where(s => segsInJour.Contains(s.FlightIdRef))?.Sum(x => Convert.ToDecimal(x.Amount.MonetaryAmount)).ToString();

                                        if (Convert.ToDecimal(jourAmount) != 0)
                                        {
                                            //need to remove the if condition after ifg integration is completed
                                            if (Constants.Application.IsSeatReportingEnabled)
                                            {
                                                var seatAmount = jourAmount.Insert(jourAmount.Length - seatFulfilmentRQ.SelectedSeatInfo.First().Amount.DecimalPrecisionCount, ".");
                                                var paymentIFGAuthorizeRQForSeat = PaymentMSRequestMapper.MapOrderCreateCCIFGAuthorizeRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, offerDoc, currencies, airportDetails, amount.ToString(), Constants.ProductType.Seat);
                                                var paymentIFGAuthorizeRSForSeat = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQForSeat, cancellationToken);

                                                if (paymentIFGAuthorizeRSForSeat?.Errors == null)
                                                {
                                                    successfullyIFGSeatAuthSegs.Add(segsInJour, (paymentIFGAuthorizeRQForSeat, paymentIFGAuthorizeRSForSeat));

                                                    seatFulfilmentRQ.SeatTransactionIds = seatFulfilmentRQ.SeatTransactionIds.Concat(segsInJour.Select(seg =>
                                                    {
                                                        return new SeatTransactionId()
                                                        {
                                                            SegmentId = seg,
                                                            TransactionId = paymentIFGAuthorizeRSForSeat.PaymentDocument.TrackId
                                                        };
                                                    }));

                                                    transactionStatus.Add(new TransactionIdStatus() { TransactionId = paymentIFGAuthorizeRSForSeat.PaymentDocument.TrackId, PaymentStatus = true, Amount = Convert.ToDecimal(seatAmount) });
                                                }
                                            }

                                            var paymentMSAuthorizeRQForSeat = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, offerDoc, currencies, airportDetails, jourAmount.Insert(jourAmount.Length - seatFulfilmentRQ.SelectedSeatInfo.First().Amount.DecimalPrecisionCount, "."), Constants.ProductType.Seat);
                                            var paymentMSAuthorizeRSForSeat = await _mediator.Send(paymentMSAuthorizeRQForSeat, cancellationToken);

                                            if (paymentMSAuthorizeRSForSeat?.Errors == null)
                                            {
                                                successfullyAuthSegs.Add(segsInJour, (paymentMSAuthorizeRQForSeat, paymentMSAuthorizeRSForSeat));
                                                seatFulfilmentRQ.SeatTransactionIds = seatFulfilmentRQ.SeatTransactionIds.Concat(segsInJour.Select(seg =>
                                                {
                                                    return new SeatTransactionId()
                                                    {
                                                        SegmentId = seg,
                                                        TransactionId = paymentMSAuthorizeRSForSeat.PaymentDocument.TransactionId
                                                    };

                                                }));

                                                transactionStatus.Add(new TransactionIdStatus() { TransactionId = paymentMSAuthorizeRSForSeat.PaymentDocument.TransactionId, PaymentStatus = true, Amount = paymentMSAuthorizeRSForSeat.PaymentDocument.Amount });

                                            }
                                            else
                                            {
                                                analyticsEventCommand.Track("ServiceName", "SeatPaymentAuth");
                                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeRSForSeat.Errors.First().Id);
                                            }
                                        }
                                    }

                                    if (successfullyAuthSegs.Count != 0 || Convert.ToDecimal(seatFulfilmentRQ.SelectedSeatInfo.Sum(s => Convert.ToDecimal(s.Amount.MonetaryAmount))) == 0)
                                    {
                                        if (successfullyAuthSegs.Count != 0)
                                        {
                                            seatFulfilmentRQ.SelectedSeatInfo = seatFulfilmentRQ.SelectedSeatInfo.Where(s => successfullyAuthSegs.SelectMany(x => x.Key).Any(y => y == s.FlightIdRef));
                                            seatFulfilmentRQ.paymentDetails.First().Amount.MonetaryAmount = seatFulfilmentRQ.SelectedSeatInfo.Sum(s => Convert.ToDecimal(s.Amount.MonetaryAmount)).ToString();
                                        }

                                        seatFulfilmentRS = await _mediator.Send(new SeatMSSeatFulfilmentCommand() { SeatFulfilmentRequest = seatFulfilmentRQ }, cancellationToken);

                                        if (seatFulfilmentRS == null || ((bool)seatFulfilmentRS.Error?.Any()))
                                        {
                                            analyticsEventCommand.Track("ServiceName", "SeatFulfilmentMS");
                                            analyticsEventCommand.Track("ErrorID", seatFulfilmentRS?.Error.First().Id);

                                            foreach (var item in successfullyAuthSegs)
                                            {
                                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, currencies, item.Value.Item2.PaymentDocument.Amount.ToString(), item.Value.Item2, null);
                                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                                            }
                                        }
                                        else
                                        {
                                            analyticsEventCommand.Track("SeatBooked", "Y");
                                        }
                                    }

                                    nDCWarnings.AddRange(SeatMapValidations.ValidateSeatMapResponse(seatFulfilmentRS, seatDetails, _hostEnvironment, nDCWarnings));

                                    OrderDocumentMapper.UpdateOrderDocument(orderMSRS.OrderDocument, null, seatFulfilmentRS, transactionStatus);

                                    if (seatFulfilmentRS?.OrderItems != null && seatFulfilmentRS.OrderItems.Any() && Convert.ToDecimal(seatFulfilmentRQ.SelectedSeatInfo.Sum(s => Convert.ToDecimal(s.Amount.MonetaryAmount))) != 0)
                                    {
                                        //need to remove the if condition after ifg integration is completed
                                        if (Constants.Application.IsSeatReportingEnabled)
                                        {
                                            foreach (var item in successfullyIFGSeatAuthSegs)
                                            {
                                                var paymentIFGOrderRQForSeat = PaymentMSRequestMapper.MapOrderCreateCCIFGOrderRequest(item.Value.Item1, seatFulfilmentRS.OrderItems.Where(o => item.Key.Contains(o.FlightIdRef)).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.ElectronicMiscellaneousDocumentNumber }), item.Value.Item2, Constants.ProductType.Seat, orderTicketMSRS?.OrderTicketDocument?.BookingReference, orderMSRS.OrderDocument);
                                                var paymentIFGOrderRSForSeat = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQForSeat, cancellationToken);

                                                if (paymentIFGOrderRSForSeat.Errors != null)
                                                {
                                                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                                                }
                                            }
                                        }

                                        foreach (var item in successfullyAuthSegs)
                                        {
                                            var paymentMsCaptureRqForSeat = PaymentMSRequestMapper.MapOrderCreateCaptureRequest(item.Value.Item1, seatFulfilmentRS.OrderItems.Where(o => item.Key.Contains(o.FlightIdRef)).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.ElectronicMiscellaneousDocumentNumber }), item.Value.Item2, offerDoc, Constants.ProductType.Seat, orderTicketMSRS?.OrderTicketDocument?.BookingReference, orderMSRS.OrderDocument);
                                            var paymentMsCaptureRsForSeat = await _mediator.Send(paymentMsCaptureRqForSeat, cancellationToken);

                                            if (paymentMsCaptureRsForSeat.Errors != null)
                                            {
                                                analyticsEventCommand.Track("ServiceName", "PaymentMSCaptureSeat");
                                                analyticsEventCommand.Track("ErrorId", paymentMsCaptureRsForSeat?.Errors?.FirstOrDefault()?.Id);

                                                orderMSRS.OrderDocument.BookingReference.ExternalRecordLocator = orderTicketMSRS.OrderTicketDocument.BookingReference.ExternalRecordLocator;
                                                var seatDelReq = OrderMgmtSeatRequestMapper.MapOrderSeatDeletionRQ(item.Key, seatDetails, null, null, orderMSRS.OrderDocument);
                                                await _mediator.Send(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = seatDelReq }, cancellationToken);

                                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, orderCreateCommand.OrderCreateRQ, currencies, item.Value.Item2.PaymentDocument.Amount.ToString(), item.Value.Item2, seatFulfilmentRS.OrderItems.Where(o => item.Key.Contains(o.FlightIdRef)).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.ElectronicMiscellaneousDocumentNumber }));
                                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                                            }

                                            else
                                            {
                                                OrderDocumentMapper.UpdateSeatPaymentDetails(orderMSRS.OrderDocument, paymentMsCaptureRsForSeat);
                                            }

                                        }

                                    }
                                }
                                else nDCWarnings.AddRange(SeatMapValidations.ValidateCityCodeResponse(_hostEnvironment));
                            }
                            else nDCWarnings.Remove(nDCWarnings.FirstOrDefault(x => x.Code == "992"));
                        }
                        else nDCWarnings.AddRange(SeatMapValidations.ValidateSeatRetrieveResponse(_hostEnvironment));
                    }

                    #endregion

                    if (orderMSRS.OrderDocument.Warnings is not null)
                    {
                        nDCWarnings.AddRange(OrderCreateValidation.ValidateSSROrderItems(orderMSRS.OrderDocument, _hostEnvironment));
                        analyticsEventCommand.Track("WarningCode", orderMSRS.OrderDocument.Warnings?.FirstOrDefault()?.Code);
                    }

                    if (isIFGIntegrated && !isUsAgent)
                    {
                        var paymentDocs = await _paymentMSRetrieveQueryHandler.Handle(new PaymentMSRetrieveQuery()
                        {
                            OrderID = orderMSRS.OrderDocument.OrderNumber.OrderId
                        }, cancellationToken);

                        var ticketPaymentDoc = paymentDocs.First(x => x.Id == ticketPaymentTransactionId);

                        OrderDocumentMapper.UpdateCCPaymentDetails(orderMSRS.OrderDocument, ticketPaymentDoc, ticketTrackId);
                    }

                }

                var totalAmount = orderMSRS.OrderDocument?.OrderItems?.Sum(orderItem =>
                {
                    var basePrice = orderItem?.PriceDetails?.Where(x => x?.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Sum(x => Convert.ToDecimal(x?.Amount?.MonetaryAmount));

                    var paxCount = orderItem?.Services?.Where(s => s?.ServiceBeneficiaries != null)?.SelectMany(s => s?.ServiceBeneficiaries)?.Distinct()?.Count();
                    return basePrice * paxCount;
                });

                analyticsEventCommand.Track("TotalAmount", totalAmount);

                var segmentCount = orderMSRS.OrderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                analyticsEventCommand.Track("SegmentCount", segmentCount);

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderMSRS.OrderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                if (!isHoldBooking)
                {                   
                    if (seatFulfilmentRS?.OrderItems != null)
                        OrderDocumentMapper.UpdateSeatProductDetails(orderMSRS.OrderDocument, seatOfferDoc);

                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.OrderCreate, orderMSRS.OrderDocument);
                    _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);                   
                }
                else
                {
                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.HoldBooking, orderMSRS.OrderDocument);
                   _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);
                }

                var orderRS = OrderResponseMapper.MapOrderResponse(orderMSRS.OrderDocument, nDCWarnings, isHoldBooking);

                LogOrderDetails(orderRS, orderMSRS.OrderDocument, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderRS };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Order Create processing was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order Create processing failed with an unhandled exception");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await TrackAnalyticsEventAsync(analyticsEventCommand, cancellationToken);
            }
        }

        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

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
                _logger.LogError(ex, "Failed to add Order Create success analytics details");
            }
        }

        private void LogErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                if (responseMessage?.Errors?.Any() != true)
                {
                    _logger.LogWarning("Order Create error analytics was requested without response errors");
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
                _logger.LogError(ex, "Failed to add Order Create error analytics details");
            }
        }

        private async Task TrackAnalyticsEventAsync(TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
        {
            try
            {
                var wasPublished = await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
                if (!wasPublished)
                {
                    _logger.LogError("Order Create analytics event publication was not completed");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Order Create analytics event");
            }
        }

        private static IEnumerable<SeatDetails> GetSeatDetails(SeatRS seatOfferDoc, SelectedPricedOfferType seatPriceOffer, OrderDocument orderDocument)
        {
            return seatPriceOffer.SelectedOfferItem.Select(offerItem =>
            {
                var seatMap = seatOfferDoc.SeatAvailabilityResponse.FlightSegmentSeatMap.FirstOrDefault(x => x.Catalogue.Any(y => y.CatalogueId == offerItem.OfferItemRefID));
                var catalogue = seatMap.Catalogue.FirstOrDefault(x => x.CatalogueId == offerItem.OfferItemRefID);

                return new SeatDetails()
                {
                    SeatNumber = offerItem.SelectedSeat.SeatRowNumber + offerItem.SelectedSeat.ColumnID,
                    AssociatedSegment = seatMap.FlightSegment,
                    Beneficiary = orderDocument.DataList.CustomerDetails.First(x => x.CustId == offerItem.PaxRefID.First())
                };
            });
        }
    }
}
