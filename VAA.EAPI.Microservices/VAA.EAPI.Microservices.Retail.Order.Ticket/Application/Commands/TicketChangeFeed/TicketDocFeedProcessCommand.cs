using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.TicketChangeFeed
{
    public class TicketDocFeedProcessCommand : IRequest<bool>
    {
        public TicketDto TicketFeedDocument { get; set; }

        public TicketDocFeedProcessCommand(TicketDto ticketFeedDocument)
        {
            TicketFeedDocument = ticketFeedDocument;
        }
    }
}
