using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Infrastructure.Email.Client
{
    public class CcpClient : ICcpClient
    {
        private readonly ILogger<CcpClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientId;
        private readonly string _authority;
        private readonly string _url;


        public CcpClient(IHttpClientFactory httpClientFactory, ILogger<CcpClient> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CcpClient()
        {
            _clientId = ConfigReader.Get(Constants.Constants.CcpClientId);
            _authority = ConfigReader.Get(Constants.Constants.AzureADEndpoint);
            Constants.Constants.CcpClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Constants.CcpClientSecret, Constants.Constants.CcpClientSecretCached);
            Constants.Constants.CcpResourceCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Constants.CcpResource, Constants.Constants.CcpResourceCached);
            Constants.Constants.CcpOcpApimSubscriptionKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Constants.CcpOcpApimSubscriptionKey, Constants.Constants.CcpOcpApimSubscriptionKeyCached);
            _url = ConfigReader.Get(Constants.Constants.CcpAPIUri);
        }

        public async Task<KeyValuePair<bool, string>> SendEmail(CcpRequest request)
        {
            CcpClient client = new();
            string token = await ExternalHelper.GetBearerToken(client._clientId, Constants.Constants.CcpClientSecretCached, client._authority, Constants.Constants.CcpResourceCached);

            HttpClient httpClient = _httpClientFactory.CreateClient(client._url);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, client._url)
            {
                Content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(Constants.Constants.OcpApimBearerKeyName, token);
            httpRequest.Headers.Add(Constants.Constants.OcpApimSubscriptionKeyName, Constants.Constants.CcpOcpApimSubscriptionKeyCached);
            httpRequest.Headers.Add("cache-control", "no-cache");

            using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"CustomerCommR API Error Response: {error}");
                return new KeyValuePair<bool, string>(false, error);
            }

            string response = await httpResponse.Content.ReadAsStringAsync();
            return new KeyValuePair<bool, string>(true, response);
        }
    }
}
