using Aliencube.AdalWrapper;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Security;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Interface;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity
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
                    worker.UseMiddleware<VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services.AccountLinkingAuthorizationMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddStackExchangeRedisCache(options => options.ConfigurationOptions = AccountLinkingRedisConfiguration.Read());
                    services.AddHealthChecks()
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "CustomerServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                            failureStatus: HealthStatus.Unhealthy,
                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                            args: new object[]
                            {
                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                            })
                        .AddCheck<AccountLinkingRedisHealthCheck>(name: "AccountLinkingRedisCheck:AzureRedis:RedisContinuity",
                            failureStatus: HealthStatus.Unhealthy,
                            tags: new[] { nameof(AccountLinkingRedisHealthCheck), "Account linking", "Mandatory" });
                    services.AddSingleton<IAuthenticationContextWrapper>(s => new AuthenticationContextWrapper(ConfigReader.Get(Environment.AuthenticationUrl) + ConfigReader.Get(Environment.B2CTenantId)));
                    services.AddSingleton(typeof(ITokenService<Claim>), typeof(TokenService<Claim>));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(System.Environment.GetEnvironmentVariable(Environment.TicketingProcessV3EndPoint)))));
                    services.AddSingleton<IAuthTokenValidator, AuthTokenValidator>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(typeof(IEntraService), typeof(EntraService));
                    services.AddSingleton<IFlyingClubTokenSigner, KeyVaultFlyingClubTokenSigner>();
                    services.AddSingleton<IFlyingClubTokenValidator, KeyVaultFlyingClubTokenValidator>();
                    services.AddSingleton<IFlyingClubTokenHandler, FlyingClubTokenHandler>();
                    services.AddSingleton<IFlyingClubTokenValidationHandler, FlyingClubTokenValidationHandler>();
                    services.AddSingleton<IConnectionMultiplexer>(_ =>
                        ConnectionMultiplexer.Connect(AccountLinkingRedisConfiguration.Read()));
                    services.AddSingleton<VAA.EAPI.Infrastructure.Caching.ISingleUseDistributedCacheService, VAA.EAPI.Infrastructure.Caching.DistributedCacheService>();
                    services.AddSingleton<IAccountLinkingProofRepository, RedisAccountLinkingProofRepository>();
                    services.AddSingleton<AccountLinkingRedisHealthCheck>(provider =>
                        new AccountLinkingRedisHealthCheck(provider.GetRequiredService<IConnectionMultiplexer>()));
                    services.AddSingleton<IAccountLinkingProofHandler, AccountLinkingProofHandler>();
                    services.AddSingleton<ILinkingTransactionValidator, LinkingTransactionValidator>();
                    services.AddSingleton(TimeProvider.System);
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IStartSignUpHandler), typeof(StartSignUpHandler));                    
                    services.AddSingleton(typeof(ICompleteSignUpHandler), typeof(CompleteSignUpHandler));
                    services.AddSingleton(typeof(IPatchRegisteredUserHandler), typeof(PatchRegisteredUserHandler));
                    services.AddSingleton(typeof(IAccountSignInHandler), typeof(AccountSignInHandler));
                    services.AddSingleton(typeof(IAccountResetHandler), typeof(AccountResetHandler));
                    services.AddSingleton(typeof(IOtpHandler), typeof(OtpHandler));
                    services.AddSingleton(typeof(INewPasswordHandler), typeof(NewPasswordHandler));
                    services.AddSingleton(typeof(IRefreshTokenHandler), typeof(RefreshTokenHandler));
                    services.AddSingleton(typeof(IEntraCustomAuthExtensionsService),typeof(EntraCustomAuthExtensionsService));
                    services.AddSingleton(typeof(IRefreshTokenService), typeof(RefreshTokenService));
                    services.AddSingleton(typeof(IGetAdUserDetailsService), typeof(GetAdUserDetailsService));
                    services.AddSingleton(typeof(IOtpTokenValidationService), typeof(OtpTokenValidationService));
                    services.AddSingleton(typeof(IVerifyMemberStatusRepository), typeof(VerifyMemberStatusRepository));
                    services.AddSingleton(typeof(IGetAdUserByFcNumberRepository), typeof(GetAdUserByFcNumberRepository));
                    services.AddSingleton<RegisterCommandHandler>();
                    services.AddSingleton<RegisterRepoCommandHandler>();
                    services.AddSingleton<RegisterYoungFlyerCommandHandler>();
                    services.AddSingleton<IRegisterYoungFlyerCommandHandler>(s => s.GetRequiredService<RegisterYoungFlyerCommandHandler>());
                    services.AddSingleton(typeof(IUpdateAdUserService), typeof(UpdateAdUserService));
                    services.AddSingleton(typeof(IDeleteB2CAccountService), typeof(DeleteB2CAccountService));
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton<IOtpEmailEventPublisher, OtpEmailEventPublisher>();
                    services.AddSingleton<IOTPGenerateCommandHandler, OTPGenerateCommandHandler>();
                    services.AddSingleton<IOTPGenerateContinuationTokenCommandHandler, OTPGenerateContinuationTokenCommandHandler>();
                    services.AddSingleton(typeof(ITrackAnalyticsEventCommandHandler), typeof(TrackAnalyticsEventCommandHandler));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<IServiceBusTopicMessageEventCommandHandler>(sp => sp.GetRequiredService<ServiceBusTopicMessageEventCommandHandler>());
                    services.AddSingleton<ITableStorageService<GenerateOtpDbEntities>>(x => new TableStorageService<GenerateOtpDbEntities>(ConfigReader.Get(Environment.ReferenceDataMicroServiceDBKey), ConfigReader.Get(Constants.Environment.OTPGenerateTableName)));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(IUpdateAzureB2CProfileCommandHandler), typeof(UpdateAzureB2CProfileCommandHandler));
                    services.AddSingleton(typeof(IUpdateAzureB2CProfileRepoCommandHandler), typeof(UpdateAzureB2CProfileRepoCommandHandler));
                    services.AddSingleton<UpdateAzureB2CProfileCommandHandler>();
                    services.AddSingleton<UpdateAzureB2CProfileRepoCommandHandler>();
                    services.AddSingleton<UnVerifiedMemberValidationServiceHandler>();
                    services.AddSingleton<UnVerifiedMemberValidationServiceHandler>();
                    services.AddSingleton<UpdateB2CServiceCommandHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRRetrieveRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(TicketingRetrieveRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(VerifyMemberCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(RegisterCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(OTPValidateCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(OTPRequestTimerCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
