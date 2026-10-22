using Moq;
using Moq.Protected;
using System.Diagnostics.CodeAnalysis;

namespace VAA.EAPI.Microservices.Framework.Tests
{
    [ExcludeFromCodeCoverage]
    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _mockResponseProvider;

        public MockHttpClientFactory()
        {
        }
        public MockHttpClientFactory(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> mockResponseProvider)
        {
            _mockResponseProvider = mockResponseProvider;
        }

        public HttpClient CreateClient(string name)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync((HttpRequestMessage a, CancellationToken b) =>
               {
                   return _mockResponseProvider(a, b);
               });
            var httpClient = new HttpClient(handlerMock.Object);

            return httpClient;
        }
    }

}
