using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor
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
                .ConfigureFunctionsWebApplication(worker =>
                {
                    worker.AddDefaultMiddleware();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddHealthChecks()
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "OrderCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.SCVOrderDBEndPoint),
                                                ConfigReader.Get(Constants.Environment.SCVOrderDBName),
                                                ConfigReader.Get(Constants.Environment.SCVOrderDBCollection)
                                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "SingleOrderViewCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                                                ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),
                                                ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection)
                                            });

                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IOrderService<CustomerOrderDetailsDTO>, OrderService<CustomerOrderDetailsDTO>>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(OrderRepoCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
