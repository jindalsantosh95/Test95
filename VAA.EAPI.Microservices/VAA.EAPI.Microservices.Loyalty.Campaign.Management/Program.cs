using Azure.Messaging.ServiceBus;
using FluentValidation;
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
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management
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
                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                            });
                    services.AddDbContext<CommonReferenceDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVCommonRefDBConnString)));
                    services.AddDbContext<ReportingDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVReportingDBConnString)));
                    services.AddScoped<ICachedCampaignDetailsById, CachedCampaignDetailsById>();
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<IReportingDBServices, ReportingDBServices>();
                    services.AddScoped(typeof(ICampaignRetrieveRepository), typeof(CampaignRetrieveRepository));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(ITargetedMemberDocumentService), typeof(TargetedMemberDocumentService));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IValidator<CampaignPromotionLinkRequestData>, CampaignPromotionLinkRequestValidator>();
                    services.AddSingleton<IValidator<CampaignPromotionDelinkRequestData>, CampaignPromotionDelinkRequestValidator>();
                    services.AddSingleton<IValidator<PromotionUpdateRequestData>, PromotionUpdateRequestValidator>();
                    services.AddSingleton<IValidator<PromotionCampaignSearchRequestData>, PromotionCampaignSearchRequestValidator>();
                    services.AddSingleton<IValidator<CampaignUnassignRequestData>, CampaignUnassignRequestValidator>();
                    services.AddSingleton<IValidator<PromotionCreateRequestData>, PromotionCreateRequestValidator>();
                    services.AddSingleton<ICustomerProvider, CustomerService>();
                    services.AddSingleton<IValidateUniqueIdRepository, ValidateUniqueIdRepository>();
                    services.AddTransient<IValidateUniqueIdHandler, ValidateUniqueIdHandler>();
                    services.AddSingleton<IPromotionListDownloadService, PromotionListDownloadService>();
                    services.AddSingleton<ICampaignListDownloadService, CampaignListDownloadService>();
                    services.AddSingleton<IAssociatedMemberListDownloadService, AssociatedMemberListDownloadService>();
                    services.AddSingleton<ICampaignAmexUpdateRequestCommand, CampaignAmexUpdateRequestCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(MemberCampaignRetrievalQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(PromotionDownloadQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
