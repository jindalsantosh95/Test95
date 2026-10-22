using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class LoyaltyTests
    {
        public string env = Config.Environment;

        [Fact]
        [Trait("Category", Traits.LoyaltyMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        public async Task RetrieveLoyaltyAirlinePartners_ReturnsPartners()
        {
            #region GrapqhQl query call for loyalty airline partners

            HttpResponseMessage response = GraphQlEndpoint.LoyaltyProgramsDetails(env).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}");

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), "Response should contain data property");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "LoyaltyProgramsDetails"), "Response should contain LoyaltyProgramsDetails property");

            #endregion

            #region - verify the response from the graphql query call

            // Parse expected response for assertions
            dynamic actualLoyaltyPrograms = BookingResult.data.LoyaltyProgramsDetails;
            dynamic loyaltyPartners = actualLoyaltyPrograms.loyaltyPrograms;

            // Assert
            Assert.True(loyaltyPartners is not null, "Loyalty partners should not be null");

            // Verify specific airlines are present
            List<string> partnerCodes = new();
            foreach (dynamic partner in loyaltyPartners)
            {
                partnerCodes.Add(partner["programCode"].ToString());
            }

            // Assert all expected program codes are present
            string[] expectedPartnerCodes = new[] { "VS", "DL", "AF", "KL" };

            foreach (string expectedCode in expectedPartnerCodes)
            {
                Assert.True(partnerCodes.Contains(expectedCode), $"Program code {expectedCode} should be present");

            }
            // Verify specific airline details
            VerifyAirlineDetails(loyaltyPartners, "VS", "Virgin Atlantic", "Flying Club");
            VerifyAirlineDetails(loyaltyPartners, "DL", "Delta Air Lines", "SkyMiles");
            VerifyAirlineDetails(loyaltyPartners, "AF", "Air France", "Flying Blue");
            VerifyAirlineDetails(loyaltyPartners, "KL", "KLM", "Flying Blue");

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.LoyaltyMobileRegression)]
        [InlineData("VS")]
        [InlineData("DL")]
        [InlineData("AF")]
        public async Task UpdateLoyaltyNumberForPartnerAirline_UpdatesPnr(string partnerCode)
        {

            #region - pre-requisite for fetching the PNR from the storage table

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForPartnerAirline(partnerCode);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            string fQTProgram = pnrDetails["fQTProgram"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName), $"PNR, First Name, and Last Name should not be null or empty. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip and verify the response from the graphql query call

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (DynamicHelper.HasProperty(BookingResult, "errors") ||
                !DynamicHelper.HasProperty(BookingResult, "data") ||
                !DynamicHelper.HasProperty(BookingResult.data, "Booking") ||
                BookingResult.data.Booking == null)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: candidate PNR is not eligible for booking retrieval in current environment. {debugContext}");
                return;
            }

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            dynamic passengers = BookingResult.data.Booking.passengers;
            Assert.True(passengers != null && passengers.HasValues, "Response does not contain 'passengers'");
            int actualPaxCount = ((IEnumerable<dynamic>)passengers).Count();

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                string passengerId = pax.passengerId;
                Assert.True(!string.IsNullOrEmpty(passengerId), "Passenger id should not be null or empty");
                passengerIds.Add(passengerId);

                // Also validating if the pax loyalty details are null before updating if already exist then return and skip the test
                dynamic loyaltyDetails = pax.loyaltyMembership;
                if (loyaltyDetails != null)
                {
                    string existingLoyaltyProgramCode = loyaltyDetails.programmeCode;
                    string existingLoyaltyMemberId = loyaltyDetails.number;
                    if (!string.IsNullOrEmpty(existingLoyaltyProgramCode) && !string.IsNullOrEmpty(existingLoyaltyMemberId))
                    {
                        TestDataPrerequisiteHelper.Skip($"Test skipped: loyalty membership already exists for passengerId {passengerId}. {debugContext}");
                        return;
                    }
                }
            }
            string leadPassengerId = passengerIds[0];

            #endregion

            #region - GraphQL call for updating loyalty number to PNR and verify the response from the graphql query call

            string loyaltyProgramCode = string.Empty;
            string loyaltyMemberId = string.Empty;

            if (!string.IsNullOrEmpty(fQTProgram) && fQTProgram.Contains("-"))
            {
                string[] parts = fQTProgram.Split('-');
                loyaltyProgramCode = parts[0];    // Will contain "VS"
                loyaltyMemberId = parts[1];       // Will contain "15002342432"
            }

            HttpResponseMessage loyaltyResponse = GraphQlEndpoint.UpdateLoyaltyMemberDetails(env, bookingId, loyaltyMemberId, loyaltyProgramCode, leadPassengerId).EnsureIsSuccessStatusCode();
            dynamic loyaltyResponseResult = JsonConvert.DeserializeObject<dynamic>(await loyaltyResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(
                loyaltyResponseResult, "errors"), $"Expected a valid response from Graphql but found \n {loyaltyResponseResult}");

            Assert.True(DynamicHelper.HasProperty(loyaltyResponseResult, "data"), "Response should contain data property");

            // Verify message content
            string actualMessage = loyaltyResponseResult.data.manageLoyaltyMemberMutation.message;

            string[] expectedMessages = new[]
            {
                        "Success",
                        "Loyalty member details already exist",
                        "Loyalty member details updated successfully"
            };

            Assert.True(expectedMessages.Contains(actualMessage),
                $"Unexpected response message: '{actualMessage}' for FqtProgram: '{fQTProgram}' and pnr: '{pnr}'. {debugContext}");

            #endregion
        }

        [Theory]
        [Trait("Category", Traits.LoyaltyMobileRegression)]
        [InlineData("6000001006", "AF", "", "tripReservationId", ErrorScenario.EmptypassengerId)]
        [InlineData("6000001006", "", "213123-231", "tripReservationId", ErrorScenario.EmptyloyaltyProgramCode)]
        [InlineData("6000001006", "AF", "232-131-1-3", "", ErrorScenario.EmptyTripReservationId)]
        public async Task LoyaltyUpdate_InvalidDetails_ReturnsExpectedError(string loyaltyMemberId, string loyaltyProgramCode, string passengerId, string TripReservationId, ErrorScenario scenario)
        {
            #region - GraphQL call with error scenarios

            HttpResponseMessage loyaltyResponse = GraphQlEndpoint.UpdateLoyaltyMemberDetails(env, TripReservationId, loyaltyMemberId, loyaltyProgramCode, passengerId).EnsureIsSuccessStatusCode();
            dynamic loyaltyResponseResult = JsonConvert.DeserializeObject<dynamic>(await loyaltyResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(
                loyaltyResponseResult, "errors"), $"Expected a valid response from Graphql but found \n {loyaltyResponseResult}");

            dynamic errors = loyaltyResponseResult.errors;
            Assert.True(errors != null && errors.Count > 0, $"Response should contain at least one error for error scenario: {scenario}");

            string actualMessage = errors[0].message;
            string expectedMessage = "We were not able to save your loyalty number. Please try again later.";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected generic error message but actual mesage is {actualMessage}");

            #endregion
        }

        [Fact]
        [Trait("Category", Traits.LoyaltyMobileRegression)]
        public async Task LoyaltyUpdate_EmptyMemberId_ReturnsExpectedError()
        {
            #region - GraphQL call with error scenario - Empty loyaltyMemberId

            string loyaltyMemberId = ""; // Empty loyaltyMemberId
            string passengerId = "3313123-231";
            string loyaltyProgramCode = "AF";
            string TripReservationId = "tripReservationId";

            HttpResponseMessage loyaltyResponse = GraphQlEndpoint.UpdateLoyaltyMemberDetails(env, TripReservationId, loyaltyMemberId, loyaltyProgramCode, passengerId).EnsureIsSuccessStatusCode();
            dynamic loyaltyResponseResult = JsonConvert.DeserializeObject<dynamic>(await loyaltyResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(
                loyaltyResponseResult, "errors"), $"Expected a valid response from Graphql but found \n {loyaltyResponseResult}");

            dynamic errors = loyaltyResponseResult.errors;
            Assert.True(errors != null && errors.Count > 0, "Response should contain at least one error");

            string actualMessage = errors[0].message;
            string expectedMessage = "We were not able to save your loyalty number. Please try again later.";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected generic error message but actual mesage is {actualMessage}");

            #endregion
        }

        [Fact]
        [Trait("Category", Traits.LoyaltyMobileRegression)]
        public async Task LoyaltyUpdate_IncorrectVariables_ReturnsGenericError()
        {
            #region - GraphQL call with incorrect variables expecting generic error

            string tripReservationId = "INVALID_TRIP";
            string loyaltyMemberId = "INVALID_PAX";
            string loyaltyProgramCode = "INVALID_PGMCODE";
            string passengerId = "INVALID_PAXID";

            HttpResponseMessage loyaltyResponse = GraphQlEndpoint.UpdateLoyaltyMemberDetails(env, tripReservationId, loyaltyMemberId, loyaltyProgramCode, passengerId).EnsureIsSuccessStatusCode();
            dynamic loyaltyResponseResult = JsonConvert.DeserializeObject<dynamic>(await loyaltyResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(loyaltyResponseResult, "errors"), $"Expected errors in response but found \n {loyaltyResponseResult}");

            ValidateGenericErrorResponse(loyaltyResponseResult);

            #endregion
        }
        private void ValidateGenericErrorResponse(dynamic errorResult)
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), "Response should contain errors");

            dynamic errors = errorResult.errors;
            Assert.True(errors != null && errors.Count > 0, "Response should contain at least one error");

            string actualMessage = errors[0].message;
            string expectedMessage = "We were not able to save your loyalty number. Please try again later.";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected message: {expectedMessage} message but actual message: {actualMessage}");
        }

        private void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);


        private void VerifyAirlineDetails(JArray programs, string programCode, string airlineName, string programName)
        {
            JToken program = programs.FirstOrDefault(p => p["programCode"].ToString() == programCode);
            Assert.True(program["programCode"].ToString() == programCode, $"Program code {programCode} should be present");
            Assert.True(airlineName.Equals(program["airlineName"].ToString()), $"Airline name for {programCode} should match");
            Assert.True(programName.Equals(program["programName"].ToString()), $"Program name for {programCode} should match");
        }
    }
}
