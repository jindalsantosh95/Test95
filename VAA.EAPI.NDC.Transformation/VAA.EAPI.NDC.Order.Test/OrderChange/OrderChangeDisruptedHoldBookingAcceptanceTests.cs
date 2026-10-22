using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeDisruptedHoldBookingAcceptanceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string OrderId = "VS932SSGCJB26";
        private const string BookingReference = "SSGCJB";
        private readonly IServiceProvider _serviceProvider;
        private readonly RecordingLogger<OrderChangeDisruptionCommandHandler> _logger = new();

        public OrderChangeDisruptedHoldBookingAcceptanceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(Constants.Environment.OrderDataMSEndpoint, "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set(Constants.Environment.OrderDataMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.ValidationMSUrl, "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set(Constants.Environment.ValidationMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.AgentRegistrationEnvironment, "test");
            ConfigReader.Set(Constants.Environment.OrderChangeMSUrl, "https://testresources.virginatlantic.com/dev/NdcOrderManagement/order/");
            ConfigReader.Set(Constants.Environment.OrderChangeMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.OrderSearchMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.PublishOrderDocUrl, "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ValidationMSCommandHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<PaymentMSRetrieveQueryHandler>()
                .AddSingleton<SabreOrderManagementMSCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<IBrandMSCommand, BrandCommandStub>()
                .AddSingleton<IBrandMSPTCCommand, BrandPtcCommandStub>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandlerStub>()
                .AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() })
                .AddSingleton<ILogger<OrderChangeDisruptionCommandHandler>>(_logger)
                .AddSingleton<OrderChangeDisruptionCommandHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task DisruptedHoldBookingAccept_UsesSharedHttpMocksAndReturnsHoldBookingResponse()
        {
            var handler = _serviceProvider.GetRequiredService<OrderChangeDisruptionCommandHandler>();

            var result = await handler.Handle(CreateAcceptChangeRequest(), "subscription-key", CancellationToken.None);

            var responseItem = result.Value.Value.Items.Single();
            var errorMessage = responseItem is ErrorType error
                ? $"{error.Code}: {error.DescText}{Environment.NewLine}{_logger.Exception}"
                : _logger.Exception?.ToString();
            Assert.True(responseItem is OrderViewResponseType, errorMessage);
            var response = (OrderViewResponseType)responseItem;
            var responseOrderItem = response.Order.Single().OrderItem.Single();

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal($"{OrderId}-1B", responseOrderItem.OrderItemID);
            Assert.Null(result.Value.Value.PaymentFunctions);
        }

        private static IATA_OrderChangeRQ CreateAcceptChangeRequest()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeDisruption");
            request.Request.Order.OrderID = OrderId;
            request.Request.BookingRef.First().BookingID = BookingReference;
            request.Request.PaymentFunctions = null;
            request.Request.ChangeOrderChoice.Item = new SelectedOrderItemType
            {
                OrderItemRefID = [$"{OrderId}-1A"]
            };

            return request;
        }

        private sealed class BrandCommandStub : IBrandMSCommand
        {
            public Task<IEnumerable<BrandRS>> Handle(BrandMSCommand brandReq, CancellationToken cancellationToken) =>
                Task.FromResult(Enumerable.Empty<BrandRS>());
        }

        private sealed class BrandPtcCommandStub : IBrandMSPTCCommand
        {
            public Task<IEnumerable<BrandRS>> Handle(BrandMSCommandPTC brandReq, CancellationToken cancellationToken) =>
                Task.FromResult(Enumerable.Empty<BrandRS>());
        }

        private sealed class TrackAnalyticsEventCommandHandlerStub : ITrackAnalyticsEventCommandHandler
        {
            public Task<bool> Handle(TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken) => Task.FromResult(true);
        }

        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public Exception? Exception { get; private set; }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Exception ??= exception;
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }

    }
}
