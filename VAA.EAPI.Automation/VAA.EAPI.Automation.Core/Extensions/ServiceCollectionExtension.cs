using System.IO.Compression;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Automation.Core.Diagnostics.Telemetry;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDefaultServices(this IServiceCollection source)
        {
            source.AddLogging();
            source.AddApplicationInsightsTelemetryWorkerService();
            source.ConfigureFunctionsApplicationInsights();
            source.AddHealthChecks();
            source.AddHttpClient();
            source.AddMemoryCache();
            source.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });
            source.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
            source.AddSingleton<ITelemetryInitializer, DefaultTelemetryInitializer>();

            return source;
        }
    }
}
