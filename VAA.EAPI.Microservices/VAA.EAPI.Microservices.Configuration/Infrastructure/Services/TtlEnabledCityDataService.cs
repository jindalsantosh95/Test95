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
    public class TtlEnabledCityDataService : ITtlEnabledCityData
    {
        private readonly TableStorageService<TtlEnabledCityDbEntity> _tableStorageService;
        private readonly ICacheService _cacheService;

        public TtlEnabledCityDataService(ITableStorageService<TtlEnabledCityDbEntity> tableStorageService, ICacheService cacheService)
        {
            _tableStorageService = (TableStorageService<TtlEnabledCityDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.TtlEnabledCityDBTableName);
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<TtlEnabledCityDbEntity>> GetTtlEnabledCityData()
        {
            var cachedData = await _cacheService.GetCacheValue<IEnumerable<TtlEnabledCityDbEntity>>(Constants.Application.TtlEnabledCityCacheKey);

            if (cachedData == null)
            {
                cachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.TtlEnabledCityCacheKey, cachedData, true);
            }
            return cachedData;
        }
    }
}
