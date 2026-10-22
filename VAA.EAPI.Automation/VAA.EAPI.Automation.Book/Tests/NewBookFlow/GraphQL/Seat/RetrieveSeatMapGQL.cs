using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Seat
{
    public class RetrieveSeatMapGQL
    {
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONOMYDELIGHT, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONOMYLIGHT, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONCLASSICFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONDELIGHTFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.COMFORTPLUSPREMIUMECON, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JNB, FareFamilyType.BUSFIRST, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.BUSFIRSTFLEXIBLE, 1, 1, 1, 1)]
        public async Task Retrieve_Validation_OneWay(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            dynamic retrieveSeatMapGQLResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());

            var operatingCarrier = retrieveSeatMapGQLResult.data.retrieveSeatMap.flightSegment.operatingCarrier.Value;
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECON, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONOMYDELIGHT, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONOMYLIGHT, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONCLASSICFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONDELIGHTFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.COMFORTPLUSPREMIUMECON, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JNB, FareFamilyType.BUSFIRST, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.BUSFIRSTFLEXIBLE, 1, 1, 1, 1)]
        public async Task Retrieve_Validation_Return(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            dynamic retrieveSeatMapGQLResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());

            var operatingCarrier = retrieveSeatMapGQLResult.data.retrieveSeatMap.flightSegment.operatingCarrier.Value;

            //Validation all segment Sector ID are not same
            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            //Calling Retrieve SeatMap with respect to inbound and outbound sector
            GraphQLHelper.RetrieveSeatMapForAllSectors(basketId, fareFamily, retrieveBasketGQLResponse);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.BOS, FareFamilyType.ECONOMYDELIGHT, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.COMFORTPLUSPREMIUMECON, 1, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, FareFamilyType.BUSFIRST, 1, 1, 1, 1)]
        public void Retrieve_Validation_Delta_OneWay(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily, FlightType.DL, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, 1, 1, 1, 1)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Book")]
        [Trait("TestType", "Contract")]
        public void Retrieve_Validation_Delta_oneway(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.DL, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorID, out HttpResponseMessage retrieveBasketGQLResponse);

            //Validation all segment Sector ID are not same
            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            //Calling Retrieve SeatMap with respect to inbound and outbound sector
            GraphQLHelper.RetrieveSeatMapForAllSectors(basketId, fareFamily, retrieveBasketGQLResponse);
        }
    }
}
