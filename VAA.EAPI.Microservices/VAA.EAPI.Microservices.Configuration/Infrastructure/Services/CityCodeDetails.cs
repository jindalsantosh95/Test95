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
    public class CityCodeDetails : ICityCodeDetails
    {
        private readonly TableStorageService<CityCodeDbEntity> _tableStorageService;
        private readonly ICacheService _cacheService;

        public CityCodeDetails(ITableStorageService<CityCodeDbEntity> tableStorageService, ICacheService iCacheService)
        {
            _tableStorageService = (TableStorageService<CityCodeDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.CityCodeDBTableName);
            _cacheService = iCacheService ?? throw new ArgumentNullException(nameof(iCacheService));
        }

        public async Task<IEnumerable<CityCodeDbEntity>> GetCityCodeList()
        {
            var cachedData = await _cacheService.GetCacheValue<IEnumerable<CityCodeDbEntity>>(Constants.Application.CityCodeCacheKey);

            if (cachedData == null)
            {
                cachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.CityCodeCacheKey, cachedData, true);
            }

            return cachedData;
        }
    }
}
