using Microsoft.Azure.Cosmos.Table;
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
    public class CachedCabinType : ICachedCabinType
    {
        private readonly IMemoryCache _cache;
        private readonly TableStorageService<CabinTypeDataEntity> _tableStorageService;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        public CachedCabinType(IMemoryCache cache, ITableStorageService<CabinTypeDataEntity> tableStorageService)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tableStorageService = (TableStorageService<CabinTypeDataEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.CabinTypeTableName);
        }
        public async virtual Task<List<CabinTypeDataEntity>> GetCabinTypeData(string compartmentNumber)
        {
            List<CabinTypeDataEntity> cabinType = null;
            await semaphoreSlim.WaitAsync();
            try
            {
                _cache.TryGetValue(compartmentNumber, out cabinType);

                if (cabinType is null)
                {
                    cabinType = await _tableStorageService.RetrieveItemsAsync(default);
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingSlidingExpirationTime))
                    .SetAbsoluteExpiration(ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingAbsoluteExpirationTime));
                    _cache.Set(compartmentNumber, cabinType, cacheEntryOptions);
                }
            }
            finally
            {
                semaphoreSlim.Release();

            }
            return cabinType;
        }
    }
}



