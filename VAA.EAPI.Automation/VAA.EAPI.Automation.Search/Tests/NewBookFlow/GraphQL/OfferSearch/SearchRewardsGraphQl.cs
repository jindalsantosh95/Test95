using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Requests.Endpoints;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.WebBFF;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public partial class SearchRewardsGraphQl : IClassFixture<TokenFixture>
    {
        private static string env = Config.Environment;
        private static readonly Lazy<string> sharedRewardB2CToken = new Lazy<string>(BuildSharedRewardB2CToken);
        private readonly TokenFixture tokenFixture;

        public SearchRewardsGraphQl(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC01_SummarySufficientLoyaltyPointsCheck_OneWay()
        {
            //Retrieving loyalty account details from storage table
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Gold", 80000, out string b2cToken);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(!string.IsNullOrEmpty(offerPriceResult?.data?.searchSummary?.id?.Value), "No offers returned for summary");

            //getting details from Vred like total points
            HttpResponseMessage vRedResponse = MicroserviceEndpoint.Vred(GetVRedToken(), fcMembershipNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(vRedResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateLoyaltyPointsEligibility(offerPriceResult, vRedResult, out string hasSufficientPoints), hasSufficientPoints);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC02_SummarySufficientLoyaltyPointsCheck_Return()
        {
            //Retrieving loyalty account details from storage table
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Red", 100000, out string b2cToken);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JContainer)offerSearchReturnResult.data.searchOffersNextSlice.result).Count > 0, "No result returned for the search details provided");

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(!string.IsNullOrEmpty(offerPriceResult?.data?.searchSummary?.id?.Value), "No offers returned for summary");

            //getting details from Vred like total points
            HttpResponseMessage vRedResponse = MicroserviceEndpoint.Vred(GetVRedToken(), fcMembershipNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(vRedResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateLoyaltyPointsEligibility(offerPriceResult, vRedResult, out string hasSufficientPoints), hasSufficientPoints);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC03_SummaryInsufficientLoyaltyPointsCheck_OneWay()
        {
            //Retrieving loyalty account details from storage table
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Silver", 10000, out string b2cToken);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(!string.IsNullOrEmpty(offerPriceResult?.data?.searchSummary?.id?.Value), "No offers returned for summary");

            //getting details from Vred like total points
            HttpResponseMessage vRedResponse = MicroserviceEndpoint.Vred(GetVRedToken(), fcMembershipNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(vRedResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateLoyaltyPointsEligibility(offerPriceResult, vRedResult, out string hasInsufficientPoints), hasInsufficientPoints);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void RewardSearch_TC04_UpdatesSummaryUrlAndFCNumberInBasket_OneWay()
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();

            //Retrieving loyalty account details from storage table
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Gold", 80000, out string b2cToken);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResponse.ReadResponseAsString(), FareFamilyType.AWARDECONOMY, FlightType.VS);
            if (!summaryLinks.Any())
            {
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchResponse.ReadResponseAsString(), FareFamilyType.AWARDECONOMY);
                if (fallbackSummaryLinks.Any())
                {
                    summaryLinks = fallbackSummaryLinks;
                }
                else
                {
                    summaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResponse.ReadResponseAsString(), "summary");
                }
            }

            Assert.True(summaryLinks.Any(), $"Summary link not returned for {FareFamilyType.AWARDECONOMY} and flight type {FlightType.VS}");

            //Getting basketId,slice and offerId from the summary url
            WebBFFHelper.SummaryURL(summaryLinks[0], out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            string summaryUrlLink = Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/search/summary/{basketId}/{slice}|{offerId}");

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId, summaryUrlLink);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(!string.IsNullOrEmpty(offerPriceResult?.data?.searchSummary?.id?.Value), "No offers returned for summary");

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.RetrieveBasket(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateSummaryFCNumberInBasket(retrieveBasketResult, fcMembershipNumber, out string fcNumberInfo), fcNumberInfo);

            Assert.True(GraphQlAssertions.ValidateSummaryLinkInBasket(retrieveBasketResult, summaryUrlLink, out string summaryUrlLinkInfo), summaryUrlLinkInfo);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void RewardSearch_TC05_SummaryLinkAndFCNumberUpdatesInBasket_Return()
        {
            //Retrieving loyalty account details from storage table
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Red", 150000, out string b2cToken);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId: basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JContainer)offerSearchReturnResult.data.searchOffersNextSlice.result).Count > 0, "No result returned for the search details provided");

            string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerReturnSearchResponse.ReadResponseAsString(), FareFamilyType.AWARDECONOMY, FlightType.VS);
            if (!summaryLinks.Any())
            {
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerReturnSearchResponse.ReadResponseAsString(), FareFamilyType.AWARDECONOMY);
                if (fallbackSummaryLinks.Any())
                {
                    summaryLinks = fallbackSummaryLinks;
                }
                else
                {
                    summaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerReturnSearchResponse.ReadResponseAsString(), "summary");
                }
            }

            Assert.True(summaryLinks.Any(), $"Summary link not returned for {FareFamilyType.AWARDECONOMY} and flight type {FlightType.VS}");

            //Getting basketId,slice and offerId from the summary url
            WebBFFHelper.SummaryURL(summaryLinks[0], out basketId, out string slice, out string offerReturnId);

            //Need to build url for Summary link from returned url
            string summaryUrlLink = Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/search/summary/{basketId}/{slice}|{offerReturnId}");

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId, summaryUrlLink);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(!string.IsNullOrEmpty(offerPriceResult?.data?.searchSummary?.id?.Value), "No offers returned for summary");

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.RetrieveBasket(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateSummaryFCNumberInBasket(retrieveBasketResult, fcMembershipNumber, out string fcNumberInfo), fcNumberInfo);

            Assert.True(GraphQlAssertions.ValidateSummaryLinkInBasket(retrieveBasketResult, summaryUrlLink, out string summaryUrlLinkInfo), summaryUrlLinkInfo);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(OfferSearchGraphQl.VaFlightsSearchTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void SearchRewardGraphQl_TC06_OneWay_MultipleRoutes_ValidatesSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: GetSharedRewardB2CToken()).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndPoints), totalTaxAndPoints);

            softAssert.True(GraphQlAssertions.ValidateEarnPoints(offerPriceResult, offerSearchGQLRequest, out string earnPoints, isAwardSearch: true), earnPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [MemberData(nameof(OfferSearchGraphQl.VaFlightsSearchTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void SearchRewardGraphQl_TC07_ReturnTrip_MultipleRoutes_ValidatesSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: GetSharedRewardB2CToken()).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, rewardSearch: true, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndPoints, offerReturnSearchResponse, offerReturnId), totalTaxAndPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 1, 1, 1, 0, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        [InlineData(LocationType.LHR, LocationType.DEL, 3, 0, 0, 1, 1, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        public void SearchRewardGraphQl_TC08_OneWay_MultiSegmentFilter_ValidatesSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, int stopCounts, FareFamilyType fareFamily, FlightType flightType)
        {
            string b2cToken = GetSharedRewardB2CToken();
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql filter request.
            string offerSearchFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", connectionFilter: stopCounts.ToString(), rewardSearch: true, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchFilterRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();
            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchFilterResponse))
            {
                Assert.True(
                    GraphQlAssertions.HasGraphQlErrors(offerSearchFilterResponse),
                    $"Filtered reward search returned an invalid payload shape without GraphQL errors. Request: {offerSearchFilterRequest}. Response: {offerSearchFilterResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string offerSearchResult = offerSearchFilterResponse.ReadResponseAsString();

            string[] offerLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchResult, fareFamily, flightType);
            if (!offerLinks.Any())
            {
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchResult, fareFamily);
                if (fallbackSummaryLinks.Any())
                {
                    offerLinks = fallbackSummaryLinks;
                }
                else
                {
                    string[] fallbackFareLinksForRequestedAirline = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchResult, fareFamily, flightType);
                    if (fallbackFareLinksForRequestedAirline.Any())
                    {
                        offerLinks = fallbackFareLinksForRequestedAirline;
                    }
                    else
                    {
                        string[] fallbackFareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchResult, fareFamily);
                        if (fallbackFareLinks.Any())
                        {
                            offerLinks = fallbackFareLinks;
                        }
                        else
                        {
                            offerLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResult, "summary");
                            if (!offerLinks.Any())
                            {
                                offerLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResult, "fare");
                            }
                        }
                    }
                }
            }

            if (!offerLinks.Any())
            {
                WriteAcceptedNoFilteredRewardOffers(
                    testName: nameof(SearchRewardGraphQl_TC08_OneWay_MultiSegmentFilter_ValidatesSummary),
                    origin: origin,
                    destination: destination,
                    stopCounts: stopCounts,
                    fareFamily: fareFamily,
                    flightType: flightType,
                    responseBody: offerSearchResult);
                return;
            }

            //getting offer id from selected offer link (summary preferred, fare fallback allowed)
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(offerLinks, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchFilterRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.OneWay, offerSearchFilterResponse, offerPriceResponse, offerId, out string totalTaxAndPoints), totalTaxAndPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, 0, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, 1, FareFamilyType.AWARDECONOMY, FlightType.AF)]
        public void SearchRewardGraphQl_TC09_ReturnTrip_MultiSegmentFilter_ValidatesInboundAndSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, int stopCounts, FareFamilyType fareFamily, FlightType flightType)
        {
            string b2cToken = GetSharedRewardB2CToken();
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql filter request.
            string offerSearchFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", connectionFilter: stopCounts.ToString(), rewardSearch: true, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchFilterRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string offerSearchOutboundResult = offerSearchFilterResponse.ReadResponseAsString();

            string[] fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(offerSearchOutboundResult, fareFamily, flightType);
            if (!fareLinks.Any())
            {
                string[] fallbackFareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchOutboundResult, fareFamily);
                if (fallbackFareLinks.Any())
                {
                    fareLinks = fallbackFareLinks;
                }
                else
                {
                    fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchOutboundResult, "fare");
                }
            }

            if (!fareLinks.Any())
            {
                WriteAcceptedNoFilteredRewardOffers(
                    testName: nameof(SearchRewardGraphQl_TC09_ReturnTrip_MultiSegmentFilter_ValidatesInboundAndSummary),
                    origin: origin,
                    destination: destination,
                    stopCounts: stopCounts,
                    fareFamily: fareFamily,
                    flightType: flightType,
                    responseBody: offerSearchOutboundResult,
                    slice: "outbound");
                return;
            }

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search return graph ql filter request.
            string offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, locationFive: LocationType.None, locationSix: LocationType.None, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: false, calenderSearch: false, offerId, pqId: "searchOffersNextBFF", connectionFilter: stopCounts.ToString(), rewardSearch: true, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId, b2cToken).EnsureGQLResponseSuccess();

            string offerSearchInboundResult = offerReturnSearchResponse.ReadResponseAsString();

            string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchInboundResult, fareFamily, flightType);
            if (!summaryLinks.Any())
            {
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(offerSearchInboundResult, fareFamily);
                if (fallbackSummaryLinks.Any())
                {
                    summaryLinks = fallbackSummaryLinks;
                }
                else
                {
                    string[] fallbackFareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(offerSearchInboundResult, fareFamily);
                    if (fallbackFareLinks.Any())
                    {
                        summaryLinks = fallbackFareLinks;
                    }
                    else
                    {
                        summaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchInboundResult, "summary");
                        if (!summaryLinks.Any())
                        {
                            summaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchInboundResult, "fare");
                        }
                    }
                }
            }

            if (!summaryLinks.Any())
            {
                WriteAcceptedNoFilteredRewardOffers(
                    testName: nameof(SearchRewardGraphQl_TC09_ReturnTrip_MultiSegmentFilter_ValidatesInboundAndSummary),
                    origin: destination,
                    destination: origin,
                    stopCounts: stopCounts,
                    fareFamily: fareFamily,
                    flightType: flightType,
                    responseBody: offerSearchInboundResult,
                    slice: "inbound");
                return;
            }

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchFilterRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.Return, offerSearchFilterResponse, offerPriceResponse, offerId, out string totalTaxAndPoints, offerReturnSearchResponse, offerReturnId), totalTaxAndPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LON, LocationType.JFK, 2, 0, 2, 0)]
        [InlineData(LocationType.LON, LocationType.MIA, 1, 1, 0, 1)]
        [InlineData(LocationType.NYC, LocationType.LHR, 1, 1, 1, 1)]
        public void SearchRewardGraphQl_TC10_ReturnTrip_CityCodeFlow_ValidatesSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDateForFlexiSearch = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDateForFlexiSearch, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest).EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: GetSharedRewardB2CToken()).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndPoints, offerReturnSearchResponse, offerReturnId), totalTaxAndPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 2, 0)]
        [InlineData(LocationType.MAN, LocationType.MCO, 1, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 1, 1, 1)]
        public void SearchRewardGraphQl_TC11_ReturnTrip_ValidatesSummary(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDateForFlexiSearch = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDateForFlexiSearch, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest).EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: GetSharedRewardB2CToken()).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndPointsInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndPoints, offerReturnSearchResponse, offerReturnId), totalTaxAndPoints);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.DEL, LocationType.DEL, LocationType.LHR })] //Same Origin and Destination
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.JFK })] //Origin and Destination different
        public void SearchRewardGraphQl_TC13_MultiCityThreeSlices_DirectVirginFlow_ValidatesSummary(LocationType[] locations)
        {
            string b2cToken = GetSharedRewardB2CToken();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchResult, FlightType.VS);
            if (!fareLinks.Any())
            {
                string[] fallbackFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResult);
                Assert.True(fallbackFareLinks.Any(), $"Slice - 0: No fare links were found matching the requested criteria. Response: {offerSearchResult}");
                fareLinks = fallbackFareLinks;
            }

            Assert.True(fareLinks.Any(), $"Slice - 0: No direct flights were found matching the requested criteria. Response: {offerSearchResult}");

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            string offerSearchSliceOneResult = offerSearchSliceOneResponse.ReadResponseAsString();

            string[] fareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.VS);
            if (!fareLinksSliceOne.Any())
            {
                string[] fallbackFareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceOneResult);
                Assert.True(fallbackFareLinksSliceOne.Any(), $"Slice - 1: No fare links were found matching the requested criteria. Response: {offerSearchSliceOneResult}");
                fareLinksSliceOne = fallbackFareLinksSliceOne;
            }

            Assert.True(fareLinksSliceOne.Any(), $"Slice - 1: No direct flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinksSliceOne, randomOffer: true);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            string offerSearchSliceTwoResult = offerSearchSliceTwoResponse.ReadResponseAsString();

            string[] summaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.VS, "summary");
            if (!summaryLinksSliceTwo.Any())
            {
                string[] fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceTwoResult, "summary");
                Assert.True(fallbackSummaryLinksSliceTwo.Any(), $"Slice - 2: No summary links were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");
                summaryLinksSliceTwo = fallbackSummaryLinksSliceTwo;
            }

            Assert.True(summaryLinksSliceTwo.Any(), $"Slice - 2: No direct flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinksSliceTwo, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildMultiCityRewardOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.LHR })]
        public void SearchRewardGraphQl_TC14_MultiCityThreeSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            string b2cToken = GetSharedRewardB2CToken();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1 request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildMultiCityRewardOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.LHR, LocationType.LHR, LocationType.DEL })]
        public void SearchRewardGraphQl_TC15_MultiCityFourSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            string b2cToken = GetSharedRewardB2CToken();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice3Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceTwoOfferId, basketId: basketId, currentTripIndexId: "3", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice3Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice3Request, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceThreeOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice3Response);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildMultiCityRewardOfferPriceRequest(basketId, sliceThreeOfferId, currentTripIndexId: 4);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.LHR, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.CDG })]
        public void SearchRewardGraphQl_TC16_MultiCityFiveSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            string b2cToken = GetSharedRewardB2CToken();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice3Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceTwoOfferId, basketId: basketId, currentTripIndexId: "3", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice3Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice3Request, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceThreeOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice3Response);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice4Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceThreeOfferId, basketId: basketId, currentTripIndexId: "4", rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice4Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice4Request, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceFourOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice4Response);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildMultiCityRewardOfferPriceRequest(basketId, sliceFourOfferId, currentTripIndexId: 5);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC17_OneWay_LoggedInVoucherAccountReturnsCompanionVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            string departureDate = OfferSearchHelper.DepartureDate();
            DateTime outboundDate = departureDate.Parse<DateTime>();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                minimumVoucherExpiryDate: outboundDate);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                departureDate: departureDate);
            JObject offerPriceResult = searchResult.SearchSummaryResult;
            JObject retrieveBasketResult = searchResult.RetrieveBasketResult;
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetails(offerPriceResult, FilterVoucherPayloadByMinimumExpiryDate(loyaltyDetails["vouchers"], outboundDate), "COMPANION", expectedPassengerCount: 2, out string voucherDetailsMessage, validateVoucherAmountCalculations: shouldValidateBasketVoucherDetails),
                $"Member {loyaltyDetails["maskedRowKey"]} companion voucher redemption details were not returned in reward search summary. {voucherDetailsMessage}");

            if (shouldValidateBasketVoucherDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsPersistedInBasket(offerPriceResult, retrieveBasketResult, out string basketVoucherDetailsMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} companion voucher details were not persisted in basket after reward search summary. {basketVoucherDetailsMessage}");
            }
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC18_ReturnTrip_LoggedInVoucherAccountReturnsCompanionVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            JObject offerPriceResult = ExecuteReturnRewardVoucherSummarySearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                out JObject retrieveBasketResult);
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetails(offerPriceResult, loyaltyDetails["vouchers"], "COMPANION", expectedPassengerCount: 2, out string voucherDetailsMessage, validateVoucherAmountCalculations: shouldValidateBasketVoucherDetails),
                $"Member {loyaltyDetails["maskedRowKey"]} return companion voucher redemption details were not returned in reward search summary. {voucherDetailsMessage}");

            if (shouldValidateBasketVoucherDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsPersistedInBasket(offerPriceResult, retrieveBasketResult, out string basketVoucherDetailsMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} return companion voucher details were not persisted in basket after reward search summary. {basketVoucherDetailsMessage}");
            }
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC19_OneWay_LoggedInVoucherAccountReturnsUpgradeVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            AssertUpgradeVoucherSummary(
                JourneyType.OneWay,
                shouldValidateBasketVoucherDetails,
                "upgrade voucher redemption details were not returned in reward search summary");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC20_OneWay_LoggedInNonVoucherAccountDoesNotReturnVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveNonVoucherLoyaltyDetailsAndGenerateB2CToken("Red", 0, out string b2cToken);
            JObject offerPriceResult = ExecuteOneWayRewardVoucherSummarySearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                out JObject retrieveBasketResult);
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetailsNotReturned(offerPriceResult, out string voucherDetailsMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} should not have voucher redemption details in reward search summary. {voucherDetailsMessage}");

            if (shouldValidateBasketVoucherDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsNotPersistedInBasket(retrieveBasketResult, out string basketVoucherDetailsMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} should not have voucher redemption details persisted in basket. {basketVoucherDetailsMessage}");
            }
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC21_OneWay_LoggedInVoucherAccountWithOnePassengerDoesNotReturnCompanionVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            JObject offerPriceResult = ExecuteOneWayRewardVoucherSummarySearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 1,
                includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                out _);
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherRedemptionTypeNotReturned(offerPriceResult, "COMPANION", out string voucherDetailsMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} should not have companion voucher redemption details for a one-passenger reward search. {voucherDetailsMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC22_ReturnTrip_LoggedInVoucherAccountReturnsUpgradeVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            AssertUpgradeVoucherSummary(
                JourneyType.Return,
                shouldValidateBasketVoucherDetails,
                "return upgrade voucher redemption details were not returned in reward search summary");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC23_OneWay_SelectedVoucherOptionUpdatesSummaryAmounts()
        {
            if (!ShouldRunRewardVoucherSelectionValidation())
                return;

            bool shouldValidatePaymentBasket = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                exactVoucherCount: 2);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: true,
                retrieveVoucherBasketDetails: shouldValidatePaymentBasket);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetails(
                    searchResult.SearchSummaryResult,
                    FilterVoucherPayloadByMinimumExpiryDate(
                        loyaltyDetails["vouchers"],
                        searchResult.OutboundDepartureDate.Parse<DateTime>(),
                        maximumVoucherCount: 2),
                    "COMPANION",
                    expectedPassengerCount: 2,
                    out string voucherDetailsMessage,
                    validateVoucherAmountCalculations: true),
                $"Member {loyaltyDetails["maskedRowKey"]} companion voucher redemption details were not returned before explicit selection. {voucherDetailsMessage}");

            Assert.True(
                GraphQlAssertions.TrySelectRewardVoucherOption(searchResult.SearchSummaryResult, out JObject selectedOption, out List<string> selectedVoucherIds, out string selectedOptionMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not have a voucher option to select. {selectedOptionMessage}");

            string selectedOptionIdText = selectedOption.SelectToken("optionId")?.ToString() ?? string.Empty;
            Assert.True(int.TryParse(selectedOptionIdText, out int _), $"Selected voucher option id was not numeric. SelectedOption: {selectedOption}");

            string selectionGQLRequest = OfferSearchHelper.SetVoucherRedemptionSelectionGQLRequest(searchResult.BasketId, selectedVoucherIds);
            HttpResponseMessage selectionResponse = GraphQLEndpoint.SetVoucherRedemptionSelection(selectionGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            JObject selectionResult = JObject.Parse(selectionResponse.ReadResponseAsString());

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherSelectionMutationResponse(selectionResult, selectedOption, selectedVoucherIds, out string selectionMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} selected voucher option was not persisted. {selectionMessage}");

            if (shouldValidatePaymentBasket)
            {
                JObject basketAfterSelectionResult = RetrieveBasketAfterVoucherSummary(searchResult.BasketId, b2cToken);
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherSelectionPersistedInPaymentBasket(selectionResult, basketAfterSelectionResult, out string basketSelectionMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} selected voucher amounts and points state were not persisted for the payment journey. {basketSelectionMessage}");
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherPaymentAvailabilitySignals(selectionResult, basketAfterSelectionResult, expectedHasSufficientPoints: true, out string paymentAvailabilityMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} did not receive the backend signals required to continue to card payment after voucher selection. {paymentAvailabilityMessage}");
            }

            string refreshedSummaryGQLRequest = BuildRewardOfferPriceRequest(
                JourneyType.OneWay,
                searchResult.BasketId,
                searchResult.OfferId,
                searchResult.SummaryLink,
                includeVoucherDetails: true,
                includeVoucherBasketDetails: true);
            HttpResponseMessage refreshedSummaryResponse = GraphQLEndpoint.OfferPrice(refreshedSummaryGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            JObject refreshedSummaryResult = JObject.Parse(refreshedSummaryResponse.ReadResponseAsString());
            string availableAwardPoints = loyaltyDetails.TryGetValue("points", out string points) ? points : string.Empty;

            Assert.True(
                GraphQlAssertions.ValidateSelectedRewardVoucherOptionAmounts(refreshedSummaryResult, selectedOption, availableAwardPoints, out string amountMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} selected voucher option amounts were not reflected in refreshed search summary. {amountMessage}");

            string deselectionGQLRequest = OfferSearchHelper.SetVoucherRedemptionSelectionGQLRequest(searchResult.BasketId, Array.Empty<string>());
            HttpResponseMessage deselectionResponse = GraphQLEndpoint.SetVoucherRedemptionSelection(deselectionGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            JObject deselectionResult = JObject.Parse(deselectionResponse.ReadResponseAsString());

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDeselectionMutationResponse(deselectionResult, out string deselectionMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} deselected voucher option was not cleared by setVoucherRedemptionSelection. {deselectionMessage}");

            if (shouldValidatePaymentBasket)
            {
                JObject basketAfterDeselectionResult = RetrieveBasketAfterVoucherSummary(searchResult.BasketId, b2cToken);
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDeselectionPersistedInBasket(basketAfterDeselectionResult, out string basketDeselectionMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} deselected voucher option was not cleared in retrieveBasket. {basketDeselectionMessage}");
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherPaymentAvailabilitySignals(deselectionResult, basketAfterDeselectionResult, expectedHasSufficientPoints: true, out string paymentAvailabilityMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} did not receive the backend signals required to restore the existing reward/card flow after voucher removal. {paymentAvailabilityMessage}");
            }
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC24_OneWay_UpperCabinCompanionVoucherInsufficientPointsEnablesBuyPoints()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            string departureDate = OfferSearchHelper.DepartureDate();
            DateTime outboundDate = departureDate.Parse<DateTime>();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                maximumPoints: GetRewardVoucherInsufficientPointsMaximumBalance(),
                minimumVoucherExpiryDate: outboundDate);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDBUSINESSFIRST,
                numberOfAdults: 2,
                includeVoucherBasketDetails: true,
                selectHighestAwardPointsFare: true,
                departureDate: departureDate);
            JObject offerPriceResult = searchResult.SearchSummaryResult;
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherInsufficientPointsBuyPointsState(
                    offerPriceResult,
                    FilterVoucherPayloadByMinimumExpiryDate(loyaltyDetails["vouchers"], outboundDate),
                    "COMPANION",
                    expectedPassengerCount: 2,
                    loyaltyDetails["points"],
                out string voucherDetailsMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} companion voucher insufficient-points summary did not enable Buy Virgin Points. {voucherDetailsMessage}");
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherPaymentAvailabilitySignals(offerPriceResult, searchResult.RetrieveBasketResult, expectedHasSufficientPoints: false, out string paymentAvailabilityMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive consistent insufficient-points signals for the Companion payment journey. {paymentAvailabilityMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC25_OneWay_PremiumCabinThreeAdultsUpgradeVoucherInsufficientPointsEnablesBuyPoints()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            DateTime outboundDate = DateTime.UtcNow.Date.AddMonths(1);
            string departureDate = outboundDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                maximumPoints: GetRewardVoucherInsufficientPointsMaximumBalance(),
                minimumVoucherExpiryDate: outboundDate,
                exactVoucherCount: 2,
                requireAllVouchersInExpiryRange: true);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY,
                numberOfAdults: 3,
                includeVoucherBasketDetails: true,
                selectHighestAwardPointsFare: true,
                departureDate: departureDate);
            JObject offerPriceResult = searchResult.SearchSummaryResult;
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                ValidateUpgradeIsBestValueAgainstCompanion(
                    offerPriceResult,
                    searchResult.BaselineCabinPoints,
                    searchResult.SelectedCabinPoints,
                    out string bestValueMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not produce a fare where Upgrade was independently proven as best value against Companion, including Upgrade tie precedence. {bestValueMessage}");
            Console.WriteLine($"TC25 independent best-value proof for member {loyaltyDetails["maskedRowKey"]}: {bestValueMessage}");

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherInsufficientPointsBuyPointsState(
                    offerPriceResult,
                    FilterVoucherPayloadByMinimumExpiryDate(loyaltyDetails["vouchers"], outboundDate),
                    "UPGRADE",
                    expectedPassengerCount: 3,
                    loyaltyDetails["points"],
                out string voucherDetailsMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} upgrade voucher insufficient-points summary did not enable Buy Virgin Points. BestValueProof: {bestValueMessage} {voucherDetailsMessage}");
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherPaymentAvailabilitySignals(offerPriceResult, searchResult.RetrieveBasketResult, expectedHasSufficientPoints: false, out string paymentAvailabilityMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive consistent insufficient-points signals for the Upgrade payment journey. {paymentAvailabilityMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC26_OneWay_UpperCabinReturnsUpgradeVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            AssertUpgradeVoucherSummary(
                JourneyType.OneWay,
                ShouldRunRewardVoucherBasketValidation(),
                "upper cabin upgrade voucher redemption details were not returned in reward search summary",
                FareFamilyType.AWARDBUSINESSFIRST);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC27_OneWay_AvailableVouchersDoNotExpireBeforeOutboundTravel()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            string outboundDepartureDate = OfferSearchHelper.DepartureDate();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                minimumVoucherExpiryDate: DateTime.Parse(outboundDepartureDate, CultureInfo.InvariantCulture));
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false,
                departureDate: outboundDepartureDate);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherOutboundExpiry(searchResult.SearchSummaryResult, searchResult.OutboundDepartureDate, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} received a voucher that was not valid for outbound travel. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC28_OneWay_NonVsOperatedFlightDoesNotReturnVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            LocationType[] candidateDestinations = [LocationType.JFK, LocationType.ATL, LocationType.BOS];
            const int availabilityWindowDays = 3;
            DateTime initialDepartureDate = OfferSearchHelper.DepartureDate().Parse<DateTime>();
            RewardVoucherSummarySearchResult searchResult = new RewardVoucherSummarySearchResult();
            List<string> attemptedSearches = new List<string>();

            foreach (LocationType destination in candidateDestinations)
            {
                for (int dayOffset = 0; dayOffset < availabilityWindowDays && !searchResult.SearchSummaryResult.HasValues; dayOffset++)
                {
                    string departureDate = initialDepartureDate.AddDays(dayOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    attemptedSearches.Add($"LHR-{destination} {departureDate}");
                    searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                        b2cToken,
                        LocationType.LHR,
                        destination,
                        FareFamilyType.AWARDECONOMY,
                        numberOfAdults: 2,
                        includeVoucherBasketDetails: false,
                        includeContinuationBasketDetails: true,
                        operatingCarrier: FlightType.DL,
                        departureDate: departureDate,
                        allowMissingOperatingCarrier: true);
                }

                if (searchResult.SearchSummaryResult.HasValues)
                    break;
            }

            Assert.True(
                searchResult.SearchSummaryResult.HasValues,
                $"No DL-operated outbound AWARDECONOMY availability was returned across the route/date discovery matrix. Attempts: {string.Join(", ", attemptedSearches)}.");

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetailsNotReturned(searchResult.SearchSummaryResult, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} received voucher redemption details for a non-VS operated flight. {message}");

            Assert.True(
                GraphQlAssertions.ValidateNonVoucherBasketContinuation(searchResult.RetrieveBasketResult, searchResult.BasketId, out string basketMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} could not continue through the non-voucher basket flow for a non-VS operated flight. {basketMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC29_OneWay_VouchersExpiringBeforeOutboundTravelAreExcluded()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            DateTime today = DateTime.UtcNow.Date;
            DateTime voucherExpiryCutoff = today.AddMonths(6);
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                minimumVoucherExpiryDate: today,
                voucherExpiryBeforeDate: voucherExpiryCutoff);
            DateTime voucherExpiryDate = GetVoucherExpiryWithinRange(loyaltyDetails["vouchers"], today, voucherExpiryCutoff);
            string outboundDepartureDate = voucherExpiryDate.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false,
                includeContinuationBasketDetails: true,
                departureDate: outboundDepartureDate);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateExpiredRewardVouchersExcluded(
                    searchResult.SearchSummaryResult,
                    loyaltyDetails["vouchers"],
                    searchResult.OutboundDepartureDate,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} received a voucher that expired before outbound travel. {message}");

            Assert.True(
                GraphQlAssertions.ValidateNonVoucherBasketContinuation(searchResult.RetrieveBasketResult, searchResult.BasketId, out string basketMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} could not continue through the non-voucher basket flow after voucher expiry. {basketMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC30_Return_MixedVsAndPartnerOperatedFlightsDoNotReturnVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            LocationType[] candidateDestinations = [LocationType.JFK, LocationType.ATL, LocationType.BOS];
            const int availabilityWindowDays = 3;
            DateTime initialDepartureDate = OfferSearchHelper.DepartureDate().Parse<DateTime>();
            JObject searchSummaryResult = new JObject();
            JObject retrieveBasketResult = new JObject();
            List<string> attemptedSearches = new List<string>();

            foreach (LocationType destination in candidateDestinations)
            {
                for (int dayOffset = 0; dayOffset < availabilityWindowDays && !searchSummaryResult.HasValues; dayOffset++)
                {
                    string departureDate = initialDepartureDate.AddDays(dayOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string returnDate = initialDepartureDate.AddDays(dayOffset + 7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    attemptedSearches.Add($"LHR-{destination} {departureDate}/{returnDate}");
                    searchSummaryResult = ExecuteReturnRewardVoucherSummarySearch(
                        b2cToken,
                        LocationType.LHR,
                        destination,
                        FareFamilyType.AWARDECONOMY,
                        numberOfAdults: 2,
                        includeVoucherBasketDetails: false,
                        out retrieveBasketResult,
                        includeContinuationBasketDetails: true,
                        outboundOperatingCarrier: FlightType.DL,
                        departureDate: departureDate,
                        returnDate: returnDate,
                        allowMissingOutboundCarrier: true,
                        allowMissingInboundCarrier: true);
                }

                if (searchSummaryResult.HasValues)
                    break;
            }

            Assert.True(
                searchSummaryResult.HasValues,
                $"No DL-operated outbound AWARDECONOMY availability was returned across the mixed-itinerary route/date discovery matrix. Attempts: {string.Join(", ", attemptedSearches)}.");
            if (!searchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetailsNotReturned(searchSummaryResult, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} received voucher redemption details for a mixed VS and partner-operated itinerary. {message}");
            Assert.True(
                GraphQlAssertions.ValidateNonVoucherBasketContinuation(retrieveBasketResult, out string basketMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} could not continue through the non-voucher basket flow for a mixed VS and partner-operated itinerary. {basketMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC31_Return_UpgradeIsBestOptionOnCompanionTieAndPersistsToPaymentBasket()
        {
            if (!ShouldRunRewardVoucherBasketValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            RewardVoucherSectorSearchResult searchResult = ExecuteReturnRewardVoucherSectorCalculationSearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                numberOfAdults: 2,
                includeVoucherBasketDetails: true);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateUpgradeSectorDeltasAndVoucherCap(
                    searchResult.SearchSummaryResult,
                    searchResult.OutboundEconomyPoints,
                    searchResult.OutboundSelectedCabinPoints,
                    searchResult.InboundEconomyPoints,
                    searchResult.InboundSelectedCabinPoints,
                    out string calculationMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} upgrade sector calculations did not produce the expected capped redemption. {calculationMessage}");
            Assert.True(
                GraphQlAssertions.ValidateUpgradePreferredOnCompanionTieAndPersisted(
                    searchResult.SearchSummaryResult,
                    searchResult.RetrieveBasketResult,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive Upgrade as the persisted best option for an Upgrade/Companion tie. {message}");

        }

        [Theory]
        [InlineData(LocationType.JFK)]
        [InlineData(LocationType.ATL)]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC32_Return_UpgradeVoucherUsesSumOfSelectedFlightSectorDeltas(LocationType destination)
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool includeVoucherBasketDetails = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            RewardVoucherSectorSearchResult searchResult = ExecuteReturnRewardVoucherSectorCalculationSearch(
                b2cToken,
                LocationType.LHR,
                destination,
                numberOfAdults: 2,
                includeVoucherBasketDetails: includeVoucherBasketDetails);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            bool calculationIsValid = GraphQlAssertions.ValidateUpgradeBaselinePointsAndVoucherCap(
                searchResult.SearchSummaryResult,
                out string message);
            Console.WriteLine(
                $"TC32 baseline calculation evidence for member {loyaltyDetails["maskedRowKey"]}. " +
                $"TravelDates: {searchResult.DepartureDate}/{searchResult.ReturnDate}. " +
                $"OutboundFlight: {searchResult.OutboundFlight}. InboundFlight: {searchResult.InboundFlight}. {message}");
            Assert.True(
                calculationIsValid,
                $"Member {loyaltyDetails["maskedRowKey"]} upgrade voucher was not calculated from the GQL baseline points for the selected LHR-{destination} return journey. OutboundFlight: {searchResult.OutboundFlight}. InboundFlight: {searchResult.InboundFlight}. {message}");

            if (includeVoucherBasketDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsPersistedInBasket(
                        searchResult.SearchSummaryResult,
                        searchResult.RetrieveBasketResult,
                        out string basketMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} sector-calculated upgrade voucher did not persist to the Payment basket. {basketMessage}");
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC33_OneWay_AllVouchersExpiredBeforeOutboundAreExcluded(int voucherCount)
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            AssertAllExpiredVoucherScenario(voucherCount);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC34_OneWay_OutboundBetweenTwoVoucherExpiriesReturnsOnlyLaterVoucher()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            (Dictionary<string, string> loyaltyDetails, string b2cToken) = RetrieveExpiryScenarioAccount(
                exactVoucherCount: 2,
                maximumExpiryMonths: 10,
                requireDifferentVoucherExpiryDates: true);
            DateTime outboundDate = GetVoucherExpiryDates(loyaltyDetails["vouchers"]).Min().AddDays(1);
            RewardVoucherSummarySearchResult searchResult = ExecuteExpiryScenarioSearch(b2cToken, outboundDate);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateEligibleRewardVouchersByOutboundDate(searchResult.SearchSummaryResult, loyaltyDetails["vouchers"], searchResult.OutboundDepartureDate, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive only the later-expiring voucher when outbound fell between two expiry dates. {message}");
            Assert.True(
                GraphQlAssertions.ValidateVoucherEligibilityIndicator(searchResult.SearchSummaryResult, expectedEligible: true, out string indicatorMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive the eligible-voucher indicator when the later voucher remained valid. {indicatorMessage}");
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetails(
                    searchResult.SearchSummaryResult,
                    FilterVoucherPayloadByMinimumExpiryDate(loyaltyDetails["vouchers"], outboundDate),
                    "COMPANION",
                    expectedPassengerCount: 2,
                    out string companionMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not use only the eligible voucher for the Companion booking. {companionMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC41_OneWay_TwoVouchersReturnsOnlyEligibleVoucherForUpgrade()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            (Dictionary<string, string> loyaltyDetails, string b2cToken) = RetrieveExpiryScenarioAccount(
                exactVoucherCount: 2,
                maximumExpiryMonths: 10,
                requireDifferentVoucherExpiryDates: true);
            DateTime outboundDate = GetVoucherExpiryDates(loyaltyDetails["vouchers"]).Min().AddDays(1);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDBUSINESSFIRST,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false,
                departureDate: outboundDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            Assert.True(
                searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable Premium reward summary for one-of-two Upgrade eligibility validation.");
            Assert.True(
                GraphQlAssertions.ValidateEligibleRewardVouchersByOutboundDate(searchResult.SearchSummaryResult, loyaltyDetails["vouchers"], searchResult.OutboundDepartureDate, out string eligibilityMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive only the eligible later-expiring voucher for Upgrade. {eligibilityMessage}");
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetails(
                    searchResult.SearchSummaryResult,
                    FilterVoucherPayloadByMinimumExpiryDate(loyaltyDetails["vouchers"], outboundDate),
                    "UPGRADE",
                    expectedPassengerCount: 2,
                    out string upgradeMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not use only the eligible voucher for the Upgrade booking. {upgradeMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC42_OneWay_ThreePassengersOneCompanionVoucherPricesAdditionalPassengerInPoints()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                exactVoucherCount: 1);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 3,
                includeVoucherBasketDetails: true);

            Assert.True(searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable three-passenger reward summary.");
            Assert.True(
                GraphQlAssertions.ValidateCompanionVoucherPassengerComposition(
                    searchResult.SearchSummaryResult,
                    expectedAppliedVoucherCount: 1,
                    expectedCoveredPassengerCount: 1,
                    requireRemainingPoints: true,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not retain the additional passenger in the standard points calculation. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC43_OneWay_ThreePassengersTwoVouchersCapsCompanionRedemptionAtOneVoucher()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                voucherType: "Virgin Red US credit card",
                exactVoucherCount: 2,
                requireAllVouchersMatchType: true);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 3,
                includeVoucherBasketDetails: false);

            Assert.True(searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable three-passenger reward summary.");
            Assert.True(
                GraphQlAssertions.ValidateCompanionVoucherCountIsCapped(
                    searchResult.SearchSummaryResult,
                    maximumAppliedVoucherCount: 1,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} was incorrectly allowed two Companion vouchers for only three passengers. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC44_OneWaySingleSector_FourPassengersTwoVouchersCapsCompanionRedemptionAtOneVoucher()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                voucherType: "Virgin Red US credit card",
                exactVoucherCount: 2,
                requireAllVouchersMatchType: true);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 4,
                includeVoucherBasketDetails: true);

            Assert.True(searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable four-passenger reward summary.");
            Assert.True(
                GraphQlAssertions.ValidateCompanionVoucherPassengerComposition(
                    searchResult.SearchSummaryResult,
                    expectedAppliedVoucherCount: 1,
                    expectedCoveredPassengerCount: 2,
                    requireRemainingPoints: true,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive the confirmed one-primary/one-companion pairing for the direct single-sector journey. {message}");
            Assert.True(
                GraphQlAssertions.ValidateCompanionVoucherCountIsCapped(
                    searchResult.SearchSummaryResult,
                    maximumAppliedVoucherCount: 1,
                    out string capMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} was offered a prohibited multi-voucher Companion option for the direct single-sector journey. {capMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC45_ReturnTwoSectors_FourPassengersTwoVouchersCoverOnePrimaryCompanionPair()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                voucherType: "Virgin Red US credit card",
                exactVoucherCount: 2,
                requireAllVouchersMatchType: true);
            JObject searchSummaryResult = ExecuteReturnRewardVoucherSummarySearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 4,
                includeVoucherBasketDetails: false,
                out _);

            Assert.True(searchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable four-passenger return reward summary.");
            Assert.True(
                GraphQlAssertions.ValidateCompanionVoucherPassengerComposition(
                    searchSummaryResult,
                    expectedAppliedVoucherCount: 2,
                    expectedCoveredPassengerCount: 2,
                    requireRemainingPoints: true,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} was not offered the valid one-primary/one-companion pairing across both sectors of the return journey. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC35_OneWay_OutboundOnVoucherExpiryDateRemainsEligible()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            (Dictionary<string, string> loyaltyDetails, string b2cToken) = RetrieveExpiryScenarioAccount(exactVoucherCount: 1);
            DateTime outboundDate = GetVoucherExpiryDates(loyaltyDetails["vouchers"]).Single();
            RewardVoucherSummarySearchResult searchResult = ExecuteExpiryScenarioSearch(b2cToken, outboundDate);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateEligibleRewardVouchersByOutboundDate(searchResult.SearchSummaryResult, loyaltyDetails["vouchers"], searchResult.OutboundDepartureDate, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a voucher valid on the outbound expiry-date boundary. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC36_Return_InboundAfterVoucherExpiryRemainsEligibleWhenOutboundIsValid()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            (Dictionary<string, string> loyaltyDetails, string b2cToken) = RetrieveExpiryScenarioAccount(exactVoucherCount: 1);
            DateTime expiryDate = GetVoucherExpiryDates(loyaltyDetails["vouchers"]).Single();
            string outboundDate = expiryDate.AddDays(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string inboundDate = expiryDate.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            JObject searchSummaryResult = ExecuteReturnRewardVoucherSummarySearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false,
                out _,
                departureDate: outboundDate,
                returnDate: inboundDate,
                includeVoucherEligibility: true);
            if (!searchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateEligibleRewardVouchersByOutboundDate(searchSummaryResult, loyaltyDetails["vouchers"], outboundDate, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} lost voucher eligibility because inbound {inboundDate} was after expiry {expiryDate:yyyy-MM-dd}, despite valid outbound {outboundDate}. {message}");
            Assert.True(
                GraphQlAssertions.ValidateVoucherEligibilityIndicator(searchSummaryResult, expectedEligible: true, out string indicatorMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive an eligible indicator when outbound was within expiry and only inbound was outside. {indicatorMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC37_Return_InboundPartnerOperatedFlightDoesNotReturnVoucherSummary()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            LocationType[] candidateDestinations = [LocationType.JFK, LocationType.ATL, LocationType.BOS];
            const int availabilityWindowDays = 3;
            DateTime initialDepartureDate = OfferSearchHelper.DepartureDate().Parse<DateTime>();
            JObject searchSummaryResult = new JObject();
            JObject retrieveBasketResult = new JObject();
            List<string> attemptedSearches = new List<string>();

            foreach (LocationType destination in candidateDestinations)
            {
                for (int dayOffset = 0; dayOffset < availabilityWindowDays && !searchSummaryResult.HasValues; dayOffset++)
                {
                    string departureDate = initialDepartureDate.AddDays(dayOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string returnDate = initialDepartureDate.AddDays(dayOffset + 7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    attemptedSearches.Add($"LHR-{destination} {departureDate}/{returnDate}");
                    searchSummaryResult = ExecuteReturnRewardVoucherSummarySearch(
                        b2cToken,
                        LocationType.LHR,
                        destination,
                        FareFamilyType.AWARDECONOMY,
                        numberOfAdults: 2,
                        includeVoucherBasketDetails: false,
                        out retrieveBasketResult,
                        includeContinuationBasketDetails: true,
                        inboundOperatingCarrier: FlightType.DL,
                        departureDate: departureDate,
                        returnDate: returnDate,
                        allowMissingInboundCarrier: true);
                }

                if (searchSummaryResult.HasValues)
                    break;
            }

            Assert.True(
                searchSummaryResult.HasValues,
                $"No DL-operated inbound AWARDECONOMY availability was returned across the route/date discovery matrix. Attempts: {string.Join(", ", attemptedSearches)}.");
            if (!searchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetailsNotReturned(searchSummaryResult, out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} received voucher redemption details for a partner-operated inbound flight. {message}");
            Assert.True(
                GraphQlAssertions.ValidateNonVoucherBasketContinuation(retrieveBasketResult, out string basketMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} could not continue without vouchers for a partner-operated inbound flight. {basketMessage}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC38_Return_UpgradeRemainsBestTieOptionAfterVoucherSelectionAndDeselection()
        {
            if (!ShouldRunRewardVoucherBasketValidation() || !ShouldRunRewardVoucherSelectionValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            RewardVoucherSectorSearchResult searchResult = ExecuteReturnRewardVoucherSectorCalculationSearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                numberOfAdults: 2,
                includeVoucherBasketDetails: true);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateUpgradeSectorDeltasAndVoucherCap(
                    searchResult.SearchSummaryResult,
                    searchResult.OutboundEconomyPoints,
                    searchResult.OutboundSelectedCabinPoints,
                    searchResult.InboundEconomyPoints,
                    searchResult.InboundSelectedCabinPoints,
                    out string calculationMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} tie recalculation did not start from an independently validated Upgrade sector calculation. {calculationMessage}");

            AssertUpgradeTieRecalculation(searchResult, b2cToken, loyaltyDetails["maskedRowKey"]);
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC39_OneWay_MoreThanTwoVouchersReturnsTwoEarliestEligibleVouchers()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                minimumVoucherCount: 3,
                requireDifferentVoucherExpiryDates: true);
            RewardVoucherSummarySearchResult searchResult = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false);
            Assert.True(
                searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable LHR-JFK Economy reward summary for earliest-voucher validation.");

            Assert.True(
                GraphQlAssertions.ValidateTwoEarliestEligibleRewardVouchers(
                    searchResult.SearchSummaryResult,
                    loyaltyDetails["vouchers"],
                    searchResult.OutboundDepartureDate,
                    out string message),
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive exactly the two earliest-expiring eligible vouchers. {message}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void SearchRewardGraphQl_TC40_Return_OutboundEconomyInboundPremiumAppliesUpgradeToInboundOnly()
        {
            if (!ShouldRunRewardVoucherValidation())
                return;

            bool shouldValidateBasketVoucherDetails = ShouldRunRewardVoucherBasketValidation();
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            RewardVoucherSectorSearchResult searchResult = ExecuteReturnRewardVoucherSectorCalculationSearch(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                numberOfAdults: 1,
                includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                outboundFareFamily: FareFamilyType.AWARDECONOMY,
                inboundFareFamily: FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY);
            Assert.True(
                searchResult.SearchSummaryResult.HasValues,
                $"Member {loyaltyDetails["maskedRowKey"]} did not receive a usable LHR-JFK mixed-cabin reward summary.");

            Assert.True(
                GraphQlAssertions.ValidateMixedCabinSelection(
                    searchResult.SearchSummaryResult,
                    FareFamilyType.AWARDECONOMY,
                    FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY,
                    out string cabinMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} mixed-cabin selection was not returned correctly. {cabinMessage}");
            Assert.True(
                GraphQlAssertions.ValidateUpgradeSectorDeltasAndVoucherCap(
                    searchResult.SearchSummaryResult,
                    searchResult.OutboundEconomyPoints,
                    searchResult.OutboundSelectedCabinPoints,
                    searchResult.InboundEconomyPoints,
                    searchResult.InboundSelectedCabinPoints,
                    out string calculationMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} inbound-only Upgrade did not use only the selected direction's cabin delta. {calculationMessage}");

            if (shouldValidateBasketVoucherDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsPersistedInBasket(
                        searchResult.SearchSummaryResult,
                        searchResult.RetrieveBasketResult,
                        out string basketMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} inbound-only Upgrade details were not persisted for payment. {basketMessage}");
            }
        }

        private static bool ShouldRunRewardVoucherValidation()
        {
            // Voucher summary backend changes are environment-gated so newly promoted code can be validated before widening daily coverage.
            return ConfigReader.Get("SearchRewardVoucherValidationEnabled", "true").Is("true")
                && IsRewardVoucherValidationEnvironmentAllowed();
        }

        private static bool ShouldRunRewardVoucherBasketValidation()
        {
            // PR #1592 adds basket persistence fields; keep this opt-in until the target environment exposes them.
            return ConfigReader.Get("SearchRewardVoucherBasketValidationEnabled", "false").Is("true")
                && IsRewardVoucherValidationEnvironmentAllowed();
        }

        private static bool ShouldRunRewardVoucherSelectionValidation()
        {
            // PR #1616/#1626 add explicit voucher option selection; keep it opt-in until the target environment exposes the mutation and amount fields.
            return ConfigReader.Get("SearchRewardVoucherSelectionValidationEnabled", "false").Is("true")
                && IsRewardVoucherValidationEnvironmentAllowed();
        }

        private static bool IsRewardVoucherValidationEnvironmentAllowed()
        {
            string[] allowedEnvironments = ConfigReader.Get("SearchRewardVoucherValidationEnvironments", "sit")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return allowedEnvironments.Any(x => env.Is(x));
        }

        private static void AssertUpgradeVoucherSummary(JourneyType journeyType, bool shouldValidateBasketVoucherDetails, string failureContext, FareFamilyType fareFamily = FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY)
        {
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(out string b2cToken);
            JObject retrieveBasketResult;
            JObject offerPriceResult = journeyType == JourneyType.Return
                ? ExecuteReturnRewardVoucherSummarySearch(
                    b2cToken,
                    LocationType.LHR,
                    LocationType.JFK,
                    fareFamily,
                    numberOfAdults: 1,
                    includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                    out retrieveBasketResult)
                : ExecuteOneWayRewardVoucherSummarySearch(
                    b2cToken,
                    LocationType.LHR,
                    LocationType.JFK,
                    fareFamily,
                    numberOfAdults: 1,
                    includeVoucherBasketDetails: shouldValidateBasketVoucherDetails,
                    out retrieveBasketResult);
            if (!offerPriceResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDetailsOrAcceptedNoUpgradeOption(
                    offerPriceResult,
                    loyaltyDetails["vouchers"],
                    expectedPassengerCount: 1,
                    out string voucherDetailsMessage,
                    out bool acceptedNoUpgradeOption,
                    validateVoucherAmountCalculations: shouldValidateBasketVoucherDetails),
                $"Member {loyaltyDetails["maskedRowKey"]} {failureContext}. {voucherDetailsMessage}");

            if (acceptedNoUpgradeOption)
            {
                Console.WriteLine($"Member {loyaltyDetails["maskedRowKey"]}: {voucherDetailsMessage}");
                return;
            }

            if (shouldValidateBasketVoucherDetails)
            {
                Assert.True(
                    GraphQlAssertions.ValidateRewardVoucherDetailsPersistedInBasket(offerPriceResult, retrieveBasketResult, out string basketVoucherDetailsMessage),
                    $"Member {loyaltyDetails["maskedRowKey"]} upgrade voucher details were not persisted in basket after reward search summary. {basketVoucherDetailsMessage}");
            }
        }
        private static JObject ExecuteOneWayRewardVoucherSummarySearch(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            FareFamilyType fareFamily,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            bool selectHighestAwardPointsFare,
            out JObject retrieveBasketResult)
        {
            RewardVoucherSummarySearchResult result = ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                origin,
                destination,
                fareFamily,
                numberOfAdults,
                includeVoucherBasketDetails,
                selectHighestAwardPointsFare);
            retrieveBasketResult = result.RetrieveBasketResult;

            return result.SearchSummaryResult;
        }

        private static JObject ExecuteOneWayRewardVoucherSummarySearch(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            FareFamilyType fareFamily,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            out JObject retrieveBasketResult)
        {
            return ExecuteOneWayRewardVoucherSummarySearch(
                b2cToken,
                origin,
                destination,
                fareFamily,
                numberOfAdults,
                includeVoucherBasketDetails,
                selectHighestAwardPointsFare: false,
                out retrieveBasketResult);
        }

        private static RewardVoucherSummarySearchResult ExecuteOneWayRewardVoucherSummarySearchWithContext(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            FareFamilyType fareFamily,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            bool selectHighestAwardPointsFare = false,
            FlightType operatingCarrier = FlightType.VS,
            string departureDate = "",
            bool includeContinuationBasketDetails = false,
            bool allowMissingOperatingCarrier = false,
            bool retrieveVoucherBasketDetails = true,
            bool includeVoucherEligibility = false)
        {
            departureDate = departureDate.IsNullOrWhiteSpace() ? OfferSearchHelper.DepartureDate() : departureDate;

            string offerSearchGQLRequest = BuildOfferSearchRequest(
                JourneyType.OneWay,
                departureDate,
                locationOne: origin,
                locationTwo: destination,
                numberOfAdults: numberOfAdults,
                rewardSearch: true);

            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return new RewardVoucherSummarySearchResult();

            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
            SectorUpgradeCandidate valueCandidate = selectHighestAwardPointsFare
                ? SelectHighestAwardPointsSummaryCandidateForFareFamily(offerSearchResponse, fareFamily, operatingCarrier)
                : null;
            string summaryLink = valueCandidate?.Link
                ?? (allowMissingOperatingCarrier
                    ? TrySelectSummaryLinkForFareFamily(offerSearchResponse, fareFamily, operatingCarrier)
                    : SelectSummaryLinkForFareFamily(offerSearchResponse, fareFamily, operatingCarrier));
            if (summaryLink.IsNullOrWhiteSpace() && allowMissingOperatingCarrier)
                return new RewardVoucherSummarySearchResult();
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks([summaryLink], randomOffer: false);

            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.OneWay, basketId, offerId, summaryLink, includeVoucherDetails: true, includeVoucherBasketDetails: includeVoucherBasketDetails, includeVoucherEligibility: includeVoucherEligibility);

            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            string offerPriceResponseBody = offerPriceResponse.ReadResponseAsString();
            JObject offerPriceResult = JObject.Parse(offerPriceResponseBody);
            JObject retrieveBasketResult = new JObject();

            if (includeVoucherBasketDetails && retrieveVoucherBasketDetails)
            {
                retrieveBasketResult = RetrieveBasketAfterVoucherSummary(basketId, b2cToken);
            }
            else if (includeContinuationBasketDetails)
            {
                retrieveBasketResult = RetrieveBasketAfterSummary(basketId, b2cToken);
            }

            return new RewardVoucherSummarySearchResult
            {
                BasketId = basketId,
                OfferId = offerId,
                SummaryLink = summaryLink,
                OutboundDepartureDate = departureDate,
                BaselineCabinPoints = valueCandidate?.EconomyPoints ?? 0M,
                SelectedCabinPoints = valueCandidate?.SelectedCabinPoints ?? 0M,
                SearchSummaryResult = offerPriceResult,
                RetrieveBasketResult = retrieveBasketResult
            };
        }

        private static JObject ExecuteReturnRewardVoucherSummarySearch(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            FareFamilyType fareFamily,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            out JObject retrieveBasketResult,
            bool includeContinuationBasketDetails = false,
            FlightType outboundOperatingCarrier = FlightType.VS,
            FlightType inboundOperatingCarrier = FlightType.VS,
            string departureDate = "",
            string returnDate = "",
            bool allowMissingOutboundCarrier = false,
            bool allowMissingInboundCarrier = false,
            bool includeVoucherEligibility = false)
        {
            retrieveBasketResult = new JObject();
            departureDate = departureDate.IsNullOrWhiteSpace() ? OfferSearchHelper.DepartureDate() : departureDate;
            returnDate = returnDate.IsNullOrWhiteSpace() ? OfferSearchHelper.ReturnDate(departureDate) : returnDate;

            string offerSearchGQLRequest = BuildOfferSearchRequest(
                JourneyType.Return,
                departureDate,
                returnDate,
                locationOne: origin,
                locationTwo: destination,
                locationThree: destination,
                locationFour: origin,
                numberOfAdults: numberOfAdults,
                rewardSearch: true);

            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return new JObject();

            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
            string fareLink = allowMissingOutboundCarrier
                ? TrySelectFareLinkForFareFamily(offerSearchResponse, fareFamily, outboundOperatingCarrier)
                : SelectFareLinkForFareFamily(offerSearchResponse, fareFamily, outboundOperatingCarrier);
            if (fareLink.IsNullOrWhiteSpace() && allowMissingOutboundCarrier)
                return new JObject();
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks([fareLink], randomOffer: false);

            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId, rewardSearch: true);
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId, b2cToken).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            string summaryLink = TrySelectSummaryLinkForFareFamily(offerReturnSearchResponse, fareFamily, inboundOperatingCarrier);
            if (summaryLink.IsNullOrWhiteSpace() && allowMissingInboundCarrier)
                return new JObject();

            Assert.False(
                summaryLink.IsNullOrWhiteSpace(),
                $"Summary link not returned for fare family {fareFamily} and operating carrier {inboundOperatingCarrier}. Response: {offerReturnSearchResponse.ReadResponseAsString()}");
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks([summaryLink], randomOffer: false);

            string offerPriceGQLRequest = BuildRewardOfferPriceRequest(JourneyType.Return, basketId, offerReturnId, summaryLink, includeVoucherDetails: true, includeVoucherBasketDetails: includeVoucherBasketDetails, includeVoucherEligibility: includeVoucherEligibility);

            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            string offerPriceResponseBody = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (includeVoucherBasketDetails)
            {
                retrieveBasketResult = RetrieveBasketAfterVoucherSummary(basketId, b2cToken);
            }
            else if (includeContinuationBasketDetails)
            {
                retrieveBasketResult = RetrieveBasketAfterSummary(basketId, b2cToken);
            }

            return JObject.Parse(offerPriceResponseBody);
        }

        private static RewardVoucherSectorSearchResult ExecuteReturnRewardVoucherSectorCalculationSearch(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            FareFamilyType outboundFareFamily = FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY,
            FareFamilyType inboundFareFamily = FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY)
        {
            const int availabilityWindowDays = 3;
            DateTime initialDepartureDate = OfferSearchHelper.DepartureDate().Parse<DateTime>();
            List<string> attemptedSearches = new List<string>();

            for (int dayOffset = 0; dayOffset < availabilityWindowDays; dayOffset++)
            {
                string departureDate = initialDepartureDate.AddDays(dayOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                string returnDate = initialDepartureDate.AddDays(dayOffset + 7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                RewardVoucherSectorSearchResult searchResult = TryExecuteReturnRewardVoucherSectorCalculationSearch(
                    b2cToken,
                    origin,
                    destination,
                    numberOfAdults,
                    includeVoucherBasketDetails,
                    departureDate,
                    returnDate,
                    outboundFareFamily,
                    inboundFareFamily,
                    out string attemptMessage);
                if (searchResult.SearchSummaryResult.HasValues)
                    return searchResult;

                attemptedSearches.Add($"{origin}-{destination} {departureDate}/{returnDate}: {attemptMessage}");
            }

            Assert.Fail(
                $"No return reward search produced VS-operated outbound and inbound flights with paired Economy and Premium fares across the route/date discovery window. Attempts: {string.Join(" | ", attemptedSearches)}");
            return new RewardVoucherSectorSearchResult();
        }

        private static RewardVoucherSectorSearchResult TryExecuteReturnRewardVoucherSectorCalculationSearch(
            string b2cToken,
            LocationType origin,
            LocationType destination,
            int numberOfAdults,
            bool includeVoucherBasketDetails,
            string departureDate,
            string returnDate,
            FareFamilyType outboundFareFamily,
            FareFamilyType inboundFareFamily,
            out string message)
        {
            message = string.Empty;
            string offerSearchGQLRequest = BuildOfferSearchRequest(
                JourneyType.Return,
                departureDate,
                returnDate,
                locationOne: origin,
                locationTwo: destination,
                locationThree: destination,
                locationFour: origin,
                numberOfAdults: numberOfAdults,
                rewardSearch: true);
            HttpResponseMessage outboundResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();
            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(outboundResponse))
            {
                message = "outbound search returned no usable offers";
                return new RewardVoucherSectorSearchResult();
            }

            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(outboundResponse);
            if (!TrySelectSectorUpgradeCandidate(
                outboundResponse,
                outboundFareFamily,
                FlightType.VS,
                linkType: "fare",
                expectedOrigin: origin,
                expectedDestination: destination,
                requirePositiveUpgradeDelta: outboundFareFamily != FareFamilyType.AWARDECONOMY,
                out SectorUpgradeCandidate outbound,
                out message))
            {
                return new RewardVoucherSectorSearchResult();
            }
            string outboundOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks([outbound.Link], randomOffer: false);
            string inboundSearchRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: outboundOfferId, basketId: basketId, rewardSearch: true);
            HttpResponseMessage inboundResponse = GraphQLEndpoint.OfferSearch(inboundSearchRequest, basketId, b2cToken).EnsureGQLResponseSuccess();
            dynamic inboundResult = JsonConvert.DeserializeObject<dynamic>(inboundResponse.ReadResponseAsString());
            if (!GraphQlAssertions.ValidateInboundSearchResult(inboundSearchRequest, inboundResult, out string inboundMessage))
            {
                message = $"inbound search was invalid: {inboundMessage}";
                return new RewardVoucherSectorSearchResult();
            }

            if (!TrySelectSectorUpgradeCandidate(
                inboundResponse,
                inboundFareFamily,
                FlightType.VS,
                linkType: "summary",
                expectedOrigin: destination,
                expectedDestination: origin,
                requirePositiveUpgradeDelta: inboundFareFamily != FareFamilyType.AWARDECONOMY,
                out SectorUpgradeCandidate inbound,
                out message))
            {
                return new RewardVoucherSectorSearchResult();
            }
            string inboundOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks([inbound.Link], randomOffer: false);
            string offerPriceRequest = BuildRewardOfferPriceRequest(
                JourneyType.Return,
                basketId,
                inboundOfferId,
                inbound.Link,
                includeVoucherDetails: true,
                includeVoucherBasketDetails: includeVoucherBasketDetails);
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceRequest, b2cToken).EnsureGQLResponseSuccess();
            JObject searchSummaryResult = JObject.Parse(offerPriceResponse.ReadResponseAsString());
            JObject retrieveBasketResult = includeVoucherBasketDetails
                ? RetrieveBasketAfterVoucherSummary(basketId, b2cToken)
                : new JObject();

            return new RewardVoucherSectorSearchResult
            {
                BasketId = basketId,
                OfferId = inboundOfferId,
                SummaryLink = inbound.Link,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                OutboundFlight = outbound.Flight,
                InboundFlight = inbound.Flight,
                OutboundEconomyPoints = outbound.EconomyPoints,
                OutboundSelectedCabinPoints = outbound.SelectedCabinPoints,
                InboundEconomyPoints = inbound.EconomyPoints,
                InboundSelectedCabinPoints = inbound.SelectedCabinPoints,
                SearchSummaryResult = searchSummaryResult,
                RetrieveBasketResult = retrieveBasketResult
            };
        }

        private static bool TrySelectSectorUpgradeCandidate(
            HttpResponseMessage response,
            FareFamilyType selectedFareFamily,
            FlightType operatingCarrier,
            string linkType,
            LocationType expectedOrigin,
            LocationType expectedDestination,
            bool requirePositiveUpgradeDelta,
            out SectorUpgradeCandidate candidate,
            out string message)
        {
            candidate = null;
            message = string.Empty;
            JObject result = JObject.Parse(response.ReadResponseAsString());
            string selectedFareFamilyName = selectedFareFamily.GetEnumDescription();
            string economyFareFamilyName = FareFamilyType.AWARDECONOMY.GetEnumDescription();
            List<SectorUpgradeCandidate> candidates = result
                .SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flightAndFare =>
                {
                    List<JToken> segments = flightAndFare.SelectTokens("flight.segments[*]").ToList();
                    return segments.Count > 0 &&
                        segments[0].SelectToken("origin.code")?.ToString().Is(expectedOrigin.ToString()) == true &&
                        segments[^1].SelectToken("destination.code")?.ToString().Is(expectedDestination.ToString()) == true &&
                        segments.All(segment => segment.SelectToken("operatingAirline.code")?.ToString().Is(operatingCarrier.ToString()) == true);
                })
                .Select(flightAndFare =>
                {
                    JToken economyFare = flightAndFare.SelectTokens("fares[*]")
                        .FirstOrDefault(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(economyFareFamilyName) == true);
                    JToken selectedFare = flightAndFare.SelectTokens("fares[*]")
                        .FirstOrDefault(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(selectedFareFamilyName) == true);
                    string link = selectedFare?.SelectTokens("links[*]")
                        .FirstOrDefault(item => item.SelectToken("rel")?.ToString().Is(linkType) == true)?
                        .SelectToken("href")?.ToString() ?? string.Empty;
                    decimal economyPoints = economyFare?.SelectToken("price.awardPoints")?.Value<decimal?>() ?? 0M;
                    decimal selectedCabinPoints = selectedFare?.SelectToken("price.awardPoints")?.Value<decimal?>() ?? 0M;
                    string flight = string.Join(", ", flightAndFare.SelectTokens("flight.segments[*]")
                        .Select(segment => $"{segment.SelectToken("flightNumber")} {segment.SelectToken("origin.code")}-{segment.SelectToken("destination.code")} {segment.SelectToken("departure")}"));
                    return new SectorUpgradeCandidate(link, flight, economyPoints, selectedCabinPoints);
                })
                .Where(candidate => !candidate.Link.IsNullOrWhiteSpace() &&
                    (!requirePositiveUpgradeDelta || candidate.UpgradeDelta > 0M))
                .OrderByDescending(candidate => candidate.UpgradeDelta)
                .ToList();

            if (candidates.Count == 0)
            {
                message = $"no {operatingCarrier}-operated {expectedOrigin}-{expectedDestination} flight returned paired {economyFareFamilyName}/{selectedFareFamilyName} fares with a '{linkType}' link. ResponsePreview: {GraphQlAssertions.BuildJsonPreview(result)}";
                return false;
            }

            candidate = candidates[0];
            return true;
        }

        private static void AssertUpgradeTieRecalculation(
            RewardVoucherSectorSearchResult searchResult,
            string b2cToken,
            string maskedMemberNumber)
        {
            Assert.True(
                GraphQlAssertions.TryGetUpgradeCompanionTie(
                    searchResult.SearchSummaryResult,
                    out JObject tiedUpgradeOption,
                    out _,
                    out List<string> selectedVoucherIds,
                    out string tieMessage),
                $"Member {maskedMemberNumber} did not have a selectable Upgrade/Companion tie. {tieMessage}");

            Assert.True(
                selectedVoucherIds.Count > 0,
                $"Member {maskedMemberNumber} had an Upgrade/Companion tie without applied voucher IDs. UpgradeOption: {tiedUpgradeOption}");

            string selectionRequest = OfferSearchHelper.SetVoucherRedemptionSelectionGQLRequest(searchResult.BasketId, selectedVoucherIds);
            JObject selectionResult = JObject.Parse(GraphQLEndpoint
                .SetVoucherRedemptionSelection(selectionRequest, b2cToken)
                .EnsureGQLResponseSuccess()
                .ReadResponseAsString());
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherSelectionMutationResponse(selectionResult, tiedUpgradeOption, selectedVoucherIds, out string selectionMessage),
                $"Member {maskedMemberNumber} did not retain Upgrade as best option after selecting tied voucher(s). {selectionMessage}");
            AssertUpgradeTieAfterSummaryRefresh(searchResult, b2cToken, maskedMemberNumber, "selection");

            string deselectionRequest = OfferSearchHelper.SetVoucherRedemptionSelectionGQLRequest(searchResult.BasketId, Array.Empty<string>());
            JObject deselectionResult = JObject.Parse(GraphQLEndpoint
                .SetVoucherRedemptionSelection(deselectionRequest, b2cToken)
                .EnsureGQLResponseSuccess()
                .ReadResponseAsString());
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDeselectionMutationResponse(deselectionResult, out string deselectionMessage),
                $"Member {maskedMemberNumber} voucher deselection did not clear the previous tie selection. {deselectionMessage}");
            JObject basketAfterDeselection = RetrieveBasketAfterVoucherSummary(searchResult.BasketId, b2cToken);
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherDeselectionPersistedInBasket(basketAfterDeselection, out string basketDeselectionMessage),
                $"Member {maskedMemberNumber} voucher deselection was not persisted before tie recalculation. {basketDeselectionMessage}");
            AssertUpgradeTieAfterSummaryRefresh(searchResult, b2cToken, maskedMemberNumber, "deselection");
        }

        private static void AssertUpgradeTieAfterSummaryRefresh(
            RewardVoucherSectorSearchResult searchResult,
            string b2cToken,
            string maskedMemberNumber,
            string action)
        {
            string refreshRequest = BuildRewardOfferPriceRequest(
                JourneyType.Return,
                searchResult.BasketId,
                searchResult.OfferId,
                searchResult.SummaryLink,
                includeVoucherDetails: true,
                includeVoucherBasketDetails: true);
            JObject refreshedSummary = JObject.Parse(GraphQLEndpoint
                .OfferPrice(refreshRequest, b2cToken)
                .EnsureGQLResponseSuccess()
                .ReadResponseAsString());
            JObject refreshedBasket = RetrieveBasketAfterVoucherSummary(searchResult.BasketId, b2cToken);
            Assert.True(
                GraphQlAssertions.ValidateUpgradePreferredOnCompanionTieAndPersisted(refreshedSummary, refreshedBasket, out string message),
                $"Member {maskedMemberNumber} did not receive Upgrade as best option after voucher {action} recalculation. {message}");
        }

        private static void AssertAllExpiredVoucherScenario(int voucherCount)
        {
            (Dictionary<string, string> loyaltyDetails, string b2cToken) = RetrieveExpiryScenarioAccount(voucherCount);
            DateTime outboundDate = GetVoucherExpiryDates(loyaltyDetails["vouchers"]).Max().AddDays(1);
            RewardVoucherSummarySearchResult searchResult = ExecuteExpiryScenarioSearch(
                b2cToken,
                outboundDate,
                includeContinuationBasketDetails: true);
            if (!searchResult.SearchSummaryResult.HasValues)
                return;

            Assert.True(
                GraphQlAssertions.ValidateExpiredRewardVouchersExcluded(searchResult.SearchSummaryResult, loyaltyDetails["vouchers"], searchResult.OutboundDepartureDate, out string expiryMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} with {voucherCount} expired voucher(s) received voucher redemption details. {expiryMessage}");
            Assert.True(
                GraphQlAssertions.ValidateVoucherEligibilityIndicator(searchResult.SearchSummaryResult, expectedEligible: false, out string indicatorMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} with {voucherCount} expired voucher(s) did not receive the expiry indicator required by AC26. {indicatorMessage}");
            Assert.True(
                GraphQlAssertions.ValidateNonVoucherBasketContinuation(searchResult.RetrieveBasketResult, searchResult.BasketId, out string basketMessage),
                $"Member {loyaltyDetails["maskedRowKey"]} could not continue without {voucherCount} expired voucher(s). {basketMessage}");
        }

        private static (Dictionary<string, string> LoyaltyDetails, string B2cToken) RetrieveExpiryScenarioAccount(
            int exactVoucherCount,
            int maximumExpiryMonths = 6,
            bool requireDifferentVoucherExpiryDates = false)
        {
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
                out string b2cToken,
                minimumVoucherExpiryDate: DateTime.UtcNow.Date.AddMonths(1),
                voucherExpiryBeforeDate: DateTime.UtcNow.Date.AddMonths(maximumExpiryMonths),
                exactVoucherCount: exactVoucherCount,
                requireAllVouchersInExpiryRange: true,
                requireDifferentVoucherExpiryDates: requireDifferentVoucherExpiryDates);
            return (loyaltyDetails, b2cToken);
        }

        private static RewardVoucherSummarySearchResult ExecuteExpiryScenarioSearch(
            string b2cToken,
            DateTime outboundDate,
            bool includeContinuationBasketDetails = false)
        {
            return ExecuteOneWayRewardVoucherSummarySearchWithContext(
                b2cToken,
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.AWARDECONOMY,
                numberOfAdults: 2,
                includeVoucherBasketDetails: false,
                includeContinuationBasketDetails: includeContinuationBasketDetails,
                departureDate: outboundDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                includeVoucherEligibility: true);
        }

        private static List<DateTime> GetVoucherExpiryDates(string voucherPayload)
        {
            List<DateTime> expiryDates = new List<DateTime>();
            foreach (JObject voucher in JArray.Parse(voucherPayload).Children<JObject>())
            {
                string expiryDate = voucher.GetValue("expiryDate", StringComparison.OrdinalIgnoreCase)?.ToString();
                if (DateTime.TryParse(
                    expiryDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedExpiryDate))
                {
                    expiryDates.Add(parsedExpiryDate.Date);
                }
            }

            expiryDates.Sort();
            Assert.True(expiryDates.Count > 0, $"Storage-table voucher payload did not contain valid expiry dates. Payload: {voucherPayload}");
            return expiryDates;
        }

        private static string FilterVoucherPayloadByMinimumExpiryDate(
            string voucherPayload,
            DateTime minimumExpiryDate,
            int? maximumVoucherCount = null)
        {
            IEnumerable<JObject> eligibleVoucherQuery = JArray.Parse(voucherPayload)
                .Children<JObject>()
                .Where(voucher => DateTime.TryParse(
                    voucher.GetValue("expiryDate", StringComparison.OrdinalIgnoreCase)?.ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime expiryDate) &&
                    expiryDate.Date >= minimumExpiryDate.Date)
                .OrderBy(voucher => DateTime.Parse(
                    voucher.GetValue("expiryDate", StringComparison.OrdinalIgnoreCase)?.ToString() ?? string.Empty,
                    CultureInfo.InvariantCulture));
            if (maximumVoucherCount.HasValue)
            {
                eligibleVoucherQuery = eligibleVoucherQuery.Take(maximumVoucherCount.Value);
            }

            JArray eligibleVouchers = new JArray(eligibleVoucherQuery);

            return eligibleVouchers.ToString(Formatting.None);
        }

        private static bool ValidateUpgradeIsBestValueAgainstCompanion(
            JObject searchSummaryResponse,
            decimal baselineCabinPoints,
            decimal selectedCabinPoints,
            out string message)
        {
            decimal upgradeDelta = selectedCabinPoints - baselineCabinPoints;
            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            JArray availableVouchers = voucherDetails?.SelectToken("availableVouchers") as JArray ?? new JArray();
            decimal availableVoucherBalance = availableVouchers
                .Children<JObject>()
                .Sum(voucher => voucher.SelectToken("balancePoints")?.Value<decimal?>() ?? 0M);
            decimal expectedUpgradeValue = Math.Min(upgradeDelta, availableVoucherBalance);
            decimal companionValue = (voucherDetails?.SelectToken("redemptionOptions") as JArray ?? new JArray())
                .Children<JObject>()
                .Where(option => option.SelectToken("redemptionType")?.ToString().Is("COMPANION") == true)
                .Select(option => option.SelectToken("totalPointsUsed")?.Value<decimal?>() ?? 0M)
                .DefaultIfEmpty(0M)
                .Max();

            message = $"BaselineCabinPoints: {baselineCabinPoints}, SelectedCabinPoints: {selectedCabinPoints}, UpgradeDelta: {upgradeDelta}, AvailableVoucherBalance: {availableVoucherBalance}, ExpectedUpgradeValue: {expectedUpgradeValue}, CompanionValue: {companionValue}.";

            return upgradeDelta > 0M &&
                availableVoucherBalance > 0M &&
                expectedUpgradeValue >= companionValue;
        }

        private static DateTime GetVoucherExpiryWithinRange(string voucherPayload, DateTime minimumExpiryDate, DateTime maximumExpiryDate)
        {
            List<DateTime> matchingExpiryDates = JArray.Parse(voucherPayload)
                .Children<JObject>()
                .Select(voucher => voucher.GetValue("expiryDate", StringComparison.OrdinalIgnoreCase)?.ToString())
                .Where(expiryDate => DateTime.TryParse(expiryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                .Select(expiryDate => DateTime.Parse(expiryDate, CultureInfo.InvariantCulture).Date)
                .Where(expiryDate => expiryDate >= minimumExpiryDate.Date && expiryDate < maximumExpiryDate.Date)
                .OrderBy(expiryDate => expiryDate)
                .ToList();

            Assert.True(
                matchingExpiryDates.Count > 0,
                $"Storage-table test data did not contain a voucher expiring between {minimumExpiryDate:yyyy-MM-dd} and {maximumExpiryDate:yyyy-MM-dd}.");

            return matchingExpiryDates[0];
        }

        private static JObject RetrieveBasketAfterVoucherSummary(string basketId, string b2cToken)
        {
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketWithVoucherDetailsGQLRequest(basketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.RetrieveBasket(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            string retrieveBasketResponseBody = retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JObject.Parse(retrieveBasketResponseBody);
        }

        private static JObject RetrieveBasketAfterSummary(string basketId, string b2cToken)
        {
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.RetrieveBasket(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            return JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());
        }

        private sealed class RewardVoucherSummarySearchResult
        {
            public string BasketId { get; set; } = string.Empty;
            public string OfferId { get; set; } = string.Empty;
            public string SummaryLink { get; set; } = string.Empty;
            public string OutboundDepartureDate { get; set; } = string.Empty;
            public decimal BaselineCabinPoints { get; set; }
            public decimal SelectedCabinPoints { get; set; }
            public JObject SearchSummaryResult { get; set; } = new JObject();
            public JObject RetrieveBasketResult { get; set; } = new JObject();
        }

        private sealed class RewardVoucherSectorSearchResult
        {
            public string BasketId { get; set; } = string.Empty;
            public string OfferId { get; set; } = string.Empty;
            public string SummaryLink { get; set; } = string.Empty;
            public string DepartureDate { get; set; } = string.Empty;
            public string ReturnDate { get; set; } = string.Empty;
            public string OutboundFlight { get; set; } = string.Empty;
            public string InboundFlight { get; set; } = string.Empty;
            public decimal OutboundEconomyPoints { get; set; }
            public decimal OutboundSelectedCabinPoints { get; set; }
            public decimal InboundEconomyPoints { get; set; }
            public decimal InboundSelectedCabinPoints { get; set; }
            public JObject SearchSummaryResult { get; set; } = new JObject();
            public JObject RetrieveBasketResult { get; set; } = new JObject();
        }

        private sealed record SectorUpgradeCandidate(
            string Link,
            string Flight,
            decimal EconomyPoints,
            decimal SelectedCabinPoints)
        {
            public decimal UpgradeDelta => SelectedCabinPoints - EconomyPoints;
        }

        private static string SelectSummaryLinkForFareFamily(HttpResponseMessage offerSearchResponse, FareFamilyType fareFamily, FlightType operatingCarrier = FlightType.VS)
        {
            string responseBody = offerSearchResponse.ReadResponseAsString();
            string summaryLink = TrySelectSummaryLinkForFareFamily(responseBody, fareFamily, operatingCarrier);

            Assert.False(summaryLink.IsNullOrWhiteSpace(), $"Summary link not returned for fare family {fareFamily} and operating carrier {operatingCarrier}. Response: {responseBody}");

            return summaryLink;
        }

        private static string TrySelectSummaryLinkForFareFamily(HttpResponseMessage offerSearchResponse, FareFamilyType fareFamily, FlightType operatingCarrier)
        {
            return TrySelectSummaryLinkForFareFamily(offerSearchResponse.ReadResponseAsString(), fareFamily, operatingCarrier);
        }

        private static string TrySelectSummaryLinkForFareFamily(string responseBody, FareFamilyType fareFamily, FlightType operatingCarrier)
        {
            return OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(responseBody, fareFamily, operatingCarrier).FirstOrDefault() ?? string.Empty;
        }

        private static SectorUpgradeCandidate SelectHighestAwardPointsSummaryCandidateForFareFamily(HttpResponseMessage offerSearchResponse, FareFamilyType fareFamily, FlightType operatingCarrier = FlightType.VS)
        {
            string responseBody = offerSearchResponse.ReadResponseAsString();
            JObject response = JObject.Parse(responseBody);
            string selectedFareFamily = fareFamily.GetEnumDescription();
            string baselineFareFamily = fareFamily switch
            {
                FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY => FareFamilyType.AWARDECONOMY.GetEnumDescription(),
                FareFamilyType.AWARDBUSINESSFIRST => FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY.GetEnumDescription(),
                _ => string.Empty
            };
            List<SectorUpgradeCandidate> candidates = response
                .SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flightAndFare => flightAndFare.SelectTokens("flight.segments[*].operatingAirline.code")
                    .Any(code => code.ToString().Is(operatingCarrier.ToString())))
                .Select(flightAndFare =>
                {
                    JToken selectedFare = flightAndFare.SelectTokens("fares[*]")
                        .FirstOrDefault(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(selectedFareFamily) == true);
                    JToken baselineFare = flightAndFare.SelectTokens("fares[*]")
                        .FirstOrDefault(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(baselineFareFamily) == true);
                    string link = selectedFare?.SelectTokens("links[*]")
                        .FirstOrDefault(item => item.SelectToken("rel")?.ToString().Is("summary") == true)?
                        .SelectToken("href")?.ToString() ?? string.Empty;
                    decimal baselineFarePoints = baselineFare?.SelectToken("price.awardPoints")?.Value<decimal?>() ?? 0M;
                    decimal selectedFarePoints = selectedFare?.SelectToken("price.awardPoints")?.Value<decimal?>() ?? 0M;
                    return new SectorUpgradeCandidate(link, string.Empty, baselineFarePoints, selectedFarePoints);
                })
                .Where(candidate => !candidate.Link.IsNullOrWhiteSpace() && candidate.UpgradeDelta > 0M)
                .OrderByDescending(candidate => candidate.SelectedCabinPoints)
                .ToList();

            Assert.True(candidates.Any(), $"Summary link not returned for fare family {fareFamily} and operating carrier {operatingCarrier}. Response: {responseBody}");

            return candidates.First();
        }

        private static List<(string Link, decimal AwardPoints)> GetSummaryLinkAwardPointCandidates(JObject response, string expectedFareFamily, FlightType? flightType)
        {
            IEnumerable<JToken> flightsAndFares = response.SelectTokens("$..slice.flightsAndFares[*]");

            if (flightType.HasValue)
            {
                string airlineCode = flightType.Value.ToString();
                flightsAndFares = flightsAndFares.Where(flightAndFare =>
                    flightAndFare.SelectTokens("flight.segments[*].operatingAirline.code")
                        .Any(code => code.ToString().Is(airlineCode)));
            }

            List<(string Link, decimal AwardPoints)> candidates = new List<(string Link, decimal AwardPoints)>();
            foreach (JToken flightAndFare in flightsAndFares)
            {
                IEnumerable<JToken> fares = flightAndFare.SelectTokens("fares[*]")
                    .Where(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(expectedFareFamily) == true);

                foreach (JToken fare in fares)
                {
                    string summaryLink = fare.SelectTokens("links[*]")
                        .Where(link => link.SelectToken("rel")?.ToString().Is("summary") == true)
                        .Select(link => link.SelectToken("href")?.ToString())
                        .FirstOrDefault(link => !link.IsNullOrWhiteSpace());

                    if (summaryLink.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    decimal awardPoints = fare.SelectToken("price.awardPoints")?.Value<decimal?>() ?? 0m;
                    candidates.Add((summaryLink, awardPoints));
                }
            }

            return candidates;
        }

        private static string SelectFareLinkForFareFamily(HttpResponseMessage offerSearchResponse, FareFamilyType fareFamily, FlightType operatingCarrier = FlightType.VS)
        {
            string responseBody = offerSearchResponse.ReadResponseAsString();
            string[] fareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(responseBody, fareFamily, operatingCarrier);

            Assert.True(fareLinks.Any(), $"Fare link not returned for fare family {fareFamily} and operating carrier {operatingCarrier}. Response: {responseBody}");

            return fareLinks[0];
        }

        private static string TrySelectFareLinkForFareFamily(HttpResponseMessage offerSearchResponse, FareFamilyType fareFamily, FlightType operatingCarrier)
        {
            return OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(
                offerSearchResponse.ReadResponseAsString(),
                fareFamily,
                operatingCarrier).FirstOrDefault() ?? string.Empty;
        }

        private static string BuildRewardOfferPriceRequest(JourneyType journeyType, string basketId, string offerId, string summaryLink = "", bool includeVoucherDetails = false, bool includeVoucherBasketDetails = false, bool includeVoucherEligibility = false)
        {
            TestConfig config = new TestConfig();
            config.Search.JourneyType = journeyType;
            config.Search.IsRewardSearch = true;
            config.Internal.BasketId = basketId;

            return OfferSearchHelper.OfferPriceNewGQLRequest(config, offerId, summaryLink, includeVoucherDetails: includeVoucherDetails, includeVoucherBasketDetails: includeVoucherBasketDetails, includeVoucherEligibility: includeVoucherEligibility);
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

        private static string GenerateB2CTokenFromAccountMemberSignIn(string email, string loyaltyAccountNumber, string password)
        {
            bool isSuccess = TryGenerateB2CTokenFromAccountMemberSignIn(email, loyaltyAccountNumber, password, out string accessToken, out string errorCode, out string errorMessage);
            Assert.True(isSuccess, $"AccountMemberSignIn did not return an accessToken. Code: {errorCode}, Message: {errorMessage}");

            return accessToken;
        }

        private static string BuildSharedRewardB2CToken()
        {
            Dictionary<string, string> loyaltyDetails = RetrieveLoyaltyDetailsAndGenerateB2CToken("Red", 1000000, out string b2cToken);
            Assert.True(!b2cToken.IsNullOrEmpty(), "Shared B2C token is null");

            return b2cToken;
        }

        private static string GetSharedRewardB2CToken()
        {
            return sharedRewardB2CToken.Value;
        }

        private string GetVRedToken()
        {
            tokenFixture.AddTokenForVRedFromKeyVault("vred");
            return tokenFixture.GetRequiredToken("vred", "Search reward loyalty points validation.");
        }

        private static Dictionary<string, string> RetrieveLoyaltyVoucherDetailsAndGenerateB2CToken(
            out string b2cToken,
            string voucherType = "",
            int? maximumPoints = null,
            DateTime? minimumVoucherExpiryDate = null,
            DateTime? voucherExpiryBeforeDate = null,
            int? exactVoucherCount = null,
            int? minimumVoucherCount = null,
            bool requireAllVouchersInExpiryRange = false,
            bool requireDifferentVoucherExpiryDates = false,
            bool requireAllVouchersMatchType = false)
        {
            const int maxAttempts = 3;
            const int retryDelayMilliseconds = 1000;
            const string invalidCredentialsCode = "50126";

            if (minimumVoucherExpiryDate == null && voucherExpiryBeforeDate == null)
            {
                // Standard reward searches generate outbound dates five to nine months ahead.
                // Select accounts with vouchers valid beyond that window. Older vouchers may
                // remain on the account and are expected to be filtered from the API response.
                minimumVoucherExpiryDate = DateTime.UtcNow.Date.AddMonths(10);
            }

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyVoucherAccountDetailsFromStorageTable(
                    environmentOverride: GetRewardVoucherAccountEnvironment(),
                    voucherType: voucherType,
                    maximumPoints: maximumPoints,
                    minimumVoucherExpiryDate: minimumVoucherExpiryDate,
                    voucherExpiryBeforeDate: voucherExpiryBeforeDate,
                    exactVoucherCount: exactVoucherCount,
                    minimumVoucherCount: minimumVoucherCount,
                    requireAllVouchersInExpiryRange: requireAllVouchersInExpiryRange,
                    requireDifferentVoucherExpiryDates: requireDifferentVoucherExpiryDates,
                    requireAllVouchersMatchType: requireAllVouchersMatchType);
                if (loyaltyDetails.ContainsKey("error"))
                {
                    b2cToken = string.Empty;
                    Assert.True(false, loyaltyDetails["error"]);
                }

                string flyingClubNumber = loyaltyDetails["rowKey"];
                string maskedFlyingClubNumber = loyaltyDetails["maskedRowKey"];

                bool isSuccess = TryGenerateB2CTokenFromAccountMemberSignIn(string.Empty, flyingClubNumber, GetLoyaltyAccountPassword(), out b2cToken, out string errorCode, out string errorMessage);
                if (isSuccess)
                {
                    return loyaltyDetails;
                }

                bool hasRetryAttemptsRemaining = attempt < maxAttempts;
                bool canRetryWithFreshData = hasRetryAttemptsRemaining && errorCode.Is(invalidCredentialsCode);
                if (!canRetryWithFreshData)
                {
                    Assert.True(false, $"AccountMemberSignIn failed for voucher-backed loyalty account after {attempt} attempt(s). Code: {errorCode}, Message: {errorMessage}, FlyingClubNumber: {maskedFlyingClubNumber}");
                }

                System.Threading.Thread.Sleep(retryDelayMilliseconds);
            }

            b2cToken = string.Empty;
            Assert.True(false, "AccountMemberSignIn failed for voucher-backed loyalty account after retrying with fresh storage-table data.");
            return new Dictionary<string, string>();
        }

        private static Dictionary<string, string> RetrieveNonVoucherLoyaltyDetailsAndGenerateB2CToken(string loyaltyTier, int minimumPoints, out string b2cToken)
        {
            const int maxAttempts = 3;
            const int retryDelayMilliseconds = 1000;
            const string invalidCredentialsCode = "50126";

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountWithoutVouchersFromStorageTable(loyaltyTier, minimumPoints, environmentOverride: GetRewardVoucherAccountEnvironment());
                if (loyaltyDetails.ContainsKey("error"))
                {
                    b2cToken = string.Empty;
                    throw new InvalidOperationException(loyaltyDetails["error"]);
                }

                string flyingClubNumber = loyaltyDetails["rowKey"];
                string maskedFlyingClubNumber = loyaltyDetails["maskedRowKey"];

                bool isSuccess = TryGenerateB2CTokenFromAccountMemberSignIn(loyaltyDetails["email"], flyingClubNumber, GetLoyaltyAccountPassword(), out b2cToken, out string errorCode, out string errorMessage);
                if (isSuccess)
                {
                    return loyaltyDetails;
                }

                bool hasRetryAttemptsRemaining = attempt < maxAttempts;
                bool canRetryWithFreshData = hasRetryAttemptsRemaining && errorCode.Is(invalidCredentialsCode);
                if (!canRetryWithFreshData)
                {
                    throw new InvalidOperationException($"AccountMemberSignIn failed for non-voucher tier '{loyaltyTier}' after {attempt} attempt(s). Code: {errorCode}, Message: {errorMessage}, FlyingClubNumber: {maskedFlyingClubNumber}");
                }

                System.Threading.Thread.Sleep(retryDelayMilliseconds);
            }

            b2cToken = string.Empty;
            throw new InvalidOperationException($"AccountMemberSignIn failed for non-voucher tier '{loyaltyTier}' after retrying with fresh storage-table data.");
        }

        private static string GetRewardVoucherAccountEnvironment()
        {
            return ConfigReader.Get("SearchRewardVoucherAccountEnvironment", env);
        }

        private static int GetRewardVoucherInsufficientPointsMaximumBalance()
        {
            string configuredMaximumBalance = ConfigReader.Get("SearchRewardVoucherInsufficientPointsMaximumBalance", "1000");
            if (int.TryParse(configuredMaximumBalance, out int maximumBalance))
                return maximumBalance;

            throw new InvalidOperationException($"SearchRewardVoucherInsufficientPointsMaximumBalance must be numeric. Actual: '{configuredMaximumBalance}'.");
        }

        private static string GetLoyaltyAccountPassword()
        {
            string password = ConfigReader.Get("LoyaltyAccountPassword");
            if (!password.IsNullOrWhiteSpace())
                return password;

            throw new InvalidOperationException(
                "LoyaltyAccountPassword is required for reward-search member sign-in. " +
                "Configure the LOYALTY_ACCOUNT_PASSWORD GitHub Actions secret or set LoyaltyAccountPassword for local runs.");
        }

        private static Dictionary<string, string> RetrieveLoyaltyDetailsAndGenerateB2CToken(string loyaltyTier, int minimumPoints, out string b2cToken)
        {
            const int maxAttempts = 3;
            const int retryDelayMilliseconds = 1000;
            const string invalidCredentialsCode = "50126";

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(loyaltyTier, minimumPoints);
                string flyingClubNumber = loyaltyDetails["rowKey"];

                bool isSuccess = TryGenerateB2CTokenFromAccountMemberSignIn(loyaltyDetails["email"], flyingClubNumber, GetLoyaltyAccountPassword(), out b2cToken, out string errorCode, out string errorMessage);
                if (isSuccess)
                {
                    return loyaltyDetails;
                }

                bool hasRetryAttemptsRemaining = attempt < maxAttempts;
                bool canRetryWithFreshData = hasRetryAttemptsRemaining && errorCode.Is(invalidCredentialsCode);
                if (!canRetryWithFreshData)
                {
                    Assert.True(false, $"AccountMemberSignIn failed for tier '{loyaltyTier}' after {attempt} attempt(s). Code: {errorCode}, Message: {errorMessage}, FlyingClubNumber: {flyingClubNumber}");
                }

                System.Threading.Thread.Sleep(retryDelayMilliseconds);
            }

            b2cToken = string.Empty;
            Assert.True(false, $"AccountMemberSignIn failed for tier '{loyaltyTier}' after retrying with fresh storage-table data.");
            return new Dictionary<string, string>();
        }

        private static bool TryGenerateB2CTokenFromAccountMemberSignIn(
            string email,
            string loyaltyAccountNumber,
            string password,
            out string accessToken,
            out string errorCode,
            out string errorMessage)
        {
            string accountMemberSignInRequest = BuildAccountMemberSignInRequest(email, loyaltyAccountNumber, password);
            string accountMemberSignInEndpoint = Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : $"{env}")}{ConfigReader.Get("graphQlMyAccountPublic")}");

            HttpResponseMessage gqlResponse = WebRequest.Json.Post(accountMemberSignInEndpoint, accountMemberSignInRequest).EnsureIsSuccessStatusCode();
            string gqlResponseBody = gqlResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JObject gqlPayload = JObject.Parse(gqlResponseBody);

            accessToken = gqlPayload.SelectToken("data.accountMemberSignIn.accessToken")?.ToString() ?? string.Empty;
            if (!accessToken.IsNullOrEmpty())
            {
                errorCode = string.Empty;
                errorMessage = string.Empty;
                return true;
            }

            errorCode = gqlPayload.SelectToken("errors[0].extensions.code")?.ToString() ?? string.Empty;
            errorMessage = gqlPayload.SelectToken("errors[0].extensions.message")?.ToString()
                ?? gqlPayload.SelectToken("errors[0].message")?.ToString()
                ?? "Unknown GraphQL sign-in failure";
            return false;
        }

        private static string BuildAccountMemberSignInRequest(string email, string loyaltyAccountNumber, string password)
        {
            JObject signInRequest = new JObject
            {
                ["flyingClubNumber"] = loyaltyAccountNumber,
                ["password"] = password
            };

            if (!email.IsNullOrWhiteSpace())
            {
                signInRequest["email"] = email;
            }

            JObject request = new JObject
            {
                ["query"] = "mutation AccountMemberSignIn($request: SignInRequestInput) { accountMemberSignIn(request: $request) { accessToken } }",
                ["variables"] = new JObject
                {
                    ["request"] = signInRequest
                }
            };

            return request.ToString(Formatting.None);
        }

        private static void WriteAcceptedNoFilteredRewardOffers(
            string testName,
            LocationType origin,
            LocationType destination,
            int stopCounts,
            FareFamilyType fareFamily,
            FlightType flightType,
            string responseBody,
            string slice = "outbound")
        {
            string diagnostic = BuildAcceptedNoFilteredRewardOffersDiagnostic(
                testName,
                origin,
                destination,
                stopCounts,
                fareFamily,
                flightType,
                responseBody,
                slice);

            Console.WriteLine(diagnostic);
        }

        private static string BuildAcceptedNoFilteredRewardOffersDiagnostic(
            string testName,
            LocationType origin,
            LocationType destination,
            int stopCounts,
            FareFamilyType fareFamily,
            FlightType flightType,
            string responseBody,
            string slice)
        {
            return $"Accepted no filtered reward offers: Test={testName}, Slice={slice}, Route={origin}-{destination}, Stops={stopCounts}, FareFamily={fareFamily}, FlightType={flightType}, Response={TruncateDiagnostic(responseBody)}";
        }

        private static string TruncateDiagnostic(string value, int maxLength = 1000)
        {
            if (value.IsNullOrWhiteSpace() || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength) + "...";
        }

        private static string BuildMultiCityRewardOfferPriceRequest(string basketId, string offerId, int currentTripIndexId = 3)
        {
            TestConfig config = new TestConfig();
            config.Search.JourneyType = JourneyType.MultiCity;
            config.Search.IsRewardSearch = true;
            config.Internal.BasketId = basketId;

            return OfferSearchHelper.OfferPriceNewGQLRequest(config, offerId, currentTripIndexId: currentTripIndexId);
        }

    }
}


