using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command.GbgService;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup
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
                    services.AddGbgServices();
                    services.AddHealthChecks()
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                            failureStatus: HealthStatus.Unhealthy,
                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                            });
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(new ServiceBusClient(Environment.GetEnvironmentVariable(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(IAddressLookupService), typeof(AddressLookupService));
                    services.AddSingleton<IAddressLookupV3Service, AddressLookupV3Service>();
                    services.AddSingleton<IAddressLookupV3ReferenceProtector, AddressLookupV3ReferenceProtector>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IAzureWesalApiConnector, AzureWesalApiConnector>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(AddressLookupEventProcessCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }

    }
}
