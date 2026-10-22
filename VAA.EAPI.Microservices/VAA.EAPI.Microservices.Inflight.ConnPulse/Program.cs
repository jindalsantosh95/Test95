using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Application;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Services;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse;

public sealed class Program
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
                services.AddDefaultServices();
                services.AddHealthChecks();
                services.AddMemoryCache();
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddSingleton<IHttpConnector, HttpConnector>();
                services.AddSingleton<IHeartbeatService, HeartbeatService>();
                services.AddSingleton<IInflightConnectivityHealthCheckHandler, InflightConnectivityHealthCheckHandler>();
            })
            .Build();

        return host.RunAsync();
    }
}
