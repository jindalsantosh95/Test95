using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class DeeplinkTests
    {
        public string env = Config.Environment;

        [Fact]
        public async Task TC01_Deeplink_ReturnsUrlAndOption()
        {
            // Default test-data convention (see docs/TEST_DATA_STRATEGY.md): pull a current-week
            // PNR from Azure storage rather than hard-coding IDs/tokens.
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string pnr = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            HttpResponseMessage bookingResponse = RestEndpoint.Booking(
                    env,
                    usingString: "PNR",
                    confirmationNum: pnr,
                    givenNames: givenNames,
                    surname: surname)
                .EnsureIsSuccessStatusCode();

            string bookingBody = await bookingResponse.Content.ReadAsStringAsync();
            JObject booking = JObject.Parse(bookingBody);

            string tripReservationId = booking.RequireToken("id").ToString();
            string deeplinkOption = "ConciergeTripAware";

            Assert.False(string.IsNullOrWhiteSpace(tripReservationId), "tripReservationId should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(deeplinkOption), "deeplinkOption should not be empty");

            HttpResponseMessage response = RestEndpoint.DeepLinks(env, tripReservationId, deeplinkOption)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "Deeplink response body was empty");

            JObject deeplink = JObject.Parse(responseBody);

            string deeplinkUrl = deeplink.RequireToken("deeplinkUrl").ToString();
            Assert.False(string.IsNullOrWhiteSpace(deeplinkUrl), "deeplinkUrl should not be empty");
            Assert.True(deeplinkUrl.StartsWith("https://"),
                $"deeplinkUrl should start with https:// but was '{deeplinkUrl}'");
            Assert.Contains(
                "/deeplink?id=",
                deeplinkUrl,
                StringComparison.OrdinalIgnoreCase);

            string deeplinkOptionFromResponse = deeplink.RequireToken("deeplinkOption").ToString();
            const string conciergeTripAwareOptionValue = "11";
            Assert.True(
                deeplinkOption.Is(deeplinkOptionFromResponse) || deeplinkOptionFromResponse.Is(conciergeTripAwareOptionValue),
                $"Expected deeplinkOption to be '{deeplinkOption}' (or '{conciergeTripAwareOptionValue}') but was '{deeplinkOptionFromResponse}'");
        }
    }
}
