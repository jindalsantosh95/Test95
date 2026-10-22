using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services
{
    public class CachedAirport : ICachedAirport
    {
        private readonly IMemoryCache _cache;
        private readonly TableStorageService<AirportLocation> _tableStorageService;

        public CachedAirport(IMemoryCache cache, ITableStorageService<AirportLocation> tableStorageService)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tableStorageService = (TableStorageService<AirportLocation>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReferenceDataTableName);
        }

        public async virtual Task<AirportLocation> GetCachedAirport(string rowKey)
        {
            _cache.TryGetValue(rowKey, out AirportLocation airportLocation);

            if (airportLocation == null)
            {
                airportLocation = await _tableStorageService.RetrieveAsync(Constants.Application.PartitionKey, rowKey);

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay)))
                    .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));
                _cache.Set(rowKey, airportLocation, cacheEntryOptions);
            }

            return airportLocation;
        }
    }
}
