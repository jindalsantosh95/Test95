using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderCreateServiceCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderCreateServiceCommandHandler>> _logger;
        private readonly OrderCreateServiceCommandHandler _handler;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IAir4TokenGenerator> _air4TokenGenerator;

        public OrderCreateServiceCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderCreateServiceCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _air4TokenGenerator = new Mock<IAir4TokenGenerator>();
            _handler = new OrderCreateServiceCommandHandler(_logger.Object, _httpConnector.Object, _air4TokenGenerator.Object);

            OrderTestsCommon.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://tempuri.org/")
            {
                Content = new StringContent(TestDataProvider.GetTestData("OrderMS\\DeltaOrderRequest"), System.Text.Encoding.UTF8, "application/json")
            };

            httpRequestMessage.Headers.TryAddWithoutValidation("x-nowait", false.ToString()); // we add this here to mimic the behaviaor of the HttpConnectory class
            httpRequestMessage.Headers.TryAddWithoutValidation("applicationId", ConfigReader.Get(Constants.Environment.DlApiApplicationId));
            httpRequestMessage.Headers.TryAddWithoutValidation("channelId", ConfigReader.Get(Constants.Environment.DlApiChannelId));
            httpRequestMessage.Headers.TryAddWithoutValidation("transactionId", AsyncContext.Current.CorrelationId.ToString());

            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(TestDataProvider.GetTestData("OrderMS\\DeltaOrderResponse"), System.Text.Encoding.UTF8, "application/json")
            };

            httpClientFactoryFixture.RegisterRequestResponse(httpRequestMessage, httpResponseMessage);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateServiceCommandHandler(null, _httpConnector.Object, _air4TokenGenerator.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenApiConnectObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateServiceCommandHandler(_logger.Object, null, _air4TokenGenerator.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4ObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateServiceCommandHandler(_logger.Object, _httpConnector.Object, null));
        }

        [Fact]
        public async Task HandlerTest_Success()
        {
            var response = TestDataProvider.GetTestData<DlOrderResponse>("OrderMS\\DeltaOrderResponse");
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");

            _httpConnector.Setup(x => x.SendAsync<DlOrderResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(response);
            
            //Act
            var result = await _handler.Handle(new OrderCreateServiceCommand { Request = request }, System.Threading.CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value);
        }
    }
}
