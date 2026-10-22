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
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateToken;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch
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
                                           tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                           args: new object[]
                                           {
                                                ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString)
                                           });
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IGetReservation), typeof(GetReservation));
                    services.AddSingleton(typeof(IGetElectronicDocument), typeof(GetElectronicDocument));
                    services.AddSingleton(typeof(ISessionClose), typeof(SessionClose));
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(ICreateSession), typeof(CreateSession));
                    services.AddSingleton(typeof(ICreateToken), typeof(CreateToken));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton(typeof(ICreateTokenCommand), typeof(CreateTokenCommandHandler));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.TicketingProcessV3EndPoint)))));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(typeof(IOrderHistoryCommand), typeof(OrderHistoryCommandHandler));
                    services.AddSingleton<OrderSearchRepoCommandHandler>();
                    services.AddSingleton<PNRRetrieveQueryHandler>();
                    services.AddSingleton<TicketingRetrieveRepoHandler>();
                    services.AddSingleton<IssueTicketRepoCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(SessionCloseCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetReservationQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetElectronicDocumentQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(TicketingRetrieveRepoQuery));
                    });
                    services.AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetElectronicDocumentPortTypeClient(GetElectronicDocumentPortTypeClient.EndpointConfiguration.GetElectronicDocumentPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new TokenCreatePortTypeClient(TokenCreatePortTypeClient.EndpointConfiguration.TokenCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
