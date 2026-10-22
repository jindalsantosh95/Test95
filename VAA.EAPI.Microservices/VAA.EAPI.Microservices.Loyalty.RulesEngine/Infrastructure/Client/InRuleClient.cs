using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Client
{
    public class InRuleClient : IInRuleClient
    {
        private readonly string _inRuleAPIURL;
        private readonly string _inRuleAPIHeader;
        private readonly ILogger<InRuleClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public InRuleClient(IHttpClientFactory httpClientFactory, ILogger<InRuleClient> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public InRuleClient()
        {
            _inRuleAPIURL = ConfigReader.Get(Constants.EnvironmentSettings.InRuleAPIURL);
            _inRuleAPIHeader = ConfigReader.Get(Constants.EnvironmentSettings.InRuleAPIHeader);
        }
        public async Task<Tuple<bool, string>> ExecuteInRule(string inRuleRequest)
        {
            InRuleClient client = new();

            HttpClient httpClient = _httpClientFactory.CreateClient(client._inRuleAPIURL);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client._inRuleAPIURL);
            request.Content = new StringContent(inRuleRequest, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", client._inRuleAPIHeader);
            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage httpResponse = await httpClient.SendAsync(request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"InRule API Error Response: {error}");
                return Tuple.Create(false, error);
            }

            string response = await httpResponse.Content.ReadAsStringAsync();
            return Tuple.Create(true, response);
        }
    }
}
