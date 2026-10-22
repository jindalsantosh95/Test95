using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 100)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, IterationsColumn]
    public class HttpMockCreateResponseMessageBenchmark
    {
        private HttpRequestMessage _request = null!;
        private string[] _responseLines = [];

        [Params(1, 5, 16)]
        public int ResponseSizeMb { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _request = new HttpRequestMessage(HttpMethod.Get, "https://tempuri.org/test");

            var bodySizeInBytes = ResponseSizeMb * 1024 * 1024;
            var body = CreateAsciiBody(bodySizeInBytes);

            var rawResponse =
                $"HTTP/1.1 200 OK{Environment.NewLine}" +
                $"Content-Type: application/json{Environment.NewLine}" +
                $"X-Benchmark: HttpMock{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"{body}";

            _responseLines = rawResponse.Split(Environment.NewLine);
        }

        [Benchmark]
        public HttpResponseMessage CreateResponseMessage()
        {
            return HttpMock.CreateResponseMessage(_responseLines, _request, out _);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _request.Dispose();
        }

        private static string CreateAsciiBody(int sizeInBytes)
        {
            if (sizeInBytes <= 0) return string.Empty;

            return new string('a', sizeInBytes);
        }
    }
}