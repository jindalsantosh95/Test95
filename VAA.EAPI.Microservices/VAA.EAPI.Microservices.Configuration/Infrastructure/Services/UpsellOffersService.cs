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
    public class UpsellOffersService : IUpsellOffersService
    {
        private readonly TableStorageService<UpsellOffersDbEntity> _tableStorageService;
        private ICacheService _cacheService;

        public UpsellOffersService(ITableStorageService<UpsellOffersDbEntity> tableStorageService, ICacheService iCacheService)
        {
            _tableStorageService = (TableStorageService<UpsellOffersDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.UpsellOffersDBTableName);
            _cacheService = iCacheService;
        }

        public async Task<IEnumerable<UpsellOffersDbEntity>> GetUpsellOffers()
        {
            var CachedData = await _cacheService.GetCacheValue<IEnumerable<UpsellOffersDbEntity>>(Constants.Application.UpsellOffersCacheKey);

            if (CachedData == null)
            {
                CachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.UpsellOffersCacheKey, CachedData, true);
            }
            return CachedData;
        }
    }
}
