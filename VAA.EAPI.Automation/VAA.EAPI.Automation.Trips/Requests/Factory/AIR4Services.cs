using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Trips.Requests.Factory
{
    public class AIR4Services
    {
        public static string RetrieveTravelReservations(string firstName, string lastName, string pnr)
        {
            string payload = File.ReadAllText("requests\\Body\\AIR4Services\\MyTrips\\TravelReservations.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);
            payloadObject["using"] = "CONFIRMATION";
            payloadObject["confirmationNum"] = pnr;
            payloadObject["givenNames"] = firstName;
            payloadObject["surname"] = lastName;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
