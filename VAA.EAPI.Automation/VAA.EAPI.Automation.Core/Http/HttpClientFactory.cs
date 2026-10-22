using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Http
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, HttpClient> _httpClients = new();
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

            Uri uri = new Uri(name);
            string key = $"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower();

            if (_httpClients.ContainsKey(key))
                return _httpClients[key];

            _semaphoreSlim.Wait();

            try
            {
                if (_httpClients.ContainsKey(key))
                    return _httpClients[key];

                HttpClient httpClient = new(new DefaultDelegatingHandler(logger)
                {
                    InnerHandler = new CachingDelegatingHandler(logger, distributedCache)
                    {
                        InnerHandler = new LocationDelegatingHandler(logger, HttpStatusCode.Accepted, HttpStatusCode.RequestTimeout)
                        {
                            InnerHandler = new HttpClientHandler()
                            {
                                AllowAutoRedirect = false,
                                AutomaticDecompression = DecompressionMethods.All,
                                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                            }
                        }
                    }
                }, false)
                {
                    BaseAddress = new Uri(key)
                };

                httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory/v1.0");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                if (_httpClientTimeOutInMilliSeconds != httpClient.Timeout.TotalMilliseconds)
                    httpClient.Timeout = TimeSpan.FromMilliseconds(_httpClientTimeOutInMilliSeconds);

                _httpClients.AddOrUpdate(key, httpClient, (k, v) => httpClient);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _httpClients[key];
        }
    }
}
