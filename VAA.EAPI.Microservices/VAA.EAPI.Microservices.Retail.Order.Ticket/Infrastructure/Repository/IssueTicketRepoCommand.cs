using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class IssueTicketRepoCommand : IRequest<IssueTicketResponse>
    {
        public IssueTicketRequest IssueTicketRequest { get; set; }

    }
}
