using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Proxy.CreditCardV2;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.IssueTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.UpsertTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.RetrieveTicket;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket
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
                                            });
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get("ServiceBusConnectionString"), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.PnrCommonV6EndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.TicketingProcessV3EndPoint)))));
                    services.AddSingleton(s => new CreditCardClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.CreditCardV2EndPoint)))));
                    services.AddSingleton(s => new Remark1Client(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.RemarksV3EndPoint)))));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton(typeof(ICreditCardV2), typeof(CreditCardV2));
                    services.AddSingleton(typeof(IAir4RemarksV3), typeof(Air4RemarksV3));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(IssueTicketRepoCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(IssueTicketCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(TicketingPerformRevenueQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRSetupContextRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(AddRemarksCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(VerifyFraudCheckQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(UpsertTicketCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(RetriveTicketQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosModifyRepoCommand));

                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
