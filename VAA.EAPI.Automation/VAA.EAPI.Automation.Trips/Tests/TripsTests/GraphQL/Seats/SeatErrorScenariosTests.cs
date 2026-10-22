using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    public class SeatErrorScenariosTests : SeatTestsBase
    {
        public SeatErrorScenariosTests(TokenFixture tokenFixture) : base(tokenFixture)
        {
        }

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_InvalidSeatSelection() =>
            await SeatRequest_IncorrectVariables_ReturnsGenericError();

        [SkippableFact]
        [Trait("Category", Traits.SeatsErrorScenariosRegression)]
        public async Task SeatRequest_IncorrectVariables_ReturnsGenericError()
        {
            #region - GraphQL call with incorrect variables expecting generic error

            string bookingId = "INVALID_TRIP";
            string segmentId = "INVALID_SEG";
            string passengerIdForSeat = "INVALID_PAX";
            string seatNumber = "37D";
            string paxSeatOfferId = "INVALID_PAXOFFERID";

            HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, segmentId, passengerIdForSeat, seatNumber, paxSeatOfferId).EnsureIsSuccessStatusCode();

            dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(selectSeatResult, "errors"), $"Expected errors in response but found \n {selectSeatResult}");

            string debugContext = RequestHelper.FormatTripsBookingContext(bookingId, passengerIdForSeat, seatNumber);
            ValidateGenericErrorResponse(selectSeatResult, debugContext);

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.SeatsErrorScenariosRegression)]
        [InlineData(FlightType.VS, CabinType.UpperClass)]
        [InlineData(FlightType.VS, CabinType.EconomyClassic)]
        public async Task SeatRequest_InvalidSegmentId_ReturnsExpectedError(FlightType flightType, CabinType cabinType)
        {
            #region - Get valid booking data

            string flightTypeString = flightType.ToString();
            string cabinTypeString = cabinType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSeatScenarios(flightTypeString, cabinTypeString);
            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, "PNR", pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (DynamicHelper.HasProperty(BookingResult, "errors") ||
                !DynamicHelper.HasProperty(BookingResult, "data") ||
                !DynamicHelper.HasProperty(BookingResult.data, "Booking") ||
                BookingResult.data.Booking == null)
            {
                SkipTest($"Test skipped: candidate PNR is not eligible for booking retrieval in current environment. {debugContext}");
                return;
            }

            string bookingId = BookingResult.data.Booking.id;
            dynamic passengers = BookingResult.data.Booking.passengers;
            string firstPassengerId = passengers[0].passengerId;

            #endregion

            #region - Test SelectSeat mutation with invalid segmentId (3)

            string invalidSegmentId = "3";
            string seatNumber = "1A";
            string seatOfferId = "test-offer-id";

            HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, invalidSegmentId, firstPassengerId, seatNumber, seatOfferId).EnsureIsSuccessStatusCode();
            dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

            ValidateExpectedErrorMessage(selectSeatResult, "selectSeatMutation", debugContext);

            #endregion

            #region - Test SeatConfirmation mutation with same booking

            HttpResponseMessage seatFulfilmentResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, "FAILED").EnsureIsSuccessStatusCode();
            dynamic seatFulfilmentResult = JsonConvert.DeserializeObject<dynamic>(await seatFulfilmentResponse.Content.ReadAsStringAsync());

            ValidateExpectedErrorMessage(seatFulfilmentResult, "seatFulfilmentMutation", debugContext);

            #endregion
        }

        private static void ValidateExpectedErrorMessage(dynamic errorResult, string expectedErrorSource, string debugContext = "")
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), $"Response should contain errors. {debugContext}");

            dynamic? errors = errorResult.errors;
            Assert.True(errors != null && errors.Count > 0, $"Response should contain at least one error. {debugContext}");

            dynamic firstError = errors[0];
            string actualMessage = firstError.message;
            string expectedMessage = "We’re unable to save your seat selection at the moment. Please try again later";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected message: {expectedMessage} but actual message is {actualMessage}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstError, "extensions"), $"Error should contain extensions. {debugContext}");
            dynamic extensions = firstError.extensions;

            Assert.True(DynamicHelper.HasProperty(extensions, "errorSource"), $"Extensions should contain errorSource. {debugContext}");
            string actualErrorSource = extensions.errorSource;
            Assert.Equal(expectedErrorSource, actualErrorSource);

            string responseString = extensions.response;
            Assert.True(responseString.Contains(expectedMessage), $"Expected Response string should contain - {expectedMessage} and actual is {responseString}. {debugContext}");
        }

        private static void ValidateGenericErrorResponse(dynamic errorResult, string debugContext = "")
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), $"Response should contain errors. {debugContext}");

            dynamic? errors = errorResult.errors;
            Assert.True(errors != null && errors.Count > 0, $"Response should contain at least one error. {debugContext}");

            string actualMessage = errors[0].message;
            string expectedMessage = "We’re unable to save your seat selection at the moment. Please try again later";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected error message {expectedMessage}  -  but actual message is {actualMessage}.  Details: {debugContext}");
        }
    }
}
