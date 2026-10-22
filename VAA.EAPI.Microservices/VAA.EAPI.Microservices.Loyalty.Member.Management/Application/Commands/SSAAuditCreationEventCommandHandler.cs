using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.Audit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSAAuditCreationEventCommandHandler : IRequestHandler<SSAAuditCreationEventCommand, Unit>
    {
        private readonly IMediator _mediator;

        public SSAAuditCreationEventCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Unit> Handle(SSAAuditCreationEventCommand request, CancellationToken cancellationToken)
        {
            var cloudEvent = SupplementarySilverAuditEventToCloudEventMapper.CommandToEventMapper((SupplementarySilverAuditEvent)request.LoyaltyGenericAudit);

            await _mediator.Send(new EventRequestInsertCommand
            {
                Id = cloudEvent.Id,
                Type = cloudEvent.Type,
                Data = cloudEvent,
                Subject = cloudEvent.Subject
            });

            try
            {
                await _mediator.Publish(cloudEvent);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }

            return Unit.Value;
        }
    }
}