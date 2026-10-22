using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Infrastructure.Caching
{
    public sealed class DistributedCacheService : IDistributedCacheService
    {
        private static readonly ConcurrentDictionary<string, System.Timers.Timer> circuitBreakers = new();

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCacheService> _logger;

        public DistributedCacheService(IDistributedCache distributedCache, ILogger<DistributedCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> Get<T>(string key)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return default;

            try
            {
                var cachedResponse = await _distributedCache.GetAsync(key);

                if (cachedResponse != null)
                {
                    await _distributedCache.RefreshAsync(key).ConfigureAwait(false);

                    return cachedResponse.DecompressTo<T>();
                }
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogError(ex, ex.Message);

                EnableCircuitBreaker(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // suppress any errors so we can carry on with execution
            }

            return default;
        }

        public async Task Set<T>(string key, T value, bool isSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime)
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

                await _distributedCache.SetAsync(key, JsonConvert.SerializeObject(value).Compress(), distributedCacheEntryOptions).ConfigureAwait(false);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogError(ex, ex.Message);

                EnableCircuitBreaker(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // suppress any errors so we can carry on with execution
            }
        }

        public async Task Refresh(string key)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return;

            try
            {
                await _distributedCache.RefreshAsync(key);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogError(ex, ex.Message);

                EnableCircuitBreaker(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // suppress any errors so we can carry on with execution
            }
        }

        public async Task Remove(string key)
        {
            key = key.ToLower();

            if (IsCircuitBreakerEnabled(key)) return;

            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogError(ex, ex.Message);

                EnableCircuitBreaker(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // suppress any errors so we can carry on with execution
            }
        }

        private static void EnableCircuitBreaker(string key)
        {
            key = key.ToLower();

            if (circuitBreakers.TryGetValue(key, out var existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Start();
            }
            else
            {
                var timeout = ConfigReader.Try(Common.Constants.Application.CacheConnectionTimeout, 30_000);
                var timer = new System.Timers.Timer(timeout)
                {
                    AutoReset = false
                };
                timer.Elapsed += (sender, e) =>
                {
                    timer.Stop();
                    circuitBreakers.TryRemove(key, out _);
                    timer.Dispose();
                };
                if (circuitBreakers.TryAdd(key, timer))
                {
                    timer.Start();
                }
            }
        }

        private static bool IsCircuitBreakerEnabled(string key) => circuitBreakers.TryGetValue(key.ToLower(), out System.Timers.Timer value) && value.Enabled;
    }
}
