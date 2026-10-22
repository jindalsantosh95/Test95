using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching
{
    public class CacheService : ICacheService
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());

        public Task<T> GetCacheValue<T>(string key)
        {
            key = key.ToLower();
            var resultFromCache = (T)_cache.Get(key);
            return Task.FromResult(resultFromCache);
        }

        public Task SetCacheValue<T>(string key, T value, bool IsSlidingExpiration)
        {
            key = key.ToLower();
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (IsSlidingExpiration)
                cacheEntryOptions = cacheEntryOptions.SetSlidingExpiration(ConfigReader.Get<TimeSpan>(Common.Constants.Environment.CommonCachingSlidingExpiration));
            else
                cacheEntryOptions = cacheEntryOptions.SetAbsoluteExpiration(ConfigReader.Get<TimeSpan>(Common.Constants.Environment.CommonCachingAbsoluteExpiration));

            _cache.Set(key, value, cacheEntryOptions);

            return Task.CompletedTask;
        }
    }
}
