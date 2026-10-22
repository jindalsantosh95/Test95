using CSharpFunctionalExtensions;
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
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderRetrieve;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderRetrieve
{
    public class OrderRetrieveQueryHandler : IRequestHandler<OrderRetrieveQuery, Result<Response<IATA_OrderViewRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderRetrieveQueryHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGdsOrderResponseMapper _gdsOrderResponseMapper;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderRetrieveQueryHandler(IMediator mediator, ILogger<OrderRetrieveQueryHandler> logger, IHostEnvironment hostEnvironment, IGdsOrderResponseMapper gdsOrderResponseMapper, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _gdsOrderResponseMapper = gdsOrderResponseMapper;
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(OrderRetrieveQuery request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderRetrieveEventSubject
            };
            var warnings = new List<NDCWarning>();

            try
            {
                var response = new Response<IATA_OrderViewRS>();
                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);

                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("PNR", request.OrderRetrieveRQ.Request.OrderValidationFilterCriteria?.BookingRefFilterCriteria?.BookingID);

                analyticsEventCommand.Track("OrderID", request.OrderRetrieveRQ.Request.OrderValidationFilterCriteria?.OrderFilterCriteria?.OrderID);

                var distributionChain = request?.OrderRetrieveRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
               
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                response = OrderRetrieveValidation.OrderRetrieveRQValidation<IATA_OrderViewRS>(request.OrderRetrieveRQ, _hostEnvironment);

                if (response?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderRetrieveRQValidation");
                    analyticsEventCommand.Track("ErrorId", response.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(response?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(response?.ResponseMessages);
                    return errorDetails;
                }

                var orderRS = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderRetrieveRQ.Request.OrderValidationFilterCriteria?.OrderFilterCriteria?.OrderID ?? $"VS{ConfigReader.Get(Constants.Environment.TicketStockNumber)}{request.OrderRetrieveRQ.Request.OrderValidationFilterCriteria?.BookingRefFilterCriteria?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                warnings.AddRange(OrderRetrieveValidation.ValidateDisruptedServices(orderRS.OrderDocument, _hostEnvironment));

                var isGDSCheckRequired = (orderRS.Errors is not null && orderRS.Errors.Any(x => x.Code == Constants.Application.OrderDocNotFoundErrorCode));
                analyticsEventCommand.Track("GDS", isGDSCheckRequired ? 'Y' : 'N');

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderRetrieve(request.OrderRetrieveRQ) }, null, true);
                var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSResponse.Error is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationMSResponse");
                    analyticsEventCommand.Track("ErrorId", validationMSResponse?.Error?.FirstOrDefault()?.Id);

                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);
                    
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

                response = OrderRetrieveValidation.OrderRetrieveRQAgentValidation<IATA_OrderViewRS>(request.OrderRetrieveRQ, validationMSResponse, _hostEnvironment);

                if (response?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderRetrieveRQAgentValidation");
                    analyticsEventCommand.Track("ErrorId", response.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(response?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(response?.ResponseMessages);
                    return errorDetails;
                }

                if (isGDSCheckRequired)
                {
                    return await GDSBookingOrderRetrieve(request.OrderRetrieveRQ, analyticsEventCommand, cancellationToken);
                }

                bool isCashPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                bool isUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode == "US";

                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument);
                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                if (getReservationOrderDoc.Errors is null && !orderRS.OrderDocument.OrderStatus.Is("Cancelled"))
                {
                    if (orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts?.CountryDailingCode != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }

                    if (orderRS.OrderDocument.OrderHistory != null) getReservationOrderDoc.OrderHistory = orderRS.OrderDocument.OrderHistory;
                    if (orderRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderRS.OrderDocument.FareTypeRequest;

                    var segmentIds = getReservationOrderDoc.OrderItems.Any(x => x.AccountableDocumentRefs?.Any(ac => ac.Coupons?.Any(c => c.CouponStatus == "USED") == true) == true) ? getReservationOrderDoc.OrderItems.First().AccountableDocumentRefs.SelectMany(x => x.Coupons).Where(x => x.CouponStatus == "USED").Select(x => x.SegmentRefId) : orderRS.OrderDocument.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);
                    foreach (var segment in getReservationOrderDoc.DataList.SegmentDetails.Where(s => segmentIds.Contains(s.SegmentId)))
                    {
                        segment.Status.DeliveryStatusCode = "Delivered";
                    }

                    foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                    }

                    foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                    }

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

                    var checkProduct = brandResponse?.Any(brand =>
                        brand?.Itineraries?.Any(itinerary =>
                            itinerary?.Legs?.Any(leg =>
                                leg?.Fares?.Any(fare => fare?.Product is null) == true
                            ) == true
                        ) == true
                    ) == true;

                        
                    if (checkProduct)
                    {
                        _logger.LogWarning("BrandMS response contains at least one fare with a null product.");
                        analyticsEventCommand.Track("BrandMSHasNullProduct", true);
                    }

                    getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);

                    if (orderRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
                    {
                        OrderDocumentMapper.MapOrderItems(orderRS.OrderDocument, getReservationOrderDoc);
                    }

                    if (getReservationOrderDoc.DataList.SegmentDetails.Any(seg => seg.Status.DeliveryStatusCode == "Delivered"))
                    {
                        OrderDocumentMapper.MapFlownSegmentPenalty(orderRS.OrderDocument, getReservationOrderDoc);
                    }

                    foreach (var orderItem in orderRS.OrderDocument.OrderItems.Where(a => a.OCNTimestamp != null))
                    {
                        getReservationOrderDoc.OrderItems.First(x => x.OrderItemId == orderItem.OrderItemId).OCNTimestamp = orderItem.OCNTimestamp;
                    }

                    foreach (var orderItem in orderRS.OrderDocument.OrderItems.Where(a => a.SellerFollowUpAction != null && a.SellerFollowUpAction.Any()))
                    {
                        var target = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);
                        if (target != null)
                        {
                            target.SellerFollowUpAction = orderItem.SellerFollowUpAction;
                        }
                    }

                    getReservationOrderDoc.Version = orderRS.OrderDocument.Version;
                    getReservationOrderDoc.ChangeType = orderRS.OrderDocument.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderRS.OrderDocument.ChangeDateTime;
                    getReservationOrderDoc.BookingReference = orderRS.OrderDocument.BookingReference;
                    getReservationOrderDoc.IsPostSaleSSR = orderRS.OrderDocument.IsPostSaleSSR;
                    getReservationOrderDoc.ReshopHoldBookingCounter = orderRS.OrderDocument?.ReshopHoldBookingCounter;

                    if (isUsAgent)
                    {
                        foreach (var orderItem in orderRS.OrderDocument.OrderItems)
                        {
                            var getReservationOrderDocOrderItem = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);
                            {
                                getReservationOrderDocOrderItem.PriceDetails = orderItem.PriceDetails;
                            }
                        }
                    }

                    warnings.AddRange(OrderRetrieveValidation.SSRWarnings(getReservationOrderDoc, _hostEnvironment));

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                    await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                }

                else if (orderRS.OrderDocument.OrderStatus.Is("Cancelled"))
                {
                    foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails)
                    {
                        var getReservationOrderDocCustomer = getReservationOrderDoc.DataList?.CustomerDetails?.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (getReservationOrderDocCustomer != null)
                        {
                            customer.DateOfBirth = getReservationOrderDocCustomer.DateOfBirth;
                            customer.Gender = getReservationOrderDocCustomer.Gender;
                        }
                    }
                    getReservationOrderDoc = orderRS.OrderDocument;
                }
                else if (getReservationOrderDoc.Errors is not null)
                {
                    var orderSearchError = OrderRetrieveValidation.OrderSearchRSValidation(getReservationOrderDoc, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "OrderSearchRSValidation");
                    analyticsEventCommand.Track("ErrorId", orderSearchError.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderSearchError?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderSearchError?.ResponseMessages);
                    return errorDetails;
                }

                bool isHoldBooking = orderRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs == null;
                
                analyticsEventCommand.Track("HoldBooking", isHoldBooking);
                analyticsEventCommand.Track("Delta_PNR", getReservationOrderDoc?.BookingReference?.ExternalRecordLocator);
                analyticsEventCommand.Track("POSCountry", getReservationOrderDoc?.PointOfSale?.CountryCode);
                analyticsEventCommand.Track("POSCity", getReservationOrderDoc?.PointOfSale?.CityCode);


                string approvalCode = orderRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;
                bool isDisruptedOrderChange = IsDisruptedOrderChange(getReservationOrderDoc);
                LogIncompleteOrderData(getReservationOrderDoc);
                var orderResponse = OrderResponseMapper.MapOrderRetrieveResponse(getReservationOrderDoc, warnings, new OrderRetrieveResponseMapOptions
                {
                    IsHoldBooking = isHoldBooking,
                    IsCashPayment = isCashPayment,
                    IsEasyPay = isEasyPayment,
                    ApprovalCode = approvalCode,
                    IsDisruptedOrderRetrieve = isDisruptedOrderChange
                });
                LogOrderDetails(orderResponse, getReservationOrderDoc, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Retrieve Query Handler");
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

        private static bool IsDisruptedOrderChange(OrderDocument orderDocument)
        {
            var hasDisruptionChangeType = orderDocument?.ChangeType is ChangeTypeEnum.FlightDateTimeChange
                or ChangeTypeEnum.FlightNumberChange
                or ChangeTypeEnum.FlightRouteChange
                or ChangeTypeEnum.FlightCancellation;

            var hasReplacementOrderItem = orderDocument?.OrderItems?.Any(orderItem => !orderItem.ExistingOrderItemId.IsNullOrWhiteSpace()) == true;
            var hasPendingSellerFollowUpAction = orderDocument?.OrderItems?.Any(orderItem => orderItem.SellerFollowUpAction?.Any() == true) == true;

            return hasDisruptionChangeType && hasReplacementOrderItem && !hasPendingSellerFollowUpAction;
        }

        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

                if (orderRS?.PaymentFunctions is not null)
                {
                    foreach (var (payment, index) in orderRS?.PaymentFunctions?.Select((payment, index) => (payment, index)))
                    {
                        int i = index;
                        i++;
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

                decimal totalAmount = decimal.Zero;
                decimal baseAmount = decimal.Zero;

                foreach (var orderItem in orderResponse.Order.First().OrderItem)
                {
                    totalAmount += Convert.ToDecimal(orderItem.Price?.TotalAmount?.Value);
                    baseAmount += Convert.ToDecimal(orderItem.Price?.BaseAmount?.Value);
                }

                analyticsEventCommand.Track("TotalAmount", totalAmount);
                analyticsEventCommand.Track("BaseAmount", baseAmount);

                List<string> taxCodes = new();
                foreach (var orderItem in orderResponse.Order.First().OrderItem)
                {
                    if (orderItem.Price.TaxSummary is not null)
                    {
                        foreach (var tax in orderItem.Price.TaxSummary)
                        {
                            taxCodes.AddRange(tax.Tax.Select(x => x.TaxCode));
                        }
                    }

                }

                foreach (var (taxCode, index) in taxCodes.Distinct().Select((taxCode, index) => (taxCode, index)))
                {
                    decimal taxAmount = decimal.Zero;
                    foreach (var orderItem in orderResponse.Order.First().OrderItem)
                    {
                        if (orderItem.Price.TaxSummary is not null)
                        {
                            taxAmount += orderItem.Price.TaxSummary.Where(x => x.Tax.Any(x => x.TaxCode == taxCode)).Sum(tax => Convert.ToDecimal(tax.Tax.First(x => x.TaxCode == taxCode).Amount.Value));
                        }
                    }
                    analyticsEventCommand.Track($"TaxCode{index + 1}", taxCode);
                    analyticsEventCommand.Track($"TaxAmount{index + 1}", taxAmount);
                }

                if (orderResponse.Warning != null)
                {
                    foreach (var (warning, index) in orderResponse.Warning.Select((warning, index) => (warning, index)))
                    {
                        int i = index;
                        i++;
                        analyticsEventCommand.Track($"WarningCode{i}", warning.Code);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }

        private void LogIncompleteOrderData(OrderDocument orderDocument)
        {
            var serviceDefinitions = orderDocument?.DataList?.ServiceDefinitions;
            if (serviceDefinitions == null)
            {
                _logger.LogWarning("Order retrieve response has no service-definition collection. Empty service definitions will be returned.");
            }
            else
            {
                var missingDescriptionCount = serviceDefinitions.Count(serviceDefinition => serviceDefinition?.Description == null);
                if (missingDescriptionCount > 0)
                {
                    _logger.LogWarning("Order retrieve response has {ServiceDefinitionCount} service definitions without descriptions. They will be returned with empty descriptions.", missingDescriptionCount);
                }
            }

            var missingCouponCount = orderDocument?.OrderItems?
                .Where(orderItem => orderItem?.AccountableDocumentRefs != null)
                .SelectMany(orderItem => orderItem.AccountableDocumentRefs)
                .Count(accountableDocument => accountableDocument?.Coupons == null) ?? 0;
            if (missingCouponCount > 0)
            {
                _logger.LogWarning("Order retrieve response has {AccountableDocumentCount} accountable documents without coupons. They will be excluded from ticket documents.", missingCouponCount);
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

        public async Task<Result<Response<IATA_OrderViewRS>>> GDSBookingOrderRetrieve(IATA_OrderRetrieveRQ request, TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
        {
            var orderSearchRequest = Air4OrderSearchRequestMapper.MapOrderSearchRequest(request.Request?.OrderValidationFilterCriteria?.BookingRefFilterCriteria.Carrier.AirlineDesigCode, request.Request?.OrderValidationFilterCriteria?.BookingRefFilterCriteria.BookingID);
            var orderSearchResponse = await _mediator.Send(new Air4OrderSearchMSQuery() { OrderSearchRequest = orderSearchRequest }, cancellationToken);

            var validationErrors = OrderRetrieveValidation.OrderSearchRSValidation<IATA_OrderViewRS>(orderSearchResponse, request.DistributionChain.OrderBy(x => x.Ordinal).LastOrDefault()?.ParticipatingOrg?.OrgID, ConfigReader.Get<bool>(Constants.Environment.SkipGDSAgentValidation), _hostEnvironment);

            if (validationErrors?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "GDSOrderSearchRSValidation");
                analyticsEventCommand.Track("ErrorId", validationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                
                LogErrorDetails(validationErrors?.ResponseMessages, analyticsEventCommand);
                var errorDetails = ErrorMapper.GetErrorDetails(validationErrors?.ResponseMessages);
                return errorDetails;
            }

            var orderViewRS = _gdsOrderResponseMapper.MapOrderResponseForGDS(orderSearchResponse.OrderSearchResponse);
            return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderViewRS };
        }
    }
}
