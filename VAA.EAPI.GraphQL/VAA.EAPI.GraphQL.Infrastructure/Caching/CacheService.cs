using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Infrastructure.Caching
{
    public class CacheService : ICacheService
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());

        public Task<T?> GetCacheValue<T>(string key)
        {
            key = key.ToLowerInvariant();

            _cache.TryGetValue(key, out T? value);

            return Task.FromResult(value);
        }

        public Task SetCacheValue<T>(string key, T value, bool isSlidingExpiration)
        {
            key = key.ToLowerInvariant();

            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (isSlidingExpiration)
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Common.Constants.Environment.CommonCachingSlidingExpirationInDay)));
            else
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Common.Constants.Environment.CommonCachingAbsoluteExpirationInDay)));

            _cache.Set(key, value, cacheEntryOptions);

            return Task.CompletedTask;
        }

        public Task RemoveCacheValue(string key)
        {
            key = key.ToLowerInvariant();

            _cache.Remove(key);

            return Task.CompletedTask;
        }
    }
}
