using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services
{
    public class CachedAmenities : ICachedAmenities
    {
        private readonly IMemoryCache _cache;
        private readonly TableStorageService<AmenitiesTableEntity> _tableStorageService;
        private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

        public CachedAmenities(IMemoryCache cache, ITableStorageService<AmenitiesTableEntity> tableStorageService)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tableStorageService = (TableStorageService<AmenitiesTableEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.AmenitiesTableName);
        }
        public virtual async Task<List<AmenitiesTableEntity>> GetAmenitiesInformation(string codeshare)
        {
            List<AmenitiesTableEntity> amenitiesData;

            await SemaphoreSlim.WaitAsync();

            try
            {
                _cache.TryGetValue(codeshare, out amenitiesData);

                if (amenitiesData is null)
                {
                    amenitiesData = await _tableStorageService.RetrieveItemsAsync(default);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingSlidingExpirationTime))
                    .SetAbsoluteExpiration(ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingAbsoluteExpirationTime));

                    _cache.Set(codeshare, amenitiesData, cacheEntryOptions);
                }
            }
            finally
            {
                SemaphoreSlim.Release();

            }
            return amenitiesData;
        }
    }
}
