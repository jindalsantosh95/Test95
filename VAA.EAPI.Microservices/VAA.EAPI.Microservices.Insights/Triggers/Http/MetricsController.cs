using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Insights.Application.Query;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{
    public sealed class MetricsController
    {
        private readonly IMediator mediator;
        private readonly ILogger<MetricsController> logger;

        public MetricsController(IMediator mediator, ILogger<MetricsController> logger)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetRedisCacheMetrics", tags: new[] { "Redis Cache Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetRedisCacheMetrics")]
        public async Task<HttpResponseData> GetRedisCacheMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rediscachemetrics")] HttpRequestData req)
        {
            var response = await mediator.Send(new RedisCacheMetricsRequest()
            {
            });

            return await ResultExtention<IEnumerable<RedisMetricsResult>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "GetServiceBusMetrics", tags: new[] { "Service Bus Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetServiceBusMetrics")]
        public async Task<HttpResponseData> GetServiceBusMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "servicebusmetrics")] HttpRequestData req)
        {
            var response = await mediator.Send(new ServiceBusMetricsRequest()
            {
            });

            return await ResultExtention<IEnumerable<ServiceBusMetricsResult>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "GetDeploymentMetrics", tags: new[] { "Deployment Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetDeploymentMetrics")]
        public async Task<HttpResponseData> GetDeploymentMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deploymentmetrics")] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var definitionIdsObject = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(requestBody);

                string[] definitionIds = definitionIdsObject.TryGetValue("DefinitionIds", out var ids) ? ids : Array.Empty<string>();

                var response = await mediator.Send(new DeploymentMetricRequest()
                {
                    DefinitionIds = definitionIds
                });

                return await ResultExtention<IEnumerable<DeploymentMetricsResult>>.ExecuteResultAsync(req, response);
            }
            catch (JsonSerializationException ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return errorResponse;
            }
        }

        [OpenApiOperation(operationId: "GetCosmosDbMetrics", tags: new[] { "Cosmos Db Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetCosmosDbMetrics")]
        public async Task<HttpResponseData> GetCosmosDbMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cosmosmetrics")] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var cosmosDbMetricRequest = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(requestBody);

                if (cosmosDbMetricRequest == null || !cosmosDbMetricRequest.TryGetValue("ResourceNames", out var resourceNames) || resourceNames == null || !resourceNames.Any())
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid request: ResourceNames are required and should not be empty.");
                    return errorResponse;
                }

                var response = await mediator.Send(new CosmosDbMetricRequest()
                {
                    ResourceNames = resourceNames
                });

                return await ResultExtention<IEnumerable<CosmosDbMetricResult>>.ExecuteResultAsync(req, response);
            }
            catch (JsonSerializationException ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"An error occurred: {ex.Message}");
                return errorResponse;
            }
        }

        [OpenApiOperation(operationId: "GetServicePlanMetrics", tags: new[] { "Service Plan Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetServicePlanMetrics")]
        public async Task<HttpResponseData> GetServicePlanMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "serviceplanmetrics")] HttpRequestData req)
        {
            try
            {
                var response = await mediator.Send(new ServicePlanMetricRequest()
                {
                });

                return await ResultExtention<IEnumerable<ServicePlanMetricResult>>.ExecuteResultAsync(req, response);
            }
            catch (JsonSerializationException ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return errorResponse;
            }
        }


        [OpenApiOperation(operationId: "GetAppInsightsMetrics", tags: new[] { "AppInsights Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetAppInsightsMetrics")]
        public async Task<HttpResponseData> GetAppInsightsMetrics([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appinsightsmetrics")] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var appInsightsRequest = JsonConvert.DeserializeObject<AppInsightsMetricsRequest>(requestBody);

                if (string.IsNullOrEmpty(appInsightsRequest.AppId) || string.IsNullOrEmpty(appInsightsRequest.ApiKey))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid request: AppId and ApiKey are required.");
                    return errorResponse;
                }

                var response = await mediator.Send(appInsightsRequest);

                return await ResultExtention<IEnumerable<AppInsightsMetricsResult>>.ExecuteResultAsync(req, response);
            }
            catch (JsonSerializationException ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetAppInsightsMetrics");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error.");
                return errorResponse;
            }
        }

    }
}