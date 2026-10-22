using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class CurrencyDetails : ICurrencyDetails
    {
        private readonly TableStorageService<CurrencyDbEntity> _tableStorageService;
        public CurrencyDetails(ITableStorageService<CurrencyDbEntity> tableStorageService)
        {
            _tableStorageService = (TableStorageService<CurrencyDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.CurrencyDBTableName);
        }

        public async Task<CurrencyDbEntity> GetCurrencyDetails(string rowKey)
        {
            return await _tableStorageService.RetrieveAsync(Constants.Application.PosCurrency, rowKey);
        }
    }
}
