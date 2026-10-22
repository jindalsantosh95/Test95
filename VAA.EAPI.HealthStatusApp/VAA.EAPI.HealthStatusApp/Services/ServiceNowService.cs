using System.ServiceModel.Syndication;
using System.Xml;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using VAA.EAPI.HealthStatusApp.Models.NewFolder;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class ServiceNowService: IServiceNowService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceNowService> _logger;
        private readonly IConfiguration _configuration;

        public ServiceNowService(HttpClient httpClient, ILogger<ServiceNowService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ServiceNowModel>> GetServiceNowFeedAsync()
        {
            const int maxRetries = 3;
            const int retryDelayMs = 1000;

            var username = _configuration["ServiceNowUsername"];
            var password = _configuration["ServiceNowPassword"];
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    using var response = await _httpClient.GetAsync(_configuration["ServiceNowUrl"], cts.Token);

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();

                    var serviceNowResponse = JsonConvert.DeserializeObject<ServiceNowResponse>(content);

                    if (serviceNowResponse?.Result?.Requests != null)
                    {
                        var incidents = serviceNowResponse.Result.Requests;

                        return incidents.Where(x => x.ConfigurationItem == "VAA WebSite" && x.Priority == "Critical" && x.State != "Cancelled" && x.State != "Closed" && x.State != "Resolved");
                    }
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning(ex, $"Attempt {i + 1} failed: {ex.Message}. Retrying in {retryDelayMs}ms.");
                    await Task.Delay(retryDelayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking Service Now after {maxRetries} attempts: {ex.Message}");
                    return Enumerable.Empty<ServiceNowModel>();
                }
            }

            return Enumerable.Empty<ServiceNowModel>();
        }
    }
}
