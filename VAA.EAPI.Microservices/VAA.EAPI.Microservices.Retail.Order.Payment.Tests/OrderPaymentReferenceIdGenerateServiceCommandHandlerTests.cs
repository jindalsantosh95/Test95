using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4TokenGenerator;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentReferenceIdGenerateServiceCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<OrderPaymentReferenceIdGenerateServiceCommandHandler> _logger;
        private readonly IHttpConnector _httpConnectorMock;
        private readonly IAir4TokenGenerator _tokenGeneratorMock;
        private readonly ICachedChannel _cachedChannelMock;
        private readonly ITableStorageService<Channel> _tableStorageServiceMock;
        private readonly IMemoryCache _cacheMock;

        private readonly OrderPaymentReferenceIdGenerateServiceCommandHandler _handler;

        public OrderPaymentReferenceIdGenerateServiceCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("DlPaymentClientId", "testValue");
            ConfigReader.Set("DlPaymentClientSecret", "testValue");
            ConfigReader.Set("DlPaymentAuthenticationUrl", "https://authentcationurl/");
            ConfigReader.Set("DlPaymentApiGenerateRefIdUri", "https://stage-snsapigateway.vs01.vs.air4.com/paymentManagement/v1/paymentReferenceId");
            ConfigReader.Set("Referencedatamicroservicedbkey", "testValue");
            ConfigReader.Set("Dlpaymentapiapplicationid", "testValue");
            ConfigReader.Set("DlPaymentApiChannelId", "testValue");

            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentReferenceIdGenerateServiceCommandHandler>.Instance;
            _httpConnectorMock = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _tableStorageServiceMock = new TableStorageService<Channel>();
            _cacheMock = new MemoryCache(new MemoryCacheOptions());
            _tokenGeneratorMock = new Air4TokenGenerator(_httpConnectorMock);
            _cachedChannelMock = new CachedChannel(_tableStorageServiceMock, _cacheMock);

            _handler = new OrderPaymentReferenceIdGenerateServiceCommandHandler(
                _logger,
                _httpConnectorMock,
                _tokenGeneratorMock,
                _cachedChannelMock
            );
            // Set a mock correlation ID in AsyncContext
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenApiCallSucceeds()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PaymentRefIdGenerateRequest>("PaymentMS\\OrderPaymentRefIdRequest");

            // Act
            var result = await _handler.Handle(new OrderPaymentReferenceIdGenerateServiceCommand() { PaymentRequest = request}, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

        }
    }
}
