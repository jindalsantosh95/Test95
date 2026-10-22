using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Search.Requests.Endpoints
{
    public static class WebBFFEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage OfferSearchBFF(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("Prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/search/Search"), requestPayload) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/search/Search"), requestPayload, headers);
        }

        public static HttpResponseMessage PostOffer(string url, string requestPayload = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Json.Post(url, requestPayload, headers);
        }

        public static HttpResponseMessage PostLink(string url, string requestPayload = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Json.Post(url, requestPayload, headers);
        }

        public static HttpResponseMessage SummaryBFF(string url)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage GetLink(string url)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "api-channel", "Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage GetMemberDetailsBFF(string token)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"Bearer {token}" },
                { "api-channel", "Monitoring" }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/search/getmemberdetails"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/search/getmemberdetails"), headers);
        }

        public static HttpResponseMessage OfferSearchBFFWithB2CToken(string requestPayload, string b2cToken)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"Bearer {b2cToken}" },
                { "api-channel", "Monitoring" }
            };

            return _env.Is("prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/search/search"), requestPayload, headers) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/search/search"), requestPayload, headers);
        }

        public static HttpResponseMessage SummaryBFFWithB2CToken(string summaryUrl, string b2cToken)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"Bearer {b2cToken}" },
                {"api-channel", "Monitoring" }
            };

            return WebRequest.Get(summaryUrl, headers);
        }
    }
}