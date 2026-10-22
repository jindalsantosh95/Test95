
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services
{
    public class ServiceResponseInsertHandler : IServiceResponseInsert
    {
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorageService;

        public ServiceResponseInsertHandler(ITableStorageService<ServiceResponseDatabaseEntity> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceResponseTableName);
        }
        public async Task<bool> Handle(ServiceResponseInsertRequest request, CancellationToken cancellationToken)
        {
            await _tableStorageService.InsertOrMergeAsync(new ServiceResponseDatabaseEntity { Id = request.Id, Status = Convert.ToString(request.Status), ResponseData = request.JsonResponseData, PartitionKey = request.Type, RowKey = request.Id });
            return true;
        }
    }
}
