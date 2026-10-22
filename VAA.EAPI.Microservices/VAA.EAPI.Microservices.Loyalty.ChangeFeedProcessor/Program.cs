using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Interfaces;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Services;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor
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
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "CustomerCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.SCVCustSearchDBEndPoint),
                                ConfigReader.Get(Constants.Environment.SCVCustSearchDBName),
                                ConfigReader.Get(Constants.Environment.SCVCustSearchDBCollection)
                            });
                   services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                   services.AddSingleton<ICustomerService<CustomerSearchDTO>, CustomerService<CustomerSearchDTO>>();
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