using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Http;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Tests
{
    public sealed class HttpClientFactoryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture httpClientFactoryFixture;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;

        public HttpClientFactoryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            this.httpClientFactoryFixture = httpClientFactoryFixture;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
        }

        [Fact]
        private void CreateHttpClientFromFactory()
        {
            var httpClient = httpClientFactoryFixture.Current.CreateClient();

            Assert.NotNull(httpClient);
            Assert.IsType<HttpClient>(httpClient);
        }

        [Fact]
        private async Task HttpConnectorExternalCall()
        {
            var response = await _httpConnector.GetAsync("http://www.tempuri.org", null, null);

            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }

        [Fact]
        private async Task MockDelegatingHandlerMapped()
        {
            var httpClient = httpClientFactoryFixture.Current.CreateClient("https://tempuri.org/mock");

            Assert.NotNull(httpClient);
            Assert.IsType<HttpClient>(httpClient);

            var response = await httpClient.GetAsync("https://tempuri.org/mock");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Welcome to a mocked page", response.ReadResponseAsString());
        }

        [Fact]
        private async Task DefaultDelegatingHandlerDoesNotInjectTraceHeadersWithoutHttpClientInstrumentation()
        {
            var captureHandler = new CaptureRequestHandler();
            var httpClient = new HttpClient(new DefaultDelegatingHandler(captureHandler, _logger));

            var response = await httpClient.GetAsync("https://tempuri.org/trace");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(captureHandler.Request);
            Assert.False(captureHandler.Request.Headers.Contains(AsyncContext.W3cTraceContextHeader));
            Assert.False(captureHandler.Request.Headers.Contains(AsyncContext.W3cTraceStateHeader));
            Assert.True(captureHandler.Request.Headers.TryGetValues(AsyncContext.W3CBaggageHeader, out var baggageValues));
            Assert.Equal(AsyncContext.Current.W3CBaggage, Assert.Single(baggageValues));
        }

        [Fact]
        private async Task HttpConnectorReturnsTrackedContentWithoutChangingResponseContract()
        {
            using var response = await _httpConnector.GetAsync("https://tempuri.org/mock", null, null);

            Assert.IsType<CompletionTrackingHttpContent>(response.Content);
            Assert.Contains(
                "Welcome to a mocked page",
                await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        private async Task CompletionTrackingHttpContentReportsConsumedByteCountOnlyOnce()
        {
            var completionCounts = new List<long>();
            using var content = new CompletionTrackingHttpContent(
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("response body"))),
                completionCounts.Add);

            Assert.Equal(
                "response body",
                await content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            content.Dispose();

            Assert.Equal([13L], completionCounts);
        }

        [Fact]
        private async Task CompletionTrackingHttpContentReportsPartialByteCountWhenDisposed()
        {
            var completionCounts = new List<long>();
            using var content = new CompletionTrackingHttpContent(
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("response body"))),
                completionCounts.Add);

            await using (var stream = await content.ReadAsStreamAsync(TestContext.Current.CancellationToken))
            {
                var buffer = new byte[4];
                Assert.Equal(4, await stream.ReadAsync(buffer, TestContext.Current.CancellationToken));
            }

            Assert.Equal([4L], completionCounts);
        }

        [Fact]
        private void CompletionTrackingHttpContentReportsKnownLengthWhenDisposedWithoutReading()
        {
            var completionCounts = new List<long>();
            using var content = new CompletionTrackingHttpContent(
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("response body"))),
                completionCounts.Add);

            content.Dispose();

            Assert.Equal([13L], completionCounts);
        }

        private sealed class CaptureRequestHandler : HttpMessageHandler
        {
            public HttpRequestMessage Request { get; private set; } = null!;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
