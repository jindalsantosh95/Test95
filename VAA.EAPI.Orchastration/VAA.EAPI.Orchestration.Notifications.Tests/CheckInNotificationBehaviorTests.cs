using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
using Notification = VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;
using NotificationEnvironment = VAA.EAPI.Orchestration.Notifications.Constants.Environment;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class CheckInNotificationBehaviorTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CheckInNotificationBehaviorTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set(NotificationEnvironment.NotificationServiceBusConnectionString, "testConn");
            ConfigReader.Set(NotificationEnvironment.NotificationTopicName, NotificationConstants.NotificationTopicName);
            ConfigReader.Set(NotificationEnvironment.CheckInOpenedAlertMessage, "You are now ready to check in for your flight from {departure} to {arrival}.");
            ConfigReader.Set(NotificationEnvironment.CheckInOpenedContent, "https://virginatlantic.com/trips");
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
                                                      .AddSingleton<IFlightDataRequestEventCommand, FlightDataRequestEventCommandHandler>()
                                                      .AddSingleton<IFlightOrderDataEventCommand, FlightOrderDataEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task CheckIn_Flow_Should_Preserve_Behavior_Contracts()
        {
            var expectations = LoadExpectations();
            var flightDataRequestHandler = _serviceProvider.GetRequiredService<IFlightDataRequestEventCommand>();
            var flightOrderDataHandler = _serviceProvider.GetRequiredService<IFlightOrderDataEventCommand>();
            var messageBusFactory = _serviceProvider.GetRequiredService<IMessageBusFactory>();
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(NotificationEnvironment.NotificationServiceBusConnectionString), ConfigReader.Get(NotificationEnvironment.NotificationTopicName));
            var messages = new List<CloudEvent>();

            messageBus.OnMessage += messages.Add;

            var flightDetail = new FlightDetail
            {
                Arrival = expectations.Value<string>("arrival"),
                Departure = expectations.Value<string>("departure"),
                Carrier = expectations.Value<string>("carrier"),
                FlightNo = int.Parse(expectations.Value<string>("flightNumber")!),
                Date = new DateOnly(2025, 09, 29),
                ScheduledTimeOfDeparture = new TimeOnly(14, 30, 00)
            };

            await flightDataRequestHandler.Handle(new CloudEvent { Data = Json.Serialize(flightDetail) }, CancellationToken.None);

            var orderRequestEvent = Assert.Single(messages, message => message.Type == NotificationConstants.FlightOrderDataReqEventSubject);
            var orderRequest = Assert.IsType<FlightOrderDataRequest>(orderRequestEvent.Data);

            Assert.Equal(expectations.Value<string>("departure"), orderRequest.Departure);
            Assert.Equal(expectations.Value<string>("arrival"), orderRequest.Arrival);
            Assert.Equal(expectations.Value<string>("flightNumber"), orderRequest.OperatingCarrier.FlightNum);

            var orderData = CreateOrderData(orderRequest, expectations);
            var cloudEventData = Json.Serialize(Json.Serialize(orderData).Compress());

            await flightOrderDataHandler.Handle(new CloudEvent { Data = JsonConvert.DeserializeObject<string>(cloudEventData) }, CancellationToken.None);

            var notificationEvent = Assert.Single(messages, message => message.Type == NotificationConstants.FlightOrderNotifReqEventSubject);
            var notification = Assert.IsType<byte[]>(notificationEvent.Data).DecompressTo<Notification.NotificationRequest>();

            Assert.Equal(expectations.Value<string>("deepLink"), notification.DeepLink);

            foreach (var expectedText in expectations["messageContains"]!.Values<string>())
            {
                Assert.Contains(expectedText!, notification.Message);
            }

            var notificationOrder = notification.FlightOrderData.Single();

            Assert.Equal(expectations.Value<string>("bookingReference"), notificationOrder.BookingReference.PnrLocator);
            Assert.Equal(expectations.Value<string>("bookingAirlineDesignator"), notificationOrder.BookingReference.AirlineDesignator);
            Assert.Equal(expectations.Value<string>("flightNumber"), notificationOrder.EventFlight.OperatingCarrier.FlightNum);
            Assert.Equal(2, messageBus.Delivered);
        }

        private static JObject LoadExpectations()
        {
            var path = Path.Combine(GetSolutionRoot(), "VAA.EAPI.Orchestration.Notifications.Tests", "BehaviorFixtures", "checkin_notification_expectations.json");
            return JObject.Parse(File.ReadAllText(path));
        }

        private static string GetSolutionRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "VAA.EAPI.Orchestration.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate VAA.EAPI.Orchestration.sln from the test output directory.");
        }

        private static List<FlightOrderData> CreateOrderData(FlightOrderDataRequest request, JObject expectations)
        {
            return
            [
                new()
                {
                    EventFlight = new()
                    {
                        Departure = request.Departure,
                        Arrival = request.Arrival,
                        OperatingCarrier = request.OperatingCarrier,
                        ScheduledDepartureLocalDateTime = request.ScheduledDepartureLocalDateTime
                    },
                    BookingReference = new()
                    {
                        AirlineDesignator = expectations.Value<string>("bookingAirlineDesignator"),
                        PnrLocator = expectations.Value<string>("bookingReference"),
                        CreatedDate = expectations.Value<DateTime>("bookingCreatedDate")
                    },
                    Journeys = [],
                    Passengers =
                    [
                        new()
                        {
                            AmperityId = "amp-317",
                            FirstName = "Priya"
                        }
                    ]
                }
            ];
        }
    }
}
