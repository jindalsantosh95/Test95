using CSharpFunctionalExtensions;
using MediatR;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.UpsertTicket
{
    public class UpsertTicketCommand : IRequest<Result<Unit>>
    {
        public string TicketFeedEventData { get; set; }
    }
}