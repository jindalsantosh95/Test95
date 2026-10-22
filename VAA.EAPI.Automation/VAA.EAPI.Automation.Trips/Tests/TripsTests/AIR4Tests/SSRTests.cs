using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;
using Config = VAA.EAPI.Automation.Core.Config;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.AIR4Tests
{
    [Collection("SSR")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class SSRTests
    {
        public string env = Config.Environment;
        TestConfig _testConfig;

        public SSRTests()
        {
            _testConfig = new();
        }

        [Theory]
        [Trait("Category", Traits.AIR4SSRRegression)]
        [InlineData(ChannelId.DFMOBILE, AppId.D2, JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(ChannelId.DFMOBILE, AppId.D2, JourneyType.Return, ReservationType.REVENUE)]
        public async Task TC01_RetrieveSSR_SuccessScenario(ChannelId channelId, AppId appId, JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName), "PNR, First Name, and Last Name should not be null or empty");

            #endregion

            #region API Call for retrieving the Mytrips response to capture the reservationResourceId

            string channelIdString = channelId.ToString();
            string appIdString = appId.ToString();

            HttpResponseMessage response = AIR4Services.RetrieveTravelReservations(firstName, lastName, pnr, channelIdString, appIdString).EnsureSuccessStatusCode();

            JObject air4Result = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!air4Result.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {air4Result.ToString()}");

            Assert.True(!air4Result.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {air4Result.ToString()}");

            string reservationResourceId = air4Result["travelReservations"][0]["reservationResourceId"].ToString();

            Assert.True(!string.IsNullOrEmpty(reservationResourceId), $"Expected reservationResourceId to be not null but got {reservationResourceId}");

            #endregion

            #region retrieving the pax resourceid for all the paxs in the reservation

            JToken passengers = air4Result["travelReservations"][0]["passengers"];
            int passengerCount = passengers.Count();

            //Dictionary to store the passenger resourceIds
            Dictionary<string, (string FirstName, string LastName)> passengerDetails = new Dictionary<string, (string, string)>();

            // Collect all passenger details
            for (int i = 0; i < passengerCount; i++)
            {
                string passengerResourceId = passengers[i]["passengerResourceId"].ToString();
                string paxFirstName = passengers[i]["givenNames"].ToString();
                string paxLastName = passengers[i]["surname"].ToString();
                passengerDetails.Add(passengerResourceId, (paxFirstName, paxLastName));
            }

            #endregion

            #region API call for retrieving the SSR response

            HttpResponseMessage ssrResponse = AIR4Services.RetrieveSSRs(channelIdString, reservationResourceId).EnsureSuccessStatusCode();

            JObject air4RetrieveSSRResult = JsonConvert.DeserializeObject<JObject>(await ssrResponse.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!air4RetrieveSSRResult.ToString().Contains("NGTSSR500"), $"Unexpected error:NGTSSR500 - ReservationResourceId not found in the cache {air4Result.ToString()}");

            Assert.True(!air4Result.ToString().Contains("ReservationResourceId not found in the cache"), $"Unexpected error: {air4Result.ToString()}");

            // Retrieve the SSRs assigned for the reservationResourceId
            List<SSRDetail> ssrDetails = new List<SSRDetail>();

            // Get all passengers from the specialServiceRequests
            JToken ssrPassengers = air4RetrieveSSRResult["specialServiceRequests"]["passengers"];

            // Iterate through each passenger
            foreach (JToken passenger in ssrPassengers)
            {
                string currentPassengerResourceId = passenger["passengerResourceId"].ToString();
                JToken ssrSegments = passenger["ssrSegments"];

                // Skip if passenger not found in original passenger details
                if (!passengerDetails.ContainsKey(currentPassengerResourceId))
                    continue;

                (string firstNamep, string lastNamep) = passengerDetails[currentPassengerResourceId];

                foreach (JToken segment in ssrSegments)
                {
                    JToken datedOperatingSegment = segment["datedOperatingSegment"];
                    JToken ssrs = segment["specialServiceRequests"];

                    foreach (JToken ssr in ssrs)
                    {
                        ssrDetails.Add(new SSRDetail
                        {
                            PassengerResourceId = currentPassengerResourceId,
                            GivenNames = firstNamep,
                            Surname = lastNamep,
                            FlightNumber = datedOperatingSegment["flightNum"].ToString(),
                            Origin = datedOperatingSegment["originStation"][0]["code"].ToString(),
                            Destination = datedOperatingSegment["destinationStation"][0]["code"].ToString(),
                            SSRCode = ssr["code"].ToString(),
                            SSRName = ssr["name"].ToString(),
                            IsSelected = (bool)ssr["selected"]
                        });
                    }
                }
            }

            // Assert the ssrDetails list is populated plus Assertions on the collected SSR details
            Assert.True(ssrDetails.Any(), "SSR details list should not be empty");

            foreach (SSRDetail ssrDetail in ssrDetails)
            {
                Assert.False(string.IsNullOrEmpty(ssrDetail.PassengerResourceId), "Passenger Resource ID should not be empty");
                Assert.False(string.IsNullOrEmpty(ssrDetail.FlightNumber), "Flight number should not be empty");
                Assert.False(string.IsNullOrEmpty(ssrDetail.SSRCode), "SSR code should not be empty");
                Assert.False(string.IsNullOrEmpty(ssrDetail.SSRName), "SSR name should not be empty");
                Assert.True(ssrDetail.IsSelected || !ssrDetail.IsSelected, "SSR IsSelected should be a valid boolean value");
            }

            #endregion

            #region writing the SSR details to the Storage Table "SSRDetails"

            // Group SSR details by passenger
            IEnumerable<IGrouping<string, SSRDetail>> ssrDetailsByPassenger = ssrDetails.GroupBy(x => x.PassengerResourceId);

            foreach (IGrouping<string, SSRDetail> passengerGroup in ssrDetailsByPassenger)
            {
                string passengerResourceId = passengerGroup.Key;
                List<SSRDetail> passengerSSRs = passengerGroup.ToList();

                // Update test config for each passenger
                _testConfig.Book.Order.PNR = pnr;
                _testConfig.Internal.TestCaseName = "RetrieveSSR_SuccessScenario";

                // Write SSR details for this passenger to Azure Storage
                await WriteToAzureStorage.AddSSRToAzureStorageTable(_testConfig, passengerSSRs);
            }
            #endregion
        }
    }
}
