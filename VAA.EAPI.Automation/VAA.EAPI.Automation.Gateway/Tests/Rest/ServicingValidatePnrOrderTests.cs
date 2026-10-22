using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class ServicingValidatePnrOrderTests
    {
        public string env = Config.Environment;

        [Theory]
        [InlineData("OneWay", "REVENUE")]
        [InlineData("Return", "REVENUE")]
        public async Task TC01_ServicingValidatePnrOrder_ReturnsExpectedResponseForMobileAppPnrs(
            string journeyType,
            string reservationType)
        {
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyType, reservationType);

            ValidatePnrDetails(pnrDetails, journeyType);

            string pnr = pnrDetails["pnr"];
            string givenName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];

            HttpResponseMessage response = RestEndpoint.ServicingValidatePnrOrder(env, pnr, givenName, lastName)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(responseBody.IsNullOrWhiteSpace(), "servicingValidatePnrOrder response body was empty");

            JObject validationResponse = JObject.Parse(responseBody);

            Assert.True(validationResponse.Value<bool?>("pnr") == true,
                $"Expected pnr=true for journey type {journeyType} but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("firstName") == true,
                $"Expected firstName=true for journey type {journeyType} but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("surname") == true,
                $"Expected surname=true for journey type {journeyType} but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("isGroup") == false,
                $"Expected isGroup=false for current revenue booking data but got: {responseBody}");
        }

        [Fact(Skip = "Enable when group booking test data is available in the MobileAppPnrs table.")]
        public async Task TC02_ServicingValidatePnrOrder_GroupBookingScenario_ReturnsIsGroupTrue()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task TC03_ServicingValidatePnrOrder_InvalidPnr_ReturnsPnrFalse()
        {
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            ValidatePnrDetails(pnrDetails, "OneWay");

            string invalidPnr = $"{pnrDetails["pnr"]}X";
            string givenName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];

            HttpResponseMessage response = RestEndpoint.ServicingValidatePnrOrder(env, invalidPnr, givenName, lastName)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(responseBody.IsNullOrWhiteSpace(), "servicingValidatePnrOrder response body was empty");

            JObject validationResponse = JObject.Parse(responseBody);

            Assert.True(validationResponse.Value<bool?>("pnr") == false,
                $"Expected pnr=false for invalid PNR but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("isGroup") == false,
                $"Expected isGroup=false for invalid PNR scenario but got: {responseBody}");
        }

        [Fact]
        public async Task TC04_ServicingValidatePnrOrder_NameMismatch_ReturnsNameFlagsFalse()
        {
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("Return", "REVENUE");

            ValidatePnrDetails(pnrDetails, "Return");

            string pnr = pnrDetails["pnr"];
            string invalidGivenName = $"{pnrDetails["firstName"]}X";
            string invalidLastName = $"{pnrDetails["lastName"]}X";

            HttpResponseMessage response = RestEndpoint.ServicingValidatePnrOrder(env, pnr, invalidGivenName, invalidLastName)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(responseBody.IsNullOrWhiteSpace(), "servicingValidatePnrOrder response body was empty");

            JObject validationResponse = JObject.Parse(responseBody);

            Assert.True(validationResponse.Value<bool?>("pnr") == true,
                $"Expected pnr=true for valid PNR with mismatched names but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("firstName") == false,
                $"Expected firstName=false for mismatched givenName but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("surname") == false,
                $"Expected surname=false for mismatched lastName but got: {responseBody}");

            Assert.True(validationResponse.Value<bool?>("isGroup") == false,
                $"Expected isGroup=false for current revenue booking data but got: {responseBody}");
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails, string journeyType)
        {
            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed for journey type {journeyType}: {error}");
            }

            Assert.True(
                pnrDetails.TryGetValue("pnr", out string? pnr) && !pnr.IsNullOrWhiteSpace() &&
                pnrDetails.TryGetValue("firstName", out string? firstName) && !firstName.IsNullOrWhiteSpace() &&
                pnrDetails.TryGetValue("lastName", out string? lastName) && !lastName.IsNullOrWhiteSpace(),
                $"PNR, First Name, and Last Name should not be null or empty for journey type {journeyType}");
        }
    }
}
