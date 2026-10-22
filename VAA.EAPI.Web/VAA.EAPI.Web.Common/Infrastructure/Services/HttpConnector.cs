using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Web.Http;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Interfaces;

namespace VAA.EAPI.Web.Common.Infrastructure.Services
{
    public sealed class HttpConnector : IHttpConnector
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, HttpClient> _httpClients = new();
        private static readonly double _httpClientTimeOutInMilliSeconds = ConfigReader.Try<double>(Common.Constants.Environment.HttpClientTimeOutInMilliSeconds, 100_000);

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<HttpConnector> _logger;

        public HttpConnector(ILogger<HttpConnector> logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public HttpConnector(ILogger<HttpConnector> logger, IDistributedCache distributedCache) : this(logger) => _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

        private static async Task<HttpClient> GetHttpClientAsync(Uri uri, ILogger logger, IDistributedCache distributedCache)
        {
            var key = $"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower();

            if (_httpClients.ContainsKey(key)) return _httpClients[key];

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_httpClients.ContainsKey(key)) return _httpClients[key];

                HttpClient httpClient = new(new DefaultDelegatingHandler(logger)
                {
                    InnerHandler = new AcceptedDelegatingHandler(logger)
                    {
                        InnerHandler = new RedirectDelegatingHandler(logger)
                        {
                            InnerHandler = new CookieDelegatingHandler(logger)
                            {
                                InnerHandler = new CachingDelegatingHandler(logger, distributedCache)
                                {
                                    InnerHandler = new HttpClientHandler()
                                    {
                                        AllowAutoRedirect = false,
                                        AutomaticDecompression = DecompressionMethods.All,
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                                        UseCookies = false
                                    }
                                }
                            }
                        }
                    }
                }, false)
                {
                    BaseAddress = new Uri(key)
                };

                httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpConnector/v1.0");
                httpClient.DefaultRequestHeaders.Add("loadTest", "true");

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

        #region Verb-specific Helpers
        public async Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken) => await SendAsync(uri, headers, bearerToken, HttpMethod.Get, string.Empty);
        public async Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers, string bearerToken) => await SendAsync(uri, headers, bearerToken, HttpMethod.Delete, string.Empty);
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="uri">Endpoint to send request to</param>
        /// <param name="headers">Any specific headers for downstream service</param>
        /// <param name="bearerToken">Bearer Token for downstream system authorization</param>
        /// <param name="httpMethod">HTTP Verb: GET, POST etc.</param>
        /// <param name="content">Body content</param>
        /// <returns></returns>
        /// <exception cref="EAPIHttpRequestException"></exception>
        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, httpMethod, content);

            return await SendAsync(httpRequestMessage, CancellationToken.None, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, httpMethod, content);

            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync(httpRequestMessage, CancellationToken.None, noWait);
        }

        private static HttpRequestMessage BuildHttpRequestMessage(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content)
        {
            headers ??= new Dictionary<string, string>();

            // lets create a copy of the dictionary as if a shared/static
            // is passed this will cause an error with existing keys
            headers = headers.Keys.ToDictionary(k => k, v => headers[v]);

            if (!bearerToken.IsNullOrWhiteSpace())
                if (headers.Any(x => x.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                    headers["Authorization"] = $"Bearer {bearerToken}";
                else
                    headers.Add("Authorization", $"Bearer {bearerToken}");

            if (headers.Any(x => x.Key.Equals("cache-control", StringComparison.OrdinalIgnoreCase)))
                headers["cache-control"] = "no-cache";
            else
                headers.Add("cache-control", "no-cache");

            HttpRequestMessage httpRequestMessage = new(httpMethod, uri);

            foreach (var header in headers)
                httpRequestMessage.Headers.Add(header.Key, header.Value);

            if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Delete)
                httpRequestMessage.Content = new StringContent(JsonConvert.DeserializeObject<JToken>(content).ToString(Formatting.None), Encoding.UTF8, "application/json");

            return httpRequestMessage;
        }
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false) => await SendAsync(await GetHttpClientAsync(httpRequestMessage.RequestUri, _logger, _distributedCache), httpRequestMessage, cancellationToken, noWait);

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync(httpRequestMessage, cancellationToken, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation("x-nowait", noWait.ToString());
            httpRequestMessage.Headers.TryAddWithoutValidation("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());

            var trace = _logger.CreateTrace();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

            if (!(httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.RedirectMethod))
            {
                var exception = new HttpRequestException("Http connector failed.", new HttpResponseException(httpResponseMessage), httpResponseMessage.StatusCode);

                exception.Data.Add("RequestUri", httpResponseMessage.RequestMessage.RequestUri);
                exception.Data.Add("StatusCode", httpResponseMessage.StatusCode.ToString());
                exception.Data.Add("ReasonPhrase", httpResponseMessage.ReasonPhrase);
                exception.Data.Add("Response", await httpResponseMessage.Content.ReadAsStringAsync());

                trace.Log.LogError(exception, $"{{0}}> {exception.Message}, {{2}}ms");

                throw exception;
            }

            trace.Diagnostics.LogInformation($"{{0}}{httpRequestMessage.Method}:{httpRequestMessage.RequestUri}:{httpResponseMessage.Content.Headers.ContentLength}:SendAsync executed in {{7}}ms");

            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync(httpClient, httpRequestMessage, cancellationToken, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            trace.SetHashCode(request.GetHashCode());

            request.Headers.TryAddWithoutValidation("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());

            var httpClient = await GetHttpClientAsync(request.RequestUri, _logger, _distributedCache);

            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request, cancellationToken);

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{httpResponseMessage.Content.Headers.ContentLength}:SendAsync executed in {{7}}ms");

            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
        {
            request.SetCookieContainer(cookies);

            return await SendAsync(request, cancellationToken);
        }
    }
}
