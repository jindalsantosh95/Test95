using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.FlightOperations.Requests.Endpoints
{
    public static class Microservice
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage FlightDetails(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/fltopsflights/detail"), requestPayload, headers);
        }

        public static HttpResponseMessage FlightSchedule(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/fltopsflights/schedule"), requestPayload, headers);
        }

        public static HttpResponseMessage FlightCrew(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/fltopsflights/crewroster"), requestPayload, headers);
        }

        public static HttpResponseMessage MasterCrewRoster(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/fltopscrew/roster"), requestPayload, headers);
        }
    }
}
