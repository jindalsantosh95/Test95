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
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderReshop
{
    public class OrderReshopQueryHandler : IRequestHandler<OrderReshopQuery, Result<Response<IATA_OrderReshopRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopQueryHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private const string McoAirportCode = "MCO";
        private const string OrlCityCode = "ORL";

        public OrderReshopQueryHandler(IMediator mediator, ILogger<OrderReshopQueryHandler> logger, IHostEnvironment hostEnvironment, IGenericDataMapper genericDataMapper, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));

        }

        public async Task<Result<Response<IATA_OrderReshopRS>>> Handle(OrderReshopQuery request, CancellationToken cancellationToken)
        {
            var nDCWarnings = new List<NDCWarning>();

            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderReshopEventSubject
            };

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("ReshopRequest", "Y");

                var distributionChain = request.OrderReshopRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var errorsValidations = OrderReshopRequestValidations.ValidateOrderReshopRequest(request.OrderReshopRQ, _hostEnvironment);

                if (errorsValidations?.Status == Status.BusinessException || errorsValidations?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOrderReshopRequest"); 
                    analyticsEventCommand.Track($"ErrorId", errorsValidations?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(errorsValidations.ResponseMessages, analyticsEventCommand);
                }

                var orderReshopRQ = OrderReshopRequestMapper.MapOrderReshopRq(request.OrderReshopRQ);

                analyticsEventCommand.Track("OrderID", orderReshopRQ.OrderId);
                analyticsEventCommand.Track("AgencyType", orderReshopRQ.Parties?.Participants.First().PartyType);
                analyticsEventCommand.Track("AgencyName", orderReshopRQ.Parties?.Participants.First().Name);

                if (orderReshopRQ.ReshopQueries?.FlightSpecific?.SearchCriteria?.CabinTypes?.Count() == 1)
                {
                    analyticsEventCommand.Track("CabinType", orderReshopRQ.ReshopQueries.FlightSpecific.SearchCriteria?.CabinTypes.First().CabinCode);
                }

                if (orderReshopRQ.ReshopQueries?.FlightSpecific?.SearchCriteria?.CabinTypes?.Count() > 1)
                {
                    for (int i = 1; i <= orderReshopRQ.OriginDestinationAssociations?.Count(); i++)
                    {
                        analyticsEventCommand.Track($"CabinType{i}", orderReshopRQ.ReshopQueries.FlightSpecific.SearchCriteria?.CabinTypes.ElementAt(i - 1).CabinCode);
                    }
                }

                for (int i = 1; i <= orderReshopRQ.OriginDestinationAssociations?.Count(); i++)
                {
                    var ond = orderReshopRQ.OriginDestinationAssociations.ElementAt(i - 1);
                    analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                    analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate.Substring(0, 10).Replace('/', '-'));
                }

                bool oneway = orderReshopRQ.OriginDestinationAssociations?.Count() == 1;
                bool roundtrip = false;
                if (orderReshopRQ.OriginDestinationAssociations?.Count() == 2)
                {
                    var outboundOrigin = orderReshopRQ.OriginDestinationAssociations.First().OriginDestination.OriginAirport;
                    var outboundDestination = orderReshopRQ.OriginDestinationAssociations.First().OriginDestination.DestinationAirport;
                    var inboundOrigin = orderReshopRQ.OriginDestinationAssociations.Last().OriginDestination.OriginAirport;
                    var inboundDestination = orderReshopRQ.OriginDestinationAssociations.Last().OriginDestination.DestinationAirport;

                    if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                    {
                        roundtrip = true;
                    }
                }

                if (orderReshopRQ.OriginDestinationAssociations != null)
                {
                    analyticsEventCommand.Track("Oneway", oneway ? 'Y' : 'N');
                    analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                    analyticsEventCommand.Track("Multicity", (!oneway && !roundtrip) ? 'Y' : 'N');
                }

                var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = orderReshopRQ.OrderId }, cancellationToken);

                // Validate Seller Follow Action for Disruption: Reshop Scenarios
                var disruptionValidation = OrderReshopRequestValidations.ValidateSellerFollowUpAction<IATA_OrderReshopRS>(orderDocument, _hostEnvironment);

                if (disruptionValidation?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "SellerFollowUpActionValidation");
                    analyticsEventCommand.Track("ErrorId", disruptionValidation.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    return ErrorDetails(disruptionValidation?.ResponseMessages, analyticsEventCommand);
                }
                bool isReshopDisruption = orderDocument?.OrderItems?.Any(oi => oi?.SellerFollowUpAction?.Any(x => x.Is("Reshop")) == true) == true && orderDocument?.DataList?.SegmentDetails?.Any(x => x.ScheduleChangeIndicator) == true;
                bool isHoldBookingReshop = orderDocument?.TimeLimits is not null && orderDocument.OrderItems?.Any(x => x.AccountableDocumentRefs is null) == true;
                
                //Validate the agency details
                var errorsInAgencyValidation = OrderReshopRequestValidations.ValidateAgentInRequest(orderDocument, request.OrderReshopRQ.DistributionChain, _hostEnvironment);

                if (errorsInAgencyValidation?.Status == Status.BusinessException || errorsInAgencyValidation?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateAgentInRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsInAgencyValidation?.ResponseMessages?.Errors?.First().Id); 
                    return ErrorDetails(errorsInAgencyValidation.ResponseMessages, analyticsEventCommand);
                }

                var validationRS = OrderReshopRequestValidations.ValidateOrderReshopVaRequest<ErrorType>(orderReshopRQ, _hostEnvironment);
               
                if (validationRS != null && (validationRS.Status == Status.BusinessException || validationRS.Status == Status.Error))
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationRS");
                    analyticsEventCommand.Track($"ErrorId", validationRS?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(validationRS.ResponseMessages, analyticsEventCommand);
                }

                analyticsEventCommand.Track($"VS_PNR", orderDocument?.BookingReference?.RLOC);
                analyticsEventCommand.Track($"Delta_PNR", orderDocument?.BookingReference?.ExternalRecordLocator);

                if (orderDocument is null)
                {
                    var response = OrderReshopRequestValidations.ValidateOrderDocument<ErrorDetails>(orderReshopRQ, orderDocument, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidateOrderDocument"); 
                    analyticsEventCommand.Track($"ErrorId", response?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(response.ResponseMessages, analyticsEventCommand);
                }

                var parties = OrderReshopRequestMapper.GetParties(request.OrderReshopRQ);
                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties);
                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSRS?.Error != null)
                {
                    var validation = OrderReshopRequestValidations.ValidateAgency<ErrorType>(validationMSRS, _hostEnvironment);
                    analyticsEventCommand.Track("ServiceName", "ValidationMSRS");
                    analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
                }
                else
                {
                    analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                    analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestOrgName);

                    if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                    {
                        analyticsEventCommand.Track("AggregatorName", validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName);
                        analyticsEventCommand.Track("AggregatorType", validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                    }
                   
                    analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId);
                    analyticsEventCommand.Track("PosCountry", validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode);

                }

                var priceClassCabin = orderDocument.DataList.PriceClass.First();
                var reshopFareFamilies = await _mediator.Send(new ReshopFareFamilyQuery(), cancellationToken);
                if (isReshopDisruption)
                {
                    var matchingFareFamily = reshopFareFamilies.FirstOrDefault(x =>
                    x.CabinCode == priceClassCabin.CabinType.CabinCode &&
                    x.ExistingFareFamily == priceClassCabin.ProductName);

                    if (matchingFareFamily != null)
                    {
                        reshopFareFamilies =
                        [
                            new ReshopFareFamily
                        {
                            CabinCode = matchingFareFamily.CabinCode,
                            ExistingFareFamily = matchingFareFamily.ExistingFareFamily,
                            ItaFareFamilies = [matchingFareFamily.ItaFareFamilies.First()]
                        }
                        ];
                    }
                }

                var ticketValidityWindowInDays = isReshopDisruption ? ConfigReader.Get<int>(Constants.Environment.ReshopDisruptionTicketValidityWindowInDays) : 0;

                var validationDoc = OrderReshopRequestValidations.ValidateOrderReshopRequestAgainstDocument<ErrorType>(isHoldBookingReshop, orderReshopRQ, orderDocument, reshopFareFamilies, validationMSRS?.ValidateResponse, _hostEnvironment, isReshopDisruption: isReshopDisruption, ticketValidityWindowInDays: ticketValidityWindowInDays);

                var svcList = ServiceListMapper.ServiceListGeneration(orderDocument, orderReshopRQ);

                if (validationDoc != null && (validationDoc.Status == Status.BusinessException || validationDoc.Status == Status.Error))
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationDoc");
                    analyticsEventCommand.Track($"ErrorId", validationDoc?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(validationDoc.ResponseMessages, analyticsEventCommand);
                }

                var getReservationReq = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);
                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                var offers = new OfferResponse();

                if (getReservationOrderDoc is not null)
                {
                    getReservationOrderDoc.PointOfSale = orderDocument.PointOfSale;
                    var seatDeleteWarning = OrderReshopRequestValidations.ValidateOrderReshopSeatDeleteWarnings(getReservationOrderDoc, orderReshopRQ, _hostEnvironment);
                    if (seatDeleteWarning.Any())
                    {
                        foreach (var item in seatDeleteWarning)
                        {
                            if (!nDCWarnings.Contains(item) && !nDCWarnings.Any(x => x.Code == "214"))
                            {
                                nDCWarnings.Add(item);
                                foreach (var (warning, index) in seatDeleteWarning.Select((warning, index) => (warning, index)))
                                {
                                    int i = index;
                                    i++;
                                    analyticsEventCommand.Track($"WarningCode{i}", warning.Code);
                                    analyticsEventCommand.Track($"WarningMessage{i}", warning.TagText);
                                }
                            }
                        }
                    }

                    foreach (var customer in orderDocument.DataList.CustomerDetails)
                    {
                        var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (getReservationOrderDocCustomer != null)
                        {
                            getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                        }
                    }

                    var segmentIds =  orderDocument.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);
                    foreach (var segment in getReservationOrderDoc.DataList.SegmentDetails.Where(s => segmentIds.Contains(s.SegmentId)))
                    {
                        segment.Status.DeliveryStatusCode = "Delivered";
                    }

                    if (orderDocument.FareTypeRequest != null)
                    {
                        getReservationOrderDoc.FareTypeRequest = orderDocument.FareTypeRequest;
                    }
                    IEnumerable<BrandRS> brandResponse;

                    if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                    {
                        var brandMsReq = BrandMsRqMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc, orderDocument);
                        brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);
                    }
                    else 
                    {
                        var brandMsReq = BrandMsRqMapper.MapBrandMSOrderRequest(getReservationOrderDoc,orderDocument);
                        brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                    }
                    
                    getReservationOrderDoc = BrandMsRSMapper.MapBrandMSResponseUsingSabreOrderDoc(getReservationOrderDoc, brandResponse, orderDocument);

                    var airportCodes = string.Join(",", getReservationOrderDoc.DataList.OriginDestinationAssociations.SelectMany(x => new[] { x.OriginDestination.OriginAirport, x.OriginDestination.DestinationAirport }).Distinct());
                    var airportDetails = await _mediator.Send(new AirportDetailsQuery(airportCodes), cancellationToken);
                    UpdateOrlandoCityCodeToMco(airportDetails);
                    var ValidationSabreReq = isHoldBookingReshop is true ? null : OrderReshopRequestValidations.ValidateSabreOrderDocSegmentCouponStatus<ErrorDetails>(getReservationOrderDoc, _hostEnvironment);

                    if (ValidationSabreReq != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ValidationSabreReq");
                        analyticsEventCommand.Track($"ErrorId", ValidationSabreReq?.ResponseMessages?.Errors?.First().Id);

                        return ErrorDetails(ValidationSabreReq.ResponseMessages, analyticsEventCommand);
                    }

                    getReservationOrderDoc = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderDocument, getReservationOrderDoc);

                    var query = OrderReshopRequestMapper.MapOrderReshopRq(getReservationOrderDoc, orderReshopRQ, reshopFareFamilies, airportDetails, svcList, validationMSRS.ValidateResponse,orderDocument , isHoldBookingReshop, isReshopDisruption);

                    offers = await _mediator.Send(query, cancellationToken);
                }

                if (offers.Error?.Any() != true && offers?.Offers != null)
                {
                    var paxGroup = offers.DataList?.CustomerDetails?.GroupBy(x => x.Ptc).Select(x => new { Pax = x.Key, PaxCount = x.Count() });

                    for (int i = 1; i <= paxGroup?.Count(); i++)
                    {
                        analyticsEventCommand.Track($"PaxType{i}", paxGroup.ElementAt(i - 1).Pax);
                        analyticsEventCommand.Track($"PaxCount{i}", paxGroup.ElementAt(i - 1).PaxCount);
                    }
                    analyticsEventCommand.Track("PosCity", offers.Pos?.CityCode);

                    var flights = offers.DataList?.Journey?.Select(journey =>
                    {
                        var segments = offers.DataList.FlightSegments.Where(fs => journey.FlightSegmentRefIds.Contains(fs.SegmentId));

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
                    analyticsEventCommand.Track("IATAOfferID", offers.Offers.ElementAt(0)?.OfferId.Split('|')[0]);
                    analyticsEventCommand.Track("OffersCount", offers.Offers?.Count());

                    var priceClassIds = offers.Offers?.SelectMany(x => x.OfferItems).SelectMany(x => x.Services).Select(x => x.PriceClassRefIds.FirstOrDefault());

                    if (priceClassIds != null)
                    {
                        var cabins = (from gpc in priceClassIds
                                      join pc in offers.DataList?.PriceClass
                                      on gpc equals pc.Id
                                      group gpc by new { pc.CabinType.CabinCode, pc.CabinType.CabinName } into g
                                      select new { g.Key.CabinName, Count = g.Count() }).ToList();

                        for (int i = 1; i <= cabins?.Count; i++)
                        {
                            analyticsEventCommand.Track($"Cabin{i}", cabins[i - 1].CabinName);
                            analyticsEventCommand.Track($"CabinCount{i}", cabins[i - 1].Count);
                        }

                        var farefamily = (from gpc in priceClassIds
                                          join pc in offers.DataList?.PriceClass
                                          on gpc equals pc.Id
                                          group gpc by pc.ProductName into g
                                          select new { g.Key, Count = g.Count() }).ToList();

                        for (int i = 1; i <= farefamily?.Count; i++)
                        {
                            analyticsEventCommand.Track($"FareFamily{i}", farefamily[i - 1].Key);
                            analyticsEventCommand.Track($"FareFamilyCount{i}", farefamily[i - 1].Count);
                        }
                    }

                    var reshopRS = OrderReshopResponseMapper.MapReshopResponse(offers, _genericDataMapper, nDCWarnings, svcList, orderDocument, isReshopDisruption);

                    analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                    DateTime? dateOfResponse = DateTime.UtcNow;
                    analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

                    return new Response<IATA_OrderReshopRS>() { Status = Status.OK, Value = reshopRS };
                }
                else
                {
                    var responseMessage = OrderReshopResponseValidations.ValidateReshopOfferMS<ErrorType>(offers, _hostEnvironment);

                     analyticsEventCommand.Track($"ErrorId", responseMessage?.ResponseMessages?.Errors?.First().Id);
                     return ErrorDetails(responseMessage.ResponseMessages, analyticsEventCommand);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Reshop Query Handler");

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                return new Response<IATA_OrderReshopRS>()
                {
                    Status = Status.BusinessException,
                    Value = MapToErrorOrderReshopRS(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

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
                Value = MapToErrorOrderReshopRS(responseMessage)
            };
        }

        private IATA_OrderReshopRS MapToErrorOrderReshopRS(ResponseMessage errors)
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
                           TagText = error.TagText,
                           URL = error.Url
                       }).ToArray()
            };
        }
    private static void UpdateOrlandoCityCodeToMco(IEnumerable<AirportDetail> airportDetails)
        {
            if (airportDetails is null) return;

            foreach (var airport in airportDetails)
            {
                if (airport.AirportCode == McoAirportCode && airport.CityCode == OrlCityCode)
                {
                    airport.CityCode = McoAirportCode;
                }
            }
        }
    }
}
