using System.Collections.Concurrent;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Search.Constants;
using GraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class OfferSearchGraphQl
    {
        private readonly ITestOutputHelper _output;
        private bool requireUsableOfferForSignOff;
        private static readonly ConcurrentDictionary<string, string> WamosLegOperatingCarrierDepartureDateCache = new ConcurrentDictionary<string, string>();

        public OfferSearchGraphQl(ITestOutputHelper output)
        {
            _output = output;
        }

        public static object[][] VaFlightsSearchTestData =>
            new object[][]
            {
                new object[] { LocationType.LHR, LocationType.LOS, 2, 0, 0, 0 },
                new object[] { LocationType.LHR, LocationType.DEL, 4, 4, 1, 0 },
                new object[] { LocationType.LHR, LocationType.MBJ, 1, 1, 1, 1 },
                new object[] { LocationType.MAN, LocationType.ATL, 1, 1, 1, 1 },
                new object[] { LocationType.IAD, LocationType.LHR, 4, 0, 0, 1 }
            };

        public static object[][] FlexiDateRoutesTestData =>
            new object[][]
            {
                new object[] { LocationType.MAN, LocationType.BGI, 2, 0, 2, 0 },
                new object[] { LocationType.LHR, LocationType.UVF, 3, 0, 0, 2 },
                new object[] { LocationType.LHR, LocationType.DXB, 1, 1, 1, 1 }
            };

        public static object[][] WamosLegOperatingCarrierRoutesTestData =>
            new object[][]
            {
                new object[] { JourneyType.OneWay, LocationType.MAN, LocationType.ATL },
                new object[] { JourneyType.Return, LocationType.MAN, LocationType.ATL },
                new object[] { JourneyType.OneWay, LocationType.MAN, LocationType.JFK },
                new object[] { JourneyType.Return, LocationType.MAN, LocationType.JFK },
                new object[] { JourneyType.OneWay, LocationType.ATL, LocationType.MAN },
                new object[] { JourneyType.Return, LocationType.ATL, LocationType.MAN },
                new object[] { JourneyType.OneWay, LocationType.JFK, LocationType.MAN },
                new object[] { JourneyType.Return, LocationType.JFK, LocationType.MAN }
            };

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void OfferSearchGraphQl_TC01_OneWay_BaselineSearchReturnsCalendarOriginDestinationAndAircraft()
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"OfferMS sign-off requires usable one-way results. OfferSearch response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertOneWayOfferSearchResponse(offerSearchResult, offerSearchGQLRequest);
        }

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "GraphQL")]
        public void SharedServicesSignOff_OfferSearch_OneWay()
        {
            requireUsableOfferForSignOff = true;
            OfferSearchGraphQl_TC01_OneWay_BaselineSearchReturnsCalendarOriginDestinationAndAircraft();
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void OfferSearchGraphQl_TC02_ReturnTrip_BaselineSearchReturnsInboundAndPriceGrid()
        {
            string deptartureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfAdults: 1, numberOfChildren: 0, numberOfYouths: 0, numberOfInfants: 0);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"OfferMS sign-off requires usable outbound results before the return search. OfferSearch response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertReturnOfferSearchResponse(offerSearchResult, offerSearchReturnGQLRequest, offerSearchReturnResult, includeAirCraftInfo: true);
        }

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "GraphQL")]
        public void SharedServicesSignOff_OfferSearch_ReturnTrip()
        {
            requireUsableOfferForSignOff = true;
            OfferSearchGraphQl_TC02_ReturnTrip_BaselineSearchReturnsInboundAndPriceGrid();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(VaFlightsSearchTestData))]
        public void OfferSearchGraphQl_TC03_OneWay_VAFlights_MultipleRoutesReturnExpectedSearchData(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertOneWayOfferSearchResponse(offerSearchResult, offerSearchGQLRequest);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(FlexiDateRoutesTestData))]
        public void OfferSearchGraphQl_TC04_OneWay_FlexiDateFlowReturnsExpectedSearchData(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string currentDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, currentDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest, "").EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertOneWayOfferSearchResponse(offerSearchResult, offerSearchGQLRequest);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(VaFlightsSearchTestData))]
        public void OfferSearchGraphQl_TC05_ReturnTrip_VAFlights_MultipleRoutesReturnInboundAndPriceGrid(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDate = OfferSearchHelper.DepartureDate();
            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertReturnOfferSearchResponse(offerSearchResult, offerSearchReturnGQLRequest, offerSearchReturnResult, includeAirCraftInfo: true);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [MemberData(nameof(FlexiDateRoutesTestData))]
        public void OfferSearchGraphQl_TC06_ReturnTrip_FlexiDateFlowReturnsInboundAndPriceGrid(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string currentDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql flexi date request.
            string offerSearchFlexiDateGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, currentDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, flexiDateSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFlexiResponse = GraphQLEndpoint.OfferSearch(offerSearchFlexiDateGQLRequest, "").EnsureGQLResponseSuccess();

            string departureDate = OfferSearchHelper.ReturnOneWayDepartureDateGQL(offerSearchFlexiResponse);

            string returnDate = OfferSearchHelper.ReturnDate(departureDate);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from search graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search return graph ql request.
            string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();

            dynamic offerSearchReturnResult = JsonConvert.DeserializeObject<dynamic>(offerReturnSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            AssertReturnOfferSearchResponse(offerSearchResult, offerSearchReturnGQLRequest, offerSearchReturnResult, includeAirCraftInfo: false);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS, 2, 0, 0, 0, FlightType.VS)]
        [InlineData(LocationType.BOS, LocationType.LHR, 3, 0, 0, 1, FlightType.AF)]
        public void OfferSearchGraphQl_TC07_OneWay_AirlineFilterReturnsRequestedOperatingCarrier(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, FlightType flightType)
        {
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql filter request.
            string offerSearchFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", airlineFilter: flightType.ToString(), sortRequestType: "RECOMMENDED", basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchFilterRequest, "").EnsureGQLResponseSuccess();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateCalenderGQLSearchResponse(offerSearchResult, out string calendar), calendar);

            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchFilterRequest, offerSearchResult, out string originDestination), originDestination);

            softAssert.True(GraphQlAssertions.ValidateAirCraftInfo(offerSearchResult, out string airCraftInfo), airCraftInfo);

            softAssert.True(GraphQlAssertions.ValidateOfferSearchFilterResult(offerSearchResult, flightType.ToString(), out string filterInfo), filterInfo);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.ATL, 3, 0, 0, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 3, 0, 0, 1, 2)]
        public void OfferSearchGraphQl_TC08_OneWay_ConnectionsFilterReturnsRequestedStopCount(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, int stopCounts)
        {
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql filter request.
            string offerSearchFilterRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", connectionFilter: stopCounts.ToString(), basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchFilterResponse = GraphQLEndpoint.OfferSearch(offerSearchFilterRequest, "").EnsureGQLResponseSuccess();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchFilterResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateCalenderGQLSearchResponse(offerSearchResult, out string calendar), calendar);

            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchFilterRequest, offerSearchResult, out string originDestination), originDestination);

            softAssert.True(GraphQlAssertions.ValidateOfferSearchConnectionsFilterResult(offerSearchResult, stopCounts, out string connectionsFiltersInfo), connectionsFiltersInfo);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS, 2, 0, 0, 0, FareFamilyType.ECONOMYLIGHT)]
        [InlineData(LocationType.LHR, LocationType.LOS, 1, 1, 1, 1, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        public void OfferSearchGraphQl_TC09_OneWay_PriceSortReturnsAscendingFaresForFareFamily(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants, FareFamilyType fareFamilyType)
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

            //generating offer search graph ql sort request.
            string offerSearchSortRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", airlineFilter: "-", sortRequestType: "PRICE", basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSortResponse = GraphQLEndpoint.OfferSearch(offerSearchSortRequest).EnsureGQLResponseSuccess();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchSortResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchSortRequest, offerSearchResult, out string originDestination), originDestination);

            softAssert.True(GraphQlAssertions.ValidateOfferSearchPriceSortResult(offerSearchResult, fareFamilyType, out string filterInfo), filterInfo);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS, 2, 0, 0, 0)]
        [InlineData(LocationType.DEL, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.BOS, LocationType.LHR, 3, 0, 0, 1)]
        public void OfferSearchGraphQl_TC10_OneWay_DepartureTimeSortReturnsAscendingResults(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql sort request.
            string offerSearchSortRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", airlineFilter: "-", sortRequestType: "DEPARTURE_TIME", basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSortResponse = GraphQLEndpoint.OfferSearch(offerSearchSortRequest, "").EnsureGQLResponseSuccess();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchSortResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchSortRequest, offerSearchResult, out string originDestination), originDestination);

            softAssert.True(GraphQlAssertions.ValidateOfferSearchDepartureTimeSortResult(offerSearchResult, out string depTimeSortInfo), depTimeSortInfo);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS, 2, 0, 0, 0)]
        [InlineData(LocationType.DEL, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.BOS, LocationType.LHR, 3, 0, 0, 1)]
        public void OfferSearchGraphQl_TC11_OneWay_DurationSortReturnsAscendingResults(LocationType origin, LocationType destination, int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            string departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //generating offer search graph ql sort request.
            string offerSearchSortRequest = OfferSearchHelper.OfferSearchGQLSortAndFiltersRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfAdults: numberOfAdults, numberOfChildren: numberOfChildren, numberOfYouths: numberOfYouths, numberOfInfants: numberOfInfants, pqId: "searchOffersBFF", airlineFilter: "-", sortRequestType: "DURATION", basketId: basketId);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSortResponse = GraphQLEndpoint.OfferSearch(offerSearchSortRequest, "").EnsureGQLResponseSuccess();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchSortResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchSortRequest, offerSearchResult, out string originDestination), originDestination);

            softAssert.True(GraphQlAssertions.ValidateOfferSearchDurationTimeSortResult(offerSearchResult, out string depTimeSortInfo), depTimeSortInfo);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLFunctional)]
        [MemberData(nameof(WamosLegOperatingCarrierRoutesTestData))]
        public async Task OfferSearchGraphQl_TC13_WamosOperatedLegReturnsLegOperatingCarrierDetails(JourneyType journeyType, LocationType origin, LocationType destination)
        {
            (string departureDate, string offerSearchGQLRequest, JToken offerSearchResult, JToken offerSearchReturnResult) =
                await FindWamosOperatedOfferSearchResponse(journeyType, origin, destination);

            _output.WriteLine($"Wamos-operated leg date selected: JourneyType={journeyType}, Route={origin}-{destination}, DepartureDate={departureDate}");

            bool outboundContainsWamos = GraphQlAssertions.ValidateLegOperatingAirlineDetails(offerSearchResult, "EB", "Wamos", out string outboundMessage);

            if (journeyType != JourneyType.Return)
            {
                Assert.True(outboundContainsWamos, outboundMessage);
                return;
            }

            Assert.NotNull(offerSearchReturnResult);
            bool inboundContainsWamos = GraphQlAssertions.ValidateLegOperatingAirlineDetails(offerSearchReturnResult, "EB", "Wamos", out string inboundMessage);

            Assert.True(
                outboundContainsWamos || inboundContainsWamos,
                $"No Wamos-operated leg was returned on either direction. Outbound: {outboundMessage} Inbound: {inboundMessage}");
        }

        private static async Task<(string DepartureDate, string Request, JToken Result, JToken ReturnResult)> FindWamosOperatedOfferSearchResponse(JourneyType journeyType, LocationType origin, LocationType destination)
        {
            string cacheKey = BuildWamosLegOperatingCarrierDepartureDateCacheKey(journeyType, origin, destination);
            List<string> attempts = new List<string>();
            DateTime startDate = DateTime.Today.AddDays(21);
            DateTime endDate = DateTime.Today.AddDays(42);
            List<DateTime> candidateDates = new List<DateTime>();

            if (WamosLegOperatingCarrierDepartureDateCache.TryGetValue(cacheKey, out string cachedDepartureDate))
            {
                candidateDates.Add(DateTime.ParseExact(cachedDepartureDate, "yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture));
            }

            for (DateTime candidateDate = startDate; candidateDate <= endDate; candidateDate = candidateDate.AddDays(1))
            {
                if (!candidateDates.Contains(candidateDate))
                {
                    candidateDates.Add(candidateDate);
                }
            }

            foreach (DateTime candidateDate in candidateDates)
            {
                string departureDate = candidateDate.ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);
                string returnDate = OfferSearchHelper.ReturnDate(departureDate);
                string offerSearchGQLRequest = journeyType == JourneyType.Return
                    ? BuildOfferSearchRequest(JourneyType.Return, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: 1)
                    : BuildOfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: 1);

                try
                {
                    using HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, string.Empty).EnsureIsSuccessStatusCode();

                    if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse, writeAcceptedNoResultsDiagnostic: false))
                    {
                        attempts.Add($"{departureDate}: accepted no-results response");
                        continue;
                    }

                    JToken offerSearchResult = JToken.Parse(await offerSearchResponse.Content.ReadAsStringAsync());

                    bool outboundContainsWamos = GraphQlAssertions.ValidateLegOperatingAirlineDetails(offerSearchResult, "EB", "Wamos", out string outboundMessage);

                    if (journeyType != JourneyType.Return)
                    {
                        if (outboundContainsWamos)
                        {
                            WamosLegOperatingCarrierDepartureDateCache.TryAdd(cacheKey, departureDate);
                            return (departureDate, offerSearchGQLRequest, offerSearchResult, null);
                        }

                        attempts.Add($"{departureDate}: {outboundMessage}");
                        continue;
                    }

                    string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
                    string offerId = outboundContainsWamos
                        ? GetFirstOfferIdForLegOperatingAirline(offerSearchResult, "EB")
                        : GetFirstOfferIdForOperatingAirline(offerSearchResult, "VS");
                    string offerSearchReturnGQLRequest = BuildOfferSearchRequest(JourneyType.Return, offerId: offerId, basketId: basketId);

                    using HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId).EnsureGQLResponseSuccess();
                    JToken offerSearchReturnResult = JToken.Parse(await offerReturnSearchResponse.Content.ReadAsStringAsync());
                    bool inboundContainsWamos = GraphQlAssertions.ValidateLegOperatingAirlineDetails(offerSearchReturnResult, "EB", "Wamos", out string inboundMessage);

                    if (outboundContainsWamos || inboundContainsWamos)
                    {
                        WamosLegOperatingCarrierDepartureDateCache.TryAdd(cacheKey, departureDate);
                        return (departureDate, offerSearchGQLRequest, offerSearchResult, offerSearchReturnResult);
                    }

                    attempts.Add($"{departureDate}: neither direction matched EB/Wamos. Outbound: {outboundMessage} Inbound: {inboundMessage}");
                }
                catch (Exception ex)
                {
                    attempts.Add($"{departureDate}: probe failed with {ex.GetType().Name}: {ex.Message}");
                }
            }

            throw new Exception($"No Wamos-operated leg was found for {journeyType} {origin}-{destination} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}. Attempts: {string.Join(" | ", attempts)}");
        }

        private static string BuildWamosLegOperatingCarrierDepartureDateCacheKey(JourneyType journeyType, LocationType origin, LocationType destination)
        {
            return string.Join("|", journeyType, origin, destination);
        }

        private static string GetFirstOfferIdForLegOperatingAirline(JToken offerSearchResult, string operatingAirlineCode)
        {
            JToken[] matchingFlightsAndFares = offerSearchResult
                .SelectTokens("$..result.slice.flightsAndFares[*]")
                .Where(flightAndFare => flightAndFare
                    .SelectTokens("flight.segments[*].legs[*].operatingAirline.code")
                    .Any(code => code.ToString().Is(operatingAirlineCode)))
                .ToArray();

            string offerId = matchingFlightsAndFares
                .SelectMany(flightAndFare => flightAndFare.SelectTokens("fares[*].id"))
                .Select(id => id.ToString())
                .FirstOrDefault(id => !id.IsNullOrWhiteSpace());

            if (!offerId.IsNullOrWhiteSpace())
            {
                return offerId;
            }

            string summaryLink = matchingFlightsAndFares
                .SelectMany(flightAndFare => flightAndFare.SelectTokens("fares[*].links[?(@.rel == 'summary')].href"))
                .Select(link => link.ToString())
                .FirstOrDefault(link => !link.IsNullOrWhiteSpace());

            if (!summaryLink.IsNullOrWhiteSpace())
            {
                return summaryLink.Split('/').Last();
            }

            throw new Exception($"No offer id or summary link was returned for an offer with leg operatingAirline '{operatingAirlineCode}'. ResponsePreview: {GraphQlAssertions.BuildJsonPreview(offerSearchResult)}");
        }

        private static string GetFirstOfferIdForOperatingAirline(JToken offerSearchResult, string operatingAirlineCode)
        {
            JToken[] matchingFlightsAndFares = offerSearchResult
                .SelectTokens("$..result.slice.flightsAndFares[*]")
                .Where(flightAndFare =>
                {
                    JToken[] segments = flightAndFare.SelectTokens("flight.segments[*]").ToArray();
                    return segments.Any()
                        && segments.All(segment => segment.SelectToken("operatingAirline.code")?.ToString().Is(operatingAirlineCode) == true);
                })
                .ToArray();

            string offerId = matchingFlightsAndFares
                .SelectMany(flightAndFare => flightAndFare.SelectTokens("fares[*].id"))
                .Select(id => id.ToString())
                .FirstOrDefault(id => !id.IsNullOrWhiteSpace());

            if (!offerId.IsNullOrWhiteSpace())
            {
                return offerId;
            }

            string summaryLink = matchingFlightsAndFares
                .SelectMany(flightAndFare => flightAndFare.SelectTokens("fares[*].links[?(@.rel == 'summary')].href"))
                .Select(link => link.ToString())
                .FirstOrDefault(link => !link.IsNullOrWhiteSpace());

            if (!summaryLink.IsNullOrWhiteSpace())
            {
                return summaryLink.Split('/').Last();
            }

            throw new InvalidOperationException($"No offer ID or summary link was returned for operating airline '{operatingAirlineCode}'. ResponsePreview: {GraphQlAssertions.BuildJsonPreview(offerSearchResult)}");
        }

        [Fact]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        public void OfferSearchGraphQl_TC12_OneWay_RewardSearchWithoutLoginReturnsExpectedError()
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = BuildOfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfAdults: 1, rewardSearch: true);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureIsSuccessStatusCode();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResults.ToString().Contains("Unhandled exception (Must be logged in to a valid account.)"), "Did not return expected error message");
        }

        private static void AssertOneWayOfferSearchResponse(dynamic offerSearchResult, string offerSearchGQLRequest)
        {
            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateCalenderGQLSearchResponse(offerSearchResult, out string calendar), calendar);
            softAssert.True(GraphQlAssertions.ValidateGQLOriginAndDestination(offerSearchGQLRequest, offerSearchResult, out string originDestination), originDestination);
            softAssert.True(GraphQlAssertions.ValidateAirCraftInfo(offerSearchResult, out string airCraftInfo), airCraftInfo);

            softAssert.AssertAll();
        }

        private static void AssertReturnOfferSearchResponse(dynamic offerSearchResult, string offerSearchReturnGQLRequest, dynamic offerSearchReturnResult, bool includeAirCraftInfo)
        {
            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(GraphQlAssertions.ValidateInboundSearchResult(offerSearchReturnGQLRequest, offerSearchReturnResult, out string inboundResult), inboundResult);
            softAssert.True(GraphQlAssertions.ValidateCalenderGQLSearchResponse(offerSearchResult, out string calendar), calendar);

            if (includeAirCraftInfo)
            {
                softAssert.True(GraphQlAssertions.ValidateAirCraftInfo(offerSearchResult, out string airCraftInfo), airCraftInfo);
            }

            softAssert.True(GraphQlAssertions.ValidatePriceGrid(offerSearchResult, out string priceGrid), priceGrid);

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



