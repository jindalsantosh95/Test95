using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Seat
{
    public class SeatPriceGQL
    {
        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Book")]
        public void SharedServicesSignOff_SeatPricing_GoldMember() =>
            Seat_Prices_Are_Zero_For_Gold_Member_Return(
                LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Gold", 1, 1, 1, 1);

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Gold", 1, 1, 1, 1)]
        public void Seat_Prices_Are_Zero_For_Gold_Member_Return(LocationType origin, LocationType destination, FareFamilyType fareFamily, string tier, int adults, int teens, int child, int infants)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult, randomOffer: false);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, true, tier);

            // Retrieve outbound seatmap
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            // Assert outbound prices are 0
            Assert.True(GraphQLAssertion.ValidateSeatPricesAreAllZero(retrieveSeatMapGQLResponse, out var result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            // Retrieve inbound seatmap
            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 1, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveInboundSeatMapGQLResponse);

            // Assert inbound prices are 0
            Assert.True(GraphQLAssertion.ValidateSeatPricesAreAllZero(retrieveInboundSeatMapGQLResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Gold", 1, 1, 1, 1, "Standard Seat", CabinName.VSCL)]
        public async Task Seat_Prices_Are_Zero_For_Gold_Member_After_Clear_Down_Return(LocationType origin, LocationType destination, FareFamilyType fareFamily, string tier, int adults, int teens, int child, int infants, string productName, CabinName cabinName)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult, randomOffer: false);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, true, tier);

            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            // Select seat
            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());

            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);
            var people = (JArray)retrieveBasket.data.retrieveBasket.people;

            GraphQLHelper.FindAvailableSeat(retrieveSeatMapGQLResponse, out string seatNumber, out double amount, out string seatCurrency, productName, cabinName);

            Assert.NotNull(seatNumber);

            foreach (var item in people)
            {
                if (item["type"].ToString() == "INFANT") break;
                int passengerId = Convert.ToInt32(item["id"]);

                var selectSeatREQ = GraphQLRequestBuilder.SeatSelectionGQLRequest(basketId, sectorId, seatOfferId, passengerId.ToString(), seatNumber, amount, seatCurrency);

                var seatSelectionGQLResponse = GraphQLEndpoint.Book(selectSeatREQ).EnsureGQLResponseSuccess();

                dynamic selectSeatGQLResult = JsonConvert.DeserializeObject<dynamic>(await seatSelectionGQLResponse.Content.ReadAsStringAsync());

                Assert.NotNull(selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber.Value);

                Assert.True(selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber == seatNumber, $"Selected seat number doesn't match with assigned SeatNumber for passenger {passengerId}\nPassenger Seat Number = {selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber}\nSelected Seat Number = {seatNumber}");

                Assert.True(GraphQLAssertion.ValidateColumnSeatSelected(seatSelectionGQLResponse, seatNumber, out message), message);

                GraphQLHelper.FindAvailableSeat(seatSelectionGQLResponse, out seatNumber, out amount, out seatCurrency, productName, cabinName);
            }

            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatAfterSeatSelectionGQLResponse, out message), message);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketAfterGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketAfterGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID doesn't match");

            //Try change the passenger information
            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out sectorId, out retrieveBasketGQLResponse, true, tier);

            //verify the basket update
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatAfterBaseketUpdateGQLResponse, out message), message);

            // Assert outbound prices are 0
            Assert.True(GraphQLAssertion.ValidateSeatPricesAreAllZero(retrieveSeatAfterBaseketUpdateGQLResponse, out var result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            // Retrieve inbound seatmap
            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 1, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveInboundSeatMapGQLResponse);

            // Assert inbound prices are 0
            Assert.True(GraphQLAssertion.ValidateSeatPricesAreAllZero(retrieveInboundSeatMapGQLResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.JNB, LocationType.LHR, "ZAR", FareFamilyType.ECON, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.CPT, LocationType.LHR, "ZAR", FareFamilyType.ECON, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.JNB, LocationType.JFK, "ZAR", FareFamilyType.ECON, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.CPT, LocationType.JFK, "ZAR", FareFamilyType.ECON, "Red", 1, 1, 1, 1)]
        public void Seat_Prices_Currency_Validation(LocationType origin, LocationType destination, string currency, FareFamilyType fareFamily, string tier, int adults, int teens, int child, int infants)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult, randomOffer: false);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, true, tier);

            // Retrieve outbound seatmap
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            // Assert outbound currency types
            Assert.True(GraphQLAssertion.ValidateSeatPriceCurrency(retrieveSeatMapGQLResponse, currency, out var result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            // Retrieve inbound seatmap
            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 1, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveInboundSeatMapGQLResponse);

            // Assert inbound currency types
            Assert.True(GraphQLAssertion.ValidateSeatPriceCurrency(retrieveInboundSeatMapGQLResponse, currency, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}
