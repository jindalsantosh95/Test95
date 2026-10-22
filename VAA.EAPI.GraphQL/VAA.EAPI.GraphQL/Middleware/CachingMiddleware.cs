using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Middleware
{
    public sealed class CachingMiddleware : IFunctionsWorkerMiddleware
    {
        IDistributedCache distributedCache;

        public CachingMiddleware(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context.FunctionDefinition.Name == nameof(Triggers.Http.NativeGraphQLBridge))
            {
                await next(context);
                return;
            }

            if (!ConfigReader.Caching.IsCacheEnabled(context, out string key))
            {
                await next(context);

                return;
            }

            var log = context.GetLogger("HttpFunction");
            var httpRequestData = await context.GetHttpRequestDataAsync();

            try
            {
                var cachedValue = await distributedCache.GetAsync(key);

                if (!cachedValue.IsNullOrEmpty())
                {
                    var response = httpRequestData.CreateResponse(System.Net.HttpStatusCode.OK);

                    response.Body = new MemoryStream(cachedValue.Decompress())
                    {
                        Position = 0
                    };

                    response.Headers.TryAddWithoutValidation("content-type", "application/json");

                    context.SetHttpResponseData(response);

                    return;
                }
            }
            catch (Exception ex)
            {
                /// The reason we catch the exception here instead of bubbling it 
                /// up is that we don't want the performance enhancement of caching 
                /// to be a critical part. If it's not available for what ever 
                /// reason we log it and carry on with normal execution

                log.LogError(ex, $"{nameof(CachingMiddleware)}-Exception, Read: {ex.Message}");
            }

            await next(context);

            key = ConfigReader.Caching.BuildKey(context);

            try
            {
                // as this could be a large string value we don't want to out it
                // as this would create a copy, by using ref we are only ever
                // pointing to single instance
                string content = null;

                if (ConfigReader.Caching.ShouldCache(context, ref content, out var expiry, out var isSliding))
                {
                    DistributedCacheEntryOptions options = isSliding ? new() { SlidingExpiration = expiry } : new() { AbsoluteExpirationRelativeToNow = expiry };

                    _ = distributedCache.SetAsync(key, content.Compress(), options);
                }
            }
            catch (Exception ex)
            {
                /// The reason we catch the exception here instead of bubbling it 
                /// up is that we don't want the performance enhancement of caching 
                /// to be a critical part. If it's not available for what ever 
                /// reason we log it and carry on with normal execution

                log.LogError(ex, $"{nameof(CachingMiddleware)}-Exception, Write: {ex.Message}");
            }
        }
    }
}
