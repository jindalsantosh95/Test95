using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Services;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class CheckInOpeningServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CheckInOpeningServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            DeleteDispatcherPersistenceFileFor<FlightDetail>();
            DeleteDispatcherPersistenceFileFor<StarlinkFlightDetail>();

            ConfigReader.Set("Notifications:FlightMs:ScheduleUri", "https://tempuri.org/FlightSchedule");
            ConfigReader.Set("Notifications:CheckInOpeningService:MessageDelay", "00:00:00:01");
            ConfigReader.Set("FlightOpsKey", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("MicroServiceDBKey", "tesDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testTable");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton<IDistributedCache, DistributedCache>()
                                                      .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                      .AddSingleton<IDelayedDispatcherService<FlightDetail>>((serviceProvider) =>
                                                      {
                                                          var messageBus = serviceProvider.GetService<IMessageBus>();
                                                          var distributedCache = serviceProvider.GetService<IDistributedCache>();
                                                          var dispatcherLogger = serviceProvider.GetService<ILogger<DelayedDispatcherService<FlightDetail>>>();
                                                          var eventStoreRequestInsertCommandHandler = serviceProvider.GetService<EventStoreRequestInsertCommandHandler>();

                                                          return new DelayedDispatcherService<FlightDetail>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                                          {
                                                              CloudEventSubject = "Orches.Notif.CheckinOpenRequest",
                                                              CloudEventType = "Orches.Notif.CheckinOpenRequest",
                                                              GenerateId = (item) => item.Id,
                                                              UseDefaultDelay = true
                                                          };
                                                      })
                                                      .AddSingleton<ICheckInOpenDelayedDispatcherService, CheckInOpenDelayedDispatcherService>()
                                                      .AddSingleton<IDelayedDispatcherService<StarlinkFlightDetail>>((serviceProvider) =>
                                                      {
                                                          var messageBus = serviceProvider.GetService<IMessageBus>();
                                                          var distributedCache = serviceProvider.GetService<IDistributedCache>();
                                                          var dispatcherLogger = serviceProvider.GetService<ILogger<DelayedDispatcherService<StarlinkFlightDetail>>>();
                                                          var eventStoreRequestInsertCommandHandler = serviceProvider.GetService<EventStoreRequestInsertCommandHandler>();

                                                          return new DelayedDispatcherService<StarlinkFlightDetail>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                                          {
                                                              CloudEventSubject = NotificationConstants.StarlinkEnabledFlightEventSubject,
                                                              CloudEventType = NotificationConstants.StarlinkEnabledFlightEventSubject,
                                                              GenerateId = (item) => item.Id,
                                                              UseDefaultDelay = true
                                                          };
                                                      })
                                                      .AddSingleton<IStarlinkEnabledFlightDelayedDispatcherService, StarlinkEnabledFlightDelayedDispatcherService>()
                                                      .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<ICheckInOpeningService, CheckInOpeningService>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task CheckInOpeningService()
        {
            var checkInOpeningService = _serviceProvider.GetService<ICheckInOpeningService>()!;
            var checkInOpenDelayedDispatcherService = _serviceProvider.GetService<ICheckInOpenDelayedDispatcherService>()!;
            var starlinkDelayedDispatcherService = _serviceProvider.GetService<IStarlinkEnabledFlightDelayedDispatcherService>()!;
            var messageBus = (MessageBus)_serviceProvider.GetService<IMessageBus>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;

            await checkInOpeningService.SynchronizeAsync();

            Assert.Equal(1, checkInOpenDelayedDispatcherService.Count);
            Assert.Equal(1, starlinkDelayedDispatcherService.Count);
            Assert.Equal(0, messageBus.Delivered);

            await WaitUntilAsync(() => checkInOpenDelayedDispatcherService.Count == 0 && starlinkDelayedDispatcherService.Count == 0 && tableStorageService.Count == 2, TestContext.Current.CancellationToken);

            Assert.Equal(0, checkInOpenDelayedDispatcherService.Count);
            Assert.Equal(0, starlinkDelayedDispatcherService.Count);
            Assert.True(messageBus.Delivered >= 1);

            Assert.Equal(2, tableStorageService.Count);

            var payloads = tableStorageService.Items.Select(entry => Json.Deserialize<JObject>(entry.Value.PayLoad)).ToArray();
            var payload = payloads.Single(x => x["type"]!.ToString() == "Orches.Notif.CheckinOpenRequest");

            Assert.Equal("Orches.Notif.CheckinOpenRequest", payload["type"]!.ToString());
            Assert.Equal("104", payload["data"]!["flightNo"]!.ToString());
            Assert.Equal("ATL", payload["data"]!["departure"]!.ToString());
            Assert.Equal("LHR", payload["data"]!["arrival"]!.ToString());

            var starlinkPayload = payloads.Single(x => x["type"]!.ToString() == NotificationConstants.StarlinkEnabledFlightEventSubject);

            Assert.Equal("104", starlinkPayload["data"]!["flightNo"]!.ToString());
            Assert.Equal("G-VDOT", starlinkPayload["data"]!["tailNumber"]!.ToString());
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
    }
}
