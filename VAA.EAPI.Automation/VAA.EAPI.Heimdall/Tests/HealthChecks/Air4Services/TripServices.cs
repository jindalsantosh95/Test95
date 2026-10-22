using VAA.EAPI.Automation.Core.Constants;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.Air4Services
{
    [Collection("Air4HealthCheckTests")]
    public class TripServices
    { 
        [Trait("Category", GlobalTraits.AIR4ServicesHealthCheck)]
        [Trait("Tag", GlobalTraits.AIR4Services)]
        [Fact]
        public async Task ValidateTripsEndpoint()
        {
            string channelIdString = "MyTrips";
            string appIdString = "DC";

            var response = AIR4Services.RetrieveTravelReservations("test", "test", "AB1234", channelIdString, appIdString);

            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest), $"BadRequest was not returned as expected status code was {response.StatusCode}");

            var air4Result = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(air4Result.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {air4Result.ToString()}");

            Assert.True(air4Result.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {air4Result.ToString()}");
        }
    }
}