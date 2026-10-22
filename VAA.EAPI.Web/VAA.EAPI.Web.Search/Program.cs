using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Diagnostics.Telemetry;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Infrastructure.Interfaces;
using VAA.EAPI.Web.Common.Infrastructure.Services;
using VAA.EAPI.Web.Common.Middleware;
using VAA.EAPI.Web.Search.Models.Loyalty;
using VAA.EAPI.Web.Search.Models.Search.Request;

namespace VAA.EAPI.Web.Search
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseMiddleware<FunctionContextAccessorMiddleware>();
                    worker.UseMiddleware<ExceptionHandlerMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.AddDefaultServices();
                    services.AddMemoryCache();
                    services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
                    services.AddResponseCompression();
                    services.AddHealthChecks();
                    services.AddSingleton<ITelemetryInitializer, DefaultTelemetryInitializer>();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddMediatR(configuarion =>
                    {
                        configuarion.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        configuarion.RegisterServicesFromAssemblyContaining<SelectAndNextRequest>();
                        configuarion.RegisterServicesFromAssemblyContaining<SelectAndSummaryRequest>();
                        configuarion.RegisterServicesFromAssemblyContaining<SearchRequest>();
                        configuarion.RegisterServicesFromAssemblyContaining<GraphQLRequest>();
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
