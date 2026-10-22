using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.PartnerReferenceData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.ProductReferenceData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Repository;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.ReferenceData.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData
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
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddDbContext<ScvReferenceDbContext>(options => options.UseSqlServer(ConfigReader.Get(Environment.SCVCommonRefDbConnString)));
                    services.AddScoped<IScvRefDbService, ScvRefDbService>();
                    services.AddScoped<IScvRefDBRepository, ScvRefDBRepository>();
                    services.AddScoped<ITitleRepository, TitleRepository>();
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddScoped<IProductDataQueryHandler, ProductDataQueryHandler>();
                    services.AddScoped<IPartnerProfileQueryHandler, PartnerProfileQueryHandler>();
                    services.AddScoped<ITitleDataQueryHandler, TitleDataQueryHandler>();
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddScoped<IProductReferenceDataCommandHandler, ProductReferenceDataCommandHandler>();
                    services.AddScoped<IPartnerReferenceDataCommandHandler, PartnerReferenceDataCommandHandler>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
