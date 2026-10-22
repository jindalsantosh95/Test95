using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class ItaFareTypeCodeDetails : IitaFareTypeCodeData
    {
        private readonly TableStorageService<ItaFareTypeCodeDbEntity> _tableStorageService;
        private readonly ICacheService _cacheService;

        public ItaFareTypeCodeDetails(ITableStorageService<ItaFareTypeCodeDbEntity> tableStorageService, ICacheService cacheService)
        {
            _tableStorageService = (TableStorageService<ItaFareTypeCodeDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ItaFareTypeCodeDBTableName);
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<ItaFareTypeCodeDbEntity>> GetFareTypeCodeData()
        {
            var cachedData = await _cacheService.GetCacheValue<IEnumerable<ItaFareTypeCodeDbEntity>>(Constants.Application.ItaFareTypeCodeCacheKey);
            
            if (cachedData == null)
            {
                cachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.ItaFareTypeCodeCacheKey, cachedData, true);
            }

            return cachedData;
        }
    }
}
