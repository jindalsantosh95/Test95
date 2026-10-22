using Azure.Identity;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.BiDi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common.Diagnostics;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Insights.Domain;
using VAA.EAPI.Microservices.Insights.Helper;
using VAA.EAPI.Microservices.Insights.Mapper;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    public sealed class MetricsProvider : IMetricsProvider
    {
        private IHttpConnector httpConnector;
        private ILogger<MetricsProvider> logger;
        private readonly string _resource;
        private readonly string _insightsApiVersion;

        public MetricsProvider(IHttpConnector httpConnector, ILogger<MetricsProvider> logger)
        {
            this.httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource).Trim('/');
            _insightsApiVersion = ConfigReader.Get(Constants.Environment.MetricsInsightsVersion);
        }


        public async Task<IEnumerable<RedisMetricsResult>> FetchRedisCacheMetricsAsync()
        {
            var uri = Constants.Environment.MetricsRedisListUri;
            var apiversion = ConfigReader.Get(Constants.Environment.MetricsRedisApiVersion);
            var metrics = new[] { "allusedmemorypercentage", "allusedmemory", "connectedclients" };
            return await FetchMetricsAsync<RedisMetricsResult>(uri, metrics, MetricsMapper.MapRedisMetricsResult, apiversion);
        }

        public async Task<IEnumerable<ServiceBusMetricsResult>> FetchServiceBusMetricsAsync()
        {
            var uri = Constants.Environment.MetricsServiceBusListUri;
            var apiversion = ConfigReader.Get(Constants.Environment.MetricsServiceBusApiVersion);
            var metrics = new[] { "DeadletteredMessages", "ActiveMessages" };
            return await FetchMetricsAsync<ServiceBusMetricsResult>(uri, metrics, MetricsMapper.MapServiceBusMetricsResult, apiversion);
        }


        public async Task<IEnumerable<CosmosDbMetricResult>> FetchCosmosMetricsAsync(IEnumerable<string> resourceNames)
        {
            var results = new List<CosmosDbMetricResult>();
            try
            {
                var clientId = ConfigReader.Get(Constants.Environment.CosmosClientId);
                var clientSecret = ConfigReader.Get(Constants.Environment.CosmosClientSecret);
                var authority = AuthorizationHelper.GetAuthority();

                await Parallel.ForEachAsync(resourceNames, async (resourceName, token) =>
                {
                    HttpRequestMessage metricsRequest = await AuthorizationHelper.CreateAuthorizationRequestV2(Constants.Environment.CosmosDbUrl, resourceName, clientId, clientSecret);

                    var metricsResponse = await httpConnector.SendAsync(metricsRequest, token);
                    if (metricsResponse.IsSuccessStatusCode)
                    {
                        var content = await metricsResponse.Content.ReadAsStringAsync();
                        var metrics = JsonConvert.DeserializeObject<MetricsResponse>(content);
                        results.Add(MetricsMapper.MapCosmosDbMetricsResult(metrics, resourceName));
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fetching Cosmos DB metrics failed.");
            }

            return results;
        }

        public async Task<IEnumerable<ServicePlanMetricResult>> FetchServicePlanMetricsAsync()
        {
            var servicePlanTasks = new List<Task<ServicePlanMetricResult>>();

            try
            {
                servicePlanTasks.Add(GetServicePlanDataAsync("Loyalty", ConfigReader.Get(Constants.Environment.LoyaltyPlanName),
                    ConfigReader.Get(Constants.Environment.LoyaltyPlanClient), ConfigReader.Get(Constants.Environment.LoyaltyPlanClientSecret)));

                servicePlanTasks.Add(GetServicePlanDataAsync("Basket", ConfigReader.Get(Constants.Environment.BasketPlanName),
                    ConfigReader.Get(Constants.Environment.BasketPlanClient), ConfigReader.Get(Constants.Environment.BasketPlanClientSecret)));

                servicePlanTasks.Add(GetServicePlanDataAsync("Offer", ConfigReader.Get(Constants.Environment.OfferPlanName),
                    ConfigReader.Get(Constants.Environment.OfferPlanClient), ConfigReader.Get(Constants.Environment.OfferPlanClientSecret)));

                servicePlanTasks.Add(GetServicePlanDataAsync("Order", ConfigReader.Get(Constants.Environment.OrderPlanName), ConfigReader.Get(Constants.Environment.OrderPlanClient), ConfigReader.Get(Constants.Environment.OrderPlanClientSecret)));

                servicePlanTasks.Add(GetServicePlanDataAsync("Graph", ConfigReader.Get(Constants.Environment.GraphPlanName),
                    ConfigReader.Get(Constants.Environment.GraphPlanClient), ConfigReader.Get(Constants.Environment.GraphPlanClientSecret)));

                servicePlanTasks.Add(GetServicePlanDataAsync("Others", ConfigReader.Get(Constants.Environment.APIMPlanName),
                    ConfigReader.Get(Constants.Environment.APIMPlanClient), ConfigReader.Get(Constants.Environment.APIMPlanClientSecret)));

                var servicePlans = await Task.WhenAll(servicePlanTasks);

                return servicePlans;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching Service Plan Metrics");
                return Enumerable.Empty<ServicePlanMetricResult>();
            }
        }


        public async Task<ServicePlanMetricResult> GetServicePlanDataAsync(string friendlyName, string name, string clientId, string clientSecret)
        {
            var servicePlanResult = new ServicePlanMetricResult
            {
                ServicePlanFriendlyName = friendlyName,
                ServicePlanName = name
            };

            try
            {
                HttpRequestMessage metricsRequest = await AuthorizationHelper.CreateAuthorizationRequestV2(Constants.Environment.ServicePlanBaseUrl, name, clientId, clientSecret);

                var metricsResponse = await httpConnector.SendAsync(metricsRequest, CancellationToken.None);
                if (metricsResponse.IsSuccessStatusCode)
                {
                    var content = await metricsResponse.Content.ReadAsStringAsync();
                    var metrics = JsonConvert.DeserializeObject<MetricsResponse>(content);
                    return MetricsMapper.MapServicePlanMetricsResult(metrics, name);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MetricsProvider.GetServicePlanDataAsync failed. Error getting Service Plan Metrics for {FriendlyName}", friendlyName);
            }

            return servicePlanResult;
        }


        public async Task<IEnumerable<DeploymentMetricsResult>> FetchDeploymentMetricsAsync(string[] definitionIds)
        {
            var deploymentsResults = new List<DeploymentMetricsResult>();

            try
            {
                var devOpsPAT = ConfigReader.Get(Constants.Environment.DevOpsPAT);
                string project = ConfigReader.Get(Constants.Environment.DevOpsProject);
                string baseUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
                string prodStageName = "prod";

                var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{devOpsPAT}"));

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                    var pipelinesUrl = string.Format(Constants.Environment.PipelineUrl, baseUrl, project);
                    var pipelinesResponse = await client.GetAsync(pipelinesUrl);
                    pipelinesResponse.EnsureSuccessStatusCode();

                    var pipelinesContent = await pipelinesResponse.Content.ReadAsStringAsync();
                    var pipelines = JObject.Parse(pipelinesContent)["value"] as JArray;
                    if (pipelines == null) return deploymentsResults;

                    var filteredPipelines = pipelines
                        .Where(pipeline => definitionIds.Contains(pipeline["id"]?.ToString()));

                    const int batchSize = 25;
                    for (int i = 0; i < filteredPipelines.Count(); i += batchSize)
                    {
                        var batch = filteredPipelines.Skip(i).Take(batchSize);
                        var batchTasks = batch.Select(pipeline =>
                            ProcessPipelineAsync(client, pipeline, baseUrl, project, prodStageName));

                        var batchResults = await Task.WhenAll(batchTasks);
                        deploymentsResults.AddRange(batchResults.Where(r => r != null));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching Deployment Metrics");
            }

            return deploymentsResults;
        }


        public async Task<IEnumerable<AppInsightsMetricsResult>> FetchAppInsightsMetricsAsync(string appId, string apiKey, string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(appId)) throw new ArgumentNullException(nameof(appId));
                if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentNullException(nameof(apiKey));

                var url = $"https://api.applicationinsights.io/v1/apps/{appId}/query";
                var queryBody = new { query = query };

                var jsonBody = JsonConvert.SerializeObject(queryBody);

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("x-api-key", apiKey);

                var response = await httpConnector.SendAsync(request, CancellationToken.None);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch metric. StatusCode: {StatusCode}", response.StatusCode);
                    return Enumerable.Empty<AppInsightsMetricsResult>();
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var traceResults = ParseTraceResults(jsonResponse);
                return traceResults;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching metric");
                return Enumerable.Empty<AppInsightsMetricsResult>();
            }
        }

        private IEnumerable<AppInsightsMetricsResult> ParseTraceResults(string jsonResponse)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            var tables = jsonObject?.tables as JArray;
            if (tables == null || !tables.Any())
                return Enumerable.Empty<AppInsightsMetricsResult>();

            var firstTable = tables.First;
            var columns = firstTable["columns"].ToObject<List<dynamic>>();
            var rows = firstTable["rows"] as JArray;

            var columnIndexMap = columns
                .Select((col, index) => new { col.name, Index = index })
                .ToDictionary(c => c.name.ToString(), c => c.Index);

            return rows.Select(row => new AppInsightsMetricsResult
            {
                Timestamp = DateTime.Parse(row[columnIndexMap["timestamp"]].ToString()),
                Message = row[columnIndexMap["message"]]?.ToString(),
            });
        }



        private async Task<DeploymentMetricsResult> ProcessPipelineAsync(HttpClient client, JToken pipeline, string baseUrl, string project, string prodStageName)
        {
            try
            {
                var pipelineId = pipeline["id"]?.ToString();
                var pipelineName = pipeline["name"]?.ToString();

                var runsUrl = string.Format(Constants.Environment.PipelineRunsUrl, baseUrl, project, pipelineId) + "&$top=100";

                var runsResponse = await client.GetAsync(runsUrl);

                if (!runsResponse.IsSuccessStatusCode) return null;

                var runsContent = await runsResponse.Content.ReadAsStringAsync();
                var builds = JObject.Parse(runsContent)["value"] as JArray;

                if (builds == null || !builds.Any()) return null;

                foreach (var build in builds)
                {
                    var buildId = build["id"]?.ToString();
                    var timelineUrl = string.Format(Constants.Environment.DevOpsBuildUrl, baseUrl, project, buildId);

                    var timelineResponse = await client.GetAsync(timelineUrl);
                    if (!timelineResponse.IsSuccessStatusCode) continue;

                    var timelineContent = await timelineResponse.Content.ReadAsStringAsync();
                    var timelineRecords = JObject.Parse(timelineContent)["records"] as JArray;

                    var prodStage = timelineRecords?
                        .FirstOrDefault(r =>
                            r["type"]?.ToString() == "Stage" &&
                            r["name"].ToString().Contains(prodStageName, StringComparison.OrdinalIgnoreCase) &&
                            r["state"]?.ToString() == "completed" &&
                            r["result"]?.ToString() == "succeeded");

                    if (prodStage != null)
                    {
                        return new DeploymentMetricsResult
                        {
                            ServiceName = pipelineName,
                            LastDeployment = DateTime.Parse(prodStage["finishTime"]?.ToString() ?? string.Empty)
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing pipeline {Name}", pipeline["name"]);
                return null;
            }
        }


        private async Task<IEnumerable<T>> FetchMetricsAsync<T>(string listUri, IEnumerable<string> metricNames, Func<MetricsResponse, string, T> mapper, string apiVersion)
        {
            var results = new List<T>();

            try
            {
                var clientId = ConfigReader.Get(Constants.Environment.MetricsSubscriptionClientId);
                var clientSecret = ConfigReader.Get(Constants.Environment.MetricsSubscriptionClientSecret);
                var authority = AuthorizationHelper.GetAuthority();

                var listRequest = AuthorizationHelper.CreateAuthorizedRequest(HttpMethod.Get, listUri, clientId, clientSecret, authority, apiVersion);

                var response = await httpConnector.SendAsync(listRequest, CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var resourceList = (JArray)JsonConvert.DeserializeObject<JToken>(json)["value"];
                    var resources = resourceList.ToObject<JToken[]>()
                                              .Select(x => new
                                              {
                                                  Id = x["id"].ToString(),
                                                  Name = x["name"].ToString()
                                              });

                    if (resources != null)
                    {
                        foreach (var resource in resources)
                        {
                            var id = resource.Id;
                            var name = resource.Name;
                            if (id == null || name == null) continue;

                            var metricsRequest = AuthorizationHelper.CreateAuthorizedRequest(
                                HttpMethod.Get,
                                $"{_resource}{id}/providers/microsoft.insights/metrics?api-version={_insightsApiVersion}&metricnames={string.Join(",", metricNames)}&timespan=PT1H&aggregation=average",
                                clientId,
                                clientSecret,
                            authority);

                            var metricsResponse = await httpConnector.SendAsync(metricsRequest, CancellationToken.None);

                            if (metricsResponse.IsSuccessStatusCode)
                            {
                                var content = await metricsResponse.Content.ReadAsStringAsync();
                                var metrics = JsonConvert.DeserializeObject<MetricsResponse>(content);
                                results.Add(mapper(metrics, name));
                            }
                        }
                    }
                }
                else
                {
                    logger.LogWarning("Failed to fetch resources: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fetching metrics failed.");
            }

            return results;
        }
    }
} 
