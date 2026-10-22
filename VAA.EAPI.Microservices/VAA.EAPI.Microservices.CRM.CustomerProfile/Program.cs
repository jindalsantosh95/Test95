using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Events;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query.Interface;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Interface;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Services;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile
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
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));                    
                    services.AddSingleton(typeof(ICustomerProfileEventCommand), typeof(CustomerProfileEventCommandHandler));
                    services.AddSingleton(typeof(IAmperityApiRepoQuery), typeof(AmperityApiRepoQueryHandler));
                    services.AddSingleton(typeof(IAmperityHandler), typeof(AmperityHandler));
                    services.AddSingleton(typeof(IAmperityService), typeof(AmperityService));
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
