using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Application.PricingFeed;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                            .ConfigureAppConfiguration(builder =>
                            {
                                builder.AddCommandLine(args);
                                builder.AddEnvironmentConfiguration();
                            })
                            .ConfigureFunctionsWebApplication(worker =>
                            {
                                worker.AddDefaultMiddleware();
                            })
                            .ConfigureOpenApi()
                            .ConfigureServices(services =>
                            {
                                services.AddDefaultServices();
                                services.AddHealthChecks();
                                services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                                services.AddSingleton(typeof(IAzureBlobService), typeof(AzureBlobService));
                                services.AddScoped<IGeneratePricingFeedService, GeneratePricingFeedService>();
                                services.AddScoped<IFederatedDataService, FederatedDataService>();
                                services.AddScoped<ITravelDestinations, TravelDestinations>();
                            })
                            .Build();
            host.Run();
        }
    }
}
