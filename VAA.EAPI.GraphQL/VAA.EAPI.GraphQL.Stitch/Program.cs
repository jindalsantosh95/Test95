using HotChocolate.AspNetCore;
using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using HotChocolate.Stitching.Pipeline;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Collections.Specialized;
using System.Web;
using VAA.EAPI.GraphQL.Stitch.Core;
using VAA.EAPI.GraphQL.Stitch.Core.Configuration;
using VAA.EAPI.GraphQL.Stitch.Core.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;
using VAA.EAPI.GraphQL.Stitch.Core.Middleware;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.WebSockets;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;

namespace VAA.EAPI.GraphQL.Stitch
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            NameValueCollection endpoints = [];
            var requestExecutorOptions = new RequestExecutorOptions
            {
                ExecutionTimeout = TimeSpan.FromSeconds(ConfigReader.Try(Core.Constants.Environment.GraphqlTimeOutInSeconds, 30)),
                IncludeExceptionDetails = true
            };

            var builder = new HostBuilder().ConfigureAppConfiguration(builder =>
            {
                if (args.Length != 0)
                    builder.AddCommandLine(args);

                builder.AddEnvironmentConfiguration();
            })
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                worker.UseMiddleware<FunctionContextAccessorMiddleware>();
                worker.UseMiddleware<HttpCompressionMiddleware>();
                worker.UseMiddleware<PeristedQueryMiddleware>();
                worker.UseMiddleware<RequestValidationMiddleware>();
            })
            .ConfigureServices(services =>
            {
                services.AddSha256DocumentHashProvider(HotChocolate.Language.HashFormat.Hex)
                        .AddApplicationInsightsTelemetryWorkerService()
                        .AddHealthChecks();

                endpoints = HttpUtility.ParseQueryString(ConfigReader.Try("Graphql:Endpoints", string.Empty));
                services.AddSingleton(new RemoteSubscriptionEndpoints(endpoints));
                services.AddSingleton(_ => new RedisSubscriptionSessionStore(
                    ConfigReader.Get("RedisCacheConnectionString"), ConfigReader.Get("NativeWebSocketsRedisPrefix")));
                services.AddSingleton<NativeSubscriptionRegistry>();
                services.AddHostedService(provider => provider.GetRequiredService<NativeSubscriptionRegistry>());

                foreach (var key in endpoints.AllKeys)
                    services.AddHttpClient(key!, c => c.BaseAddress = new Uri(endpoints[key]!));

                // Primary HC Services
                IRequestExecutorBuilder builder = services.AddGraphQLFunction()
                                                          .UseDefaultPipeline()
                                                          .TryAddTypeInterceptor<RemoteSubscriptionTypeInterceptor>()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
                                                          .AddType<HotChocolate.Types.NodaTime.LocalDateType>()
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
                                                          .UseRequest<TimeoutMiddleware>()
                                                          .SetRequestOptions(_ => requestExecutorOptions)
                                                          .AddErrorFilter<ErrorFilter>()
                                                          .AllowIntrospection(false)
                                                          .AddHttpRequestInterceptor<Core.HttpRequestInterceptor>();

                foreach (var key in endpoints.AllKeys)
                    builder.AddRemoteSchema(key!)
                           .AddGraphQL(key!)
                           .AllowIntrospection(false)
                           .AddHttpRequestInterceptor<Core.HttpRequestInterceptor>()
                           .AddErrorFilter<ErrorFilter>()
                           .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
                           .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
                           .AddType<HotChocolate.Types.NodaTime.LocalDateType>();

                services.AddLogging();

                services.AddHttpClient();
                services.AddHttpContextAccessor();

                services.AddApplicationInsightsTelemetryWorkerService()
                        //.ConfigureFunctionsApplicationInsights()
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
                        .AddSingleton<ITelemetryInitializer, DefaultTelemetryInitializer>()
                        .AddSingleton<TelemetryConfiguration, TelemetryConfiguration>()
                        .AddDefaultServices()
                        .RemoveAll<IHttpStitchingRequestInterceptor>()
                        .AddSingleton<IHttpStitchingRequestInterceptor, Core.HttpStitchingRequestInterceptor>()
                        .RemoveAll<IHttpRequestInterceptor>()
                        .RemoveAll<DefaultHttpRequestInterceptor>()
                        .AddSingleton<IHttpRequestInterceptor, Core.HttpRequestInterceptor>();
            });

            return builder.Build().RunAsync();
        }
    }
}
