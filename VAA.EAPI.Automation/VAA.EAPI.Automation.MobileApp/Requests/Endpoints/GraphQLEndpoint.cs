using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.Requests.Endpoints
{
    public static class GraphQLEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage SearchAirportInfo(string searchCriteria, string b2cToken = "")
        {
            string requestPayloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Payloads", "SearchAirportInfo.json");
            string requestPayload = File.ReadAllText(requestPayloadPath);
            dynamic dynamicPayload = JsonConvert.DeserializeObject<dynamic>(requestPayload);

            dynamicPayload.variables.searchTerm.Value = searchCriteria;

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "api-channel", "Web Monitoring" }
            };

            if (!b2cToken.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{Core.Helpers.ConfigReader.Get("graphQL")}"), dynamicPayload.ToString(), headers);
        }

        public static HttpResponseMessage Introspection()
        {
            string requestPayloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Payloads", "IntrospectionRequest.json");
            string requestPayload = File.ReadAllText(requestPayloadPath);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{Core.Helpers.ConfigReader.Get("graphQL")}"), requestPayload);
        }

        public static HttpResponseMessage Booking(string usingString, string pnr, string givenNames, string surname)
        {
            string requestPayloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "Trips", "Booking.json");
            JObject requestPayload = JObject.Parse(File.ReadAllText(requestPayloadPath));
            JObject tripDetailsRequest = requestPayload["variables"]?["tripDetailsRequest"] as JObject
                ?? throw new InvalidOperationException("The booking payload does not contain variables.tripDetailsRequest.");

            tripDetailsRequest["using"] = usingString;
            tripDetailsRequest["confirmationNum"] = pnr;
            tripDetailsRequest["givenNames"] = givenNames;
            tripDetailsRequest["surname"] = surname;

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{Core.Helpers.ConfigReader.Get("graphQL")}"), requestPayload.ToString());
        }

        public static HttpResponseMessage OperationOverload(string payload)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{Core.Helpers.ConfigReader.Get("graphQL")}"), payload);
        }
    }
}
