using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class CachedAmenities : ICachedAmenities
    {
        private readonly IMemoryCache _cache;
        private readonly TableStorageService<AmenitiesTableEntity> _tableStorageService;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

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

            if (_cache.TryGetValue(codeshare, out amenitiesData)) return amenitiesData;

            await semaphoreSlim.WaitAsync();

            try
            {
                if (_cache.TryGetValue(codeshare, out amenitiesData)) return amenitiesData;

                amenitiesData = await _tableStorageService.RetrieveItemsAsync(default);

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay)))
                .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));

                _cache.Set(codeshare, amenitiesData, cacheEntryOptions);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return amenitiesData;
        }
    }
}
