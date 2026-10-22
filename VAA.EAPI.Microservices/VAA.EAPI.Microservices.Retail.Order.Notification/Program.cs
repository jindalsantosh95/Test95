using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Retail.Order.Notification
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
                    worker.UseWhen<AuthenticationMiddleware>(x => !Debugger.IsAttached);
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
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get("ServiceBusConnectionString"), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(ServiceResponseInsertCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
