using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
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
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Client;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Client;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine
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
                    worker.UseWhen<AuthorizationMiddleware>(x => !Debugger.IsAttached);
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
                              ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString)
                    });
                    services.AddHttpClient<IInRuleClient, InRuleClient>();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<IDisruptionInRuleService, DisruptionInRuleService>();
                    services.AddSingleton<IDisruptionRulesClient, DisruptionRulesClient>();
                    services.AddDbContext<CommonReferenceDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.EnvironmentSettings.SCVCommonRefDBConnString)));
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<ICachedVSPartner, CachedVSPartner>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IInRuleRepository, InRuleRepository>();
                    services.AddSingleton<IInRuleClient, InRuleClient>();
                    services.AddScoped<IExecuteTierRuleSetCommand, ExecuteTierRuleSetCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
