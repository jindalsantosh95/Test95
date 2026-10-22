using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;

namespace VAA.EAPI.Infrastructure.Repository
{

    public class ServiceResponseInsertCommand : ServiceResponse, IRequest<bool>
    {

    }
    public class ServiceResponseInsertCommandHandler : IRequestHandler<ServiceResponseInsertCommand, bool>
    {

        private readonly IMediator _mediator;
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorageService;

        public ServiceResponseInsertCommandHandler(IMediator mediator, ITableStorageService<ServiceResponseDatabaseEntity> tableStorageService)
        {
            _mediator = mediator;
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Constants.MicroServiceResponseTableName);

        }

        public async Task<bool> Handle(ServiceResponseInsertCommand request, CancellationToken cancellationToken)
        {
            await _tableStorageService.InsertOrMergeAsync(new ServiceResponseDatabaseEntity { Id = request.Id, Status = Convert.ToString(request.Status), ResponseData = request.JsonResponseData, PartitionKey = request.Type, RowKey = request.Id });
            return true;
        }
    }

}
