using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor;
using VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{

    public class AppSettingsController(IAppSettingsMonitor configMonitor)
    {
        private readonly IAppSettingsMonitor _configMonitor = configMonitor ?? throw new ArgumentNullException(nameof(configMonitor));

        [OpenApiOperation(operationId: "FindRecentDiffAlerts", tags: new[] { "AppSettings" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "timeframe", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = false, Type = typeof(TimeSpan), Summary = "Timeframe as Timespan", Description = "The timeframe to look back for diff alerts.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<AppInsightAlert>), Summary = "OK", Description = "Recent diff alerts from Application Insights.")]
        [Function("FindRecentDiffAlerts")]
        public async Task<HttpResponseData> FindRecentDiffAlerts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "appSettings/diffalerts")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            // Default timeframe to look back is 60 minutes if not provided or invalid
            var timeframeMinutes = TimeSpan.TryParse(query["timeframe"], out var timeFrame) ? timeFrame.TotalMinutes : 60;
            if ((timeframeMinutes <= 0) || (timeframeMinutes > (1440 * 7))) // Validate timeframe is between 1 minute and 7 days
            {
                timeframeMinutes = 60; // Default to 60 minutes if invalid value provided
            }

            var result = await _configMonitor.FindRecentDiffAlerts(TimeSpan.FromMinutes(timeframeMinutes));

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "FullCompareAppSettings", tags: new[] { "AppSettings" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "Full comparison executed.")]
        [Function("FullCompareAppSettings")]
        public async Task<HttpResponseData> FullCompare([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appSettings/fullcompare")] HttpRequestData req)
        {
            await _configMonitor.FullCompare();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync("Full comparison executed.");

            return response;
        }

        [OpenApiOperation(operationId: "FullCheckKeyVaultResolverStatus", tags: new[] { "AppSettings" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "Full Key Vault resolver status check executed.")]
        [Function("FullCheckKeyVaultResolverStatus")]
        public async Task<HttpResponseData> FullCheckKeyVaultResolverStatus([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appSettings/keyvaultresolverstatus/fullcheck")] HttpRequestData req)
        {
            await _configMonitor.FullCheckKeyVaultResolverStatus();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync("Full Key Vault resolver status check executed.");

            return response;
        }

        [OpenApiOperation(operationId: "CompareAppEnvironmentSettings", tags: new[] { "AppSettings" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroupName", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiParameter(name: "appName", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "App Name", Description = "Azure App Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AppSettingCompareResponse), Summary = "OK", Description = "App environment settings returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Summary = "Bad Request", Description = "Required request fields are missing.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Summary = "Internal Server Error", Description = "Unexpected processing error")]
        [Function("CompareAppEnvironmentSettings")]
        public async Task<HttpResponseData> CompareAppEnvironmentSettings([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "appSettings/compare")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var subscriptionId = query["subscriptionId"];
            var resourceGroupName = query["resourceGroupName"];
            var appName = query["appName"];

            if (string.IsNullOrWhiteSpace(subscriptionId) || string.IsNullOrWhiteSpace(resourceGroupName) || string.IsNullOrWhiteSpace(appName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync("Missing required query parameters.");
                return badResponse;
            }

            var result = await _configMonitor.Compare(subscriptionId, resourceGroupName, appName);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [OpenApiOperation(operationId: "SnapshotAppSettings", tags: new[] { "AppSettings" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionId", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Subscription Id", Description = "Azure Subscription Id.")]
        [OpenApiParameter(name: "resourceGroupName", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Resource Group Name", Description = "Azure Resource Group Name.")]
        [OpenApiParameter(name: "appName", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "App Name", Description = "Azure App Name.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AppSettingCompareResponse), Summary = "OK", Description = "Snapshot taken and updated comparison returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Summary = "Internal Server Error", Description = "Unexpected processing error")]
        [Function("SnapshotAppSettings")]
        public async Task<HttpResponseData> SnapshotAppSettings([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appSettings/snapshot")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var subscriptionId = query["subscriptionId"];
            var resourceGroupName = query["resourceGroupName"];
            var appName = query["appName"];

            if (string.IsNullOrWhiteSpace(subscriptionId) || string.IsNullOrWhiteSpace(resourceGroupName) || string.IsNullOrWhiteSpace(appName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync("Missing required query parameters.");
                return badResponse;
            }

            var result = await _configMonitor.Snapshot(subscriptionId, resourceGroupName, appName);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }
    }
}
