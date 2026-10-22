using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure;
using VAA.EAPI.Microservices.Insights.Mapper;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{
    public sealed class AzureController(IAzureResources azureResources)
    {
        private readonly IAzureResources _azureResources = azureResources ?? throw new ArgumentNullException(nameof(azureResources));

        [OpenApiOperation(operationId: "RetrieveSubscriptionList", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<Infrastructure.Azure.Models.AzureSubscription>), Summary = "OK", Description = "List of Azure subscriptions.")]
        [Function("RetrieveSubscriptionList")]
        public async Task<HttpResponseData> RetrieveSubscriptionList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions")] HttpRequestData req)
        {
            var result = await _azureResources.RetrieveSubscriptionListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "RetrieveResourceGroupForSubscription", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "projectCode", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Project Code", Description = "Optional ProjectCode tag filter for resource groups.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<Infrastructure.Azure.Models.AzureResourceGroup>), Summary = "OK", Description = "List of Azure resource groups.")]
        [Function("RetrieveResourceGroupForSubscription")]
        public async Task<HttpResponseData> RetrieveResourceGroupForSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions/{subscriptionId}/resourcegroups")] HttpRequestData req, string subscriptionId)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var projectCode = query["projectCode"];

            var result = await _azureResources.RetrieveResourceGroupForSubscriptionAsync(subscriptionId, projectCode);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "RetrieveAppServiceSitesForResourceGroup", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroup", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<Infrastructure.Azure.Models.AzureResource>), Summary = "OK", Description = "List of Azure App Servce Sites.")]
        [Function("RetrieveAppServiceSitesForResourceGroup")]
        public async Task<HttpResponseData> RetrieveAppServiceSitesForResourceGroup([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/sites")] HttpRequestData req, string subscriptionId, string resourceGroup)
        {
            var result = await _azureResources.RetrieveAppServiceSitesForResourceGroupAsync(subscriptionId, resourceGroup);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "RetrieveServiceBusNamespacesForResourceGroup", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroup", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<Infrastructure.Azure.Models.AzureServiceBusNamespace>), Summary = "OK", Description = "List of Azure Service Bus namespaces with topics, subscriptions, and queue depths.")]
        [Function("RetrieveServiceBusNamespacesForResourceGroup")]
        public async Task<HttpResponseData> RetrieveServiceBusNamespacesForResourceGroup([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/servicebus")] HttpRequestData req, string subscriptionId, string resourceGroup)
        {
            var result = await _azureResources.RetrieveServiceBusNamespacesForResourceGroupAsync(subscriptionId, resourceGroup);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "RetrieveAppSettings", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroup", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiParameter(name: "appName", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "App Name", Description = "Azure App Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<AzureAppSettingViewModel>), Summary = "OK", Description = "List of app settings with Key Vault resolver status where applicable.")]
        [Function("RetrieveAppSettings")]
        public async Task<HttpResponseData> RetrieveAppSettings([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/sites/{appName}/settings")] HttpRequestData req, string subscriptionId, string resourceGroup, string appName)
        {
            var appSettingsTask = _azureResources.RetrieveAppSettingsAsync(subscriptionId, resourceGroup, appName);
            var keyVaultResolverStatusTask = _azureResources.RetrieveKeyVaultResolverStatusAsync(subscriptionId, resourceGroup, appName);

            await Task.WhenAll(appSettingsTask, keyVaultResolverStatusTask);

            var result = AzureAppSettingsMapper.Map(await appSettingsTask, await keyVaultResolverStatusTask);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "RetrieveAllResourcesForResourceGroup", tags: new[] { "Azure Resources" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroup", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(System.Collections.Generic.IEnumerable<Infrastructure.Azure.Models.AzureResource>), Summary = "OK", Description = "List of all Azure resources in the resource group.")]
        [Function("RetrieveAllResourcesForResourceGroup")]
        public async Task<HttpResponseData> RetrieveAllResourcesForResourceGroup([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/resources")] HttpRequestData req, string subscriptionId, string resourceGroup)
        {
            var result = await _azureResources.RetrieveAllResourcesForResourceGroupAsync(subscriptionId, resourceGroup);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }
    }
}
