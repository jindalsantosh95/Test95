using Newtonsoft.Json;
using System.Diagnostics;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using GraphQlAssertions = VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch.GraphQlAssertions;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Production.GraphQL
{
    public class SearchGraphQl
    {
        public static string env = Config.Environment;

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task SearchGraphQl_OneWay()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 0, numberOfYouths: 0, numberOfInfants: 0);

            Debug.WriteLine("Search graphql");

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse)) return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());

            OfferSearchHelper.GetOfferIdAndPassengersCountsFromDynamicCabinLink(offerSearchResponse, out string offerId, out string passengersCounts, out string dynamicCabinLink, isEnvProd: true);

            //generating brand graph ql request.
            var brandGQLRequest = OfferSearchHelper.BrandGQLRequest(offerId, passengersCounts);

            Debug.WriteLine("Brand graphql");

            // calling brand graph ql end point.
            var brandResponse = GraphQLEndpoint.Brand(brandGQLRequest).EnsureIsSuccessStatusCode();

            //generating sustainability graph ql request
            var sustainabilityGraphGQLRequest = OfferSearchHelper.SustainabilityGQLRequest(passengersCounts, offerId);

            Debug.WriteLine("Sustainability graphql");

            //post search GRAPHQL endpoint.
            var sustainabilityResponse = GraphQLEndpoint.OfferSearch(sustainabilityGraphGQLRequest, "").EnsureIsSuccessStatusCode();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            Debug.WriteLine("Summary graphql");

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            string summaryOfferId = offerPriceResult.data.searchSummary.id.Value;

            //generating brand graph ql request.
            var fareConditionsGQLRequest = OfferSearchHelper.FareConditionsGQLRequest(summaryOfferId);

            Debug.WriteLine("FareConditions graphql");

            //calling brand graph ql end point.
            var fareConditionsResponse = GraphQLEndpoint.Brand(fareConditionsGQLRequest).EnsureIsSuccessStatusCode();

            dynamic fareConditionsResult = JsonConvert.DeserializeObject<dynamic>(await fareConditionsResponse.Content.ReadAsStringAsync());

            Debug.WriteLine("Sales banner graphql");

            //generating sales banner graph ql request.
            var salesBannerGqlRequest = OfferSearchHelper.SalesBannerGQLRequest(origin: LocationType.LHR, destination: LocationType.JFK, departureDate: deptartureDate, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flightDirection: "one-way");

            //post sales banner GRAPHQL endpoint.
            var salesBannerResponse = GraphQLEndpoint.OfferSearch(salesBannerGqlRequest, "").EnsureIsSuccessStatusCode();
        }
    }
}