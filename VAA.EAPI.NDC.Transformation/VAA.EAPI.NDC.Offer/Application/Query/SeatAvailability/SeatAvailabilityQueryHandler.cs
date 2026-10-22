using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using VAA.EAPI.NDC.Offer.Mapper.SeatAvailability;
using VAA.EAPI.NDC.Offer.Validations;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;
using ErrorType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS.ErrorType;
using Status = VAA.EAPI.Domain.Entities.Common.Status;
using WarningType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS.WarningType;

namespace VAA.EAPI.NDC.Offer.Application.Query.Delta.SeatAvailability
{
    public class SeatAvailabilityQueryHandler : IRequestHandler<SeatAvailabilityQuery, Result<Response<IATA_SeatAvailabilityRS>>>
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IMediator _mediator;
        private readonly ISeatAvailabilityValidations<IATA_SeatAvailabilityRS> _seatAvailabilityValidations;
        private readonly ILogger<SeatAvailabilityQueryHandler> _logger;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public SeatAvailabilityQueryHandler(IHostEnvironment hostEnvironment, IMediator mediator, ISeatAvailabilityValidations<IATA_SeatAvailabilityRS> seatAvailabilityValidations, ILogger<SeatAvailabilityQueryHandler> logger, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _seatAvailabilityValidations = seatAvailabilityValidations ?? throw new ArgumentNullException(nameof(seatAvailabilityValidations));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));

        }

        public async Task<Result<Response<IATA_SeatAvailabilityRS>>> Handle(SeatAvailabilityQuery request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool isCreditCardAccepted = false;

            var ndc_Warnings = Enumerable.Empty<NDCWarning>();

            IATA_SeatAvailabilityRS seatResponse = new();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.SeatAvailabilityEventSubject
            };

            var distributionChain = request.SeatAvailabilityRQ?.DistributionChain;

            var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
            var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

            analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
            if (distributor?.ParticipatingOrg?.OrgID is not null)
            {
                analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
            }
            var responseMessage = RequestValidations.NullSeatAvailabilityErrorResponseMessage(request.SeatAvailabilityRQ);

            if (responseMessage.Errors != null && responseMessage.Errors.Any(x => x.Code == Constants.Application.NDCNULLRQERRORCODE))
            {
                analyticsEventCommand.Track("ErrorId", responseMessage?.Errors?.First().Id);
                analyticsEventCommand.Track("ServiceName", "RequestValidation");
                return ErrorDetails(responseMessage, analyticsEventCommand);
            }

            var seatValidationResults = _seatAvailabilityValidations.ValidateSeatRequest(request.SeatAvailabilityRQ);

            if (seatValidationResults != null || seatValidationResults?.Status == Status.BusinessException || seatValidationResults?.Status == Status.Error)
            {
                analyticsEventCommand.Track("ErrorId", seatValidationResults?.ResponseMessages?.Errors?.First().Id);
                analyticsEventCommand.Track("ServiceName", "SeatValidation");
                return ErrorDetails(seatValidationResults.ResponseMessages, analyticsEventCommand);
            }

            var parties = OfferRequestMapper.GetPartiesFromSeatReq(request.SeatAvailabilityRQ);
            var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties);
            var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

            if (validationMSResponse?.Error != null)
            {
                var validation = _seatAvailabilityValidations.ValidateAgency(validationMSResponse);
                analyticsEventCommand.Track("ServiceName", "ValidationMS");
                analyticsEventCommand.Track("ErrorId", validation?.ResponseMessages?.Errors?.First().Id);

                return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
            }
            else
            {
                analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails.RequestOrgName);
                if (validationMSResponse?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }
                analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode);
            }

            var countryCode = validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode;

            var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

            if (currencyData != null)
            {
                var currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                var bspMarketPayment = await _mediator.Send(new BspMarketPaymentQuery(countryCode, currencyCode), cancellationToken);
                isCreditCardAccepted = bspMarketPayment.IsCreditCardAccepted;
            }

            bool isPrimeSeatFlow = (request.SeatAvailabilityRQ.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType)?.Offer != null;

            // Prime sale Flow
            if (isPrimeSeatFlow)
            {
                try
                {
                    var seatReqValidation = _seatAvailabilityValidations.ValidateSeatServiceRequest(request.SeatAvailabilityRQ);

                    if (seatReqValidation != null || seatReqValidation?.Status == Status.BusinessException || seatReqValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatReqValidation");
                        analyticsEventCommand.Track("ErrorId", seatReqValidation?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(seatReqValidation.ResponseMessages, analyticsEventCommand);
                    }

                    var offerId = (request.SeatAvailabilityRQ.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType)?.Offer.FirstOrDefault()?.OfferID;
                    analyticsEventCommand.Track("IsPrime", "Y");
                    analyticsEventCommand.Track("OfferID", offerId);

                    var offerDoc = offerId != null ? await _mediator.Send(new OfferMSSearchQuery(offerId)) : null;
                    seatReqValidation = _seatAvailabilityValidations.ValidateSeatAvailabilityRequest(request.SeatAvailabilityRQ, offerDoc, offerId);

                    if (seatReqValidation != null || seatReqValidation?.Status == Status.BusinessException || seatReqValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "SeatReqValidation"); 
                        analyticsEventCommand.Track("ErrorId", seatReqValidation?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(seatReqValidation.ResponseMessages, analyticsEventCommand);
                    }

                    if (offerDoc.Error == null || offerDoc.Error.Any())
                    {
                        analyticsEventCommand.Track("Trigger", "OfferID");
                        analyticsEventCommand.Track("OfferID", offerDoc.Offers.First().OfferId);
                        analyticsEventCommand.Track("AgencyName", offerDoc.Parties?.Participants?.FirstOrDefault()?.Name);
                        analyticsEventCommand.Track("PosCountry", offerDoc.Pos?.CountryCode);

                        bool oneWay = offerDoc.DataList?.OriginDestinationAssociations?.Count() == 1;
                        bool roundtrip = false;

                        if (offerDoc.DataList?.OriginDestinationAssociations?.Count() == 2)
                        {
                            var outboundOrigin = offerDoc.DataList?.OriginDestinationAssociations.First().OriginDestination.OriginAirport;
                            var outboundDestination = offerDoc.DataList?.OriginDestinationAssociations.First().OriginDestination.DestinationAirport;
                            var inboundOrigin = offerDoc.DataList?.OriginDestinationAssociations.Last().OriginDestination.OriginAirport;
                            var inboundDestination = offerDoc.DataList?.OriginDestinationAssociations.Last().OriginDestination.DestinationAirport;

                            if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                            {
                                roundtrip = true;
                            }
                        }

                        analyticsEventCommand.Track("Oneway", oneWay ? 'Y' : 'N');
                        analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                        analyticsEventCommand.Track("Multicity", (!oneWay && !roundtrip) ? 'Y' : 'N');

                        for (int i = 1; i <= offerDoc.DataList?.OriginDestinationAssociations?.Count(); i++)
                        {
                            var ond = offerDoc.DataList.OriginDestinationAssociations.ElementAt(i - 1);
                            analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                            analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);

                        }
                        var paxGroup = offerDoc.DataList?.CustomerDetails?.GroupBy(x => x.Ptc).Select(x => new { Pax = x.Key, PaxCount = x.Count() }).ToList();

                        for (int i = 1; i <= paxGroup?.Count; i++)
                        {
                            analyticsEventCommand.Track($"PaxType{i}", paxGroup[i - 1].Pax);
                            analyticsEventCommand.Track($"PaxCount{i}", paxGroup[i - 1].PaxCount);
                        }

                        var priceClassIds = offerDoc.Offers?.SelectMany(x => x.OfferItems).SelectMany(x => x.Services).Select(x => x.PriceClassRefIds.FirstOrDefault());

                        if (priceClassIds != null)
                        {
                            var cabins = (from gpc in priceClassIds
                                          join pc in offerDoc.DataList.PriceClass
                                          on gpc equals pc.Id
                                          group gpc by new { pc.CabinType.CabinCode, pc.CabinType.CabinName } into g
                                          select new { g.Key.CabinName, Count = g.Count() }).ToList();

                            for (int i = 1; i <= cabins?.Count; i++)
                            {
                                analyticsEventCommand.Track($"Cabin{i}", cabins.ElementAt(i - 1).CabinName);
                            }

                            var fareFamily = (from gpc in priceClassIds
                                              join pc in offerDoc.DataList.PriceClass
                                              on gpc equals pc.Id
                                              group gpc by pc.ProductName into g
                                              select new { g.Key, Count = g.Count() }).ToList();

                            for (int i = 1; i <= fareFamily?.Count; i++)
                            {
                                analyticsEventCommand.Track($"FareFamily{i}", fareFamily[i - 1].Key);

                            }
                        }

                        var seatMSResponse = new VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS.SeatRS();
                        var flightSegmentsList = GetFlightSegments(offerDoc);

                        bool vssSegmentCheck = false;

                        foreach (var vssSeg in flightSegmentsList.Where(seg => (seg.OperatingCarrier == null) || (seg.OperatingCarrier == "DL")))
                        {
                            vssSegmentCheck = true;
                        }

                        seatReqValidation = (!vssSegmentCheck) ? _seatAvailabilityValidations.ValidateEligibleFlights(flightSegmentsList) : null;

                        if (seatReqValidation != null || seatReqValidation?.Status == Status.BusinessException || seatReqValidation?.Status == Status.Error)
                        {
                            analyticsEventCommand.Track("ServiceName", "SeatReqValidation");
                            analyticsEventCommand.Track("ErrorId", seatReqValidation?.ResponseMessages?.Errors?.First().Id);

                            return ErrorDetails(seatReqValidation.ResponseMessages, analyticsEventCommand);
                        }

                        flightSegmentsList = flightSegmentsList.Where(x => (x.MarketingCarrier == Constants.Application.OwnerCode && x.OperatingCarrier is null) || (x.OperatingCarrier == "DL"));

                        if (flightSegmentsList.Any())
                        {
                            var seatMSRQ = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestOfferDoc(request, offerDoc, isCreditCardAccepted, flightSegmentsList);
                            seatMSResponse = await _mediator.Send(seatMSRQ, cancellationToken);
                        }

                        if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap == null)
                        {
                            return new Response<IATA_SeatAvailabilityRS>()
                            {
                                Status = Status.Error,
                                Value = MapToErrorSeatAvailabilityRS(GetTechError())
                            };
                        }

                        var paxList = GetPaxListOfferDoc(offerDoc);
                        var warningList = _seatAvailabilityValidations.PopulateNonVSSegmentWarnings(offerDoc).Concat(_seatAvailabilityValidations.PopulateSeatMapResponseWarnings(seatMSResponse).Concat(_seatAvailabilityValidations.PopulateFOPWarning()));

                        OrderDocument orderDocument = new();

                        if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap is not null)
                        {
                            seatResponse = SeatAvailabilityRSMapper.MapSeatAvailabilityResponse(seatMSResponse, paxList, orderDocument, offerDoc, _hostEnvironment);

                            List<string> seatData = new List<string>();

                            for (int i = 0; i < seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.Count(); i++)
                            {
                                for (int j = 0; j < seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.Count(); j++)
                                {
                                    var seatProductName = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).ProductName;
                                    var seatPrice = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).SeatPrice.MonetaryAmount;
                                    var seatCurrency = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).SeatPrice.BaseCurrency;
                                    var key = $"SeatMap{i}:" + "Product Name:" + seatProductName + "| Price:" + seatPrice + " " + seatCurrency;

                                    seatData.Add(key);

                                }
                            }

                            var seatInfo = seatData?.GroupBy(seatData => seatData).Select(x => new { SeatKey = x.Key, SeatKeyCount = x.Count() }).ToList();

                        }

                        var seatWarnings = ErrorHelper<NDCWarning>.GetWarnings(warningList, _hostEnvironment);

                        if (seatResponse != null)
                        {
                            (seatResponse?.Items as SeatAvailResponseType[]).First().Warning = MapWarnings(seatWarnings).Items as WarningType[];
                        }

                        if ((seatResponse?.Items as SeatAvailResponseType[]).FirstOrDefault()?.ALaCarteOffer == null)
                        {
                            if (seatWarnings.Any())
                            {
                                for (int i = 0; i < seatWarnings?.Count(); i++)
                                {
                                    if (seatWarnings.ElementAt(i).ShortText != null && seatWarnings.ElementAt(i).Type == "Warning")
                                    {
                                        analyticsEventCommand.Track($"warnings{i}", seatWarnings.ElementAt(i).ShortText);
                                    }
                                }

                                return new Response<IATA_SeatAvailabilityRS>() { Status = Status.BusinessWarnings, Value = seatResponse };
                            }

                            var alacarteOfferCheck = _seatAvailabilityValidations.ValidateAlacarteOfferInResponse(seatResponse);

                            if (alacarteOfferCheck != null || alacarteOfferCheck?.Status == Status.BusinessException || alacarteOfferCheck?.Status == Status.Error)
                            {
                                analyticsEventCommand.Track("ServiceName", "alacarteOfferCheck"); 
                                analyticsEventCommand.Track("ErrorId", alacarteOfferCheck?.ResponseMessages?.Errors?.First().Id);

                                return ErrorDetails(alacarteOfferCheck.ResponseMessages, analyticsEventCommand);
                            }
                        }

                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == Constants.Application.CombinationNotAllowed)
                    {
                        var error = ErrorHelper<IATA_SeatAvailabilityRS>.GetErrors(new[] { new ErrorDetail() { ErrorCode = Constants.Application.NoOffers } }, _hostEnvironment);
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        return ErrorDetails(error.ResponseMessages, analyticsEventCommand);
                    }
                    else
                    {
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        return new Response<IATA_SeatAvailabilityRS>()
                        {
                            Status = Status.BusinessException,
                            Value = MapToErrorSeatAvailabilityRS(DefaultError.DefaultErrorMesaage())
                        };
                    }
                }
                finally
                {
                    stopwatch.Stop();
                    analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                    await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
                }
            }

            // Post Sale Flow
            else
            {
                try
                {
                    analyticsEventCommand.Track("IsPrime", "N");
                    var seatOrderReqValidation = _seatAvailabilityValidations.ValidateSeatOrderRequest(request.SeatAvailabilityRQ);

                    if (seatOrderReqValidation != null || seatOrderReqValidation?.Status == Status.BusinessException || seatOrderReqValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "seatOrderReqValidation");
                        analyticsEventCommand.Track("ErrorId", seatOrderReqValidation?.ResponseMessages?.Errors?.First().Id); 
                        
                        return ErrorDetails(seatOrderReqValidation.ResponseMessages, analyticsEventCommand);
                    }

                    var seatOrderId = (request.SeatAvailabilityRQ.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order;

                    // Check if booking is a GDS booking
                    if (!(seatOrderId.OrderID.StartsWith(Constants.Application.Vs932TicketPrefix)))
                        return await GetSeatMapForGdsBookingAsync(request, analyticsEventCommand, isCreditCardAccepted, _hostEnvironment, cancellationToken);

                    var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = seatOrderId.OrderID }, cancellationToken);

                    seatOrderReqValidation = _seatAvailabilityValidations.ValidateSeatAvailabilitySeatOrderRequest(validationMSResponse, orderDocument);

                    if (seatOrderReqValidation != null || seatOrderReqValidation?.Status == Status.BusinessException || seatOrderReqValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "seatOrderReqValidation");
                        analyticsEventCommand.Track("ErrorId", seatOrderReqValidation?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(seatOrderReqValidation.ResponseMessages, analyticsEventCommand);
                    }
                    
                    var paymentTypeWarning = ValidateSeatAvailability.ValidateFormOfPaymentType(validationMSResponse, isCreditCardAccepted, _hostEnvironment);
                    ndc_Warnings = ndc_Warnings.Concat(paymentTypeWarning ?? Enumerable.Empty<NDCWarning>());

                    if (seatOrderReqValidation != null || seatOrderReqValidation?.Status == Status.BusinessException || seatOrderReqValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "seatOrderReqValidation"); 
                        analyticsEventCommand.Track("ErrorId", seatOrderReqValidation?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                        return ErrorDetails(seatOrderReqValidation.ResponseMessages, analyticsEventCommand);
                    }

                    var getReservationReq = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);

                    var getReservationReqOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                    if (getReservationReqOrderDoc is not null)
                    {
                        if (orderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                        {
                            getReservationReqOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                        }
                        foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                        {
                            getReservationReqOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                        }


                        foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                        {
                            getReservationReqOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                        }

                        if (orderDocument.FareTypeRequest != null) getReservationReqOrderDoc.FareTypeRequest = orderDocument.FareTypeRequest;

                        foreach (var customer in orderDocument.DataList.CustomerDetails)
                        {
                            var getReservationOrderDocCustomer = getReservationReqOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                            if (getReservationOrderDocCustomer != null)
                            {
                                getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                            }
                        }

                        foreach (var orderItem in orderDocument.OrderItems.Where(a => a.OCNTimestamp != null))
                        {
                            getReservationReqOrderDoc.OrderItems.First(x => x.OrderItemId == orderItem.OrderItemId).OCNTimestamp = orderItem.OCNTimestamp;
                        }

                        var seatOrderItems = getReservationReqOrderDoc.OrderItems.SelectMany(x => x.Services);
                        getReservationReqOrderDoc = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderDocument, getReservationReqOrderDoc);
                        IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                        // calling BrandMS
                        if (getReservationReqOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                        {
                            var brandMsReq = BrandMsRqMapper.MapBrandMSPTCOrderRequest(getReservationReqOrderDoc, null);
                            brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);
                        }
                        else
                        {
                            var brandMsReq = BrandMsRqMapper.MapBrandMSOrderRequest(getReservationReqOrderDoc, null);
                            brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                        }

                        getReservationReqOrderDoc = BrandMsRSMapper.MapBrandMSResponseUsingSabreOrderDoc(getReservationReqOrderDoc, brandResponse, orderDocument);

                        if (orderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
                        {
                            OrderDataMsToSaberDocumentMapper.MapOrderItems(orderDocument, getReservationReqOrderDoc);
                        }

                        getReservationReqOrderDoc.Version = orderDocument.Version;
                        getReservationReqOrderDoc.ChangeType = orderDocument.ChangeType;
                        getReservationReqOrderDoc.ChangeDateTime = orderDocument.ChangeDateTime;
                        getReservationReqOrderDoc.BookingReference = orderDocument.BookingReference;
                        // calling publish order

                        bool isCashPayment = orderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                        bool isEasyPayment = orderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationReqOrderDoc };
                        var publishOrderDocResponse = await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    var seatMSResponse = new Infrastructure.Model.SeatMS.SeatRS();

                    //Check eligible segments for seatMap
                    var segmentList = GetEligibleSegmentList(getReservationReqOrderDoc);

                    var eligibilityResult = _seatAvailabilityValidations.ValidateSegmentSeatsWithOrderId(segmentList);

                    if (eligibilityResult != null || eligibilityResult?.Status == Status.BusinessException || eligibilityResult?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        analyticsEventCommand.Track("ServiceName", "eligibilityResult");
                        analyticsEventCommand.Track("ErrorCode", eligibilityResult.ResponseMessages.Errors.First().Code);

                        analyticsEventCommand.Track("ErrorId", eligibilityResult?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(eligibilityResult.ResponseMessages, analyticsEventCommand);
                    }

                    bool vssSegmentCheck = segmentList.Any(seg => seg.OperatingCarrier == null);

                    eligibilityResult = (!vssSegmentCheck) ? _seatAvailabilityValidations.ValidateEligibleVssFlights(segmentList) : null;

                    if (eligibilityResult != null || eligibilityResult?.Status == Status.BusinessException || eligibilityResult?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "eligibilityResult"); 
                        analyticsEventCommand.Track("ErrorId", eligibilityResult?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(eligibilityResult.ResponseMessages, analyticsEventCommand);
                    }

                    segmentList = segmentList.Where(x => (x.MarketingCarrier == Constants.Application.OwnerCode && x.OperatingCarrier is null) || (x.OperatingCarrier == "DL"));

                    eligibilityResult = _seatAvailabilityValidations.ValidateHoldBookingPNR(getReservationReqOrderDoc);

                    if (eligibilityResult != null || eligibilityResult?.Status == Status.BusinessException || eligibilityResult?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "eligibilityResult"); 
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        analyticsEventCommand.Track("ErrorCode", eligibilityResult.ResponseMessages.Errors.First().Code);

                        analyticsEventCommand.Track("ErrorId", eligibilityResult?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(eligibilityResult.ResponseMessages, analyticsEventCommand);
                    }

                    if (segmentList.Any())
                    {
                        var command = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestOrderDoc(request, getReservationReqOrderDoc, isCreditCardAccepted, segmentList);
                        seatMSResponse = await _mediator.Send(command, cancellationToken);
                    }

                    if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap == null)
                    {
                        return new Response<IATA_SeatAvailabilityRS>()
                        {
                            Status = Status.Error,
                            Value = MapToErrorSeatAvailabilityRS(GetTechError())
                        };
                    }

                    var warningList = _seatAvailabilityValidations.PopulateNonVSSegmentWarnings(getReservationReqOrderDoc).Concat(_seatAvailabilityValidations.PopulateVSSegmentWarnings(getReservationReqOrderDoc)).Concat(_seatAvailabilityValidations.PopulateSeatMapResponseWarningsPostSale(seatMSResponse)).Concat(_seatAvailabilityValidations.PopulateWarningsForPaxWithSeat(getReservationReqOrderDoc).Concat(ndc_Warnings)).Concat(_seatAvailabilityValidations.PopulateFOPWarning());

                    var paxList = GetPaxListFromOrderDoc(getReservationReqOrderDoc);

                    OfferResponse offerDoc = new OfferResponse();

                    if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap is not null)
                    {
                        seatResponse = SeatAvailabilityRSMapper.MapSeatAvailabilityResponse(seatMSResponse, paxList, getReservationReqOrderDoc, offerDoc, _hostEnvironment);

                        List<string> seatData = new List<string>();

                        for (int i = 0; i < seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.Count(); i++)
                        {
                            for (int j = 0; j < seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.Count(); j++)
                            {
                                var seatProductName = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).ProductName;
                                var seatPrice = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).SeatPrice.MonetaryAmount;
                                var seatCurrency = seatMSResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.ElementAt(i).Catalogue.ElementAt(j).SeatPrice.BaseCurrency;
                                var key = $"SeatMap{i}:" + "Product Name:" + seatProductName + "| Price:" + seatPrice + " " + seatCurrency;

                                seatData.Add(key);

                            }
                        }

                        var seatInfo = seatData?.GroupBy(seatData => seatData).Select(x => new { SeatKey = x.Key, SeatKeyCount = x.Count() }).ToList();

                    }

                    var paxGroup = getReservationReqOrderDoc.DataList?.CustomerDetails?.GroupBy(x => x.Ptc).Select(x => new { Pax = x.Key, PaxCount = x.Count() }).ToList();

                    for (int i = 1; i <= paxGroup?.Count; i++)
                    {
                        analyticsEventCommand.Track($"PaxType{i}", paxGroup[i - 1].Pax);
                        analyticsEventCommand.Track($"PaxCount{i}", paxGroup[i - 1].PaxCount);
                    }
                    analyticsEventCommand.Track("PosCountry", getReservationReqOrderDoc.PointOfSale?.CountryCode);
                    analyticsEventCommand.Track("PosCurrency", getReservationReqOrderDoc.PointOfSale?.CurrencyCode);
                    analyticsEventCommand.Track("RLOC", getReservationReqOrderDoc.BookingReference?.RLOC);

                    var seatWarnings = ErrorHelper<NDCWarning>.GetWarnings(warningList, _hostEnvironment);

                    if (seatResponse.Items != null)
                    {
                        (seatResponse?.Items as SeatAvailResponseType[]).First().Warning = MapWarnings(seatWarnings).Items as WarningType[];
                    }

                    bool oneWay = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.Count() == 1;
                    bool roundtrip = false;

                    if (getReservationReqOrderDoc.DataList.OriginDestinationAssociations?.Count() == 2)
                    {
                        var outboundOrigin = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.First().OriginDestination.OriginAirport;
                        var outboundDestination = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.First().OriginDestination.DestinationAirport;
                        var inboundOrigin = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.Last().OriginDestination.OriginAirport;
                        var inboundDestination = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.Last().OriginDestination.DestinationAirport;

                        if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                        {
                            roundtrip = true;
                        }
                    }

                    if (getReservationReqOrderDoc.DataList.OriginDestinationAssociations != null)
                    {
                        analyticsEventCommand.Track("Oneway", oneWay ? 'Y' : 'N');
                        analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                        analyticsEventCommand.Track("Multicity", (!oneWay && !roundtrip) ? 'Y' : 'N');
                    }

                    for (int i = 1; i <= getReservationReqOrderDoc.DataList.OriginDestinationAssociations?.Count(); i++)
                    {
                        var ond = getReservationReqOrderDoc.DataList.OriginDestinationAssociations.ElementAt(i - 1);
                        analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                        analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate[..10].Replace('/', '-'));
                    }

                    // var priceClassIds = getReservationReqOrderDoc.OrderItems.SelectMany(x => x.Services.Select(y => y.PriceClassRefIds.FirstOrDefault() && y.PriceClassRefIds.FirstOrDefault() is not null));
                    var priceClassIds = getReservationReqOrderDoc.OrderItems.SelectMany(x => x.Services).SelectMany(s => s.PriceClassRefIds ?? Enumerable.Empty<string>()).Where(id => id != null).ToList();
                    if (priceClassIds != null)
                    {
                        var cabins = (from gpc in priceClassIds
                                      join pc in getReservationReqOrderDoc.DataList?.PriceClass
                                      on gpc equals pc.Id
                                      group gpc by new { pc?.CabinType?.CabinCode, pc?.CabinType?.CabinName } into g
                                      select new { g.Key?.CabinName, Count = g.Count() }).ToList();

                        for (int i = 1; i <= cabins?.Count; i++)
                        {
                            analyticsEventCommand.Track($"Cabin{i}", cabins[i - 1].CabinName);
                        }

                        var fareFamily = (from gpc in priceClassIds
                                          join pc in getReservationReqOrderDoc.DataList?.PriceClass
                                          on gpc equals pc.Id
                                          group gpc by pc.ProductName into g
                                          select new { g.Key, Count = g.Count() }).ToList();

                        for (int i = 1; i <= fareFamily?.Count; i++)
                        {
                            analyticsEventCommand.Track($"FareFamily{i}", fareFamily[i - 1].Key);

                        }

                    }

                    if ((seatResponse?.Items as SeatAvailResponseType[]).FirstOrDefault()?.ALaCarteOffer == null)
                    {
                        if (seatWarnings.Any())
                        {
                            for (int i = 0; i < seatWarnings?.Count(); i++)
                            {
                                if (seatWarnings.ElementAt(i).ShortText != null && seatWarnings.ElementAt(i).Type == "Warning")
                                {
                                    analyticsEventCommand.Track($"warnings{i}", seatWarnings.ElementAt(i).ShortText);
                                }
                            }

                            return new Response<IATA_SeatAvailabilityRS>() { Status = Status.BusinessWarnings, Value = seatResponse };
                        }

                        var alacarteOfferCheck = _seatAvailabilityValidations.ValidateAlacarteOfferInResponse(seatResponse);

                        if (alacarteOfferCheck != null || alacarteOfferCheck?.Status == Status.BusinessException || alacarteOfferCheck?.Status == Status.Error)
                        {
                            analyticsEventCommand.Track("ServiceName", "alacarteOfferCheck"); 
                            analyticsEventCommand.Track("ErrorId", seatValidationResults?.ResponseMessages?.Errors?.First().Id);

                            return ErrorDetails(seatValidationResults.ResponseMessages, analyticsEventCommand);
                        }
                    }

                    analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Seat Availability Query Handler");

                    if (ex.Message == Constants.Application.CombinationNotAllowed)
                    {
                        var error = ErrorHelper<IATA_SeatAvailabilityRS>.GetErrors(new[] { new ErrorDetail() { ErrorCode = Constants.Application.NoOffers } }, _hostEnvironment);
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        return ErrorDetails(error.ResponseMessages, analyticsEventCommand);
                    }
                    else
                    {
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                        return new Response<IATA_SeatAvailabilityRS>()
                        {
                            Status = Status.BusinessException,
                            Value = MapToErrorSeatAvailabilityRS(DefaultError.DefaultErrorMesaage())
                        };
                    }
                }

                finally
                {
                    stopwatch.Stop();
                    analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                    await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
                }

            }

            return new Response<IATA_SeatAvailabilityRS>()
            {
                Status = Status.OK,
                Value = seatResponse
            };
        }

        private IEnumerable<VAA.EAPI.Domain.Virgin.Models.Customer.Customer> GetPaxListOfferDoc(OfferResponse offerDoc)
        {
            return offerDoc.DataList?.CustomerDetails.Select(pax => new VAA.EAPI.Domain.Virgin.Models.Customer.Customer
            {
                CustId = pax.CustId,
                Ptc = pax.Ptc
            });
        }

        private static IATA_SeatAvailabilityRS MapWarnings(IEnumerable<NDCWarning> warnings)
        {
            return new IATA_SeatAvailabilityRS
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

        public static IEnumerable<VAA.EAPI.Domain.Virgin.Models.Customer.Customer> GetPaxListFromOrderDoc(OrderDocument orderDocument)
        {
            var customerDetails = orderDocument?.OrderItems.SelectMany(orderItem => orderItem.Services).Select(service => service.ServiceBeneficiaries).GroupBy(serviceBeneficiary => serviceBeneficiary.First()).SelectMany(cust => cust.First());

            return from custId in customerDetails
                   join customer in orderDocument?.DataList.CustomerDetails on custId equals customer.CustId
                   select new VAA.EAPI.Domain.Virgin.Models.Customer.Customer()
                   {
                       CustId = customer.CustId,
                       Ptc = customer.Ptc == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : customer.Ptc
                   };
        }

        private IEnumerable<Segment> GetEligibleSegmentList(OrderDocument orderDocument)
        {
            IEnumerable<Segment> dlsegmentList = orderDocument?.DataList.SegmentDetails.Select((value, index) => new { index, value }).Select(seg => new Segment
            {
                MarketingCarrier = seg.value.MarketingCarrier,
                MarketingFlightNumber = seg.value.MarketingFlightNumber,
                OperatingCarrier = seg.value.OperatingCarrier,
                OperatingFlightNumber = seg.value.OperatingFlightNumber,
                OriginAirportCode = seg.value.OriginAirportCode,
                DestinationAirportCode = seg.value.DestinationAirportCode,
                ClassOfService = seg.value.ClassOfService,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = DateTime.Parse(seg.value.ScheduledArrivalTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                },
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = DateTime.Parse(seg.value.ScheduledDepartureTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                },
                SegmentNumber = seg.value.SegmentNumber,
                SegmentId = seg.value.SegmentId,
                FlightDuration = seg.value.FlightDuration

            });

            var paxList = GetPaxListFromOrderDoc(orderDocument).Count(x => x.Ptc != Constants.Application.INF);

            if (!orderDocument.OrderItems.Any(oi => oi.Services.Any(s => s.Product != null && s.Product.ProductType == ProductTypes.SeatEnum)))
            {
                return dlsegmentList;
            }

            var eligibleSegments = dlsegmentList.Where(seg => (seg.MarketingCarrier == Constants.Application.OwnerCode && seg.OperatingCarrier == null) || seg.OperatingCarrier == "DL").Where(seg =>
            {
                var seatOrderItems = orderDocument.OrderItems.Where(oi => oi.Services.Any() && oi.Services.First().Product.ProductType == ProductTypes.SeatEnum);

                var paxSeatCount = seatOrderItems.Count(soi => soi.Services.First().SegmentRefIds.First() == seg.SegmentId);

                return paxSeatCount != paxList;

            });

            return eligibleSegments;
        }

        public IEnumerable<FlightSegment> GetFlightSegments(OfferResponse offerDoc)
        {
            return offerDoc.DataList.FlightSegments.Select((value, index) => new { index, value }).Select(seg => new FlightSegment
            {
                OriginAirportCode = seg.value.OriginAirportCode,
                DestinationAirportCode = seg.value.DestinationAirportCode,
                MarketingCarrier = seg.value.MarketingCarrier,
                MarketingFlightNumber = seg.value.MarketingFlightNumber,
                OperatingCarrier = seg.value.OperatingCarrier,
                OperatingFlightNumber = seg.value.OperatingFlightNumber,
                SegmentID = (seg.index + 1).ToString("00"),
                ClassOfService = offerDoc.DataList.OfferSegments.First(oS => oS.FlightSegmentRefId == seg.value.SegmentId).ClassOfService,
                ScheduledArrivalTime = new LocalAndUTCDateTime
                {
                    LocalDateTime = Convert.ToDateTime(seg.value.ScheduledArrivalTime.LocalDateTime),
                },
                ScheduledDepartureTime = new LocalAndUTCDateTime
                {
                    LocalDateTime = Convert.ToDateTime(seg.value.ScheduledDepartureTime.LocalDateTime),
                }

            });
        }
        private Response<IATA_SeatAvailabilityRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);
            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            
            analyticsEventCommand.Track("StatusCode", statusCode);
            
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            return new Response<IATA_SeatAvailabilityRS>()
            {
                Status = resultCode,
                Value = MapToErrorSeatAvailabilityRS(responseMessage)
            };
        }

        private IATA_SeatAvailabilityRS MapToErrorSeatAvailabilityRS(ResponseMessage errors)
        {
            return new IATA_SeatAvailabilityRS
            {
                Items = errors.Errors.Select(error => new ErrorType()
                {
                    Code = error.Code,
                    DescText = error.Message,
                    LangCode = Constants.Application.LangCode,
                    TypeCode = error.StatusText,
                    TagText = error.TagText

                }).ToArray()
            };

        }

        private ResponseMessage GetTechError()
        {
            return new ResponseMessage()
            {
                Errors = new ErrorDetails[]
                                {
                                    new()
                                    {
                                        Code = Constants.Application.TECHERROR,
                                        Message = Constants.Application.TECHERRORMSG,
                                        StatusText = Constants.Application.NOTPROCESSED,
                                        TagText = Constants.Application.TAGTEXT
                                    }
                                }

            };
        }

        private async Task<Result<Response<IATA_SeatAvailabilityRS>>> GetSeatMapForGdsBookingAsync(SeatAvailabilityQuery request, TrackAnalyticsEventCommand analyticsEventCommand, bool isCreditCardAccepted, IHostEnvironment _hostEnvironment, CancellationToken cancellationToken)
        {
            analyticsEventCommand.Track("GDS", "Y");
            var orderSearchRequest = Air4OrderSearchRequestMapper.MapOrderSearchRequest((request.SeatAvailabilityRQ.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order.OrderID);
            var air4OrderSearchMSResponse = await _mediator.Send(new Air4OrderSearchMSQuery() { OrderSearchRequest = orderSearchRequest }, cancellationToken);

            if (air4OrderSearchMSResponse?.OrderSearchResponse is null || air4OrderSearchMSResponse?.ErrorDetails is not null)
            {
                return new Response<IATA_SeatAvailabilityRS>()
                {
                    Status = EAPI.Domain.Entities.Common.Status.BusinessException,
                    Value = MapToErrorSeatAvailabilityRS(DefaultError.DefaultErrorMesaage())
                };
            }

            var validationError = _seatAvailabilityValidations.ValidateOrderSearchRSForGdsAgentBooking(air4OrderSearchMSResponse, request.SeatAvailabilityRQ.DistributionChain.First()?.ParticipatingOrg?.OrgID);

            if (validationError != null || validationError?.Status == EAPI.Domain.Entities.Common.Status.BusinessException || validationError?.Status == EAPI.Domain.Entities.Common.Status.Error)
            {
                analyticsEventCommand.Track("ErrorId", validationError?.ResponseMessages?.Errors?.First().Id);
                return ErrorDetails(validationError.ResponseMessages, analyticsEventCommand);
            }

            var segmentList = GetGDSEligibleSegmentList(air4OrderSearchMSResponse);
            var eligibilityResult = _seatAvailabilityValidations.ValidateGDSSegmentSeats(segmentList);

            if (eligibilityResult != null || eligibilityResult?.Status == EAPI.Domain.Entities.Common.Status.BusinessException || eligibilityResult?.Status == EAPI.Domain.Entities.Common.Status.Error)
            {
                analyticsEventCommand.Track("ErrorId", eligibilityResult?.ResponseMessages?.Errors?.First().Id);
                return ErrorDetails(eligibilityResult.ResponseMessages, analyticsEventCommand);
            }
            var command = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestGDS(request, air4OrderSearchMSResponse, segmentList, isCreditCardAccepted);
            var seatMSResponse = await _mediator.Send(command, cancellationToken);

            if (seatMSResponse == null || seatMSResponse.SeatAvailabilityResponse == null || seatMSResponse.Error != null)
            {
                return new Response<IATA_SeatAvailabilityRS>()
                {
                    Status = EAPI.Domain.Entities.Common.Status.BusinessException,
                    Value = MapToErrorSeatAvailabilityRS(DefaultError.DefaultErrorMesaage())
                };
            }

            var warningList = _seatAvailabilityValidations.PopulateGDSNonVSSegmentWarnings(air4OrderSearchMSResponse.OrderSearchResponse, _hostEnvironment).Concat(_seatAvailabilityValidations.PopulateGDSVSSegmentWarnings(air4OrderSearchMSResponse.OrderSearchResponse, _hostEnvironment)).Concat(_seatAvailabilityValidations.PopulateGDSSeatMapResponseWarnings(seatMSResponse, _hostEnvironment));

            var paxList = GetPaxListFromOrderSearchRS(air4OrderSearchMSResponse);

            var seatResponse = GDSSeatAvailabilityRSMapper.MapSeatAvailaibilityResponse(seatMSResponse.SeatAvailabilityResponse, air4OrderSearchMSResponse.OrderSearchResponse, paxList, warningList, _hostEnvironment);

            return new Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = seatResponse };
        }

        private List<Domain.RequestResponseModels.SeatAvailabilityRS.PaxType> GetPaxListFromOrderSearchRS(Air4OrderSearchMSResponse orderSearchResponse)
        {
            var paxList = new List<Domain.RequestResponseModels.SeatAvailabilityRS.PaxType>();
            int adultCustomerIdCount = 1;
            int youngAdultCustomerIdCount = 1;
            int childCustomerIdCount = 1;
            int infantCustomerIdCount = 1;
            int paxCustomerIdCount = 1;

            foreach (var cust in orderSearchResponse.OrderSearchResponse.CustomerDetails)
            {
                var pax = new Domain.RequestResponseModels.SeatAvailabilityRS.PaxType();

                var pTC = GetPtc(cust, orderSearchResponse.OrderSearchResponse.SegmentDetails.First().ScheduledDepartureTime.LocalDateTime, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount);
                cust.CustIdNdc = pTC.LastOrDefault();
                pax.PaxID = pTC.LastOrDefault();
                pax.PTC = pTC.FirstOrDefault();
                paxList.Add(pax);
            }

            return paxList;
        }

        private static List<string> GetPtc(EAPI.Domain.Virgin.Models.Customer.Customer customer, string departureDate, ref int adultCustomerIdCount, ref int youngAdultCustomerIdCount, ref int childCustomerIdCount, ref int infantCustomerIdCount, ref int paxCustomerIdCount)
        {
            if (!string.IsNullOrEmpty(customer.DateOfBirth))
            {
                string ptc = "";
                DateTime deptDate = Convert.ToDateTime(departureDate);
                DateTime birthDate = Convert.ToDateTime(customer.DateOfBirth);
                var age = (deptDate - birthDate).TotalDays / 365.2425;

                if (age < Constants.Application.InfantMaxAge && age > Constants.Application.InfantMinAge)
                {
                    ptc = Constants.Application.InfantPTCCode;
                }
                else if (age < Constants.Application.ChildMaxAge && age > Constants.Application.ChildMinAge)
                {
                    ptc = Constants.Application.ChildPTCCode;
                }
                else if (age < Constants.Application.YoungMaxAge && age > Constants.Application.YoungMinAge)
                {
                    ptc = Constants.Application.YoungAdultPTCCode;
                }
                else if (age < Constants.Application.AdultMaxAge && age > Constants.Application.AdultMinAge)
                {
                    ptc = Constants.Application.AdultPTCCode;
                }

                return new List<string>() { ptc, GetCustomerID(ptc, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount) };
            }
            else
            {
                return new List<string>() { Constants.Application.PaxPTCCode, GetCustomerID(Constants.Application.PaxPTCCode, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount) };
            }
        }

        private static string GetCustomerID(string pTC, ref int adultCustomerIdCount, ref int youngAdultCustomerIdCount, ref int childCustomerIdCount, ref int infantCustomerIdCount, ref int paxCustomerIdCount)
        {
            string customerID = string.Empty;

            switch (pTC)
            {
                case Constants.Application.AdultPTCCode:
                    customerID = Constants.Application.AdultCustomerID + adultCustomerIdCount;
                    adultCustomerIdCount++;
                    break;

                case Constants.Application.YoungAdultPTCCode:
                    customerID = Constants.Application.YoungAdultCustomerID + youngAdultCustomerIdCount;
                    youngAdultCustomerIdCount++;
                    break;

                case Constants.Application.ChildPTCCode:
                    customerID = Constants.Application.ChildCustomerID + childCustomerIdCount;
                    childCustomerIdCount++;
                    break;

                case Constants.Application.InfantPTCCode:
                    customerID = Constants.Application.InfantCustomerID + infantCustomerIdCount;
                    infantCustomerIdCount++;
                    break;

                case Constants.Application.PaxPTCCode:
                    customerID = Constants.Application.PassengerCustomerID + infantCustomerIdCount;
                    infantCustomerIdCount++;
                    break;
            }

            return customerID;
        }

        private IEnumerable<Segment> GetGDSEligibleSegmentList(Air4OrderSearchMSResponse orderSearchResponse)
        {
            var paxList = orderSearchResponse.OrderSearchResponse.CustomerDetails;

            //check if vs segment has seat assigned for all pax
            var eligibleSegments = orderSearchResponse.OrderSearchResponse.SegmentDetails.Select(seg =>
            {
                if ((seg.OperatingCarrier == null && seg.MarketingCarrier == Constants.Application.OwnerCode) || (seg.OperatingCarrier == "DL"))
                {
                    var seatOrderItems = orderSearchResponse.OrderSearchResponse.OrderItems.Where(oi => oi.Services.First().ProductOrder.ProductType == ProductTypes.SeatEnum);
                    var paxSeatCount = seatOrderItems.Count(sOI => sOI.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == seg.SegmentId);

                    if (paxSeatCount != paxList.Count())
                        return seg;
                    else
                        return null;
                }
                else
                    return null;
            }).Where(x => x != null);

            return eligibleSegments;
        }

    }
}
