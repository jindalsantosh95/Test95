using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Application.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Services;

namespace VAA.EAPI.Orchestration.Notifications
{
    public sealed class Program
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
                            services.AddSingleton<IHttpConnector, HttpConnector>();
                            services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                            services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));

                            services.AddSingleton(typeof(IDelayedDispatcherService<FlightDetail>), (x) =>
                            {
                                var azureServiceBusFactory = x.GetService<IMessageBusFactory>();
                                var messageBus = azureServiceBusFactory.GetClient(ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ConfigReader.Get(Constants.Environment.NotificationTopicName));
                                var distributedCache = x.GetService<IDistributedCache>();
                                var eventStoreRequestInsertCommandHandler = x.GetService<EventStoreRequestInsertCommandHandler>();
                                var dispatcherLogger = x.GetService<ILogger<DelayedDispatcherService<FlightDetail>>>();

                                return new DelayedDispatcherService<FlightDetail>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                {
                                    CloudEventSubject = "Orches.Notif.CheckinOpenRequest",
                                    CloudEventType = "Orches.Notif.CheckinOpenRequest",
                                    GenerateId = (item) => item.Id
                                };
                            });
                            services.AddSingleton<ICheckInOpenDelayedDispatcherService, CheckInOpenDelayedDispatcherService>();

                            services.AddSingleton(typeof(IDelayedDispatcherService<SeatSelectionReminderFlightDetail>), (x) =>
                            {
                                var azureServiceBusFactory = x.GetService<IMessageBusFactory>();
                                var messageBus = azureServiceBusFactory.GetClient(ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ConfigReader.Get(Constants.Environment.NotificationTopicName));
                                var distributedCache = x.GetService<IDistributedCache>();
                                var eventStoreRequestInsertCommandHandler = x.GetService<EventStoreRequestInsertCommandHandler>();
                                var dispatcherLogger = x.GetService<ILogger<DelayedDispatcherService<SeatSelectionReminderFlightDetail>>>();

                                return new DelayedDispatcherService<SeatSelectionReminderFlightDetail>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                {
                                    CloudEventSubject = Constants.Constants.SeatSelectionReminderRequestEventSubject,
                                    CloudEventType = Constants.Constants.SeatSelectionReminderRequestEventSubject,
                                    GenerateId = (item) => item.Id
                                };
                            });
                            services.AddSingleton<ISeatSelectionReminderDelayedDispatcherService, SeatSelectionReminderDelayedDispatcherService>();

                            services.AddSingleton(typeof(IDelayedDispatcherService<StarlinkFlightDetail>), (x) =>
                            {
                                var azureServiceBusFactory = x.GetService<IMessageBusFactory>();
                                var messageBus = azureServiceBusFactory.GetClient(ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ConfigReader.Get(Constants.Environment.NotificationTopicName));
                                var distributedCache = x.GetService<IDistributedCache>();
                                var eventStoreRequestInsertCommandHandler = x.GetService<EventStoreRequestInsertCommandHandler>();
                                var dispatcherLogger = x.GetService<ILogger<DelayedDispatcherService<StarlinkFlightDetail>>>();

                                return new DelayedDispatcherService<StarlinkFlightDetail>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                {
                                    CloudEventSubject = Constants.Constants.StarlinkEnabledFlightEventSubject,
                                    CloudEventType = Constants.Constants.StarlinkEnabledFlightEventSubject,
                                    GenerateId = (item) => item.Id
                                };
                            });
                            services.AddSingleton<IStarlinkEnabledFlightDelayedDispatcherService, StarlinkEnabledFlightDelayedDispatcherService>();

                            services.AddSingleton(typeof(IDelayedDispatcherService<AircraftMessage>), (x) =>
                            {
                                var azureServiceBusFactory = x.GetService<IMessageBusFactory>();
                                var messageBus = azureServiceBusFactory.GetClient(ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ConfigReader.Get(Constants.Environment.NotificationTopicName));
                                var distributedCache = x.GetService<IDistributedCache>();
                                var eventStoreRequestInsertCommandHandler = x.GetService<EventStoreRequestInsertCommandHandler>();
                                var dispatcherLogger = x.GetService<ILogger<DelayedDispatcherService<AircraftMessage>>>();

                                return new DelayedDispatcherService<AircraftMessage>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                {
                                    CloudEventSubject = Constants.Constants.AcSwapsFlightDataRequest,
                                    CloudEventType = Constants.Constants.AcSwapsFlightDataRequest,
                                    GenerateId = (item) => item.Id,
                                    Mode = DelayedDispatcherService<AircraftMessage>.InsertMode.LastWriteWins
                                };
                            });
                            services.AddSingleton<IAircraftSwapsDelayedDispatcherService, AircraftSwapsDelayedDispatcherService>();

                            services.AddSingleton<IIropEventCommand, IropEventCommandHandler>();
                            services.AddSingleton<ICheckInOpeningService, CheckInOpeningService>();
                            services.AddSingleton<ISeatSelectionReminderService, SeatSelectionReminderService>();
                            services.AddSingleton<IAcSwapsFlightDataRequestEventCommand, AcSwapsFlightDataRequestEventCommandHandler>();
                            services.AddSingleton<IAircraftSwapsEventCommand, AircraftSwapsEventCommandHandler>();
                            services.AddSingleton<IFlightDataRequestEventCommand, FlightDataRequestEventCommandHandler>();
                            services.AddSingleton<IFlightOrderDataEventCommand, FlightOrderDataEventCommandHandler>();
                            services.AddSingleton<ICustomerProfileDataEventCommand, CustomerProfileDataEventCommandHandler>();
                            services.AddSingleton<IStarlinkNotificationEventCommand, StarlinkNotificationEventCommandHandler>();
                            services.AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>();
                            services.AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>();
                            services.AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>();
                            services.AddSingleton<IServiceBusTopicMessageEventCommandHandler>(serviceProvider => serviceProvider.GetRequiredService<ServiceBusTopicMessageEventCommandHandler>());
                            services.AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>();
                            services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                            services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                            services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                            services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                            services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                        })
                        .Build();

            return host.RunAsync();
        }

        private static bool IsTimerLocked(string connectionString, string functionAppName, string fullFunctionName)
        {
            if (connectionString.IsNullOrWhiteSpace()) return false;
            if (functionAppName.IsNullOrWhiteSpace()) return false;
            if (fullFunctionName.IsNullOrWhiteSpace()) return false;

            var blobClient = (new BlobServiceClient(connectionString)).GetBlobContainerClient("azure-webjobs-hosts")
                                                                      .GetBlobClient($"locks/{functionAppName.ToLower()}/Host.Functions.{fullFunctionName}.Listener");

            if (!blobClient.ExistsAsync().GetAwaiter().GetResult()) return false;

            return blobClient.GetPropertiesAsync().GetAwaiter().GetResult().Value.LeaseState == LeaseState.Leased;
        }
    }
}
