using System.Text.Json;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Services
{
    public class HeartBeatService : IHeartBeatService
    {
        public IHttpConnector _httpConnector;

        public HeartBeatService(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector;
        }

        public async Task<HeartBeatResponse> GetOfferHeartBeatStatus()
        {
            var result = await _httpConnector.GetAsync<HeartBeatResponse>(ConfigReader.Get(Constants.Environment.OfferHeartBeatUrl), null, null);

            return result.Response;
        }

        public async Task<HeartBeatResponse> GetOrderHeartBeatStatus()
        {
            var result = await _httpConnector.GetAsync<HeartBeatResponse>(ConfigReader.Get(Constants.Environment.OrderHeartBeatUrl), null, null);

            return result.Response;
        }

        public async Task<HeartBeatResponse> GetPaymentFacadeHeartBeatStatus()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ConfigReader.Get(Constants.Environment.PaymentFacadeTokenUrl));

            var tokenRequestBody = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", ConfigReader.Get(Constants.Environment.PaymentFacadeTokenClientId) },
                { "client_secret", ConfigReader.Get(Constants.Environment.PaymentFacadeTokenClientSecret) },
                { "resource", ConfigReader.Get(Constants.Environment.PaymentFacadeTokenResource) }
            };

            request.Content = new FormUrlEncodedContent(tokenRequestBody);
            request.Headers.Add("Accept", "application/json");

            var tokenResponse = await _httpConnector.SendAsync(request, CancellationToken.None);

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenObj = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

            if (tokenObj.TryGetProperty("access_token", out var accessTokenElement))
            {
                var accessToken = accessTokenElement.GetString();

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    var headers = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {accessToken}" },
                        { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.PaymentFacadeSubKey) }
                    };

                    var result = await _httpConnector.GetAsync<HeartBeatResponse>(ConfigReader.Get(Constants.Environment.PaymentFacadeHeartBeatUrl), headers, null);

                    return result.Response;
                }
            }

            throw new System.InvalidOperationException("Payment facade token response did not contain a valid access_token.");
        }
    }
}
