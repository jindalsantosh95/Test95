using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class FareFamilyListService : IFareFamilyListService
    {
        private readonly TableStorageService<FareFamilyListDbEntity> _tableStorageService;

        public FareFamilyListService(ITableStorageService<FareFamilyListDbEntity> tableStorageService)
        {
            _tableStorageService = (TableStorageService<FareFamilyListDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.FareFamilyListDBTableName);
        }

        public async Task<IEnumerable<FareFamilyListDbEntity>> GetFareFamily()
        {
            return await _tableStorageService.RetrieveItemsAsync(null);
        }
    }
}
