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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OrderQuote;
using VAA.EAPI.NDC.Offer.Validations.OrderQuote;


namespace VAA.EAPI.NDC.Offer.Application.Query.OrderQuote
{
    public class OrderQuoteQueryHandler : IRequestHandler<OrderQuoteQuery, Result<Response<IATA_OrderReshopRS>>>
    {
        private readonly IMediator _mediator;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly IHttpConnector _httpConnector;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ILogger<OrderQuoteQueryHandler> _logger;
        public OrderQuoteQueryHandler(IMediator mediator, IHostEnvironment hostEnvironment, IGenericDataMapper genericDataMapper, IHttpConnector httpConnector, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<OrderQuoteQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IATA_OrderReshopRS>>> Handle(OrderQuoteQuery request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderQuoteEventSubject
            };

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                var distributionChain = request.OrderQuoteRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);
                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var errorsValidations = OrderQuoteRequestValidations.ValidateOrderQuoteRequest(request.OrderQuoteRQ, _hostEnvironment);
                
                if (errorsValidations?.Status == Status.BusinessException || errorsValidations?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOrderQuoteRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsValidations?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(errorsValidations.ResponseMessages, analyticsEventCommand);
                }

                var orderQuoteRq = OrderQuoteRQMapper.MapOrderQuoteRequestToVAJson(request.OrderQuoteRQ, false,null);

                if (orderQuoteRq != null)
                {
                    analyticsEventCommand.Track("AgencyName", orderQuoteRq?.Parties?.Participants?.FirstOrDefault()?.Name);
                    analyticsEventCommand.Track("AgencyType", orderQuoteRq?.Parties?.Participants?.FirstOrDefault()?.PartyType);
                    analyticsEventCommand.Track("OfferID", orderQuoteRq?.PricedOffer?.SelectedOffer?.OfferRefID);
                    analyticsEventCommand.Track("ISHoldBooking", orderQuoteRq?.IsHeldBooking);
                    analyticsEventCommand.Track("OrderID", orderQuoteRq?.OrderId);

                    var paxGroup = orderQuoteRq.CustomerDetails?.GroupBy(x => x.Ptc).Select(x => new { Pax = x.Key, PaxCount = x.Count() });

                    if (paxGroup != null)
                    {
                        int i = 1;
                        foreach (var pax in paxGroup)
                        {
                            analyticsEventCommand.Track($"PaxType{i}", pax.Pax);
                            analyticsEventCommand.Track($"PaxCount{i}", pax.PaxCount);
                            i++;
                        }

                    }
                }

                var upsellOffersConfigResponse = await _mediator.Send(new UpsellOffersDBQuery(), default);
                var upsellConfigErrors = OrderQuoteRequestValidations.ValidateUpsellConfigResponse(upsellOffersConfigResponse, _hostEnvironment);

                if (upsellConfigErrors?.Status == Status.BusinessException || upsellConfigErrors?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "UpsellConfigErrors");
                    analyticsEventCommand.Track($"ErrorId", upsellConfigErrors?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(upsellConfigErrors.ResponseMessages, analyticsEventCommand);
                }

                var result = orderQuoteRq.IsHeldBooking ? await OrderQuoteForHoldBooking(request, orderQuoteRq, cancellationToken, analyticsEventCommand) : await OrderQuote(request, orderQuoteRq, upsellOffersConfigResponse, cancellationToken, analyticsEventCommand);

                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Order Quote Query Handler");

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                return new Response<IATA_OrderReshopRS>()
                {
                    Status = Status.BusinessException,
                    Value = MapToErrorOrderQuoteRS(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        public async Task<Result<Response<IATA_OrderReshopRS>>> OrderQuoteForHoldBooking(OrderQuoteQuery request, OrderRepriceQuery orderRequest, CancellationToken cancellationToken, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var quoteResponse = new OfferResponse();

            var orderQuoteResult = OrderQuoteRequestValidations.ValidateMandatoryRequestFields(orderRequest, _hostEnvironment);

            if (orderQuoteResult?.Status == Status.BusinessException || orderQuoteResult?.Status == Status.Error)
            {
                analyticsEventCommand.Track("ServiceName", "OrderQuoteResult");
                analyticsEventCommand.Track($"ErrorId", orderQuoteResult?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(orderQuoteResult.ResponseMessages, analyticsEventCommand);
            }

            var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = orderRequest.OrderId }, cancellationToken);
            bool isHoldBookingReshop = orderDocument?.TimeLimits is not null && orderDocument.OrderItems?.Any(x => x.AccountableDocumentRefs is null) == true;

            bool isReshopDisruption = orderDocument?.OrderItems?.Any(oi => oi?.SellerFollowUpAction?.Any(x => x.Is("Reshop")) == true) == true && orderDocument?.DataList?.SegmentDetails?.Any(x => x.ScheduleChangeIndicator) == true;

            analyticsEventCommand.Track($"VS_PNR", orderDocument?.BookingReference?.RLOC);
            analyticsEventCommand.Track($"Delta_PNR", orderDocument?.BookingReference?.ExternalRecordLocator);

            if (orderDocument != null)
            {
                var orderStatusValidation = OrderQuoteRequestValidations.ValidateOrderCancelledStatus(orderDocument, _hostEnvironment);
                if (orderStatusValidation != null && orderStatusValidation?.Status == Status.BusinessException)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderStatusValidation");
                    return orderStatusValidation;
                }

                var ttlValidation = OrderQuoteRequestValidations.ValidateHoldBookingTTL(orderDocument, _hostEnvironment);

                if (ttlValidation != null && ttlValidation?.Status == Status.BusinessException)
                {
                    analyticsEventCommand.Track("ServiceName", "TtlValidation");
                    analyticsEventCommand.Track($"ErrorId", ttlValidation?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(ttlValidation.ResponseMessages, analyticsEventCommand);
                }
            }

            var validationResult = ValidationMSRequestMapper.MapValidationRQReshop(orderRequest?.Parties, orderDocument?.Parties?.CorporateAccount);
            var Response = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationResult, AgentSubscriptionKey = request.SubscriptionKey });

            if (Response?.Error != null)
            {
                var validation = OrderQuoteRequestValidations.ValidateAgency(Response, _hostEnvironment);
                
                analyticsEventCommand.Track("ServiceName", "ValidateAgency");
                analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
            }

            else
            {
                analyticsEventCommand.Track("AgencyType", Response?.ValidateResponse?.AgencyDetails.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", Response?.ValidateResponse?.AgencyDetails.RequestOrgName);

                if (Response?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", Response?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", Response?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }

                analyticsEventCommand.Track("CorporateId", Response?.ValidateResponse?.CorporateDetails.CorporateId);
                analyticsEventCommand.Track("PosCountry", Response?.ValidateResponse?.AgencyDetails?.CountryCode);

            }

            var orderDocumentValidation = OrderQuoteRequestValidations.ValidateOrderQuoteHoldBookingRequest(orderDocument, _hostEnvironment);

            if (orderDocumentValidation != null || orderDocumentValidation?.Status == Status.BusinessException || orderDocumentValidation?.Status == Status.Error)
            {
                analyticsEventCommand.Track("ServiceName", "OrderDocumentValidation");
                analyticsEventCommand.Track($"ErrorId", orderDocumentValidation?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(orderDocumentValidation.ResponseMessages, analyticsEventCommand);
            }

            if (orderDocument?.Warnings?.Any() ?? false)
            {
                var warnings = OrderQuoteRequestValidations.ValidateOrderQuoteOrderWarnings(orderDocument, _hostEnvironment);

                return new Response<IATA_OrderReshopRS>() { Status = Status.BusinessException, Value = MapWarnings(warnings) };
            }
            
            bool isUsAgent = Response?.ValidateResponse?.AgencyDetails?.CountryCode == "US";
            analyticsEventCommand.Track("isUsAgent", isUsAgent);

            var getReservationReq = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);
            var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);
            IEnumerable<BrandRS> brandResponse;

            if (getReservationOrderDoc is not null)
            {
                if (orderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                {
                    getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                }
                foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                {
                    getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                }


                foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                {
                    getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                }

                if (orderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderDocument.FareTypeRequest;

                foreach (var customer in orderDocument.DataList.CustomerDetails)
                {
                    var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                    if (getReservationOrderDocCustomer != null)
                    {
                        getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                    }
                }

                getReservationOrderDoc = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderDocument, getReservationOrderDoc);
                
                if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var brandMsReq = BrandMsRqMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc, orderDocument);
                    brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);
                }
                else 
                {
                    var brandMsReq = BrandMsRqMapper.MapBrandMSOrderRequest(getReservationOrderDoc, orderDocument);
                    brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                }
                
                getReservationOrderDoc = BrandMsRSMapper.MapBrandMSResponseUsingSabreOrderDoc(getReservationOrderDoc, brandResponse, orderDocument);

                getReservationOrderDoc.Version = orderDocument.Version;
                getReservationOrderDoc.ChangeType = orderDocument.ChangeType;
                getReservationOrderDoc.ChangeDateTime = orderDocument.ChangeDateTime;
                getReservationOrderDoc.BookingReference = orderDocument.BookingReference;

                if (isUsAgent)
                {
                    foreach (var orderItem in orderDocument.OrderItems)
                    {
                        var getReservationOrderDocOrderItem = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);
                        {
                            getReservationOrderDocOrderItem.PriceDetails = orderItem.PriceDetails;
                        }
                    }
                }

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                var publishOrderDocResponse = await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
            }

            quoteResponse.NoChangeId = true;
            quoteResponse.OrderId = getReservationOrderDoc?.OrderNumber?.OrderId;

            var orderQuoteResponse = await OrderQuoteResponse(isHoldBookingReshop,isReshopDisruption, quoteResponse, orderRequest, null, cancellationToken, analyticsEventCommand, Response);

            return orderQuoteResponse;
        }
        public async Task<Result<Response<IATA_OrderReshopRS>>> OrderQuote(OrderQuoteQuery request, OrderRepriceQuery orderQuoteRQ, IEnumerable<UpsellOffersData> upsellOffersConfigResponse, CancellationToken cancellationToken, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            bool isCreditCardAccepted = false;

            var orderQuoteResult = OrderQuoteRequestValidations.ValidateMandatoryFields(orderQuoteRQ, _hostEnvironment);

            if (orderQuoteResult?.Status == Status.BusinessException || orderQuoteResult?.Status == Status.Error)
            {
                analyticsEventCommand.Track("ServiceName", "OrderQuoteResult");
                analyticsEventCommand.Track($"ErrorId", orderQuoteResult?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(orderQuoteResult.ResponseMessages, analyticsEventCommand);
            }

            var offerDocument = await _mediator.Send(new OfferMSSearchQuery(orderQuoteRQ.PricedOffer.SelectedOffer.OfferRefID), default);
                        
            orderQuoteResult = OrderQuoteRequestValidations.ValidateOrderReshopDB(orderQuoteRQ, offerDocument, _hostEnvironment);

            if (orderQuoteResult?.Status == Status.BusinessException || orderQuoteResult?.Status == Status.Error)
            {
                analyticsEventCommand.Track("ServiceName", "ValidateOrderReshopDB");
                analyticsEventCommand.Track($"ErrorId", orderQuoteResult?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(orderQuoteResult.ResponseMessages, analyticsEventCommand);
            }

            var validationMSRequest = ValidationMSRequestMapper.MapValidationRQQuote(orderQuoteRQ.Parties, offerDocument?.Parties.CorporateAccount, true);
            var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey });

            if (validationMSRS?.Error != null)
            {
                var validation = OrderQuoteRequestValidations.ValidateAgency(validationMSRS, _hostEnvironment);
                analyticsEventCommand.Track("ServiceName", "ValidationMSRS");
                analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
            }

            else
            {
                analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse.AgencyDetails.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse.AgencyDetails.RequestOrgName);
                analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode);
            }

            var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = orderQuoteRQ.OrderId }, cancellationToken);
            
            // Validate Seller Follow Action for Disruption: Reshop Scenarios
            var disruptionValidation = OrderQuoteRequestValidations.ValidateSellerFollowUpAction<IATA_OrderReshopRS>(orderDocument, _hostEnvironment);

            if (disruptionValidation?.ResponseMessages != null)
            {
                analyticsEventCommand.Track("ServiceName", "SellerFollowUpActionValidation");
                analyticsEventCommand.Track("ErrorId", disruptionValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                return ErrorDetails(disruptionValidation?.ResponseMessages, analyticsEventCommand);
            }

            bool isReshopDisruption = orderDocument?.OrderItems?.Any(oi => oi?.SellerFollowUpAction?.Any(x => x.Is("Reshop")) == true) == true && orderDocument?.DataList?.SegmentDetails?.Any(x => x.ScheduleChangeIndicator) == true;

            bool isHoldBookingReshop = orderDocument?.TimeLimits is not null && orderDocument.OrderItems?.Any(x => x.AccountableDocumentRefs is null) == true;

                       var countryCode = validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode;
            var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

            if (currencyData != null)
            {
                var currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                var bspMarketPayment = await _mediator.Send(new BspMarketPaymentQuery(countryCode, currencyCode), cancellationToken);
                isCreditCardAccepted = bspMarketPayment.IsCreditCardAccepted;
            }

            var accountCode = offerDocument.Parties?.Participants?.FirstOrDefault(x => x.PartyType.Value == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller)?.AccountCode;
            var corporateAccountDetails = offerDocument.Parties?.CorporateAccount;

            var orderRepriceQuery = OrderQuoteRQMapper.MapOrderQuoteRq(request, orderQuoteRQ, offerDocument, upsellOffersConfigResponse, isCreditCardAccepted, _genericDataMapper,accountCode , corporateAccountDetails, isReshopDisruption, orderDocument, isHoldBookingReshop);
            
            var reshopOfferDocResponse = await _mediator.Send(orderRepriceQuery);

            var allUpsellOffers = reshopOfferDocResponse.Error == null && reshopOfferDocResponse.Offers.Count() == reshopOfferDocResponse.Offers.Count(x => x.Selected == false);
            
            if (allUpsellOffers || reshopOfferDocResponse.Error != null)
            {
                var errorDetails = OrderQuoteResponseValidations.ValidateOrderQuoteResponse(reshopOfferDocResponse, allUpsellOffers, _hostEnvironment);
                analyticsEventCommand.Track("ServiceName", "ValidateOrderQuoteResponse");
                analyticsEventCommand.Track($"ErrorId", errorDetails?.ResponseMessages?.Errors?.First().Id);
                
                return ErrorDetails(errorDetails.ResponseMessages, analyticsEventCommand);
            }
            //Disrupted PNR 
            if (isReshopDisruption && reshopOfferDocResponse?.Offers != null)
            {
                var bookedCabinCode = orderDocument?.DataList?.PriceClass?.FirstOrDefault()?.CabinType?.CabinCode;
                var priceClassCabinById = reshopOfferDocResponse.DataList?.PriceClass?
             .Where(pc => !string.IsNullOrWhiteSpace(pc.Id))
                     .GroupBy(pc => pc.Id)
                     .ToDictionary(g => g.Key, g => g.First().CabinType?.CabinCode)
                     ?? new Dictionary<string, string>();

                reshopOfferDocResponse.Offers = reshopOfferDocResponse.Offers
                    .Where(o => !o.UpsellOffer)
                    .Where(o => 
                    {
                        if (string.IsNullOrWhiteSpace(bookedCabinCode)) return true;

                        var offerCabinCodes = (o.OfferItems ?? Enumerable.Empty<OfferItem>())
                             .SelectMany(oi => oi.Services ?? Enumerable.Empty<OfferService>())
                             .SelectMany(svc => svc.PriceClassRefIds ?? Enumerable.Empty<string>())
                             .Select(pcRefId => priceClassCabinById.TryGetValue(pcRefId, out var code) ? code : null)
                             .Where(code => !string.IsNullOrWhiteSpace(code))
                             .Distinct()
                             .ToList();
                        return offerCabinCodes.Count == 0 || offerCabinCodes.All(code => code == bookedCabinCode);
                    })
                    .ToList();
            }

            if (orderRepriceQuery.RepriceQueries.SearchCriteria.FareFamilies.Count() == 1 && orderRepriceQuery.RepriceQueries.SearchCriteria.FareFamilies.Last() == Constants.Application.UpperClassFlex)
            {
                reshopOfferDocResponse.Offers = reshopOfferDocResponse.Offers.Where(x => x.UpsellOffer == false);
            }
                        
            reshopOfferDocResponse.DataList.BaggageAllowances = [];
            reshopOfferDocResponse.DataList.ProductComponents = [];
            bool isPartialFlown = orderDocument.DataList.SegmentDetails.Any(x => x.Status?.DeliveryStatusCode == "Delivered");
            ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsPTCResponseList = new();
            ConcurrentDictionary<OfferDetails, Infrastructure.Model.BrandMS.BrandRS> brandMsResponseList = new();
            IEnumerable<BrandRS> brandResponse;

            int penaltyIndex = 1;
            int checkedBagCounter = 1;
            int serviceCounter = 1;
            IEnumerable<BaggageAllowances> baggageList = null;

            if (reshopOfferDocResponse.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
            {
                var tasks = reshopOfferDocResponse.Offers.Select(async offer =>
                {
                    var brandMsReq = BrandMsRqMapper.MapBrandMsPTCRequest(offer, reshopOfferDocResponse, isPartialFlown);
                    brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);

                    if (!brandMsPTCResponseList.ContainsKey(offer))
                        brandMsPTCResponseList.TryAdd(offer, brandResponse);
                });
                await Task.WhenAll(tasks);
            }
            else 
            {
                var tasks = reshopOfferDocResponse.Offers.Select(async offer =>
                {
                    var brandMsReq = BrandMsRqMapper.MapBrandMsRequest(offer, reshopOfferDocResponse, isPartialFlown);
                    brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);

                    if (!brandMsResponseList.ContainsKey(offer))
                        brandMsResponseList.TryAdd(offer, brandResponse.First());
                });
                await Task.WhenAll(tasks);

            }

            if (brandMsPTCResponseList.Any())
            {
                foreach (var brandRespPTC in brandMsPTCResponseList)
                {
                    reshopOfferDocResponse = BrandMsRSMapper.UpdateOrderQuotePTC(brandRespPTC.Value, brandRespPTC.Key, reshopOfferDocResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);
                }
            }
            else 
            {
                foreach (KeyValuePair<OfferDetails, Infrastructure.Model.BrandMS.BrandRS> kvp in brandMsResponseList)
                {
                    reshopOfferDocResponse = BrandMsRSMapper.UpdateOrderQuote(kvp.Value, kvp.Key, reshopOfferDocResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);
                }
            }
                

            var orderQuoteResponse = await OrderQuoteResponse(isHoldBookingReshop, isReshopDisruption, reshopOfferDocResponse, orderQuoteRQ, offerDocument ,cancellationToken, analyticsEventCommand, validationMSRS);

            return orderQuoteResponse;
        }
        public async Task<Result<Response<IATA_OrderReshopRS>>> OrderQuoteResponse(bool isHoldBookingReshop, bool isReshopDisruption, OfferResponse offerResponse, OrderRepriceQuery orderRepriceQuery, OfferResponse offerDocument, CancellationToken cancellationToken, TrackAnalyticsEventCommand analyticsEventCommand, Infrastructure.Model.ValidationMS.ValidationRS validationMSRS)
        {
            var orderQuoteResponse = new Response<IATA_OrderReshopRS>();
           
            bool isNonIataAgent = validationMSRS!.ValidateResponse.AgencyDetails.RequestAgencyIdType == Constants.Application.TIDS;

            var agencyPermission = validationMSRS.ValidateResponse.AgencyPermissions;

            bool isCashPaymentAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.CashFopAllowedInd == true;
            bool isEasyFopAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.EasyPayFopAllowedInd == true;
            bool isCreditCardAccepted = false;

            var countryCode = validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode;
            var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

            BspMarketPaymentData bspMarketPaymentData = new BspMarketPaymentData();

            if (currencyData != null)
            {
                var currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                bspMarketPaymentData = await _mediator.Send(new BspMarketPaymentQuery(countryCode, currencyCode), cancellationToken);
                isCreditCardAccepted = bspMarketPaymentData.IsCreditCardAccepted;
            }

            var nDCWarnings = new List<NDCWarning>();

            if (offerResponse != null && !isReshopDisruption)
            {
                var refundWarning = OrderQuoteResponseValidations.ValidateQuoteRefundOffer(offerResponse, _hostEnvironment);
                if (refundWarning.Any())
                {
                    nDCWarnings.AddRange(refundWarning.Where(item => !nDCWarnings.Contains(item) && !nDCWarnings.Any(x => x.Code == "745")));

                }
            }

            if (offerResponse != null && offerResponse?.Error == null)
            {
                analyticsEventCommand.Track("PosCity", offerResponse?.Pos?.CityCode);
                bool oneway = offerResponse?.DataList?.OriginDestinationAssociations?.Count() == 1;
                bool roundtrip = false;

                if (offerResponse?.DataList?.OriginDestinationAssociations?.Count() == 2)
                {
                    var outboundOrigin = offerResponse?.DataList?.OriginDestinationAssociations?.FirstOrDefault()?.OriginDestination.OriginAirport;
                    var outboundDestination = offerResponse?.DataList?.OriginDestinationAssociations?.FirstOrDefault()?.OriginDestination.DestinationAirport;
                    var inboundOrigin = offerResponse?.DataList?.OriginDestinationAssociations?.FirstOrDefault()?.OriginDestination.OriginAirport;
                    var inboundDestination = offerResponse?.DataList?.OriginDestinationAssociations?.FirstOrDefault()?.OriginDestination.DestinationAirport;

                    if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                    {
                        roundtrip = true;
                    }
                }

                if (offerResponse?.DataList?.OriginDestinationAssociations != null)
                {
                    analyticsEventCommand.Track("Oneway", oneway ? 'Y' : 'N');
                    analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                    analyticsEventCommand.Track("Multicity", (!oneway && !roundtrip) ? 'Y' : 'N');
                }

                for (int i = 1; i <= offerResponse?.DataList?.OriginDestinationAssociations.Count(); i++)
                {
                    var ond = offerResponse?.DataList?.OriginDestinationAssociations.ElementAt(i - 1);
                    analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                    analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);
                }

                var priceClassIds = offerResponse.Offers?.SelectMany(x => x.OfferItems).SelectMany(x => x.Services).Select(x => x.PriceClassRefIds.FirstOrDefault());

                if (priceClassIds != null)
                {
                    var cabins = (from gpc in priceClassIds
                                  join pc in offerResponse.DataList?.PriceClass
                                  on gpc equals pc.Id
                                  group gpc by new { pc.CabinType.CabinCode, pc.CabinType.CabinName } into g
                                  select new { g.Key.CabinName, Count = g.Count() }).ToList();

                    for (int i = 1; i <= cabins?.Count; i++)
                    {
                        analyticsEventCommand.Track($"Cabin{i}", cabins[i - 1].CabinName);
                        analyticsEventCommand.Track($"CabinCount{i}", cabins[i - 1].Count);
                    }

                    var farefamily = (from gpc in priceClassIds
                                      join pc in offerResponse.DataList?.PriceClass
                                      on gpc equals pc.Id
                                      group gpc by pc.ProductName into g
                                      select new { g.Key, Count = g.Count() }).ToList();

                    for (int i = 1; i <= farefamily?.Count; i++)
                    {
                        analyticsEventCommand.Track($"FareFamily{i}", farefamily[i - 1].Key);
                        analyticsEventCommand.Track($"FareFamilyCount{i}", farefamily[i - 1].Count);
                    }
                }
                var flights = offerResponse.DataList?.Journey?.Select(journey =>
                {
                    var segments = offerResponse.DataList?.FlightSegments.Where(fs => journey.FlightSegmentRefIds.Contains(fs.SegmentId));

                    return new
                    {
                        Segments = segments.Select(s =>
                        {
                            return new { StopCount = s.Legs.Count() - 1 };
                        })
                    };
                });

                analyticsEventCommand.Track("DirectFlights", flights?.Count(f => f.Segments.Count() == 1 && f.Segments.First().StopCount == 0));
                analyticsEventCommand.Track("1StopFlights", flights?.Count(f => f.Segments.Count() == 2));
                analyticsEventCommand.Track("2+StopFlights", flights?.Count(f => f.Segments.Count() > 2));
                analyticsEventCommand.Track("OffersCount", offerResponse.Offers?.Count());
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);


                orderQuoteResponse = orderRepriceQuery.IsHeldBooking? OrderQuoteRSMapper.OrderQuoteHoldBookingRSMapping(offerResponse, isCashPaymentAllowed, isEasyFopAllowed, bspMarketPaymentData, isNonIataAgent): OrderQuoteRSMapper.OrderQuoteRSMapping(isHoldBookingReshop, isReshopDisruption, offerResponse, offerDocument, _genericDataMapper, nDCWarnings, isCashPaymentAllowed, isEasyFopAllowed, bspMarketPaymentData, isNonIataAgent);
                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsEventCommand.Track("DateOfResponse", dateOfResponse); 

                return orderQuoteResponse;
            }
            else
            {
                var errors = offerResponse.Error.Where(x => x.DeveloperMessage.Contains("ErrorID"));
                var nonErrors = offerResponse.Error.Where(x => !x.DeveloperMessage.Contains("ErrorID"));
                var warnings = nonErrors.Select(x => new EAPI.Domain.Entities.Common.Warning { Code = x.Code, Details = x.DeveloperMessage, Message = x.Message });
                var responseMessage = new ResponseMessage { Errors = errors, Warnings = warnings };

                return ErrorDetails(responseMessage, analyticsEventCommand);
            }

        }

        #region "Error Handling"
        private Response<IATA_OrderReshopRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);
            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            
            analyticsEventCommand.Track("StatusCode", statusCode);
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            return new Response<IATA_OrderReshopRS>()
            {
                Status = resultCode,
                Value = MapToErrorOrderQuoteRS(responseMessage)
            };
        }

        private static IATA_OrderReshopRS MapToErrorOrderQuoteRS(ResponseMessage errors)
        {
            return new IATA_OrderReshopRS
            {
                Items = errors.Errors.Select(error =>
                        new ErrorType()
                        {
                            Code = error.Code,
                            DescText = error.Message,
                            LangCode = Constants.Application.LangCode,
                            TypeCode = error.StatusText,
                            TagText = error.TagText
                        }).ToArray()
            };
        }

        private IATA_OrderReshopRS MapWarnings(IEnumerable<NDCWarning> warnings)
        {
            return new IATA_OrderReshopRS
            {
                Items = warnings.Select(x => new WarningType()
                {
                    Code = Convert.ToString(x.Code),
                    DescText = x.ShortText,
                    LangCode = Constants.Application.LangCode,
                    TypeCode = x.Type,
                    TagText = x.TagText
                }).ToArray()
            };
        }
        #endregion
    }
}
