using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order
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
                    services.AddHealthChecks()
                    .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                        })
                    .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderDataServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString)
                                        })
                    .AddTypeActivatedCheck<TableStorageHealthCheck>(name: "OrderTableStorageCheck",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(TableStorageHealthCheck)},
                                        args: new object[]
                                        {
                                            ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey),
                                            new List<string>()
                                            {
                                                ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceEventStoreTableName),
                                                ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceResponseTableName)
                                            }
                                        });
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(ICreateSession), typeof(CreateSession));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets })); 
                    services.AddSingleton(typeof(ICreatePNR), typeof(CreatePNR));
                    services.AddSingleton(typeof(ISessionClose), typeof(SessionClose));
                    services.AddSingleton(typeof(IGetReservation), typeof(GetReservation));
                    services.AddSingleton(typeof(IManualPriceQuoteAddInfo), typeof(ManualPriceQuoteAddInfo));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<OrderCreateRepoCommandHandler>();
                    services.AddSingleton<ServiceResponseInsertCommandHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CreateSessionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CreatePNRCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(ManualPriceQuoteAddInfoCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetReservation));
                    });
                    services.AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new CreatePassengerNameRecordPortTypeClient(CreatePassengerNameRecordPortTypeClient.EndpointConfiguration.CreatePassengerNameRecordPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new ManualPriceQuoteAddInfoPortTypeClient(ManualPriceQuoteAddInfoPortTypeClient.EndpointConfiguration.ManualPriceQuoteAddInfoPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
