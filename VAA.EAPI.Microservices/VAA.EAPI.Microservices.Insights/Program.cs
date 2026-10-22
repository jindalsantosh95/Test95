using Azure.Core.Serialization;
using MediatR;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor;
using VAA.EAPI.Microservices.Insights.Application.GitHub;
using VAA.EAPI.Microservices.Insights.Application.Providers;
using VAA.EAPI.Microservices.Insights.Application.Query;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure;
using VAA.EAPI.Microservices.Insights.Infrastructure.GitHub;
using VAA.EAPI.Microservices.Insights.Interfaces;

namespace VAA.EAPI.Microservices.Insights
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentConfiguration();
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.AddDefaultMiddleware();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.Configure<WorkerOptions>(ConfigureWorkerSerialization);
                    services.AddDefaultServices();
                    services.AddHealthChecks()
                            .AddTypeActivatedCheck<HttpHealthCheck>(name: "HttpHealthCheck:AzureAPIM:Connectivity",
                                                                   failureStatus: HealthStatus.Unhealthy,
                                                                   tags: new[] { nameof(HttpHealthCheck), "Azure APIM Api" },
                                                                   args: new object[]
                                                                   {
                                                                       new Common.Http.HttpClientFactory(new LoggerFactory().CreateLogger<Common.Http.HttpClientFactory>()),
                                                                       string.Format(Constants.Environment.ApimSubscriptionListUri, ConfigReader.Get(Constants.Environment.ApimSubscriptionApiResource).Trim('/'), ConfigReader.Get<Guid>(Constants.Environment.ApimSubscriptionId), ConfigReader.Try(Constants.Environment.ApimApiVersion, Constants.Application.APIMApiVersion)),
                                                                       HttpMethod.Get,
                                                                       HttpStatusCode.Unauthorized
                                                                   });

                    services.AddSingleton<IKeyVault>(KeyVault.Default);
                    services.AddSingleton<ICatalogueProvider, CatalogueProvider>();
                    services.AddSingleton<IMetricsProvider, MetricsProvider>();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(VAA.EAPI.Infrastructure.Constants.Environment.RedisConnectionString));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<IDistributedCacheService, DistributedCacheService>();
                    services.AddSingleton<IMemoryCache, MemoryCache>();
                    services.AddSingleton<IADODeploymentCompiler, ADODeploymentCompiler>();
                    services.AddScoped<IAppSettingsMonitor, AppSettingsMonitor>();
                    services.AddScoped<IAzureResources, AzureResources>();
                    services.AddScoped<IGitHubApiService, GitHubApiService>();
                    services.AddScoped<IGitHubMergedPullRequestQueryHandler, GitHubMergedPullRequestQueryHandler>();
                    services.AddScoped<IADOPipelineProcessor, ADOPipelineProcessor>();
                    services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                    services.AddMediatR(options =>
                    {
                        options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        options.RegisterServicesFromAssemblyContaining(typeof(CatalogueRequest));
                        options.RegisterServicesFromAssemblyContaining(typeof(RedisCacheMetricsRequest));
                        options.RegisterServicesFromAssemblyContaining(typeof(ServiceBusMetricsRequest));
                        options.RegisterServicesFromAssemblyContaining(typeof(DeploymentMetricRequest));
                        options.RegisterServicesFromAssemblyContaining(typeof(AppInsightsMetricsRequest));
                    });
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();

                    // Re-registering to keep full worker service telemetry for monitoring/alerting by disabling adaptive sampling, which
                    // would otherwise drop some telemetry. QuickPulse is also disabled here because Live Metrics is not required.
                    var applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions
                    {
                        EnableAdaptiveSampling = false,
                        EnableQuickPulseMetricStream = false
                    };

                    services.AddApplicationInsightsTelemetryWorkerService(applicationInsightsServiceOptions);

                })
            .Build();

            return host.RunAsync();
        }

        public static void ConfigureWorkerSerialization(WorkerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            options.Serializer ??= new JsonObjectSerializer();
        }
    }
}
