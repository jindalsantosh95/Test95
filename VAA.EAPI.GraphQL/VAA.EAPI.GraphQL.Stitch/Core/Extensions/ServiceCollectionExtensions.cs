using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;

namespace VAA.EAPI.GraphQL.Stitch.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string ApplicationInsightsLoggerProvider = "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider";

        public static IServiceCollection AddDefaultServices(this IServiceCollection services)
        {
            var loggingProvider = ConfigReader.Try(Constants.Environment.LoggingProvider, "AppInsights");
            if (loggingProvider.Equals("Datadog", StringComparison.OrdinalIgnoreCase))
                RemoveApplicationInsightsLoggerProvider(services);
            else if (!loggingProvider.Equals("Both", StringComparison.OrdinalIgnoreCase))
                return services;

            return services.AddDatadogLogging();
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
