using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Interface;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models;

namespace VAA.EAPI.Microservices.FlightInformation
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
                                    failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                    tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Check" },
                                    args: new object[]
                                    {
                                       ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                    });
                   services.AddDbContext<CommonRefDbContext>(options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, ConfigReader.Get(Constants.Environment.OneTicketCommonRefDBConnString)));
                   services.AddScoped<ICommonRefDBService, CommonRefDBService>();
                   services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                   services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                   services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                   services.AddMediatR(config =>
                   {
                       config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                       config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                   });
               })
               .Build();

            return host.RunAsync();
        }
    }
}
