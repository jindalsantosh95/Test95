using Newtonsoft.Json;
using System.Linq;
using VAA.EAPI.HealthStatusApp.Enums;
using VAA.EAPI.HealthStatusApp.Helpers;
using VAA.EAPI.HealthStatusApp.Models;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using static System.Net.WebRequestMethods;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly ILogger<InsightsService> _logger;
        private readonly IInsightsConnector _insightsConnector;

        public InsightsService(IInsightsConnector insightsConnector, ILogger<InsightsService> logger)
        {
            _logger = logger;
            _insightsConnector = insightsConnector;
        }

        public async Task<IEnumerable<APIMStatusModel>> GetAPIMStatusTableData()
        {
            try
            {
                var aPIMStatusList = new List<APIMStatusModel>();
                var response = await _insightsConnector.SendAsync("catalogue");
                var endpoints = JsonConvert.DeserializeObject<IEnumerable<CustomHealthReport>>(response);


                foreach (var item in endpoints)
                {
                    var model = new APIMStatusModel();
                    model.ServiceUrl = item.Source;
                    model.ServiceName = item.Source.GetSourceName();
                    model.StatusCode = item.StatusCode;

                    aPIMStatusList.Add(model);

                    if (item.Result != null && item.Result.Entries != null && item.Result.Entries.Any())
                    {
                        var heartbeatItems = item.Result.Entries.Where(entry => entry.Value.Tags.Contains("Heartbeat"));

                        if (heartbeatItems != null && heartbeatItems.Any())
                            model.Description = heartbeatItems.FirstOrDefault().Value.Description;

                        model.ServiceBusChecks = new Dictionary<string, bool>();
                        model.CosmosChecks = new Dictionary<string, bool>();

                        foreach (var entry in item.Result.Entries.Where(entry => entry.Value.Tags.Contains("Service Bus Api")))
                        {
                            model.ServiceBusChecks.Add(entry.Key.Split(new string[] { "ServiceBusCheck" }, StringSplitOptions.None)[0], entry.Value.Status == 2);
                        }

                        foreach (var entry in item.Result.Entries.Where(entry => entry.Value.Tags.Contains("Cosmos Check")))
                        {
                            model.CosmosChecks.Add(entry.Key.Split(new string[] { "CosmosCheck" }, StringSplitOptions.None)[0], entry.Value.Status == 2);
                        }
                    }
                }

                return aPIMStatusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking health statuses for {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<LinkedServiceStatusModel>> GetLinkedServiceTableData()
        {
            try
            {
                var serviceStatusList = new List<LinkedServiceStatusModel>();
                var response = await _insightsConnector.SendAsync("catalogue");
                if (response == null) return null;
                var endpoints = JsonConvert.DeserializeObject<IEnumerable<CustomHealthReport>>(response);

                if (endpoints == null) return null;


                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        if (endpoint.Result != null && endpoint.Result.Entries != null && endpoint.Result.Entries.Any())
                        {

                            foreach (var entry in endpoint.Result.Entries)
                            {
                                if (entry.Value == null || !entry.Value.Tags.Any())
                                    continue;

                                if (entry.Value.Tags.Contains("Service Bus Api"))
                                {
                                    serviceStatusList.Add(new LinkedServiceStatusModel
                                    {
                                        ServiceName = entry.Key.Split(new string[] { "ServiceBusCheck" }, StringSplitOptions.None)[0],
                                        StatusCode = entry.Value.Status,
                                        Description = entry.Value.Description,
                                        ParentServiceCheckName = endpoint?.Source?.GetSourceName(),
                                        ServiceType = "Service Bus"
                                    });
                                }
                                if (entry.Value.Tags.Contains("Cosmos Check"))
                                {
                                    serviceStatusList.Add(new LinkedServiceStatusModel
                                    {
                                        ServiceName = entry.Key.Split(new string[] { "CosmosCheck" }, StringSplitOptions.None)[0],
                                        StatusCode = entry.Value.Status,
                                        Description = entry.Value.Description,
                                        ParentServiceCheckName = endpoint?.Source?.GetSourceName(),
                                        ServiceType = "Cosmos Db"
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error checking health status endpoint: {ex.Message}");
                        continue;
                    }
                }
                return serviceStatusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting health status ednpoints: {ex.Message}");
                return null;
            }
        }

        public async Task<HealthCheckDashboardData> GetInisghtsCatalogueForDashboard(InsightsEnvironment env = InsightsEnvironment.Default)
        {
            try
            {
                var model = new HealthCheckDashboardData();

                var response = await _insightsConnector.SendAsync("catalogue", env);

                if (response == null) return null;
                var endpoints = JsonConvert.DeserializeObject<IEnumerable<CustomHealthReport>>(response);
                if (endpoints == null) return null;

                var entries = endpoints
                    .Where(x => x?.Result != null && x.Result.Entries != null && x.Result.Entries.Any())
                    .SelectMany(apiResponse => apiResponse.Result.Entries
                    .Where(entry => entry.Value != null
                            && entry.Value.Tags != null))
                    .Distinct();

                var healthyEntries = entries
                    .Where(entry => entry.Value.Status == 2);

                var unHealthyEntries = entries
                    .Where(entry => entry.Value.Status != 2);

                model.GraphQlServices = GetGraphQLList(endpoints);
                model.ServiceBusServices = GetServiceStatusList(entries, "Service Bus Api");
                model.CosmosServices = GetServiceStatusList(entries, "Cosmos Check");


                 model.APIMScore += endpoints
                    .Where(x => x.Result != null && (x.StatusCode == 200 || x.Result.Status == 2))
                    .Where(apiResponse => !apiResponse.Source.Contains("graphql", StringComparison.OrdinalIgnoreCase) && !apiResponse.Source.Contains("/search/", StringComparison.OrdinalIgnoreCase))
                    .Count();


                model.UnhealthyScore += endpoints
                    .Where(x => x.Result != null && (x.Result.Status != 200 && x.Result.Status != 2))
                    .Where(apiResponse => !apiResponse.Source.Contains("graphql", StringComparison.OrdinalIgnoreCase) && !apiResponse.Source.Contains("/search/", StringComparison.OrdinalIgnoreCase))
                    .Count();
                
                model.CosmosScore = healthyEntries.Where(x => x.Value.Tags.Contains("Cosmos Check")).Count();
                model.ServiceBusScore = healthyEntries.Where(x => x.Value.Tags.Contains("Service Bus Api")).Count();
                model.UnhealthyCosmosScore = unHealthyEntries.Where(x => x.Value.Tags.Contains("Cosmos Check")).Count();
                model.UnhealthyServiceBusScore = unHealthyEntries.Where(x => x.Value.Tags.Contains("Service Bus Api")).Count();

                var graphqlEndpoints = endpoints
                   .Where(x => x.Result != null)
                   .Where(apiResponse => apiResponse.Source.Contains("graphql", StringComparison.OrdinalIgnoreCase) || apiResponse.Source.Contains("/search/", StringComparison.OrdinalIgnoreCase));

               model.GraphQlScore = graphqlEndpoints
                    .Where(apiResponse => apiResponse.StatusCode == 200)
                    .Count();
                model.UnhealthyGraphQlScore = graphqlEndpoints
                    .Where(apiResponse => apiResponse.StatusCode != 200)
                    .Count();

                model.Issues = GetIssues(endpoints);

                if (model.Issues != null && model.Issues.Any())
                {
                    if (model.Issues.Count() == 1)
                    {
                        model.IssueAlert = model.Issues.First().Service;
                    }
                    else if (model.Issues.Count() > 1)
                    {
                        model.IssueAlert = "multiple services!";
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Inisghts Catalogue for Dashboard {ex.Message}");
                return null;
            }
        }

        private IEnumerable<DashboardIssue> GetIssues(IEnumerable<CustomHealthReport> endpoints)
        {
            try
            {
                return endpoints
                    .Where(endpoint => endpoint != null)
                    .SelectMany(endpoint =>
                    {
                        if (endpoint.Result?.Entries != null && endpoint.Result.Entries.Any())
                        {
                            return endpoint.Result.Entries
                                .Where(item =>
                                    (item.Value.Data.ExpectedHttpStatusCode != item.Value.Data.StatusCode) ||
                                    (!item.Value.Tags.Contains("Heartbeat") && item.Value.Status != 2))
                                .Select(item => new DashboardIssue
                                {
                                    Service = item.Value.Data.ExpectedHttpStatusCode != item.Value.Data.StatusCode
                                        ? item.Key.GetSourceName()
                                        : item.Key.Split('.')[0],
                                    StatusCode = item.Value.Data.ExpectedHttpStatusCode != item.Value.Data.StatusCode
                                        ? (item.Value.Data.StatusCode == null ? item.Value.Status : endpoint.StatusCode)
                                        : item.Value.Status,
                                    Description = item.Value.Description
                                });
                        }
                        else if (endpoint.StatusCode != 200)
                        {
                            return
                            [
                                new DashboardIssue
                                {
                                    Service = endpoint.Source.GetSourceName(),
                                    StatusCode = endpoint.StatusCode,
                                }
                            ];
                        }
                        return Enumerable.Empty<DashboardIssue>();
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking Issues list {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, bool> GetGraphQLList(IEnumerable<CustomHealthReport> endpoints)
        {
            try
            {
                var HealthyServices = endpoints
                    .Where(apiResponse =>
                        (apiResponse.Source.Contains("graphql", StringComparison.OrdinalIgnoreCase) ||
                         apiResponse.Source.EndsWith("search/public/heartbeat", StringComparison.OrdinalIgnoreCase))
                        && apiResponse.StatusCode == 200)
                    .Select(x => x.Source.GetGraphQLCoreName())
                    .Distinct()
                    .ToList();

                var failingServices = endpoints
                    .Where(apiResponse =>
                        (apiResponse.Source.Contains("graphql", StringComparison.OrdinalIgnoreCase) ||
                         apiResponse.Source.EndsWith("search/public/heartbeat", StringComparison.OrdinalIgnoreCase))
                        && apiResponse.StatusCode != 200)
                    .Select(x => x.Source.GetGraphQLCoreName())
                    .Distinct()
                    .ToList();


                var servicesStatus = new Dictionary<string, bool>();

                foreach (var key in failingServices)
                {
                    if (!servicesStatus.ContainsKey(key))
                    {
                        servicesStatus[key] = false;
                    }
                }

                foreach (var key in HealthyServices)
                {
                    if (!servicesStatus.ContainsKey(key))
                    {
                        servicesStatus[key] = true;
                    }
                }

                return servicesStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Graph QL list {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, bool> GetServiceStatusList(IEnumerable<KeyValuePair<string,Entry>> entries, string tag)
        {
            try
            {
                var HealthyServices = entries
                        .Where(entry => entry.Value.Tags.Contains(tag)
                                        && entry.Value.Status == 2)
                        .Select(entry => entry.Key);

                var failingServices = entries
                        .Where(entry => entry.Value.Tags.Contains(tag)
                                        && entry.Value.Status != 2)
                        .Select(entry => entry.Key);

                var servicesStatus = new Dictionary<string, bool>();

                foreach (var key in failingServices)
                {
                    if (!servicesStatus.ContainsKey(key))
                    {
                        servicesStatus[key] = false;
                    }
                }

                foreach (var key in HealthyServices)
                {
                    if (!servicesStatus.ContainsKey(key))
                    {
                        servicesStatus[key] = true;
                    }
                }

                return servicesStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Service Status list {ex.Message}");
                return null;
            }
        }
    }
}
