using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Diagnostics;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Core.Xunit.Http
{
    public sealed class HttpClientFactory : IHttpClientFactory
    {
        public static readonly ConcurrentDictionary<string, (HttpRequestMessage, HttpResponseMessage)> RequestResponseCache = new();

        private readonly double _httpClientTimeOutInMilliSeconds = ConfigReader.Try<double>(Common.Constants.Environment.HttpClientTimeOutInMilliSeconds, 100_000);
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly ILogger<HttpClientFactory> logger = new LoggerFactory().CreateLogger<HttpClientFactory>();
        private readonly ConcurrentDictionary<string, DefaultDelegatingHandler> _defaultDelegatingHandlers = new();

        /// <summary>
        /// Returns the list of HttpResponseMessage for the given name (url). If no responses are found, it returns an empty list.
        /// </summary>
        /// <param name="name">The name (url) for which to retrieve the responses.</param>
        /// <returns>A list of HttpResponseMessage instances.</returns>
        public IReadOnlyList<HttpResponseMessage> GetResponses(string? name = null)
        {
            if (name.IsNullOrWhiteSpace())
                name = "http://tempuri.org/";

            var uri = new Uri(name);
            var key = $"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower();

            if (_defaultDelegatingHandlers.TryGetValue(key, out var value))
                return ((MockDelegatingHandler)((CachingDelegatingHandler)((CookieDelegatingHandler)((LocationDelegatingHandler)value.InnerHandler!).InnerHandler!).InnerHandler!).InnerHandler!).Responses;

            return [];
        }

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

        public DefaultDelegatingHandler CreateHandler(string name)
        {
            if (name.IsNullOrWhiteSpace())
                name = "http://tempuri.org/";

            var uri = new Uri(name);
            var key = $"{uri.Scheme}://{uri.Host}:{uri.Port}".ToLower();

            if (_defaultDelegatingHandlers.TryGetValue(key, out DefaultDelegatingHandler? value)) return value;

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
                            InnerHandler = new CachingDelegatingHandler(logger, Caching.DistributedCache.Instance)
                            {
                                InnerHandler = new MockDelegatingHandler(logger)
                                {
                                    InnerHandler = new RequestResponseDelegatingHandler(logger, RequestResponseCache)
                                    {
                                        InnerHandler = new TerminatingDelegatingHandler()
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
                        }
                    }
                };

                _defaultDelegatingHandlers.AddOrUpdate(key, defaultDelegatingHandler, (k, v) => defaultDelegatingHandler);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _defaultDelegatingHandlers[key];
        }

        public static void RegisterRequestResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            PrepareHeaders(request);

            var key = request.BuildKey();

            if (response.RequestMessage is null)
                response.RequestMessage = request;

            (HttpRequestMessage request, HttpResponseMessage response) entry = (request, response);

            RequestResponseCache.AddOrUpdate(key, entry, (k, v) => entry);
        }

        public static void PrepareHeaders(HttpRequestMessage request, Dictionary<string, string>? headers = default, string bearerToken = "", bool preventFollow202Location = false)
        {
            // lets create a copy of the dictionary as if a shared/static
            // is passed this will cause an error with existing keys
            headers = headers?.Keys.ToDictionary(k => k, v => headers[v]) ?? new();

            var key = string.Empty;

            if (!bearerToken.IsNullOrWhiteSpace())
                request.Headers.AddOrUpdate("Authorization", $"Bearer {bearerToken}");

            request.Headers.AddOrUpdate("cache-control", "no-cache");

            if (!headers.Keys.Any(k => k.Is("Accept")))
                headers.Add("Accept", "*/*");

            request.Headers.AddOrUpdate("user-agent", "HttpClientFactory/v1.0");

            foreach (var header in headers)
                request.Headers.AddOrUpdate(header.Key, header.Value);

            request.Headers.AddOrUpdate("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, Trace<Common.Http.DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());

            request.Headers.AddOrUpdate("accept-encoding", "gzip, deflate, br");
        }
    }
}
