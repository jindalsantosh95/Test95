using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment
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
                    services.AddHealthChecks();
                    services.AddHttpClient();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(RetroCreditCommandHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(RetroFeedbackCommandHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(ResetPendingRetroCommandHandler));
                    });
                    services.AddSingleton(s => new FlownDbContext(ConfigReader.Get(Constants.Environment.FlownSQLDBConnString)));
                    services.AddScoped<IFlownDbServices, FlownDBServices>();
                    services.AddScoped<IFlownDbRepository, FlownDBRepository>();
                    services.AddScoped<IRetroApiService, RetroApiServices>();
                    services.AddScoped<IVSRetroFlownDBRepository, VSRetroFlownDBRepository>();
                    services.AddScoped<IVSRetroFlownDBService, VSRetroFlownDBService>();
                    services.AddSingleton<IMVTMessageEventHandler, MVTMessageEventHandler>();
                    services.AddScoped<IMVTStorageEventHandler, MVTStorageEventHandler>();
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                    services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                    services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                    services.AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>>(serviceProvider =>
                        serviceProvider.GetRequiredService<EventStoreRequestInsertCommandHandler>());
                    services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddScoped(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddScoped<ITableStorageInsertService, TableStorageInsertService>();
                    services.AddScoped<ISkyTeamAutoAccrualEventHandler, SkyTeamAutoAccrualEventHandler>();
                    services.AddScoped<ISkyTeamInboundAutoAccrualEventHandler, SkyTeamInboundAutoAccrualEventHandler>();
                    services.AddScoped<IAutoAccrualFeedbackOutboundEventHandler, AutoAccrualFeedbackOutboundEventHandler>();
                    services.AddScoped<ISkyTeamInboundFeedbackProcessingEventHandler, SkyTeamInboundFeedbackProcessingEventHandler>();
                    services.AddScoped<ISkyTeamInboundFeedbackGenerationEventHandler, SkyTeamInboundFeedbackGenerationEventHandler>();
                    services.AddScoped<IAutoAccrualFeedbackGenerationPublisher, AutoAccrualFeedbackGenerationPublisher>();
                    services.AddScoped<ISkyTeamInboundAutoAccrualProcessingEventHandler, SkyTeamInboundAutoAccrualProcessingEventHandler>();
                    services.AddScoped<IFlownPassenger, FlownPassengerCommandHandler>();
                    services.AddScoped<IPgpEncryptionService, PgpEncryptionService>();
                    services.AddScoped<IPgpDecryptionService, PgpDecryptionService>();
                    services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
                    services.AddScoped<IAutoAccrualFallbackHandler, AutoAccrualFallbackHandler>();
                    services.AddScoped<ISkyTeamAutoAccrualKafkaPublisher, SkyTeamAutoAccrualKafkaPublisher>();
                    services.AddScoped<ISkyTeamAutoAccrualFeedbackKafkaPublisher, SkyTeamAutoAccrualFeedbackKafkaPublisher>();
                    services.AddScoped<IPartnerDataService, PartnerDataService>();
                    services.AddScoped<AutoAccrualFeedbackOutboundKafkaMessageHandler>();
                    services.AddScoped<Infrastructure.Repositories.TableStorage.Interface.ITableStorageInsertService, Infrastructure.Repositories.TableStorage.TableStorageInsertService>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
