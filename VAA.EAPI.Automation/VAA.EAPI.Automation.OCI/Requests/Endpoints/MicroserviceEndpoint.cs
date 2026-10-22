using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.OCI.Requests.Endpoints
{
    public static class MicroserviceEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage CheckIn(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/CheckIn"), requestPayload, headers);
        }

        public static HttpResponseMessage SelectPassengers(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/CheckIn/SelectPassengers"), requestPayload, headers);
        }

        public static HttpResponseMessage SavePassport(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/CheckIn/SavePassport"), requestPayload, headers);
        }

        public static HttpResponseMessage SaveContactTracing(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/CheckIn/SaveContactTracing"), requestPayload, headers);
        }

        public static HttpResponseMessage SaveEmergencyContact(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/CheckIn/SaveEmergencyContactDetails"), requestPayload, headers);
        }

        public static HttpResponseMessage GetBaggageDetails(string travelPartyId, string ocpSubKey, bool hidePaidBaggage)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/digital/baggage/GetBaggage/{travelPartyId}?hidePaidBaggage={hidePaidBaggage.ToString().ToLowerInvariant()}"), headers);
        }

        public static HttpResponseMessage SaveBaggageDetails(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/digital/baggage/SaveBaggage"), requestPayload, headers);
        }

        public static HttpResponseMessage CheckPayment(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/digital/baggage/CheckPayment"), requestPayload, headers);
        }

        public static HttpResponseMessage TravelRequirements(string requestPayload, string ocpSubKey)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Paxops/TravelRequirements"), requestPayload, headers);
        }
    }
}
