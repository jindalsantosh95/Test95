using VAA.EAPI.HealthStatusApp.Models.AzureAppSettings;
using VAA.EAPI.HealthStatusApp.Models.AzureServiceBus;

namespace VAA.EAPI.HealthStatusApp.Services;

public interface IAzureAppSettingsService
{
    Task<IReadOnlyList<AzureSubscriptionModel>> GetSubscriptionsAsync();
    Task<IReadOnlyList<AzureResourceGroupModel>> GetResourceGroupsAsync(string subscriptionId);
    Task<IReadOnlyList<AzureSiteAppModel>> GetSitesAsync(string subscriptionId, string resourceGroupName);
    Task<IReadOnlyList<AzureAppSettingsDiffAlert>> GetRecentDiffAlertsAsync();
    Task<AzureAppSettingCompareResponse> GetSiteAppSettingsAsync(string subscriptionId, string resourceGroupName, string siteAppName);
    Task<IReadOnlyList<AzureServiceBusNamespace>> GetServiceBusNamespacesAsync(string subscriptionId, string resourceGroupName);
    Task<AzureAppSettingCompareResponse> Snapshot(string subscriptionId, string resourceGroupName, string siteAppName);

}
