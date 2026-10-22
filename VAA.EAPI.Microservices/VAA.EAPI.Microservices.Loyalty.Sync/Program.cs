using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.PointsCurrencyConvertServiceRef;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using VAA.EAPI.Common.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Loyalty.Sync
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
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddHealthChecks()
                          .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                              failureStatus: HealthStatus.Unhealthy,
                              tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                              args: new object[]
                              {
                                  ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                              });
                    services.AddSingleton(typeof(IAir4PassengerV4), typeof(Air4PassengerV4));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(s => new LoyaltyProfileSyncClient(WcfCustomBinding.Binding(false), new EndpointAddress(new Uri(Environment.GetEnvironmentVariable(Constants.Environment.DeltaLoyaltyProfileSyncEndPoint)))));
                    services.AddSingleton(s => new PassengerClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(Environment.GetEnvironmentVariable(Constants.Environment.Air4PassengerV4EndPoint)))));
                    services.AddSingleton(s=> new CurrencyConvertClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(Environment.GetEnvironmentVariable(Constants.Environment.DeltaCurrencyConvertServiceEndpoint)))));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(LoyaltySyncCommandHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(PSDSSyncRepoCommand));
                    });
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                })
                 .Build();

            return host.RunAsync();
        }
    }
}
