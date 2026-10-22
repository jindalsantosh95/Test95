using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
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
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions
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
                            })
                        .AddTypeActivatedCheck<CosmosHealthCheck>(name: "CustomerTransactionsCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                            tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBEndPoint),
                                ConfigReader.Get(Constants.EnvironmentSettings.TransactionDB),
                                ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBCollection)
                            });
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton<IMemberCustomAttributesCreateEventHandler, MemberCustomAttributesCreateEventHandler>();
                    services.AddSingleton<IMemberCustomAttributesPatchEventHandler, MemberCustomAttributesPatchEventHandler>();
                    services.AddSingleton<IMemberCustomAttributesCreateCommandHandler, MemberCustomAttributesCreateCommandHandler>();
                    services.AddSingleton<IMemberCustomAttributesPatchCommandHandler, MemberCustomAttributesPatchCommandHandler>();
                    services.AddSingleton<IMemberCustomAttributesQueryHandler, MemberCustomAttributesQueryHandler>();
                    services.AddSingleton<IMemberCustomAttributesStatusQueryHandler, MemberCustomAttributesStatusQueryHandler>();
                    services.AddSingleton<IProductDataEventHandler, ProductDataEventHandler>();
                    services.AddSingleton<ICustomerProfileUpdatePreferencesCommand, CustomerProfileUpdatePreferencesCommandHandler>();
                    services.AddSingleton<CustomerProfileQueryHandler>();
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<CosmosModifyServiceCommandHandler>();
                    services.AddSingleton<IVouchersQueryHandler, VouchersQueryHandler>();
                    services.AddSingleton<AwardsQueryHandler>();
                    services.AddSingleton<FlyingClubMemberQueryHandler>();
                    services.AddSingleton<TransactionDocumentQueryHandler>();
                    services.AddMemoryCache();
                    services.AddSingleton<ReferenceCacheHelper>();
                    services.AddSingleton<IReferenceService, ReferenceService>();
                    services.AddSingleton<IRedeemVoucherCommandHandler, RedeemVoucherCommandHandler>();
                    services.AddSingleton<IRedeemVoucherEventHandler, RedeemVoucherEventHandler>();
                    services.AddSingleton<ServiceBusTopicMessageEventHandler>();
                    services.AddSingleton<TransactionCreateEventHandler>();
                    services.AddSingleton<ITransactionStatementTextService, TransactionStatementTextService>();
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddSingleton<TransactionCommandStatusQueryHandler>();
                    services.AddSingleton<IPartnerDataEventHandler, PartnerDataEventHandler>();
                    services.AddSingleton<IVoucherRelatedTransactionQueryHandler, VoucherRelatedTransactionQueryHandler>();
                    services.AddSingleton<IRollbackVoucherRdmCommandHandler, RollbackVoucherRdmCommandHandler>();
                    services.AddSingleton<IRollbackVoucherRdmEventHandler, RollbackVoucherRdmEventHandler>();
                    services.AddSingleton<ServiceResponseInsertCommandHandler>();
                    services.AddSingleton<TransactionCancelEventHandler>();
                    services.AddSingleton<IPatchVoucherRelatedTxnsPnrCommandHandler, PatchVoucherRelatedTxnsPnrCommandHandler>();
                    services.AddSingleton<IPatchVoucherRelatedTxnsPnrEventHandler, PatchVoucherRelatedTxnsPnrEventHandler>();
                    services.AddSingleton<TransactionUpdateEventHandler>();
                    services.AddSingleton<IVoucherTxnsPnrUpdateCommandStatusQuery, VoucherTxnsPnrUpdateCommandStatusQueryHandler>();
                    services.AddSingleton<ServiceResponseQueryHandler>();
                    services.AddSingleton<ICustomerTransactionDataProvider, CustomerTransactionDataProvider>();
                    services.AddSingleton<IVoucherMonitoringService, VoucherMonitoringService>();
                    services.AddSingleton<IVoucherMonitoringHandler, VoucherMonitoringHandler>();
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
