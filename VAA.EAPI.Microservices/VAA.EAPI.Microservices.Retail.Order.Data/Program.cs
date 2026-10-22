using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCNameCorrectionUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.BookingDetails;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Service;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderList;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.PassengerRetrieval;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.NDCOrderList;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.NDCOrderList.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Data
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
                    services.AddHealthChecks()
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "SORServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "NotificationServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "OrderCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                                                ConfigReader.Get(Constants.Environment.DatabaseName),
                                                ConfigReader.Get(Constants.Environment.OrderCollection)
                                            });
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get("NotificationServiceBusConnectionString"), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get("OrderServiceBusConnectionString"), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get("ServiceBusConnectionString"), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(INameUpdateCommand), typeof(NameUpdateCommandHandler));
                    services.AddSingleton(typeof(INDCOrderHistoryUpdateCommand), typeof(NDCOrderHistoryUpdateCommandHandler));
                    services.AddSingleton<IArcDocumentRepoQuery, ArcDocumentRepoQueryHandler>();
                    services.AddSingleton<IArcDocumentUpdateCommand, ArcDocumentUpdateCommandHandler>();
                    services.AddSingleton<IARCDocumentQuery, ARCDocumentQueryHandler>();
                    services.AddSingleton(typeof(IOrderListQuery), typeof(OrderListQueryHandler));
                    services.AddSingleton(typeof(IOrderListRepoQuery), typeof(OrderListRepoQueryHandler));
                    services.AddSingleton<IFlightOrderNotificationDetails, FlightOrderNotificationDetails>();
                    services.AddSingleton<IFlightOrderAssociationService, FlightOrderAssociationService>();
                    services.AddSingleton<ITripDetailsService, TripService>();
                    services.AddSingleton<OrderDocumentRepoQueryHandler>();
                    services.AddSingleton<CosmosRepoCommandHandler>();
                    services.AddSingleton<CosmosReaderRepoQueryHandler>();
                    services.AddSingleton<CosmosReaderServiceQueryHandler>();
                    services.AddSingleton<ISvcOrderRepository, SvcOrderRepository>();
                    services.AddSingleton<ISvcOrderQuery, SvcOrderQueryHandler>();
                    services.AddSingleton<IScvBookingQuery, ScvBookingQueryHandler>();
                    services.AddSingleton<IScvBookingRepository, SvcOrderRepository>();
                    services.AddScoped<IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>>, OrdersRepoQueryHandler>();
                    services.AddScoped<EventStoreRequestInsertCommandHandler>();
                    services.AddScoped<EventStoreRequestDeleteCommandHandler>();
                    services.AddScoped<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<IPassengerRetrieval, PassengerRetrieval>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
