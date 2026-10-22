using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Insights.Helper
{
    public class AuthorizationHelper
    {
        public static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string uri, string clientId, string clientSecret, string authority, string apiversion = null)
        {
            var resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource).Trim('/');
            var requestUri = string.IsNullOrEmpty(apiversion)
                ? uri
                : string.Format(uri, resource, ConfigReader.Get(Constants.Environment.MetricsSubscriptionId), apiversion);

            var request = new HttpRequestMessage(method, requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                AzureADService.GetAccessToken(clientId, clientSecret, authority, resource).Result);

            return request;
        }


        public static async Task<HttpRequestMessage> CreateAuthorizationRequestV2(string url, string name, string clientId, string clientSecret)
        {
            var authority = GetAuthority();
            var subscriptionId = ConfigReader.Get(Constants.Environment.MetricsSubscriptionId);
            DateTime endTime = DateTime.UtcNow;
            DateTime startTime = endTime.AddHours(-24);
            string timespan = $"{startTime:yyyy-MM-ddTHH:mm:ssZ}/{endTime:yyyy-MM-ddTHH:mm:ssZ}";
            string baseUrl = string.Format(url, subscriptionId, name, timespan);

            var metricsRequest = new HttpRequestMessage(HttpMethod.Get, baseUrl);

            metricsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AzureADService.GetAccessTokenV2(clientId, clientSecret, authority, "https://management.azure.com/.default"));
            return metricsRequest;
        }

        public static string GetAuthority() =>
            $"{ConfigReader.Get(Constants.Environment.AzureAdInstance)}/{ConfigReader.Get(Constants.Environment.AzureAdTenantId)}";
    }
}
