using Azure.Messaging.ServiceBus;
using FluentValidation;
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
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.BulkMemberUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.EmailUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.UpdatePhoneNumber;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query.CustProfileMS;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management
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
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                            failureStatus: HealthStatus.Unhealthy,
                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.OrderMgtServiceBusConnectionString)
                            });
                  services.AddDbContext<CommonReferenceDataContext>(options => options.UseSqlServer(
                      ConfigReader.Get(Constants.Environment.SCVCommonRefDBConnString),
                      sqlOptions => sqlOptions.EnableRetryOnFailure()));
                    services.AddDbContext<ReportingDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.SCVReportingDBConnString)));
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                  services.AddScoped<IAutoAccrualReferenceValidationService, AutoAccrualReferenceValidationService>();
                    services.AddScoped<IReportingDBServices, ReportingDBServices>();
                    services.AddSingleton<AwardDocumentServiceQueryHandler>();
                    services.AddScoped<IWingsMemberAssessmentCommand, WingsMemberAssessmentCommandHandler>();
                    services.AddScoped<ICachedPartners, CachedPartners>();
                    services.AddScoped<ICachedFlightProduct, CachedFlightProduct>();
                    services.AddScoped<ICachedMemberAccountReferenceData, CachedMemberAccountReferenceData>();
                    services.AddSingleton<ICustomerProfileQuery, CustomerProfileQueryHandler>();
                    services.AddSingleton<IBulkMemberUpdateCommand, BulkMemberUpdateCommandHandler>();
                    services.AddSingleton<IVredCustomerDetailsResponseQuery, UpdateVredCustomerDetailsResponseQueryHandler>();
                    services.AddSingleton<IAddFCNumberToPNRCommand, AddFCNumberToPNRCommandHandler>();
                    services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                    services.AddSingleton<AddFCNumberToPNRResponseQueryHandler>();
                    services.AddSingleton<CustomerSearchDocumentServiceQueryHandler>();
                    services.AddSingleton<CustomerDetailsServiceQueryHandler>();
                    services.AddSingleton<CustomerDocumentServiceQueryHandler>();
                    services.AddSingleton<ICustomerDocumentQuery, CustomerDocumentQuery>();
                    services.AddSingleton<FlyingClubMemberDocumentServiceQueryHandler>();
                    services.AddSingleton<CustomerTransactionDocumentsServiceQueryHandler>();
                    services.AddSingleton<TransactionStatementTextQueryHandler>();
                    services.AddSingleton<HouseHoldAccountEmailTriggerCommandHandler>();
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<AzureStorageCommandHandler>();
                    services.AddSingleton<GroupDocumentServiceQueryHandler>();
                    services.AddSingleton<GroupHistoryDocumentServiceQueryHandler>();
                    services.AddSingleton<MemberAccountDetailsQueryHandler>();
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<IRulesEngineService, RulesEngineService>();
                    services.AddSingleton<ICustomerTransactionService, CustomerTransactionService>();
                    services.AddSingleton<ICustomerTransactionTokenProvider, CustomerTransactionTokenProvider>();
                    services.AddScoped<IMemberAccountDetailsService, MemberAccountDetailsService>();
                    services.AddSingleton<IGroupDetailsService, GroupDetailsServiceHandler>();
                    services.AddSingleton<IValidator<HouseHoldAccountLinkingCommand>, HouseHoldAccountLinkingCommandValidator>();
                    services.AddSingleton<IValidator<HouseHoldAccountDelinkingCommand>, HouseHoldAccountDelinkingCommandValidator>();
                    services.AddSingleton<IHouseholdDetailsRetrievalQuery ,HouseholdDetailsRetrievalQueryHandler>();
                    services.AddSingleton<IHouseHoldAccountLinkingCommand, HouseHoldAccountLinkingCommandHandler>();
                    services.AddSingleton<IEmailChangeNotificationCommand, EmailChangeNotificationEventHandler>();
                    services.AddScoped<IAutoAccrualInboundHandler, AutoAccrualInboundHandler>();
                    services.AddScoped<IAutoAccrualFeedbackPublisher, AutoAccrualFeedbackPublisher>();
                    services.AddSingleton<IUpdatePhoneNumberCommand, UpdatePhoneNumberNotificationHandler>();
                    services.AddSingleton<IValidator<SupplementarySilverDelinkingCommand>, SupplementarySilverDelinkingCommandValidator>();
                    services.AddSingleton<IValidator<SupplementarySilverAccountLinkCommand>, SupplementarySilverAccountLinkCommandValidator>();
                    services.AddSingleton<ICustomerProvider, CustomerService>();
                    services.AddSingleton<IPromotionEnrolledMemberService, PromotionEnrolledMemberService>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ITxnStatementTextService, TxnStatementTextService>();
                    services.AddSingleton<IBlobStorageServices>(x => new BlobStorageServices(ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion), ConfigReader.Get(Constants.Environment.ContainerNameForStatementText), ConfigReader.Get(Constants.Environment.ItemNameForStatementText)));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(MemberAccountDetailsQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetMemberAccountDetailsQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(MemberAccountRequestQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(MemberAccountReferenceDataQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(BirthdayEmailCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetMembersForBirthdayEmailQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(BirthdayBonusCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetMembersForBirthdayBonusQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(GetProductRefrenceDetailsQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(ValidateBookingClassQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(HouseHoldDissolveCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CustomerDetailsServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(AddressUpdateNotificationCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CustomerDocumentServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(AddressChangeEventCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(CustomerSearchDocumentServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(AzureStorageCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(ServiceBusTopicMessageEventCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
