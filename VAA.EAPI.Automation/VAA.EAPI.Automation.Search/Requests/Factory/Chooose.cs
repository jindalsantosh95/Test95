using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using ChooseRequest = VAA.EAPI.Automation.Search.Models.SustainabilityMS.ChooseRequest;

namespace VAA.EAPI.Automation.Search.Requests.Factory
{
    public static class Chooose
    {

        public static string ChooseRequest(LocationType origin = LocationType.None, LocationType destination = LocationType.None, string flightNumber = "", string date = "", string travelClass = "")
        {
            var chooseRequest = new List<ChooseRequest>()
            {
               new ChooseRequest()
              {
                Origin = origin.ToString(),
                Destination = destination.ToString(),
                FlightNumber = flightNumber,
                Date = date,
                travelClass = travelClass
              }
            };

            return JsonConvert.SerializeObject(chooseRequest);
        }

        public static HttpResponseMessage CreateChooseRequest(string bearer, string requestPayload, int passengersCount)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.ChooseAPIEndPoint.GetUrl($"/v1/footprint/flights?passengers={passengersCount}"), requestPayload, headers);
        }
    }
}
