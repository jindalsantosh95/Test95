using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure
{
    public interface IAzureResources
    {
        Task<IEnumerable<AzureSubscription>> RetrieveSubscriptionListAsync();

        Task<IEnumerable<AzureResourceGroup>> RetrieveResourceGroupForSubscriptionAsync(string subscriptionId, string projectCode);

        Task<IEnumerable<AzureResource>> RetrieveAppServiceSitesForResourceGroupAsync(string subscriptionId, string resourceGroupName);

        Task<IEnumerable<AzureServiceBusNamespace>> RetrieveServiceBusNamespacesForResourceGroupAsync(string subscriptionId, string resourceGroupName);

        Task<IEnumerable<AzureResource>> RetrieveAllResourcesForResourceGroupAsync(string subscriptionId, string resourceGroupName);

        Task<IEnumerable<AzureAppSetting>> RetrieveAppSettingsAsync(string subscriptionId, string resourceGroupName, string functionAppName);

        Task<IEnumerable<AzureKeyVaultReference>> RetrieveKeyVaultResolverStatusAsync(string subscriptionId, string resourceGroupName, string appName);
    }
}
