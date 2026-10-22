using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.AIR4Tests
{
    [Collection("MyTrips")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MyTripsTests
    {
        public string env = Config.Environment;

        [Theory]
        [Trait("Category", Traits.AIR4MyTripsRegression)]
        [InlineData(ChannelId.DFMOBILE, AppId.D2, JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(ChannelId.DFMOBILE, AppId.D2, JourneyType.Return, ReservationType.REVENUE)]
        public async Task TC01_ValidateRetrieveMyTrips_SuccessScenario(ChannelId channelId, AppId appId, JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

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

            #endregion

            #region API Call
            string channelIdString = channelId.ToString();
            string appIdString = appId.ToString();

            HttpResponseMessage response = AIR4Services.RetrieveTravelReservations(firstName, lastName, pnr, channelIdString, appIdString).EnsureIsSuccessStatusCode();

            JObject air4Result = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!air4Result.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {air4Result.ToString()}");

            Assert.True(!air4Result.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {air4Result.ToString()}");

            #endregion

            #region Validation

            // Assert the travelReservationRequest properties
            Assert.True(air4Result["travelReservationRequest"]["confirmationNum"].ToString() == pnr, $"Expected 'confirmationNum' to be {pnr} but got {air4Result.ToString()}");

            Assert.True(air4Result["travelReservationRequest"]["givenNames"].ToString() == firstName, $"Expected 'givenNames' to be {firstName} but got {air4Result.ToString()}");

            Assert.True(air4Result["travelReservationRequest"]["surname"].ToString() == lastName, $"Expected 'surname' to be {lastName} but got {air4Result.ToString()}");

            //Assert that travelReservations is not an  empty array
            JArray travelReservations = (JArray)air4Result["travelReservations"];
            Assert.NotEmpty(travelReservations);

            //// Get the first travel reservation
            JObject firstReservation = (JObject)travelReservations[0];

            // Validate reservation type & trip type
            Assert.True(firstReservation["reservation"]["type"].ToString() == "REVENUE", $"Expected reservation type to be 'REVENUE' but got {firstReservation.ToString()}");
            string expectedJourneyType;
            switch (journeyTypeString.ToUpper())
            {
                case "ONEWAY":
                    expectedJourneyType = "ONE_WAY";
                    break;
                case "RETURN":
                    expectedJourneyType = "ROUND_TRIP";
                    break;
                default:
                    Assert.Fail($"Unexpected journey type: {journeyTypeString}");
                    return;
            }
            Assert.True(firstReservation["reservation"]["tripType"].ToString() == expectedJourneyType,
                $"Expected trip type to be '{expectedJourneyType}' but got {firstReservation["reservation"]["tripType"]}");

            Assert.True(firstReservation["reservation"]["tripType"].ToString() == expectedJourneyType, $"Expected trip type to be '{expectedJourneyType}' but got {firstReservation["reservation"]["tripType"]}");

            // Ensure ticketed status is true
            Assert.True((bool)firstReservation["reservation"]["ticketed"], "Expected reservation to be ticketed but it was not");

            // Validate passengers array is not empty
            JArray passengers = (JArray)firstReservation["passengers"];
            Assert.NotEmpty(passengers);

            // Count infants - if it present within the pax object 
            int infantCount = passengers.Count(p => p["infant"] != null);

            // Total passenger count including infants
            int totalPaxCount = passengers.Count + infantCount;

            // Validate the number of passengers matches the expected count
            Assert.True(paxCount.Equals(totalPaxCount), $"Expected {paxCount} passengers but found {totalPaxCount}");

            // Validate each passenger in the array
            for (int i = 0; i < passengers.Count; i++)
            {
                JObject passenger = (JObject)passengers[i];
                Assert.True(passenger["givenNames"] is not null, $"Passenger {i} is missing givenNames");
                Assert.True(passenger["surname"] is not null, $"Passenger {i} is missing surname");

                // Optionally validate infant details if present
                if (passenger["infant"] != null)
                {
                    JObject infant = (JObject)passenger["infant"];
                    Assert.True(infant["givenNames"] is not null, $"Infant of passenger {i} is missing givenNames");
                    Assert.True(infant["surname"] is not null, $"Infant of passenger {i} is missing surname");
                }
            }
            #endregion
        }
    }
}
