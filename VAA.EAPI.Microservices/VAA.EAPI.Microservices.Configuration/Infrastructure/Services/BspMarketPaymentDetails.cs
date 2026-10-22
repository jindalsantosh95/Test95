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
    public class BspMarketPaymentDetails : IBspMarketPaymentDetails
    {
        private readonly TableStorageService<BspMarketPaymentDbEntity> _tableStorageService;
        private ICacheService _cacheService;

        public BspMarketPaymentDetails(ITableStorageService<BspMarketPaymentDbEntity> tableStorageService, ICacheService iCacheService)
        {
            _tableStorageService = (TableStorageService<BspMarketPaymentDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.BspMarketPaymentDBTableName);
            _cacheService = iCacheService;
        }

        public async Task<IEnumerable<BspMarketPaymentDbEntity>> GetBspMarketPaymentList()
        {
            var CachedData = await _cacheService.GetCacheValue<IEnumerable<BspMarketPaymentDbEntity>>(Constants.Application.BspMarketPaymentCacheKey);

            if (CachedData == null)
            {
                CachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.BspMarketPaymentCacheKey, CachedData, true);
            }

            return CachedData;
        }
    }
}
