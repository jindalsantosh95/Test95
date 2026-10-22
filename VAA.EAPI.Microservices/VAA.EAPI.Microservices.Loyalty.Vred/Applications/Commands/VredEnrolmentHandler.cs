using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands
{
    public class VredEnrolmentHandler : IRequestHandler<CreateCustomerEntities, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VredEnrolmentHandler> _logger;

        public VredEnrolmentHandler(IMediator mediator, ILogger<VredEnrolmentHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateCustomerEntities request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            bool status = false;
            _logger.LogInformation($"CustomerId: {request.CustomerId}");
            _logger.LogInformation($"Vred - MemberEnrolment Start");
            bool vredSyncFlag = await VredSync(request, cancellationToken);
            _logger.LogInformation($"Vred - MemberEnrolment End");


            #region Store in Event Store & Publish Failure Event 
            if (!vredSyncFlag)
            {
                string id = Guid.NewGuid().ToString();
                TransactionalEvent eventData = TransactionalEventRequestMapper.CreateEventGridSchemaData(request, id);
                EventGridTopicEvent eventGridEvent = EventGridEventMapper.EventDataMapper(eventData);

                try
                {
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = eventGridEvent.Id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject }, cancellationToken);
                    status = true;
                    _logger.LogDebug($"Store event in event store - End");

                    _logger.LogInformation("Send Failure Event-Start");
                    await _mediator.Publish(eventGridEvent);
                    _logger.LogInformation("Send Failure Event-End");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Rollback from event store due to error: {ex.Message}");
                    if (status)
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject }, cancellationToken);
                }
            }
            #endregion

            return true;
        }

        public async Task<bool> VredSync(CreateCustomerEntities request, CancellationToken cancellationToken)
        {
            try
            {
                Result<bool> result = await _mediator.Send(new VredRepoQuery(request), cancellationToken);

                if (result.Value)
                {
                    _logger.LogInformation("Vred auto enrolment-Successful");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Vred auto enrolment-Exception-{ex}");
                return false;
            }

            _logger.LogInformation("Vred auto enrolment-Not Successful");
            return false;
        }
    }
}
