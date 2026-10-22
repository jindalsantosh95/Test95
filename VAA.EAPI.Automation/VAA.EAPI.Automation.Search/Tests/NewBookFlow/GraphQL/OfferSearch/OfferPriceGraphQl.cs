using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Search.Constants;
using Xunit.Abstractions;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class OfferPriceGraphQl
    {
        private readonly ITestOutputHelper testOutputHelper;
        private bool requireUsableOfferForSignOff;

        public OfferPriceGraphQl(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.MAN, LocationType.JFK, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.LAX, FareFamilyType.ECON)]
        [InlineData(LocationType.AUS, LocationType.LHR, FareFamilyType.ECONDELIGHTFLEXIBLE)]
        [InlineData(LocationType.LHR, LocationType.BOM, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE)]
        [InlineData(LocationType.SFO, LocationType.LHR, FareFamilyType.ECONDELIGHTFLEXIBLE)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        [InlineData(LocationType.LON, LocationType.JFK, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LON, LocationType.NYC, FareFamilyType.ECON)]
        [InlineData(LocationType.NYC, LocationType.LHR, FareFamilyType.ECONDELIGHTFLEXIBLE)]
        [InlineData(LocationType.LON, LocationType.BOM, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LON, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE)]
        [InlineData(LocationType.SFO, LocationType.LON, FareFamilyType.ECONDELIGHTFLEXIBLE)]
        [InlineData(LocationType.LON, LocationType.NYC, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        public void OfferPriceGraphQl_TC01_OneWay_VAFlights_SummaryValidatesPricingUpgradesAndEarnPoints(LocationType origin, LocationType destination, FareFamilyType fareFamily)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"OfferMS pricing sign-off requires a usable offer before invoking OfferPrice. OfferSearch response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), $"Offer not returned for the fare family->{fareFamily} and flight type passed->VS");

            //getting offer id from summary link
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            bool hasUpgradeOffers = GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers);
            if (!hasUpgradeOffers)
            {
                testOutputHelper.WriteLine(
                    $"No upgrade offers returned for this scenario. Continuing with pricing and earn-points assertions.{System.Environment.NewLine}" +
                    $"{upgradeOffers}{System.Environment.NewLine}" +
                    $"Offer Search request: {offerSearchGQLRequest}{System.Environment.NewLine}" +
                    $"Offer Search response: {offerSearchResponse.ReadResponseAsString()}{System.Environment.NewLine}" +
                    $"Offer Price request: {offerPriceGQLRequest}{System.Environment.NewLine}" +
                    $"Offer Price response: {offerPriceResponse.ReadResponseAsString()}");
            }

            softAssert.True(GraphQlAssertions.ValidateEarnPoints(offerPriceResult, offerSearchGQLRequest, out string earnPoints), earnPoints);

            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "GraphQL")]
        public void SharedServicesSignOff_OfferPrice_OneWaySummary()
        {
            requireUsableOfferForSignOff = true;
            OfferPriceGraphQl_TC01_OneWay_VAFlights_SummaryValidatesPricingUpgradesAndEarnPoints(
                LocationType.LHR,
                LocationType.JFK,
                FareFamilyType.ECON);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.MAN, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.LAX)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.LON, LocationType.JFK)]
        [InlineData(LocationType.LON, LocationType.NYC)]
        [InlineData(LocationType.LHR, LocationType.NYC)]
        public void OfferPriceGraphQl_TC02_OneWay_KLMFlights_SummaryValidatesPricingAndUpgrades(LocationType origin, LocationType destination)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificAirline(offerSearchResult, FlightType.KL);
            if (!summaryLinks.Any())
            {
                // Keep this test KLM-specific: if KL inventory is absent, do not continue with a different carrier.
                string[] fallbackSummaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchResult, "summary");
                Assert.True(fallbackSummaryLinks.Any(), $"No summary links were returned for airline '{FlightType.KL}', and no summary links were available for any carrier. Response: {offerSearchResult}");
                return;
            }

            //getting offer id from summary link
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.LAX, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.LAS, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.SFO, LocationType.LHR, FareFamilyType.BUSFIRST)]
        [InlineData(LocationType.JFK, LocationType.LHR, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        public void OfferPriceGraphQl_TC03_ReturnTrip_VAFlights_SummaryValidatesRoundTripPricing(LocationType origin, LocationType destination, FareFamilyType fareFamily)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: 1, numberOfChildren: 0, numberOfYouths: 0, numberOfInfants: 0);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = FindFareLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.VS);

            Assert.True(fareLinks.Any(), $"Offer(Outbound) not returned for the fare family->{fareFamily} and flight type passed->VS");

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: false);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            string offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchReturnResult, fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), $"Offer(Inbound) not returned for the fare family->{fareFamily} and flight type passed->VS");

            //getting offer id from summary link
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(OfferSearchGraphQl.VaFlightsSearchTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void OfferPriceGraphQl_TC04_OneWay_VAFlights_MultipleRoutesValidateSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(OfferSearchGraphQl.FlexiDateRoutesTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void OfferPriceGraphQl_TC05_OneWay_FlexiDateFlow_ValidatesSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDateForFlexiSearch = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDateForFlexiSearch, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest).EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [MemberData(nameof(OfferSearchGraphQl.VaFlightsSearchTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void OfferPriceGraphQl_TC06_ReturnTrip_VAFlights_ValidatesInboundAndSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(OfferSearchGraphQl.FlexiDateRoutesTestData), MemberType = typeof(OfferSearchGraphQl))]
        public void OfferPriceGraphQl_TC07_ReturnTrip_FlexiDateFlow_ValidatesInboundAndSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDateForFlexiSearch = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDateForFlexiSearch, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest).EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.BUSFIRST)]
        [InlineData(LocationType.LON, LocationType.JFK, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LON, LocationType.NYC, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.NYC, FareFamilyType.ECONOMYDELIGHT)]
        public void OfferPriceGraphQl_TC08_OneWay_DeltaFlights_SummaryValidatesPricingAndUpgrades(LocationType origin, LocationType destination, FareFamilyType fareFamily)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.DL);

            Assert.True(summaryLinks.Any(), $"Offer not returned for the fare family->{fareFamily} and flight type passed->DL");

            //getting offer id from summary link
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 2, 0, 1, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 3, 1, 2, 0, FareFamilyType.BUSFIRST)]
        public void OfferPriceGraphQl_TC09_ReturnTrip_DeltaFlights_ValidatesInboundAndSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, FareFamilyType fareFamily)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchOutboundResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = FindFareLinksForSpecificCabinAndAirline(offerSearchOutboundResult, fareFamily, FlightType.DL);

            Assert.True(fareLinks.Any(), $"Offer(Outbound) not returned for the fare family->{fareFamily} and flight type passed->DL");

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: false);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            string offerSearchInboundResult = offerReturnSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchInboundResult, fareFamily, FlightType.DL);

            Assert.True(summaryLinks.Any(), $"Offer(Inbound) not returned for the fare family->{fareFamily} and flight type passed->DL");

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: false);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 1, 1, 1, 0, FareFamilyType.ECON, FlightType.DL)]
        [InlineData(LocationType.LHR, LocationType.JFK, 3, 0, 0, 1, 1, FareFamilyType.ECONOMYLIGHT, FlightType.AF)]
        public void OfferPriceGraphQl_TC10_OneWay_MultiSegmentFilter_ValidatesSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, int stopCounts, FareFamilyType fareFamily, FlightType flightType)
        {
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql sort and filter request.
            string offerSearchGQLFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", connectionFilter: stopCounts.ToString(), basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLFilterRequest).EnsureGQLResponseSuccess();

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string offerSearchResult = offerFilterResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, flightType);

            Assert.True(summaryLinks.Any(), $"Offer not returned for the fare family->{fareFamily} and flight type passed->{flightType}");

            //getting offer id from summary link
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerFilterResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, 0, FareFamilyType.ECON, FlightType.DL)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, 1, FareFamilyType.ECON, FlightType.DL)]
        public void OfferPriceGraphQl_TC11_ReturnTrip_MultiSegmentFilter_ValidatesInboundAndSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, int stopCounts, FareFamilyType fareFamily, FlightType flightType)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql filter request.
            string offerSearchGQLFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", connectionFilter: stopCounts.ToString(), basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLFilterRequest).EnsureGQLResponseSuccess();

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string offerSearchOutboundResult = offerFilterResponse.ReadResponseAsString();

            string[] fareLinks = FindFareLinksForSpecificCabinAndAirline(offerSearchOutboundResult, fareFamily, flightType);

            Assert.True(fareLinks.Any(), $"Offer(Outbound) not returned for the fare family->{fareFamily} and flight type passed->{flightType}");

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search return graph ql filter request.
            string offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, locationFive: LocationType.None, locationSix: LocationType.None, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: false, calenderSearch: false, offerId, pqId: "searchOffersNextBFF", connectionFilter: stopCounts.ToString(), basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            string offerSearchInboundResult = offerReturnSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchInboundResult, fareFamily, flightType);

            Assert.True(summaryLinks.Any(), $"Offer(Inbound) not returned for the fare family->{fareFamily} and flight type passed->{flightType}");

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerFilterResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, FareFamilyType.ECONOMYLIGHT, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, FareFamilyType.ECON, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1, FareFamilyType.ECONOMYDELIGHT, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 2, 0, 1, FareFamilyType.COMFORTPLUSPREMIUMECON, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 3, 1, 2, 0, FareFamilyType.BUSFIRST, FareFamilyType.BUSFIRST)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, FareFamilyType.ECONCLASSICFLEXIBLE, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 4, 4, 1, 0, FareFamilyType.ECONDELIGHTFLEXIBLE, FareFamilyType.ECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 2, 0, 1, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        [InlineData(LocationType.LHR, LocationType.JFK, 3, 1, 2, 0, FareFamilyType.BUSFIRSTFLEXIBLE, FareFamilyType.BUSFIRST)]
        public void OfferPriceGraphQl_TC12_ReturnTrip_VirginOutboundDeltaInbound_ValidatesSummaryPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, FareFamilyType VSFareFamily, FareFamilyType DLFareFamily)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchOutboundResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = FindFareLinksForSpecificCabinAndAirline(offerSearchOutboundResult, VSFareFamily, FlightType.VS);

            Assert.True(fareLinks.Any(), $"Offer(Outbound) not returned for the fare family->{VSFareFamily} and flight type passed->VS");

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            string offerSearchInboundResult = offerReturnSearchResponse.ReadResponseAsString();

            string[] summaryLinks = FindSummaryLinksForSpecificCabinAndAirline(offerSearchInboundResult, DLFareFamily, FlightType.DL);

            Assert.True(summaryLinks.Any(), $"Offer(Inbound) not returned for the fare family->{DLFareFamily} and flight type passed->DL");

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.SummaryUpgradeOffersValidation(offerPriceResult, out string upgradeOffers), upgradeOffers);

            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void OfferPriceGraphQl_TC13_ReturnTrip_GovtApprovalFlagPresentInSearchInboundAndSummary()
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.RUH, locationThree: LocationType.RUH, locationFour: LocationType.LHR, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            Assert.True(GraphQlAssertions.ValidatePendingGovtApprovalFlag(offerSearchResponse, out string message), message);

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidatePendingGovtApprovalFlag(offerReturnSearchResponse, out string inbound), inbound);

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);

            //getting offer id from graph ql inbound response
            string offerReturnId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerReturnSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.ValidatePendingGovtApprovalFlag(offerPriceResponse, out string summary), summary);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, PromoCode.VESTA)]
        [InlineData(LocationType.JFK, LocationType.LHR, PromoCode.VESTA)]
        [InlineData(LocationType.MAN, LocationType.JFK, PromoCode.GB02)]
        public void OfferPriceGraphQl_TC14_ReturnTrip_PromoCodeAppliedInSummary(LocationType origin, LocationType destination, PromoCode promoCode)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1, promoCode: promoCode.ToString());

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql outbound response
            if (!OfferSearchHelper.TryGetOfferIdByPromoCodeApplied(offerSearchResponse, promoCode.ToString(), out string offerId))
            {
                testOutputHelper.WriteLine(
                    $"No outbound offers returned with promo code '{promoCode}'. Treating as inventory/promo availability gap for this run.{System.Environment.NewLine}" +
                    $"Offer Search request: {offerSearchGQLRequest}{System.Environment.NewLine}" +
                    $"Offer Search response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            //getting offer id from graph ql inbound response
            if (!OfferSearchHelper.TryGetOfferIdByPromoCodeApplied(offerReturnSearchResponse, promoCode.ToString(), out string offerReturnId))
            {
                testOutputHelper.WriteLine(
                    $"No inbound offers returned with promo code '{promoCode}'. Treating as inventory/promo availability gap for this run.{System.Environment.NewLine}" +
                    $"Offer Search return request: {offerSearchReturnGQLRequest}{System.Environment.NewLine}" +
                    $"Offer Search return response: {offerReturnSearchResponse.ReadResponseAsString()}");
                return;
            }

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.CheckIfErrorExistsInSummary(offerSearchGQLRequest, offerPriceGQLRequest, offerPriceResponse, out string summaryError), summaryError);

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.Return, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount, offerReturnSearchResponse, offerReturnId), totalTaxAndAmount);

            softAssert.True(GraphQlAssertions.ValidatePromoCodeAppliedExistInSummaryResponse(offerPriceResult, promoCode.ToString(), out string promoCodeExist), promoCodeExist);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData(LocationType.CDG, LocationType.JFK, 2, 0, 0, 0)]
        [InlineData(LocationType.AMS, LocationType.JFK, 4, 4, 1, 0)]
        [InlineData(LocationType.MAD, LocationType.JFK, 4, 4, 0, 0)]
        [InlineData(LocationType.LIS, LocationType.JFK, 1, 1, 1, 1)]
        [InlineData(LocationType.BER, LocationType.JFK, 2, 0, 2, 0)]
        [InlineData(LocationType.BRU, LocationType.JFK, 2, 2, 0, 1)]
        [InlineData(LocationType.DUB, LocationType.JFK, 2, 1, 1, 0)]
        [InlineData(LocationType.LIN, LocationType.JFK, 3, 1, 2, 0)]
        [InlineData(LocationType.CPH, LocationType.JFK, 3, 0, 0, 2)]
        [InlineData(LocationType.OSL, LocationType.JFK, 2, 0, 0, 1)]
        [InlineData(LocationType.ARN, LocationType.JFK, 1, 1, 1, 1)]
        [InlineData(LocationType.SOF, LocationType.JFK, 3, 1, 2, 0)]
        [InlineData(LocationType.ATH, LocationType.JFK, 1, 1, 1, 1)]
        [InlineData(LocationType.TLV, LocationType.JFK, 2, 0, 0, 1)]
        [InlineData(LocationType.LOS, LocationType.JFK, 1, 1, 1, 1)]
        [InlineData(LocationType.MLE, LocationType.JFK, 3, 1, 2, 0)]
        [InlineData(LocationType.NAS, LocationType.JFK, 2, 1, 0, 1)]
        [InlineData(LocationType.PLS, LocationType.JFK, 2, 1, 1, 1)]
        [InlineData(LocationType.ACC, LocationType.JFK, 4, 0, 0, 1)]
        [InlineData(LocationType.BGI, LocationType.JFK, 3, 0, 0, 1)]
        [InlineData(LocationType.ANU, LocationType.JFK, 5, 0, 0, 1)]
        [InlineData(LocationType.SVD, LocationType.JFK, 6, 0, 0, 1)]
        [InlineData(LocationType.GND, LocationType.JFK, 7, 0, 0, 1)]
        [InlineData(LocationType.UVF, LocationType.JFK, 8, 0, 0, 1)]
        [InlineData(LocationType.GVA, LocationType.JFK, 1, 1, 1, 1)]
        [InlineData(LocationType.TLV, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.DEL, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.BOM, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.BLR, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.MLE, LocationType.LHR, 2, 1, 1, 1)]
        [InlineData(LocationType.NAS, LocationType.LHR, 2, 1, 0, 1)]
        [InlineData(LocationType.PLS, LocationType.LHR, 2, 0, 1, 1)]
        [InlineData(LocationType.ACC, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.UVF, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.SVD, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.ANU, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.BGI, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.LOS, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.GND, LocationType.LHR, 1, 1, 1, 1)]
        public void OfferPriceGraphQl_LocalRun_NewRoutesRollout_OneWay_SummaryValidatesPricing(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            softAssert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);

            softAssert.True(GraphQlAssertions.ValidateTotalTaxAndAmountInSummary(JourneyType.OneWay, offerSearchResponse, offerPriceResponse, offerId, out string totalTaxAndAmount), totalTaxAndAmount);

            softAssert.AssertAll();
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

        private static string[] FindFareLinksForSpecificCabinAndAirline(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            TestConfig config = new TestConfig();
            config.Search.FareFamilyType = fareFamilyType;
            config.Search.FlightType = flightType;

            string[] fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(config, json);
            if (fareLinks.Any())
            {
                return fareLinks;
            }

            string[] fallbackFareLinks = OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(json, fareFamilyType);
            if (fallbackFareLinks.Any())
            {
                return fallbackFareLinks;
            }

            return OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(json, "fare");
        }

        private static string[] FindSummaryLinksForSpecificCabinAndAirline(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            TestConfig config = new TestConfig();
            config.Search.FareFamilyType = fareFamilyType;
            config.Search.FlightType = flightType;
            config.Search.JourneyType = JourneyType.OneWay;

            // Preferred output is summary links; final fallback returns fare links so callers can still resolve an offer id.
            string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForAirlineAndFareFamily(config, json);
            if (summaryLinks.Any())
            {
                return summaryLinks;
            }

            string[] fallbackSummaryLinks = OfferSearchHelper.FindAllSummaryLinksForFareFamilyAndOptionalAirline(json, fareFamilyType);
            if (fallbackSummaryLinks.Any())
            {
                return fallbackSummaryLinks;
            }

            string[] anySummaryLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(json, "summary");
            if (anySummaryLinks.Any())
            {
                return anySummaryLinks;
            }

            return OfferSearchHelper.FindAllFareLinksForFareFamilyAndOptionalAirline(json, fareFamilyType);
        }

        private static string[] FindSummaryLinksForSpecificAirline(string json, FlightType flightType)
        {
            return OfferSearchHelper.FindAllSummaryLinksForSpecificAirline(json, flightType);
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


