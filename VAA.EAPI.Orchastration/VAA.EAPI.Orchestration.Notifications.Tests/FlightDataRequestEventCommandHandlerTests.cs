using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
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


namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public class FlightDataRequestEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public FlightDataRequestEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set("NotificationServiceBusConnectionString", "testConn");
            ConfigReader.Set("NotificationTopicName", "testTopic");
            ConfigReader.Set("Notifications:ReferenceMs:Uri", "https://tempuri.org/reference");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()

                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))

                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()

                                                      .AddSingleton<IFlightDataRequestEventCommand, FlightDataRequestEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            // Arrange
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightDataRequestEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var flightDataReq = new FlightDetail
            {
                Arrival = "LHR",
                Departure = "BLR",
                Carrier = "VS",
                FlightNo = 1234,
                Date = new DateOnly(2025, 10, 1),
                ScheduledTimeOfDeparture = new TimeOnly(15, 50, 20)
            };

            var cloudEvent = new CloudEvent
            {
                Data = Json.Serialize(flightDataReq)
            };

            // Act
            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);


            // validate data sent to table storage
            Assert.Equal(1, tableStorageService.Count);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                                                                                                                                  .PayLoad
                                                                                                                                  .Parse<JObject>();

            var recordData = Json.Deserialize<FlightOrderDataRequest>(record["data"]!.ToString());

            Assert.Equal(recordData.OperatingCarrier.FlightNum, flightDataReq.FlightNo.ToString());
            Assert.Equal(recordData.OperatingCarrier.CarrierCode, flightDataReq.Carrier);
            Assert.Equal(recordData.ScheduledDepartureUtcDateTime, flightDataReq.Date.ToDateTime(flightDataReq.ScheduledTimeOfDeparture));
            Assert.Equal(recordData.ScheduledDepartureLocalDateTime, "2025-10-01T20:20:20.0000000".Parse<DateTime>());

            // validate data sent to service bus
            Assert.Equal(1, messageBus.Delivered);
            Assert.NotNull(message);

            var messageData = (FlightOrderDataRequest)message.Data;

            Assert.Equal(messageData.OperatingCarrier.FlightNum, flightDataReq.FlightNo.ToString());
            Assert.Equal(messageData.OperatingCarrier.CarrierCode, flightDataReq.Carrier);
            Assert.Equal(messageData.ScheduledDepartureUtcDateTime, flightDataReq.Date.ToDateTime(flightDataReq.ScheduledTimeOfDeparture));
            Assert.Equal(messageData.ScheduledDepartureLocalDateTime, "2025-10-01T20:20:20.0000000".Parse<DateTime>());
        }

        [Fact]
        public async Task Handle_Should_Set_NotificationType_For_SeatSelectionReminder()
        {
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightDataRequestEventCommand>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;
            messageBus.OnMessage += (m) => message = m;

            var flightDataReq = new FlightDetail
            {
                Arrival = "LHR",
                Departure = "BLR",
                Carrier = "VS",
                FlightNo = 1234,
                Date = new DateOnly(2025, 10, 1),
                ScheduledTimeOfDeparture = new TimeOnly(15, 50, 20)
            };

            var cloudEvent = new CloudEvent
            {
                Subject = VAA.EAPI.Orchestration.Notifications.Constants.Constants.SeatSelectionReminderRequestEventSubject,
                Type = VAA.EAPI.Orchestration.Notifications.Constants.Constants.SeatSelectionReminderRequestEventSubject,
                Data = Json.Serialize(flightDataReq)
            };

            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            Assert.NotNull(message);

            var messageData = (FlightOrderDataRequest)message.Data;

            Assert.Equal(VAA.EAPI.Orchestration.Notifications.Constants.Constants.NotificationTypeSeatSelectionReminder, messageData.NotificationType);
        }
    }
}
