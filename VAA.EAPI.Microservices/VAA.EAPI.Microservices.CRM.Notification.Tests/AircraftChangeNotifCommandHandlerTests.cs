using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Xml;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Notification.Application.Events;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.CRM.Notification.Tests
{
    public class AircraftChangeNotifCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private IServiceProvider _serviceProvider = default!;

        public AircraftChangeNotifCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.DelayTime, "00:00:00");
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.ACMSessionToken, "TestToken");
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.ACMCommunicationChannel, "Testchannel");
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.EnableEmailMSAuthentication, "True");
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.AircraftChangeTemplate, "vaaTypeChange");
            ConfigReader.Set(VAA.EAPI.Microservices.CRM.Notification.Constants.Environment.RetryCounter, "3");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Email.Constants.Constants.EnableAdobeNewEndpoint, "true");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2, "https://virginatlantic-rt-stage4-all-lb.campaign.adobe.com/nl/jsp/soaprouter.jsp");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUri, "https://virginatlantic-rt-stage2.campaign.adobe.com/nl/jsp/soaprouter.jsp");
            ConfigReader.Set("AuthenticationUrl", "https://ims-na1.adobelogin.com/ims/");
            ConfigReader.Set("ClientId", "testId");
            ConfigReader.Set("ClientSecret", "testSecret");
            ConfigReader.Set("GrantType", "client_credentials");
            ConfigReader.Set("Scope", "AdobeID,openid,read_organizations,additional_info.projectedProductContext,additional_info.roles,adobeio_api,read_client_secret,manage_client_secrets,campaign_sdk,campaign_config_server_general,deliverability_service_general");
            ConfigReader.Set(Constants.Environment.TestEmail, string.Empty);

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMessageCenterService, MessageCenterService>()
                .AddSingleton<IAircraftChangeNotifCommand, AircraftChangeNotifCommandHandler>()
                .AddSingleton<IACMClient, ACMClient>()
                .AddSingleton<ITokenHelper, TokenHelper>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_WhenCloudEventIsValid_ShouldSendMappedMessageToMessageCenter()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IAircraftChangeNotifCommand>();
            var httpClientFactory = (TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var responseCount = httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2)).Count();
            var cloudEvent = CreateTestCloudEvent();

            // Act
            await handler.Handle(cloudEvent, CancellationToken.None);
            var response = httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2));

            // Assert
            var messageCenterResponse = Assert.Single(response.Skip(responseCount));

            var payloadBytes = await messageCenterResponse.RequestMessage!.Content!.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Encoding.UTF8.GetString(payloadBytes));
            Assert.NotNull(xmlDoc.DocumentElement);

            var rtEvent = xmlDoc.SelectSingleNode("//rtEvent");
            Assert.NotNull(rtEvent);
            Assert.Equal("vaaTypeChange", rtEvent!.Attributes!["type"]!.Value);
            Assert.Equal("test@fly.virgin.com", rtEvent.Attributes["email"]!.Value);

            Assert.Equal("Jeya", xmlDoc.SelectSingleNode("//ctx/FirstName")?.InnerText);
            Assert.Equal("Priya", xmlDoc.SelectSingleNode("//ctx/LastName")?.InnerText);
            Assert.Equal("AB1234", xmlDoc.SelectSingleNode("//ctx/BookingReference")?.InnerText);
            Assert.Equal("AI101", xmlDoc.SelectSingleNode("//ctx/FlightNumber")?.InnerText);
            Assert.Equal("DEL", xmlDoc.SelectSingleNode("//ctx/Origin")?.InnerText);
            Assert.Equal("BOM", xmlDoc.SelectSingleNode("//ctx/Destination")?.InnerText);
            Assert.Equal("Jun", xmlDoc.SelectSingleNode("//ctx/DepartureMonth")?.InnerText);
            Assert.Equal("15", xmlDoc.SelectSingleNode("//ctx/DepartureDay")?.InnerText);
            Assert.Equal("10:30", xmlDoc.SelectSingleNode("//ctx/DepartureTime")?.InnerText);
            Assert.Equal("Jun", xmlDoc.SelectSingleNode("//ctx/ArrivalMonth")?.InnerText);
            Assert.Equal("15", xmlDoc.SelectSingleNode("//ctx/ArrivalDay")?.InnerText);
            Assert.Equal("12:45", xmlDoc.SelectSingleNode("//ctx/ArrivalTime")?.InnerText);
        }

        [Fact]
        public async Task Handle_WhenCloudEventDataIsInvalid_ShouldSkipMessageCenterRequest()
        {
            // Arrange
            var httpClientFactory = (TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var responseCount = httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2)).Count();
            var handler = _serviceProvider.GetRequiredService<IAircraftChangeNotifCommand>();
            var cloudEvent = new CloudEvent { Data = "invalid-data" };

            // Act
            await handler.Handle(cloudEvent, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(responseCount, httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2)).Count());
        }

        [Fact]
        public async Task Handle_WhenCloudEventContainsMultipleItems_ShouldSendMultipleRequestsToMessageCenter()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IAircraftChangeNotifCommand>();
            var httpClientFactory = (TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var responseCount = httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2)).Count();
            var cloudEvent = CreateTestCloudEventWithMultipleItems();

            // Act
            await handler.Handle(cloudEvent, TestContext.Current.CancellationToken);
            var responses = httpClientFactory.GetResponses(ConfigReader.Get(VAA.EAPI.Infrastructure.Email.Constants.Constants.SoapEndpointUriV2)).Skip(responseCount).ToList();

            // Assert
            Assert.Equal(2, responses.Count);

            var firstBytes = await responses[0].RequestMessage!.Content!.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            var firstDoc = new XmlDocument();
            firstDoc.LoadXml(Encoding.UTF8.GetString(firstBytes));
            Assert.Equal("test1@fly.virgin.com", firstDoc.SelectSingleNode("//rtEvent")?.Attributes?["email"]?.Value);
            Assert.Equal("AB1234", firstDoc.SelectSingleNode("//ctx/BookingReference")?.InnerText);

            var secondBytes = await responses[1].RequestMessage!.Content!.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            var secondDoc = new XmlDocument();
            secondDoc.LoadXml(Encoding.UTF8.GetString(secondBytes));
            Assert.Equal("test2@fly.virgin.com", secondDoc.SelectSingleNode("//rtEvent")?.Attributes?["email"]?.Value);
            Assert.Equal("CD5678", secondDoc.SelectSingleNode("//ctx/BookingReference")?.InnerText);
        }

        private static CloudEvent CreateTestCloudEvent()
        {
            var request = new List<FlightOrderData>()
            {
                new()
                {
                    EventFlight = new Flight
                    {
                        Arrival = "BOM",
                        Departure = "DEL",
                        OperatingCarrier = new Carrier{ FlightNum = "AI101"},
                        ScheduledDepartureLocalDateTime = new DateTime(2026, 6, 15, 10, 30, 0),
                        ScheduledArrivalLocalDateTime = new DateTime(2026, 6, 15, 12, 45, 0)
                    },
                    BookingReference = new BookingReference { PnrLocator ="AB1234" },
                    Passengers = new List<Passenger>
                    {
                        new Passenger
                        {
                            FirstName = "Jeya",
                            LastName = "Priya",
                            Email = "test@fly.virgin.com"
                        }
                    }
                }
            };

            return new CloudEvent
            {
                Data = Convert.ToBase64String(Json.Serialize(request).Compress())
            };
        }

        private static CloudEvent CreateTestCloudEventWithMultipleItems()
        {
            var request = new List<FlightOrderData>
            {
                new()
                {
                    EventFlight = new Flight
                    {
                        Arrival = "BOM",
                        Departure = "DEL",
                        OperatingCarrier = new Carrier { FlightNum = "AI101" },
                    },
                    BookingReference = new BookingReference { PnrLocator = "AB1234" },
                    Journeys = new List<Journey>
                    {
                        new()
                        {
                            ScheduledDepartureLocalDateTime = new DateTime(2026, 6, 15, 10, 30, 0),
                            ScheduledArrivalLocalDateTime = new DateTime(2026, 6, 15, 12, 45, 0)
                        }
                    },
                    Passengers = new List<Passenger>
                    {
                        new()
                        {
                            FirstName = "Jeya",
                            LastName = "Priya",
                            Email = "test1@fly.virgin.com"
                        }
                    }
                },
                new()
                {
                    EventFlight = new Flight
                    {
                        Arrival = "LHR",
                        Departure = "JFK",
                        OperatingCarrier = new Carrier { FlightNum = "VS004" },
                    },
                    BookingReference = new BookingReference { PnrLocator = "CD5678" },
                    Journeys = new List<Journey>
                    {
                        new()
                        {
                            ScheduledDepartureLocalDateTime = new DateTime(2026, 7, 01, 9, 00, 0),
                            ScheduledArrivalLocalDateTime = new DateTime(2026, 7, 01, 21, 00, 0)
                        }
                    },
                    Passengers = new List<Passenger>
                    {
                        new()
                        {
                            FirstName = "Alex",
                            LastName = "Doe",
                            Email = "test2@fly.virgin.com"
                        }
                    }
                }
            };

            return new CloudEvent
            {
                Data = Convert.ToBase64String(Json.Serialize(request).Compress())
            };
        }

    }
}
