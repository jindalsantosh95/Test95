using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
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
using Notification = VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public class FlightOrderDataEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public FlightOrderDataEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set("NotificationServiceBusConnectionString", "testConn");
            ConfigReader.Set("NotificationTopicName", "testTopic");
            ConfigReader.Set("FlightCancelledAlertMessage", "Your flight has been cancelled. Please contact customer service for rebooking options.");
            ConfigReader.Set("FlightCancelledContent", "https://virginatlantic.com/trips");
            ConfigReader.Set("CheckInOpenedAlertMessage", "You’re now ready to check in for your flight to {arrival}.");
            ConfigReader.Set("CheckInOpenedContent", "https://virginatlantic.com/trips");
            ConfigReader.Set("SeatSelectionReminderAlertMessage", "It’s time to choose your seat for your upcoming flight.");
            ConfigReader.Set("SeatSelectionReminderContent", "https://virginatlantic.com/manage-booking");
            ConfigReader.Set("StarlinkEnabledAlertMessage", "Starlink WiFi is onboard your flight. Tap to find out more.");
            ConfigReader.Set("StarlinkEnabledContent", "https://virginatlantic.com/wifi");
            ConfigReader.Set("StarlinkCustomEventName", "test_starlink_event");
            ConfigReader.Set("FlightTimeChangedAlertMessage", "Your flight time has been changed. Please check your updated itinerary for new departure details. Please check your booking for details.");
            ConfigReader.Set("FlightTimeChangedContent", "https://virginatlantic.com/trips");
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

                                                      .AddSingleton<IFlightOrderDataEventCommand, FlightOrderDataEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            // Arrange
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightOrderDataEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var flightOrderReqData = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "ATL",
                        Arrival = "LHR",
                        OperatingCarrier = new()
                        {
                            FlightNum = "317",
                            CarrierCode = "VS",
                            CarrierName = "test"
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VA",
                        PnrLocator = "PNR-VA945",
                        CreatedDate = DateTime.Parse("2025-07-24T14:30:00")
                    },
                    Journeys =
                    [
                        new()
                        {
                            ScheduledDepartureLocalDateTime = DateTime.Parse("2025-07-24T14:30:00"),
                            ScheduledDepartureUtcDateTime = DateTime.Parse("2025-07-24T14:30:00"),
                            ScheduledArrivalLocalDateTime = DateTime.Parse("2025-07-24T14:30:00"),
                            ScheduledArrivalUtcDateTime = DateTime.Parse("2025-07-24T14:30:00"),
                            Origin = new Airport
                            {
                                TerminalName = null,
                                Code = null,
                                AirportName = "MAS"
                            },
                            Destination = null,
                            Segments = null
                        }
                    ],
                    Passengers =
                    [
                        new()
                        {
                            AmperityId = "123",
                            FirstName = "priya",
                            LastName = null,
                            PaxType = null
                        }
                    ]
                }
            };

            string reqData = Json.Serialize(Json.Serialize(flightOrderReqData).Compress());
            var cloudEvent = new CloudEvent
            {
                Data = JsonConvert.DeserializeObject<string>(reqData)
            };


            // Act            
            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            // validate data sent to table storage
            Assert.Equal(1, tableStorageService.Count);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                                                                                                                                  .PayLoad
                                                                                                                                  .Parse<JObject>();

            var recordData = Json.Deserialize<Notification.NotificationRequest>(Encoding.UTF8.GetBytes(record["data"].ToString()).DecompressToString());

            Assert.Equal(recordData.FlightOrderData.First().BookingReference.AirlineDesignator, flightOrderReqData.First().BookingReference.AirlineDesignator);
            Assert.Equal(recordData.FlightOrderData.First().BookingReference.PnrLocator, flightOrderReqData.First().BookingReference.PnrLocator);
            Assert.Equal(recordData.FlightOrderData.First().BookingReference.CreatedDate, flightOrderReqData.First().BookingReference.CreatedDate);

            // validate data sent to service bus
            Assert.Equal(1, messageBus.Delivered);
            Assert.NotNull(message);

            var messageData = ((byte[])message.Data).DecompressTo<Notification.NotificationRequest>();

            Assert.Equal(messageData.FlightOrderData.First().EventFlight.OperatingCarrier.FlightNum, flightOrderReqData.First().EventFlight.OperatingCarrier.FlightNum);
            Assert.Equal(messageData.FlightOrderData.First().EventFlight.ScheduledDepartureLocalDateTime, flightOrderReqData.First().EventFlight.ScheduledDepartureLocalDateTime);
        }

        [Fact]
        public async Task Handle_Should_Use_Starlink_Copy_When_Notification_Type_Is_Starlink()
        {
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightOrderDataEventCommand>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            messageBus.OnMessage += (m) => message = m;

            var flightOrderReqData = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "ATL",
                        Arrival = "LHR",
                        NotificationType = NotificationConstants.NotificationTypeStarlink,
                        OperatingCarrier = new()
                        {
                            FlightNum = "317",
                            CarrierCode = "VS",
                            CarrierName = "test"
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VA",
                        PnrLocator = "PNR-VA317",
                        CreatedDate = DateTime.Parse("2025-07-24T14:30:00")
                    },
                    Passengers =
                    [
                        new()
                        {
                            AmperityId = "123",
                            FirstName = "priya"
                        }
                    ]
                }
            };

            string reqData = Json.Serialize(Json.Serialize(flightOrderReqData).Compress());
            var cloudEvent = new CloudEvent
            {
                Data = JsonConvert.DeserializeObject<string>(reqData)
            };

            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            Assert.NotNull(message);

            var messageData = ((byte[])message.Data).DecompressTo<Notification.NotificationRequest>();

            Assert.Equal("https://virginatlantic.com/wifi", messageData.DeepLink);
            Assert.Equal("test_starlink_event", messageData.CustomEventName);
            Assert.Equal("Starlink WiFi is onboard your flight. Tap to find out more.", messageData.Message);
        }

        [Fact]
        public async Task Handle_Should_Not_Look_Up_Airports_When_Template_Has_No_Placeholders()
        {
            var flightDataRequestHandler = _serviceProvider.GetRequiredService<IFlightOrderDataEventCommand>();
            var messageBusFactory = _serviceProvider.GetRequiredService<IMessageBusFactory>();
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            messageBus.OnMessage += (m) => message = m;

            var cloudEvent = CreateCloudEvent(
            [
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "JFK",
                        Arrival = "BOS",
                        NotificationType = NotificationConstants.NotificationTypeStarlink,
                        OperatingCarrier = new()
                        {
                            FlightNum = "25",
                            CarrierCode = "VS",
                            CarrierName = "test"
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VA",
                        PnrLocator = "PNR-VA25",
                        CreatedDate = DateTime.Parse("2025-07-24T14:30:00")
                    },
                    Passengers =
                    [
                        new()
                        {
                            AmperityId = "123",
                            FirstName = "priya"
                        }
                    ]
                }
            ]);

            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            Assert.NotNull(message);

            var messageData = ((byte[])message.Data).DecompressTo<Notification.NotificationRequest>();

            Assert.Equal("Starlink WiFi is onboard your flight. Tap to find out more.", messageData.Message);
        }

        [Fact]
        public async Task Handle_Should_Process_AircraftSwap_Notification()
        {
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightOrderDataEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var flightOrderReqData = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "ATL",
                        Arrival = "LHR",
                        NotificationType = NotificationConstants.NotificationTypeAircraftSwap,
                        OperatingCarrier = new()
                        {
                            FlightNum = "317",
                            CarrierCode = "VS",
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VS",
                        PnrLocator = "PNR-VS317",
                    },
                    Passengers =
                    [
                        new()
                        {
                            FirstName = "priya",
                            LastName = "test",
                            Email = "test@fly.virgin.com"
                        }
                    ]
                }
            };

            string reqData = Json.Serialize(Json.Serialize(flightOrderReqData).Compress());
            var cloudEvent = new CloudEvent
            {
                Data = JsonConvert.DeserializeObject<string>(reqData)
            };

            // Act
            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            // validate data sent to table storage
            Assert.Equal(1, tableStorageService.Count);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                                                                                                                              .PayLoad
                                                                                                                              .Parse<JObject>();

            // For aircraft change the stored data is a compressed json list of FlightOrderData
            var decompressed = Encoding.UTF8.GetBytes(record["data"].ToString()).DecompressToString();
            var storedList = Json.Deserialize<List<FlightOrderData>>(decompressed);

            Assert.Equal(NotificationConstants.NotificationTypeAircraftSwap, storedList.First().EventFlight.NotificationType);
            Assert.Equal("317", storedList.First().EventFlight.OperatingCarrier.FlightNum);
            Assert.Equal("ATL", storedList.First().EventFlight.Departure);
            Assert.Equal("LHR", storedList.First().EventFlight.Arrival);
            Assert.Equal("priya", storedList.First().Passengers.First().FirstName);
            Assert.Equal("test", storedList.First().Passengers.First().LastName);
            Assert.Equal("test@fly.virgin.com", storedList.First().Passengers.First().Email);


            // validate message sent to service bus
            Assert.Equal(1, messageBus.Delivered);
            Assert.NotNull(message);
        }

        private static CloudEvent CreateCloudEvent(List<FlightOrderData> flightOrderData)
        {
            string reqData = Json.Serialize(Json.Serialize(flightOrderData).Compress());

            return new CloudEvent
            {
                Data = JsonConvert.DeserializeObject<string>(reqData)
            };
        }

        [Fact]
        public async Task Handle_Should_Skip_SeatSelectionReminder_When_AnySeatAssigned()
        {
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightOrderDataEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;
            messageBus.OnMessage += (m) => message = m;

            var flightOrderReqData = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "ATL",
                        Arrival = "LHR",
                        NotificationType = NotificationConstants.NotificationTypeSeatSelectionReminder,
                        OperatingCarrier = new()
                        {
                            FlightNum = "317",
                            CarrierCode = "VS",
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VS",
                        PnrLocator = "PNR-VS317",
                    },
                    Passengers =
                    [
                        new()
                        {
                            FirstName = "priya",
                            LastName = "test",
                            Email = "test@fly.virgin.com"
                        }
                    ],
                    Seats =
                    [
                        new SeatAllocation
                        {
                            FlightNumber = "317",
                            CurrentSeatAssignment = "12A",
                            DeparturePoint = "ATL",
                            ArrivalPoint = "LHR"
                        }
                    ]
                }
            };

            Assert.Single(flightOrderReqData);
            Assert.Equal(NotificationConstants.NotificationTypeSeatSelectionReminder, flightOrderReqData[0].EventFlight.NotificationType);

            var seat = Assert.Single(flightOrderReqData[0].Seats);
            Assert.Equal("317", seat.FlightNumber);
            Assert.Equal("12A", seat.CurrentSeatAssignment);

            var cloudEvent = CreateCloudEvent(flightOrderReqData);

            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            Assert.Equal(0, tableStorageService.Count);
            Assert.Equal(0, messageBus.Delivered);
            Assert.Equal(0, messageBus.Count);
            Assert.Null(message);
        }

        [Fact]
        public async Task Handle_Should_Process_SeatSelectionReminder_When_NoSeatsAssigned()
        {
            var flightDataRequestHandler = _serviceProvider.GetService<IFlightOrderDataEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("NotificationServiceBusConnectionString"), ConfigReader.Get("NotificationTopicName"));

            CloudEvent? message = default;
            messageBus.OnMessage += (m) => message = m;

            var flightOrderReqData = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new()
                    {
                        Departure = "ATL",
                        Arrival = "LHR",
                        NotificationType = NotificationConstants.NotificationTypeSeatSelectionReminder,
                        OperatingCarrier = new()
                        {
                            FlightNum = "317",
                            CarrierCode = "VS",
                        },
                        ScheduledDepartureLocalDateTime = DateTime.Parse("2025-09-29T14:30:00")
                    },
                    BookingReference = new BookingReference
                    {
                        AirlineDesignator = "VS",
                        PnrLocator = "PNR-VS317",
                    },
                    Passengers =
                    [
                        new()
                        {
                            FirstName = "priya",
                            LastName = "test",
                            Email = "test@fly.virgin.com"
                        }
                    ],
                    Seats = []
                }
            };

            Assert.Single(flightOrderReqData);
            Assert.Empty(flightOrderReqData[0].Seats);

            var cloudEvent = CreateCloudEvent(flightOrderReqData);

            await flightDataRequestHandler.Handle(cloudEvent, CancellationToken.None);

            Assert.Equal(1, tableStorageService.Count);
            Assert.Equal(1, messageBus.Delivered);
            Assert.Equal(0, messageBus.Count);
            Assert.NotNull(message);

            var messageData = ((byte[])message.Data).DecompressTo<Notification.NotificationRequest>();

            Assert.Equal("It’s time to choose your seat for your upcoming flight.", messageData.Message);
            Assert.Equal("https://virginatlantic.com/manage-booking", messageData.DeepLink);
            Assert.Single(messageData.FlightOrderData);
            Assert.Equal(flightOrderReqData[0].BookingReference.PnrLocator, messageData.FlightOrderData.First().BookingReference.PnrLocator);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                                                                                                                              .PayLoad
                                                                                                                              .Parse<JObject>();

            var recordData = Json.Deserialize<Notification.NotificationRequest>(Encoding.UTF8.GetBytes(record["data"].ToString()).DecompressToString());

            Assert.Equal("It’s time to choose your seat for your upcoming flight.", recordData.Message);
            Assert.Equal("https://virginatlantic.com/manage-booking", recordData.DeepLink);
            Assert.Single(recordData.FlightOrderData);
            Assert.Equal(flightOrderReqData[0].BookingReference.PnrLocator, recordData.FlightOrderData.First().BookingReference.PnrLocator);
        }
    }
}
