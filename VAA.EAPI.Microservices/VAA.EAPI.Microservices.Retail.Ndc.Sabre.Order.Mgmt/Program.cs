using Azure.Messaging.ServiceBus;
using MediatR;
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
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TravelItineraryDivideLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.VoidAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt
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
                           .AddTypeActivatedCheck<TableStorageHealthCheck>(name: "OrderMgmtTableStorageCheck",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(TableStorageHealthCheck) },
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
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton(typeof(ICreateSession), typeof(CreateSession));
                    services.AddSingleton(typeof(IGetReservation), typeof(GetReservation));
                    services.AddSingleton(typeof(IOTACancelLLS), typeof(OTACancelLLS));
                    services.AddSingleton(typeof(IEnhancedAirBook), typeof(EnhancedAirBook));
                    services.AddSingleton(typeof(IEnhancedAirTicket), typeof(EnhancedAirTicket));
                    services.AddSingleton(typeof(IEnhancedEndTransaction), typeof(EnhancedEndTransaction));
                    services.AddSingleton(typeof(IManualPriceQuoteAddInfo), typeof(ManualPriceQuoteAddInfo));
                    services.AddSingleton(typeof(ISabreCommandLLS), typeof(SabreCommandLLS));
                    services.AddSingleton(typeof(ITravelItineraryDivideCommandLLS), typeof(TravelItineraryDivideCommandLLS));
                    services.AddSingleton(typeof(IVoidAirTicket), typeof(VoidAirTicket));
                    services.AddSingleton(typeof(IUpdatePassengerNameRecord), typeof(UpdatePassengerNameRecord));
                    services.AddSingleton(typeof(ISessionClose), typeof(SessionClose));
                    services.AddSingleton(typeof(IIgnoreTransaction), typeof(IgnoreTransaction));
                    services.AddSingleton(typeof(IGetElectronicDocument), typeof(GetElectronicDocument));
                    services.AddSingleton(typeof(IDesignatePrinter), typeof(DesignatePrinter));
                    services.AddSingleton(typeof(IUpdateReservation), typeof(UpdateReservation));
                    services.AddSingleton(typeof(IUpdateReservationCommand), typeof(UpdateReservationCommandHandler));
                    services.AddSingleton(typeof(IEndTransaction), typeof(EndTransaction));
                    services.AddSingleton(typeof(ISplitPnrResponseQuery), typeof(SplitPnrResponseQueryHandler));
                    services.AddSingleton(typeof(ISplitPnrCommand), typeof(SplitPnrCommandHandler));
                    services.AddSingleton(typeof(IUpdateOrderCommand), typeof(UpdateOrderCommandHandler));
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<ServiceResponseQueryHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton(typeof(ISplitPnrEventProcessCommand), typeof(SplitPnrEventProcessCommandHandler));
                    services.AddSingleton(typeof(ISplitPnrRepoCommand), typeof(SplitPnrRepoCommandHandler));
                    services.AddSingleton<CreateSessionCommandHandler>();
                    services.AddSingleton<GetReservationQueryHandler>();
                    services.AddSingleton<SessionCloseCommandHandler>();
                    services.AddSingleton<EnhancedEndTransactionCommandHandler>();
                    services.AddSingleton<EnhancedAirTicketCommandHandler>();
                    services.AddSingleton<SabreCommandLLSCommandHandler>();
                    services.AddSingleton<GetElectronicDocumentQueryHandler>();
                    services.AddSingleton<UpdatePassengerNameRecordCommandHandler>();
                    services.AddSingleton<OTACancelLLSCommandHandler>();
                    services.AddSingleton<ManualPriceQuoteAddInfoCommandHandler>();
                    services.AddSingleton(typeof(ITravelItineraryDivideLLSCommand), typeof(TravelItineraryDivideLLSCommandHandler));
                    services.AddSingleton(typeof(IEndTransactionCommand), typeof(EndTransactionCommandHandler));
                    services.AddSingleton<ServiceResponseInsertCommandHandler>();
                    services.AddSingleton(typeof(IOrderChangeAcceptCancellationDisruptionCommand), typeof(OrderChangeAcceptCancellationDisruptionCommandHandler));
                    services.AddSingleton(typeof(IOrderChangeAcceptTimeChangeDisruptionCommand), typeof(OrderChangeAcceptTimeChangeDisruptionCommandHandler));
                    services.AddSingleton(typeof(ITKTExchange), typeof(TKTExchange));
                    services.AddSingleton(typeof(ITKTExchangeCommand), typeof(TKTExchangeCommandHandler));
                    services.AddSingleton(typeof(IOrderChangeReshopDisruptionCommand), typeof(OrderChangeReshopDisruptionCommandHandler));
                    services.AddSingleton(typeof(IOrderChangeReshopHoldBookingCommand), typeof(OrderChangeReshopHoldBookingCommandHandler));
                    services.AddSingleton<DesignatePrinterCommandHandler>();
                    services.AddSingleton<EnhancedAirBookCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CreateSessionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetReservationQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(OTACancelLLSCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(EnhancedAirBookCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(ManualPriceQuoteAddInfoCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(EnhancedEndTransactionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(EnhancedAirTicketCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(SabreCommandLLSCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(ITravelItineraryDivideLLSCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(VoidAirTicketCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(UpdatePassengerNameRecord));
                        config.RegisterServicesFromAssemblyContaining(typeof(SessionCloseCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(IgnoreTransactionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetElectronicDocumentQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(DesignatePrinterCommand));
                    });
                    services.AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new ManualPriceQuoteAddInfoPortTypeClient(ManualPriceQuoteAddInfoPortTypeClient.EndpointConfiguration.ManualPriceQuoteAddInfoPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new OTA_CancelPortTypeClient(OTA_CancelPortTypeClient.EndpointConfiguration.OTA_CancelPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new EnhancedAirBookPortTypeClient(EnhancedAirBookPortTypeClient.EndpointConfiguration.EnhancedAirBookPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new AirTicketPortTypeClient(AirTicketPortTypeClient.EndpointConfiguration.AirTicketPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new VoidTicketPortTypeClient(VoidTicketPortTypeClient.EndpointConfiguration.VoidTicketPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new EnhancedEndTransactionPortTypeClient(EnhancedEndTransactionPortTypeClient.EndpointConfiguration.EnhancedEndTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SabreCommandPortTypeClient(SabreCommandPortTypeClient.EndpointConfiguration.SabreCommandPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new TravelItineraryDividePortTypeClient(TravelItineraryDividePortTypeClient.EndpointConfiguration.TravelItineraryDividePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new UpdatePassengerNameRecordPortTypeClient(UpdatePassengerNameRecordPortTypeClient.EndpointConfiguration.UpdatePassengerNameRecordPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new IgnoreTransactionPortTypeClient(IgnoreTransactionPortTypeClient.EndpointConfiguration.IgnoreTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetElectronicDocumentPortTypeClient(GetElectronicDocumentPortTypeClient.EndpointConfiguration.GetElectronicDocumentPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new DesignatePrinterPortTypeClient(DesignatePrinterPortTypeClient.EndpointConfiguration.DesignatePrinterPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new UpdateReservationPortTypeClient(UpdateReservationPortTypeClient.EndpointConfiguration.UpdateReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new EndTransactionPortTypeClient(EndTransactionPortTypeClient.EndpointConfiguration.EndTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new ExchangePortTypeClient(ExchangePortTypeClient.EndpointConfiguration.ExchangePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
