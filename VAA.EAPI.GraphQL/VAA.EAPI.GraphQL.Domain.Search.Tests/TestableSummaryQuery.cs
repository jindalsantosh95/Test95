using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Search.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Summary = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class TestableSummaryQuery(
    ILogger<SummaryQueryBFF> logger,
    IOfferPriceServices offerPriceService,
    IOfferRetrieveService offerRetrieveService,
    IBasketService basketService,
    IBrandService brandService,
    IRefDataService<AircraftInfo> aircraftInfoCacheService,
    IRefDataService<AirportInfo> airportInfoCacheService,
    IRefDataService<AirlineInfo> airlineInfoCacheService,
    IRefDataService<CabinInfo> cabinInfoCacheService,
    IRefDataService<BaggageDetails> baggageDetailsCacheService,
    IRefDataService<TaxCodeInfo> taxcodeInfoCacheService,
    IRefDataService<UpgradeInfo> upgradeInfoCacheService,
    ITokenValidatorExtension tokenValidatorExtension,
    IVRedServices vRedServices,
    IInRuleService inRuleService,
    ICustomerProvider customerProvider,
    ICustomerTransactionProvider customerTransactionProvider,
    IVoucherRedemptionService voucherRedemptionService
        ) : SummaryQueryBase(logger, offerPriceService, offerRetrieveService, basketService, brandService,
          aircraftInfoCacheService, airportInfoCacheService,
          airlineInfoCacheService, cabinInfoCacheService,
          baggageDetailsCacheService, taxcodeInfoCacheService,
          upgradeInfoCacheService, tokenValidatorExtension,
          vRedServices, inRuleService, customerProvider, customerTransactionProvider, voucherRedemptionService)
    {
        public Task<Summary> InvokeSearchSummaryShared(Basket basket, bool isAwardSearch, string? summaryLink, Task<FlightOfferResponse> flightResponse, Amount amount)
        {
            return SearchSummaryShared(basket, isAwardSearch, summaryLink, flightResponse, amount);
        }
    }
}
