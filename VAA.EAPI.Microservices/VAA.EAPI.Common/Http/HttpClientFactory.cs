using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Http
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, DefaultDelegatingHandler> _defaultDelegatingHandlers = new();
        private static readonly double _httpClientTimeOutInMilliSeconds = ConfigReader.Try<double>(Constants.Environment.HttpClientTimeOutInMilliSeconds, 100_000);

        private readonly ILogger<HttpClientFactory> logger;
        private readonly IDistributedCache distributedCache;

        public HttpClientFactory(ILogger<HttpClientFactory> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public HttpClientFactory(ILogger<HttpClientFactory> logger, IDistributedCache distributedCache) : this(logger) => this.distributedCache = distributedCache;

        /// <summary>
        /// A factory component that can create <see cref="HttpClient"/> instances.
        /// </summary>
        /// <param name="name">name: target url, default when empty http://tempuri.org</param>
        /// </remarks>
        public HttpClient CreateClient(string name)
        {
            if (name.IsNullOrWhiteSpace())
                name = "http://tempuri.org/";

            var uri = new Uri(name);

            HttpClient httpClient = new(CreateHandler(name), disposeHandler: false)
            {
                BaseAddress = new Uri($"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower())
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory/v1.0");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            if (_httpClientTimeOutInMilliSeconds != httpClient.Timeout.TotalMilliseconds)
                httpClient.Timeout = TimeSpan.FromMilliseconds(_httpClientTimeOutInMilliSeconds);

            return httpClient;
        }

        private DefaultDelegatingHandler CreateHandler(string name)
        {
            if (name.IsNullOrWhiteSpace())
                name = "http://tempuri.org/";

            var uri = new Uri(name);
            var key = $"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower();
            var config = $"HttpClient:AllowAutoRedirect:{HttpUtility.UrlEncode($"{uri.Scheme}://{uri.Host}".ToLower())}";

            if (_defaultDelegatingHandlers.TryGetValue(key, out DefaultDelegatingHandler value)) return value;

            _semaphoreSlim.Wait();

            try
            {
                if (_defaultDelegatingHandlers.TryGetValue(key, out value)) return value;

                var defaultDelegatingHandler = new DefaultDelegatingHandler(logger)
                {
                    InnerHandler = new LocationDelegatingHandler(logger, HttpStatusCode.Accepted, HttpStatusCode.RequestTimeout)
                    {
                        InnerHandler = new CookieDelegatingHandler(logger)
                        {
                            InnerHandler = new CachingDelegatingHandler(logger, distributedCache)
                            {
                                InnerHandler = new MockDelegatingHandler(logger)
                                {
                                    InnerHandler = new HttpClientHandler()
                                    {
                                        AllowAutoRedirect = ConfigReader.Try(config, false),
                                        AutomaticDecompression = DecompressionMethods.All,
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                                        UseCookies = false
                                    }
                                }
                            }
                        }
                    }
                };

                _defaultDelegatingHandlers.AddOrUpdate(key, defaultDelegatingHandler, (k, v) => defaultDelegatingHandler);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _defaultDelegatingHandlers[key];
        }
    }
}
