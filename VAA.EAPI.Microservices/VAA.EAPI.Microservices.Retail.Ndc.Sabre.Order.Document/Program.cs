using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document
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
                    .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderDataServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: [nameof(ServiceBusHealthCheck), "Service Bus Api"],
                                        args:
                                        [
                                            ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString)
                                        ]);
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(ICreateSession), typeof(CreateSession));                  
                    services.AddSingleton(typeof(IManualPriceQuoteAddInfo), typeof(ManualPriceQuoteAddInfo));
                    services.AddSingleton(typeof(IGetReservation), typeof(GetReservation));
                    services.AddSingleton(typeof(IEnhancedEndTransaction), typeof(EnhancedEndTransaction));
                    services.AddSingleton(typeof(IEnhancedAirTicket), typeof(EnhancedAirTicket));
                    services.AddSingleton(typeof(IGetElectronicDocument), typeof(GetElectronicDocument));
                    services.AddSingleton(typeof(ISessionClose), typeof(SessionClose));

                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CreateSessionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetReservation));
                        config.RegisterServicesFromAssemblyContaining(typeof(ManualPriceQuoteAddInfoCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(EnhancedEndTransactionCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(EnhancedAirTicketCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetElectronicDocumentQuery));
                    });

                    services.AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new ManualPriceQuoteAddInfoPortTypeClient(ManualPriceQuoteAddInfoPortTypeClient.EndpointConfiguration.ManualPriceQuoteAddInfoPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new EnhancedEndTransactionPortTypeClient(EnhancedEndTransactionPortTypeClient.EndpointConfiguration.EnhancedEndTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new AirTicketPortTypeClient(AirTicketPortTypeClient.EndpointConfiguration.AirTicketPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetElectronicDocumentPortTypeClient(GetElectronicDocumentPortTypeClient.EndpointConfiguration.GetElectronicDocumentPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
