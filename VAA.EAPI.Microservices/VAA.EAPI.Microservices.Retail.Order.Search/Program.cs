using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
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
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Services;
using VAA.EAPI.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.Extentions;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search
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
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "OrderCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                                                ConfigReader.Get(Constants.Environment.DatabaseName),
                                                ConfigReader.Get(Constants.Environment.OrdersCollection)
                                            })
                        .AddTypeActivatedCheck<TableStorageHealthCheck>(name: "ReferenceTableStorageCheck",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(TableStorageHealthCheck)},
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey),
                                                new List<string>()
                                                {
                                                    ConfigReader.Get(Constants.Environment.ReferenceDataTableName),
                                                    ConfigReader.Get(Constants.Environment.AmenitiesTableName),
                                                    ConfigReader.Get(Constants.Environment.CabinTypeTableName),
                                                    ConfigReader.Get(Constants.Environment.StatusCodeTableName),
                                                    ConfigReader.Get(Constants.Environment.ProductCodeTableName),
                                                    ConfigReader.Get(Constants.Environment.RFICTableName)
                                                }
                                            });
                    services.AddDbContext<SCVReferenceDBContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVRefDBConnString)));
                    services.AddScoped<ISCVRefDBService, SCVRefDBService>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<ICachedAirport, CachedAirport>();
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton<ICachedRFICData, CachedRFICData>();
                    services.AddSingleton<IProductName, ProductName>();
                    services.AddSingleton<ICachedCabinType, CachedCabinType>();
                    services.AddSingleton<ICachedAmenities, CachedAmenities>();
                    services.AddSingleton<ICachedSegmentStatusCode, CachedSegmentStatusCode>();
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.TicketingProcessV3EndPoint)))));
                    services.AddSingleton(s => new PassengerClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PassengerV4EndPoint)))));
                    services.AddSingleton(typeof(IAir4PassengerV4), typeof(Air4PassengerV4));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IRetrieveAmenities), typeof(RetrieveAmenities));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRRetrieveRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(TicketingRetrieveRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(RetrieveAmenitiesQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
