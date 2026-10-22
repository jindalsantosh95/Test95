using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Requests.Endpoints;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class DeepLinkGraphQl
    {
        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "GraphQL")]
        public void SharedServicesSignOff_DeepLink_ReturnSummary() =>
            DeepLinkGraphQl_TC01_ReturnTrip_VAFlights_DeepLinkSummaryReturnsFlights(
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.ECONOMYDELIGHT);

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.LAX, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.LAS, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.SFO, LocationType.LHR, FareFamilyType.BUSFIRST)]
        [InlineData(LocationType.JFK, LocationType.LHR, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        public void DeepLinkGraphQl_TC01_ReturnTrip_VAFlights_DeepLinkSummaryReturnsFlights(LocationType origin, LocationType destination, FareFamilyType fareFamily)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(
                journeyType: JourneyType.Return,
                departureDate: deptartureDate,
                returnDate: returnDate,
                locationOne: origin,
                locationTwo: destination,
                locationThree: destination,
                locationFour: origin,
                numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            TestConfig outboundConfig = BuildLinkConfig(fareFamily, FlightType.VS, isReturnJourney: false);
            string[] fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(outboundConfig, offerSearchResult);
            if (!fareLinks.Any())
            {
                string[] fallbackFareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchResult, fareFamily);
                if (fallbackFareLinks.Any())
                {
                    fareLinks = fallbackFareLinks;
                }
                else
                {
                    fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResult, "fare");
                }
            }

            Assert.True(fareLinks.Any(), $"Offer(Outbound) not returned for the fare family->{fareFamily} and flight type passed->VS");

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: false);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(
                journeyType: JourneyType.Return,
                offerId: offerId,
                basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            string offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

            TestConfig inboundConfig = BuildLinkConfig(fareFamily, FlightType.VS, isReturnJourney: true);
            string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForAirlineAndFareFamily(inboundConfig, offerSearchReturnResult);
            if (!summaryLinks.Any())
            {
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchReturnResult, fareFamily);
                if (fallbackSummaryLinks.Any())
                {
                    summaryLinks = fallbackSummaryLinks;
                }
                else
                {
                    summaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchReturnResult, "summary");
                    if (!summaryLinks.Any())
                    {
                        summaryLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchReturnResult, fareFamily);
                    }
                }
            }

            Assert.True(summaryLinks.Any(), $"Offer(Inbound) not returned for the fare family->{fareFamily} and flight type passed->VS");

            //getting offer id from summary link
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            string deepLinkSummaryGQLRequest = GraphQLRequestBuilder.DeepLinkSummaryGQLRequest(offerPriceResponse);

            //post price GRAPHQL endpoint.
            HttpResponseMessage deeplinkSummaryResponse = GraphQLEndpoint.OfferPrice(deepLinkSummaryGQLRequest);
            EnsureDeepLinkSummarySuccess(deeplinkSummaryResponse);

            JToken deeplinkSummaryResult = JsonConvert.DeserializeObject<JToken>(deeplinkSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] deeplinkFlightsResult = deeplinkSummaryResult.SelectTokens("$..flights").Select(x => x.ToString()).ToArray();

            Assert.True(deeplinkFlightsResult.Any(), $"Deeplink summary flights result not exists : {deeplinkSummaryResult}");
        }

        private static void EnsureDeepLinkSummarySuccess(HttpResponseMessage deeplinkSummaryResponse)
        {
            try
            {
                deeplinkSummaryResponse.EnsureGQLResponseSuccess();
            }
            catch (Exception exception) when (IsDeltaRetailOfferError(deeplinkSummaryResponse))
            {
                throw new Exception(
                    $"FailureBucket=DeepLink.DeltaRetailOfferError.G052. searchSummaryDeepLink returned a downstream Delta RetailOfferError. {BuildDeepLinkFailureBucketDiagnostic(deeplinkSummaryResponse)}",
                    exception);
            }
        }

        private static bool IsDeltaRetailOfferError(HttpResponseMessage response)
        {
            string responseBody = response.ReadResponseAsString();
            return responseBody.Contains("G052")
                && responseBody.Contains("RetailOfferError")
                && responseBody.Contains("Delta services");
        }

        private static string BuildDeepLinkFailureBucketDiagnostic(HttpResponseMessage response)
        {
            string responseBody = response.ReadResponseAsString();

            try
            {
                JObject responseJson = JObject.Parse(responseBody);
                string downstreamResponse = responseJson.SelectToken("$.errors[0].extensions.response")?.ToString() ?? string.Empty;
                string downstreamId = string.Empty;

                if (!downstreamResponse.IsNullOrWhiteSpace())
                {
                    JArray downstreamErrors = JArray.Parse(downstreamResponse);
                    downstreamId = downstreamErrors.FirstOrDefault()?["id"]?.ToString() ?? string.Empty;
                }

                string reason = responseJson.SelectToken("$.errors[0].extensions.reason")?.ToString()
                    ?? responseJson.SelectToken("$.errors[0].extensions.message")?.ToString()
                    ?? "Unprocessable Entity";

                return $"DownstreamId={downstreamId}, Reason={reason}";
            }
            catch (JsonException)
            {
                return "Unable to parse downstream error payload.";
            }
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

        private static string BuildOfferPriceRequest(JourneyType journeyType, string basketId, string offerId)
        {
            TestConfig config = new TestConfig();
            config.Search.JourneyType = journeyType;
            config.Internal.BasketId = basketId;

            return OfferSearchHelper.OfferPriceNewGQLRequest(config, offerId);
        }

        private static TestConfig BuildLinkConfig(FareFamilyType fareFamilyType, FlightType flightType, bool isReturnJourney)
        {
            TestConfig config = new TestConfig();

            if (isReturnJourney)
            {
                config.Search.JourneyType = JourneyType.Return;
                config.Search.InboundFareFamilyType = fareFamilyType;
                config.Search.InboundFlightType = flightType;
            }
            else
            {
                config.Search.JourneyType = JourneyType.OneWay;
                config.Search.FareFamilyType = fareFamilyType;
                config.Search.FlightType = flightType;
            }

            return config;
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
