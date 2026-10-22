using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    public class BasketGraphQLSummaryValidation : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        private readonly TestConfig _testConfig;

        public BasketGraphQLSummaryValidation(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 0, 0, "GBP")]
        [InlineData(LocationType.MAN, LocationType.ATL, 1, 1, 1, 0, "GBP")]
        [InlineData(LocationType.LHR, LocationType.LOS, 1, 2, 1, 1, "GBP")]
        [InlineData(LocationType.LHR, LocationType.LAS, 2, 3, 1, 0, "GBP")]
        [InlineData(LocationType.MAN, LocationType.SEA, 1, 1, 1, 1, "GBP")]
        [InlineData(LocationType.JFK, LocationType.LHR, 1, 0, 1, 1, "USD")]
        [InlineData(LocationType.LAS, LocationType.LHR, 1, 1, 0, 2, "USD")]
        [InlineData(LocationType.SEA, LocationType.LHR, 4, 0, 0, 0, "USD")]
        [InlineData(LocationType.JNB, LocationType.LHR, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.CPT, LocationType.LHR, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.JNB, LocationType.JFK, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.CPT, LocationType.JFK, 1, 0, 0, 0, "ZAR")]
        [InlineData(LocationType.BGI, LocationType.LHR, 1, 0, 0, 0, "BBD")]
        [InlineData(LocationType.BGI, LocationType.MAN, 1, 0, 0, 0, "BBD")]
        [InlineData(LocationType.ANU, LocationType.LHR, 1, 0, 0, 0, "JMD")]
        [InlineData(LocationType.SVD, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.GND, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.UVF, LocationType.LHR, 1, 0, 0, 0, "XCD")]
        [InlineData(LocationType.DXB, LocationType.LHR, 1, 0, 0, 0, "AED")]
        [InlineData(LocationType.PVG, LocationType.LHR, 1, 0, 0, 0, "CNY")]
        public async Task BasketSummaryCurrencyValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants, string expectedCurrency)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants);
            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Asserting the Basket Summary Currency
            Assert.True(GraphQLAssertion.ValidateBasketSummaryCurrency(offerPriceResult, retrieveBasketResult, expectedCurrency, out string currency), currency);
        }

        [Theory]
        [InlineData(LocationType.ANU, LocationType.LHR, "XCD")]  // Antigua & Barbuda - East Caribbean Dollar
        [InlineData(LocationType.DXB, LocationType.LHR, "AED")]  // Dubai, UAE - UAE Dirham
        [InlineData(LocationType.PVG, LocationType.LHR, "CNY")]  // Shanghai, China - Chinese Yuan
        [InlineData(LocationType.GND, LocationType.LHR, "XCD")]  // Grenada - East Caribbean Dollar
        [InlineData(LocationType.SEA, LocationType.LHR, "USD")]  // Seattle, USA - US Dollar
        [InlineData(LocationType.UVF, LocationType.LHR, "XCD")]  // St Vincent & Grenadines - East Caribbean Dollar
        [InlineData(LocationType.JNB, LocationType.LHR, "ZAR")]  // Johannesburg, South Africa - South African Rand
        [InlineData(LocationType.BGI, LocationType.LHR, "BBD")]  // Barbados - Barbadian Dollar
        public async Task BasketSummaryCurrencyANUValidation(LocationType origin, LocationType destination, string expectedCurrency)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, 0);

            // Group origins by their flight days
            var monThu = new[] { LocationType.ANU, LocationType.SEA };
            var monFri = new[] { LocationType.DXB };
            var tueSat = new[] { LocationType.PVG, LocationType.JNB, LocationType.BGI };
            var tueFri = new[] { LocationType.GND };
            var wedSun = new[] { LocationType.UVF };

            if (monThu.Contains(origin))
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate(DayOfWeek.Monday, DayOfWeek.Thursday);
            else if (monFri.Contains(origin))
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate(DayOfWeek.Monday, DayOfWeek.Friday);
            else if (tueSat.Contains(origin))
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate(DayOfWeek.Tuesday, DayOfWeek.Saturday);
            else if (tueFri.Contains(origin))
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate(DayOfWeek.Tuesday, DayOfWeek.Friday);
            else if (wedSun.Contains(origin))
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate(DayOfWeek.Wednesday, DayOfWeek.Sunday);
            else
                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate();

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECON, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Asserting the Basket Summary Currency
            Assert.True(GraphQLAssertion.ValidateBasketSummaryCurrency(offerPriceResult, retrieveBasketResult, expectedCurrency, out string currency), currency);
        }


        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(1, 0, 0, 0)]
        [InlineData(1, 1, 1, 0)]
        [InlineData(1, 2, 1, 1)]
        [InlineData(2, 3, 1, 0)]
        [InlineData(1, 1, 1, 1)]
        [InlineData(1, 0, 1, 1)]
        [InlineData(1, 1, 0, 2)]
        [InlineData(4, 0, 0, 0)]
        public async Task BasketSummaryPassengerCount(int adults, int teens, int child, int infants)
        {
            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants);
            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResult, FareFamilyType.ECONOMYDELIGHT, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Asserting the Basket Passenger count
            Assert.True(GraphQLAssertion.ValidateBasketSummaryPassengerCount(retrieveBasketResult, adults, teens, child, infants, out string passengers), passengers);
        }


        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYLIGHT, 2, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, FareFamilyType.ECONOMYDELIGHT, 2, 0, 1, 1)]
        public async Task BasketSummaryPricePerPassengerValidation(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.VS);

            Assert.True(fareLinks.Any(), "Fare link not returned for the cabin and flight type passed");

            //getting basket id and offer id from fare link
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search return graph ql request.
            var offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, flexiDateSearch: false, calenderSearch: false, offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            var offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            var offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchReturnResult, fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerReturnId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Asserting the Basket PricePerPerson Passenger
            Assert.True(GraphQLAssertion.ValidateBasketSummaryPassengerPrice(retrieveBasketResult, adults, teens, child, infants, out string passengers), passengers);
        }


        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [Trait("Category", Traits.NbfGraphQLCICD)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONOMYLIGHT, 1, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, FareFamilyType.ECONOMYDELIGHT, 1, 0, 1, 1)]
        public async Task BasketSummaryTotalPriceValidation(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            var offerSearchResult = offerSearchResponse.ReadResponseAsString();

            var fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(offerSearchResult, fareFamily, FlightType.VS);

            Assert.True(fareLinks.Any(), "Fare link not returned for the cabin and flight type passed");

            //getting basket id and offer id from fare link
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search return graph ql request.
            var offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, flexiDateSearch: false, calenderSearch: false, offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            var offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            var offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchReturnResult, fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), "Summary link not returned for the cabin and flight type passed");

            //getting offer id from summary link
            string offerReturnId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.Return, basketId, offerReturnId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Asserting the Basket PricePerPerson Passenger
            Assert.True(GraphQLAssertion.ValidateBasketSummaryTotalPrice(offerPriceResult, retrieveBasketResult, out string passengers), passengers);
        }
    }
}