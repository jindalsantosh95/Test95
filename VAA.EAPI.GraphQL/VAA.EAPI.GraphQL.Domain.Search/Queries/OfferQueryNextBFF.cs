using HotChocolate.Resolvers;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class OfferSearchNextBFFQuery : OfferSearchBFFBase
{
    public OfferSearchNextBFFQuery(IOfferSearchServices offerSearchServices, IBasketService basketService, IRefDataService<AircraftInfo> aircraftInfoCacheService,
        IRefDataService<AirportInfo> airportInfoCacheService, IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService,
        IGraphqlHelper graphqlHelper, TimeProvider timeProvider, ITokenValidatorExtension tokenValidatorExtension, IAnalyticsService analyticsService) :
        base(offerSearchServices, basketService, aircraftInfoCacheService, airportInfoCacheService, airlineInfoCacheService, cabinInfoCacheService, graphqlHelper, timeProvider, tokenValidatorExtension, analyticsService)
    { }

    [GraphQLName("searchOffersNextBFF")]
    [GraphQLDeprecated("Use searchOffersNextSlice")]
    public async Task<SearchResults> OfferSearchBFF(FlightOfferNextRequest request, IResolverContext resolverContext)
    {
        return await OfferSearch(request, resolverContext);
    }

    [GraphQLName("searchOffersNext")]
    [GraphQLDeprecated("Use searchOffersNextSlice")]
    [GraphQLDescription("After selecting the first leg, retrieve a list of offers from Offer MS and map to BFF / FE response")]
    public async Task<SearchResults> OfferSearch(FlightOfferNextRequest request, IResolverContext resolverContext)
    {
        return await base.NextSliceSearch(request, resolverContext);
    }

    [GraphQLName("searchOffersNextSlice")]
    [GraphQLDescription("After selecting the first leg, retrieve a list of offers from Offer MS and map to BFF / FE response")]
    public async Task<SearchResults> OfferSearch(SearchOffersNextSliceRequest request, IResolverContext resolverContext)
    {
        return await base.NextSliceSearch(request, resolverContext);
    }
}