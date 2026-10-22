using HotChocolate.Resolvers;
using System.Security.Authentication;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class OfferSearchBFFQuery : OfferSearchBFFBase
{
    public OfferSearchBFFQuery(IOfferSearchServices offerSearchServices, IBasketService basketService, IRefDataService<AircraftInfo> aircraftInfoCacheService,
        IRefDataService<AirportInfo> airportInfoCacheService, IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService,
        IGraphqlHelper graphqlHelper, TimeProvider timeProvider, ITokenValidatorExtension tokenValidatorExtension, IAnalyticsService analyticsService) :
        base(offerSearchServices, basketService, aircraftInfoCacheService, airportInfoCacheService, airlineInfoCacheService, cabinInfoCacheService, graphqlHelper, timeProvider, tokenValidatorExtension, analyticsService)
    { }

    [GraphQLName("searchOffersBFF")]
    public async Task<SearchResults> OfferSearchBFF(Models.FlightOffer.FlightOfferRequest request, IResolverContext resolverContext)
    {
        return await OfferSearch(request, resolverContext);
    }

    [GraphQLName("searchOffers")]
    [GraphQLDescription("Retrieve a list of offers from Offer MS and map to BFF / FE response")]
    public async Task<SearchResults> OfferSearch(Models.FlightOffer.FlightOfferRequest request, IResolverContext resolverContext)
    {
        string? flyingClubNumber = null;
        if (request.FlightSearchRequest?.AwardSearch == true)
        {
            flyingClubNumber = await _tokenValidatorExtension.ValidateTokenExtractFCNumber(TokenHelper.GetAuthorizationToken());
            if (flyingClubNumber.IsNullOrEmpty()) throw new AuthenticationException("Must be logged in to a valid account.");
        }

        return await offerSearch(request, resolverContext);
    }

    [GraphQLName("searchOffersCallCentre")]
    [GraphQLDescription("Retrieve a list of offers from Offer MS for the call centre")]
    public async Task<SearchResults> OfferSearchCallCentre(Models.FlightOffer.FlightOfferRequest request, IResolverContext resolverContext)
    {
        // This is unauthenticated as it is used by the call centre app not by a consumer.
        return await offerSearch(request, resolverContext);
    }

    private async Task<SearchResults> offerSearch(Models.FlightOffer.FlightOfferRequest request, IResolverContext resolverContext)
    {
        if (request.BasketId != null && request.BasketId != Guid.Empty)
        {
            var basketData = await _basketService.RetrieveBasketAsync(request.BasketId.Value);

            var requestFromBasket = OfferMSRequestMapper.BasketToOfferMSRequestMapper(basketData, string.Empty, "0");
            requestFromBasket.SortRequest = request.SortRequest;
            requestFromBasket.Filters = request.Filters;

            return await base.FirstSliceSearch(requestFromBasket, resolverContext);
        }

        return await base.FirstSliceSearch(request, resolverContext);
    }
}