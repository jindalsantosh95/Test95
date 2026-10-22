using Newtonsoft.Json;
using VAA.EAPI.HealthStatusApp.Enums;
using VAA.EAPI.HealthStatusApp.Models;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public class InsightsConnector : IInsightsConnector
    {
        private readonly ILogger<InsightsConnector> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _insightsAppUrl;
        private readonly string _subscriptionKey;
        private readonly string _subscriptionKeyTest;

        public InsightsConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<InsightsConnector> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            _insightsAppUrl = configuration["HEALTH_CHECK_URL"] ?? throw new InvalidOperationException("HEALTH_CHECK_URL configuration is missing");
            _subscriptionKey = configuration["HEALTH_CHECK_SUBSCRIPTION_KEY"] ?? throw new InvalidOperationException("HEALTH_CHECK_SUBSCRIPTION_KEY configuration is missing");
            _subscriptionKeyTest = configuration["HEALTH_CHECK_SUBSCRIPTION_KEY_TEST"] ?? throw new InvalidOperationException("HEALTH_CHECK_SUBSCRIPTION_KEY_TEST configuration is missing");
        }

        public async Task<string> SendAsync(string route, InsightsEnvironment env = InsightsEnvironment.Default)
        {
            var url = GetEnvironmentUrl(env, route); 

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(route))
            {
                throw new ArgumentException("URL cannot be null or empty");
            }

            using var client = _httpClientFactory.CreateClient();


            if (env == InsightsEnvironment.Default)
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKeyTest);
            }

            try
            {
                return await client.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to send request to Insights Function App at {Url}", url);
                throw;
            }
        }

        public async Task<string> SendAsync(string route, string body, InsightsEnvironment env = InsightsEnvironment.Default)
        {
            var url = GetEnvironmentUrl(env, route);

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(route))
            {
                throw new ArgumentException("URL and route cannot be null or empty");
            }

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            try
            {
                using var response = await client.PostAsync(url, content);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested == false)
            {
                _logger.LogWarning("Request to Insights Function App at {Url} timed out.", url);
                return string.Empty; 
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to send request to Insights Function App at {Url}", url);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to Insights Function App at {Url}", url);
                throw;
            }
        }



        public async Task<(int Status, string Message)> GetInsightsStatus()
        {
            try
            {
                var response = await SendAsync("heartbeat");

                var report = JsonConvert.DeserializeObject<Result>(response);
                if (report == null)
                {
                    return (0, "Could not connect to insights function app");
                }

                return (report.Status, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health status for Insights Function App");
                return (0, ex.Message);
            }
        }

        private string GetEnvironmentUrl(InsightsEnvironment env, string route)
        {
            if (env == InsightsEnvironment.Default)
            {
                return $"{_insightsAppUrl}{route}";
            }
            else
            {
                var baseUrl = $"{_configuration["HEALTH_CHECK_TEST_URL"]}{route}";
                var tag = string.Empty;

                switch (env)
                {
                    case InsightsEnvironment.Development:
                        tag = "?environment=dev";
                        break;
                    case InsightsEnvironment.Staging:
                        tag = "?environment=sit";
                        break;
                    case InsightsEnvironment.UAT:
                        tag = "?environment=uat";
                        break;
                }

                return baseUrl + tag;
            }
        }
    }

}
