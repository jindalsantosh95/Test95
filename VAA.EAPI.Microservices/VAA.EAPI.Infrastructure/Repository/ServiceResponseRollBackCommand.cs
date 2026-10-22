using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;

namespace VAA.EAPI.Infrastructure.Repository
{
    public class ServiceResponseRollBackCommand : ServiceResponse, IRequest<bool>
    {

    }

    public class ServiceResponseRollBackCommandHandler : IRequestHandler<ServiceResponseRollBackCommand, bool>
    {
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorageService;

        public ServiceResponseRollBackCommandHandler(ITableStorageService<ServiceResponseDatabaseEntity> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Constants.MicroServiceResponseTableName);
        }

        public async Task<bool> Handle(ServiceResponseRollBackCommand request, CancellationToken cancellationToken)
        {
            await _tableStorageService.DeleteAsync(new ServiceResponseDatabaseEntity { Id = request.Id, ResponseData = request.JsonResponseData, PartitionKey = request.Type, RowKey = request.Id });
            return true;
        }
    }
}
