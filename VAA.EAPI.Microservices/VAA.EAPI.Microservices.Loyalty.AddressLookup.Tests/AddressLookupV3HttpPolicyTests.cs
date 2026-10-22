using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class AddressLookupV3HttpPolicyTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExistingUriLoggingFlag_DoesNotOverrideConfiguredCaching(bool synchronous)
        {
            const string uri = "https://cache.example.test/address-v3";
            ConfigReader.Set($"Cache:get:{uri}", "expiry=00:05:00");
            var cache = new CountingCache();
            var transport = new CountingHandler(HttpStatusCode.OK);
            using var client = new HttpClient(new CachingDelegatingHandler(transport, NullLogger.Instance, cache));
            for (var attempt = 0; attempt < 2; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.SuppressRequestUriLogging();
                using var response = synchronous ? client.Send(request, TestContext.Current.CancellationToken) : await client.SendAsync(request, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            Assert.Equal(2, transport.Calls);
            Assert.Equal(2, cache.Reads);
            Assert.Equal(2, cache.Writes);
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        public async Task NoWait_PreventsLocationReplay(HttpStatusCode status)
        {
            var transport = new CountingHandler(status);
            using var client = new HttpClient(new LocationDelegatingHandler(transport, NullLogger.Instance, status));
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://provider.example.test/retrieve");
            request.Headers.Add("x-nowait", "true");
            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(status, response.StatusCode);
            Assert.Equal(1, transport.Calls);
        }

        private sealed class CountingHandler(HttpStatusCode status) : HttpMessageHandler
        {
            public int Calls { get; private set; }
            protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                var response = new HttpResponseMessage(status) { RequestMessage = request, Content = new StringContent("{}") };
                response.Headers.Location = request.RequestUri;
                return response;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(Send(request, cancellationToken));
        }

        private sealed class CountingCache : IDistributedCache
        {
            public int Reads { get; private set; }
            public int Writes { get; private set; }
            public byte[]? Get(string key) { Reads++; return null; }
            public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));
            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) { Writes++; }
            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            { Set(key, value, options); return Task.CompletedTask; }
            public void Refresh(string key) { }
            public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
            public void Remove(string key) { }
            public Task RemoveAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        }
    }
}
