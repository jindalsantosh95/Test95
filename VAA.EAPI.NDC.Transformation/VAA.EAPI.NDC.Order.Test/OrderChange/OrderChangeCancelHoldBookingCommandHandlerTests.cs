using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeCancelHoldBookingCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderChangeCancelHoldBookingCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderCancelMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/ordercancel");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("OrderCancelMSSubscriptionKey", "Test_OrderCancelMSSubscriptionKey");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "NONPROD");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() })
                .AddSingleton<ValidationMSCommandHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<OrderCancelCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandlerStub>()
                .AddSingleton<IOrderChangeCancelHoldBookingCommand, OrderChangeCancelHoldBookingCommandHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_WhenHoldBookingCancellationSucceeds_ReturnsCancelledOrder()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeCancelHoldBooking");
            var commandHandler = _serviceProvider.GetRequiredService<IOrderChangeCancelHoldBookingCommand>();

            var result = await commandHandler.Handle(request, "1234", CancellationToken.None);
            var orderResponse = Assert.IsType<OrderViewResponseType>(result.Value.Value.Items.First());

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932PAYOHD26", orderResponse.Order.First().OrderID);
            Assert.Equal("CLOSED", orderResponse.Order.First().StatusCode);
        }

        private sealed class TrackAnalyticsEventCommandHandlerStub : ITrackAnalyticsEventCommandHandler
        {
            public Task<bool> Handle(TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}
