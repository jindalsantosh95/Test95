using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    public sealed class CatalogueProvider : ICatalogueProvider
    {
        private IHttpConnector httpConnector;
        private ILogger<CatalogueProvider> logger;
        private static readonly ConcurrentDictionary<string, (string token, DateTime expiration)> _tokenCache = new ConcurrentDictionary<string, (string token, DateTime expiration)>();


        public CatalogueProvider(IHttpConnector httpConnector, ILogger<CatalogueProvider> logger)
        {
            this.httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICollection<Endpoint>> FetchCatalogueAsync(Guid subscriptionId, string apiVersion, string? environment)
        {
            var clientId = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientId);
            var clientSecret = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiClientSecret);
            var authority = $"{ConfigReader.Get(Constants.Environment.AzureAdInstance)}/{ConfigReader.Get(Constants.Environment.AzureAdTenantId)}";
            var resource = ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource).Trim('/');
            var authRequest = new HttpRequestMessage(HttpMethod.Get, string.Format(Constants.Environment.ApimSubscriptionListUri, resource, subscriptionId, apiVersion));

            authRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource));

            var response = await httpConnector.SendAsync(authRequest, CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var id = ((JArray)JsonConvert.DeserializeObject<JObject>(json)["value"])[1]["id"].ToString();

                var apiServiceRequest = new HttpRequestMessage(HttpMethod.Get, $"{resource}{id}?api-version={apiVersion}");

                apiServiceRequest.Headers.Authorization = authRequest.Headers.Authorization;

                response = await httpConnector.SendAsync(apiServiceRequest, CancellationToken.None);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();

                    var apimServiceResult = (JArray)JsonConvert.DeserializeObject<JToken>(jsonResult)["properties"]["hostnameConfigurations"];

                    var hostname = apimServiceResult.ToObject<JToken[]>().First(x => x["type"].ToString() == "Proxy" && x["hostName"].ToString() == ConfigReader.Get(Constants.Environment.ApimHostName) && x["defaultSslBinding"].ToString() == "True")["hostName"].ToString();

                    if (!string.IsNullOrEmpty(hostname))
                    {
                        var environmentTag = !string.IsNullOrEmpty(environment) ? environment : ConfigReader.Try("ApimEnvironment", "dev");

                        var apiRequest = new HttpRequestMessage(HttpMethod.Get, $"{resource}{id}/apis?api-version={apiVersion}&tags={environmentTag}");

                        apiRequest.Headers.Authorization = authRequest.Headers.Authorization;

                        response = await httpConnector.SendAsync(apiRequest, CancellationToken.None);

                        if (response.IsSuccessStatusCode)
                        {
                            json = await response.Content.ReadAsStringAsync();

                            var apis = (JArray)JsonConvert.DeserializeObject<JToken>(json)["value"];
                            var endpoints = apis.ToObject<JToken[]>()
                                                .Select(x => x["properties"]["path"].ToString())
                                                .Distinct()
                                                .Where(x => !x.IsNullOrWhiteSpace())
                                                .OrderBy(x => x)
                                                .ToArray();

                            ConcurrentDictionary<string, HttpResponseMessage> responses = new();

                            var threadStarts = endpoints.Select(x => new ThreadStart(() =>
                            {
                                var trace = logger.CreateTrace();
                                var heartbeatUrl = $"https://{hostname}/{x}/heartbeat";
                                var heartbeatRequest = new HttpRequestMessage(HttpMethod.Get, heartbeatUrl);

                                heartbeatRequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.HealthCheckSubscriptionKey));

                                HttpResponseMessage response = default;

                                try
                                {
                                    var token = GetTokenForEndpoint(x.ToLowerInvariant());

                                    if(x == "emailmicroservice")
                                    {
                                        heartbeatRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AzureADService.GetAccessToken(ConfigReader.Get(Constants.Environment.EmailAuthClientId), ConfigReader.Get(Constants.Environment.EmailAuthClientSecret), authority, token).GetAwaiter().GetResult());
                                    }
                                    else if (!token.IsNullOrEmpty())
                                        heartbeatRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AzureADService.GetAccessToken(ConfigReader.Get(Constants.Environment.HealthCheckAuthClientId), ConfigReader.Get(Constants.Environment.HealthCheckAuthClientSecret), authority, token).GetAwaiter().GetResult());


                                    response = httpConnector.SendAsync(heartbeatRequest, CancellationToken.None).GetAwaiter().GetResult();

                                    if (!response.IsSuccessStatusCode)
                                        throw new HttpResponseException(response);

                                    responses.AddOrUpdate(x, response, (k, v) => response);
                                }
                                catch (HttpResponseException ex)
                                {
                                    response = ConvertToHealthReport(ex, trace);
                                }
                                catch (Exception ex)
                                {
                                    response = ConvertToHealthReport(ex, heartbeatRequest, trace);
                                }

                                responses.AddOrUpdate(x, response, (k, v) => response);
                            }));

                            await Common.Threading.Concurrent.WhenAll(threadStarts);

                            return responses.Select(x => (Endpoint)x.Value)
                                            .ToArray();
                        }
                    }
                }
            }

            return Array.Empty<Endpoint>();
        }

        private HttpResponseMessage ConvertToHealthReport(HttpResponseException exception, Trace<CatalogueProvider> trace)
        {
            var healthEntries = new Dictionary<string, HealthReportEntry>();
            var data = new Dictionary<string, object>
            {
                { "endPoint", exception.Response.RequestMessage.RequestUri.ToString() },
                { "expectedHttpStatusCode", 200 },
                { "StatusCode", exception.Response.StatusCode }
            };

            healthEntries.Add(exception.Response.RequestMessage.RequestUri.ToString(), new HealthReportEntry(HealthStatus.Unhealthy, exception.Message, trace.Elapsed, exception, data, new string[] { "Heartbeat" }));

            return new HttpResponseMessage()
            {
                RequestMessage = exception.Response.RequestMessage,
                StatusCode = exception.Response.StatusCode,
                Content = new StringContent(Json.Serialize(new HealthReport(healthEntries, HealthStatus.Unhealthy, trace.Elapsed)), System.Text.Encoding.UTF8, "applciation/json")
            };
        }

        private HttpResponseMessage ConvertToHealthReport(Exception exception, HttpRequestMessage request, Trace<CatalogueProvider> trace)
        {
            var healthEntries = new Dictionary<string, HealthReportEntry>();
            var data = new Dictionary<string, object>
            {
                { "endPoint", request.RequestUri.ToString() },
                { "expectedHttpStatusCode", 200 },
                { "StatusCode", string.Empty}
            };

            healthEntries.Add(request.RequestUri.ToString(), new HealthReportEntry(HealthStatus.Unhealthy, exception.Message, trace.Elapsed, exception, data, new string[] { "Heartbeat" }));

            return new HttpResponseMessage()
            {
                RequestMessage = request,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Content = new StringContent(Json.Serialize(new HealthReport(healthEntries, HealthStatus.Unhealthy, trace.Elapsed)), System.Text.Encoding.UTF8, "applciation/json")
            };
        }


        private string GetTokenForEndpoint(string endpoint) => endpoint switch
        {
            "customermicroservice" => ConfigReader.Get(Constants.Environment.CustomerMicroserviceResourceId),
            "emailmicroservice" => ConfigReader.Get(Constants.Environment.EmailMicroserviceResourceId),
            "identitymicroservice" => ConfigReader.Get(Constants.Environment.IdentityMicroserviceResourceId),
            "inrule" => ConfigReader.Get(Constants.Environment.InRuleMicroserviceResourceId),
            "vredmicroservice" => ConfigReader.Get(Constants.Environment.VredMicroserviceResourceId),
            "campaignmanagement" => ConfigReader.Get(Constants.Environment.CampaignManagementMicroserviceResourceId),
            "enrolmentmicroservice" => ConfigReader.Get(Constants.Environment.EnrolmentMicroserviceResourceId),
            "crmmicroservice" => ConfigReader.Get(Constants.Environment.CrmMicroserviceResourceId),
            _ => string.Empty
        };

    }
}
