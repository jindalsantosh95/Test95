using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Requests.Endpoints;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class SearchMultiCityGraphQl
    {
        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.DEL, LocationType.DEL, LocationType.LHR })] //Same Origin and Destination
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.JFK })] //Origin and Destination different
        public void SearchMultiCityGraphQl_TC01_ThreeSlices_DirectVirginFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

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

            Assert.True(fareLinks.Any(), $"Slice - 0: No eligible fare links were found matching the requested criteria. Response: {offerSearchResult}");

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: false);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest);
            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchSliceOneResponse))
                return;
            offerSearchSliceOneResponse.EnsureGQLResponseSuccess();

            string offerSearchSliceOneResult = offerSearchSliceOneResponse.ReadResponseAsString();

            string[] fareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.VS);
            if (!fareLinksSliceOne.Any())
            {
                // Dynamic inventory can return this slice as direct or one-stop, and on partner metal.
                string[] fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.DL);
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceOneResult, FlightType.VS);
                }
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceOneResult, FlightType.DL);
                }
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceOneResult);
                }
                Assert.True(fallbackSliceOneFareLinks.Any(), $"Slice - 1: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");
                fareLinksSliceOne = fallbackSliceOneFareLinks;
            }

            Assert.True(fareLinksSliceOne.Any(), $"Slice - 1: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinksSliceOne, randomOffer: true);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest);
            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchSliceTwoResponse))
                return;
            offerSearchSliceTwoResponse.EnsureGQLResponseSuccess();

            string offerSearchSliceTwoResult = offerSearchSliceTwoResponse.ReadResponseAsString();

            string[] summaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.VS, "summary");
            if (!summaryLinksSliceTwo.Any())
            {
                string[] fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.DL, "summary");
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceTwoResult, FlightType.VS, "summary");
                }
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceTwoResult, FlightType.DL, "summary");
                }
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceTwoResult, "summary");
                }
                Assert.True(fallbackSummaryLinksSliceTwo.Any(), $"Slice - 2: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");
                summaryLinksSliceTwo = fallbackSummaryLinksSliceTwo;
            }

            Assert.True(summaryLinksSliceTwo.Any(), $"Slice - 2: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinksSliceTwo, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.IAD })] //Same Origin and Destination
        [InlineData(new LocationType[] { LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL })] //Origin and Destination different
        public void SearchMultiCityGraphQl_TC02_ThreeSlices_VirginCodeshareFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

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

            Assert.True(fareLinks.Any(), $"Slice - 0: No eligible fare links were found matching the requested criteria. Response: {offerSearchResult}");

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            string offerSearchSliceOneResult = offerSearchSliceOneResponse.ReadResponseAsString();

            string[] fareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForVirginCodeShareWithOneStopByFlightShareCode(offerSearchSliceOneResult, "6E");

            Assert.True(fareLinksSliceOne.Any(), $"Slice - 1: No codeshare flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinksSliceOne, randomOffer: true);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.JFK, LocationType.JFK, LocationType.LHR })] //Same Origin and Destination
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.JFK, LocationType.JFK, LocationType.DEL })] //Origin and Destination different
        public void SearchMultiCityGraphQl_TC03_ThreeSlices_DirectWithStopFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchResult, FlightType.VS);
            if (!fareLinks.Any())
            {
                // Route inventory is dynamic; fallback to direct VS fares so the multi-slice flow still validates.
                string[] fallbackFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchResult, FlightType.VS);
                Assert.True(fallbackFareLinks.Any(), $"Slice - 0: No one-stop or direct flights were found matching the requested criteria. Response: {offerSearchResult}");
                fareLinks = fallbackFareLinks;
            }

            Assert.True(fareLinks.Any(), $"Slice - 0: No one-stop flights were found matching the requested criteria. Response: {offerSearchResult}");

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            string offerSearchSliceOneResult = offerSearchSliceOneResponse.ReadResponseAsString();

            string[] fareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.VS);
            if (!fareLinksSliceOne.Any())
            {
                // Dynamic inventory can return this slice as direct or one-stop, and on partner metal.
                string[] fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.DL);
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceOneResult, FlightType.VS);
                }
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceOneResult, FlightType.DL);
                }
                if (!fallbackSliceOneFareLinks.Any())
                {
                    fallbackSliceOneFareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceOneResult);
                }
                Assert.True(fallbackSliceOneFareLinks.Any(), $"Slice - 1: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");
                fareLinksSliceOne = fallbackSliceOneFareLinks;
            }

            Assert.True(fareLinksSliceOne.Any(), $"Slice - 1: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinksSliceOne, randomOffer: true);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();
            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchSliceTwoResponse))
            {
                Assert.True(
                    GraphQlAssertions.HasGraphQlErrors(offerSearchSliceTwoResponse),
                    $"Slice - 2 returned an invalid payload shape without GraphQL errors. Request: {offerSearchGQLSliceTwoRequest}. Response: {offerSearchSliceTwoResponse.ReadResponseAsString()}");
                return;
            }

            string offerSearchSliceTwoResult = offerSearchSliceTwoResponse.ReadResponseAsString();

            string[] summaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.VS, "summary");
            if (!summaryLinksSliceTwo.Any())
            {
                string[] fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.DL, "summary");
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceTwoResult, FlightType.VS, "summary");
                }
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(offerSearchSliceTwoResult, FlightType.DL, "summary");
                }
                if (!fallbackSummaryLinksSliceTwo.Any())
                {
                    fallbackSummaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(offerSearchSliceTwoResult, "summary");
                }
                Assert.True(fallbackSummaryLinksSliceTwo.Any(), $"Slice - 2: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");
                summaryLinksSliceTwo = fallbackSummaryLinksSliceTwo;
            }

            Assert.True(summaryLinksSliceTwo.Any(), $"Slice - 2: No direct or one-stop flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinksSliceTwo, randomOffer: true);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.DEL, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.DEL })] //Same Origin and Destination
        [InlineData(new LocationType[] { LocationType.DEL, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.AMS })] //Origin and Destination different
        public void SearchMultiCityGraphQl_TC04_ThreeSlices_MultiSegmentFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, numberOfChildren: 1, numberOfYouths: 1, numberOfInfants: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            string offerSearchResult = offerSearchResponse.ReadResponseAsString();

            string[] fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForMultiSegmentsFlight(offerSearchResult);

            Assert.True(fareLinks.Any(), $"Slice - 0: No multi segments flights were found matching the requested criteria. Response: {offerSearchResult}");

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: false);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = GetFirstOfferId(offerSearchSliceOneResponse, "Slice - 1");

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = GetFirstOfferId(offerSearchSliceTwoResponse, "Slice - 2");

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.LHR })]
        [InlineData(new LocationType[] { LocationType.SEA, LocationType.AMS, LocationType.AMS, LocationType.JFK, LocationType.JFK, LocationType.LHR })]
        [InlineData(new LocationType[] { LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL })]
        public void SearchMultiCityGraphQl_TC05_ThreeSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = GetFirstOfferId(offerSearchResponse, "Slice - 0");

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = GetFirstOfferId(offerSearchSliceOneResponse, "Slice - 1");

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = GetFirstOfferId(offerSearchSliceTwoResponse, "Slice - 2");

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceTwoOfferId);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.LHR, LocationType.LHR, LocationType.DEL })]
        [InlineData(new LocationType[] { LocationType.SEA, LocationType.AMS, LocationType.AMS, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.BLR })]
        [InlineData(new LocationType[] { LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, LocationType.ATL, LocationType.LHR })]
        public void SearchMultiCityGraphQl_TC06_FourSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice3Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceTwoOfferId, basketId: basketId, currentTripIndexId: "3");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice3Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice3Request).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceThreeOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice3Response);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceThreeOfferId, currentTripIndexId: 4);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        [Theory]
        [Trait("Category", Traits.SearchGraphQLRegression)]
        [InlineData(new LocationType[] { LocationType.BOS, LocationType.AMS, LocationType.AMS, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.BLR, LocationType.BLR, LocationType.DEL, LocationType.DEL, LocationType.CDG })]
        [InlineData(new LocationType[] { LocationType.LON, LocationType.JFK, LocationType.JFK, LocationType.AMS, LocationType.AMS, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR })]
        public void SearchMultiCityGraphQl_TC07_SixSlices_BaselineFlow_ValidatesSummary(LocationType[] locations)
        {
            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse))
                return;

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: basketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: basketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice3Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceTwoOfferId, basketId: basketId, currentTripIndexId: "3");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice3Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice3Request).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceThreeOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice3Response);

            //generating offer search graph ql slice 4 request.
            string offerSearchGQLSlice4Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceThreeOfferId, basketId: basketId, currentTripIndexId: "4");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice4Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice4Request).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceFourOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice4Response);

            //generating offer search graph ql slice 5 request.
            string offerSearchGQLSlice5Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceFourOfferId, basketId: basketId, currentTripIndexId: "5");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice5Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice5Request).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceFiveOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice5Response);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = BuildOfferPriceRequest(basketId, sliceFiveOfferId, currentTripIndexId: 6);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination), originAndDestination);
        }

        private static string BuildOfferPriceRequest(string basketId, string offerId, int currentTripIndexId = 3)
        {
            TestConfig config = new TestConfig();
            config.Search.JourneyType = JourneyType.MultiCity;
            config.Internal.BasketId = basketId;

            return OfferSearchHelper.OfferPriceNewGQLRequest(config, offerId, currentTripIndexId: currentTripIndexId);
        }

        private static string GetFirstOfferId(HttpResponseMessage response, string sliceName)
        {
            string responseBody = response.ReadResponseAsString();
            string[] offerLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(responseBody);
            if (!offerLinks.Any())
            {
                offerLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(responseBody, "summary");
            }

            Assert.True(
                offerLinks.Any(),
                $"{sliceName}: No eligible offer links were returned. Response: {responseBody}");

            return OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(offerLinks, randomOffer: false);
        }
    }
}



