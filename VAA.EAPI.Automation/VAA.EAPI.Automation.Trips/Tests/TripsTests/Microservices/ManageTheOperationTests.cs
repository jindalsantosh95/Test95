using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class ManageTheOperationTests
    {
        public static string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

        [Theory]
        [Trait("Category", Traits.OperationalScheduleForAircraftDetails)]
        [InlineData(302)] // passing flightnumbers to validate aircraft details
        [InlineData(300)]
        public async Task OperationalStatus_ReturnsAircraftDetails(int flightNumber)
        {
            #region operational Status API Call - for returning aircraft details

            string airlineCode = "VS";
            string localDateFrom = DateTime.Now.ToString("yyyy-MM-dd");
            string localDateTo = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
            string direction = "D";
            string originAirportCode = "LHR";
            string destinationAirportCode = "DEL";
            int limit = 500;

            HttpResponseMessage operationalScheduleResponse = Microservice.OperationalScheduleAPI(airlineCode, localDateFrom, localDateTo, direction, originAirportCode, destinationAirportCode, flightNumber, limit, ocpSubKey).EnsureIsSuccessStatusCode();
            JObject operationalScheduleJobjectResponse = JsonConvert.DeserializeObject<JObject>(await operationalScheduleResponse.Content.ReadAsStringAsync());

            #endregion

            #region Validations

            Assert.NotNull(operationalScheduleJobjectResponse["Flights"]);
            Assert.True(((JArray)operationalScheduleJobjectResponse["Flights"]).Count > 0, "Flights array should not be empty");

            JToken? flight = operationalScheduleJobjectResponse["Flights"]?[0];
            Assert.NotNull(flight); // Ensures flight is not null before dereferencing

            Assert.True(flight["Aircraft"] != null, "Aircraft should not be null");
            Assert.True(flight["Aircraft"]?["Registration"] != null, "Aircraft Registration should not be null");
            Assert.True(flight["Aircraft"]?["Model_Code"] != null, "Aircraft Model_Code should not be null");
            Assert.True(flight["Marketing_Flight_Number"] != null, "Marketing_Flight_Number should not be null");
            Assert.True(flight["Operating_Flight_Number"] != null, "Operating_Flight_Number should not be null");
            Assert.True(flight["Legs"] != null, "Legs should not be null");
            Assert.True(((JArray)flight["Legs"]).Count > 0, "Legs array should not be empty");

            JToken leg = flight["Legs"][0];
            Assert.True(leg["Origin_Airport_Code"] != null, "Origin_Airport_Code should not be null");
            Assert.True(leg["Destination_Airport_Code"] != null, "Destination_Airport_Code should not be null");
            Assert.True(leg["Scheduled_Departure_Time"] != null, "Scheduled_Departure_Time should not be null");
            Assert.True(leg["Scheduled_Arrival_Time"] != null, "Scheduled_Arrival_Time should not be null");

            #endregion
        }
    }
}
