using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Mapper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services
{
    public class ServiceResponseQueryHandler : IServiceResponseQuery
    {
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorageService;
        public ServiceResponseQueryHandler(ITableStorageService<ServiceResponseDatabaseEntity> tableStorageService)
        {

            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(EAPI.Infrastructure.Constants.Constants.MicroServiceResponseTableName);
        }
        public async Task<ServiceResponse> Handle(ServiceResponseQueryRequest request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.ConnectionString) && !string.IsNullOrWhiteSpace(request.TableName))
            {
                _tableStorageService.ConnectionString = request.ConnectionString;
                _tableStorageService.TableName = request.TableName;
            }
            request.ConnectionString = null;
            request.TableName = null;
            var response = await _tableStorageService.RetrieveAsync(request.EventType, request.CorrelationId);
            if (response == null)
            {
                return new ServiceResponse { Status = ServiceResponseProcessStatus.UpdatePending };
            }
            return MicroServiceResponseMapping.MicroServiceDbToResponseMapper(response);
        }
    }
}
