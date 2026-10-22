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
    public class PccCurrencyDetails : IPccCurrencyDetails
    {
        private readonly TableStorageService<PccCurrencyDbEntity> _tableStorageService;
        private ICacheService _cacheService;

        public PccCurrencyDetails(ITableStorageService<PccCurrencyDbEntity> tableStorageService, ICacheService iCacheService)
        {
            _tableStorageService = (TableStorageService<PccCurrencyDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.PccCurrencyDBTableName);
            _cacheService = iCacheService;
        }

        public async Task<IEnumerable<PccCurrencyDbEntity>> GetPccCurrencyList()
        {
            var CachedData = await _cacheService.GetCacheValue<IEnumerable<PccCurrencyDbEntity>>(Constants.Application.PccCurrencyCacheKey);

            if (CachedData == null)
            {
                CachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.PccCurrencyCacheKey, CachedData, true);
            }

            return CachedData;
        }
    }
}
