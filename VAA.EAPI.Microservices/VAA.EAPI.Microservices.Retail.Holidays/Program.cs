using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.Application;

namespace VAA.EAPI.Microservices.Retail.Holidays
{
    public class Program
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
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                    services.AddSingleton<IRetrieveBookingService, RetrieveBookingService>();
                    services.AddSingleton<IRetrieveDocumentService, RetrieveDocumentService>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
