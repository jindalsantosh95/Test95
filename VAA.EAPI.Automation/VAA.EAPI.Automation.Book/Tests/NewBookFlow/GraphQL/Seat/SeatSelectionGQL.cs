using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Seat
{
    public class SeatSelectionGQL 
    {
        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Book")]
        public void SharedServicesSignOff_SelectSeat_Return() =>
            SelectSeat_Return(
                LocationType.LHR, LocationType.JFK, FareFamilyType.ECON,
                "Standard Seat", CabinName.VSCL, 1, 1, 1, 0);

        //Discussed with developers and Return will fulfull Seats so no need for OneWay tests specifically
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Standard Seat", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECON, "Preferred Seat - Economy Cabin", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONOMYDELIGHT, "Economy Delight seat", CabinName.VSDT, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONCLASSICFLEXIBLE, "Standard Seat", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.ECONDELIGHTFLEXIBLE, "Economy Delight seat", CabinName.VSDT, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.COMFORTPLUSPREMIUMECON, "Premium Economy seat", CabinName.VSPE, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, "Premium Economy Cabin Seat", CabinName.VSPE, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JNB, FareFamilyType.BUSFIRST, "Upper Class seat", CabinName.VSUP, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, FareFamilyType.BUSFIRSTFLEXIBLE, "Upper Class seat", CabinName.VSUP, 1, 1, 1, 0)]
        public void SelectSeat_Return(LocationType origin, LocationType destination, FareFamilyType fareFamily, string productName, CabinName cabinName, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Validation all segment Sector ID are not same 
            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            //Calling Retrieve SeatMap and Select Seat with respect to inbound and outbound sector
            GraphQLHelper.SelectSeatsForAllSectors(basketId, fareFamily, retrieveBasketGQLResponse, cabinName, out double totalSeatPrice, productName);

            GraphQLHelper.RetrieveBasket(basketId, out HttpResponseMessage basketResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage aretrieveSeatMapGQLResponse, out string amessage), amessage);
        }


        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 1, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 0, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Exit Seat", CabinName.VSCL, 1, 0, 0, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONCLASSICFLEXIBLE, "Exit Seat", CabinName.VSCL, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONCLASSICFLEXIBLE, "Exit Seat", CabinName.VSCL, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONCLASSICFLEXIBLE, "Exit Seat", CabinName.VSCL, 1, 1, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONCLASSICFLEXIBLE, "Exit Seat", CabinName.VSCL, 1, 0, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECONCLASSICFLEXIBLE, "Exit Seat", CabinName.VSCL, 1, 0, 0, 1)]
        public async Task SelectSeatProximityReturn(LocationType origin, LocationType destination, FareFamilyType fareFamily, string productName, CabinName cabinName, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Validation all segment Sector ID are not same 
            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 0, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveOutboundSeatMapGqlResponse);

            if (child > 0 || infants > 0)
            {
                Assert.True(GraphQLAssertion.CheckProximity(retrieveOutboundSeatMapGqlResponse, productName, cabinName, out string errorMessage), errorMessage);
            }
            else
            {
                Assert.False(GraphQLAssertion.CheckProximity(retrieveOutboundSeatMapGqlResponse, productName, cabinName, out string errorMessage), errorMessage);
            }

            GraphQLHelper.SelectSeatsForSlice(basketId, 0, fareFamily, retrieveBasketGQLResponse, cabinName, "Standard Seat");

            GraphQLHelper.RetrieveBasket(basketId, out HttpResponseMessage basketResponse);

            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 1, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveInboundSeatMapGqlResponse);

            if (child > 0 || infants > 0)
            {
                Assert.True(GraphQLAssertion.CheckProximity(retrieveInboundSeatMapGqlResponse, productName, cabinName, out string errorMessage), errorMessage);
            }
            else
            {
                Assert.False(GraphQLAssertion.CheckProximity(retrieveInboundSeatMapGqlResponse, productName, cabinName, out string errorMessage), errorMessage);
            }

            GraphQLHelper.SelectSeatsForSlice(basketId, 1, fareFamily, basketResponse, cabinName, "Standard Seat");

            dynamic dynamicBasketResponse = JsonConvert.DeserializeObject<dynamic>(await basketResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateSeatsForAllPassengers(dynamicBasketResponse, out string seatMessage), seatMessage);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Standard Seat", CabinName.VSCL, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, "Standard Seat", CabinName.VSCL, 4, 0, 0, 0)]
        public async Task SelectSeatForFirstPassengerOnly(LocationType origin, LocationType destination, FareFamilyType fareFamily, string productName, CabinName cabinName, int adults, int teens, int child, int infants)
        {
            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily, FlightType.VS, origin, destination, adults, teens, child, infants, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            GraphQLHelper.RetrieveSeatMapForSlice(basketId, 0, fareFamily, retrieveBasketGQLResponse, out HttpResponseMessage retrieveOutboundSeatMapGqlResponse);

            GraphQLHelper.SelectSeatsForSliceFirstPassenger(basketId, 0, fareFamily, retrieveBasketGQLResponse, productName, cabinName);

            GraphQLHelper.RetrieveBasket(basketId, out HttpResponseMessage basketResponse);

            dynamic dynamicBasketResponse = JsonConvert.DeserializeObject<dynamic>(await basketResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateSeatForFirstPassengerOnly(dynamicBasketResponse, out string errorMessage), errorMessage);

            //Payment retrieve - PRI
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId);

            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();

            GraphQLAssertion.ValidatePaymentRetreiveGQL(retrieveBasketGQLResponse, paymentRetGraphqlResponse);

            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, out string hppUrl, out string pri);

            //Delta HPP Page
            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", "4761344136141390", "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "12345");

            //PostPayment status
            var paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(basketId, pri);

            var paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();

            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            var orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(basketId);

            var orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errMessage), $"{errMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

            GraphQLHelper.ConfirmationInfo(orderCreateGQLResponse, out string leadPassengerFirstName, out string leadPassengerSurname, out string email, out string currency, out double totalPrice, out string outBoundcabinClass, out string inBoundcabinClass, out string pnr);

        }
    }
}
