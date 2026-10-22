using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.NDC.IFGFacade.Application.Command;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;
using static VAA.Facade.Test.Framework.TestHelpers;

namespace VAA.Facade.NDC.IFGFacade.Tests
{
    public class OrderCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IPaymentMSCaptureCommand _paymentMSCaptureCommand;
        private readonly IAirportDetailsQuery _airportDetailsQuery;
        private readonly ICurrencyDetailsQuery _currencyDetailsQuery;
        private readonly IOrderDataQuery _orderDataQuery;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly OrderCommandHandler _handler;

        public OrderCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _paymentMSCaptureCommand = new PaymentMSCaptureCommandHandler(_httpConnector);
            _airportDetailsQuery = new AirportDetailsQueryHandler(_httpConnector);
            _currencyDetailsQuery = new CurrencyDetailsQueryHandler(_httpConnector);
            _orderDataQuery = new OrderDataMSQueryHandler(_httpConnector);
            _handler = new OrderCommandHandler(_paymentMSCaptureCommand, _airportDetailsQuery, _currencyDetailsQuery, _orderDataQuery);
        }

        [Fact]
        public async Task OrderSuccess()
        {
            ConfigReader.Set("PaymentMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("PaymentMSUrl", "https://testresources.virginatlantic.com/dev/orderpaymentmicroservice/");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("ReferenceMSUrl", "https://testresources.virginatlantic.com/dev/referencemicroservice/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/DEV/orderdata/orders/");

            var captureRequest = TestDataProvider.GetTestData<Domain.OrderRequest>("NdcIfgFacade\\CaptureRequest");

            var result = await _handler.Handle(captureRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Value.TransactionId);
            Assert.Equal("COMPLETED", result.Value.Value.CaptureStatus);
            Assert.Equal("AUTH_ACCEPTED", result.Value.Value.PaymentStatus);
        }

        [Fact]
        public async Task OrderHandler_ReturnsControlledError_WhenDependencyFails()
        {
            var request = TestDataProvider.GetTestData<Domain.OrderRequest>("NdcIfgFacade\\CaptureRequest");
            var logger = new RecordingLogger<OrderCommandHandler>();
            var handler = new OrderCommandHandler(
                null,
                new ThrowingAirportDetailsQuery(new InvalidOperationException("synthetic failure")),
                null,
                null,
                logger);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Equal("Order command failed.", result.Value.Errors.First().Message);
            Assert.Equal(LogLevel.Error, logger.LastLevel);
        }

        [Fact]
        public async Task OrderHandler_RethrowsCallerCancellation()
        {
            var request = TestDataProvider.GetTestData<Domain.OrderRequest>("NdcIfgFacade\\CaptureRequest");
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            var cancellation = new OperationCanceledException(cancellationSource.Token);
            var handler = new OrderCommandHandler(
                null,
                new ThrowingAirportDetailsQuery(cancellation),
                null,
                null);

            await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(request, cancellationSource.Token));
        }
    }
}
