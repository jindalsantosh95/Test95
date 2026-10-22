using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class UpsertTicketRepoCommand : IRequest<Result<Unit>>
    {
        public TicketDto TicketDocument { get; set; }
    }
}
