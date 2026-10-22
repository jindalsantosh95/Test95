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
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Flight.Services.Application.Command;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate.Interfaces;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Caching;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.SpecialService;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Flight.Services.Interface;
using SpecialServiceRequest = VAA.EAPI.Microservices.Flight.Services.Application.Service.SpecialServiceRequest;

namespace VAA.EAPI.Microservices.Flight.Services
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
                            .AddTypeActivatedCheck<RedisHealthCheck>(name: "RedisCacheCheck",
                                failureStatus: HealthStatus.Unhealthy,
                                tags: new[] { nameof(RedisHealthCheck) },
                                args: new object[]
                                {
                                    ConfigReader.Get(Constants.Environment.RedisConnectionString),
                                }
                            );
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));
                    services.AddScoped<ICachedSsrDetails, CachedSsrDetails>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));

                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<ISpecialServiceRequest, SpecialServiceRequest>();
                    services.AddSingleton<IDeltaSSRDetailsService, DeltaSSRDetailsService>();
                    services.AddSingleton<IDeltaSSRDetailsPrivateService, DeltaSSRDetailsPrivateService>();
                    services.AddSingleton<IAir4TokenGenerator, Air4TokenGenerator>();
                    services.AddSingleton<IMealService, MealService>();
                    services.AddSingleton<IMealRepository, MealRepository>();
                    services.AddSingleton<ISpecialServiceRequestV4, SpecialServiceRequestV4>();
                    services.AddSingleton<SpecialServiceRequestClient>();
                    services.AddSingleton<IPnrCommonV6, PnrCommonV6>();
                    services.AddSingleton(s => new SpecialServiceRequestClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SpecialServiceRequestV4EndPoint)))));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));

                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQueryHandler));

                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}