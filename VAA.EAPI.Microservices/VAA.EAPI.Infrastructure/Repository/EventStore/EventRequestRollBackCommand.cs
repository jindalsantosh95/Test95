using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;

namespace VAA.EAPI.Infrastructure.Repository.EventStore
{
    public class EventStoreRequestDeleteCommand : CloudEvent, IRequest<bool>
    {

    }
    public class EventStoreRequestDeleteCommandHandler : IRequestHandler<EventStoreRequestDeleteCommand, bool>
    {
        private readonly ITableStorageService<EventStoreDatabaseEntity> _tableStorageService;
        private readonly ILogger<EventStoreRequestDeleteCommandHandler> _logger;

        public EventStoreRequestDeleteCommandHandler(ITableStorageService<EventStoreDatabaseEntity> tableStorageService, ILogger<EventStoreRequestDeleteCommandHandler> logger)
        {
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Constants.MicroServiceEventStoreTableName);
            _logger = logger;

        }

        public async Task<bool> Handle(EventStoreRequestDeleteCommand request, CancellationToken cancellationToken)
        {
            await _tableStorageService.DeleteAsync(new EventStoreDatabaseEntity { ID = request.Id, PayLoad = request.Data != null ? Json.Serialize(request.Data) : null, PartitionKey = request.Type, RowKey = request.Id });
            return true;

        }
    }
}
