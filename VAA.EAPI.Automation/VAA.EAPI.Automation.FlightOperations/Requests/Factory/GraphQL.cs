using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.FlightOperations.Requests.Factory
{
    public static class GraphQL
    {
        public static string RetrieveBookingWithPnrDetails(string bookingRef, string firstName, string lastName)
        {
            string payloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "RetrieveBooking.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"]!;
            variables["bookingRef"] = bookingRef;
            variables["firstName"] = firstName;
            variables["lastName"] = lastName;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveBookingsLoyaltyAccount()
        {
            string payloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "RetrieveBookings.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
