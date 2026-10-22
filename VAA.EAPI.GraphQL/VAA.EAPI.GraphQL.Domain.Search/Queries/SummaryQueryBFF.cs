using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Summary = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SummaryQueryBFF : SummaryQueryBase
    {
        private readonly IOfferPriceServices _offerPriceService;
        private readonly IBasketService _basketService;

        public SummaryQueryBFF(ILogger<SummaryQueryBFF> logger, IOfferPriceServices offerPriceService, IOfferRetrieveService offerRetrieveService, IBasketService basketService, IBrandService brandService,
            IRefDataService<AircraftInfo> aircraftInfoCacheService, IRefDataService<AirportInfo> airportInfoCacheService,
            IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService, IRefDataService<BaggageDetails> baggageDetailsCacheService,
            IRefDataService<TaxCodeInfo> taxcodeInfoCacheService, IRefDataService<UpgradeInfo> upgradeInfoCacheService, ITokenValidatorExtension tokenValidatorExtension,
            IVRedServices vRedServices, IInRuleService inRuleService, ICustomerProvider customerProvider,
            ICustomerTransactionProvider customerTransactionProvider, IVoucherRedemptionService voucherRedemptionService)
            : base(logger, offerPriceService, offerRetrieveService, basketService, brandService,
            aircraftInfoCacheService, airportInfoCacheService, airlineInfoCacheService, cabinInfoCacheService, baggageDetailsCacheService,
            taxcodeInfoCacheService, upgradeInfoCacheService, tokenValidatorExtension, vRedServices, inRuleService, customerProvider, customerTransactionProvider, voucherRedemptionService)
        {
            _offerPriceService = offerPriceService ?? throw new ArgumentNullException(nameof(offerPriceService));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        }

        [GraphQLName("searchSummary")]
        [GraphQLDescription("Prices a selected offer, then adds the priced OfferID to the basket as the selected offer. Returns contents of the basket to display on the summary page.")]
        public async Task<Summary> SearchSummary(SearchSummaryRequest request)
        {
            // Call flightSummary (a slow call that requires basketData) as early as possible
            var basketData = await _basketService.RetrieveBasketAsync(request.BasketId);
            var offerMSRequest = OfferMSRequestMapper.BasketToOfferMSPriceRequestMapper(basketData, offerId: request.OfferId);
            var flightSummaryResponseTask = _offerPriceService.OfferPriceAsync(offerMSRequest);

            return await SearchSummaryShared(basketData, basketData.FlightsearchRequest.isAwardSearch, request.SummaryLink, flightSummaryResponseTask, null, request.OfferId);
        }
    }
}
