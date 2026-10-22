using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using VAA.EAPI.Common.Diagnostics.Logging;
using VAA.EAPI.Common.Diagnostics.Telemetry;
using VAA.EAPI.Common.Diagnostics.Logging.Datadog;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDefaultServices(this IServiceCollection source) => source.AddDefaultServices(GetLoggingProvider());

        internal static IServiceCollection AddDefaultServices(this IServiceCollection source, LoggingProvider loggingProvider)
        {
            source.AddLogging();

            if (loggingProvider is LoggingProvider.Both or LoggingProvider.AppInsights)
            {
                source.AddApplicationInsightsTelemetryWorkerService();
                source.ConfigureFunctionsApplicationInsights();
                source.Configure<LoggerFilterOptions>(options =>
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
                });
                source.AddSingleton<ITelemetryInitializer, DefaultTelemetryInitializer>();
                source.AddSingleton<ICustomEventSink, ApplicationInsightsCustomEventSink>();
            }

            source.AddHealthChecks();
            //source.AddHttpClient();
            source.AddMemoryCache();
            source.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });
            source.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
            source.AddSingleton<IHttpClientFactory, Http.HttpClientFactory>();

            if (loggingProvider is LoggingProvider.Both or LoggingProvider.Datadog)
            {
                source.AddDatadogLogging();
                source.AddSingleton<ICustomEventSink, DatadogCustomEventSink>();
            }

            source.AddSingleton<ICustomEventTracker, CustomEventTracker>();

            return source;
        }

        internal static LoggingProvider GetLoggingProvider()
        {
            var configuredValue = ConfigReader.Try(Constants.Environment.LoggingProvider, nameof(LoggingProvider.AppInsights));

            return ParseLoggingProvider(configuredValue);
        }

        internal static LoggingProvider ParseLoggingProvider(string configuredValue) =>
            System.Enum.TryParse(configuredValue, true, out LoggingProvider loggingProvider) && System.Enum.IsDefined(loggingProvider)
                ? loggingProvider
                : LoggingProvider.AppInsights;

        public static IServiceCollection AddDatadogLogging(this IServiceCollection source)
        {
            var options = DatadogLoggingOptions.FromEnvironment();

            source.AddSingleton(options);
            source.AddSingleton<DatadogLogQueue>();
            source.AddSingleton<ILoggerProvider>(provider => new DatadogLoggerProvider(provider.GetRequiredService<DatadogLogQueue>(), provider.GetRequiredService<DatadogLoggingOptions>()));
            source.AddHostedService<DatadogLogSender>();

            return source;
        }
    }
}
