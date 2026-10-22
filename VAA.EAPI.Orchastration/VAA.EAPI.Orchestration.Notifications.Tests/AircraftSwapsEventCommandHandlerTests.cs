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
using VAA.EAPI.Orchestration.Notifications.Application.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Services;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class AircraftSwapsEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AircraftSwapsEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            DeleteDispatcherPersistenceFileFor<AircraftMessage>();

            ConfigReader.Set("AircraftChangeNotifDelay", "00:00:00");
            ConfigReader.Set("MicroServiceDBKey", "tesDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testTable");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton<IDistributedCache, DistributedCache>()
                                                      .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                      .AddSingleton<IDelayedDispatcherService<AircraftMessage>>((serviceProvider) =>
                                                      {
                                                          var messageBus = serviceProvider.GetService<IMessageBus>();
                                                          var distributedCache = serviceProvider.GetService<IDistributedCache>();
                                                          var dispatcherLogger = serviceProvider.GetService<ILogger<DelayedDispatcherService<AircraftMessage>>>();
                                                          var eventStoreRequestInsertCommandHandler = serviceProvider.GetService<EventStoreRequestInsertCommandHandler>();

                                                          return new DelayedDispatcherService<AircraftMessage>(messageBus, distributedCache, eventStoreRequestInsertCommandHandler, dispatcherLogger)
                                                          {
                                                              CloudEventSubject = Constants.Constants.AcSwapsFlightDataRequest,
                                                              CloudEventType = Constants.Constants.AcSwapsFlightDataRequest,
                                                              GenerateId = (item) => item.Id,
                                                              UseDefaultDelay = true,
                                                              Mode = DelayedDispatcherService<AircraftMessage>.InsertMode.LastWriteWins
                                                          };
                                                      })
                                                      .AddSingleton<IAircraftSwapsDelayedDispatcherService, AircraftSwapsDelayedDispatcherService>()
                                                      .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IAircraftSwapsEventCommand, AircraftSwapsEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            var aircraftSwapsCommand = _serviceProvider.GetService<IAircraftSwapsEventCommand>()!;
            var aircraftSwapsDelayedDispatcherService = _serviceProvider.GetService<IAircraftSwapsDelayedDispatcherService>()!;
            var messageBus = (MessageBus)_serviceProvider.GetService<IMessageBus>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var request = new VAA.EAPI.Domain.Events.CloudEvent
            {               
                Data = "ASM\nUTC\nEQT\nVS110/28MAY36 ATL/MAN 6/VS127/29\nJ 333 J31W48Y185.J31W48Y185 G-VUFO\nSI LFO28"
            };

            await aircraftSwapsCommand.Handle(request, TestContext.Current.CancellationToken);

            Assert.Equal(1, aircraftSwapsDelayedDispatcherService.Count);
            Assert.Equal(0, messageBus.Delivered);

            await WaitUntilAsync(() => aircraftSwapsDelayedDispatcherService.Count == 0 && tableStorageService.Count == 1, TestContext.Current.CancellationToken);

            Assert.Equal(0, aircraftSwapsDelayedDispatcherService.Count);
            Assert.True(messageBus.Delivered == 1);

            Assert.Equal(1, tableStorageService.Count);

            var payloads = tableStorageService.Items.Select(entry => Json.Deserialize<JObject>(entry.Value.PayLoad)).ToArray();
            var payload = payloads.Single(x => x["type"]!.ToString() == Constants.Constants.AcSwapsFlightDataRequest);

            Assert.Equal(Constants.Constants.AcSwapsFlightDataRequest, payload["type"]!.ToString());
            Assert.Equal("110", payload["data"]!["flightNumber"]!.ToString());
            Assert.Equal("333", payload["data"]!["aircraftCode"]!.ToString());
            Assert.Equal("ATL", payload["data"]!["origin"]!.ToString());

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
