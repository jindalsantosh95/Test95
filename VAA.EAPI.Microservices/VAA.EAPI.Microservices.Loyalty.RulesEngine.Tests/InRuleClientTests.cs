using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Client;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class InRuleClientTests

    {
        private readonly Mock<System.Net.Http.IHttpClientFactory> _mockIHttpClientFactory;
        private readonly Mock<ILogger<InRuleClient>> _logger;
        private readonly InRuleClient _inRuleClient;
        private readonly Mock<HttpClient> _mockClient;

        public InRuleClientTests()
        {
            _logger = new Mock<ILogger<InRuleClient>>();
            _mockIHttpClientFactory = new Mock<System.Net.Http.IHttpClientFactory>();
            _mockClient = new Mock<HttpClient>();
            _inRuleClient = new InRuleClient(_mockIHttpClientFactory.Object, _logger.Object);
            ConfigReader.Set("InRuleAPIURL", "https://vaa-dev-execute.inrulecloud.com/HttpService.svc/ApplyRules");
            ConfigReader.Set("InRuleAPIHeader", "APIKEY T5UE359897O54E41K5G10C3A669B73RQ69");
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenHttpClientFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new InRuleClient(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new InRuleClient(_mockIHttpClientFactory.Object, null));
        }

        [Fact]
        public void ExecuteInRuleShouldSuccess()
        {
            var ruleEngineRequest = TestDataProvider.GetTestData<string>("InRuleEngine\\CombinedRuleSetEventCommand");
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
               })
               .Verifiable();
            HttpClient httpClient = new HttpClient(handlerMock.Object);
            Task<HttpResponseMessage> msg = null;
            _mockIHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None)).Returns(msg);
            var result = _inRuleClient.ExecuteInRule(ruleEngineRequest);
            Assert.True(result.Result.Item1);
        }

        [Fact]
        public void ExecuteInRuleShouldReturnError()
        {
            var ruleEngineRequest = TestDataProvider.GetTestData<string>("InRuleEngine\\CombinedRuleSetEventCommand");
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.NotFound,
               })
               .Verifiable();
            HttpClient httpClient = new HttpClient(handlerMock.Object);
            Task<HttpResponseMessage> msg = null;
            _mockIHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None)).Returns(msg);
            var result = _inRuleClient.ExecuteInRule(ruleEngineRequest);
            Assert.False(result.Result.Item1);
        }
    }
}

