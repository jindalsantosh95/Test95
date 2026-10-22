using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.EventGridPublisher;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;
using ServiceResponseQueryHandler = VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services.ServiceResponseQueryHandler;

namespace VAA.EAPI.Microservices.Loyalty.Vred
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
                    services.AddDbContext<TransactionDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.VGLCDBConnString)));
                    services.AddScoped<ITransactionDBRepository, TransactionDBRepository>();
                    services.AddScoped<ITransactionDBServices, TransactionDBServices>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<ITokenHelper, TokenHelper>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<IVredCustomerDetails, UpdateVredCustomerDetailsHandler>();
                    services.AddSingleton<IServiceResponseInsert, ServiceResponseInsertHandler>();
                    services.AddSingleton<IServiceResponseQuery, ServiceResponseQueryHandler>();
                    services.AddSingleton<IVredCustomerDetailsResponseQuery, UpdateVredCustomerDetailsResponseQueryHandler>();
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<UpdateVredRepoHandler>();
                    services.AddSingleton<ServiceResponseInsertCommandHandler>();
                    services.AddScoped<IUpdateTransactionPnrEventHandler, UpdateTransactionPnrEventHandler>();
                    services.AddSingleton<IUpdateVredTransactionServiceHandler, UpdateVredTransactionServiceHandler>();
                    services.AddScoped<VredStatementsRepoHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(VredEnrolmentHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(VirginPointsQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(VirginPointsServiceHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(VirginPointsRepoHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(UpdateVredRepoHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(UpdateVredServiceHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(VredConsentReceiptCommandHandler));

                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
