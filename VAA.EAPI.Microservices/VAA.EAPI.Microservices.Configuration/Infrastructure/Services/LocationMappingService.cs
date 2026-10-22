using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class LocationMappingService : ILocationMappingService
    {
        private readonly TableStorageService<LocationMapDbEntity> _tableStorageService;

        public LocationMappingService(ITableStorageService<LocationMapDbEntity> tableStorageService)
        {
            _tableStorageService = (TableStorageService<LocationMapDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.PricingLocationDBTableName);
        }

        public async Task<List<LocationMapDbEntity>> GetLocationMapping()
        {
            return await _tableStorageService.RetrieveItemsAsync(null);
        }
    }
}
