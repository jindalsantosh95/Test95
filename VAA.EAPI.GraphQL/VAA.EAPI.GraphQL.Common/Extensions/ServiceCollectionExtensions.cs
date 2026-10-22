using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string ApplicationInsightsLoggerProvider = "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider";

        public static IServiceCollection AddDefaultServices(this IServiceCollection services)
        {
            var loggingProvider = LoggingProviderResolver.FromEnvironment();
            if (loggingProvider is LoggingProvider.Datadog)
                RemoveApplicationInsightsLoggerProvider(services);

            if (loggingProvider is LoggingProvider.Datadog or LoggingProvider.Both)
                services.AddDatadogLogging();

            services.AddSingleton<ICustomEventTracker, CustomEventTracker>();
            return services;
        }

        private static void RemoveApplicationInsightsLoggerProvider(IServiceCollection services)
        {
            for (var index = services.Count - 1; index >= 0; index--)
            {
                var descriptor = services[index];
                if (descriptor.ServiceType == typeof(ILoggerProvider) &&
                    (descriptor.ImplementationType?.FullName == ApplicationInsightsLoggerProvider ||
                     descriptor.ImplementationInstance?.GetType().FullName == ApplicationInsightsLoggerProvider))
                    services.RemoveAt(index);
            }
        }
    }
}
