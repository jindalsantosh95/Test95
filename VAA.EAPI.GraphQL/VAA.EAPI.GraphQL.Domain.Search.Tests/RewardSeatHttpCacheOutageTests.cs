using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Http;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class RewardSeatHttpCacheOutageTests
    {
        [Theory]
        [InlineData("https://reward-routes-cache-outage.test/supported", "{\"source\":\"routes\"}")]
        [InlineData("https://reward-reference-cache-outage.test/airportdetail?airportCodes=LHR,JFK", "{\"source\":\"airports\"}")]
        public async Task SharedHttpCacheOutageFallsBackToLiveReferenceGet(string uri, string expectedBody)
        {
            var policyKey = "Cache:GET:" + uri.Split('?')[0];
            var previousValue = ConfigReader.Try(policyKey);
            var previousProcessValue = Environment.GetEnvironmentVariable(policyKey, EnvironmentVariableTarget.Process);
            try
            {
                ConfigReader.Set(policyKey, "expiry=00:15:00&isSliding=false");
                var cache = new FailingCache();
                var factory = new RecordingHttpClientFactory(cache);
                IHttpConnector connector = new HttpConnector(NullLogger<HttpConnector>.Instance, factory);

                for (var requestNumber = 0; requestNumber < 2; requestNumber++)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    using var response = await connector.SendAsync(request, CancellationToken.None);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(expectedBody, await response.Content.ReadAsStringAsync());
                }

                Assert.Equal(2, factory.RequestCount(uri));
                Assert.Equal(2, cache.Reads);
                Assert.Equal(2, cache.Writes);
            }
            finally
            {
                ConfigReader.Set(policyKey, previousValue ?? string.Empty);
                Environment.SetEnvironmentVariable(policyKey, previousProcessValue, EnvironmentVariableTarget.Process);
            }
        }

        private sealed class RecordingHttpClientFactory(IDistributedCache cache) : IHttpClientFactory
        {
            private readonly ConcurrentDictionary<string, int> _requests = new(StringComparer.OrdinalIgnoreCase);

            public int RequestCount(string uri) => _requests.TryGetValue(uri, out var count) ? count : 0;

            public HttpClient CreateClient(string name) => new(new CachingDelegatingHandler(
                NullLogger<CachingDelegatingHandler>.Instance, cache)
            {
                InnerHandler = new RecordingHandler(_requests)
            });
        }

        private sealed class RecordingHandler(ConcurrentDictionary<string, int> requests) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var uri = request.RequestUri!.ToString();
                requests.AddOrUpdate(uri, 1, (_, count) => count + 1);
                var body = uri.Contains("airportdetail", StringComparison.OrdinalIgnoreCase)
                    ? "{\"source\":\"airports\"}"
                    : "{\"source\":\"routes\"}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            }
        }

        private sealed class FailingCache : IDistributedCache
        {
            public int Reads { get; private set; }
            public int Writes { get; private set; }

            public byte[]? Get(string key) => throw new InvalidOperationException("Cache unavailable");

            public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            {
                Reads++;
                return Task.FromException<byte[]?>(new InvalidOperationException("Cache unavailable"));
            }

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
                throw new InvalidOperationException("Cache unavailable");

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
                CancellationToken token = default)
            {
                Writes++;
                return Task.FromException(new InvalidOperationException("Cache unavailable"));
            }

            public void Refresh(string key) => throw new InvalidOperationException("Cache unavailable");
            public Task RefreshAsync(string key, CancellationToken token = default) =>
                Task.FromException(new InvalidOperationException("Cache unavailable"));
            public void Remove(string key) => throw new InvalidOperationException("Cache unavailable");
            public Task RemoveAsync(string key, CancellationToken token = default) =>
                Task.FromException(new InvalidOperationException("Cache unavailable"));
        }
    }
}
