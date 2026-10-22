using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Notification.Application.Events;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services;
using Campaigns = VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification.Campaigns;
using NotificationEnvironment = VAA.EAPI.Microservices.CRM.Notification.Constants.Environment;

namespace VAA.EAPI.Microservices.CRM.Notification.Tests
{
    public class NotificationEventCommandHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationEventCommandHandlerTests()
        {
            ConfigReader.Set(NotificationEnvironment.AirshipApiUri, "https://go.airship.eu/api/push");
            ConfigReader.Set(NotificationEnvironment.AirshipToken, "test-token");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAirshipService, AirshipService>()
                                                      .AddSingleton<INotificationEventCommand, NotificationEventCommandHandler>()
                                                      .BuildServiceProvider();
        }

        [Theory]
        [InlineData(NotificationType.FlightCancelled)]
        [InlineData(NotificationType.CheckInOpened)]
        [InlineData(NotificationType.FlightTimeChanged)]
        public async Task Handle_WhenCloudEventIsValid_ShouldSendMappedAirshipRequest(NotificationType notificationType)
        {
            // Arrange
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var handler = _serviceProvider.GetRequiredService<INotificationEventCommand>();
            var responseCount = httpClientFactory.GetResponses(ConfigReader.Get(NotificationEnvironment.AirshipApiUri)).Count;
            var notificationRequest = new NotificationRequest
            {
                Message = "Test",
                DeepLink = "TestLink",
                FlightOrderData =
                [
                    new()
                    {
                        NotificationType = notificationType,
                        BookingReference = new BookingReference
                        {
                            PnrLocator = "PNR123",
                            CreatedDate = new DateTime(2024, 11, 25)
                        },
                        EventFlight = new Flight
                        {
                            OperatingCarrier = new Carrier { FlightNum = "VS123" }
                        }
                    }
                ]
            };

            var cloudEvent = new CloudEvent
            {
                Data = Convert.ToBase64String(Json.Serialize(notificationRequest).Compress())
            };

            // Act
            await handler.Handle(cloudEvent, TestContext.Current.CancellationToken);

            var response = httpClientFactory.GetResponses(ConfigReader.Get(NotificationEnvironment.AirshipApiUri));

            // Assert
            var airshipResponse = Assert.Single(response.Skip(responseCount));
            var payload = JObject.Parse(await airshipResponse.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));

            Assert.Equal("https://go.airship.eu/api/push", airshipResponse.RequestMessage.RequestUri!.ToString());
            Assert.Equal("PNR12325NOV24", Assert.Single(payload["audience"]!["tag"]!.Values<string>()));
            Assert.Equal("Trips", payload["audience"]!["group"]!.Value<string>());
            Assert.Equal("Test", payload["notification"]!["alert"]!.Value<string>());
            Assert.Equal("TestLink", payload["notification"]!["actions"]!["open"]!["content"]!.Value<string>());
            Assert.Null(payload["notification"]!["actions"]!["add_custom_event"]);
            Assert.Null(payload["campaigns"]);
        }

        [Fact]
        public async Task Handle_WhenCustomEventAndCampaignsAreProvided_ShouldPassOptionalPayloadToAirshipRequest()
        {
            // Arrange
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var handler = _serviceProvider.GetRequiredService<INotificationEventCommand>();
            var notificationRequest = new NotificationRequest
            {
                Message = "Test",
                DeepLink = "TestLink",
                CustomEventName = "emit_starlink",
                Campaigns = new Campaigns
                {
                    Categories = ["Starlink", "WiFi"]
                },
                FlightOrderData =
                [
                    new()
                    {
                        NotificationType = NotificationType.CheckInOpened,
                        BookingReference = new BookingReference
                        {
                            PnrLocator = "PNR123",
                            CreatedDate = new DateTime(2024, 11, 25)
                        },
                        EventFlight = new Flight
                        {
                            OperatingCarrier = new Carrier { FlightNum = "VS123" }
                        }
                    }
                ]
            };

            var cloudEvent = new CloudEvent
            {
                Data = Convert.ToBase64String(Json.Serialize(notificationRequest).Compress())
            };

            // Act
            await handler.Handle(cloudEvent, TestContext.Current.CancellationToken);

            var response = httpClientFactory.GetResponses(ConfigReader.Get(NotificationEnvironment.AirshipApiUri));

            // Assert
            var airshipResponse = Assert.Single(response);
            var payload = JObject.Parse(await airshipResponse.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));

            Assert.Equal("emit_starlink", payload["notification"]!["actions"]!["add_custom_event"]!["name"]!.Value<string>());
            Assert.Equal(new[] { "Starlink", "WiFi" }, payload["campaigns"]!["categories"]!.Values<string>());
        }

        [Fact]
        public async Task Handle_WhenCloudEventDataIsInvalid_ShouldSkipAirshipRequest()
        {
            // Arrange
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var handler = _serviceProvider.GetRequiredService<INotificationEventCommand>();
            var responseCount = httpClientFactory.GetResponses(ConfigReader.Get(NotificationEnvironment.AirshipApiUri)).Count;
            var cloudEvent = new CloudEvent { Data = "invalid-data" };

            // Act
            await handler.Handle(cloudEvent, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(responseCount, httpClientFactory.GetResponses(ConfigReader.Get(NotificationEnvironment.AirshipApiUri)).Count);
        }
    }
}
