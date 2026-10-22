using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
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
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Repository;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Services;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Repository;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Repository;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Services;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment
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
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                            failureStatus: HealthStatus.Unhealthy,
                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "CustomerCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint),
                                ConfigReader.Get(Constants.Environment.SCVCustDB),
                                ConfigReader.Get(Constants.Environment.SCVCustDBCustColCntnr)
                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "CustomerTransactionsCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                                ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                                ConfigReader.Get(Constants.Environment.SCVCustTxnDBMmbrPrfleContnr)
                            });
                    services.AddDbContext<CommonReferenceDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVCommonRefDBConnString)));
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddSingleton<EnrolmentEventProcessCommand>();
                    services.AddSingleton<IDeDupRepository, DeDupValidationRepository>();
                    services.AddSingleton<IFCNumberGenerationRepository, FCNumberGenerationRepository>();
                    services.AddSingleton<IFCNumberGenerationService, FCNumberGenerationService>();
                    services.AddSingleton<IErrorMapper, Mapper.ErrorMapper>();
                    services.AddSingleton<IValidateReferenceDB, ValidateReferenceDB>();
                    services.AddSingleton<IDeDupValidation, DeDupValidation>();
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(Environment.GetEnvironmentVariable(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IBlobStorageServices>(x => new BlobStorageServices(ConfigReader.Get(Constants.Environment.ConnectionstringBlacklistedEmail), ConfigReader.Get(Constants.Environment.BlacklistedEmaiidcontainerName), ConfigReader.Get(Constants.Environment.BlacklistedEmailsFileName)));
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
