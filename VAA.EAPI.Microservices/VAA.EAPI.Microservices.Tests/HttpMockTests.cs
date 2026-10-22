using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Microservices.Tests
{
    public sealed class HttpMockTests
    {
        [Fact]
        public void TryExtractMockRequestReturnsFalseWhenRawRequestHeaderIsMissing()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK);

            var result = HttpMock.TryExtractMockRequest(response, out var request);

            Assert.False(result);
            Assert.Null(request);
        }

        [Fact]
        public void TryExtractMockRequestReturnsFalseWhenRawRequestHeaderIsNotBase64()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.Headers.TryAddWithoutValidation("x-mock-raw-request", "not-base64");

            var result = HttpMock.TryExtractMockRequest(response, out var request);

            Assert.False(result);
            Assert.Null(request);
        }

        [Fact]
        public void TryExtractMockRequestReturnsParsedRequestWhenRawRequestHeaderIsPresent()
        {
            const string rawRequest = "GET https://tempuri.org/mock HTTP/1.1\r\nAccept: application/json\r\n\r\n";
            using var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.Headers.TryAddWithoutValidation(
                "x-mock-raw-request",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(rawRequest)));

            var result = HttpMock.TryExtractMockRequest(response, out var request);

            Assert.True(result);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://tempuri.org/mock", request.RequestUri!.ToString());
            Assert.True(request.Headers.TryGetValues("Accept", out var values));
            Assert.Equal("application/json", Assert.Single(values));
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        public void FindMockUsesXPathForXmlContentTypes(string mediaType)
        {
            var mock = new HttpMock([
                "###Request",
                "POST https://tempuri.org/mock HTTP/1.1",
                string.Empty,
                "/search/inputs/slice[@destination='MCO' and @origin='LHR']",
                "###Response",
                "HTTP/1.1 200 OK",
                string.Empty
            ]);
            var mocks = new ConcurrentBag<HttpMock>([mock]);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://tempuri.org/mock")
            {
                Content = new StringContent("<search><inputs><slice origin=\"LHR\" destination=\"MCO\" /></inputs></search>", Encoding.UTF8, mediaType)
            };

            var result = mocks.FindMock(request, NullLogger.Instance);

            Assert.Same(mock, result);
        }

        [Fact]
        public async Task MockHandlerReusesLargeCachedBodyUnderConcurrentLoad()
        {
            const string requestUri = "https://tempuri.org/large-response";
            const int responseSize = 16 * 1024 * 1024;
            ConfigReader.Set("MockDelegatingHandler:Enabled", "true");
            ConfigReader.Set("MockDelegatingHandler:MockServiceUri", "https://tempuri.org/mock");

            var innerHandler = new LargeMockServiceHandler(new byte[responseSize]);
            using var handler = new MockDelegatingHandler(NullLogger.Instance)
            {
                InnerHandler = innerHandler
            };
            using var invoker = new HttpMessageInvoker(handler);
            using (var seedRequest = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (var seedResponse = await invoker.SendAsync(seedRequest, CancellationToken.None))
            {
                Assert.Equal(responseSize, seedResponse.Content.Headers.ContentLength);
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var requests = Enumerable.Range(0, 64).Select(async _ =>
            {
                await start.Task;
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                return await invoker.SendAsync(request, CancellationToken.None);
            }).ToArray();

            start.SetResult();
            var responses = await Task.WhenAll(requests);

            try
            {
                Assert.All(responses, response => Assert.Equal(responseSize, response.Content.Headers.ContentLength));
                Assert.Equal(1, innerHandler.CallCount);
            }
            finally
            {
                foreach (var response in responses)
                    response.Dispose();
            }
        }

        private sealed class LargeMockServiceHandler(byte[] body) : HttpMessageHandler
        {
            private readonly byte[] _body = body;

            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                const string rawRequest = "GET https://tempuri.org/large-response HTTP/1.1\r\n\r\n";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(_body),
                    RequestMessage = request
                };

                response.Headers.TryAddWithoutValidation(
                    "x-mock-raw-request",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(rawRequest)));

                return Task.FromResult(response);
            }
        }
    }
}
