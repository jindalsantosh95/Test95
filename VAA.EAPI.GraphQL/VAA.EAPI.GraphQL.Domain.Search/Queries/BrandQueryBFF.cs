using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Amenity;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class BrandQueryBFF
    {
        private readonly ILogger<BrandQueryBFF> _logger;
        private readonly IBrandService _brandService;
        private readonly IOfferRetrieveService _offerRetrieveService;
        protected readonly IRefDataService<AirlineInfo> _airlineInfoCacheService;
        protected readonly IRefDataService<CabinInfo> _cabinInfoCacheService;

        public BrandQueryBFF(ILogger<BrandQueryBFF> logger, IBrandService brandService, IOfferRetrieveService offerRetrieveService, IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _offerRetrieveService = offerRetrieveService ?? throw new ArgumentNullException(nameof(offerRetrieveService));
            _airlineInfoCacheService = airlineInfoCacheService ?? throw new ArgumentNullException(nameof(airlineInfoCacheService));
            _cabinInfoCacheService = cabinInfoCacheService ?? throw new ArgumentNullException(nameof(cabinInfoCacheService));
        }

        [GraphQLName("brandBFF")]
        [GraphQLDescription("Retrieves brand info from Brand MS")]
        public async Task<CabinAmenity> GetBrandsBFF(BrandServiceRequest request)
        {
            return await GetBrands(request);
        }

        [GraphQLName("brand")]
        [GraphQLDescription("Retrieves brand info from Brand MS")]
        public async Task<CabinAmenity> GetBrands(BrandServiceRequest request)
        {
            _logger.LogInformation($"Correlation Id - {AsyncContext.Current.CorrelationId}, Brand query request \n {Json.Serialize(request)}");

            var airlineInfoTask = _airlineInfoCacheService.Query();
            var cabinInfoTask = _cabinInfoCacheService.Query();
            var offerResponse = await _offerRetrieveService.OfferRetrieveAsync(new Models.FlightOffer.OfferSearchRequest() { OfferID = request.OfferId });
            var cabinInfo = await cabinInfoTask;
            var promoCode = BrandMsRequestMapper.GetAppliedPromoCode(offerResponse);

            var brandRQ = BrandMsRequestMapper.BrandMsReqMapper(offerResponse, cabinInfo, PassengerTypeList.ParseString(request.PassengerCounts), promoCode);
            var brandRS = await _brandService.GetBrandAsync(brandRQ);

            var dynamicCabinData = new CabinAmenity(brandRS, offerResponse.Offers, offerResponse.DataList.FlightSegments, offerResponse.IsAwardSearchResult, await airlineInfoTask, cabinInfo);
            if (!dynamicCabinData?.CabinDetails?.Legs?.Any(l => l.CarryOnBags.IsNullOrEmpty() || l.CheckedBags.IsNullOrEmpty()) ?? true)
            {
                _logger.LogInformation($"No baggage details offer details | Journey Details: {Json.Serialize(offerResponse?.Offers?.FirstOrDefault()?.OfferItems?.FirstOrDefault())} | DataList: {Json.Serialize(offerResponse?.DataList)}");
            }

            return dynamicCabinData;
        }
    }
}
