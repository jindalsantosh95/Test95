using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Search.Constants;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class BrandGraphQl
    {
        private bool requireUsableOfferForSignOff;

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void BrandGraphQlTest() => ValidateBrandGraphQl();

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "BrandMS")]
        [Trait("SharedServiceConsumer", "Search")]
        public void SharedServicesSignOff_Brand_RequiresCabinDetails()
        {
            requireUsableOfferForSignOff = true;
            ValidateBrandGraphQl();
        }

        private void ValidateBrandGraphQl()
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"BrandMS sign-off requires a usable offer before invoking Brand. OfferSearch response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.data.searchOffers.result.slice.flightsAndFares.Count > 0, "No offers have been returned for the search details provided");

            OfferSearchHelper.GetOfferIdAndPassengersCountsFromDynamicCabinLink(offerSearchResponse, out string offerId, out string passengersCounts, out string dynamicCabinLink);

            //generating brand graph ql request.
            string brandGQLRequest = OfferSearchHelper.BrandGQLRequest(offerId, passengersCounts);

            // calling brand graph ql end point.
            HttpResponseMessage brandResponse = GraphQLEndpoint.Brand(brandGQLRequest).EnsureGQLResponseSuccess();

            dynamic brandResult = JsonConvert.DeserializeObject<dynamic>(brandResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandResult.data.brandBFF.cabinDetails.legs.Count > 0, "Cabin legs details not exists.");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void FareConditionGraphQlTest() => ValidateFareConditionGraphQl();

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "BrandMS")]
        [Trait("SharedServiceConsumer", "Search")]
        public void SharedServicesSignOff_Brand_RequiresFareConditions()
            => ValidateFareConditionGraphQl(requireUsableOfferForSignOff: true);

        private void ValidateFareConditionGraphQl(bool requireUsableOfferForSignOff = false)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
            {
                Assert.False(
                    requireUsableOfferForSignOff,
                    $"BrandMS sign-off requires a usable offer before invoking fare conditions. OfferSearch response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            string summaryOfferId = offerPriceResult.data.searchSummary.id.Value;

            //generating brand graph ql request.
            string fareConditionsGQLRequest = OfferSearchHelper.FareConditionsGQLRequest(summaryOfferId);

            //calling brand graph ql end point.
            HttpResponseMessage fareConditionsResponse = GraphQLEndpoint.Brand(fareConditionsGQLRequest).EnsureGQLResponseSuccess();

            dynamic fareConditionsResult = JsonConvert.DeserializeObject<dynamic>(fareConditionsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(fareConditionsResult.data.fareConditions.rules.Count > 0, "Fare rules details not exists.");
        }

        private static string BuildOfferSearchRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None,
            int numberOfAdults = 0, int numberOfChildren = 0, int numberOfYouths = 0, int numberOfInfants = 0, bool flexiDateSearch = false, bool calenderSearch = false, string offerId = "", bool rewardSearch = false, string promoCode = "", LocationType connectionAirportOne = LocationType.None, LocationType connectionAirportTwo = LocationType.None, string basketId = "")
        {
            TestConfig config = new TestConfig();
            config.Search.JourneyType = journeyType;
            config.Search.DepartureDate = departureDate.IsNullOrEmpty() ? config.Search.DepartureDate : departureDate;
            config.Search.ReturnDate = returnDate.IsNullOrEmpty() ? config.Search.ReturnDate : returnDate;
            config.Search.Origin = locationOne == LocationType.None ? config.Search.Origin : locationOne;
            config.Search.Destination = locationTwo == LocationType.None ? config.Search.Destination : locationTwo;
            config.Search.IsFlexiDateSearch = flexiDateSearch;
            config.Search.IsCalendarSearch = calenderSearch;
            config.Search.IsRewardSearch = rewardSearch;
            config.Search.PromoCode = promoCode;

            if (connectionAirportOne != LocationType.None)
            {
                config.Search.Connections[0] = connectionAirportOne;
            }

            if (connectionAirportTwo != LocationType.None)
            {
                config.Search.Connections[1] = connectionAirportTwo;
            }

            config.Internal.OfferId = offerId;
            config.Internal.BasketId = basketId;

            config.Search.Passengers.Adults = CreatePassengers(numberOfAdults);
            config.Search.Passengers.Children = CreatePassengers(numberOfChildren);
            config.Search.Passengers.YoungAdults = CreatePassengers(numberOfYouths);
            config.Search.Passengers.Infants = CreatePassengers(numberOfInfants);

            return OfferSearchHelper.OfferSearchGQLRequest(config);
        }

        private static List<Passenger> CreatePassengers(int count)
        {
            List<Passenger> passengers = new List<Passenger>();

            for (int i = 0; i < count; i++)
            {
                passengers.Add(new Passenger());
            }

            return passengers;
        }
    }
}

