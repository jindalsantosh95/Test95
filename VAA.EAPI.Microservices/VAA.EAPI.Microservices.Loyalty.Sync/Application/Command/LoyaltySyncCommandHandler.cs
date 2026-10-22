using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command
{
    public class LoyaltySyncCommandHandler : IRequestHandler<LoyaltySyncEventProcessCommand, Result<Unit>>
    {
        private readonly ILogger<LoyaltySyncCommandHandler> _logger;
        private readonly IMediator _mediator;

        public LoyaltySyncCommandHandler(IMediator mediator, ILogger<LoyaltySyncCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(LoyaltySyncEventProcessCommand request, CancellationToken cancellationToken)
        {
            bool status = false;
            #region OCDB Sync
            _logger.LogInformation("Loyalty Sync Application handler");
            _logger.LogInformation($"CorrelationId: {request.Id} & CustomerId: {request.Data.CustomerId}");
            bool ocdbSyncFlag = await OcdbSync(request, cancellationToken);
            #endregion

            #region Publish Transaction Event 
            if (!ocdbSyncFlag)
            {
                _logger.LogInformation("Send Transactional Event-Start");
                TransactionalEvent eventData = TransactionalEventRequestMapper.CreateEventGridSchemaData(request, ocdbSyncFlag);
                EventGridTopicEvent eventGridEvent = EventGridEventMapper.EventDataMapper(eventData);

                _logger.LogDebug($"Store event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = eventGridEvent.Id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject }, cancellationToken);
                _logger.LogDebug($"Store event in event store - End");

                try
                {
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = eventGridEvent.Id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject }, cancellationToken);
                    status = true;
                    _logger.LogDebug($"Store event in event store - End");

                    _logger.LogInformation($"Send Event Request:");
                    await _mediator.Publish(eventGridEvent);
                    _logger.LogInformation("Send Transactional Event-End");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Rollback from event store due to error: {ex.Message}");
                    if (status)
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = eventGridEvent.Id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject }, cancellationToken);
                }
            }
            #endregion
            return Unit.Value;
        }

        public async Task<bool> OcdbSync(LoyaltySyncEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Data.AddressDetails.Any(x => x.PostalCode.Contains('-')))
                {
                    foreach (var item in request.Data.AddressDetails)
                    {
                        item.PostalCode = item.PostalCode.Replace("-", "");
                    }
                }

                Validations.ValidatePayload.Validate(request.Data);
                Result<bool> result = await _mediator.Send(new LoyaltySyncRepoCommand(request), cancellationToken);

                if (result.Value)
                {
                    _logger.LogInformation("OCDBSync-Successful");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"OCDBSync-Exception-{ex}");
                return false;
            }

            _logger.LogInformation("OCDBSync-Not Successful");
            return false;
        }
    }
}
