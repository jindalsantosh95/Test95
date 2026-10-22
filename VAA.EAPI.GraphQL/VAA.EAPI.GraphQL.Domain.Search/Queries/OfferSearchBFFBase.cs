using HotChocolate.Resolvers;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Diagnostics;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferSearch;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    public abstract class OfferSearchBFFBase
    {
        private IOfferSearchServices _offerSearchServices;
        protected readonly IBasketService _basketService;
        protected readonly IAnalyticsService _analyticsService;
        protected readonly IRefDataService<AircraftInfo> _aircraftInfoCacheService;
        protected readonly IRefDataService<AirportInfo> _airportInfoCacheService;
        protected readonly IRefDataService<AirlineInfo> _airlineInfoCacheService;
        protected readonly IRefDataService<CabinInfo> _cabinInfoCacheService;
        protected readonly IGraphqlHelper _graphqlHelper;
        protected readonly TimeProvider _timeProvider;
        protected readonly ITokenValidatorExtension _tokenValidatorExtension;

        public OfferSearchBFFBase(IOfferSearchServices offerSearchServices, IBasketService basketService, IRefDataService<AircraftInfo> aircraftInfoCacheService,
            IRefDataService<AirportInfo> airportInfoCacheService, IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService,
            IGraphqlHelper graphqlHelper, TimeProvider timeProvider, ITokenValidatorExtension tokenValidatorExtension, IAnalyticsService analyticsService)
        {
            _offerSearchServices = offerSearchServices ?? throw new ArgumentNullException(nameof(offerSearchServices));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
            _aircraftInfoCacheService = aircraftInfoCacheService ?? throw new ArgumentNullException(nameof(aircraftInfoCacheService));
            _airportInfoCacheService = airportInfoCacheService ?? throw new ArgumentNullException(nameof(airportInfoCacheService));
            _airlineInfoCacheService = airlineInfoCacheService ?? throw new ArgumentNullException(nameof(airlineInfoCacheService));
            _cabinInfoCacheService = cabinInfoCacheService ?? throw new ArgumentNullException(nameof(cabinInfoCacheService));
            _graphqlHelper = graphqlHelper ?? throw new ArgumentNullException(nameof(graphqlHelper));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _tokenValidatorExtension = tokenValidatorExtension ?? throw new ArgumentNullException(nameof(tokenValidatorExtension));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        public async Task<SearchResults> FirstSliceSearch(FlightOfferRequest request, IResolverContext resolverContext)
        {
            ArgumentNullException.ThrowIfNull(request.FlightSearchRequest);
            ArgumentNullException.ThrowIfNull(request.FlightSearchRequest.SearchOriginDestinations);

            //_analyticsService.TrackAnalytics(AnalyticsRequestMapper.MapFirstSliceRequest(request));


            if (request.FlightSearchRequest.CurrentTripIndexId != "0")
            {
                throw new ArgumentOutOfRangeException("Must be first slice.");
            }

            var trace = Trace<OfferSearchBFFBase>.Create();

            var airportDetails = await _airportInfoCacheService.Query(request?.FlightSearchRequest?.SearchOriginDestinations?.First().Origin);
            if (airportDetails?.Any() ?? false)
            {
                var posDetails = airportDetails.First();

                request.Pos = new() { CityCode = posDetails.PosCityCode, CountryCode = posDetails.CountryCode };
            }

            var flightSearchResponseTask = OfferSearchAsync(request, resolverContext);
            var cabinInfoTask = _cabinInfoCacheService.Query();
            var aircraftInfoTask = _aircraftInfoCacheService.Query();
            var airlineInfoTask = _airlineInfoCacheService.Query();

            var flightSearchResponse = await flightSearchResponseTask;

            var airportCodes = request.FlightSearchRequest.SearchOriginDestinations.Select(x => x.Origin)
                    .Concat(request.FlightSearchRequest.SearchOriginDestinations.Select(x => x.Destination));
            if (flightSearchResponse.FlightOfferResponse != null)
            {
                airportCodes = airportCodes
                    .Concat(flightSearchResponse.FlightOfferResponse!.DataList.FlightSegments.SelectMany(fs => fs.Legs).SelectMany(x => new string[] { x.OriginAirportCode, x.DestinationAirportCode }))
                    .Concat(flightSearchResponse.FlightOfferResponse.DataList.OriginDestinationAssociations.SelectMany(x => new string[] { x.OriginDestination.OriginAirport, x.OriginDestination.DestinationAirport }));
            }
            var airportDetailsTask = _airportInfoCacheService.Query(airportCodes.Distinct().ToArray());

            var basketResponse = await _basketService.CreateBasketAsync((Basket)request);

            return OfferMStoBFFMapper.Map(
                flightSearchResponse, request.FlightSearchRequest.SearchOriginDestinations, request.FlightSearchRequest.SearchOriginDestinations.Count(),
                request.FlightSearchRequest.CurrentTripIndexId.Parse<int>(), basketResponse.BasketId, request.GetPassengerCounts(),
                request.FlightSearchRequest.OfferID, request.SortRequest, request.Filters, await airportDetailsTask, await aircraftInfoTask, await airlineInfoTask,
                await cabinInfoTask);
        }

        public async Task<SearchResults> NextSliceSearch(SearchOffersNextSliceRequest request, IResolverContext resolverContext)
        {
            if (request.OfferId.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(request.OfferId));
            }
            if (request.CurrentTripIndexId > 0 == false)
            {
                throw new ArgumentOutOfRangeException("Must be second or later slice.");
            }

            Guid? basketId = request.BasketId;
            if (basketId == null || basketId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(request.BasketId));
            }

            var basketData = await _basketService.RetrieveBasketAsync(basketId.Value);
            var offerMSRequest = OfferMSRequestMapper.BasketToOfferMSRequestMapper(basketData, offerId: request.OfferId, tripIndex: request.CurrentTripIndexId.ToString());
            return await NextSliceSearch(offerMSRequest, basketData, request.CurrentTripIndexId, request.SortRequest, request.Filters, resolverContext);
        }

        public async Task<SearchResults> NextSliceSearch(FlightOfferNextRequest request, IResolverContext resolverContext)
        {
            if (request.FlightSearchRequest == null || request.FlightSearchRequest.OfferID.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(request.FlightSearchRequest.OfferID));
            }
            if (Convert.ToInt16(request.FlightSearchRequest.CurrentTripIndexId) > 0 == false)
            {
                throw new ArgumentOutOfRangeException("Must be second or later slice.");
            }

            Guid? basketId = request.BasketId;
            if (basketId == null || basketId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(request.BasketId));
            }

            var basketData = await _basketService.RetrieveBasketAsync(basketId.Value);
            var offerMSRequest = OfferMSRequestMapper.BasketToOfferMSRequestMapper(basketData, offerId: request.FlightSearchRequest.OfferID, tripIndex: request.FlightSearchRequest.CurrentTripIndexId);
            return await NextSliceSearch(offerMSRequest, basketData, request.FlightSearchRequest.CurrentTripIndexId.Parse<int>(), request.SortRequest, request.Filters, resolverContext);
        }

        private async Task<SearchResults> NextSliceSearch(FlightOfferRequest offerMSRequest, Basket basketData, int currentSlice, string selectedSort, FlightOfferRequestFilters selectedFilters, IResolverContext resolverContext)
        {
            var flightSearchResponseTask = OfferSearchAsync(offerMSRequest, resolverContext);
            var cabinInfoTask = _cabinInfoCacheService.Query();
            var airportDetails = Enumerable.Empty<AirportInfo>();

            var flightSearchResponse = await flightSearchResponseTask;

            if (flightSearchResponse.FlightOfferResponse != null)
            {
                var airportCodes = flightSearchResponse.FlightOfferResponse!.DataList.FlightSegments.SelectMany(fs => fs.Legs).SelectMany(x => new string[] { x.OriginAirportCode, x.DestinationAirportCode })
                                                                               .Concat(flightSearchResponse.FlightOfferResponse.DataList.OriginDestinationAssociations.SelectMany(x => new string[] { x.OriginDestination.OriginAirport, x.OriginDestination.DestinationAirport }))
                                                                               .Concat(offerMSRequest.FlightSearchRequest.SearchOriginDestinations.Select(x => x.Origin))
                                                                               .Concat(offerMSRequest.FlightSearchRequest.SearchOriginDestinations.Select(x => x.Destination))
                                                                               .Distinct()
                                                                               .ToArray();
                airportDetails = await _airportInfoCacheService.Query(airportCodes);
            }

            if (flightSearchResponse != null && flightSearchResponse.FlightOfferResponse != null && flightSearchResponse.FlightOfferResponse.Offers.Any())
            {
                var offerId = flightSearchResponse.FlightOfferResponse.Offers.First(o => o.Selected)?.OfferId;
                await _basketService.UpdateOfferIDAsync(offerId, basketData.BasketId);

                var currentSliceFlightSearchDetails = offerMSRequest.FlightSearchRequest.SearchOriginDestinations.ElementAt(currentSlice);

                var bookingJourneyOffer = new BookingJourneyOfferDetails
                {
                    Slice = currentSlice.ToString(),
                    OfferId = offerMSRequest.FlightSearchRequest.OfferID,
                    OriginDestinationLabel = $"{currentSliceFlightSearchDetails.Origin}-{currentSliceFlightSearchDetails.Destination}",
                    OriginDestinationDepartureDate = currentSliceFlightSearchDetails.DepartureDate
                };

                await _basketService.UpdateSelectedOfferAsync(basketData.BasketId, bookingJourneyOffer);
            }

            var aircraftInfo = await _aircraftInfoCacheService.Query();
            var airlineInfo = await _airlineInfoCacheService.Query();


            return OfferMStoBFFMapper.Map(
                flightSearchResponse, basketData.FlightsearchRequest.SearchOriginDestinations.Select(sod => (Models.FlightOffer.SearchOriginDestination)sod), offerMSRequest.FlightSearchRequest.SearchOriginDestinations.Count(),
                offerMSRequest.FlightSearchRequest.CurrentTripIndexId.Parse<int>(), basketData.BasketId, offerMSRequest.GetPassengerCounts(),
                offerMSRequest.FlightSearchRequest.OfferID, selectedSort, selectedFilters, airportDetails, aircraftInfo, airlineInfo,
                await cabinInfoTask);
        }


        private async Task<FlightSearchResponse> OfferSearchAsync(FlightOfferBaseRequest request, IResolverContext resolverContext)
        {
            var selections = _graphqlHelper.GetSelections(resolverContext);

            DateTime.TryParse(request.FlightSearchRequest?.SearchOriginDestinations?.Min(ond => ond.DepartureDate), out var firstSliceFlightDate);
            if (firstSliceFlightDate < _timeProvider.GetUtcNow().Date)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Search Error")
                    .SetExtension("Reason", Json.Serialize(new List<ErrorDetails>()
                    {
                        new ErrorDetails
                        {
                            Code = Constants.ErrorCodes.OfferSearchNoOffers,
                            DeveloperMessage = $"{firstSliceFlightDate} is the past future | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                            Message = $"Doesn't time fly? The date you've search for has now passed. Please try a different date."
                        }
                    }))
                    .SetCode(((int)HttpStatusCode.UnprocessableEntity).ToString())
                    .Build());
            }

            var trace = Trace<OfferSearchBFFBase>.Create();

            if (request is null) throw new ArgumentNullException(nameof(request));

            if (request.FlightSearchRequest.AwardSearch != true)
                request.LoyaltyMember = new() { PayWithPoints = true };

            var flightSearchResponse = new FlightSearchResponse();

            HttpRequestException? flightSearchException = null;
            IEnumerable<ErrorDetails> flightSearchError = null;

            var flightOfferTask = _offerSearchServices.FlightOfferSearchAsync((FlightOfferRequest)request);

            Task<FlexiDateSearchResponse?>? flexiSearchTask = null;
            if (_graphqlHelper.FieldIsSelected(selections, "priceGrid"))
            {
                flexiSearchTask = _offerSearchServices.FlexiDateSearchAsync((FlexiDateOfferRequest)request);
            }
            Task<CalendarSearchResponse?>? calendarSearchTask = null;
            if (_graphqlHelper.FieldIsSelected(selections, "calendar"))
            {
                calendarSearchTask = _offerSearchServices.CalendarSearchAsync((CalendarOfferRequest)request);
            }

            try
            {
                flightSearchResponse.FlightOfferResponse = await flightOfferTask;

                if (flightSearchResponse.FlightOfferResponse.Offers == null)
                    flightSearchResponse.FlightOfferResponse = null;
            }
            catch (HttpRequestException ex)
            {
                var content = await ex.ReadResponseContentAsStringAsync(CancellationToken.None);

                if (!content.IsNullOrWhiteSpace())
                {
                    var errorDetails = Json.Deserialize<IEnumerable<DeltaErrorResponse>>(content).First();

                    if (errorDetails != null)
                    {
                        flightSearchError = ErrorHelper.OfferErrorMapper(errorDetails);
                    }
                }

                flightSearchException = ex;
                flightSearchResponse.FlightOfferResponse = null;
            }

            if (flexiSearchTask != null)
            {
                try
                {
                    flightSearchResponse.FlexiDateSearchResponse = await flexiSearchTask;
                }
                catch
                {
                    flightSearchResponse.FlexiDateSearchResponse = null;
                }
            }

            if (calendarSearchTask != null)
            {
                try
                {
                    flightSearchResponse.CalendarSearchResponse = await calendarSearchTask;
                }
                catch
                {
                    flightSearchResponse.CalendarSearchResponse = null;
                }
            }

            // If we have either a good set of flight results or multidate calendar, that's enoguh for a response page - no need for error handling.
            if (Constants.Environment.EnableErrorHandling && flightSearchResponse.FlightOfferResponse == null && flightSearchResponse.FlexiDateSearchResponse == null)
            {
                DateTime.TryParse(request.FlightSearchRequest?.SearchOriginDestinations?.Max(ond => ond.DepartureDate), out var lastSliceFlightDate);
                IEnumerable<ErrorDetails> errorDetails = null;
                if (lastSliceFlightDate > DateTime.UtcNow.AddDays(Constants.Application.FlightsGoOnSaleFromDays - 1))
                {
                    errorDetails = new List<ErrorDetails>()
                    {
                        new ErrorDetails
                        {
                            Code = Constants.ErrorCodes.OfferSearchNoOffers,
                            DeveloperMessage = $"{lastSliceFlightDate} is too far in the future | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                            Message = $"We'll be ready for you soon. The date you've searched for isn't available to book yet. Please try a different date, or come back on {lastSliceFlightDate.AddDays(- Constants.Application.FlightsGoOnSaleFromDays + 1).ToString(Constants.Application.FriendlyDayAndMonth)}."
                        }
                    };
                }
                else if (flightSearchError != null)
                {
                    errorDetails = flightSearchError;
                }
                else if (flightSearchException != null)
                {
                    throw flightSearchException;
                }

                throw new GraphQLException(ErrorBuilder.New().
                    SetMessage("Search Error").
                    SetExtension("Reason", Json.Serialize(errorDetails)).
                    SetCode(((int)HttpStatusCode.UnprocessableEntity).ToString()).
                             Build());
            }

            return flightSearchResponse;
        }

    }
}
