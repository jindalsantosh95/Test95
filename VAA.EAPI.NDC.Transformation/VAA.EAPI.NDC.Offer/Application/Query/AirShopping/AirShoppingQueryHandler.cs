using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Interface;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Clid;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Validations.ConfigMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.AirShopping;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.AirShopping;
using ErrorType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS.ErrorType;

namespace VAA.EAPI.NDC.Offer.Application.Query.AirShopping
{
    public class AirShoppingQueryHandler : IRequestHandler<AirShoppingQuery, Result<Response<IATA_AirShoppingRS>>>
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IMediator _mediator;
        private readonly IAirShoppingRequestValidations<IATA_AirShoppingRS> _validations;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly ILogger<AirShoppingQueryHandler> _logger;
        private readonly IitaFareTypeCode _itaFareTypeCode;
        private readonly ITtlRules _ttlRules;
        private readonly ITtlEnabledCity _ttlEnabledCity;
        private readonly ICorporateQuery _corporateQuery;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public AirShoppingQueryHandler(IHostEnvironment hostEnvironment, IMediator mediator, IAirShoppingRequestValidations<IATA_AirShoppingRS> validations, IGenericDataMapper genericDataMapper, IitaFareTypeCode itaFareTypeCode, ILogger<AirShoppingQueryHandler> logger, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ITtlRules ttlRules, ITtlEnabledCity ttlEnabledCity, ICorporateQuery corporateQuery)
        {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validations = validations ?? throw new ArgumentNullException(nameof(validations));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _itaFareTypeCode = itaFareTypeCode ?? throw new ArgumentNullException(nameof(itaFareTypeCode));
            _ttlRules = ttlRules ?? throw new ArgumentNullException(nameof(ttlRules));
            _ttlEnabledCity = ttlEnabledCity ?? throw new ArgumentNullException(nameof(ttlEnabledCity));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _corporateQuery = corporateQuery ?? throw new ArgumentNullException(nameof(corporateQuery));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
           }

        public async Task<Result<Response<IATA_AirShoppingRS>>> Handle(AirShoppingQuery request, CancellationToken cancellationToken)
        {

            var pccCurrency = new PccCurrencyData();
            Stopwatch stopwatch = Stopwatch.StartNew();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.AirshoppingEventSubject
            };

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                var requestedWithCabin = (request.AirShoppingRQ.Request?.FlightRequest?.Item as FlightRequestOriginDestinationsCriteriaType)?.OriginDestCriteria?.FirstOrDefault()?.CabinType?.FirstOrDefault()?.CabinTypeCode;
                analyticsEventCommand.Track("RequestedCabinTypeCode", requestedWithCabin is not null ? requestedWithCabin : "NA");

                var distributionChain = request.AirShoppingRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var validationRS = _validations.ValidateAgentInAirShoppingRequest(request.AirShoppingRQ);

                if (validationRS != null && (validationRS.Status == Status.BusinessException || validationRS.Status == Status.Error))
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationRS");
                    analyticsEventCommand.Track("ErrorId", validationRS.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(validationRS.ResponseMessages, analyticsEventCommand);
                }

                var parties = OfferRequestMapper.GetParties(request.AirShoppingRQ);
                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties);

                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSRS != null && validationMSRS.Error == null)
                {
                    var accountCodeValidate = _validations.ValidateAccountCode(validationMSRS, request.AirShoppingRQ);

                    if (accountCodeValidate != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "AccountCodeValidation");
                        analyticsEventCommand.Track($"ErrorId", accountCodeValidate?.ResponseMessages?.Errors?.First().Id);
                        return ErrorDetails(accountCodeValidate.ResponseMessages, analyticsEventCommand);

                    }
                }

                if (validationMSRS?.Error != null)
                {
                    var validation = _validations.ValidateAgency(validationMSRS);
                    analyticsEventCommand.Track("ServiceName", "ValidationMSRS");
                    analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
                }

                analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse.AgencyDetails.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse?.AgencyDetails.RequestOrgName);

                if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }
                if (!string.IsNullOrWhiteSpace(validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId))
                {
                    analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails.CorporateId);
                }
                var errors = _validations.ValidateAirShoppingRequest(request.AirShoppingRQ);

                if (errors?.Status == Status.BusinessException || errors?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateAirShoppingRequest");
                    analyticsEventCommand.Track($"ErrorId", errors?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(errors.ResponseMessages, analyticsEventCommand);
                }
                else
                {
                    CorporateData configMSRS = null;
                    
                    var programOwnerData = request.AirShoppingRQ?.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner?.Item;
                    var org = programOwnerData as VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ.OrgType;
                    string cbaClid = org?.OrgID;

                    if (!string.IsNullOrWhiteSpace(cbaClid))
                    {
                        configMSRS = await _corporateQuery.Handle(cbaClid, cancellationToken);

                        if (configMSRS != null)
                        {
                            var configMSValidationError = ConfigMSResponseValidation.ValidateResponse<IATA_AirShoppingRS>(configMSRS, _hostEnvironment);
                            if (configMSValidationError is not null)
                            {
                                analyticsEventCommand.Track("ServiceName", "ConfigMS");
                                analyticsEventCommand.Track("ErrorId", configMSValidationError.ResponseMessages?.Errors?.First().Id);
                                return ErrorDetails(configMSValidationError.ResponseMessages, analyticsEventCommand);
                            }
                        }
                    }
                    
                    var countryCode = validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode;
                    var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

                    if (currencyData != null)
                    {
                        string currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                        pccCurrency = await _mediator.Send(new BspMarketPCCCurrencyQuery(currencyCode), cancellationToken);
                    }

                    if (currencyData is null)
                    {
                        analyticsEventCommand.Track("ServiceName", "CurrencyData");
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);

                        return new Response<IATA_AirShoppingRS>()
                        {
                            Status = Status.BusinessException,
                            Value = MapToErrorAirShoppingRS(DefaultError.DefaultErrorMesaage())
                        };
                    }

                    string fareTypeCode = request.AirShoppingRQ.Request.OfferCriteria?.FareCriteria?.FirstOrDefault()?.FareTypeCode?.FirstOrDefault();
                    string fareCodeSubType = request.AirShoppingRQ.Request.OfferCriteria?.FareCriteria?.FirstOrDefault()?.FareSubTypeCode?.FirstOrDefault();

                    var itaPaxTypeCode = await _itaFareTypeCode.Handle(fareTypeCode, fareCodeSubType, cancellationToken);
                    var matchedPTCCode = itaPaxTypeCode?.Where(x => x.FareTypeCode == fareTypeCode && x.FareSubTypeCode == fareCodeSubType?.Replace(" ", "").ToUpper());

                    if (itaPaxTypeCode is null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ItaPaxTypeCode");
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);

                        return new Response<IATA_AirShoppingRS>()
                        {
                            Status = Status.BusinessException,
                            Value = MapToErrorAirShoppingRS(DefaultError.DefaultErrorMesaage())
                        };
                    }

                    if(fareTypeCode == Constants.Application.PrivateFareTypeCode || fareTypeCode == Constants.Application.CorporateFareTypeCode)
                    {
                        var paxFareValidationErrors = _validations.ValidatePaxBasedOnFareType(request.AirShoppingRQ.Request.PaxList, matchedPTCCode);

                        if (paxFareValidationErrors != null)
                        {
                            analyticsEventCommand.Track("ServiceName", "paxFareValidation");
                            analyticsEventCommand.Track($"ErrorId", paxFareValidationErrors?.ResponseMessages?.Errors?.First().Id);
                            return ErrorDetails(paxFareValidationErrors.ResponseMessages, analyticsEventCommand);
                        }
                    }

                    var fareFamilies = await _mediator.Send(new FareFamilyQuery(), cancellationToken);

                    if (fareFamilies is null)
                    {
                        analyticsEventCommand.Track("ServiceName", "FareFamilies");
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);

                        return new Response<IATA_AirShoppingRS>()
                        {
                            Status = Status.BusinessException,
                            Value = MapToErrorAirShoppingRS(DefaultError.DefaultErrorMesaage())
                        };
                    }

                    bool calendarSearch;
                    CalendarSearchResponse calendarSearchResponse = new();
                    OfferResponse offers = new();

                    var originAirportCode = (request.AirShoppingRQ?.Request?.FlightRequest?.Item as FlightRequestOriginDestinationsCriteriaType)?.OriginDestCriteria?.First()?.OriginDepCriteria?.IATA_LocationCode;
                    bool isTtlEnabledAirport = false;

                    if (!string.IsNullOrWhiteSpace(originAirportCode))
                    {
                        var airportCodeDetails = await _ttlEnabledCity.Handle(originAirportCode, cancellationToken);
                        isTtlEnabledAirport = airportCodeDetails?.Any() == true;
                    }

                    var ttlRules = isTtlEnabledAirport ? await _ttlRules.Handle(fareTypeCode, fareCodeSubType, cancellationToken) : null;

                    var command = AirShoppingRQMapper.MapOfferRequest(request, currencyData, pccCurrency, fareFamilies, itaPaxTypeCode, validationMSRS?.ValidateResponse, fareTypeCode, fareCodeSubType, ttlRules, configMSRS);

                    analyticsEventCommand.Track("PosCountry", command.Pos.CountryCode);
                    analyticsEventCommand.Track("CabinCodeFilter", command.FlightsearchRequest.CabinTypes != null ? "Y" : "N");
                    analyticsEventCommand.Track("CalendarSearch", command.FlightsearchRequest.CalendarSearch ? 'Y' : 'N');

                    var searchOriginDestinations = command.FlightsearchRequest.SearchOriginDestinations.ToArray();
                    bool oneway = searchOriginDestinations.Length == 1;
                    bool roundtrip = false;

                    if (searchOriginDestinations.Length == 2)
                    {
                        var outbound = searchOriginDestinations[0];
                        var inbound = searchOriginDestinations[1];

                        if (outbound.Origin == inbound.Destination && outbound.Destination == inbound.Origin)
                        {
                            roundtrip = true;
                        }
                    }
                    analyticsEventCommand.Track("Oneway", oneway ? 'Y' : 'N');
                    analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                    analyticsEventCommand.Track("Multicity", (!oneway && !roundtrip) ? 'Y' : 'N');

                    for (int i = 0; i < searchOriginDestinations.Length; i++)
                    {
                        var ond = searchOriginDestinations[i];
                        analyticsEventCommand.Track($"OriginDestination{i + 1}", ond.Origin + ond.Destination);
                        analyticsEventCommand.Track($"DepartureDate{i + 1}", ond.DepartureDate);
                    }

                    calendarSearch = command.FlightsearchRequest.CalendarSearch;

                    var parallelTasks = new[] { "SpecificDateSearch", "CalendarSearch" };

                    var tasks = parallelTasks.Select(async fs =>
                    {
                        if (fs == "SpecificDateSearch")
                        {
                            offers = await _mediator.Send(command, cancellationToken);
                        }
                        else if (fs == "CalendarSearch" && calendarSearch)
                        {
                            var calendarRequest = AirShoppingRQMapper.MapCalendarOfferRequest(request, currencyData, pccCurrency, fareFamilies, itaPaxTypeCode, validationMSRS?.ValidateResponse, request.AirShoppingRQ?.Request?.ResponseParameters?.CurParameter?[0]?.CurCode, fareTypeCode, configMSRS);
                            calendarSearchResponse = await _mediator.Send(calendarRequest, cancellationToken);
                        }
                    });

                    await Task.WhenAll(tasks);

                    var responseErrors = AirShoppingResponseValidations.ValidateMicroServiceResponse<ErrorType>(offers, _hostEnvironment);

                    if (responseErrors?.Status != Status.BusinessException && errors?.Status != Status.Error)
                    {
                        var warnings = Enumerable.Empty<NDCWarning>();
                        var calendarSearchWarning = Enumerable.Empty<NDCWarning>();

                        if (calendarSearch)
                        {
                            calendarSearchWarning = AirShoppingResponseValidations.ValidateCalendarSearchResponse(calendarSearchResponse, _hostEnvironment);
                            if (calendarSearchWarning != null)
                                warnings = warnings.Concat(calendarSearchWarning);
                        }

                        if (!string.IsNullOrEmpty(request.AirShoppingRQ.Request.ResponseParameters?.CurParameter?[0]?.CurCode))
                        {
                            var currencyWarning = _validations.ValidateCurrency(request.AirShoppingRQ, offers, currencyData, validationMSRS, calendarSearchResponse);
                            if (currencyWarning != null)
                                warnings = warnings.Concat(currencyWarning);
                        }

                        var penaltyWarnings = offers.DataList.Penalty is not null ? AirShoppingResponseValidations.ValidatePenaltyCurrencyMismatch(offers, _hostEnvironment) : null;

                        if (!penaltyWarnings.IsNullOrEmpty())
                        {
                            warnings = warnings.Concat(penaltyWarnings);
                        }

                        analyticsEventCommand.Track("OfferID", offers.Offers.First().OfferId.Split('|')[0]);

                        var flights = offers.DataList.Journey.Select(journey =>
                        {
                            var segments = offers.DataList.FlightSegments.Where(fs => journey.FlightSegmentRefIds.Contains(fs.SegmentId));
                            return new { StopCounts = segments.Select(s => s.Legs.Count() - 1) };
                        });

                        var response = (calendarSearch && calendarSearchWarning.IsNullOrEmpty()) ? AirshoppingResponseMapper.MapOfferResponse(offers, _genericDataMapper, calendarSearchResponse, warnings, fareCodeSubType, fareTypeCode) : AirshoppingResponseMapper.MapOfferResponse(offers, _genericDataMapper, null, warnings, fareCodeSubType, fareTypeCode);
                        analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);
                        return new Response<IATA_AirShoppingRS>() { Status = Status.OK, Value = response };
                    }
                    else
                    {
                        var trackedErrorId = responseErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id?? offers?.Error?.FirstOrDefault()?.Id;
                        analyticsEventCommand.Track("ErrorId", trackedErrorId);
                        analyticsEventCommand.Track("ServiceName", "OfferMS");
                        return ErrorDetails(responseErrors?.ResponseMessages, analyticsEventCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in AirShopping Query Handler");

                if (ex.Message == Constants.Application.CombinationNotAllowed)
                {
                    var error = ErrorHelper<IATA_AirShoppingRS>.GetErrors([new ErrorDetail() { ErrorCode = Constants.Application.NoOffers }], _hostEnvironment);
                    return ErrorDetails(error.ResponseMessages, analyticsEventCommand);
                }
                else
                {
                    analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                    return new Response<IATA_AirShoppingRS>()
                    {
                        Status = Status.Error,
                        Value = MapToErrorAirShoppingRS(DefaultError.DefaultErrorMesaage())
                    };
                }
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                _ = _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }
        private Response<IATA_AirShoppingRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);
            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            analyticsEventCommand.Track("StatusCode", statusCode);
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            return new Response<IATA_AirShoppingRS>()
            {
                Status = resultCode,
                Value = MapToErrorAirShoppingRS(responseMessage)
            };
        }

        private IATA_AirShoppingRS MapToErrorAirShoppingRS(ResponseMessage errors)
        {
            return new IATA_AirShoppingRS
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
    }
}
