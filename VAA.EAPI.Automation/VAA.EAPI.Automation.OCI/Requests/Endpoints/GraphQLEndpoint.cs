using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.OCI.Requests.Endpoints
{
    public static class GraphQLEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage GraphQLCheckInMobile(string requestPayload, string authorizationToken = "")
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "api-channel", "Web Monitoring" },
                {"Authorization", $"{authorizationToken}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{Core.Helpers.ConfigReader.Get("graphQlCheckInMobile")}"), requestPayload, headers);
        }
    }
}
