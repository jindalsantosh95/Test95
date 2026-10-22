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
    public class EventRequestInsertCommand : CloudEvent, IRequest<bool>
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }

    public class EventStoreRequestInsertCommandHandler : IRequestHandler<EventRequestInsertCommand, bool>
    {
        private readonly ITableStorageService<EventStoreDatabaseEntity> _tableStorageService;
        private readonly ILogger<EventStoreRequestInsertCommandHandler> _logger;

        public EventStoreRequestInsertCommandHandler(ITableStorageService<EventStoreDatabaseEntity> tableStorageService, ILogger<EventStoreRequestInsertCommandHandler> logger)
        {
            _tableStorageService = tableStorageService;
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Constants.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Constants.MicroServiceEventStoreTableName);
            _logger = logger;
        }
        public async Task<bool> Handle(EventRequestInsertCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.ConnectionString) && !string.IsNullOrWhiteSpace(request.TableName))
            {
                _tableStorageService.ConnectionString = request.ConnectionString;
                _tableStorageService.TableName = request.TableName;
            }
            request.ConnectionString = null; //Otherwise the paylod will contain connection string
            request.TableName = null;  //Otherwise the paylod will contain TableName           
            _logger.LogInformation($"Add event in Event Store with Event ID :{request.Id}");
            await _tableStorageService.InsertOrMergeAsync(new EventStoreDatabaseEntity { ID = request.Id, PayLoad = Json.Serialize(request), PartitionKey = request.Type, RowKey = request.Id });
            return true;
        }

    }
}

