using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class MetaPartnerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;

        public MetaPartnerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;

            ConfigReader.Set("ServiceBusConnectionString", "testConnectionString");
            ConfigReader.Set("SkyScannerConfiguration:Url", "https://tempuri.org/skyscanner");
            ConfigReader.Set("SkyScannerConfiguration:TrackingSourceId", "skyscanner");
            ConfigReader.Set("SkyScannerConfiguration:AuthorizationSchema", "Bearer");
            ConfigReader.Set("SkyScannerConfiguration:Secret", "000000000000-0000-0000-0000-0000000000000000");
            ConfigReader.Set("KayakConfiguration:Url", "https://tempuri.org/kayak");
            ConfigReader.Set("KayakConfiguration:PartnerCode", "VS");
            ConfigReader.Set("KayakConfiguration:CommissionPercentage", "0.02");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<IMetaPartnerCommandHandler, MetaPartnerCommandHandler>()
                                                      .AddSingleton<IMetaPartnerEventCommandHandler, MetaPartnerEventCommandHandler>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Theory]
        [InlineData(null, true, false)]   // MetaPartnerId null, flag enabled  -> NOT called
        [InlineData("", true, false)]     // MetaPartnerId empty, flag enabled  -> NOT called
        [InlineData("skyscanner", false, false)] // valid id, flag disabled     -> NOT called
        [InlineData("skyscanner", true, true)]   // valid id, flag enabled      -> called
        [InlineData("kayak", true, true)]         // valid id, flag enabled      -> called
        public async Task OrderEventProcessCommandHandler_MetaPartnerEvent_CalledOnlyWhenFlagEnabledAndPartnerIdPresent(
            string metaPartnerId, bool featureFlagEnabled, bool expectHandlerCalled)
        {
            ConfigReader.Set(Constants.Environment.EnableMetaPartnerEvent, featureFlagEnabled.ToString());

            var mockMetaPartnerEventCommandHandler = new Mock<IMetaPartnerEventCommandHandler>();
            mockMetaPartnerEventCommandHandler
                .Setup(x => x.Handle(It.IsAny<MetaPartnerEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var orderCreateRequest = new OrderCreateRequest
            {
                MetaPartner = metaPartnerId is null
                    ? new Dictionary<string, string> { { "MetaPartnerId", null } }
                    : new Dictionary<string, string> { { "MetaPartnerId", metaPartnerId } }
            };

            var resolvedMetaPartnerId = orderCreateRequest.MetaPartner?.GetValueOrDefault("MetaPartnerId");
            var shouldCall = featureFlagEnabled && !resolvedMetaPartnerId.IsNullOrWhiteSpace();

            if (shouldCall)
                await mockMetaPartnerEventCommandHandler.Object.Handle(new MetaPartnerEventCommand(), CancellationToken.None);

            if (expectHandlerCalled)
                mockMetaPartnerEventCommandHandler.Verify(x => x.Handle(It.IsAny<MetaPartnerEventCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            else
                mockMetaPartnerEventCommandHandler.Verify(x => x.Handle(It.IsAny<MetaPartnerEventCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("skyscanner")]
        [InlineData("kayak")]
        [InlineData("")]
        public async Task MetaPartnerEventCommandHandler(string partnerId)
        {
            var metaPartnerEventCommandHandler = _serviceProvider.GetService<IMetaPartnerEventCommandHandler>()!;
            var metaPartnerCommandHandler = _serviceProvider.GetService<IMetaPartnerCommandHandler>()!;
            var messageBusFactory = (MessageBusFactory)_serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("ServiceBusConnectionString"), Constants.Application.MetaPartnerEventHandlerTopicName);

            MetaPartnerEventCommand cloudEvent = default;

            // we do this to emulate the behavior of the service bus
            messageBus.OnMessage += (ce) => cloudEvent = Json.Deserialize<MetaPartnerEventCommand>(Json.Serialize(ce));

            var metaPartnerCommand = new MetaPartnerCommand();

            metaPartnerCommand.Attributes.AddOrUpdate("MetaPartnerId", partnerId);

            if (partnerId.Is("skyscanner"))
                metaPartnerCommand.Attributes.AddOrUpdate("skyscanner_redirectid", "adbcdef");

            if (partnerId.Is("kayak"))
                metaPartnerCommand.Attributes.AddOrUpdate("kayakclickid", "ABCDEFG");

            metaPartnerCommand.Attributes.AddOrUpdate("MetaPartnerId", partnerId);

            metaPartnerCommand.Attributes.AddOrUpdate("Currency", "GBP");
            metaPartnerCommand.Attributes.AddOrUpdate("Total", "100.00");
            metaPartnerCommand.Attributes.AddOrUpdate("BookedOn", "2025-11-01T18:30:00");
            metaPartnerCommand.Attributes.AddOrUpdate("BookingId", "ABC123");

            metaPartnerCommand.Attributes.AddOrUpdate("PaxCount", "1");
            metaPartnerCommand.Attributes.AddOrUpdate("AdultCount", "1");
            metaPartnerCommand.Attributes.AddOrUpdate("TeenCount", "0");
            metaPartnerCommand.Attributes.AddOrUpdate("ChildCount", "0");
            metaPartnerCommand.Attributes.AddOrUpdate("InfantCount", "0");

            metaPartnerCommand.Attributes.AddOrUpdate("FlightFare", "100.00");
            metaPartnerCommand.Attributes.AddOrUpdate("PaymentFee", "0");
            metaPartnerCommand.Attributes.AddOrUpdate("PaymentMethod", "debit-card");
            metaPartnerCommand.Attributes.AddOrUpdate("FareBasisCode", "AB123");
            metaPartnerCommand.Attributes.AddOrUpdate("MarketingCarrier", "VA");
            metaPartnerCommand.Attributes.AddOrUpdate("CabinClass", "economy");
            metaPartnerCommand.Attributes.AddOrUpdate("RouteType", "");
            metaPartnerCommand.Attributes.AddOrUpdate("HaulType", "long-haul");
            metaPartnerCommand.Attributes.AddOrUpdate("CommissionCurrency", "GBP");

            var metaPartnerEventCommand = (MetaPartnerEventCommand)metaPartnerCommand;

            Assert.NotNull(metaPartnerEventCommand.Id);
            Assert.Equal(Constants.Application.MetaPartnerEventType, metaPartnerEventCommand.Type);
            Assert.Equal(Constants.Application.MetaPartnerEventSubject, metaPartnerEventCommand.Subject);
            Assert.Equal(Constants.Application.MetaPartnerEventHandlerTopicName, metaPartnerEventCommand.Source);
            Assert.Equal(Constants.Application.EventSpecversion, metaPartnerEventCommand.Specversion);
            Assert.NotNull(metaPartnerEventCommand.Time);

            await metaPartnerEventCommandHandler.Handle(metaPartnerEventCommand, CancellationToken.None);

            await Task.Delay(100, TestContext.Current.CancellationToken);

            await metaPartnerCommandHandler.Handle((MetaPartnerCommand)cloudEvent, CancellationToken.None);
        }

        [Theory]
        [InlineData("100.25", null, "100.25")]
        [InlineData("100.25", "25.75", "126")]
        public void MetaPartnerCommand_ShouldCalculateTotalAndFlightFareFromPaymentDetails(string monetaryAmount, string milesEquivalentAmount, string expectedTotal)
        {
            var orderCreateRequest = BuildOrderCreateRequest();
            var orderDocument = BuildOrderDocument(monetaryAmount, milesEquivalentAmount);

            var command = (MetaPartnerCommand)(orderCreateRequest, orderDocument);

            Assert.Equal(expectedTotal, command.Attributes["Total"]);
            Assert.Equal(expectedTotal, command.Attributes["FlightFare"]);
        }

        [Theory]
        [InlineData(1, "LHR", "BGI", null, null, "one-way")]
        [InlineData(2, "LHR", "BGI", "BGI", "LHR", "return")]
        [InlineData(2, "LHR", "BGI", "JFK", "LAX", "multiple-destination")]
        [InlineData(3, "LHR", "BGI", "BGI", "LHR", "multiple-destination")]
        public void MetaPartnerCommand_ShouldSetRouteTypeFromOriginDestinationAssociations(
            int odCount,
            string origin1,
            string destination1,
            string origin2,
            string destination2,
            string expectedRouteType)
        {
            var orderCreateRequest = BuildOrderCreateRequest();
            var orderDocument = BuildOrderDocument("100.00", null, BuildOriginDestinationAssociations(odCount, origin1, destination1, origin2, destination2));

            var command = (MetaPartnerCommand)(orderCreateRequest, orderDocument);

            Assert.Equal(expectedRouteType, command.Attributes["RouteType"]);
        }

        [Theory]
        [InlineData("VSLT", "economy")]
        [InlineData("VSPE", "premium-economy")]
        [InlineData("VSUP", "business")]
        [InlineData("MAIN", "economy")]
        [InlineData("DPPS", "premium-economy")]
        [InlineData("D1", "business")]
        [InlineData("UNKNOWN", "UNKNOWN")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void MetaPartnerCommandHandler_ShouldMapSkyScannerCabinCodeToCabinClass(string cabinName, string expectedCabinClass)
        {
            var mappedCabinClass = MetaPartnerCommandHandler.MapSkyScannerCabinClass(cabinName);
            Assert.Equal(expectedCabinClass, mappedCabinClass);
        }

        [Fact]
        public async Task MetaPartnerCommandHandler_Kayak_ShouldGenerateUriWithSingleParameter()
        {
            // Arrange
            var mockHttpConnector = new Mock<IHttpConnector>();
            HttpRequestMessage capturedRequest = null;
            mockHttpConnector
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((msg, _) => capturedRequest = msg)
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var handler = new MetaPartnerCommandHandler(mockHttpConnector.Object);

            var command = new MetaPartnerCommand
            {
                Attributes = new Dictionary<string, string>
                {
                    ["MetaPartnerId"] = "Kayak",
                    ["kayakclickid"] = "testclickid",
                    ["CommissionCurrency"] = "GBP",
                    ["Currency"] = "GBP",
                    ["Total"] = "100.00",
                    ["BookedOn"] = "2025-11-01T18:30:00",
                    ["BookingId"] = "ABC123"
                }
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Get, capturedRequest.Method);
   
            var query = System.Web.HttpUtility.ParseQueryString(capturedRequest.RequestUri.Query);

            // Check currency parameter
            Assert.Equal("GBP", query["currency"]);
            var currencyParams = query.GetValues("currency");
            Assert.Equal(1, currencyParams.Length);
            Assert.Equal("GBP", currencyParams[0]);

            // Check other parameters
            Assert.Equal("VS", query["partnercode"]);
            Assert.Equal("ABC123", query["bookingid"]);
            Assert.Equal("2025-11-01T18:30:00+00:00", query["bookedon"]);
            Assert.Equal("100.00", query["price"]);
            Assert.Equal("2.00", query["kayakcommission"]); // 100.00 * 0.02
            Assert.Equal("GBP", query["commissioncurrency"]);
            Assert.Equal("testclickid", query["kayakclickid"]);
            Assert.Equal("flight", query["bookingtype"]);

            // Ensure no duplicates for any parameter
            foreach (string key in query.AllKeys)
            {
                var values = query.GetValues(key);
                Assert.True(values.Length == 1, $"Parameter '{key}' has duplicates");
            }
        }

        [Fact]
        public async Task MetaPartnerCommandHandler_Kayak_ShouldHandleMissingClickIdWithoutThrowing()
        {
            var handler = _serviceProvider.GetService<IMetaPartnerCommandHandler>()!;

            var command = new MetaPartnerCommand
            {
                Attributes = new Dictionary<string, string>
                {
                    ["MetaPartnerId"] = "Kayak",
                    ["CommissionCurrency"] = "GBP",
                    ["Currency"] = "GBP",
                    ["Total"] = "100.00",
                    ["BookedOn"] = "2025-11-01T18:30:00",
                    ["BookingId"] = "ABC123"
                }
            };
            
            var exception = await Record.ExceptionAsync(() => handler.Handle(command, CancellationToken.None));
            Assert.Null(exception);
        }

        private static OrderCreateRequest BuildOrderCreateRequest(string cabinName = "economy") => new()
        {
            MetaPartner = new Dictionary<string, string> { { "MetaPartnerId", "skyscanner" } },
            CustomerDetails = new[] { new Customer { Ptc = Constants.Application.DeltaAdultPTC } },
            SelectedOffers = new[]
            {
                new Offers
                {
                    OfferItems = new[]
                    {
                        new OfferItem
                        {
                            Services = new[]
                            {
                                new OfferService
                                {
                                    FareDetails = new FareDetails
                                    {
                                        FareBasisCode = "Y26",
                                        FareSegments = new[] { new FareSegments { CabinName = cabinName } }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        private static IEnumerable<OriginDestinationAssociation> BuildOriginDestinationAssociations(int odCount, string origin1, string destination1, string origin2, string destination2)
        {
            if (odCount <= 0) return [];

            var odAssociations = new List<OriginDestinationAssociation>
            {
                new() { OriginDestination = new OriginDestination { OriginAirport = origin1, DestinationAirport = destination1 } }
            };

            if (odCount >= 2)
            {
                odAssociations.Add(new OriginDestinationAssociation
                {
                    OriginDestination = new OriginDestination { OriginAirport = origin2, DestinationAirport = destination2 }
                });
            }

            if (odCount >= 3)
            {
                odAssociations.Add(new OriginDestinationAssociation
                {
                    OriginDestination = new OriginDestination { OriginAirport = "LAX", DestinationAirport = "SFO" }
                });
            }

            return odAssociations;
        }

        private static OrderDocument BuildOrderDocument(
            string monetaryAmount,
            string milesEquivalentAmount,
            IEnumerable<OriginDestinationAssociation> originDestinationAssociations = null) => new()
            {
                CreatedOn = "2026-04-30T00:00:00Z",
                BookingReference = new BookingReference { RLOC = "ABC123" },
                DataList = new DataList
                {
                    SegmentDetails = new[] { new Segment { MarketingCarrier = "VA" } },
                    OriginDestinationAssociations = originDestinationAssociations?.ToArray()
                },
                PaymentDetails = new[]
            {
                new PaymentDetails
                {
                    Amount = new CurrencyAmount { MonetaryAmount = monetaryAmount, BaseCurrency = "GBP" },
                    MilesEquivalentPrice = milesEquivalentAmount is null ? null : new CurrencyAmount { MonetaryAmount = milesEquivalentAmount, BaseCurrency = "GBP" },
                    CardMethod = new CardDetails { CardType = CardDetails.CardTypes.MasterCard }
                }
            }
            };
    }
}
