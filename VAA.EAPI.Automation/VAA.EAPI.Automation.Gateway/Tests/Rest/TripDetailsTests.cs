using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class TripDetailsTests
    {
        public string env = Config.Environment;

        [Fact]
        public async Task TC01_TripDetails_ReturnsTripData()
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

            HttpResponseMessage response = RestEndpoint.TripDetails(
                    env,
                    bookingReference: string.Empty,
                    dateOfBirth: string.Empty,
                    bookingSurname: string.Empty,
                    pnrLocator: pnr,
                    givenNames: givenNames,
                    pnrSurname: surname)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "TripDetails response body was empty");

            JArray body = JArray.Parse(responseBody);
            Assert.True(body.Count > 0, "TripDetails response array was empty");

            JObject? pnrResult = body
                .Cast<JObject>()
                .FirstOrDefault(trip => pnr.Is(trip.SelectToken("tripSearchRequest.pnrLocator")?.ToString()));
            Assert.NotNull(pnrResult);

            // Some TripDetails responses may omit a top-level `status` field; if
            // present, assert minimally on it, otherwise skip gracefully.
            JToken? statusToken = pnrResult.SelectToken("status");
            if (statusToken != null)
            {
                string status = statusToken.ToString();
                Assert.False(string.IsNullOrWhiteSpace(status), "status should not be empty");
            }

            JToken? responseStatusToken = pnrResult.SelectToken("responseStatus.status");
            if (responseStatusToken != null)
            {
                int responseStatus = responseStatusToken.ToObject<int>();
                Assert.Equal(200, responseStatus);
            }

            string responsePnr = pnrResult.RequireToken("tripSearchRequest.pnrLocator").ToString();
            Assert.True(pnr.Is(responsePnr), $"Expected tripSearchRequest.pnrLocator to be '{pnr}' but was '{responsePnr}'");

            string responseGivenNames = pnrResult.RequireToken("tripSearchRequest.givenNames").ToString();
            Assert.True(givenNames.Is(responseGivenNames), $"Expected tripSearchRequest.givenNames to be '{givenNames}' but was '{responseGivenNames}'");

            string responseSurname = pnrResult.RequireToken("tripSearchRequest.surname").ToString();
            Assert.True(surname.Is(responseSurname), $"Expected tripSearchRequest.surname to be '{surname}' but was '{responseSurname}'");

            JArray productArray = (JArray)pnrResult.RequireToken("product");
            Assert.True(productArray.Count > 0, "product array was empty");

            JObject firstProduct = (JObject)productArray[0];
            string bookingType = firstProduct.RequireToken("bookingType").ToString();
            Assert.False(string.IsNullOrWhiteSpace(bookingType), "product[0].bookingType should not be empty");

            JArray slicesArray = (JArray)firstProduct.RequireToken("slices");
            Assert.True(slicesArray.Count > 0, "product[0].slices was empty");

            string originCode = firstProduct.RequireToken("slices[0].origin.code").ToString();
            string destinationCode = firstProduct.RequireToken("slices[0].destination.code").ToString();
            Assert.False(string.IsNullOrWhiteSpace(originCode), "product[0].slices[0].origin.code should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(destinationCode), "product[0].slices[0].destination.code should not be empty");

            // Passengers can be present on the product item or at the trip root. Prefer
            // the product-level passengers when available, otherwise fall back to the
            // top-level `passengers` array on the trip object.
            JArray? passengersArray = firstProduct.SelectToken("passengers") as JArray
                ?? pnrResult.SelectToken("passengers") as JArray;
            Assert.True(passengersArray != null && passengersArray.Count > 0, "product[0].passengers or root passengers was empty");

            string firstPassengerName = passengersArray[0]?["givenNames"]?.ToString()
                ?? passengersArray[0]?["firstName"]?.ToString()
                ?? string.Empty;
            Assert.False(string.IsNullOrWhiteSpace(firstPassengerName), "product[0].passengers[0].givenNames or firstName should not be empty");
        }
    }
}
