using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Http.Mocking;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    public sealed class HttpMockExtensionsTests
    {
        private static readonly ILogger Logger = LoggerFactory.Create(static _ => { }).CreateLogger<HttpMockExtensionsTests>();

        [Fact]
        public void FindMock_ReturnsNull_WhenSourceIsEmpty()
        {
            HttpMock[] mocks = [];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "https://tempuri.org/orders");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Null(result);
        }

        [Fact]
        public void FindMock_ReturnsNull_WhenRequestLineDoesNotMatch()
        {
            HttpMock[] mocks = [LoadMock("get-orders-header-match.mock")];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "https://tempuri.org/customers");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Null(result);
        }

        [Fact]
        public void FindMock_ReturnsMock_WhenHeadersMatch_AndNoContentRules()
        {
            HttpMock mock = LoadMock("get-orders-header-match.mock");
            HttpMock[] mocks = [mock];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "https://tempuri.org/orders");

            request.Headers.TryAddWithoutValidation("x-correlation-id", "abc123");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Same(mock, result);
        }

        [Fact]
        public void FindMock_ReturnsMock_WhenJsonRulesMatch()
        {
            HttpMock mock = LoadMock("post-orders-json-rules.mock");
            HttpMock[] mocks = [mock];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "https://tempuri.org/orders", "{\"orderId\":\"O-1\",\"status\":\"created\"}", "application/json");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Same(mock, result);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        public void FindMock_ReturnsMock_WhenXmlRulesMatch(string mediaType)
        {
            HttpMock mock = LoadMock("put-order-xml-rules.mock");
            HttpMock[] mocks = [mock];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Put, "https://tempuri.org/orders/1", "<root><id>1</id></root>", mediaType);

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Same(mock, result);
        }

        [Fact]
        public void FindMock_ReturnsMock_WhenRegexRulesMatch()
        {
            HttpMock mock = LoadMock("patch-order-regex-rules.mock");
            HttpMock[] mocks = [mock];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Patch, "https://tempuri.org/orders/1", "order-123", "text/plain");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Same(mock, result);
        }

        [Fact]
        public void FindMock_ReturnsNull_WhenRegexRuleIsInvalid()
        {
            HttpMock mock = LoadMock("delete-order-invalid-regex-rule.mock");
            HttpMock[] mocks = [mock];

            using HttpRequestMessage request = CreateRequest(HttpMethod.Delete, "https://tempuri.org/orders/1", "order-123", "text/plain");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Null(result);
        }

        [Fact]
        public void FindMock_Works_WithConcurrentBagOverload()
        {
            HttpMock mock = LoadMock("get-ping.mock");
            ConcurrentBag<HttpMock> mocks = new([mock]);

            using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "https://tempuri.org/ping");

            HttpMock? result = mocks.FindMock(request, Logger);

            Assert.Same(mock, result);
        }

        private static HttpMock LoadMock(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, ".mocks", "HttpMockExtensions", fileName);

            Assert.True(File.Exists(path), $"Mock file not found: {path}");

            return (HttpMock)new FileInfo(path);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, string? content = null, string? mediaType = null)
        {
            HttpRequestMessage request = new(method, requestUri);

            if (content is not null)
                request.Content = mediaType is null ? new StringContent(content) : new StringContent(content, System.Text.Encoding.UTF8, mediaType);

            return request;
        }
    }
}
