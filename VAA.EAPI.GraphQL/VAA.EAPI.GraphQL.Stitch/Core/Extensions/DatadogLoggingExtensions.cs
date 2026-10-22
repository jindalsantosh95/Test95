using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Stitch.Core.Diagnostics.Logging.Datadog;

namespace VAA.EAPI.GraphQL.Stitch.Core.Extensions
{
    public static class DatadogLoggingExtensions
    {
        public static IServiceCollection AddDatadogLogging(this IServiceCollection services)
        {
            var options = DatadogLoggingOptions.FromEnvironment();

            services.AddSingleton(options);
            services.AddSingleton<DatadogLogQueue>();
            services.AddSingleton<ILoggerProvider>(provider => new DatadogLoggerProvider(
                provider.GetRequiredService<DatadogLogQueue>(),
                provider.GetRequiredService<DatadogLoggingOptions>()));
            services.AddHostedService<DatadogLogSender>();

            return services;
        }
    }
}
