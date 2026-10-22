using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Common.Http
{
    public sealed class CachingDelegatingHandler : DelegatingHandler
    {
        private readonly IDistributedCache distributedCache;
        private readonly ILogger logger;

        public CachingDelegatingHandler(ILogger logger, IDistributedCache distributedCache) : base()
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.distributedCache = distributedCache;
        }
        public CachingDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger, IDistributedCache distributedCache) : base(innerHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.distributedCache = distributedCache;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isCacheEnabled = ConfigReader.Caching.IsCacheEnabled(request, out string key) && distributedCache != null;

            if (isCacheEnabled)
            {
                try
                {
                    var cachedValue = distributedCache.Get(key);

                    if (!cachedValue.IsNullOrEmpty())
                        return BuildHttpResponseMessage(cachedValue);
                }
                catch (Exception ex)
                {
                    /// The reason we catch the exception here instead of bubbling it 
                    /// up is that we don't want the performance enhancement of caching 
                    /// to be a critical part. If it's not available for what ever 
                    /// reason we carry on with normal execution

                    logger.LogError(ex, $"{nameof(CachingDelegatingHandler)}-Exception, Read: {ex.Message}");
                }
            }

            var response = base.Send(request, cancellationToken);

            if (ConfigReader.Caching.ShouldCache(response, out TimeSpan expiry, out bool isSliding))
                _ = CacheResponse(distributedCache, response, key, isSliding, expiry, logger, cancellationToken);

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isCacheEnabled = ConfigReader.Caching.IsCacheEnabled(request, out string key) && distributedCache != null;

            if (isCacheEnabled)
            {
                try
                {
                    var cachedValue = await distributedCache.GetAsync(key, cancellationToken);

                    if (!cachedValue.IsNullOrEmpty())
                        return BuildHttpResponseMessage(cachedValue);
                }
                catch (Exception ex)
                {
                    /// The reason we catch the exception here instead of bubbling it 
                    /// up is that we don't want the performance enhancement of caching 
                    /// to be a critical part. If it's not available for what ever 
                    /// reason we carry on with normal execution

                    logger.LogError(ex, $"{nameof(CachingDelegatingHandler)}-Exception, Read: {ex.Message}");
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (ConfigReader.Caching.ShouldCache(response, out TimeSpan expiry, out bool isSliding))
                _ = CacheResponse(distributedCache, response, key, isSliding, expiry, logger, cancellationToken);

            return response;
        }


        private static HttpResponseMessage BuildHttpResponseMessage(byte[] source)
        {
            var cachedResponse = source.DecompressTo<CachedItem>();
            var requestHeaders = HttpUtility.ParseQueryString(cachedResponse.Request.Headers);
            var responseHeaders = HttpUtility.ParseQueryString(cachedResponse.Response.Headers);

            HttpResponseMessage httpResponseMessage = new(cachedResponse.Response.StatusCode)
            {
                RequestMessage = new HttpRequestMessage(new HttpMethod(cachedResponse.Request.Method.ToString()), cachedResponse.Request.Uri)
            };

            foreach (var header in requestHeaders.AllKeys)
                httpResponseMessage.RequestMessage.Headers.TryAddWithoutValidation(header, requestHeaders[header]);

            httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Request.Body);

            foreach (var header in responseHeaders.AllKeys)
                httpResponseMessage.Headers.TryAddWithoutValidation(header, responseHeaders[header]);

            httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Response.Body);

            httpResponseMessage.Headers.TryAddWithoutValidation("x-data-source", "CACHED");

            return httpResponseMessage;
        }

        private static async Task CacheResponse(IDistributedCache distributedCache, HttpResponseMessage response, string key, bool isSliding, TimeSpan expiry, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                CachedItem cacheItem = (CachedItem)response;

                DistributedCacheEntryOptions options = isSliding ? new() { SlidingExpiration = expiry } : new() { AbsoluteExpirationRelativeToNow = expiry };

                await distributedCache.SetAsync(key, Json.Serialize(cacheItem).Compress(), options, cancellationToken);
            }
            catch (Exception ex)
            {
                /// The reason we catch the exception here instead of bubbling it 
                /// up is that we don't want the performance enhancement of caching 
                /// to be a critical part. If it's not available for what ever 
                /// reason we carry on with normal execution

                logger.LogError(ex, $"{nameof(CachingDelegatingHandler)}-Exception, Set: {ex.Message}");
            }
        }


        private sealed class CachedItemRequest
        {
            public Uri Uri { get; set; }
            public string Headers { get; set; }
            public HttpMethod Method { get; set; }
            public byte[] Body { get; set; }
        }

        private sealed class CachedItemResponse
        {
            public string Headers { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public byte[] Body { get; set; }
        }

        private sealed class CachedItem
        {
            public static explicit operator CachedItem(HttpResponseMessage source) => new()
            {
                Request = new()
                {
                    Uri = source.RequestMessage.RequestUri,
                    Headers = string.Join('&', source.RequestMessage.Headers.Select(x => $"{x.Key}={HttpUtility.UrlEncode(string.Join(',', x.Value))}")),
                    Method = source.RequestMessage.Method,
                    Body = source.RequestMessage.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult() ?? Array.Empty<byte>()
                },
                Response = new()
                {
                    Headers = string.Join('&', source.Headers.Select(x => $"{x.Key}={HttpUtility.UrlEncode(string.Join(',', x.Value))}")),
                    StatusCode = source.StatusCode,
                    Body = source.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                },
            };

            public CachedItemRequest Request { get; set; }
            public CachedItemResponse Response { get; set; }
        }
    }
}
