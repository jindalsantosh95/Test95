using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.OrderChangeNotif
{
    public class OrderChangeNotifCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string DocumentRefundedOrderSearchUrl = "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNDocumentRefunded";
        private const string DocumentVoidedOrderSearchUrl = "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNDocumentVoided";
        private const string HistoricalCancellationOrderSearchUrl = "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNHistoricalCancellation";
        private const string OrderNotificationUrl = "https://Testresources.virginatlantic.com/dev/digital/NdcOrderNotification/OrderChangeNotif";
        private const string AgencyOCNDetailsUrl = "https://Testresources.virginatlantic.com/sit/validationmicroservice/AgencyOCNDetails/00003333";
        private const string AgencyOCNDetailsWithAggregatorUrl = "https://Testresources.virginatlantic.com/dev/validationmicroservice/AggregatorIdTest/AgencyOCNDetails/00003333?aggregatorId=AGGREGATOR1";
        private const string OrderDataMSUrlWithAggregator = "https://Testresources.virginatlantic.com/dev/orderdata/orders-with-aggregator/";
        private const string ValidationMSUrlWithAggregator = "https://Testresources.virginatlantic.com/dev/validationmicroservice/AggregatorIdTest/AgencyOCNDetails/";

        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IServiceProvider _serviceProvider;

        public OrderChangeNotifCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;

            var analyticsMock = new Mock<ITrackAnalyticsEventCommandHandler>();
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderSearchMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNTimeChange");
            ConfigReader.Set("ValidationMSAgencyOCNDetailsUrl", "https://Testresources.virginatlantic.com/sit/validationmicroservice/AgencyOCNDetails/");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation/OCN");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDocOCN");
            ConfigReader.Set("OrderNotificationMSUrl", OrderNotificationUrl);

            ConfigReader.Set("OrderDataMSSubscriptionKey", "1234");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "1234");
            ConfigReader.Set("ValidationMSSubscriptionKey", "1234");
            ConfigReader.Set("AgentRegistrationEnvironment", "test");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("OrderNotificationMSSubscriptionKey", "1234");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOrderNotificationMSCommand, OrderNotificationMSCommandHandler>()
                                                      .AddSingleton<IMediator>((serviceProvider) => new Mediator(serviceProvider))
                                                      .AddSingleton<PublishOrderDocMSCommandHandler>()
                                                      .AddSingleton<OrderDataMSQueryHandler>()
                                                      .AddSingleton<ValidationMSAgencyOCNDetailsQueryHandler>()
                                                      .AddSingleton<OrderSearchMSCommandHandler>()
                                                      .AddSingleton<ValidationMSCommandHandler>()
                                                      .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                                                      .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                                                      .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>()
                                                      .AddSingleton<IInRuleService, InRuleService>()
                                                      .AddSingleton<IOrderChangeNotifCommand, OrderChangeNotifCommandHandler>()
                                                      .AddSingleton(analyticsMock.Object)
                                                      .BuildServiceProvider();
        }

        [Theory]
        [InlineData(DocumentVoidedOrderSearchUrl, ChangeTypeEnum.DocumentVoided)]
        [InlineData(DocumentRefundedOrderSearchUrl, ChangeTypeEnum.DocumentRefunded)]
        public async Task Handle_SendsOrderChangeNotification_WhenLatestChangeIsCancellation(string orderSearchUrl, ChangeTypeEnum expectedChangeType)
        {
            // Arrange
            ConfigReader.Set("OrderSearchMSUrl", orderSearchUrl);
            var orderChangeNotifCommandHandler = _serviceProvider.GetService<IOrderChangeNotifCommand>()!;
            var request = Microservices.Framework.Tests.TestHelpers.TestDataProvider.GetXmlTestData<OrderChangeNotifRQ>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
            var notificationResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(OrderNotificationUrl)
                .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, OrderNotificationUrl, StringComparison.OrdinalIgnoreCase));

            // Act
            var result = await orderChangeNotifCommandHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Accepted, result.Value.Status);

            var notificationResponsesAfter = _httpClientFactoryFixture.Current.GetResponses(OrderNotificationUrl)
                .Where(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, OrderNotificationUrl, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Equal(notificationResponsesBefore + 1, notificationResponsesAfter.Count);
            Assert.True(notificationResponsesAfter[^1].IsSuccessStatusCode);

            var notificationRequestJson = await notificationResponsesAfter[^1].RequestMessage!.Content!.ReadAsStringAsync();
            var notificationRequest = Json.Deserialize<OrderNotificationRequest>(notificationRequestJson);

            Assert.NotNull(notificationRequest);
            Assert.NotNull(notificationRequest.OrderDocument);
            Assert.Equal(3, notificationRequest.OrderDocument.Version);
            Assert.Equal(expectedChangeType, notificationRequest.OrderDocument.ChangeType);
            Assert.Equal("2026-01-16T17:23:40Z", notificationRequest.OrderDocument.ChangeDateTime.UtcDateTime);
        }

        [Fact]
        public async Task Handle_DoesNotSendOrderChangeNotification_WhenCancellationIsNotLatestChange()
        {
            // Arrange
            ConfigReader.Set("OrderSearchMSUrl", HistoricalCancellationOrderSearchUrl);
            var orderChangeNotifCommandHandler = _serviceProvider.GetService<IOrderChangeNotifCommand>()!;
            var request = Microservices.Framework.Tests.TestHelpers.TestDataProvider.GetXmlTestData<OrderChangeNotifRQ>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
            var notificationResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(OrderNotificationUrl)
                .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, OrderNotificationUrl, StringComparison.OrdinalIgnoreCase));

            // Act
            var result = await orderChangeNotifCommandHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Accepted, result.Value.Status);

            var notificationResponsesAfter = _httpClientFactoryFixture.Current.GetResponses(OrderNotificationUrl)
                .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, OrderNotificationUrl, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(notificationResponsesBefore, notificationResponsesAfter);
        }

        [Fact]
 
        public async Task OCNTimeChangeSuccessScenario()
        {
            // Arrange
            var orderChangeNotifCommandHandler = _serviceProvider.GetService<IOrderChangeNotifCommand>()!;
            var request = Microservices.Framework.Tests.TestHelpers.TestDataProvider.GetXmlTestData<OrderChangeNotifRQ>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
            var publishOrderDocUrl = ConfigReader.Get("PublishOrderDocUrl");
            var agencyOCNDetailsResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(AgencyOCNDetailsUrl)
                .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, AgencyOCNDetailsUrl, StringComparison.OrdinalIgnoreCase));
            var publishResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(publishOrderDocUrl)
                .Count(r => string.Equals(r.RequestMessage?.RequestUri?.AbsoluteUri, publishOrderDocUrl, StringComparison.OrdinalIgnoreCase));
 
            // Act
            var result = await orderChangeNotifCommandHandler.Handle(request, CancellationToken.None);
 
            // as the internal call is not awaited, lets wait for a bit
            await Task.Delay(2000, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Accepted, result.Value.Status);

            var publishResponsesAfter = _httpClientFactoryFixture.Current.GetResponses(publishOrderDocUrl)
                .Where(r => string.Equals(r.RequestMessage?.RequestUri?.AbsoluteUri, publishOrderDocUrl, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Equal(publishResponsesBefore + 2, publishResponsesAfter.Count);
            Assert.True(publishResponsesAfter[^1].IsSuccessStatusCode);

            var agencyOCNDetailsResponsesAfter = _httpClientFactoryFixture.Current.GetResponses(AgencyOCNDetailsUrl)
                .Where(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, AgencyOCNDetailsUrl, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Equal(agencyOCNDetailsResponsesBefore + 1, agencyOCNDetailsResponsesAfter.Count);
            Assert.True(agencyOCNDetailsResponsesAfter[^1].IsSuccessStatusCode);
        }

        [Fact]
        public async Task Handle_TimeChange_UsesDistributorAgencyIdAsAggregatorId()
        {
            var originalOrderDataMSUrl = ConfigReader.Get("OrderDataMSEndpoint");
            var originalValidationMSUrl = ConfigReader.Get("ValidationMSAgencyOCNDetailsUrl");
            ConfigReader.Set("OrderDataMSEndpoint", OrderDataMSUrlWithAggregator);
            ConfigReader.Set("ValidationMSAgencyOCNDetailsUrl", ValidationMSUrlWithAggregator);

            try
            {
                var orderChangeNotifCommandHandler = _serviceProvider.GetService<IOrderChangeNotifCommand>()!;
                var request = Microservices.Framework.Tests.TestHelpers.TestDataProvider.GetXmlTestData<OrderChangeNotifRQ>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
                var responsesBefore = _httpClientFactoryFixture.Current.GetResponses(AgencyOCNDetailsWithAggregatorUrl)
                    .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, AgencyOCNDetailsWithAggregatorUrl, StringComparison.OrdinalIgnoreCase));

                var result = await orderChangeNotifCommandHandler.Handle(request, TestContext.Current.CancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

                var responsesAfter = _httpClientFactoryFixture.Current.GetResponses(AgencyOCNDetailsWithAggregatorUrl)
                    .Where(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, AgencyOCNDetailsWithAggregatorUrl, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Assert.True(result.IsSuccess);
                Assert.Equal(responsesBefore + 1, responsesAfter.Count);
                Assert.True(responsesAfter[^1].IsSuccessStatusCode);
            }
            finally
            {
                ConfigReader.Set("OrderDataMSEndpoint", originalOrderDataMSUrl);
                ConfigReader.Set("ValidationMSAgencyOCNDetailsUrl", originalValidationMSUrl);
            }
        }

        [Fact]
        public async Task OCNHoldBookingIssuanceSuccessScenario()
        {
            // Arrange
            ConfigReader.Set("OrderSearchMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderHistoryOCNHoldBookingIssuance");
            var orderChangeNotifCommandHandler = _serviceProvider.GetService<IOrderChangeNotifCommand>()!;
            var request = Microservices.Framework.Tests.TestHelpers.TestDataProvider.GetXmlTestData<OrderChangeNotifRQ>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
            var publishOrderDocUrl = ConfigReader.Get("PublishOrderDocUrl");
            var publishResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(publishOrderDocUrl)
                .Count(r => string.Equals(r.RequestMessage?.RequestUri?.AbsoluteUri, publishOrderDocUrl, StringComparison.OrdinalIgnoreCase));

            // Act
            var result = await orderChangeNotifCommandHandler.Handle(request, CancellationToken.None);

            // as the internal call is not awaited, lets wait for a bit
            await Task.Delay(2000, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Accepted, result.Value.Status);

            var publishResponsesAfter = _httpClientFactoryFixture.Current.GetResponses(publishOrderDocUrl)
                .Where(r => string.Equals(r.RequestMessage?.RequestUri?.AbsoluteUri, publishOrderDocUrl, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Equal(publishResponsesBefore, publishResponsesAfter.Count);
        }

     }
 }
