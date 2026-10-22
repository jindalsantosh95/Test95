using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FareQuery
    {
        private readonly IBrandService _brandService;
        private readonly IOfferRetrieveService _offerRetrieveService;
        private readonly IBasketService _basketService;
        private readonly IOfferSearchServices _offerSearchService;
        private readonly IRefDataService<AirportInfo> _airportInfoCacheService;

        public FareQuery(IBrandService brandService, IOfferRetrieveService offerRetrieveService, IBasketService basketService, IOfferSearchServices offerSearchService, IRefDataService<AirportInfo> airportInfoCacheService)
        {
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _offerRetrieveService = offerRetrieveService ?? throw new ArgumentNullException(nameof(offerRetrieveService));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
            _offerSearchService = offerSearchService ?? throw new ArgumentNullException(nameof(offerSearchService));
            _airportInfoCacheService = airportInfoCacheService ?? throw new ArgumentNullException(nameof(airportInfoCacheService));
        }

        [GraphQLName("fareConditions")]
        [GraphQLDescription("Retrieves fare conditions")]
        public async Task<FareInfoResponse> GetFareConditions(FareServiceRequest request)
        {
            var offerResponse = await _offerRetrieveService.OfferRetrieveAsync(new Models.FlightOffer.OfferSearchRequest() { OfferID = request.OfferId });

            var offerId = request.OfferId!.Split('|').Last();
            var fareRulesResponse = await _offerSearchService.FareRulesAsync(offerId);

            var airportDetails = await _airportInfoCacheService.Query();

            return new(offerResponse, fareRulesResponse, airportDetails);
        }
    }
}