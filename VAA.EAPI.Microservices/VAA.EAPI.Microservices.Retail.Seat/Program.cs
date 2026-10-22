using Azure.Messaging.ServiceBus;
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
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Caching;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using SeatAvailabilityQuery = VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability.SeatAvailabilityQuery;
using VAA.EAPI.Microservices.Retail.Seat.Application;

namespace VAA.EAPI.Microservices.Retail.Seat
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
                    .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "SeatServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
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
                    .AddTypeActivatedCheck<RedisHealthCheck>(name: "RedisCacheCheck",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(RedisHealthCheck) },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.RedisConnectionString)
                                        });
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(IAir4SeatV8), typeof(Air4SeatV8));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton<ICachedChannel, CachedChannel>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton(s => new SeatClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4SeatV8EndPoint)))));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.PNRCommonV6EndPoint)))));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ISeatService, SeatService>();
                    services.AddSingleton<IEmailService, EmailService>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(SeatAvailabilityQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(SeatV8RetrievePassengerMapQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRSetupContextRepoQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
