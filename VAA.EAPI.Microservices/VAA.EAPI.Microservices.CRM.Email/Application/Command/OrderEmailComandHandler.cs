using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public class OrderEmailComandHandler : IRequestHandler<OrderEmailCommand, Unit>
    {
        private readonly IMediator _mediator;

        public OrderEmailComandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Unit> Handle(OrderEmailCommand request, CancellationToken cancellationToken)
        {
            var cloudEventId = AsyncContext.Current.CorrelationId == Guid.Empty ? Guid.NewGuid().ToString() : AsyncContext.Current.CorrelationId.ToString();

            var orderEmailCloudEvent = OrderEmailEventMapper.EventMapper(request, cloudEventId);

            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEventId, Type = orderEmailCloudEvent.Type, Data = orderEmailCloudEvent.Data, Subject = orderEmailCloudEvent.Subject }, cancellationToken);

            try
            {
                var sessionId = await _mediator.Send(new PNRSetupContextRepoQuery() { RecordLocator = request.RecordLocator, AssignPrinter = false }, cancellationToken);

                var emailRequest = new TicketingEmailReceiptQuery() { EmailId = request.Email, SessionId = sessionId };
                var result = await _mediator.Send(emailRequest, cancellationToken);

                await _mediator.Send(new PnrTeardownContextRepoCommand() { SessionId = sessionId, IsOrderEmail = true }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = cloudEventId, Status = ServiceResponseProcessStatus.Error, Type = orderEmailCloudEvent.Type }, cancellationToken);
                throw new Exception($"Error occurred in OrderConfirmEmailCommandHandler while sending email for correlation id {AsyncContext.Current.CorrelationId}: {ex.Message} for channel {string.Join(',', AsyncContext.Current.Channel)}");
            }

            await _mediator.Send(new ServiceResponseInsertCommand { Id = cloudEventId, Status = ServiceResponseProcessStatus.Completed, Type = orderEmailCloudEvent.Type }, cancellationToken);

            return Unit.Value;

        }
    }
}
