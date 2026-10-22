using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class BspMarketCurrencyDetails : IBspMarketCurrencyDetails
    {
        private readonly TableStorageService<BspMarketCurrencyDbEntity> _tableStorageService;
        private ICacheService _cacheService;

        public BspMarketCurrencyDetails(ITableStorageService<BspMarketCurrencyDbEntity> tableStorageService, ICacheService iCacheService)
        {
            _tableStorageService = (TableStorageService<BspMarketCurrencyDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.BspMarketCurrencyDBTableName);
            _cacheService = iCacheService;
        }

        public async Task<IEnumerable<BspMarketCurrencyDbEntity>> GetBspMarketCurrencyList()
        {
            var CachedData = await _cacheService.GetCacheValue<IEnumerable<BspMarketCurrencyDbEntity>>(Constants.Application.BspMarketCurrencyCacheKey);

            if (CachedData == null)
            {
                CachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.BspMarketCurrencyCacheKey, CachedData, true);
            }

            return CachedData;
        }
    }
}
