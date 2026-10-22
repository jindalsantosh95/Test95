using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching
{
    public class ReferenceCacheHelper
    {
        private readonly IMemoryCache _cache;

        public ReferenceCacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrFetchAsync<T>(string cacheKey, Func<Task<T>> fetchFromStorage)
        {

            if (_cache.TryGetValue(cacheKey, out T cachedValue))
                return cachedValue;

            var value = await fetchFromStorage();

            if (value != null)
            {
                var hours = int.Parse(ConfigReader.Get(Constants.EnvironmentSettings.CacheExpiryHours));
                _cache.Set(cacheKey, value, TimeSpan.FromHours(hours));
            }

            return value;
        }
    }
}
