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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OfferPrice;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;

namespace VAA.EAPI.NDC.Offer.Application.Query.OfferPrice
{
    public class OfferPriceQueryHandler : IRequestHandler<OfferPriceQuery, Result<Response<IATA_OfferPriceRS>>>
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IMediator _mediator;
        private readonly IOfferPriceValidations<IATA_OfferPriceRS> _validations;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly ILogger<OfferPriceQueryHandler> _logger;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OfferPriceQueryHandler(IHostEnvironment hostEnvironment, IMediator mediator, ILogger<OfferPriceQueryHandler> logger, IOfferPriceValidations<IATA_OfferPriceRS> validations, IGenericDataMapper genericDataMapper, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validations = validations ?? throw new ArgumentNullException(nameof(validations));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Response<IATA_OfferPriceRS>>> Handle(OfferPriceQuery request, CancellationToken cancellationToken)
        {
            bool isCreditCardAccepted = false;
            var warnings = Enumerable.Empty<NDCWarning>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OfferPriceEventSubject
            };

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);
                var distributionChain = request.OfferPriceRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var errorsInValidation = _validations.ValidateOfferPriceVaRequest(request.OfferPriceRQ, _hostEnvironment);

                if (errorsInValidation?.Status != Status.BusinessException && errorsInValidation?.Status != Status.Error)
                {
                    var pricedOffer = (SelectedOfferListType)request.OfferPriceRQ?.Request?.PricedOffer?.Item;

                    var offerDocument = await _mediator.Send(new OfferMSSearchQuery(pricedOffer?.SelectedOffer?[0].OfferRefID), cancellationToken);

                    var dbErrors = _validations.ValidateDbOffer(offerDocument, pricedOffer?.SelectedOffer?[0].OfferRefID, _hostEnvironment);

                    if (dbErrors?.Status != Status.BusinessException && dbErrors?.Status != Status.Error)
                    {
                        var errorsInAgencyValidation = _validations.ValidateAgentInRequest(offerDocument, request.OfferPriceRQ.DistributionChain, _hostEnvironment);

                        if (errorsInAgencyValidation?.Status == Status.BusinessException || errorsInAgencyValidation?.Status == Status.Error)
                        {
                            analyticsEventCommand.Track("ServiceName", "ErrorsInAgencyValidation");
                            analyticsEventCommand.Track($"ErrorId", errorsInAgencyValidation?.ResponseMessages?.Errors?.First().Id);
                            return ErrorDetails(errorsInAgencyValidation.ResponseMessages, analyticsEventCommand);
                        }

                        var parties = request.OfferPriceRQ.DistributionChain.Length > 2 ? OfferPriceRQMapper.GetParties(request.OfferPriceRQ.DistributionChain, offerDocument.Parties.CorporateAccount, isCreditCardAccepted, null) : offerDocument?.Parties;

                        var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties, true);

                        var validationMSResponse = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                        if (validationMSResponse?.Error != null)
                        {
                            var validation = _validations.ValidateAgency(validationMSResponse, _hostEnvironment);
                            analyticsEventCommand.Track("ServiceName", "ValidationMSResponse");
                            analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);
                            return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
                        }

                        analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails.RequestAgencyIdType);
                        analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails.RequestOrgName);
                        
                        if (validationMSResponse?.ValidateResponse?.AggregatorDetails is not null)
                        {
                            analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                            analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                        }
                        if (!string.IsNullOrWhiteSpace(validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId))
                        {
                            analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails.CorporateId);
                        }
                        var agentErrors = _validations.ValidateAgentAndPax(offerDocument, request.OfferPriceRQ, _hostEnvironment, validationMSResponse, _genericDataMapper);

                        if (agentErrors?.Status != Status.BusinessException && agentErrors?.Status != Status.Error)
                        {
                            bool isNonIataAgent = validationMSResponse!.ValidateResponse.AgencyDetails.RequestAgencyIdType == Constants.Application.TIDS;

                            var agencyPermission = validationMSResponse.ValidateResponse.AgencyPermissions;
                            
                            bool isCashPaymentAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.CashFopAllowedInd == true;

                            bool isEasyFopAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.EasyPayFopAllowedInd == true;
                            
                            bool isCardPaymentAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.CardFopAllowedInd == true;
                           
                            var upsellOffersConfigResponse = await _mediator.Send(new UpsellOffersDBQuery(), default);

                            var upsellConfigErrors = _validations.ValidateUpsellConfigResponse(upsellOffersConfigResponse, _hostEnvironment);

                            if (upsellConfigErrors?.Status == Status.BusinessException || upsellConfigErrors?.Status == Status.Error)
                            {
                                analyticsEventCommand.Track("ServiceName", "ValidateUpsellConfigResponse");
                                analyticsEventCommand.Track($"ErrorId", upsellConfigErrors?.ResponseMessages?.Errors?.First().Id);
                                return ErrorDetails(upsellConfigErrors.ResponseMessages, analyticsEventCommand);
                            }

                            var countryCode = validationMSResponse?.ValidateResponse?.AgencyDetails?.CountryCode;
                            var currencyData = await _mediator.Send(new BspMarketCurrencyQuery(countryCode), cancellationToken);

                            BspMarketPaymentData bspMarketPaymentData = new BspMarketPaymentData();

                            if (currencyData != null)
                            {
                                var currencyCode = string.IsNullOrEmpty(currencyData.LocalCurrencyCode) ? currencyData.DualCurrencyCode : currencyData.LocalCurrencyCode;
                                bspMarketPaymentData = await _mediator.Send(new BspMarketPaymentQuery(countryCode, currencyCode), cancellationToken);
                                isCreditCardAccepted = bspMarketPaymentData.IsCreditCardAccepted;
                            }

                            var paymentTypeWarning = ValidateOfferPrice.ValidateFormOfPaymentType(validationMSResponse, isCreditCardAccepted, _hostEnvironment);
                            warnings = paymentTypeWarning ?? warnings.Concat(paymentTypeWarning);

                            var accountCode = offerDocument.Parties.Participants.First(x => x.PartyType.Value == Participant.PartyTypeEnum.Seller).AccountCode;
                            var command = OfferPriceRQMapper.MapOfferPriceRequest(request, pricedOffer, isCreditCardAccepted, _genericDataMapper, offerDocument, upsellOffersConfigResponse, accountCode);
                            
                            analyticsEventCommand.Track("AccountCode", accountCode);
                            analyticsEventCommand.Track("IATAOfferID", command.PricedOffer.SelectedOffer.OfferRefID);
                            analyticsEventCommand.Track("PosCountry", command.Pos.CountryCode);

                            var offerResponse = await _mediator.Send(command, cancellationToken);

                            var responseErrors = _validations.ValidateOfferPriceResponse(offerResponse, _hostEnvironment);

                            if (responseErrors?.Status == Status.BusinessException || responseErrors?.Status == Status.Error)
                            {
                                analyticsEventCommand.Track("ServiceName", "ValidateOfferPriceResponse");
                                analyticsEventCommand.Track("ErrorId", responseErrors?.ResponseMessages?.Errors?.First().Id);
                                return ErrorDetails(responseErrors.ResponseMessages, analyticsEventCommand);
                            }

                            offerResponse.DataList.Penalty = new List<Penalty>();
                            offerResponse.DataList.BaggageAllowances = new List<BaggageAllowances>();
                            offerResponse.DataList.ProductComponents = new List<ProductComponent>();

                            ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsResponseListPTC = new();
                            ConcurrentDictionary<OfferDetails, Infrastructure.Model.BrandMS.BrandRS> brandMsResponseList = new();

                            if (request.OfferPriceRQ.Request.DataLists.PaxList.Any(x => x.PTC == Constants.Application.INF))
                            {
                                var tasks = offerResponse.Offers.Select(async offer =>
                                {
                                    var brandMsReq = BrandMsRqMapper.MapBrandMsPTCRequest(offer, offerResponse);
                                    var brandResponsePTC = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);

                                    if (!brandMsResponseListPTC.ContainsKey(offer))
                                        brandMsResponseListPTC.TryAdd(offer, brandResponsePTC);
                                });

                                await Task.WhenAll(tasks);
                               
                                if (brandMsResponseListPTC.Values.SelectMany(x => x ?? Enumerable.Empty<Infrastructure.Model.BrandMS.BrandRS>()).Any(x => x?.Errors?.Any() == true))
                                {
                                    analyticsEventCommand.Track("BrandMSHasErrors", true);
                                }
                            }
                            else
                            {
                                var tasks = offerResponse.Offers.Select(async offer =>
                                {
                                    var brandMsReq = BrandMsRqMapper.MapBrandMsRequest(offer, offerResponse);
                                    var brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);

                                    if (!brandMsResponseList.ContainsKey(offer))
                                        brandMsResponseList.TryAdd(offer, brandResponse.First());
                                });

                                await Task.WhenAll(tasks);

                                if (brandMsResponseList.Values.Any(x => x?.Errors?.Any() == true))
                                {
                                    analyticsEventCommand.Track("BrandMSHasErrors", true);
                                }
                            }

                            int penaltyIndex = 1;
                            int checkedBagCounter = 1;
                            int serviceCounter = 1;
                            IEnumerable<BaggageAllowances> baggageList = null;

                            if (brandMsResponseListPTC.Any())
                            {
                                foreach (var brandRespPTC in brandMsResponseListPTC)
                                {
                                    offerResponse = BrandMsRSMapper.UpdateOfferPricePTC(brandRespPTC.Value, brandRespPTC.Key, offerResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<OfferDetails, Infrastructure.Model.BrandMS.BrandRS> kvp in brandMsResponseList)
                                {
                                    offerResponse = BrandMsRSMapper.UpdateOfferPrice(kvp.Value, kvp.Key, offerResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);
                                }
                            }

                            var response = OfferPriceRSMapper.MapOfferPriceResponse(offerResponse, _genericDataMapper, isNonIataAgent, bspMarketPaymentData, warnings, offerDocument.Offers?.FirstOrDefault()?.ItaAccountCode, isCashPaymentAllowed, isEasyFopAllowed, isCardPaymentAllowed);

                            bool oneway = offerResponse.DataList?.OriginDestinationAssociations.Count() == 1;
                            bool roundtrip = false;

                            if (offerResponse.DataList?.OriginDestinationAssociations.Count() == 2)
                            {
                                var outboundOrigin = offerResponse.DataList?.OriginDestinationAssociations.ElementAt(0).OriginDestination.OriginAirport;
                                var outboundDestination = offerResponse.DataList?.OriginDestinationAssociations.ElementAt(0).OriginDestination.DestinationAirport;
                                var inboundOrigin = offerResponse.DataList?.OriginDestinationAssociations.ElementAt(1).OriginDestination.OriginAirport;
                                var inboundDestination = offerResponse.DataList?.OriginDestinationAssociations.ElementAt(1).OriginDestination.DestinationAirport;

                                if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                                {
                                    roundtrip = true;
                                }
                            }
                            analyticsEventCommand.Track("Oneway", oneway ? 'Y' : 'N');
                            analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                            analyticsEventCommand.Track("Multicity", (!oneway && !roundtrip) ? 'Y' : 'N');

                            for (int i = 1; i <= offerResponse.DataList.OriginDestinationAssociations.Count(); i++)
                            {
                                var ond = offerResponse.DataList.OriginDestinationAssociations.ElementAt(i - 1);

                                analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                                analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);
                            }

                            var priceClassIds = offerResponse.Offers.SelectMany(x => x.OfferItems).SelectMany(x => x.Services).Select(x => x.PriceClassRefIds.FirstOrDefault());

                            var flights = offerResponse.DataList.Journey.Select(journey =>
                            {
                                var segments = offerResponse.DataList.FlightSegments.Where(fs => journey.FlightSegmentRefIds.Contains(fs.SegmentId));

                                return new
                                {
                                    StopCount = segments.Select(s => s.Legs.Count() - 1)
                                };

                            });

                            analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                            return new Response<IATA_OfferPriceRS>() { Status = Status.OK, Value = response };
                        }
                        else
                        {
                            analyticsEventCommand.Track("ServiceName", "ValidateAgentAndPax");
                            analyticsEventCommand.Track($"ErrorId", agentErrors?.ResponseMessages?.Errors?.First().Id);
                            return ErrorDetails(agentErrors.ResponseMessages, analyticsEventCommand);
                        }
                    }
                    else
                    {
                        analyticsEventCommand.Track("ServiceName", "ValidateDbOffer");
                        analyticsEventCommand.Track($"ErrorId", dbErrors?.ResponseMessages?.Errors?.First().Id);
                        return ErrorDetails(dbErrors.ResponseMessages, analyticsEventCommand);
                    }
                }
                else
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOfferPriceVaRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsInValidation?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(errorsInValidation.ResponseMessages, analyticsEventCommand);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Offer Price Query Handler");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);

                return new Response<IATA_OfferPriceRS>()
                {
                    Status = Status.Error,
                    Value = MapToErrorOfferPriceRS(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private Response<IATA_OfferPriceRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);
            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException: resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            analyticsEventCommand.Track("StatusCode", statusCode);
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            return new Response<IATA_OfferPriceRS>()
            {
                Status = resultCode,
                Value = MapToErrorOfferPriceRS(responseMessage)
            };
        }

        private IATA_OfferPriceRS MapToErrorOfferPriceRS(ResponseMessage errors)
        {
            return new IATA_OfferPriceRS
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
    }
}
