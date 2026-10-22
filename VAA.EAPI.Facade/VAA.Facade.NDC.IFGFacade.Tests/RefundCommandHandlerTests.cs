using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Application.Command;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using static VAA.Facade.Test.Framework.TestHelpers;

namespace VAA.Facade.NDC.IFGFacade.Tests
{
    public class RefundCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IPaymentMSRefundCommand _paymentMSRefundCommand;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly RefundCommandHandler _handler;

        public RefundCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _paymentMSRefundCommand = new PaymentMSRefundCommandHandler(_httpConnector);
            _handler = new RefundCommandHandler(_paymentMSRefundCommand);
        }

        [Fact]
        public async Task RefundSuccess()
        {
            ConfigReader.Set("PaymentMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("PaymentMSUrl", "https://testresources.virginatlantic.com/dev/orderpaymentmicroservice/");

            var refundRequest = TestDataProvider.GetTestData<Domain.RefundRequest>("NdcIfgFacade\\RefundRequest");

            var result = await _handler.Handle(refundRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Value.HttpStatus.ToString());
            Assert.Equal(200, result.Value.Value.HttpStatus);
        }

        [Fact]
        public async Task RefundHandler_ReturnsControlledError_WhenPaymentCommandFails()
        {
            var request = TestDataProvider.GetTestData<RefundRequest>("NdcIfgFacade\\RefundRequest");
            var logger = new RecordingLogger<RefundCommandHandler>();
            var handler = new RefundCommandHandler(
                new ThrowingRefundCommand(new InvalidOperationException("synthetic failure")),
                logger);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Equal("Payment refund failed.", result.Value.Errors.First().Message);
            Assert.Equal(LogLevel.Error, logger.LastLevel);
        }
    }
}
