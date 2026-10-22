using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services
{
    public class CachedRFICData : ICachedRFICData
    {
        private readonly IMemoryCache _cache;
        private readonly TableStorageService<RFICTableEntity> _tableStorageService;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public CachedRFICData(IMemoryCache cache, ITableStorageService<RFICTableEntity> tableStorageService)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tableStorageService = (TableStorageService<RFICTableEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.RFICTableName);
        }

        public async virtual Task<RFICTableEntity> GetCachedRFICData(string rowKey)
        {
            RFICTableEntity rficData;
            //Using SemaphoreSlim here as lock doesn't allow async calls
            await semaphoreSlim.WaitAsync();

            try
            {
                _cache.TryGetValue(rowKey, out rficData);

                if (rficData is null)
                {
                    rficData = await _tableStorageService.RetrieveAsync(Constants.Application.RFICTablePartitionKey, rowKey);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay))).SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));
                    _cache.Set(rowKey, rficData, cacheEntryOptions);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return rficData;
        }
    }
}
