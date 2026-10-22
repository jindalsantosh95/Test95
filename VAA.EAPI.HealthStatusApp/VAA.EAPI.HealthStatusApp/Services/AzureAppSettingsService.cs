using VAA.EAPI.HealthStatusApp.Models.AzureAppSettings;
using VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.HealthStatusApp.Services;

public class AzureAppSettingsService : IAzureAppSettingsService
{

    private readonly IConfiguration _configuration;
    private readonly IHttpConnector _httpConnector;

    public AzureAppSettingsService(IConfiguration configuration, IHttpConnector httpConnector)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
    }

    public async Task<IReadOnlyList<AzureSubscriptionModel>> GetSubscriptionsAsync()
    {
        var response = await SendAsync<List<AzureSubscriptionModel>>(HttpMethod.Get, "azure/subscriptions");

        return response
            .OrderBy(subscription => subscription.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<AzureResourceGroupModel>> GetResourceGroupsAsync(string subscriptionId)
    {
        var response = await SendAsync<List<AzureResourceGroupModel>>(HttpMethod.Get, $"azure/subscriptions/{subscriptionId}/resourcegroups?projectCode=345");

        return response
            .OrderBy(resourceGroup => resourceGroup.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<AzureSiteAppModel>> GetSitesAsync(string subscriptionId, string resourceGroupName)
    {
        var response = await SendAsync<List<AzureSiteAppModel>>(HttpMethod.Get, $"azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/sites");

        return response
            .OrderBy(site => site.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<AzureAppSettingCompareResponse> GetSiteAppSettingsAsync(string subscriptionId, string resourceGroupName, string appName)
    {
        return await SendAsync<AzureAppSettingCompareResponse>(HttpMethod.Get, $"appSettings/compare?subscriptionId={subscriptionId}&resourceGroupName={resourceGroupName}&appName={appName}");
    }

    public async Task<IReadOnlyList<AzureAppSettingsDiffAlert>> GetRecentDiffAlertsAsync()
    {
        var response = await SendAsync<List<AzureAppSettingsDiffAlert>>(HttpMethod.Get, "appSettings/diffalerts");
        return response
            .OrderByDescending(alert => alert.Timestamp)
            .ToList();
    }

    public async Task<IReadOnlyList<AzureServiceBusNamespace>> GetServiceBusNamespacesAsync(string subscriptionId, string resourceGroupName)
    {
        var subscriptionSegment = Uri.EscapeDataString(subscriptionId);
        var resourceGroupSegment = Uri.EscapeDataString(resourceGroupName);
        var response = await SendAsync<List<AzureServiceBusNamespace>>(HttpMethod.Get, $"azure/subscriptions/{subscriptionSegment}/resourcegroups/{resourceGroupSegment}/servicebus");

        return response
            .OrderBy(serviceBusNamespace => serviceBusNamespace.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<AzureAppSettingCompareResponse> Snapshot(string subscriptionId, string resourceGroupName, string appName)
    {
        return await SendAsync<AzureAppSettingCompareResponse>(HttpMethod.Post, $"appSettings/snapshot?subscriptionId={subscriptionId}&resourceGroupName={resourceGroupName}&appName={appName}", new StringContent(""));
    }


    private async Task<T> SendAsync<T>(HttpMethod method, string route, HttpContent? content = null)
        where T : class, new()
    {
        var healthCheckUrl = _configuration["HEALTH_CHECK_URL"];
        var subscriptionKey = _configuration["HEALTH_CHECK_SUBSCRIPTION_KEY"];

        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri($"{healthCheckUrl.TrimEnd('/')}/{route}"),
            Headers =
            {
                { "Accept", "application/json" },
                { "Ocp-Apim-Subscription-Key", subscriptionKey }
            }
        };

        if (method == HttpMethod.Post && content != null)
        {
            request.Content = content;
        }

        return await _httpConnector.SendAsync<T>(request, CancellationToken.None);
    }
}
