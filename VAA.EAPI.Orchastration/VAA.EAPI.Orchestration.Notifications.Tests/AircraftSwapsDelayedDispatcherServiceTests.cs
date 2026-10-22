using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Services;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class AircraftSwapsDelayedDispatcherServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public AircraftSwapsDelayedDispatcherServiceTests()
        {
            DeleteDispatcherPersistenceFileFor<AircraftMessage>();

            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testTable");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IDistributedCache, DistributedCache>()
                                                      .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                      .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IDelayedDispatcherService<AircraftMessage>>((serviceProvider) =>
                                                      {
                                                          var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
                                                          var distributedCache = serviceProvider.GetRequiredService<IDistributedCache>();
                                                          var dispatcherLogger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DelayedDispatcherService<AircraftMessage>>>();
                                                          var eventStoreRequestInsertCommandHandler = serviceProvider.GetRequiredService<EventStoreRequestInsertCommandHandler>();

                                                          return new DelayedDispatcherService<AircraftMessage>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                                          {
                                                              CloudEventSubject = NotificationConstants.AcSwapsFlightDataRequest,
                                                              CloudEventType = NotificationConstants.AcSwapsFlightDataRequest,
                                                              GenerateId = (item) => item.Id,
                                                              Mode = DelayedDispatcherService<AircraftMessage>.InsertMode.LastWriteWins
                                                          };
                                                      })
                                                      .AddSingleton<IAircraftSwapsDelayedDispatcherService, AircraftSwapsDelayedDispatcherService>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Schedule_Should_Use_Last_Write_Wins_For_Same_Aircraft_Message_Id()
        {
            var dispatcher = _serviceProvider.GetRequiredService<IAircraftSwapsDelayedDispatcherService>();
            var messageBus = (MessageBus)_serviceProvider.GetRequiredService<IMessageBus>();
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetRequiredService<ITableStorageService<EventStoreDatabaseEntity>>();

            var flightDate = DateTime.UtcNow.Date.AddDays(30);
            var flightNumber = $"LW{DateTime.UtcNow:HHmmssfff}";

            var firstWrite = CreateAircraftMessage(flightNumber, flightDate, "G-OLD01");
            var lastWrite = CreateAircraftMessage(flightNumber, flightDate, "G-NEW99");

            Assert.True(dispatcher.Schedule(firstWrite, TimeSpan.FromMilliseconds(600)));
            Assert.True(dispatcher.Schedule(lastWrite, TimeSpan.FromMilliseconds(600)));
            Assert.Equal(1, dispatcher.Count);

            await WaitUntilAsync(() => dispatcher.Count == 0 && tableStorageService.Count == 1, TestContext.Current.CancellationToken);

            Assert.Equal(1, tableStorageService.Count);
            Assert.Equal(1, messageBus.Delivered);

            var storedPayload = tableStorageService.Items.Single().Value.PayLoad;
            Assert.Contains(lastWrite.AircraftRegistration, storedPayload);
            Assert.DoesNotContain(firstWrite.AircraftRegistration, storedPayload);
        }

        [Fact]
        public async Task Schedule_Should_Not_Replace_When_Existing_Item_Is_Processing()
        {
            DeleteDispatcherPersistenceFileFor<AircraftMessage>();

            var tableStorageService = new Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>();
            var distributedCache = new DistributedCache();
            var messageBus = new BlockingMessageBus();
            var eventStoreRequestInsertCommandHandler = new EventStoreRequestInsertCommandHandler(tableStorageService, NullLogger<EventStoreRequestInsertCommandHandler>.Instance);

            using var dispatcher = new DelayedDispatcherService<AircraftMessage>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, NullLogger<DelayedDispatcherService<AircraftMessage>>.Instance)
            {
                CloudEventSubject = NotificationConstants.AcSwapsFlightDataRequest,
                CloudEventType = NotificationConstants.AcSwapsFlightDataRequest,
                GenerateId = (item) => item.Id,
                Mode = DelayedDispatcherService<AircraftMessage>.InsertMode.LastWriteWins
            };

            var flightDate = DateTime.UtcNow.Date.AddDays(45);
            var flightNumber = $"INFLIGHT{DateTime.UtcNow:HHmmssfff}";
            var firstWrite = CreateAircraftMessage(flightNumber, flightDate, "G-BLOCK1");
            var secondWrite = CreateAircraftMessage(flightNumber, flightDate, "G-BLOCK2");

            Assert.True(dispatcher.Schedule(firstWrite, TimeSpan.FromMilliseconds(30)));

            await messageBus.WaitUntilPublishStartedAsync(TestContext.Current.CancellationToken);

            var replaced = dispatcher.Schedule(secondWrite, TimeSpan.FromMilliseconds(30));

            Assert.False(replaced);
            Assert.Equal(1, dispatcher.Count);

            messageBus.Release();

            await WaitUntilAsync(() => dispatcher.Count == 0 && tableStorageService.Count == 1, TestContext.Current.CancellationToken);

            Assert.Equal(1, messageBus.Delivered);
            Assert.Equal(1, tableStorageService.Count);
        }

        private static AircraftMessage CreateAircraftMessage(string flightNumber, DateTime flightDate, string registration)
        {
            return new AircraftMessage
            {
                MessageType = "CHG",
                Timezone = "UTC",
                Category = "EQT",
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                Route = "ATL/LHR",
                Origin = "ATL",
                Destination = "LHR",
                CabinClass = "J",
                AircraftCode = "300",
                SeatConfig = "J31W48Y185",
                AircraftRegistration = registration
            };
        }

        private static async Task WaitUntilAsync(Func<bool> condition, CancellationToken cancellationToken)
        {
            var timeout = DateTime.UtcNow.AddSeconds(10);

            while (!condition() && DateTime.UtcNow < timeout)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        private static void DeleteDispatcherPersistenceFileFor<T>()
        {
            var serviceName = $"{AppDomain.CurrentDomain.FriendlyName}.DelayedDispatcherService[{typeof(T).FullName}]";

            if (System.IO.File.Exists(serviceName))
                System.IO.File.Delete(serviceName);
        }

        private sealed class BlockingMessageBus : IMessageBus
        {
            private readonly TaskCompletionSource<bool> _publishStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _delivered;

            public int Delivered => Volatile.Read(ref _delivered);

            public async Task PublishMessageAsync(CloudEvent @event)
            {
                Interlocked.Increment(ref _delivered);
                _publishStarted.TrySetResult(true);
                await _release.Task.ConfigureAwait(false);
            }

            public Task WaitUntilPublishStartedAsync(CancellationToken cancellationToken) => _publishStarted.Task.WaitAsync(cancellationToken);

            public void Release() => _release.TrySetResult(true);
        }
    }
}
