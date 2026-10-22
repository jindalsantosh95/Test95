using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Book.Requests.Endpoints
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

        public static HttpResponseMessage UpdatePassengerBFF(string basketId, string link, string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "Authorization", b2cToken.Is("")? b2cToken: "Bearer " + b2cToken },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Json.Put(link, requestPayload, headers);
        }

        public static HttpResponseMessage UpdateContactBFF(string basketId, string link, string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "Authorization", b2cToken.Is("")? b2cToken: "Bearer " + b2cToken },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Json.Put(link, requestPayload, headers);
        }

        public static HttpResponseMessage UpdateTnCBFF(string basketId, string link, string requestPayload, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "Authorization", b2cToken.Is("")? b2cToken: "Bearer " + b2cToken },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return WebRequest.Json.Put(link, requestPayload, headers);
        }

        public static HttpResponseMessage GetBasketBFF(string basketId, string b2cToken = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "Authorization", b2cToken.Is("")? b2cToken: "Bearer " + b2cToken },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/book/basket"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/basket"), headers);
        }

        public static HttpResponseMessage PostPaymentBFF(string basketId, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/book/payment"), requestPayload, headers) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/payment"), requestPayload, headers);
        }

        public static HttpResponseMessage Confirmation(string basketId, string childPri = "", string fraudCheck = "false")
        {
            string requestBody = $"{{\"customerConsentInfo\":{{ \"creditCardCheckRequested\":{fraudCheck},\"creditCardCheckAccepted\":{fraudCheck}}}}}";

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/book/confirmation"), requestBody, headers) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/confirmation"), requestBody, headers);
        }

        public static HttpResponseMessage ConfirmationFraudCheck(string basketId)
        {
            string requestBody = $"{{\"paymentDetails\":{{\"paymentReferenceId\":null}}}}";

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/book/confirmation"), requestBody, headers) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/confirmation"), requestBody, headers);
        }

        public static HttpResponseMessage ConfirmationPayPal(string basketId, string childPri = "")
        {
            string requestBody = $"{{\"customerConsentInfo\":null,\"paymentDetails\":{{\"PaymentReferenceId\":\"{childPri}\"}}}}";

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/book/confirmation"), requestBody, headers) : WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/confirmation"), requestBody, headers);
        }

        public static HttpResponseMessage UpdatePaymentStatus(string basketId, string childPri = "", string paymentStatus = "")
        {
            string requestBody = $"{{\"paymentReferenceId\":\"{childPri}\",\"hppPaymentStatus\":\"{paymentStatus}\"}}";

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Json.Put(Config.TestResourcesVirginAtlantic.GetUrl($"/book/Payment/PaymentStatus"), requestBody, headers) : WebRequest.Json.Put(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/Payment/PaymentStatus"), requestBody, headers);
        }

        public static HttpResponseMessage GetPaymentBFF(string basketId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/book/Payment/PaymentStatus"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/Payment/PaymentStatus"), headers);
        }

        public static HttpResponseMessage GetConfirmation(string basketId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "basketId", basketId },
                { "X-FORWARDED-FOR", "10.15.230.170" },
                { "api-channel", "WEB Monitoring" },
                { "userAgent", "Chrome/116.0.0.0" }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/book/Confirmation"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/book/Confirmation"), headers);
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