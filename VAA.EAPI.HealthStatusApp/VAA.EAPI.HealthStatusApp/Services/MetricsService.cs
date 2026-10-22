using Newtonsoft.Json;
using System.Diagnostics;
using System;
using System.Text;
using VAA.EAPI.HealthStatusApp.Components;
using VAA.EAPI.HealthStatusApp.Helpers;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using VAA.EAPI.HealthStatusApp.Models.MetricsModels;
using System.Text.RegularExpressions;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IInsightsConnector _insightsConnector;

        public MetricsService(IInsightsConnector insightsConnector, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<MetricsService> logger)
        {
            _insightsConnector = insightsConnector;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<IEnumerable<RedisCacheMetricsModel>> GetRedisCacheMetrics() => GetMetricsAsync<RedisCacheMetricsModel>("rediscachemetrics");

        public Task<IEnumerable<ServiceBusMetricsModel>> GetServiceBusMetrics() => GetMetricsAsync<ServiceBusMetricsModel>("servicebusmetrics");

        public Task<IEnumerable<ServicePlanMetricsModel>> GetServicePlanMetrics() => GetMetricsAsync<ServicePlanMetricsModel>("serviceplanmetrics");

        public Task<IEnumerable<CosmosDbMetricsModel>> GetCosmosDbMetrics()
        {
            var resourceNames = _configuration["HEALTH_CHECK_COSMOS"]?.Split(",") ?? Array.Empty<string>();
            var jsonBody = JsonConvert.SerializeObject(new { ResourceNames = resourceNames });

            return GetMetricsAsync<CosmosDbMetricsModel>("cosmosmetrics", jsonBody);
        }

        public async Task<LatestHeimdallResult> GetLatestHeimdallResult()
        {
            var heimdallResult = new LatestHeimdallResult();

            var tracesQuery = @"traces | where message contains ""Finished:"" | where timestamp > ago(1d) | order by timestamp desc | take 1";

            var result = await QueryHeimdallMetricsAsync<HeimdallTrace>(tracesQuery);

            if (result.Any())
            {
                var message = result.First().Message;
                int indexOfIn = message.IndexOf(" in ");
                string trimmed = message;
                if (indexOfIn >= 0)
                {
                    trimmed = message.Substring(0, indexOfIn) + message.Substring(message.IndexOf('(', indexOfIn));
                }
                message = "Heimdall " + trimmed;

                heimdallResult.Message = message;
                heimdallResult.TimeStamp = result.First().TimeStamp;
            }

            return heimdallResult;
        }

        public async Task<AppInsightsMetricsModel> GetHeimdallDataAsync(string timespan)
        {
            try
            {
                var exceptionsQuery = $@"exceptions | where timestamp > ago({timespan}) | extend OuterMessageJson = parse_json(outerMessage) | extend message = tostring(OuterMessageJson.Message) | project timestamp, message, outerMessage | order by timestamp desc"
                ;

                var exceptionsTask = await QueryHeimdallMetricsAsync<HeimdallException>(exceptionsQuery);

                var tracesQuery = $@" traces | where message contains 'Finished:' | where timestamp > ago({timespan}) | order by timestamp desc";

                var tracesTask = await QueryHeimdallMetricsAsync<HeimdallTrace>(tracesQuery);


                return new AppInsightsMetricsModel
                {
                    LatestHeimdallResult = new  LatestHeimdallResult{ Message = tracesTask.First().Message, TimeStamp = tracesTask.First().TimeStamp },
                    TotalSuccessful = HeimdallResultParser.GetTotalSuccessfulTests(tracesTask.Select(x=>x.Message)),
                    TotalFailed = HeimdallResultParser.GetTotalFailedOrSkippedTests(tracesTask.Select(x => x.Message)),
                    Exceptions = exceptionsTask
                                 .GroupBy(e => NormalizeMessage(e.Message))
                                 .Select(g => g.First())
                                 .OrderByDescending(e => e.TimeStamp)
                                
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying exceptions and traces.");
                return new AppInsightsMetricsModel { LatestHeimdallResult = new LatestHeimdallResult { Message = ex.Message } };
            }
        }

        private async Task<IEnumerable<T>> QueryHeimdallMetricsAsync<T>(string query)
        {
            try
            {
                var payload = new
                {
                    AppId = _configuration["HEALTH_CHECK_HEIMDALL_ID"],
                    ApiKey = _configuration["HEALTH_CHECK_HEIMDALL_KEY"],
                    Query = query
                };

                var body = JsonConvert.SerializeObject(payload);

                var response = await _insightsConnector.SendAsync("appinsightsmetrics", body, Enums.InsightsEnvironment.Default);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning("Received empty response for App Insights metrics.");
                    return Enumerable.Empty<T>();
                }

                return JsonConvert.DeserializeObject<IEnumerable<T>>(response) ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching App Insights metrics: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }



        public async Task<IEnumerable<DeploymentMetricsModel>> GetDeploymentMetrics()
        {
            var definitionIdsString = _configuration["HEALTH_CHECK_Definition_Ids"];
            var definitionIds = definitionIdsString?.Split(",") ?? Array.Empty<string>();

            var tasks = definitionIds.Select(GetDeploymentMetric);
            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null).SelectMany(result => result);
        }

        public async Task<IEnumerable<DeploymentMetricsModel>> GetDeploymentMetric(string definitionId)
        {
            try
            {
                var jsonBody = JsonConvert.SerializeObject(new { DefinitionIds = new[] { definitionId } });

                var response = await _insightsConnector.SendAsync("deploymentmetrics", jsonBody);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning("Received empty response for deploymentmetrics");
                    return Enumerable.Empty<DeploymentMetricsModel>();
                }

                return JsonConvert.DeserializeObject<IEnumerable<DeploymentMetricsModel>>(response) ?? Enumerable.Empty<DeploymentMetricsModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching deployment metrics for definitionId {definitionId}: {ex.Message}");
                return Enumerable.Empty<DeploymentMetricsModel>();
            }
        }



        private async Task<IEnumerable<T>> GetMetricsAsync<T>(string metricType)
        {
            try
            {
                var response = await _insightsConnector.SendAsync(metricType);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning($"Received empty response for {metricType}");
                    return Enumerable.Empty<T>();
                }

                return JsonConvert.DeserializeObject<IEnumerable<T>>(response) ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching {metricType} metrics: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        private async Task<IEnumerable<T>> GetMetricsAsync<T>(string metricType, string body)
        {
            try
            {
                var response = await _insightsConnector.SendAsync(metricType, body);

                if (string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning($"Received empty response for {metricType}");
                    return Enumerable.Empty<T>();
                }

                return JsonConvert.DeserializeObject<IEnumerable<T>>(response) ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching {metricType} metrics: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        private (int TotalSuccessful, int TotalFailedOrSkipped) ParseTestResults(List<string> testResults)
        {
            int totalSuccessful = 0;
            int totalFailedOrSkipped = 0;

            var pattern = @"\((\d+) passed, (\d+) failed, (\d+) skipped\)";
            var regex = new Regex(pattern);

            foreach (var result in testResults)
            {
                var match = regex.Match(result);
                if (match.Success)
                {
                    int passed = int.Parse(match.Groups[1].Value);
                    int failed = int.Parse(match.Groups[2].Value);
                    int skipped = int.Parse(match.Groups[3].Value);

                    totalSuccessful += passed;
                    totalFailedOrSkipped += failed + skipped;
                }
            }

            return (totalSuccessful, totalFailedOrSkipped);
        }


        private string NormalizeMessage(string message)
        {
            if (message.Contains("ErrorID"))
            {
                message = HeimdallResultParser.ExtractDistinctLogString(message);
            }

            return message;
        }
    }

}

