using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
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
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Repository;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;

namespace VAA.EAPI.Microservices.CRM.Email
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
                                                    ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                                    });
                    services.AddHttpClient<ICcpClient, CcpClient>();
                    services.AddHttpClient<IWorkflowClient, WorkflowClient>();
                    services.AddSingleton<ICcpRepository, CcpRepository>();
                    services.AddSingleton<ICcpClient, CcpClient>();
                    services.AddSingleton<ITokenHelper, TokenHelper>();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton<ICommunicationRepository, CommunicationRepository>();
                    services.AddSingleton<IWorkflowClient, WorkflowClient>();
                    services.AddSingleton<IACMClient, ACMClient>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>();
                    services.AddScoped(typeof(OrderEmailComandHandler));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.PNRCommonV6EndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.TicketingProcessV3EndPoint)))));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRSetupContextRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(TicketingPerformRevenueQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
