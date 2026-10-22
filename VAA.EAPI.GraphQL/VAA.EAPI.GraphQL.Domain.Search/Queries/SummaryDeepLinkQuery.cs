using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Summary = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SummaryDeepLinkQuery : SummaryQueryBase
    {
        private readonly IOfferPriceServices _offerPriceService;
        private readonly IBasketService _basketService;
        protected readonly IRefDataService<AirportInfo> _airportInfoCacheService;
        private readonly IRefDataService<CabinInfo> _cabinInfoCacheService;

        private static readonly Dictionary<string, string> flexNonFlexfarefamilyMapping = new()
        {
            { "ECON-CLASSIC-FLEXIBLE","ECON" },
            { "ECON-DELIGHT-FLEXIBLE","ECONOMY-DELIGHT"},
            { "COMFORT-PLUS-PREMIUM-FLEXIBLE","COMFORT-PLUS-PREMIUM-ECON"},
            { "BUS-FIRST-FLEXIBLE","BUS-FIRST"}
        };

        public SummaryDeepLinkQuery(ILogger<SummaryQueryBFF> logger, IOfferPriceServices offerPriceService, IOfferRetrieveService offerRetrieveService, IBasketService basketService, IBrandService brandService,
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
            _airportInfoCacheService = airportInfoCacheService ?? throw new ArgumentNullException(nameof(airportInfoCacheService));
            _cabinInfoCacheService = cabinInfoCacheService ?? throw new ArgumentNullException(nameof(cabinInfoCacheService));
        }

        [GraphQLName("searchSummaryDeepLink")]
        [GraphQLDescription("Prices the selected flights and fare from Metas and returns priced offer details")]
        public async Task<Summary> SearchSummaryDeepLink(SearchSummaryDeepLinkQueryRequest request)
        {
            var isCabinUpdatedinDeeplinkSummaryRQ = false;
            var requestedBrandId = request?.DominantSegmentBrandId;

            var airportDetails = await _airportInfoCacheService.Query(request?.FlightSegments?.First().OriginAirportCode);
            var refCabinMappings = await _cabinInfoCacheService.Query();

            var requestflexNonFlexFarefamily = flexNonFlexfarefamilyMapping.FirstOrDefault(ff => ff.Key.Equals(refCabinMappings.FirstOrDefault(x => x.BrandID.Equals(requestedBrandId))?.FareFamily));

            if (requestflexNonFlexFarefamily.Value != null) // Flex farefamily requested in deeplink
            {
                var nonFlexBrandId = refCabinMappings.FirstOrDefault(c => c.FareFamily.Equals(requestflexNonFlexFarefamily.Value) && c.Carrier == "VS")?.BrandID;

                if (nonFlexBrandId != null)
                {
                    request.DominantSegmentBrandId = nonFlexBrandId;
                    isCabinUpdatedinDeeplinkSummaryRQ = true;
                }
            }

            var offerRequest = OfferMSSummaryDeepLinkRequestMapper.Map(request, airportDetails?.FirstOrDefault());
            var deeplinkResponseTask = _offerPriceService.OfferPriceDeeplinkAsync(offerRequest);
            var basketRequest = BasketRequestMapper.CreateForDeepLink(offerRequest, requestedBrandId);

            if (isCabinUpdatedinDeeplinkSummaryRQ)
            {
                var deeplinkResponse = await deeplinkResponseTask;
                var upselloffer = deeplinkResponse?.Offers?.FirstOrDefault(c => c.UpsellOffer && c.OfferItems.First().Services.First().FareDetails.DominantFareProduct.Equals(requestedBrandId));

                if (upselloffer != null)
                {
                    var offerMSRequest = OfferMSRequestMapper.BasketToOfferMSPriceRequestMapper(basketRequest, offerId: upselloffer.OfferId);
                    deeplinkResponseTask = _offerPriceService.OfferPriceAsync(offerMSRequest);
                }
            }

            return await SearchSummaryShared(basketRequest, basketRequest.FlightsearchRequest.isAwardSearch, request.SummaryLink, deeplinkResponseTask, request.TotalOfferedAmount);
        }

    }

}
