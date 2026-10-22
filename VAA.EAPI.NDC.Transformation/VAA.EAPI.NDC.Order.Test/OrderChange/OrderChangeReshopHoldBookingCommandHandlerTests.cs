using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeReshopHoldBookingCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderChangeReshopHoldBookingCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var analyticsMock = new Mock<ITrackAnalyticsEventCommandHandler>();

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderChangeMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/order/");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("ReferenceMSUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/");

            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("OrderChangeMSSubscriptionKey", "Test_OrderChangeMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "NONPROD");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "Test_ReferenceMSSubscriptionKey");

            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _serviceProvider = new ServiceCollection().AddLogging()
                                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                                  .AddSingleton<IMediator>((serviceProvider) => new Mediator(serviceProvider))
                                                                  .AddSingleton<IHostEnvironment>(_hostEnvironment)
                                                                  .AddSingleton<ValidationMSCommandHandler>()
                                                                  .AddSingleton<OfferMSQueryHandler>()
                                                                  .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                                                                  .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                                                                  .AddSingleton<OrderDataMSQueryHandler>()
                                                                  .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                                                                  .AddSingleton<SabreOrderManagementMSCommandHandler>()
                                                                  .AddSingleton<PublishOrderDocMSCommandHandler>()
                                                                  .AddSingleton<ManageCustomerRepoCommandHandler>()
                                                                  .AddSingleton<AirportDetailsQueryHandler>()
                                                                  .AddSingleton<IOrderChangeReshopHoldBookingCommand, OrderChangeReshopHoldBookingCommandHandler>()
                                                                  .AddSingleton(analyticsMock.Object)
                                                                  .BuildServiceProvider();
        }

        [Fact]
        public async Task OrderChangeReshopHoldBookingCommandHandlerSuccessScenario()
        {
          // Arrange

            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\ChangeReshopHoldBooking");
            var subscriptionKey = "1234";
            var orderChangeReshopHoldBookingCommand = _serviceProvider.GetService<IOrderChangeReshopHoldBookingCommand>()!;

            // Act
            var result = await orderChangeReshopHoldBookingCommand.Handle(request,subscriptionKey, CancellationToken.None);

           // Assert
            Assert.NotNull(result);
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932OBNDSA26", ((result.Value.Value.Items.First()) as OrderViewResponseType).Order.First().OrderID);
            Assert.Equal("VS932OBNDSA26-1A", ((result.Value.Value.Items.First()) as OrderViewResponseType).Order.First().OrderItem.Last().OrderItemID);
        }
    }
}
