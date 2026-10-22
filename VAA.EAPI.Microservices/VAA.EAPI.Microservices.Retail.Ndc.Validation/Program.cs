using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.OCNEndpoints;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Command;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation
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
                    .AddTypeActivatedCheck<SqlHealthCheck>(name: "NDCConnectSqlDBCheck:SqlDB:SqlDBConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(SqlHealthCheck)},
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.NDCConnectDBConnectionString)
                                        })
                    .AddTypeActivatedCheck<RedisHealthCheck>(name: "RedisCacheCheck",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(RedisHealthCheck) },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.RedisConnectionString)
                                        })
                    .AddTypeActivatedCheck<HttpHealthCheck>(name: "SecurityMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.SecurityMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.SecuritySvcOCPKey)] }
                                            }
                                       });
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));
                    services.AddDbContext<AgencyDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.NDCConnectDBConnectionString)));
                    services.AddDbContext<AgencyOCNDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.NDCConnectDBConnectionString)));
                    services.AddScoped(typeof(IAgencyDataService), typeof(AgencyDataService));
                    services.AddScoped(typeof(IAgencyDataRepository), typeof(AgencyDataRepository));
                    services.AddScoped(typeof(IAgencyOCNDetailsService), typeof(AgencyOCNDetailsService));
                    services.AddScoped(typeof(IAgencyOCNDetailsRepository), typeof(AgencyOCNDetailsRepository));
                    services.AddSingleton(typeof(ISecuritySvc), typeof(SecuritySvc));
                    services.AddSingleton(typeof(IAgentTktAuthSvc), typeof(AgentTktAuthSvc));
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton<IIFGAgentFopLookUp, IFGAgentFopLookUpCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(AgencyValidateQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(AgencyOCNDetailsQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}