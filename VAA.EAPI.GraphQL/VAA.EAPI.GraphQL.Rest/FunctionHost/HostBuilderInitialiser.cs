using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.NodaTime;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Configuration;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Middleware;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using VAA.EAPI.GraphQL.Rest.FunctionHost.Security;
using VAA.EAPI.GraphQL.Rest.Middleware;
using VAA.EAPI.GraphQL.Rest.Middleware.Mcp;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    public sealed class HostBuilderInitialiser
    {
        public static explicit operator HostBuilderInitialiser(HostBuilder source) => new(source);

        static HostBuilderInitialiser()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
            {
                var root = ((AppDomain)s).BaseDirectory;

                var assemblyName = new AssemblyName(args.Name);

                var path = Directory.GetFiles(root, $"{assemblyName.Name}*.dll").FirstOrDefault();

                return path.IsNullOrWhiteSpace() ? default : Assembly.LoadFrom(path);
            };
        }

        private readonly IEnumerable<Assembly> assemblies;
        private readonly HostBuilder hostBuilder;
        private readonly string[] commandLineArguments = [];
        private readonly IEnumerable<Action<IConfigurationBuilder>> configureAppConfiguration = [];
        private readonly IEnumerable<Action<IFunctionsWorkerApplicationBuilder>> configureFunctionsWorkerDefaults = [];
        private readonly IEnumerable<Action<IServiceCollection>> configureServices = [];
        private readonly IEnumerable<Action<IRequestExecutorBuilder, bool>> registerQueries = [];
        private readonly IEnumerable<Action<IRequestExecutorBuilder, bool>> registerMutations = [];
        private readonly IEnumerable<Action<IHealthChecksBuilder>> registerHealthChecks = [];

        private HostBuilderInitialiser(HostBuilder hostBuilder)
        {
            this.hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            commandLineArguments = this.hostBuilder.Properties.ContainsKey("args") ? (string[])hostBuilder.Properties["args"] : [];

            assemblies = Directory.GetFiles(".", "VAA.EAPI.GraphQL.Domain.*.dll", SearchOption.TopDirectoryOnly)
                                  .Distinct()
                                  .Select(x => Assembly.LoadFile(System.IO.Path.GetFullPath(x)))
                                  .ToArray();

            var assembliesFilter = commandLineArguments.FirstOrDefault(x => RegexCompiled.IsMatch(x, @"^--assemblies:\s*"));

            if (!assembliesFilter.IsNullOrWhiteSpace())
            {
                var assembliesFilters = assembliesFilter.Split(':')[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

                assemblies = assemblies.Where(x => assembliesFilters.Any(f => RegexCompiled.IsMatch(x.FullName, f)));
            }

            var startups = assemblies.Where(x => x.ExportedTypes.Any(t => t.Name.Is("Startup") && t.GetInterface(nameof(IStartup)) != null))
                                     .Select(x =>
                                     {
                                         var type = x.ExportedTypes.First(t => t.Name.Is("Startup") && t.GetInterface(nameof(IStartup)) != null);

                                         var instance = (IStartup)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

                                         return new ConfigurationActions(
                                                     instance.ConfigureAppConfiguration,
                                                     instance.ConfigureFunctionsWorkerDefaults,
                                                     instance.ConfigureServices,
                                                     instance.RegisterQueries,
                                                     instance.RegisterMutations,
                                                     instance.RegisterHealthChecks);
                                     })
                                     .ToArray();

            configureAppConfiguration = startups.Select(x => x.ConfigureAppConfiguration);
            configureFunctionsWorkerDefaults = startups.Select(x => x.ConfigureFunctionsWorkerDefaults);
            configureServices = startups.Select(x => x.ConfigureServices);
            registerQueries = startups.Select(x => x.RegisterQueries);
            registerMutations = startups.Select(x => x.RegisterMutations);
            registerHealthChecks = startups.Select(x => x.RegisterHealthChecks);
        }

        public IHostBuilder Configure() => hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                if (commandLineArguments.Any())
                    builder.AddCommandLine(commandLineArguments);

                foreach (var invoke in configureAppConfiguration)
                    invoke(builder);

                builder.AddEnvironmentConfiguration();
            })
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                worker.UseMiddleware<FunctionContextAccessorMiddleware>();
                worker.UseMiddleware<HttpCompressionMiddleware>();
                worker.UseMiddleware<RequestValidationMiddleware>();
                worker.UseMiddleware<AuthenticationMiddleware>();
                worker.UseMiddleware<McpCallerContextMiddleware>();
                worker.UseMiddleware<McpMiddleware>();

                foreach (var invoke in configureFunctionsWorkerDefaults)
                    invoke(worker);
            })
            .ConfigureOpenApi()
            .ConfigureServices(services =>
            {
                var isSecure = ConfigReader.Get(Common.Constants.Environment.OperationsVisibilityMode).Is("Secure");

                services.AddSha256DocumentHashProvider(HotChocolate.Language.HashFormat.Hex);

                var builder = new RequestExecutorBuilder(services);

                builder.AddType<LocalDateTimeType>()
                       .AddType<LocalTimeType>()
                       .AddType<LocalDateType>();

                builder.TryAddType<VersionQuery>();

                foreach (var invoke in registerQueries)
                    invoke(builder, isSecure);

                builder.AddQueryType(q => q.Name("Query"));

                if (registerMutations.Any())
                {
                    var initial = builder.Services.Count;

                    foreach (var invoke in registerMutations)
                        invoke(builder, isSecure);

                    if (initial < builder.Services.Count)
                        builder.AddMutationType(q => q.Name("Mutation"));
                }

                services.AddLogging()
                        .AddHttpContextAccessor()
                        .AddApplicationInsightsTelemetryWorkerService()
                        .ConfigureFunctionsApplicationInsights()
                        .Configure<LoggerFilterOptions>(options =>
                        {
                            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs.
                            // Application Insights requires an explicit override. Log levels can also be configured using appsettings.json.
                            // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                            LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                              == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                            if (toRemove is not null)
                            {
                                options.Rules.Remove(toRemove);
                            }
                        })
                        .AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Common.Constants.Environment.RedisCacheConnectionString))
                        .AddSingleton<ITelemetryInitializer, DefaultTelemetryInitializer>()
                        .AddApplicationInsightsTelemetry()
                        .AddDefaultServices()
                        .AddSingleton<IMemoryCache, MemoryCache>()
                        .AddSingleton<INamingConventions>(new EnumNamingConvention())
                        .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
                        .AddSingleton<IApplicationOAuthTokenProvider, OAuthTokenProvider_AAD>()
                        .AddSingleton<IHttpConnector, HttpConnector>()
                        .AddSingleton<IAuthorizationHandler, HasRoleAuthorizationHandler>()
                        .AddSingleton<IAnalyticsService, AnalyticsService>()
                        .AddSingleton<IHttpClientFactory, Common.Http.HttpClientFactory>()
                        .AddSingleton<ITelemetryService, TelemetryService>()
                        .AddSingleton<IServiceCollection>((sp) => services)
                        .AddSingleton<Router>((serviceProvider) => new Router(services, serviceProvider));

                var healthChecksBuilder = services.AddHealthChecks();

                foreach (var invoke in registerHealthChecks)
                    invoke(healthChecksBuilder);

                foreach (var invoke in configureServices)
                    invoke(services);

                // remove duplicate registrations of the same service type, which can happen when multiple startups register the same service,
                // for example when multiple domains register the same repository interfaces and implementations, which is a common pattern
                // in our GraphQL API
                var duplicates = services.Select((x, i) => new { ServiceType = x.ServiceType, Index = i })
                                                   .Where(x => x.ServiceType.FullName.StartsWith("VAA."))
                                                   .GroupBy(x => x.ServiceType.FullName)
                                                   .Where(x => x.Count() > 1)
                                                   .ToArray();

                foreach (var index in duplicates.SelectMany(g => g.SkipLast(1)).Select(x => x.Index).OrderDescending())
                    services.RemoveAt(index);
            });
    }
}
