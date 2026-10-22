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
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class PostSaleOrderChangeCommandHandler : IRequestHandler<PostSaleOrderChangeCommand, Response<IATA_OrderViewRS>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostSaleOrderChangeCommandHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGdsOrderResponseMapper _gdsOrderResponseMapper;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommandHandler;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ITransactionCommand _transactionCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;

        public PostSaleOrderChangeCommandHandler(IMediator mediator, ILogger<PostSaleOrderChangeCommandHandler> logger, IHostEnvironment hostEnvironment, IGdsOrderResponseMapper gdsOrderResponseMapper, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, PaymentIFGMSAuthorizeCommandHandler paymentIFGMSAuthorizeCommandHandler, PaymentIFGMSOrderCommandHandler paymentIFGMSOrderCommandHandler, PaymentIFGMSAuthorizeCancelCommandHandler paymentIFGMSAuthorizeCancelCommandHandler, ISendOrderEmailCommand sendOrderEmailCommand, ITransactionCommand transactionCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ICityCodeQuery cityCodeQuery)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _gdsOrderResponseMapper = gdsOrderResponseMapper ?? throw new ArgumentNullException(nameof(gdsOrderResponseMapper));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _paymentIFGMSAuthorizeCommandHandler = paymentIFGMSAuthorizeCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCommandHandler));
            _paymentIFGMSOrderCommandHandler = paymentIFGMSOrderCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSOrderCommandHandler));
            _paymentIFGMSAuthorizeCancelCommandHandler = paymentIFGMSAuthorizeCancelCommandHandler ?? throw new ArgumentNullException(nameof(paymentIFGMSAuthorizeCancelCommandHandler));
            _sendOrderEmailCommand = sendOrderEmailCommand ?? throw new ArgumentNullException(nameof(sendOrderEmailCommand));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _cityCodeQuery = cityCodeQuery ?? throw new ArgumentNullException(nameof(cityCodeQuery));

        }

        public async Task<Response<IATA_OrderViewRS>> Handle(PostSaleOrderChangeCommand request, CancellationToken cancellationToken)
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

                analyticsEventCommand.Track("RequestType", "OrderChange_PostSaleSeat");

                var distributionChain = request?.OrderChangeRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                IEnumerable<SeatTransactionId> transactionIds = [];
                var transactionStatus = new List<TransactionIdStatus>();
                var postSaleChangeOrderResult = OrderChangeValidations.ValidatePostOrderChangeRequest<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                if (postSaleChangeOrderResult?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "postSaleChangeOrderResult");
                    analyticsEventCommand.Track("ErrorId", postSaleChangeOrderResult.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(postSaleChangeOrderResult?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(postSaleChangeOrderResult?.ResponseMessages);
                    return errorDetails;
                }

                var freeSeatPaymentFunctionErrors = OrderChangeValidations.ValidateOTPaymentFunctions<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                if (freeSeatPaymentFunctionErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "freeSeatPaymentFunctionErrors");                    
                    analyticsEventCommand.Track("ErrorId", freeSeatPaymentFunctionErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(freeSeatPaymentFunctionErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(freeSeatPaymentFunctionErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderChangeRQ.Request?.Order?.OrderID ?? $"VS932{request.OrderChangeRQ.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);
                if(orderRS is null) { analyticsEventCommand.Track("ErrorId", orderRS?.Errors?.FirstOrDefault()?.Id); }

                var formOfPaymentValidationErrors = OrderChangeValidations.ValidateOrderChangeFormOfPayment<IATA_OrderViewRS>(request.OrderChangeRQ, orderRS, _hostEnvironment, isPostSaleSeat: true);

                if (formOfPaymentValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "formOfPaymentValidationErrors");
                    analyticsEventCommand.Track("ErrorId", formOfPaymentValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(formOfPaymentValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(formOfPaymentValidationErrors?.ResponseMessages);
                    return errorDetails;
                }
               
                string seatOfferId = (request.OrderChangeRQ.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.OfferRefID;
                var seatOfferDoc = await _mediator.Send(new SeatMSQuery() { OfferID = seatOfferId }, cancellationToken);

                analyticsEventCommand.Track("SeatOfferId", seatOfferId);

                var paymentRq = request.OrderChangeRQ.Request.PaymentFunctions?.First().PaymentProcessingDetails;
                bool isCashPayment = orderRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPay = orderRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;
                string approvalCode = orderRS?.OrderDocument?.OrderItems.FirstOrDefault()?.PaymentDetails?.FirstOrDefault(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay)?.ApprovalCode;

                var seatofferDocErrors = OrderChangeValidations.ValidatePostSaleOrderChangeSeatRQ<IATA_OrderViewRS>(request.OrderChangeRQ, seatOfferDoc?.SeatAvailabilityResponse, _hostEnvironment, isCashPayment);

                if (seatofferDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "SeatMS");
                    analyticsEventCommand.Track("ErrorId", seatofferDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(seatofferDocErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(seatofferDocErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request.OrderChangeRQ) }, null, true);
                var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, new CancellationToken());
                bool isNonUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode != "US";

                var airportDetails = await _mediator.Send(new AirportDetailsQuery(), cancellationToken);

                if (airportDetails?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "AirportDetailsMissing");
                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                if (validationMSResponse.Error != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                if (isCashPayment)
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

                analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestOrgName);
               
                if (validationMSResponse?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }
                analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode);

                if (orderRS?.OrderDocument == null)
                    return await PostSaleForGDS(request.OrderChangeRQ, request.SubscriptionKey, validationMSResponse, seatOfferDoc, airportDetails, transactionStatus, transactionIds, cancellationToken);
                else
                {
                    var paymentStatusValidationErrors = OrderChangeValidations.ValidatePaymentStatus(orderRS, _hostEnvironment);

                    if (paymentStatusValidationErrors?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "PaymentStatusValidation");
                        analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                        return errorDetails;
                    }

                    var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, orderRS?.OrderDocument, null);

                    var gstValidation = OrderChangeValidations.ValidateGSTDetailsAgainstOrderDoc(orderRS.OrderDocument, request.OrderChangeRQ, _hostEnvironment);
                    if (gstValidation.Any())
                    {
                        isGSTValidationFailed = true;
                        warnings.AddRange(gstValidation);
                    }

                    var gstWarnings = OrderChangeValidations.ValidateGSTDetails<NDCWarning>(request.OrderChangeRQ, _hostEnvironment);
                    if (gstWarnings.Any())
                    {
                        isGSTValidationFailed = true;
                        warnings.AddRange(gstValidation);
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

                        foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
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

                        getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);

                        getReservationOrderDoc.Version = orderRS.OrderDocument.Version;
                        getReservationOrderDoc.ChangeType = orderRS.OrderDocument.ChangeType;
                        getReservationOrderDoc.ChangeDateTime = orderRS.OrderDocument.ChangeDateTime;
                        getReservationOrderDoc.BookingReference = orderRS.OrderDocument.BookingReference;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                        await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }
                    else
                    {
                        throw new Exception($"SabreOrderDoc not found for orderId: {orderRS?.OrderDocument?.OrderNumber?.OrderId}");
                    }

                    warnings.AddRange(OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment, getReservationOrderDoc));

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

                    UpdateGenderAndDOB(request.OrderChangeRQ, getReservationOrderDoc);

                    var serviceListOfferId = ((AcceptSelectedQuotedOfferListType)request.OrderChangeRQ?.Request?.ChangeOrderChoice?.Item)?.SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem != null && !((Domain.OrderChangeRQ.PaxSegmentRefListType)x.SelectedALaCarteOfferItem.OfferFlightAssociations.Item).PaxSegmentRefID.IsNullOrEmpty()));
                    Infrastructure.Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse = new();

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

                    var seatDetails = PopulateSeatDetails(request.OrderChangeRQ, seatOfferDoc?.SeatAvailabilityResponse, getReservationOrderDoc, null);
                    var seatOfferErros = OrderChangeValidations.ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc<IATA_OrderViewRS>(request.OrderChangeRQ, seatOfferDoc?.SeatAvailabilityResponse, seatDetails, _hostEnvironment);

                    if (seatOfferErros?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatOfferValidation");
                        analyticsEventCommand.Track("ErrorId", seatOfferErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(seatOfferErros?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(seatOfferErros?.ResponseMessages);
                        return errorDetails;
                    }
                    var seatMapErros = OrderChangeValidations.ValidatePostSaleOrderChangeRequestAgainstSeatMap<IATA_OrderViewRS>(request.OrderChangeRQ, seatDetails, seatOfferDoc?.SeatAvailabilityResponse, _hostEnvironment);

                    if (seatMapErros?.ResponseMessages != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatMapValidation");
                        analyticsEventCommand.Track("ErrorId", seatMapErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(seatMapErros?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(seatMapErros?.ResponseMessages);
                        return errorDetails;
                    }

                    if (!isCashPayment && request.OrderChangeRQ.Request.PaymentFunctions != null && request.OrderChangeRQ.Request.PaymentFunctions.Any() && request.OrderChangeRQ.Request.PaymentFunctions.FirstOrDefault().PaymentProcessingDetails.Amount.Value != 0)
                    {
                        postSaleChangeOrderResult = OrderChangeValidations.ValidatePaymentInfoOrderChange<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                        if (postSaleChangeOrderResult?.ResponseMessages != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "PaymentInfoValidation");
                            analyticsEventCommand.Track("ErrorId", postSaleChangeOrderResult.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                            
                            LogErrorDetails(postSaleChangeOrderResult?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(postSaleChangeOrderResult?.ResponseMessages);
                            return errorDetails;
                        }

                        if (((PaymentProcessingPaymentCardType)request.OrderChangeRQ.Request.PaymentFunctions.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item).SecurePaymentVersion2.PaymentTrxChannelCode.ToString() != Constants.Application.TO && ((PaymentProcessingPaymentCardType)request.OrderChangeRQ.Request.PaymentFunctions.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item).SecurePaymentVersion2.PaymentTrxChannelCode.ToString() != Constants.Application.MO)
                        {
                            postSaleChangeOrderResult = OrderChangeValidations.Validate3DS2<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                            if (postSaleChangeOrderResult?.ResponseMessages != null)
                            {
                                analyticsEventCommand.Track("ErrorId", postSaleChangeOrderResult.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(postSaleChangeOrderResult?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(postSaleChangeOrderResult?.ResponseMessages);
                                return errorDetails;
                            }

                            if (((PaymentProcessingPaymentCardType)request.OrderChangeRQ?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item)?.SecurePaymentVersion2 != null)
                            {
                                postSaleChangeOrderResult = OrderChangeValidations.Validate3DSecureAuthenticationData<IATA_OrderViewRS>(request.OrderChangeRQ, _hostEnvironment);

                                if (postSaleChangeOrderResult?.ResponseMessages != null)
                                {
                                    analyticsEventCommand.Track("ErrorId", postSaleChangeOrderResult.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                    LogErrorDetails(postSaleChangeOrderResult?.ResponseMessages, analyticsEventCommand);
                                    var errorDetails = ErrorMapper.GetErrorDetails(postSaleChangeOrderResult?.ResponseMessages);
                                    return errorDetails;
                                }
                            }
                        }
                    }

                    var successfullyAuthSegs = new Dictionary<IEnumerable<string>, (Infrastructure.Services.PaymentMS.PaymentMSAuthorizeCommand, Infrastructure.Model.PaymentMS.PaymentRS)>();

                    foreach (var journey in getReservationOrderDoc.DataList.Journey)
                    {
                        var segsInJour = journey.FlightSegmentRefIds.Select(x => x.Replace("SEG", "").PadLeft(2, '0'));
                        var seatAmountPerJour = seatDetails.Where(s => segsInJour.Contains(s.AssociatedSegment.SegmentId)).Sum(x => Convert.ToDecimal(x.SeatPrice)).ToString();

                        if (Convert.ToDecimal(seatAmountPerJour) != 0)
                        {
                            var currencyRS = await _mediator.Send(new ReferenceMSCurrencyQuery { ReferenceDataType = Constants.Application.Air4CurrencyNumber }, cancellationToken);
                            var currencies = Json.Deserialize<IEnumerable<Currency>>(currencyRS);
                            if (currencyRS is null) { analyticsEventCommand.Track("ServiceName", "ReferenceMSCurrencyFailed"); }
                            
                            var paymentMethodDetail = ((VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)paymentRq?.PaymentMethod?.Item);
                            var transactionType = paymentMethodDetail?.SecurePaymentVersion2?.PaymentTrxChannelCode.ToString();

                            analyticsEventCommand.Track("PaymentTypeCode", request?.OrderChangeRQ?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria?.PaymentTypeCode);

                            analyticsEventCommand.Track("PaymentTxnChannelCode_TO", transactionType == "TO" ? 'Y' : 'N');
                            analyticsEventCommand.Track("PaymentTxnChannelCode_MO", transactionType == "MO" ? 'Y' : 'N');

                            var paymentMSAuthorizeRequestSeat = PaymentMSRequestMapper.MapHoldBookingAuthorizeRequest(getReservationOrderDoc, request.OrderChangeRQ, airportDetails, seatAmountPerJour, currencies, Constants.ProductType.Seat);
                            var paymentMSAuthorizeRSSeat = await _mediator.Send(paymentMSAuthorizeRequestSeat, cancellationToken);

                            if (paymentMSAuthorizeRSSeat.Errors is not null)
                            {
                                var paymentMSAuthorizeErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSAuthorizeRSSeat, _hostEnvironment);

                                analyticsEventCommand.Track("ServiceName", "paymentMSAuthorizeSeat");
                                analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                LogErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages, analyticsEventCommand);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);
                                return errorDetails;
                            }
                            else
                            {
                                successfullyAuthSegs.Add(segsInJour, (paymentMSAuthorizeRequestSeat, paymentMSAuthorizeRSSeat));
                                transactionIds = transactionIds.Concat(segsInJour.Select(x =>
                                {
                                    return new SeatTransactionId()
                                    {
                                        SegmentId = x,
                                        TransactionId = paymentMSAuthorizeRSSeat.PaymentDocument.TransactionId
                                    };
                                }));
                                transactionStatus.Add(new TransactionIdStatus() { TransactionId = paymentMSAuthorizeRSSeat.PaymentDocument.TransactionId, PaymentStatus = true, Amount = paymentMSAuthorizeRSSeat.PaymentDocument.Amount });
                            }
                        }
                        else
                        {
                            string transactionId = GenerateTransactionId();
                            transactionIds = transactionIds.Concat(segsInJour.Select(x =>
                            {
                                return new SeatTransactionId()
                                {
                                    SegmentId = x,
                                    TransactionId = transactionId.ToString(),
                                };
                            }));
                            transactionStatus.Add(new TransactionIdStatus() { TransactionId = transactionId, PaymentStatus = true, Amount = Convert.ToDecimal("0.00") });
                        }
                    }

                    ChangeOrderRS seatFulfilmentResponse = null;

                    if ((successfullyAuthSegs.Count != 0) || Convert.ToDecimal(seatDetails.Sum(x => Convert.ToDecimal(x.SeatPrice))) == 0)
                    {
                        if (successfullyAuthSegs.Count != 0)
                        {
                            var authorizedSegmentIds = successfullyAuthSegs.SelectMany(x => x.Key).ToHashSet();
                            seatDetails = seatDetails.Where(s => Convert.ToDecimal(s.SeatPrice) == decimal.Zero || authorizedSegmentIds.Contains(s.AssociatedSegment.SegmentId));
                        }

                        string countryCode = getReservationOrderDoc.PointOfSale.CountryCode;
                        var cityCodeData = await _cityCodeQuery.Handle(countryCode, cancellationToken);

                        var cityCodeValidationErrors = OrderChangeValidations.ValidateCityCode<IATA_OrderViewRS>(cityCodeData, _hostEnvironment);

                        if (cityCodeValidationErrors != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "cityCodeValidation");
                            analyticsEventCommand.Track($"ErrorId", cityCodeValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                            foreach (var item in successfullyAuthSegs)
                            {
                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderRS, request.OrderChangeRQ, null, item.Value.Item2, item.Value.Item1);
                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                            }

                            LogErrorDetails(cityCodeValidationErrors?.ResponseMessages, analyticsEventCommand);
                            var errorDetails = ErrorMapper.GetErrorDetails(cityCodeValidationErrors?.ResponseMessages);
                            return errorDetails;
                        }

                        getReservationOrderDoc.PointOfSale.CityCode = cityCodeData.CityCode;
                        getReservationOrderDoc.PointOfSale.NDCCityCodeLocation = cityCodeData.NDCCityCodeLocation;

                        var seatFulfilmentReq = PostSaleForOrderChange(request.OrderChangeRQ, seatOfferDoc.SeatAvailabilityResponse, seatDetails, isGSTValidationFailed, null, validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode, getReservationOrderDoc, transactionIds);

                        seatFulfilmentResponse = await _mediator.Send(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = seatFulfilmentReq }, cancellationToken);

                        if (seatFulfilmentResponse?.orderChangeResponse?.OrderDetails?.OrderItems != null)
                        {
                            OrderDocumentMapper.UpdateOrderDocument(getReservationOrderDoc, seatFulfilmentResponse.orderChangeResponse, seatDetails, transactionIds, transactionStatus);
                            
                            foreach (var item in successfullyAuthSegs)
                            {
                                var accountableDocumentRef = getReservationOrderDoc.OrderItems.Where(oi => oi.PaymentDetails != null && oi.PaymentDetails.First().PaymentReferenceId == item.Value.Item2.PaymentDocument.TransactionId).SelectMany(oi => oi.AccountableDocumentRefs);
                                var paymentMSCaptureRequestSeat = PaymentMSRequestMapper.MapHoldBookingCaptureRequest(item.Value.Item1, item.Value.Item2, getReservationOrderDoc, accountableDocumentRef.Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.DocumentNumber }), Constants.ProductType.Seat);
                                var paymentMSCaptureRSSeat = await _mediator.Send(paymentMSCaptureRequestSeat, cancellationToken);

                                if (paymentMSCaptureRSSeat.Errors is not null)
                                {
                                    analyticsEventCommand.Track("ServiceName", "paymentMSCaptureSeat");
                                    var seatDelReq = OrderMgmtSeatRequestMapper.MapOrderSeatDeletionRQ(item.Key, seatDetails, null, null, getReservationOrderDoc);
                                    await _mediator.Send(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = seatDelReq }, cancellationToken);

                                    var paymentMSRefundRQ = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderRS, request.OrderChangeRQ, seatFulfilmentResponse.orderChangeResponse.OrderDetails.OrderItems.SelectMany(x => x.AccountableDocuments).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.DocumentNumber }), item.Value.Item2, item.Value.Item1);
                                    await _mediator.Send(paymentMSRefundRQ, cancellationToken);

                                    var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSCaptureRSSeat, _hostEnvironment);

                                    analyticsEventCommand.Track("ErrorId", paymentMSCaptureErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                                    LogErrorDetails(paymentMSCaptureErrors?.ResponseMessages, analyticsEventCommand);
                                    var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors?.ResponseMessages);
                                    return errorDetails;
                                }

                                OrderDocumentMapper.UpdateSeatPaymentDetails(getReservationOrderDoc, paymentMSCaptureRSSeat);

                            }
                        }
                        else
                        {
                            foreach (var item in successfullyAuthSegs)
                            {
                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapOrderChangeRefundRequest(orderRS, request.OrderChangeRQ, null, item.Value.Item2, item.Value.Item1);
                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                            }

                            LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                            return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                        }
                    }

                    try
                    {
                        //Update APIS , SSR and GST
                        if (orderRS?.OrderDocument.OrderStatus != nameof(OrderStatusEnum.Unticketed) && (request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.IdentityDoc != null) || (!isGSTValidationFailed && request.OrderChangeRQ.Request.DataLists.PaxList.Any(x => x.Employer != null)) || flightServiceListResponse?.ServiceListResponse != null))
                        {
                            var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequest(request.OrderChangeRQ, getReservationOrderDoc, flightServiceListResponse, isGSTValidationFailed);
                            var orderChangeAPISResponse = await _mediator.Send(orderManagementAPISRequest, cancellationToken);

                            if (orderChangeAPISResponse != null)
                            {
                                if (orderChangeAPISResponse.orderDocument.Warnings is not null)
                                {
                                    warnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderChangeAPISResponse.orderDocument, _hostEnvironment, warnings));
                                }

                                OrderDocumentMapper.UpdateSSRAPISGSTToOrderDoc(getReservationOrderDoc, orderChangeAPISResponse.orderDocument);

                                await _mediator.Send(new OrderDataMSCommand() { OrderDocument = getReservationOrderDoc }, cancellationToken);
                            }
                            else
                            {
                                analyticsEventCommand.Track("ServiceName", "orderChangeAPISResponseNull");

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while updating APIS/SSR/GST in OrderChangeManagement");

                    }

                    getReservationOrderDoc.Version = orderRS.OrderDocument.Version + 1;

                    var publishOrderDoc = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                    await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDoc), cancellationToken);

                    if (seatFulfilmentResponse?.orderChangeResponse?.OrderDetails?.OrderItems != null)
                        OrderDocumentMapper.UpdateSeatProductDetails(getReservationOrderDoc, seatOfferDoc);

                    var emailRequest = SendOrderEmailRequestMapper.MapEmailRequest(RequestType.PostSale, getReservationOrderDoc);
                    _ = _sendOrderEmailCommand.SendOrderEmail(emailRequest);

                    var orderResponse = OrderResponseMapper.MapOrderResponse(getReservationOrderDoc, warnings, false, false, false, false, null, null, isCashPayment, isEasyPay, approvalCode);

                    LogOrderDetails(orderResponse, getReservationOrderDoc, analyticsEventCommand);

                    return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderResponse };
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(PostSaleOrderChangeCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in {HandlerName}.", nameof(PostSaleOrderChangeCommandHandler));
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
                    int i = index;
                    i++;
                    analyticsEventCommand.Track($"OriginDestination{i}", $"{ond.OriginDestination.OriginAirport}{ond.OriginDestination.DestinationAirport}");
                    analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);
                }

                foreach (var (ptc, index) in orderResponse.DataLists.PaxList.GroupBy(x => x.PTC).Select((ptc, index) => (ptc, index)))
                {
                    int i = index;
                    i++;
                    analyticsEventCommand.Track($"PaxType{i}", ptc.Key);
                    analyticsEventCommand.Track($"PaxCount{i}", ptc.Count());
                }

                foreach (var (ond, index) in orderDocument.DataList.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
                {
                    int i = index;
                    i++;
                    var segRefIds = orderDocument.DataList.Journey.FirstOrDefault(x => x.JourneyId == ond.JourneyRefIds.FirstOrDefault()).FlightSegmentRefIds;
                    var priceClassRefId = orderDocument.OrderItems.First().Services.FirstOrDefault(x => segRefIds.Contains(x.SegmentRefIds.FirstOrDefault())).PriceClassRefIds.First();

                    analyticsEventCommand.Track($"O&D{i}Cabin", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).CabinType.CabinName);
                    analyticsEventCommand.Track($"O&D{i}FareBrand", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).ProductName);
                }

                var paidSeatCount = orderDocument.OrderItems.Count(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0);
                
                if (paidSeatCount > 0)
                {
                    var paidSeatAmount = orderDocument.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.Sum(x => Convert.ToDecimal(x.PriceDetails.First(p => p.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));
                    analyticsEventCommand.Track("PaidSeatAmount", paidSeatAmount);
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


                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsEventCommand.Track("DateOfResponse", dateOfResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }

        private async Task<Response<IATA_OrderViewRS>> PostSaleForGDS(IATA_OrderChangeRQ request, string subscriptionKey, ValidationRS validationMSResponse, SeatRS seatOfferDoc, IEnumerable<AirportDetail> airportDetails, List<TransactionIdStatus> transactionStatus, IEnumerable<SeatTransactionId> transactionIds, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            var orderSearchRequest = Air4OrderSearchRequestMapper.MapOrderSearchRequest(Constants.Application.OwnerAirline, request.Request.BookingRef?.FirstOrDefault()?.BookingID);
            var orderSearchResponse = await _mediator.Send(new Air4OrderSearchMSQuery() { OrderSearchRequest = orderSearchRequest }, cancellationToken);

            var validationErrors = OrderChangeValidations.ValidateOrderSearchResponse<IATA_OrderViewRS>(request.DistributionChain.OrderBy(x => x.Ordinal).LastOrDefault()?.ParticipatingOrg?.OrgID, orderSearchResponse, ConfigReader.Get<bool>(Constants.Environment.SkipGDSAgentValidation), _hostEnvironment);

            if (validationErrors?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "ValidateOrderSearchResponse");
                analyticsEventCommand.Track("ErrorId", validationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
               
                LogErrorDetails(validationErrors?.ResponseMessages, analyticsEventCommand);
                var errorDetails = ErrorMapper.GetErrorDetails(validationErrors?.ResponseMessages);
                return errorDetails;
            }

            var postSaleChangeOrderForGDSResult = OrderChangeValidations.ValidateVaPostSaleOrderChangeRequestForGds<IATA_OrderViewRS>(request, _hostEnvironment);

            if (postSaleChangeOrderForGDSResult?.ResponseMessages != null)
                return ErrorMapper.GetErrorDetails(postSaleChangeOrderForGDSResult.ResponseMessages);

            var seatDetails = PopulateSeatDetails(request, seatOfferDoc?.SeatAvailabilityResponse, null, orderSearchResponse?.OrderSearchResponse);
            var seatOfferErros = OrderChangeValidations.ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc<IATA_OrderViewRS>(request, seatOfferDoc?.SeatAvailabilityResponse, seatDetails, _hostEnvironment);

            if (seatOfferErros?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "seatOfferErros");
                analyticsEventCommand.Track("ErrorId", seatOfferErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id); 
                
                LogErrorDetails(seatOfferErros?.ResponseMessages, analyticsEventCommand);
                var errorDetails = ErrorMapper.GetErrorDetails(seatOfferErros?.ResponseMessages);
                return errorDetails;
            }

            var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, null, orderSearchResponse?.OrderSearchResponse);

            var seatMapErros = OrderChangeValidations.ValidatePostSaleOrderChangeRequestAgainstSeatMap<IATA_OrderViewRS>(request, seatDetails, seatOfferDoc?.SeatAvailabilityResponse, _hostEnvironment);

            if (seatMapErros?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "seatMapErros");
                analyticsEventCommand.Track("ErrorId", seatMapErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                LogErrorDetails(seatMapErros?.ResponseMessages, analyticsEventCommand);
                var errorDetails = ErrorMapper.GetErrorDetails(seatMapErros?.ResponseMessages);
                return errorDetails;
            }

            var gdsWarnings = OrderChangeValidations.ValidatePostSaleOrderChangeRequestAgainstSeatMapForWarning<IATA_OrderViewRS>(request, seatDetails, seatOfferDoc?.SeatAvailabilityResponse, _hostEnvironment);

            var successfullyAuthSegs = new Dictionary<IEnumerable<string>, (PaymentMSAuthorizeCommand, Infrastructure.Model.PaymentMS.PaymentRS)>();

            foreach (var journey in orderSearchResponse.OrderSearchResponse.OriginDestinationAssociations)
            {
                var segmentsInJour = orderSearchResponse.OrderSearchResponse.SegmentDetails.Where(a => a.OriginDestinationRefId == journey.ODLabel).Select(x => x.SegmentId);
                var seatAmountPerJour = seatDetails.Where(s => segmentsInJour.Contains(s.AssociatedSegment.SegmentId)).Sum(x => Convert.ToDecimal(x.SeatPrice)).ToString();

                if (Convert.ToDecimal(seatAmountPerJour) != 0)
                {
                    var paymentMSAuthorizeRequestSeat = PaymentMSRequestMapper.MapPostSaleGDSAuthorizeRequest(orderSearchResponse.OrderSearchResponse, request, airportDetails, seatAmountPerJour);
                    var paymentMSAuthorizeRSSeat = await _mediator.Send(paymentMSAuthorizeRequestSeat, cancellationToken);

                    if (paymentMSAuthorizeRSSeat.Errors is not null)
                    {
                        var paymentMSAuthorizeErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSAuthorizeRSSeat, _hostEnvironment);
                        
                        analyticsEventCommand.Track("ServiceName", "paymentMSAuthorizeRSSeat");
                        analyticsEventCommand.Track("ErrorId", paymentMSAuthorizeErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        LogErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages, analyticsEventCommand);
                        var errorDetails = ErrorMapper.GetErrorDetails(paymentMSAuthorizeErrors?.ResponseMessages);
                        return errorDetails;
                    }
                    else
                    {
                        successfullyAuthSegs.Add(segmentsInJour, (paymentMSAuthorizeRequestSeat, paymentMSAuthorizeRSSeat));
                        transactionIds = transactionIds.Concat(segmentsInJour.Select(x =>
                        {
                            return new SeatTransactionId()
                            {
                                SegmentId = x,
                                TransactionId = paymentMSAuthorizeRSSeat.PaymentDocument.TransactionId
                            };
                        }));
                        transactionStatus.Add(new TransactionIdStatus() { TransactionId = paymentMSAuthorizeRSSeat.PaymentDocument.TransactionId, PaymentStatus = true, Amount = paymentMSAuthorizeRSSeat.PaymentDocument.Amount });
                    }

                }
            }

            if (successfullyAuthSegs.Count != 0 || Convert.ToDecimal(seatDetails.Sum(x => Convert.ToDecimal(x.SeatPrice))) == 0)
            {
                if (successfullyAuthSegs.Count != 0)
                {
                    var authorizedSegmentIds = successfullyAuthSegs.SelectMany(x => x.Key).ToHashSet();
                    seatDetails = seatDetails.Where(s => Convert.ToDecimal(s.SeatPrice) == decimal.Zero || authorizedSegmentIds.Contains(s.AssociatedSegment.SegmentId));
                }

                var orderSeatFulfilmentrequest = PostSaleForOrderChange(request, seatOfferDoc?.SeatAvailabilityResponse, seatDetails, isGSTValidationFailed, orderSearchResponse?.OrderSearchResponse, validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode, null);

                var changeOrderRS = await _mediator.Send(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = orderSeatFulfilmentrequest }, cancellationToken);

                if (changeOrderRS != null)
                {
                    var updatedWarnings = OrderChangeValidations.ValidateSeatFulfilmentResponse<IATA_OrderViewRS>(changeOrderRS.orderChangeResponse, seatDetails, gdsWarnings, _hostEnvironment);

                    PaymentRS paymentMSCaptureRSSeat = null;

                    if (changeOrderRS?.orderChangeResponse?.OrderDetails?.OrderItems != null)
                    {
                        foreach (var item in successfullyAuthSegs)
                        {
                            var paymentMSCaptureRequestSeat = PaymentMSRequestMapper.MapPostSaleGDSCaptureRequest(item.Value.Item1, item.Value.Item2, orderSearchResponse.OrderSearchResponse, changeOrderRS.orderChangeResponse.OrderDetails.OrderItems.SelectMany(x => x.AccountableDocuments).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.DocumentNumber }));
                            paymentMSCaptureRSSeat = await _mediator.Send(paymentMSCaptureRequestSeat, cancellationToken);

                            if (paymentMSCaptureRSSeat.Errors is not null)
                            {
                                var seatDelReq = OrderMgmtSeatRequestMapper.MapOrderSeatDeletionRQ(item.Key, seatDetails, orderSearchResponse.OrderSearchResponse);
                                await _mediator.Send(new Infrastructure.Services.Air4OrderManagementMS.OrderManagementMSCommand() { OrderChangeRequest = seatDelReq }, cancellationToken);

                                var paymentMSRefundRQ = PaymentMSRequestMapper.MapPostSaleGDSRefundRequest(orderSearchResponse.OrderSearchResponse, request, changeOrderRS.orderChangeResponse.OrderDetails.OrderItems.SelectMany(x => x.AccountableDocuments).Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef() { DocumentNumber = x.DocumentNumber }), item.Value.Item2, item.Value.Item1);
                                await _mediator.Send(paymentMSRefundRQ, cancellationToken);

                                var paymentMSCaptureErrors = PaymentResponseValidation.ValidatePaymentResponse<IATA_OrderViewRS>(paymentMSCaptureRSSeat, _hostEnvironment);
                                var errorDetails = ErrorMapper.GetErrorDetails(paymentMSCaptureErrors.ResponseMessages);

                                return errorDetails;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in successfullyAuthSegs)
                        {
                            var paymentMSRefundRQ = PaymentMSRequestMapper.MapPostSaleGDSRefundRequest(orderSearchResponse.OrderSearchResponse, request, null, item.Value.Item2, item.Value.Item1);
                            await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                        }

                        LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                        return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                    }

                    var orderSearchResponseWithSeatOrderItems = _gdsOrderResponseMapper.AddSeatOrderItems(changeOrderRS.orderChangeResponse, orderSearchResponse.OrderSearchResponse, seatDetails, transactionIds, transactionStatus, paymentMSCaptureRSSeat);
                    var response = _gdsOrderResponseMapper.MapOrderResponseForGDS(orderSearchResponseWithSeatOrderItems, updatedWarnings);

                    return new() { Status = Status.OK, Value = response };
                }
                else
                {
                    foreach (var item in successfullyAuthSegs)
                    {
                        var paymentMSRefundRQ = PaymentMSRequestMapper.MapPostSaleGDSRefundRequest(orderSearchResponse.OrderSearchResponse, request, null, item.Value.Item2, item.Value.Item1);
                        await _mediator.Send(paymentMSRefundRQ, cancellationToken);
                    }

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }
            }
            else
            {
                LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
        }

        private IEnumerable<SeatDetails> PopulateSeatDetails(IATA_OrderChangeRQ request, SeatAvailabilityResponse seatOfferDoc, Infrastructure.Model.OrderMS.OrderDocument orderDoc = null, OrderSearchResponse orderSearchResponse = null)
        {
            var offerItems = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault().SelectedOfferItem;

            return offerItems.Where(item => seatOfferDoc.FlightSegmentSeatMap.Any(x => x.Catalogue.Any(x => x.CatalogueId == item.OfferItemRefID))).Select(item =>
            {
                var seatMap = seatOfferDoc.FlightSegmentSeatMap?.FirstOrDefault(x => x.Catalogue.Any(x => x.CatalogueId == item.OfferItemRefID));
                var pax = request.Request.DataLists?.PaxList?.FirstOrDefault(x => x.PaxID == item.PaxRefID?.FirstOrDefault());
                var orderBeneficiary = orderDoc != null ? orderDoc.DataList?.CustomerDetails?.FirstOrDefault(y => y.CustId == item.PaxRefID?.FirstOrDefault()) : orderSearchResponse?.CustomerDetails?.FirstOrDefault(x => x.FirstName == pax?.Individual?.GivenName?.FirstOrDefault() && x.LastName == pax?.Individual?.Surname);

                var seatColumnsList = seatMap.Seatmap.SelectMany(y => y.SeatRows.SelectMany(z => z.SeatColumns));
                var seatColumn = seatColumnsList?.FirstOrDefault(sc => sc.SeatIdentifer == item.SelectedSeat?.SeatRowNumber + item.SelectedSeat?.ColumnID);
                var eligibilityCatalogueID = seatColumn?.Eligibility?.FirstOrDefault(e => e.CustomerId.First() == orderBeneficiary?.LastNameNumber + "-" + orderBeneficiary?.FirstNameNumber)?.CatalogueRefId;
                var catalogue = seatMap.Catalogue?.FirstOrDefault(c => c.CatalogueId == eligibilityCatalogueID);

                return new SeatDetails()
                {
                    SeatNumber = item.SelectedSeat?.SeatRowNumber + item.SelectedSeat?.ColumnID,
                    AssociatedSegment = seatMap?.FlightSegment,
                    Beneficiary = new Customer()
                    {
                        CustId = orderBeneficiary?.CustId,
                        FirstName = orderBeneficiary?.FirstName,
                        LastName = orderBeneficiary?.LastName,
                        FirstNameNumber = orderBeneficiary?.FirstNameNumber,
                        LastNameNumber = orderBeneficiary?.LastNameNumber
                    },
                    SeatPrice = seatColumn?.Available == true ? catalogue?.SeatPrice?.MonetaryAmount : null,
                    DecimalPrecisionCount = seatColumn?.Available == true ? catalogue?.SeatPrice?.DecimalPrecisionCount.ToString() : null,
                    Currency = seatColumn?.Available == true ? catalogue?.SeatPrice?.BaseCurrency : null,
                    SeatOfferID = seatColumn?.Available == true ? catalogue?.CatalogueId : null,
                };
            });
        }

        public OrderChangeRequest PostSaleForOrderChange(IATA_OrderChangeRQ request, SeatAvailabilityResponse seatMap, IEnumerable<SeatDetails> seatDetailsForGds, bool isGSTValidationFailed, OrderSearchResponse orderSearchResponse, string countryCode, OrderDocument orderDoc = null, IEnumerable<SeatTransactionId> transactionIds = null)
        {
            var orderSeatFulfilmentrequest = orderDoc != null ? OrderMgmtSeatRequestMapper.MapOrderSeatFulfilmentRQ(request, seatMap, seatDetailsForGds, null, null, orderDoc, transactionIds)
                                             : OrderMgmtSeatRequestMapper.MapOrderSeatFulfilmentRQ(request, seatMap, seatDetailsForGds, orderSearchResponse, countryCode);

            return orderSeatFulfilmentrequest;
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

        public static string GenerateTransactionId()
        {
            var random = new Random();
            int id = random.Next(10000000, 99999999);
            return id.ToString();
        }
    }
}

