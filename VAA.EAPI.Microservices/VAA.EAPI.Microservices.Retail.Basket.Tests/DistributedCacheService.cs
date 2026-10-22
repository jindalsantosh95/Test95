using System.Collections.Concurrent;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    internal sealed class DistributedCacheService : IDistributedCacheService, IDisposable
    {
        private ConcurrentDictionary<string, object>? cache = new();

        public void Dispose()
        {
            foreach (var item in cache.Where(x => x.Value is IDisposable))
                ((IDisposable)item.Value).Dispose();

            cache = null;
        }

        public async Task<T> Get<T>(string key)
        {
            if (!cache.ContainsKey(key)) return default;

            return (T)cache[key];
        }

        public Task Refresh(string key)
        {
            return Task.CompletedTask;
        }

        public async Task Remove(string key)
        {
            if (!cache.ContainsKey(key)) return;

            cache.TryRemove(key, out _);
        }

        public async Task Set<T>(string key, T value, bool IsSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime)
        {
            if (cache.ContainsKey(key))
            {
                if (cache.TryGetValue(key, out object current))
                    cache.TryUpdate(key, value, current);
            }
            else
                cache.TryAdd(key, value);
        }
    }
}
