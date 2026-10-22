using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Serialisation;

namespace VAA.EAPI.Automation.Core.Http
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
            bool isCacheEnabled = ConfigReader.Caching.IsCacheEnabled(request, out string key, out TimeSpan redisCacheExpiry, out bool isSliding) && distributedCache != null;

            if (isCacheEnabled)
            {
                try
                {
                    byte[] cachedValue = distributedCache?.Get(key);

                    if (!cachedValue.IsNullOrEmpty())
                    {
                        VAA.EAPI.Automation.Core.Http.CachingDelegatingHandler.CachedItem cachedResponse = cachedValue.DecompressTo<CachedItem>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                distributedCache.Refresh(key);
                            }
                            catch
                            {
                                /// The reason we catch the exception here instead of bubbling it 
                                /// up is that we don't want the performance enhancement of caching 
                                /// to be a critical part. If it's not available for what ever 
                                /// reason we carry on with normal execution
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        NameValueCollection requestHeaders = HttpUtility.ParseQueryString(cachedResponse.Request.Headers);
                        NameValueCollection responseHeaders = HttpUtility.ParseQueryString(cachedResponse.Response.Headers);

                        HttpResponseMessage httpResponseMessage = new(cachedResponse.Response.StatusCode);

                        httpResponseMessage.RequestMessage = new HttpRequestMessage(new HttpMethod(cachedResponse.Request.Method.ToString()), cachedResponse.Request.Uri);

                        foreach (string header in requestHeaders.AllKeys)
                            httpResponseMessage.RequestMessage.Headers.TryAddWithoutValidation(header, requestHeaders[header]);

                        httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Request.Body);

                        foreach (string header in responseHeaders.AllKeys)
                            httpResponseMessage.Headers.TryAddWithoutValidation(header, responseHeaders[header]);

                        httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Response.Body);

                        return httpResponseMessage;
                    }
                }
                catch
                {
                    /// The reason we catch the exception here instead of bubbling it 
                    /// up is that we don't want the performance enhancement of caching 
                    /// to be a critical part. If it's not available for what ever 
                    /// reason we carry on with normal execution
                }
            }

            System.Net.Http.HttpResponseMessage response = base.Send(request, cancellationToken);

            if (isCacheEnabled && response.IsSuccessStatusCode)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        CachedItem cacheItem = (CachedItem)response;

                        distributedCache?.Set(key, Json.Serialize(cacheItem).Compress(), new DistributedCacheEntryOptions() { SlidingExpiration = redisCacheExpiry });
                    }
                    catch
                    {
                        /// The reason we catch the exception here instead of bubbling it 
                        /// up is that we don't want the performance enhancement of caching 
                        /// to be a critical part. If it's not available for what ever 
                        /// reason we carry on with normal execution
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isCacheEnabled = ConfigReader.Caching.IsCacheEnabled(request, out string key, out TimeSpan expiry, out bool isSliding) && distributedCache != null;

            if (isCacheEnabled)
            {
                try
                {
                    byte[] cachedValue = distributedCache?.Get(key);

                    if (!cachedValue.IsNullOrEmpty())
                    {
                        VAA.EAPI.Automation.Core.Http.CachingDelegatingHandler.CachedItem cachedResponse = cachedValue.DecompressTo<CachedItem>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                distributedCache.Refresh(key);
                            }
                            catch
                            {
                                /// The reason we catch the exception here instead of bubbling it 
                                /// up is that we don't want the performance enhancement of caching 
                                /// to be a critical part. If it's not available for what ever 
                                /// reason we carry on with normal execution
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        NameValueCollection requestHeaders = HttpUtility.ParseQueryString(cachedResponse.Request.Headers);
                        NameValueCollection responseHeaders = HttpUtility.ParseQueryString(cachedResponse.Response.Headers);

                        HttpResponseMessage httpResponseMessage = new(cachedResponse.Response.StatusCode);

                        httpResponseMessage.RequestMessage = new HttpRequestMessage(new HttpMethod(cachedResponse.Request.Method.ToString()), cachedResponse.Request.Uri);

                        foreach (string header in requestHeaders.AllKeys)
                            httpResponseMessage.RequestMessage.Headers.TryAddWithoutValidation(header, requestHeaders[header]);

                        httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Request.Body);

                        foreach (string header in responseHeaders.AllKeys)
                            httpResponseMessage.Headers.TryAddWithoutValidation(header, responseHeaders[header]);

                        httpResponseMessage.Content = new ByteArrayContent(cachedResponse.Response.Body);

                        return httpResponseMessage;
                    }
                }
                catch
                {
                    /// The reason we catch the exception here instead of bubbling it 
                    /// up is that we don't want the performance enhancement of caching 
                    /// to be a critical part. If it's not available for what ever 
                    /// reason we carry on with normal execution
                }
            }

            System.Net.Http.HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (isCacheEnabled && response.IsSuccessStatusCode)
            {
                CachedItem cacheItem = (CachedItem)response;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        DistributedCacheEntryOptions options = isSliding ? new() { SlidingExpiration = expiry } : new() { AbsoluteExpirationRelativeToNow = expiry };

                        distributedCache?.Set(key, Json.Serialize(cacheItem).Compress(), options);
                    }
                    catch
                    {
                        /// The reason we catch the exception here instead of bubbling it 
                        /// up is that we don't want the performance enhancement of caching 
                        /// to be a critical part. If it's not available for what ever 
                        /// reason we carry on with normal execution
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return response;
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
