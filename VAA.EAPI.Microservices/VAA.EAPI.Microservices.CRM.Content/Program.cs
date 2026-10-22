using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using Environment = VAA.EAPI.Microservices.CRM.Content.Constants.Environment;

namespace VAA.EAPI.Microservices.CRM.Content
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
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddSingleton<Application.SalesBannersCommandHandler, Application.SalesBannersCommandHandler>();
                    services.AddSingleton<Application.RefreshSalesBannersCommandHandler, Application.RefreshSalesBannersCommandHandler>();
                    services.AddSingleton<IContentRequestHandler, ContentRequestHandler>();
                    services.AddScoped<IMessageBus, ServiceBusHelper>();
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ContentServiceBusConnection), new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpWebSockets
                    }));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
