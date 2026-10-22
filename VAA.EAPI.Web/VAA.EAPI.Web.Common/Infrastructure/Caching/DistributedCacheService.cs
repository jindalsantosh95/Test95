using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.Json;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Infrastructure.Caching
{
    public sealed class DistributedCacheService : IDistributedCacheService
    {
        private static readonly ConcurrentDictionary<string, System.Timers.Timer> circuitBreakers = new();

        private readonly IDistributedCache _distributedCache;

        public DistributedCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> GetCacheData<T>(string key)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return default;

            try
            {
                var cachedResponse = await _distributedCache.GetStringAsync(key);

                if (cachedResponse != null)
                    return JsonSerializer.Deserialize<T>(cachedResponse);
            }
            catch (StackExchange.Redis.RedisConnectionException)
            {
                EnableCircuitBreaker(key);
            }
            catch
            {
                // suppress any errors so we can carry on with execution
            }

            return default;
        }

        public async Task RemoveCacheData(string key)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return;

            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (StackExchange.Redis.RedisConnectionException)
            {
                EnableCircuitBreaker(key);
            }
            catch
            {
                // suppress any errors so we can carry on with execution
            }
        }

        public async Task SetCacheData<T>(string key, T value, bool isSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return;

            try
            {
                var distributedCacheEntryOptions = new DistributedCacheEntryOptions();

                if (isSlidingExpiration)
                    distributedCacheEntryOptions.SetSlidingExpiration(slidingExpirationTime);
                else
                    distributedCacheEntryOptions.SetAbsoluteExpiration(absoluteExpirationTime);

                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), distributedCacheEntryOptions);
            }
            catch (StackExchange.Redis.RedisConnectionException)
            {
                EnableCircuitBreaker(key);
            }
            catch
            {
                // suppress any errors so we can carry on with execution
            }
        }

        private static void EnableCircuitBreaker(string key)
        {
            key = key.ToLower();

            if (circuitBreakers.ContainsKey(key))
            {
                circuitBreakers[key].Stop();
                circuitBreakers[key].Start();
            }
            else if (circuitBreakers.TryAdd(key, new System.Timers.Timer(ConfigReader.Try(Constants.Application.CacheConnectionTimeout, 30_000))))
                circuitBreakers[key].Elapsed += (sender, e) => ((System.Timers.Timer)sender).Stop();
        }

        private static bool IsCircuitBreakerEnabled(string key)
        {
            key = key.ToLower();

            if (!circuitBreakers.ContainsKey(key)) return false;

            return circuitBreakers[key].Enabled;
        }
    }
}
