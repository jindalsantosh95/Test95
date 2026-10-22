using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Orchestration.Notifications.Application.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;
using NotificationEnvironment = VAA.EAPI.Orchestration.Notifications.Constants.Environment;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class StarlinkNotificationEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public StarlinkNotificationEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set(NotificationEnvironment.NotificationServiceBusConnectionString, "starlinkTestConn");
            ConfigReader.Set(NotificationEnvironment.NotificationTopicName, NotificationConstants.NotificationTopicName);
            ConfigReader.Set(NotificationEnvironment.FlightOpsKey, "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("Notifications:StarlinkNotification:CategoryIndex", "7");
            ConfigReader.Set("Notifications:ReferenceMs:Uri", "https://tempuri.org/reference");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<IStarlinkNotificationEventCommand, StarlinkNotificationEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_Should_Publish_Flight_Order_Request_When_Tail_Is_Starlink_Eligible()
        {
            ConfigReader.Set("Notifications:FlightMs:CheckAircraftCategoryUri", "https://tempuri.org/checkaircraftcategory/eligible");

            var handler = _serviceProvider.GetService<IStarlinkNotificationEventCommand>()!;
            var messageBus = GetMessageBus();
            var tableStorageService = GetTableStorageService();
            CloudEvent? message = default;

            messageBus.OnMessage += published => message = published;

            await handler.Handle(new CloudEvent { Data = Json.Serialize(CreateStarlinkFlight("G-VTEA")) }, CancellationToken.None);

            Assert.Equal(1, messageBus.Delivered);
            Assert.Equal(1, tableStorageService.Count);
            Assert.NotNull(message);
            Assert.Equal(NotificationConstants.FlightOrderDataReqEventSubject, message.Type);

            var request = Assert.IsType<FlightOrderDataRequest>(message.Data);

            Assert.Equal(NotificationConstants.NotificationTypeStarlink, request.NotificationType);
            Assert.Equal("317", request.OperatingCarrier.FlightNum);
            Assert.Equal("VS", request.OperatingCarrier.CarrierCode);
            Assert.Equal(new DateTime(2025, 09, 29, 09, 30, 00), request.ScheduledDepartureLocalDateTime);
        }

        [Fact]
        public async Task Handle_Should_NoOp_When_Tail_Is_Not_Starlink_Eligible()
        {
            ConfigReader.Set("Notifications:FlightMs:CheckAircraftCategoryUri", "https://tempuri.org/checkaircraftcategory/ineligible");

            var handler = _serviceProvider.GetService<IStarlinkNotificationEventCommand>()!;
            var messageBus = GetMessageBus();
            var tableStorageService = GetTableStorageService();

            await handler.Handle(new CloudEvent { Data = Json.Serialize(CreateStarlinkFlight("G-VDOT")) }, CancellationToken.None);

            Assert.Equal(0, messageBus.Delivered);
            Assert.Equal(0, tableStorageService.Count);
        }

        private MessageBus GetMessageBus()
        {
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            return (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(NotificationEnvironment.NotificationServiceBusConnectionString), ConfigReader.Get(NotificationEnvironment.NotificationTopicName));
        }

        private Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity> GetTableStorageService()
            => (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;

        private static StarlinkFlightDetail CreateStarlinkFlight(string tailNumber) => new()
        {
            Arrival = "LHR",
            Departure = "ATL",
            Carrier = "VS",
            FlightNo = 317,
            Date = new DateOnly(2025, 09, 29),
            ScheduledTimeOfDeparture = new TimeOnly(14, 30, 00),
            TailNumber = tailNumber
        };
    }
}
