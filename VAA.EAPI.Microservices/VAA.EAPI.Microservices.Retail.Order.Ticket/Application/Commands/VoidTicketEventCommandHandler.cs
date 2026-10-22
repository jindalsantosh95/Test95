using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands
{
    public class VoidTicketEventCommandHandler : IRequestHandler<VoidTicketEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VoidTicketEventCommandHandler> _logger;
        public VoidTicketEventCommandHandler(IMediator mediator, ILogger<VoidTicketEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(VoidTicketEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"VoidTicket Event Subscribe Handler Start for Id: {request.Id}");
                var ticketFeedVoidRequestData = request.Data?.ToString();

                _logger.LogDebug($"TicketFeedHelper.GetPnrTicketDetails - Start");
                var pnrTicketResult = TicketFeedHelper.GetPnrTicketDetails(ticketFeedVoidRequestData);
                _logger.LogDebug($"TicketFeedHelper.GetPnrTicketDetails - End");

                if (pnrTicketResult.IsSuccess)
                {
                    var cloudEvent = TicketVoidCommandToEventMapper.CommandToEventMapper(pnrTicketResult.Value, request.Id);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    _logger.LogDebug($"Store event in event store - End");

                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent, cancellationToken);
                        _logger.LogDebug($"Publish event - End");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Rollback from event store for CorrelationId: {request.Id} due to error in VoidTicketEventCommand handler");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }
                }
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while handling event for CorrelationId: {request.Id} due to error in VoidTicketEventCommand handler");
                throw;
            }
        }
    }
}
