using Azure.Identity;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
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
using VAA.EAPI.Infrastructure.Client;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;
using VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Caching;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.Interface;

namespace VAA.EAPI.Microservices.Retail.Order
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
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.CustomerOrderEnrollmentServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "OrderCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseEndPoint),
                                                ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseName),
                                                ConfigReader.Get(Constants.Environment.OrdersCollectionName)
                                            });
                    services.AddDbContext<CommonRefDbContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.OneTicketCommonRefDBConnString)));
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddScoped<ICachedAirport, CachedAirport>();
                    services.AddScoped<ICachedCarrier, CachedCarrier>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4TicketingProV3EndPoint)))));
                    services.AddSingleton(s => new SpecialServiceRequestClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SpecialServiceRequestV4EndPoint)))));
                    services.AddSingleton(s => new SegmentClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SegmentV4EndPoint)))));
                    services.AddSingleton<ICachedChannel, CachedChannel>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IAir4TokenGenerator), typeof(Air4TokenGenerator));
                    services.AddSingleton<IValidatePayload, ValidatePayload>();
                    services.AddSingleton<IValidatePayloadAssociateFlights, ValidatePayloadAssociateFlights>();
                    services.AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<IMetaPartnerCommandHandler, MetaPartnerCommandHandler>();
                    services.AddSingleton<IMetaPartnerEventCommandHandler, MetaPartnerEventCommandHandler>();
                    services.AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ICosmosSQLClient, CosmosSQLClient>();
                    services.AddSingleton(new CosmosClient(ConfigReader.Get(Constants.Environment.CosmosSCVDatabaseEndPoint), new DefaultAzureCredential(), new CosmosClientOptions { ConnectionMode = ConfigReader.Get("CosmosSqlDBConnectionMode").Is(nameof(ConnectionMode.Direct)) ? ConnectionMode.Direct : ConnectionMode.Gateway }));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                    });
                    services.AddAzureClients(builder => builder.AddServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
