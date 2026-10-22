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

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public class AcSwapsFlightDataRequestEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AcSwapsFlightDataRequestEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("AircraftSwap:FlightMs:OperationalScheduleUri", "https://tempuri.org/OperationalScheduleUri");
            ConfigReader.Set("FlightOpsKey", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set("NotificationServiceBusConnectionString", "testConn");
            ConfigReader.Set("NotificationTopicName", "testTopic");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()

                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))

                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()

                                                      .AddSingleton<IAcSwapsFlightDataRequestEventCommand, AcSwapsFlightDataRequestEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_Should_Not_Process_When_AircraftCode_Unchanged()
        {
            // Arrange
            var flightDataRequestHandler = _serviceProvider.GetService<IAcSwapsFlightDataRequestEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var request = new AircraftMessage
            {
                MessageType = "ASM",
                Timezone = "UTC",
                Category = "EQT",
                FlightNumber = "110",
                FlightDate = new DateTime(2036, 5, 28),
                Route = "LHR/BGI",
                Origin = "LHR",
                Destination = "BGI",
                ReturnInfo = "VS127/29MAY26",
                CabinClass = "J",
                // match the mock response Model_Code: "330" so handler should detect no change and return
                AircraftCode = "330",
                SeatConfig = "J31W48Y185",
                AircraftRegistration = "G-VUFO",
                AdditionalInfo = "LFO28"
            };

            var cloudEvent = new CloudEvent
            {
                Data = Json.Serialize(request)
            };

            // Act
            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            // validate nothing was written or published because aircraft code didn't change
            Assert.Equal(0, tableStorageService.Count);
            Assert.Equal(0, messageBus.Delivered);
            Assert.Null(message);
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            // Arrange
            var flightDataRequestHandler = _serviceProvider.GetService<IAcSwapsFlightDataRequestEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var request = new AircraftMessage
            {
                MessageType = "ASM",
                Timezone = "UTC",
                Category = "EQT",
                FlightNumber = "110",
                FlightDate = new DateTime(2036, 5, 28),
                Route = "LHR/BGI/SVD",
                Origin = "LHR",
                Destination = "SVD",
                ReturnInfo = "VS127/29MAY26",
                CabinClass = "J",
                AircraftCode = "264",
                SeatConfig = "J31W48Y185",
                AircraftRegistration = "G-VUFO",
                AdditionalInfo = "LFO28"
            };

            var cloudEvent = new CloudEvent
            {
                Data = Json.Serialize(request)
            };

            // Act
            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);


            // validate data sent to table storage
            Assert.Equal(1, tableStorageService.Count);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                          .PayLoad
                          .Parse<JObject>();

            var recordData = Json.Deserialize<FlightOrderDataRequest>(record["data"]!.ToString());

            Assert.Equal(recordData.OperatingCarrier.FlightNum, request.FlightNumber.ToString());
            Assert.Equal(recordData.Departure, request.Origin);
            Assert.Equal(recordData.Arrival, request.Destination);
            Assert.Equal(recordData.ScheduledDepartureLocalDateTime.ToString("dd-MMM-yyyy"), request.FlightDate.ToString("dd-MMM-yyyy"));

            // validate data sent to service bus
            Assert.Equal(1, messageBus.Delivered);

            Assert.NotNull(message);

            var messageData = (FlightOrderDataRequest)message.Data;

            Assert.Equal(messageData.OperatingCarrier.FlightNum, request.FlightNumber.ToString());
            Assert.Equal(messageData.Departure, request.Origin);
            Assert.Equal(messageData.Arrival, request.Destination);
            Assert.Equal(messageData.ScheduledDepartureLocalDateTime.ToString("dd-MMM-yyyy"), request.FlightDate.ToString("dd-MMM-yyyy"));
        }
    }
}
