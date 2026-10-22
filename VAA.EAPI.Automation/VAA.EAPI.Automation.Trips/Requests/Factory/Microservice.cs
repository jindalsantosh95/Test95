using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Trips.Requests.Factory
{
    public static class Microservice
    {
        public static string RetrieveBookingUsingPNR(string usingSearch, string firstName, string lastName, string pnr)
        {
            string payload = File.ReadAllText(@"requests\Body\Microservices\TripSearch.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["using"] = usingSearch;
            payloadObject["confirmationNum"] = pnr;
            payloadObject["givenNames"] = firstName;
            payloadObject["surname"] = lastName;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string TripsList(string usingString, List<string> givenNames, List<string> surnames, List<string> pnrs)
        {
            string payload = File.ReadAllText(@"requests\Body\Microservices\TripSearch.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


            JArray tripDetailsArray = new JArray();

            for (int i = 0; i < pnrs.Count; i++)
            {
                JObject tripDetail = new JObject
                {
                    ["using"] = usingString,
                    ["confirmationNum"] = pnrs[i],
                    ["givenNames"] = givenNames[i],
                    ["surname"] = surnames[i]
                };
                tripDetailsArray.Add(tripDetail);
            }

            return JsonConvert.SerializeObject(tripDetailsArray);
        }
    }
}
