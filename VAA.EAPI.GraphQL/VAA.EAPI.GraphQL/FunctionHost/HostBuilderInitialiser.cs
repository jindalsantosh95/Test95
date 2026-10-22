using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Descriptors;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.FunctionHost.Security;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using VAA.EAPI.GraphQL.Infrastructure.Configuration;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Middleware;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using VAA.EAPI.GraphQL.Middleware;

namespace VAA.EAPI.GraphQL.FunctionHost
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

        private readonly HostBuilder hostBuilder;
        private readonly string[] commandLineArguments = Array.Empty<string>();
        private readonly IEnumerable<Action<IConfigurationBuilder>> configureAppConfiguration = Enumerable.Empty<Action<IConfigurationBuilder>>();
        private readonly IEnumerable<Action<IFunctionsWorkerApplicationBuilder>> configureFunctionsWorkerDefaults = Enumerable.Empty<Action<IFunctionsWorkerApplicationBuilder>>();
        private readonly IEnumerable<Action<IServiceCollection>> configureServices = Enumerable.Empty<Action<IServiceCollection>>();
        private readonly IEnumerable<Action<IRequestExecutorBuilder, bool>> registerQueries = Enumerable.Empty<Action<IRequestExecutorBuilder, bool>>();
        private readonly IEnumerable<Action<IRequestExecutorBuilder, bool>> registerMutations = Enumerable.Empty<Action<IRequestExecutorBuilder, bool>>();
        private readonly IEnumerable<Action<IRequestExecutorBuilder, bool>> registerSubscriptions = Enumerable.Empty<Action<IRequestExecutorBuilder, bool>>();
        private readonly IEnumerable<Action<IHealthChecksBuilder>> registerHealthChecks = Enumerable.Empty<Action<IHealthChecksBuilder>>();

        private HostBuilderInitialiser(HostBuilder hostBuilder)
        {
            this.hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            commandLineArguments = this.hostBuilder.Properties.ContainsKey("args") ? (string[])hostBuilder.Properties["args"] : Array.Empty<string>();

            var assembliesFilter = commandLineArguments.FirstOrDefault(x => RegexCompiled.IsMatch(x, @"^--assemblies:\s*"));
            var filter = assembliesFilter?.Substring(assembliesFilter.IndexOf(':') + 1);
            var startups = DomainStartupDiscovery.Discover(".", filter);

            configureAppConfiguration = startups.Select(x => x.ConfigureAppConfiguration);
            configureFunctionsWorkerDefaults = startups.Select(x => x.ConfigureFunctionsWorkerDefaults);
            configureServices = startups.Select(x => x.ConfigureServices);
            registerQueries = startups.Select(x => x.RegisterQueries);
            registerMutations = startups.Select(x => x.RegisterMutations);
            registerSubscriptions = startups.Select(x => x.RegisterSubscriptions);
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
                //worker.UseMiddleware<ExceptionHandlerMiddleware>();
                worker.UseMiddleware<HttpCompressionMiddleware>();
                worker.UseMiddleware<RequestValidationMiddleware>();
                worker.UseMiddleware<AuthenticationMiddleware>();
                worker.UseMiddleware<AccountLinkingProofRequestMiddleware>();
                worker.UseMiddleware<CachingMiddleware>();

                foreach (var invoke in configureFunctionsWorkerDefaults)
                    invoke(worker);
            })
            .ConfigureOpenApi()
            .ConfigureServices(services =>
            {
                var isSecure = ConfigReader.Get(Common.Constants.Environment.OperationsVisibilityMode).Is("Secure");

                services.AddNativeWebSocketSubscriptions();

                services.AddSha256DocumentHashProvider(HotChocolate.Language.HashFormat.Hex);

                // Primary HC Services
                IRequestExecutorBuilder builder = services.AddGraphQLServer()
                                                          //.AddDiagnosticEventListener<VAAExecutionEventListener>() // Middleware to log Start / End of each query / mutation
                                                          .UseDefaultPipeline()
                                                          //.UsePersistedQueryPipeline()
                                                          //.AddReadOnlyFileSystemQueryStorage("./PersistedQueryStore")
                                                          .AddAuthorization()
                                                          .AddFiltering<VAAFilterConvention>()
                                                          .AddSorting()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalDateType>()
                                                          .AddType<HotChocolate.Types.AnyType>()
                                                          .AddConvention<IFilterConvention>(new FilterConventionExtension(configure =>
                                                          {
                                                              configure.BindRuntimeType<LocalDateTime, ComparableOperationFilterInputType<LocalDateTime>>();
                                                              configure.BindRuntimeType<LocalDateTime?, ComparableOperationFilterInputType<LocalDateTime?>>();
                                                              configure.BindRuntimeType<LocalDate, ComparableOperationFilterInputType<LocalDate>>();
                                                              configure.BindRuntimeType<LocalDate?, ComparableOperationFilterInputType<LocalDate?>>();
                                                              configure.BindRuntimeType<LocalTime, ComparableOperationFilterInputType<LocalTime>>();
                                                              configure.BindRuntimeType<LocalTime?, ComparableOperationFilterInputType<LocalTime?>>();
                                                          }))
                                                          .UseRequest<OperationLimiterMiddleware>()
                                                          .UseTimeout()
                                                          .SetRequestOptions(_ => new HotChocolate.Execution.Options.RequestExecutorOptions { ExecutionTimeout = TimeSpan.FromSeconds(ConfigReader.Try(Common.Constants.Environment.GraphqlTimeOutInSeconds, 30)) })
                                                          .AddErrorFilter<ErrorFilter>()
                                                          .AllowIntrospection(ConfigReader.Try<bool>(Common.Constants.Environment.AllowIntrospection));

                builder.TryAddType<VersionQuery>();

                builder.RegisterDomainSchema(isSecure, registerQueries, registerMutations, registerSubscriptions);

                services.AddLogging()
                        //.AddHttpClient()
                        .AddHttpContextAccessor()
                        .AddApplicationInsightsTelemetryWorkerService()
                        .ConfigureFunctionsApplicationInsights()
                        .Configure<LoggerFilterOptions>(options =>
                        {
                            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs.
                            // Application Insights requires an explicit override. Log levels can also be configured using appsettings.json.
                            // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                            LoggerFilterRule? toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
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
                        .AddSingleton<Gateway, Gateway>()
                        .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
                        .AddSingleton<IApplicationOAuthTokenProvider, OAuthTokenProvider_AAD>()
                        .AddSingleton<IHttpConnector, HttpConnector>()
                        .AddSingleton<IAuthorizationHandler, HasRoleAuthorizationHandler>()
                        .AddSingleton<IAnalyticsService, AnalyticsService>()
                        .AddSingleton<IHttpClientFactory, Common.Http.HttpClientFactory>()
                        .AddSingleton<ITelemetryService, TelemetryService>();

                var healthChecksBuilder = services.AddHealthChecks();

                foreach (var invoke in registerHealthChecks)
                    invoke(healthChecksBuilder);

                foreach (var invoke in configureServices)
                    invoke(services);

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
