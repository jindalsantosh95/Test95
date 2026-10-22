using Azure.Messaging.ServiceBus;
using MediatR;
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
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Service;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.DuplicateCheck.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer
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
                                ConfigReader.Get(Constants.Environment.TransactionDB),
                                ConfigReader.Get(Constants.Environment.TransactionDBCollection)
                            });
                    services.AddDbContext<CommonReferenceDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVCommonRefDBConnString)));
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<IValidateMediaCodeReferenceDB, ValidateMediaCodeReferenceDB>();
                    services.AddScoped<IValidateReferenceDB, ValidateReferenceDB>();
                    services.AddScoped<IFCNumberGenerationRepository, FCNumberGenerationRepository>();
                    services.AddScoped<IFCNumberGenerationService, FCNumberGenerationService>();
                    services.AddScoped<Application.Query.CustomerSearch.CustomerSearchQueryHandler>();
                    services.AddScoped<Application.Query.Account.CustomerDucomentQueryHandler>();
                    services.AddScoped<Infrastructure.Repositories.CustomerDoc.CustomerRepoUpdateCommandHandler>();
                    services.AddScoped<EventStoreRequestInsertCommandHandler>();
                    services.AddScoped<ServiceBusTopicMessageEventHandler>();
                    services.AddScoped<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<ICustomerRepository, CustomerRepository>();
                    services.AddSingleton<ICustomerSearchRepository, CustomerSearchRepository>();
                    services.AddSingleton<ICustomerProfileRepository, CustomerProfileRepository>();
                    services.AddSingleton<IMemberProfileRepository, MemberProfileRepository>();
                    services.AddSingleton<IMembershipChangeRepository, MembershipChangeRepository>();
                    services.AddSingleton<ICustomerService<CustomersDTO>, CustomerService<CustomersDTO>>();
                    services.AddSingleton<ICustomerService<CustomerSearchDTO>, CustomerService<CustomerSearchDTO>>();
                    services.AddSingleton<ICustomerTransactionService<CustomerProfileDTO>, CustomerTransactionService<CustomerProfileDTO>>();
                    services.AddSingleton<ICustomerTransactionService<MemberProfileDTO>, CustomerTransactionService<MemberProfileDTO>>();
                    services.AddSingleton<ICustomerTransactionService<MembershipChangeDTO>, CustomerTransactionService<MembershipChangeDTO>>();
                    services.AddSingleton<ICustomerTransactionService<FlyingClubMemberDTO>, CustomerTransactionService<FlyingClubMemberDTO>>();
                    services.AddSingleton<IMembershipChangeRepository, MembershipChangeRepository>();
                    services.AddSingleton<IPartnerDocumentRepository, PartnerDocumentRepository>();
                    services.AddSingleton<ICustomerTransactionService<PartnerMembershipDTO>, CustomerTransactionService<PartnerMembershipDTO>>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<ICustomerTransactionService<AwardDocumentDataDTO>, CustomerTransactionService<AwardDocumentDataDTO>>();
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IDeDupRepositoryEnrolment, DeDupValidationRepositoryEnrolment>();
                    services.AddSingleton<IErrorMapper, Mapper.Enrolment.ErrorMapper>();
                    services.AddSingleton<IDeDupValidation, DeDupValidation>();
                    services.AddSingleton<IDeDupValidationEnrolment, DeDupValidationEnrolment>();
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton<IBlobStorageServices>(x => new BlobStorageServices(ConfigReader.Get(Constants.Environment.ConnectionstringBlacklistedEmail), ConfigReader.Get(Constants.Environment.BlacklistedEmaiidcontainerName), ConfigReader.Get(Constants.Environment.BlacklistedEmailsFileName)));
                    services.AddSingleton<ICustomerService<AddressHistoryDTO>, CustomerService<AddressHistoryDTO>>();
                    services.AddSingleton<ICustomerService<ContactHistoryDTO>, CustomerService<ContactHistoryDTO>>();
                    services.AddSingleton(typeof(IGetCustTransDocumentService<>), typeof(GetCustTransDocumentService<>));
                    services.AddSingleton(typeof(ICustomerSearchService), typeof(CustomerSearchService));
                    services.AddSingleton(typeof(ICustomerProfileService), typeof(CustomerProfileService));
                    services.AddSingleton<IUpdateMemberStatusCommand,UpdateMemberStatusCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CustomerCommandHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
