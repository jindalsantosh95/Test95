using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, IterationsColumn]
    public class MockDelegatingHandlerBenchmark
    {
        private const string RequestUri = "https://tempuri.org/large-response";

        private HttpMessageInvoker _invoker = null!;

        [Params(1, 16)]
        public int ResponseSizeMb { get; set; }

        [Params(1, 8, 32)]
        public int ConcurrentRequests { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            ConfigReader.Set("MockDelegatingHandler:Enabled", "true");
            ConfigReader.Set("MockDelegatingHandler:MockServiceUri", "https://tempuri.org/mock");

            var body = Encoding.UTF8.GetBytes(new string('a', ResponseSizeMb * 1024 * 1024));
            var handler = new MockDelegatingHandler(NullLogger.Instance)
            {
                InnerHandler = new LargeMockServiceHandler(body)
            };

            _invoker = new HttpMessageInvoker(handler);

            using var seedRequest = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            using var seedResponse = _invoker.SendAsync(seedRequest, CancellationToken.None).GetAwaiter().GetResult();
        }

        [Benchmark]
        public Task SendCachedResponsesUnderLoad() =>
            Task.WhenAll(Enumerable.Range(0, ConcurrentRequests).Select(_ => SendAndConsumeResponseAsync()));

        [GlobalCleanup]
        public void Cleanup() => _invoker.Dispose();

        private async Task SendAndConsumeResponseAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            using var response = await _invoker.SendAsync(request, CancellationToken.None);

            await response.Content.CopyToAsync(Stream.Null);
        }

        private sealed class LargeMockServiceHandler(byte[] body) : HttpMessageHandler
        {
            private readonly byte[] _body = body;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
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
