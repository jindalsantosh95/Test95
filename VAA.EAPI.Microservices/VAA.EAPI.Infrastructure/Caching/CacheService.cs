using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Caching
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
                cacheEntryOptions = cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay)));
            else
                cacheEntryOptions = cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));
            _cache.Set(key, value, cacheEntryOptions);
            return Task.CompletedTask;
        }
        public Task RemoveCacheValue<T>(string key)
        {
            key = key.ToLower();
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
