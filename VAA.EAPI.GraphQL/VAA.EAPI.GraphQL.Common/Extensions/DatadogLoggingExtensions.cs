using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class DatadogLoggingExtensions
    {
        public static IServiceCollection AddDatadogLogging(this IServiceCollection services)
        {
            var options = DatadogLoggingOptions.FromEnvironment();

            services.AddSingleton(options);
            services.AddHostedService<DatadogDeliveryMetricsExporter>();
            if (!options.IsConfigured)
            {
                DatadogDeliveryMetrics.RecordInvalidConfiguration();
                return services;
            }

            services.AddSingleton<DatadogLogQueue>();
            services.AddSingleton<ICustomEventSink, DatadogCustomEventSink>();
            services.AddSingleton<ILoggerProvider>(provider => new DatadogLoggerProvider(
                provider.GetRequiredService<DatadogLogQueue>(),
                provider.GetRequiredService<DatadogLoggingOptions>()));
            services.AddHostedService<DatadogLogSender>();

            return services;
        }
    }
}
