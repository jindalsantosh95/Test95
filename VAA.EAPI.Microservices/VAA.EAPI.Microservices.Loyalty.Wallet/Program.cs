using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Service;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet
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
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddHealthChecks()
                          .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                              failureStatus: HealthStatus.Unhealthy,
                              tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                              args: new object[]
                              {
                                  ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                              });
                    services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<ITableStorageService<PassEntities>>(x => new TableStorageService<PassEntities>(ConfigReader.Get(Environment.WalletDBConnectionString), ConfigReader.Get(Environment.RegisterPassTable)));
                    services.AddSingleton<ITableStorageService<PushEntity>>(x => new TableStorageService<PushEntity>(ConfigReader.Get(Environment.WalletDBConnectionString), ConfigReader.Get(Environment.PushPassTable)
                    ));
                    services.AddSingleton<IBoardingPassWalletService, BoardingPassWalletService>();
                    services.AddSingleton<IAirshipWalletService, AirshipWalletService>();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));
                    services.AddSingleton<IDistributedCacheService, DistributedCacheService>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GeneratePassCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(DeletePassCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(RegisterPassCommand));
                    });
                })              
               .Build();

            return host.RunAsync();
        }
    }
}
