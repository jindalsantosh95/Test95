using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Mapper;

namespace VAA.EAPI.Infrastructure.Repository
{

    public class ServiceResponseQuery : IRequest<ServiceResponse>
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public string CorrelationId { get; set; }
        public string EventType { get; set; }

    }
    public class ServiceResponseQueryHandler : IRequestHandler<ServiceResponseQuery, ServiceResponse>
    {
        private readonly IMediator _mediator;
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorageService;

        public ServiceResponseQueryHandler(IMediator mediator, ITableStorageService<ServiceResponseDatabaseEntity> tableStorageService)
        {
            _mediator = mediator;
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Constants.MicroServiceResponseTableName);
        }

        public async Task<ServiceResponse> Handle(ServiceResponseQuery request, CancellationToken cancellationToken)
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
