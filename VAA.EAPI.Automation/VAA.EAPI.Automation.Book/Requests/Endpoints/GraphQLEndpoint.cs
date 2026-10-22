using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Book.Requests.Endpoints
{
    public static class GraphQLEndpoint
    {
        private static readonly string _env = Config.Environment;

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

        public static HttpResponseMessage Book(string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            if (!b2cToken.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlBook")}"), requestPayload, headers);
        }

        public static HttpResponseMessage InsuranceQuote(string basketId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlBook")}"), GraphQLRequestBuilder.InsuranceQuote(basketId), headers);
        }

        public static HttpResponseMessage InsuranceDecision(string basketId, bool isInsuranceSelected, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Web Monitoring" }
            };

            if (!b2cToken.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {b2cToken}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}{ConfigReader.Get("graphQlBook")}"), GraphQLRequestBuilder.InsuranceDecision(basketId, isInsuranceSelected), headers);
        }
    }
}
