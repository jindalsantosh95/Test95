using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.UpsertTicket
{
    public class UpsertTicketCommandHandler : IRequestHandler<UpsertTicketCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpsertTicketCommandHandler> _logger;

        public UpsertTicketCommandHandler(IMediator mediator, ILogger<UpsertTicketCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(UpsertTicketCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ticketParseResp = TicketFeedParsingHelper.GetTicketDocDetails(request.TicketFeedEventData, _logger);

                if (ticketParseResp.IsSuccess)
                {
                    await _mediator.Send(new UpsertTicketRepoCommand { TicketDocument = ticketParseResp.Value }, cancellationToken);
                }
                else
                {
                    _logger.LogWarning($"UpsertTicketCommandHandler Failed in Parsing Ticekt Feed {request.TicketFeedEventData}");
                }
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpsertTicketCommandHandler exceptions {request.TicketFeedEventData}" + ex.Message);
                throw;
            }
        }

    }
}

