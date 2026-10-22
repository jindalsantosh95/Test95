using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Search.Requests.Endpoints
{
    public static class GraphQLEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage OfferPrice(string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            if (!b2cToken.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlSearch")}"), requestPayload, headers);
        }

        public static HttpResponseMessage SetVoucherRedemptionSelection(string requestPayload, string b2cToken = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            if (!b2cToken.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlBook")}"), requestPayload, headers);
        }

        public static HttpResponseMessage RetrieveBasket(string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", b2cToken.Is("") ? b2cToken : $"Bearer {b2cToken}" },
                { "api-channel", "Web Monitoring" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlBook")}"), requestPayload, headers);
        }

        public static HttpResponseMessage OfferSearch(string requestPayload, string basketId = "", string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection();
            var postbackUrl = Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"/{_env}/search")}");

            headers.Add("BasketId", basketId);
            headers.Add("PostbackUrl", postbackUrl);
            headers.Add("api-channel", "Web Monitoring");

            if (!string.IsNullOrEmpty(b2cToken))
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlSearch")}"), requestPayload, headers);
        }

        public static HttpResponseMessage Brand(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlSearch")}"), requestPayload, headers);
        }
    }
}
