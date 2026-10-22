using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOfferPrice
{
    public class OfferPriceQueryHandler : IRequestHandler<OfferPriceQuery, Result<Response<FlightOfferResponse>>>
    {
        private readonly IDeltaGqlService _deltaGqlService;
        private readonly ILogger<OfferPriceQueryHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IDistributedCacheService _distributedCacheService;

        public OfferPriceQueryHandler(IDeltaGqlService deltaGqlService, ILogger<OfferPriceQueryHandler> logger, IMediator mediator, IDistributedCacheService distributedCacheService)
        {
            _deltaGqlService = deltaGqlService ?? throw new ArgumentNullException(nameof(deltaGqlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<FlightOfferResponse>>> Handle(OfferPriceQuery requestData, CancellationToken cancellationToken)
        {
            var documentId = requestData.Request.FlightsearchRequest.OfferID.Split(Constants.Application.Pipeline)[0];
            var offerdoc = await _distributedCacheService.Get<OfferDocument>(documentId) ?? new();

            if (offerdoc.FlightOfferResponse.Offers.Any() == false)
            {
                return new Response<FlightOfferResponse>
                {
                    Status = Status.BadRequest
                };
            }

            requestData.Request.Pos = offerdoc.FlightOfferResponse.Pos;

            GqlSearchOffersVariables gqlSearchOffersVariables = new()
            {
                OfferSearchCriteria = new OfferSearchCriteria()
                {
                    ProductGroups = new ProductCategory[]
                    {
                            new() { ProductCategoryCode = ProductCategory.ProductCategoryCodeEnum.FLIGHTS },
                    },
                    OffersCriteria = new OffersCriteria
                    {
                        PricingCriteria = new PricingCriteria
                        {
                            PriceableIn = new List<string> {
                            requestData.Request.FlightsearchRequest.AwardSearch ? Constants.Application.Miles : Constants.Application.Currency
                        }
                        },
                        Preferences = new(),
                        FlightRequestCriteria = new FlightOfferCriteria
                        {
                            CurrentTripIndexId = requestData.Request.FlightsearchRequest.CurrentTripIndexId,
                            SortByBrandId = requestData.Request.FlightsearchRequest.BrandId ?? Constants.Application.VS_ECON_LIGHT,
                            SearchOriginDestination = (from Ond in requestData.Request.FlightsearchRequest.SearchOriginDestinations
                                                       select new Infrastructure.Proxy.DeltaOffer.SearchOriginDestination
                                                       {
                                                           Destinations = new Airport[] { new Airport { AirportCode = Ond.Destination } },
                                                           Origins = new Airport[] { new Airport { AirportCode = Ond.Origin } },
                                                           ConnectionAirportCode = Ond.ConnectionAirports,
                                                           DepartureLocalTs = DateTime.Parse(Ond.DepartureDate, System.Globalization.CultureInfo.InvariantCulture).ToString(Constants.Application.DateTimeFormat),
                                                       }),
                            SelectedOfferId = requestData.Request.FlightsearchRequest.OfferID.Split(Constants.Application.Pipeline).Last(),
                        }
                    },
                    Sender = (Party)requestData.Request.LoyaltyMember,
                    Customers = (from cus in requestData.Request.CustomerDetails
                                 select new Passenger
                                 {
                                     CustomerId = cus.CustId,
                                     PassengerTypeCode = cus.Ptc,
                                     PassengerId = cus.CustId
                                 })
                }
            };

            var query = File.ReadAllText("GraphqlQuery/gqlOfferPrice.graphql");
            var response = await _deltaGqlService.SendAsync(gqlSearchOffersVariables, query, cancellationToken);

            if (response.Data == null)
            {
                return Result.Success(new Response<FlightOfferResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Join(", ", response.Errors?.Select(e => e.Message)))
                });
            }

            var offersSetList = Json.DeserializeObject<OffersSetList>(response.Data["searchOffers"]);

            if (offersSetList.SelectedOfferSets == null || !offersSetList.SelectedOfferSets.Any())
                _logger.LogError($"No selected offer in OfferPriceQuery. Response: {Json.Serialize(response)}");

            var docId = Guid.NewGuid().ToString();
            var document = new OfferDocument
            {
                Id = docId.ToString(),
                PartitionKey = docId.ToString(),
                CreatedBy = Constants.Application.SearchOfferChannelID,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = Constants.Application.OfferDocType,
                FlightOfferResponse = OfferPriceRsMapper.OfferPriceResMapper(requestData.Request.Pos, offersSetList, docId, requestData.Request.FlightsearchRequest.AwardSearch, requestData.Request.FlightsearchRequest.PromoCode)
            };

            await _distributedCacheService.Set(docId, document, false, TimeSpan.Zero, TimeSpan.FromMinutes(ConfigReader.Try(Constants.Environment.DeltaOfferExpiryLimitInMinutes, 120)));

            return new Response<FlightOfferResponse>
            {
                Status = Status.OK,
                Value = document.FlightOfferResponse
            };
        }
    }
}