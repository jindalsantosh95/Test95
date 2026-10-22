using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class PlusgradeTests
    {
        public static string env = Config.Environment;
        public static string plusgradeApikey = Vault.Default.GetSecretValue("TestingPlusGradeAPIKey");

        [Theory]
        [Trait("Category", Traits.PlusGradeAPIRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task PlusGrade_WithValidInputs_ReturnsExpectedFieldsAndValues(JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            Assert.True(!string.IsNullOrEmpty(pnrDetails["pnr"]) && !string.IsNullOrEmpty(pnrDetails["firstName"]) && !string.IsNullOrEmpty(pnrDetails["lastName"]) && int.Parse(pnrDetails["passengersCount"]) > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnr), "PNR should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(firstName), "First Name should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(lastName), "Last Name should not be null or empty");
            Assert.True(paxCount > 0, "Pax Count should be greater than 0");

            #endregion

            #region Plus grade API Call - for BidUpgrade Tests

            string plusGradeRequest = RequestHelper.PlusGradeAPIRequest(pnr, lastName, plusgradeApikey);
            HttpResponseMessage plusGradeResponse = Microservice.PlusGradeAPIForBidForUpgradeAPI(plusGradeRequest, pnr, lastName, plusgradeApikey).EnsureIsSuccessStatusCode();
            JObject plusGradeJobjectResponse = JsonConvert.DeserializeObject<JObject>(await plusGradeResponse.Content.ReadAsStringAsync());

            #endregion

            #region Validations

            Assert.NotNull(plusGradeJobjectResponse["responseStatus"]);
            Assert.NotNull(plusGradeJobjectResponse["flights"]);
            Assert.True(((JArray)plusGradeJobjectResponse["flights"]).Count > 0, "Flights array should not be empty");

            #endregion
        }
    }
}
