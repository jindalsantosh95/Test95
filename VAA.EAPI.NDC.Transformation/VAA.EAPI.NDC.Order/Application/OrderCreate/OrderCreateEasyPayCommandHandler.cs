using CSharpFunctionalExtensions;
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
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
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

namespace VAA.EAPI.NDC.Order.Application.OrderCreate
{
    public class OrderCreateEasyPayCommandHandler : IOrderCreateEasyPayCommand
    {
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OfferMSQueryHandler _offerMSQueryHandler;
        private readonly ReferenceMSCurrencyQueryHandler _referenceMSCurrencyQueryHandler;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommand;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly OrderMSCommandHandler _orderMSCommandHandler;
        private readonly OrderTicketMSCommandHandler _orderTicketMSCommandHandler;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<OrderCreateEasyPayCommandHandler> _logger;
        private readonly SeatMSQueryHandler _seatMSQueryHandler;
        private readonly SeatMSRetrieveSeatMapQueryHandler _seatMSRetrieveSeatMapQueryHandler;
        private readonly SeatMSSeatFulfillmentCommandHandler _seatMSSeatFulfillmentCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly PaymentMSAuthorizeCommandHandler _paymentMSAuthorizeCommandHandler;
        private readonly PaymentMSCaptureCommandHandler _paymentMSCaptureCommandHandler;
        private readonly PaymentMSRefundCommandHandler _paymentMSRefundCommandHandler;
        private readonly OrderManagementMSCommandHandler _orderManagementMSCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;
        private readonly OrderCancelCommandHandler _orderCancelCommandHandler;

        public OrderCreateEasyPayCommandHandler(ILogger<OrderCreateEasyPayCommandHandler> logger, IHostEnvironment hostEnvironment, ValidationMSCommandHandler validationMSCommandHandler, OfferMSQueryHandler offerMSQueryHandler, ReferenceMSCurrencyQueryHandler referenceMSCurrencyQuery, AirportDetailsQueryHandler airportDetailsQueryHandler, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommand, PaymentIFGMSOrderCommandHandler paymentIFGMSOrderCommandHandler, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, OrderMSCommandHandler orderMSCommandHandler, OrderTicketMSCommandHandler orderTicketMSCommandHandler, SeatMSQueryHandler seatMSQueryHandler, SeatMSRetrieveSeatMapQueryHandler seatMSRetrieveSeatMapQueryHandler, SeatMSSeatFulfillmentCommandHandler seatMSSeatFulfillmentCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, ICreatePaymentDocumentCommand createPaymentDocumentCommand, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, Infrastructure.Services.IFGAgentFopLookUp.IFGAgentFopLookUpCommandHandler iFGAgentFopLookUpCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, PaymentMSAuthorizeCommandHandler paymentMSAuthorizeCommandHandler, PaymentMSCaptureCommandHandler paymentMSCaptureCommandHandler , PaymentMSRefundCommandHandler paymentMSRefundCommandHandler, OrderManagementMSCommandHandler orderManagementMSCommandHandler, ICityCodeQuery cityCodeQuery, OrderCancelCommandHandler orderCancelCommandHandler   )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _offerMSQueryHandler = offerMSQueryHandler ?? throw new ArgumentNullException(nameof(offerMSQueryHandler));
            _referenceMSCurrencyQueryHandler = referenceMSCurrencyQuery ?? throw new ArgumentNullException(nameof(referenceMSCurrencyQuery));
            _airportDetailsQueryHandler = airportDetailsQueryHandler ?? throw new ArgumentNullException(nameof(airportDetailsQueryHandler));
            _paymentIFGMSAuthorizeCommand = paymentIFGMSAuthorizeCommand ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommand));
            _paymentIFGMSOrderCommandHandler = paymentIFGMSOrderCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _orderMSCommandHandler = orderMSCommandHandler ?? throw new ArgumentNullException(nameof(orderMSCommandHandler));
            _orderTicketMSCommandHandler = orderTicketMSCommandHandler ?? throw new ArgumentNullException(nameof(orderTicketMSCommandHandler));
            _seatMSQueryHandler = seatMSQueryHandler ?? throw new ArgumentNullException(nameof(seatMSQueryHandler));
            _seatMSRetrieveSeatMapQueryHandler = seatMSRetrieveSeatMapQueryHandler ?? throw new ArgumentNullException(nameof(seatMSRetrieveSeatMapQueryHandler));
            _seatMSSeatFulfillmentCommandHandler = seatMSSeatFulfillmentCommandHandler ?? throw new ArgumentNullException(nameof(seatMSSeatFulfillmentCommandHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _iFGAgentFopLookUpCommandHandler = iFGAgentFopLookUpCommandHandler ?? throw new ArgumentNullException(nameof(iFGAgentFopLookUpCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _paymentMSAuthorizeCommandHandler = paymentMSAuthorizeCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSAuthorizeCommandHandler));
            _paymentMSCaptureCommandHandler = paymentMSCaptureCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSCaptureCommandHandler));
            _paymentMSRefundCommandHandler = paymentMSRefundCommandHandler ?? throw new ArgumentNullException(nameof(paymentMSRefundCommandHandler));
            _orderManagementMSCommandHandler = orderManagementMSCommandHandler ?? throw new ArgumentNullException(nameof(orderManagementMSCommandHandler));
            _cityCodeQuery = cityCodeQuery ?? throw new ArgumentNullException(nameof(cityCodeQuery));
            _orderCancelCommandHandler = orderCancelCommandHandler ?? throw new ArgumentNullException(nameof(orderCancelCommandHandler));

        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderCreateRQ request, bool isEasyPay, string subscriptionKey, CancellationToken cancellationToken)
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
                analyticsEventCommand.Track("RequestType", "OrderCreate_EasyPay");

                bool isHoldBooking = request?.Request?.PaymentFunctions?.FirstOrDefault() == null;

                var distributionChain = request?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var orderCreateRQValidationErrors = OrderCreateValidation.OrderCreateRQValidation<IATA_OrderViewRS>(request, isHoldBooking, false, false, _hostEnvironment);

                if (orderCreateRQValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderCreateRQValidation");
                    analyticsEventCommand.Track("ErrorId", orderCreateRQValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderCreateRQValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderCreateRQValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRQ = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderCreate(request) }, null, true);
                var validationMSRS = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRQ, AgentSubscriptionKey = subscriptionKey }, cancellationToken);
                
                const string TidsAgencyIdType = "TIDS";
                bool isNonUsAgent = validationMSRS.ValidateResponse?.AgencyDetails?.CountryCode != "US";
                bool isTidsAgent = validationMSRS.ValidateResponse?.AgencyDetails?.RequestAgencyIdType == TidsAgencyIdType;

                if (validationMSRS != null)
                {
                    var easyPayValidation = AgencyValidation.ValidateEasyPayFOP<IATA_OrderViewRS>(validationMSRS, _hostEnvironment, isEasyPay);

                    if (easyPayValidation?.ResponseMessages?.Errors.Any() == true)
                    {
                        analyticsEventCommand.Track("ServiceName", "easyPayValidation");
                        analyticsEventCommand.Track("ErrorId", easyPayValidation?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                        
                        LogErrorDetails(easyPayValidation?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(easyPayValidation?.ResponseMessages);
                        return errorDetails;
                    }
                }

                if (validationMSRS.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSRS, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestOrgName);
                if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }
                analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId);

                var offerId = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault()?.OfferRefID;
                analyticsEventCommand.Track("IATAOfferID", offerId); 
                
                var offerDoc = await _offerMSQueryHandler.Handle((OfferMSQuery)offerId, cancellationToken);
                offerDoc = OfferDocMapper.UpdateOfferDocWithAgentType(offerDoc, validationMSRS);

                var offerDocValidationErrors = OrderCreateValidation.ValidateOfferOrder<IATA_OrderViewRS>(request, offerDoc, validationMSRS, isHoldBooking, _hostEnvironment);

                if (offerDocValidationErrors is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferMS");
                    analyticsEventCommand.Track("ErrorId", offerDocValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(offerDocValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(offerDocValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                #region Seat Validation

                SeatRS seatOfferDoc = new();
                SeatFulfilmentResponse seatFulfilmentRS = null;
                List<NDCWarning> seatWarnings = [];

                if ((request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)))
                {
                    var seatmsOfferID = (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).OfferRefID;
                    seatOfferDoc = await _seatMSQueryHandler.Handle(new SeatMSQuery() { OfferID = seatmsOfferID }, cancellationToken);
                    var seatValidationError = OrderCreateValidation.ValidateSeatOfferDetails<IATA_OrderViewRS>(request, seatOfferDoc?.SeatAvailabilityResponse?.FlightSegmentSeatMap, _hostEnvironment);

                    if (seatValidationError != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatMS");
                        analyticsEventCommand.Track("ErrorId", seatValidationError?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(seatValidationError?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(seatValidationError?.ResponseMessages);
                        return errorDetails;
                    }

                    var seatPaymentValidationWarnings = OrderCreateValidation.ValidateSeatCCFOP(request, seatmsOfferID, _hostEnvironment);
                    seatWarnings.AddRange(seatPaymentValidationWarnings);
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

                var currencyRS = await _referenceMSCurrencyQueryHandler.Handle(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);

                if (currencyRS is null) analyticsEventCommand.Track("ServiceName", "ReferenceCurrencyMS_Failure");

                var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);
                var amount = (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Where(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null)).Sum(x => x?.TotalOfferPriceAmount?.Value);

                PaymentRS paymentIFGAuthorizeRS = null;
                OrderTicketRS orderTicketMSRS;
                OrderRS orderMSRS = null;

                var airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);
                
                if (airportDetails?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                if (isNonUsAgent)
                {
                    var paymentIFGAuthorizeRQ = PaymentMSRequestMapper.MapOrderCreateIFGAuthorizeRequest(request, offerDoc, airportDetails, amount.ToString(), Constants.ProductType.Ticket, isEasyPay);
                    paymentIFGAuthorizeRS = await _paymentIFGMSAuthorizeCommand.Handle(paymentIFGAuthorizeRQ, cancellationToken);

                    if (paymentIFGAuthorizeRS?.Errors != null && paymentIFGAuthorizeRS.Errors.Any())
                    {
                        analyticsEventCommand.Track("ServiceName", "PaymentIFGAuthorize");
                        var message = paymentIFGAuthorizeRS?.Errors?.FirstOrDefault()?.Message;
                        var errorCode = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("IFGCode"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                        analyticsEventCommand.Track("ErrorId", errorCode);
                        var trackId = message?.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("TrackId"))?.Split(':', 2).ElementAtOrDefault(1)?.Trim();

                        await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = trackId }, cancellationToken);
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }
                   
                    var orderMSRQ = OrderMSRequestMapper.MapOrderMSRequest(request, offerDoc, false, null);
                    orderMSRS = await _orderMSCommandHandler.Handle(orderMSRQ, cancellationToken);

                    if (orderMSRS.Errors is not null)
                    {
                        if (orderMSRS.Errors.Any(x => x.Code == "F002"))
                        {
                            var error = orderMSRS.Errors.FirstOrDefault(x => x.Code == "F002");
                            string rloc = error.DeveloperMessage?.Split("RecordLocator:").LastOrDefault()?.Trim();
                            BookingReference bookingReference = new BookingReference { RLOC = rloc };

                            var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(bookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties, false, null);
                            var orderCancelStatus = await _orderCancelCommandHandler.Handle(orderCancelRQ, cancellationToken);

                            analyticsEventCommand.Track("OrderCancelStatus", orderCancelStatus);
                        }
                        var orderMSValidtionError = OrderMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderMSRS, _hostEnvironment);

                        analyticsEventCommand.Track("ServiceName", "OrderMS");
                        analyticsEventCommand.Track("ErrorId", orderMSValidtionError?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId }, cancellationToken);
                        LogErrorDetails(orderMSValidtionError?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(orderMSValidtionError?.ResponseMessages);
                        return errorDetails;
                    }

                    // calling IFG Agent LookUp API to check agent permission
                    if (!isTidsAgent)
                    {
                        IFGAgentFopLookUpCommand ifgAgentFopLookUpRequest = null;
                        ifgAgentFopLookUpRequest = IFGAgentFopLookUpRequestMapper.MapIfgAgentLookUpRequest(validationMSRS);

                        var ifgAgentFopLookUpResponse = await _iFGAgentFopLookUpCommandHandler.Handle(ifgAgentFopLookUpRequest, cancellationToken);
                        var validateIFGAgentsPermission = OrderCreateValidation.ValidateIFGAgentsPermission(request, ifgAgentFopLookUpResponse, _hostEnvironment);

                        if (validateIFGAgentsPermission is not null)
                        {
                            analyticsEventCommand.Track("ServiceName", "iFGAgentFopLookUp Validation");
                            analyticsEventCommand.Track("ErrorId", validateIFGAgentsPermission.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                            LogErrorDetails(validateIFGAgentsPermission?.ResponseMessages, analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(validateIFGAgentsPermission.ResponseMessages);
                        }

                    }                    

                    var orderTicketMSRQ = OrderTicketMSRequestMapper.MapOrderTicketMSRequest(request, offerDoc, orderMSRS.OrderDocument, null, false, isEasyPay);
                    orderTicketMSRS = await _orderTicketMSCommandHandler.Handle(orderTicketMSRQ, cancellationToken);

                    if (orderTicketMSRS.Errors is not null)
                    {
                        var orderCancelRQ = OrderCancelRequestMapper.MapOrderCancelRequest(orderMSRS.OrderDocument.BookingReference, true, Infrastructure.Model.OrderManagementMS.OrderChangeRequest.Operations.OrderCreate, null, offerDoc.Parties);
                        await _orderCancelCommandHandler.Handle(orderCancelRQ, cancellationToken);

                        analyticsEventCommand.Track("OrderCancelStatus", true);
                        analyticsEventCommand.Track("ServiceName", "OrderTicketMS");
                        analyticsEventCommand.Track("ErrorId", orderTicketMSRS.Errors?.FirstOrDefault()?.Id);

                        await _paymentIFGMSAuthorizeCancelCommandHandler.Handle(new AuthorizePaymentRequest() { TrackId = paymentIFGAuthorizeRS.PaymentDocument.TrackId }, cancellationToken);
                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }
                    OrderDocumentMapper.UpdateOrderDocument(orderMSRS.OrderDocument, orderTicketMSRS.OrderTicketDocument, null, null, offerDoc.DataList.PriceClass);

                    var paymentIFGOrderRQ = PaymentMSRequestMapper.MapOrderCreateIFGOrderRequest(paymentIFGAuthorizeRQ, orderTicketMSRS?.OrderTicketDocument?.AccountableDocumentRef, paymentIFGAuthorizeRS, offerDoc, airportDetails, orderMSRS.OrderDocument, orderTicketMSRS.OrderTicketDocument.BookingReference, Constants.ProductType.Ticket);
                    var paymentIFGOrderRS = await _paymentIFGMSOrderCommandHandler.Handle(paymentIFGOrderRQ, cancellationToken);

                    if (paymentIFGOrderRS?.Errors != null && paymentIFGOrderRS.Errors.Any())
                    {
                        nDCWarnings.AddRange(OrderCreateValidation.PaymentReportingFailureWarning(_hostEnvironment));
                    }

                    var easyPayCardNumber = (request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item as PaymentProcessingIATA_EasyPayType).AccountNumber;
                    var paymentDocumentRequest = PaymentMSRequestMapper.MapPaymentDocumentRequest(paymentIFGAuthorizeRS, paymentIFGAuthorizeRQ, orderMSRS.OrderDocument, Constants.Application.PaymentDocType, false, false, isEasyPay, null, null, easyPayCardNumber);
                    var paymentDocumentResponse = await _createPaymentDocumentCommand.Handle(paymentDocumentRequest, cancellationToken);

                    if (paymentDocumentResponse.PaymentDocument != null)
                        OrderDocumentMapper.UpdateFopPaymentDetails(orderMSRS.OrderDocument, paymentIFGAuthorizeRS, paymentDocumentResponse.PaymentDocument, isEasyPay, null, paymentIFGOrderRS, false, null, paymentDocumentRequest.PaymentDetails.First().CardMethod);

                    #region Seat FulFillment

                    if ((request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)) && seatWarnings.Count == 0)
                    {
                        var seatDetails = GetSeatDetails(seatOfferDoc, (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)), orderMSRS.OrderDocument);

                        var seatRetrieveRQ = RetrieveSeatMapRequestMapper.MapSeatAvailabilityMSRequestOrderDoc(orderMSRS.OrderDocument, request, orderTicketMSRS.OrderTicketDocument.BookingReference.ExternalRecordLocator);
                        var seatRetrieveRS = await _seatMSRetrieveSeatMapQueryHandler.Handle(seatRetrieveRQ, cancellationToken);

                        if (seatRetrieveRS != null && seatRetrieveRS.Error == null && seatRetrieveRS.SeatAvailabilityResponse != null)
                        {
                            List<string> invalidSeats = [];
                            nDCWarnings.AddRange(SeatMapValidations.ValidateSeatDetails(seatRetrieveRS.SeatAvailabilityResponse, seatDetails, invalidSeats, request.Request.PaymentFunctions.First().PaymentProcessingDetails.Amount.CurCode, (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).TotalOfferPriceAmount.Value, _hostEnvironment));

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

                                    foreach (var item in orderMSRS.OrderDocument.DataList.Journey)
                                    {
                                        var segsInJour = item.FlightSegmentRefIds.Select(x => x.Replace("SEG", "").PadLeft(2, '0'));
                                        var jourAmount = seatFulfilmentRQ.SelectedSeatInfo.Where(s => segsInJour.Contains(s.FlightIdRef))?.Sum(x => Convert.ToDecimal(x.Amount.MonetaryAmount)).ToString();

                                        if (Convert.ToDecimal(jourAmount) != 0)
                                        {
                                            var seatAmount = jourAmount.Insert(jourAmount.Length - seatFulfilmentRQ.SelectedSeatInfo.First().Amount.DecimalPrecisionCount, ".");
                                            var seatPaymentDetails = request.Request.PaymentFunctions.FirstOrDefault(x => x.OfferAssociation?.Any(y => y.OfferRefID == (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).OfferRefID) == true)?.PaymentProcessingDetails;
                                            var paymentMSAuthorizeRQForSeat = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderMSRS, request, offerDoc, currencies, airportDetails, jourAmount.Insert(jourAmount.Length - seatFulfilmentRQ.SelectedSeatInfo.First().Amount.DecimalPrecisionCount, "."), Constants.ProductType.Seat, seatPaymentDetails);
                                            var paymentMSAuthorizeRSForSeat = await _paymentMSAuthorizeCommandHandler.Handle(paymentMSAuthorizeRQForSeat, cancellationToken);

                                            if (paymentMSAuthorizeRSForSeat != null && paymentMSAuthorizeRSForSeat?.Errors == null && paymentMSAuthorizeRSForSeat.PaymentDocument != null)
                                            {
                                                successfullyAuthSegs.Add(segsInJour, (paymentMSAuthorizeRQForSeat, paymentMSAuthorizeRSForSeat));
                                                seatFulfilmentRQ.SeatTransactionIds = seatFulfilmentRQ.SeatTransactionIds.Concat(segsInJour.Select(seg =>
                                                {
                                                    return new SeatTransactionId()
                                                    {
                                                        SegmentId = seg,
                                                        TransactionId = paymentMSAuthorizeRSForSeat?.PaymentDocument?.TransactionId
                                                    };

                                                }));

                                                transactionStatus.Add(new TransactionIdStatus() { TransactionId = paymentMSAuthorizeRSForSeat.PaymentDocument?.TransactionId, PaymentStatus = true, Amount = paymentMSAuthorizeRSForSeat.PaymentDocument.Amount });

                                            }
                                            else
                                            {
                                                analyticsEventCommand.Track("ServiceName", "SeatPaymentAuth");
                                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeRSForSeat?.Errors?.FirstOrDefault()?.Id);
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

                                        seatFulfilmentRS = await _seatMSSeatFulfillmentCommandHandler.Handle(new SeatMSSeatFulfilmentCommand() { SeatFulfilmentRequest = seatFulfilmentRQ }, cancellationToken);

                                        if (seatFulfilmentRS == null || seatFulfilmentRS?.Error?.Any() == true)
                                        {
                                            var seatPaymentDetails = request.Request.PaymentFunctions.FirstOrDefault(x => x.OfferAssociation.Any(y => y.OfferRefID == (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).OfferRefID))?.PaymentProcessingDetails;
                                            analyticsEventCommand.Track("ServiceName", "SeatFulfilmentMS");
                                            analyticsEventCommand.Track("ErrorID", seatFulfilmentRS?.Error?.FirstOrDefault()?.Id);

                                            foreach (var item in successfullyAuthSegs)
                                            {
                                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, request, currencies, item.Value.Item2.PaymentDocument.Amount.ToString(), item.Value.Item2, null, seatPaymentDetails);
                                                await _paymentMSRefundCommandHandler.Handle(paymentMSRefundRQ, cancellationToken);
                                            }
                                        }
                                        else { analyticsEventCommand.Track("IsSeatBooked", "Y"); }
                                    }

                                    nDCWarnings.AddRange(SeatMapValidations.ValidateSeatMapResponse(seatFulfilmentRS, seatDetails, _hostEnvironment, nDCWarnings));

                                    OrderDocumentMapper.UpdateOrderDocument(orderMSRS.OrderDocument, null, seatFulfilmentRS, transactionStatus);

                                    if (seatFulfilmentRS?.OrderItems != null && seatFulfilmentRS.OrderItems.Any() && Convert.ToDecimal(seatFulfilmentRQ.SelectedSeatInfo.Sum(s => Convert.ToDecimal(s.Amount.MonetaryAmount))) != 0)
                                    {
                                        foreach (var item in successfullyAuthSegs)
                                        {
                                            var paymentMsCaptureRqForSeat = PaymentMSRequestMapper.MapOrderCreateCaptureRequest(item.Value.Item1, seatFulfilmentRS.OrderItems.Where(o => item.Key.Contains(o.FlightIdRef)).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.ElectronicMiscellaneousDocumentNumber }), item.Value.Item2, offerDoc, Constants.ProductType.Seat, orderTicketMSRS?.OrderTicketDocument?.BookingReference, orderMSRS.OrderDocument);
                                            var paymentMsCaptureRsForSeat = await _paymentMSCaptureCommandHandler.Handle(paymentMsCaptureRqForSeat, cancellationToken);

                                            if (paymentMsCaptureRsForSeat.Errors != null)
                                            {
                                                var seatPaymentDetails = request.Request.PaymentFunctions.FirstOrDefault(x => x.OfferAssociation.Any(y => y.OfferRefID == (request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).OfferRefID))?.PaymentProcessingDetails;

                                                analyticsEventCommand.Track("ServiceName", "paymentMsCaptureSeat");
                                                analyticsEventCommand.Track("ErrorId", paymentMsCaptureRsForSeat?.Errors?.FirstOrDefault()?.Id);

                                                orderMSRS.OrderDocument.BookingReference.ExternalRecordLocator = orderTicketMSRS.OrderTicketDocument.BookingReference.ExternalRecordLocator;
                                                var seatDelReq = OrderMgmtSeatRequestMapper.MapOrderSeatDeletionRQ(item.Key, seatDetails, null, null, orderMSRS.OrderDocument);
                                                await _orderManagementMSCommandHandler.Handle(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = seatDelReq }, cancellationToken);

                                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPaymentMSRefundRequest(orderMSRS, request, currencies, item.Value.Item2.PaymentDocument.Amount.ToString(), item.Value.Item2, seatFulfilmentRS.OrderItems.Where(o => item.Key.Contains(o.FlightIdRef)).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.ElectronicMiscellaneousDocumentNumber }), seatPaymentDetails);
                                                await _paymentMSRefundCommandHandler.Handle(paymentMSRefundRQ, cancellationToken);
                                            }

                                            else if (paymentMsCaptureRsForSeat.PaymentDocument != null)
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

                    var totalAmount = orderMSRS.OrderDocument?.OrderItems?.Sum(orderItem =>
                    {
                        var basePrice = orderItem?.PriceDetails?.Where(x => x?.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Sum(x => Convert.ToDecimal(x?.Amount?.MonetaryAmount));

                        var paxCount = orderItem?.Services?.Where(s => s?.ServiceBeneficiaries != null)?.SelectMany(s => s.ServiceBeneficiaries)?.Distinct()?.Count();

                        return basePrice * paxCount;
                    });

                    analyticsEventCommand.Track("TotalAmount", totalAmount);

                    var segmentCount = orderMSRS.OrderDocument?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                    analyticsEventCommand.Track("SegmentCount", segmentCount);

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderMSRS.OrderDocument };
                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                }

                var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(Constants.RequestType.OrderCreate, orderMSRS.OrderDocument);
                _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                string approvalCode = paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode;

                var orderRS = OrderResponseMapper.MapOrderResponse(orderMSRS.OrderDocument, nDCWarnings, isHoldBooking, false, false, false, null, null, false, isEasyPay, approvalCode);

                LogOrderDetails(orderRS, orderMSRS.OrderDocument, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderRS };

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Order Create Easy Pay processing was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order Create Easy Pay processing failed with an unhandled exception");
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

        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                analyticsEventCommand.Track("PNR", orderDocument?.BookingReference?.RLOC);

                analyticsEventCommand.Track("OrderID", orderDocument?.OrderNumber?.OrderId);

                if (orderRS?.PaymentFunctions != null)
                {
                    foreach (var (payment, index) in orderRS?.PaymentFunctions?.Select((payment, index) => (payment, index)))
                    {
                        var paymentMethodDetail = payment?.PaymentProcessingSummary?.PaymentProcessingSummaryPaymentMethod.Item;

                        analyticsEventCommand.Track($"TransactionId{index + 1}", payment?.PaymentProcessingSummary?.PaymentID);
                        analyticsEventCommand.Track($"TransactionAmount{index + 1}", payment?.PaymentProcessingSummary?.Amount?.Value);
                        analyticsEventCommand.Track($"TransactionCurrencyCode{index + 1}", payment?.PaymentProcessingSummary?.Amount?.CurCode);
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

                var paidSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0);

                if (paidSeatCount > 0)
                {
                    var paidSeatAmount = orderDocument.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.Sum(x => Convert.ToDecimal(x.PriceDetails.First(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));
                }

                var freeSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) == 0);

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
                _logger.LogError(ex, "Failed to add Order Create Easy Pay success analytics details");
            }

        }

        private void LogErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                if (responseMessage?.Errors?.Any() != true)
                {
                    _logger.LogWarning("Order Create Easy Pay error analytics was requested without response errors");
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
                _logger.LogError(ex, "Failed to add Order Create Easy Pay error analytics details");
            }
        }

        private async Task TrackAnalyticsEventAsync(TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
        {
            try
            {
                var wasPublished = await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
                if (!wasPublished)
                {
                    _logger.LogError("Order Create Easy Pay analytics event publication was not completed");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Order Create Easy Pay analytics event");
            }
        }
    }
}
