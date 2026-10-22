using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.Requests;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class TripsTests
    {
        public static string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingTripMicroserviceSubscriptionKey");

        [Theory(Skip = "Skipping the tripsMS schema validation scenarios as they have not been implemented till now")]
        [Trait("Category", Traits.TripsMSRegression)]
        [InlineData(ResponseCode.OK)]
        [InlineData(ResponseCode.InternalServerError)]
        [InlineData(ResponseCode.Unauthorized)]
        [InlineData(ResponseCode.UnprocessableEntity)]
        public async Task TripsSchemaValidation(ResponseCode responseCode)
        {
            string schemaResponse = Endpoints.GetSchema("tripmicroservice", ocpSubKey).EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Don't assert the response for success as this is to check the schema on the response even if it errors

            HttpResponseMessage tripsSearchResponse;
            string tripsSearchRequest = RequestHelper.TripsSearchRequest("", "", "", "");
            switch (responseCode)
            {
                case ResponseCode.Unauthorized:
                    // Send request with invalid or missing auth token
                    tripsSearchRequest = RequestHelper.TripsSearchRequest("PNR", "ERKBMN", "ADULTNLRMC", "ADULTONVND");
                    tripsSearchResponse = Microservice.TripsSearch(env, tripsSearchRequest, "invalid-ocpSubKey");
                    break;

                case ResponseCode.InternalServerError:
                    // You might need to modify the request to trigger a 500 error
                    // This could be through a specific test endpoint or invalid data format
                    tripsSearchRequest = RequestHelper.TripsSearchRequest("PNR-Invalid", "ERKBMN", "ADULTNLRMC", "ADULTONVND");
                    tripsSearchResponse = Microservice.TripsSearch(env, tripsSearchRequest, ocpSubKey);
                    break;

                case ResponseCode.UnprocessableEntity:
                    // Modify request to be invalid but well-formed
                    tripsSearchRequest = RequestHelper.TripsSearchRequest("PNR", "ERKBMN", "ADULTNLRMC", "ADULTONVND");
                    tripsSearchResponse = Microservice.TripsSearch(env, tripsSearchRequest, ocpSubKey);
                    break;

                case ResponseCode.OK:
                    // Normal happy path
                    GetValidPnrDetails(out string firstName, out string lastName, out string pnr);
                    tripsSearchRequest = RequestHelper.TripsSearchRequest("PNR", pnr, firstName, lastName);
                    tripsSearchResponse = Microservice.TripsSearch(env, tripsSearchRequest, ocpSubKey);
                    break;

                default:
                    throw new ArgumentException($"Unsupported response code: {responseCode}");
            }

            string responseCodeReturned = ((int)tripsSearchResponse.StatusCode).ToString();

            string schemaName = "/trips/search";

            string tripsSearchResponseString = await tripsSearchResponse.Content.ReadAsStringAsync();

            SchemaValidator.JsonSchemaValidator(tripsSearchResponseString, schemaResponse, schemaName, responseCodeReturned);
        }

        private static void GetValidPnrDetails(out string firstName, out string lastName, out string pnr)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            ValidatePnrDetails(pnrDetails);

            firstName = pnrDetails["firstName"];
            lastName = pnrDetails["lastName"];
            pnr = pnrDetails["pnr"];

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName), "PNR, First Name, and Last Name should not be null or empty ");

            #endregion - pre-requisite for fetching the PNR from the storage table
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
        {
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);

            Assert.True(!string.IsNullOrEmpty(pnrDetails["pnr"]) && !string.IsNullOrEmpty(pnrDetails["firstName"]) && !string.IsNullOrEmpty(pnrDetails["lastName"]) && int.Parse(pnrDetails["passengersCount"]) > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");
        }

        [Theory]
        [Trait("Category", Traits.TripsMSRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveBookingUsingPnr_ReturnsBookingDetails(JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            // Check for error first
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");

            #endregion Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            #region Trip MS API Call

            HttpResponseMessage response = Microservice.RetrieveBookingUsingPNR("PNR", firstName, lastName, pnr, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject bookingResponse = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!bookingResponse.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {bookingResponse.ToString()}");

            Assert.True(!bookingResponse.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {bookingResponse.ToString()}");

            #endregion Trip MS API Call

            #region Response Validations

            // Assert PNR locator
            string actualPnrLocator = bookingResponse["booking"]["product"][0]["reference"]["pnrLocator"].ToString();
            Assert.True(pnr.ToUpper().Equals(actualPnrLocator), "PNR locator does not match expected value. Expected: " + pnr + " Actual: " + actualPnrLocator);

            // Assert number of slices based on journey type
            JToken slicesArray = bookingResponse["booking"]["product"][0]["slices"];
            int slicesCount = slicesArray.Count();

            journeyTypeString = journeyType.ToString();

            switch (journeyTypeString.ToUpper())
            {
                case "ONEWAY":
                    Assert.True(slicesCount == 1, $"One-way journey should have exactly 1 slice but found {slicesCount}");
                    break;

                case "RETURN":
                    Assert.True(slicesCount == 2, $"Return journey should have exactly 2 slices but found {slicesCount}");
                    break;

                default:
                    Assert.Fail($"Unexpected journey type: {journeyTypeString}");
                    return;
            }

            #endregion Response Validations
        }


        [Theory]
        [Trait("Category", Traits.TripsMSRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveMultipleTripsUsingPnr_ReturnsTripList(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching 2 PNRs from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            // Get first PNR
            Dictionary<string, string> pnrDetails1 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
            ValidatePnrDetails(pnrDetails1);

            // Get second PNR (different from first)
            Dictionary<string, string> pnrDetails2;
            do
            {
                pnrDetails2 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
                ValidatePnrDetails(pnrDetails2);
            } while (pnrDetails1["pnr"] == pnrDetails2["pnr"]); // Ensure different PNRs

            List<string> pnrs = new() { pnrDetails1["pnr"], pnrDetails2["pnr"] };
            List<string> firstNames = new() { pnrDetails1["firstName"], pnrDetails2["firstName"] };
            List<string> lastNames = new() { pnrDetails1["lastName"], pnrDetails2["lastName"] };

            #endregion

            #region - TripsList MS API  call for retrieve multiple trips

            string usingString = "PNR";

            HttpResponseMessage response = Microservice.TripsList(env, usingString, pnrs, firstNames, lastNames, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject tripsResponseObj = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Validate top-level tripListResponse exists and is an array
            Assert.True(tripsResponseObj != null, "TripsList response JSON is null");
            JToken tripListToken = tripsResponseObj["tripListResponse"];
            Assert.True(tripListToken != null && tripListToken.Type == JTokenType.Array, "Expected 'tripListResponse' array in response");

            JArray tripList = (JArray)tripListToken;
            Assert.True(tripList.Count == pnrs.Count, $"Expected tripListResponse count to be {pnrs.Count} but found {tripList.Count}");

            // For each requested PNR, validate presence and fields in response
            for (int i = 0; i < pnrs.Count; i++)
            {
                string expectedPnr = pnrs[i];
                string expectedFirstName = firstNames[i];
                string expectedLastName = lastNames[i];

                // Find the matching entry by confirmationNum (case-insensitive)
                JToken matchingEntry = tripList.FirstOrDefault(t =>
                {
                    string conf = t?["tripSearchRequest"]?["confirmationNum"]?.ToString();
                    return !string.IsNullOrEmpty(conf) && string.Equals(conf, expectedPnr, StringComparison.OrdinalIgnoreCase);
                });

                Assert.True(matchingEntry != null, $"No tripListResponse entry found for PNR '{expectedPnr}'");

                // Validate status code in tripDetails
                int statusValue = 0;
                JToken statusToken = matchingEntry["tripDetails"]?["status"];
                Assert.True(statusToken != null && int.TryParse(statusToken.ToString(), out statusValue), $"Missing or invalid 'tripDetails.status' for PNR '{expectedPnr}'");
                Assert.True(statusValue == 200, $"Expected tripDetails.status 200 for PNR '{expectedPnr}' but found {statusValue}");

                // Validate booking and reference pnrLocator
                JToken bookingToken = matchingEntry["tripDetails"]?["booking"];
                Assert.True(bookingToken != null, $"Missing 'tripDetails.booking' for PNR '{expectedPnr}'");

                string pnrLocator = bookingToken["product"]?[0]?["reference"]?["pnrLocator"]?.ToString();
                Assert.True(!string.IsNullOrEmpty(pnrLocator), $"Missing 'pnrLocator' in booking reference for PNR '{expectedPnr}'");
                Assert.True(string.Equals(pnrLocator, expectedPnr, StringComparison.OrdinalIgnoreCase), $"Expected pnrLocator '{expectedPnr}' but found '{pnrLocator}'");

                // Validate passenger first & last name (use first passenger if multiple)
                string respFirstName = bookingToken["passengers"]?[0]?["firstName"]?.ToString();
                string respLastName = bookingToken["passengers"]?[0]?["lastName"]?.ToString();

                Assert.True(!string.IsNullOrEmpty(respFirstName), $"Missing passenger.firstName for PNR '{expectedPnr}'");
                Assert.True(!string.IsNullOrEmpty(respLastName), $"Missing passenger.lastName for PNR '{expectedPnr}'");

                Assert.True(string.Equals(respFirstName, expectedFirstName, StringComparison.OrdinalIgnoreCase),
                    $"Passenger firstName mismatch for PNR '{expectedPnr}'. Expected '{expectedFirstName}', Actual '{respFirstName}'");

                Assert.True(string.Equals(respLastName, expectedLastName, StringComparison.OrdinalIgnoreCase),
                    $"Passenger lastName mismatch for PNR '{expectedPnr}'. Expected '{expectedLastName}', Actual '{respLastName}'");
            }

            #endregion
        }
    }
}
