using System.Web;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class GetDeepLinkForPostTests
    {
        public string env = Config.Environment;

        [Fact]
        public async Task TC01_GetDeepLinkForPost_ReturnsFlightItems()
        {
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string pnr = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            // Use Gateway Booking + Deeplink to generate a deep link so we can extract a non-hardcoded id.
            const string deeplinkOption = "ConciergeTripAware";

            HttpResponseMessage bookingResponse = RestEndpoint.Booking(
                    env,
                    usingString: "PNR",
                    confirmationNum: pnr,
                    givenNames: givenNames,
                    surname: surname)
                .EnsureIsSuccessStatusCode();

            string bookingBody = await bookingResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(bookingBody), "Booking response body was empty");

            JObject bookingResult = JObject.Parse(bookingBody);
            string tripReservationId = bookingResult.RequireToken("id").ToString();
            Assert.False(string.IsNullOrWhiteSpace(tripReservationId), "tripReservationId should not be empty");

            HttpResponseMessage deeplinkResponse = RestEndpoint.DeepLinks(env, tripReservationId, deeplinkOption)
                .EnsureIsSuccessStatusCode();

            string deeplinkBody = await deeplinkResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(deeplinkBody), "Deeplink response body was empty");

            JObject deeplinkResult = JObject.Parse(deeplinkBody);
            string deeplinkUrl = deeplinkResult.RequireToken("deeplinkUrl").ToString();
            Assert.False(string.IsNullOrWhiteSpace(deeplinkUrl), "deeplinkUrl should not be empty");

            Uri deeplinkUri = new Uri(deeplinkUrl);
            string id = HttpUtility.ParseQueryString(deeplinkUri.Query).Get("id") ?? string.Empty;
            Assert.False(string.IsNullOrWhiteSpace(id), "id should not be empty (expected query param 'id')");

            // Call under test (matches curl): POST /digital/Graphql/rest/v1/GetDeepLinkForPost { \"id\": \"<id>\" }
            HttpResponseMessage response = RestEndpoint.GetDeepLinkForPost(env, id)
                .EnsureIsSuccessStatusCode();

            string body = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(body), "Response body was empty");

            JObject json = JObject.Parse(body);

            JToken parameters = json.RequireToken("parameters");
            Assert.True(parameters is JArray, "parameters should be an array");
            Assert.True(parameters.Count() == 0, $"parameters should be empty but had {parameters.Count()} entries");

            JToken itemsToken = json.RequireToken("items");
            Assert.True(itemsToken is JArray, "items should be an array");

            JArray items = (JArray)itemsToken;
            Assert.NotEmpty(items);

            JObject firstItem = (JObject)items[0]!;
            string type = firstItem.RequireToken("type").ToString();
            string pnrLocator = firstItem.RequireToken("pnrLocator").ToString();
            string responseGivenName = firstItem.RequireToken("givenName").ToString();
            string responseSurname = firstItem.RequireToken("surname").ToString();

            Assert.True(type.Is("flight"), $"Expected item.type to be 'flight' but was '{type}'");
            Assert.False(string.IsNullOrWhiteSpace(pnrLocator), "item.pnrLocator should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(responseGivenName), "item.givenName should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(responseSurname), "item.surname should not be empty");

            // Validate response is related to the booking used to generate the deep link.
            Assert.True(pnrLocator.Is(pnr), $"Expected item.pnrLocator to match PNR '{pnr}' but was '{pnrLocator}'");
            Assert.True(responseSurname.Is(surname), $"Expected item.surname to match '{surname}' but was '{responseSurname}'");
        }
    }
}
