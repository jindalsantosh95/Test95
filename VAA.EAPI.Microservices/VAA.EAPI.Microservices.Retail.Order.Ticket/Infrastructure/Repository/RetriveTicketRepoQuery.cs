using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class RetriveTicketRepoQuery : IRequest<TicketData>
    {
        public RetriveTicketRequest RetriveTicketRequest { get; set; }

        public RetriveTicketRepoQuery(RetriveTicketRequest retriveTicketRepoQuery)
        {
            RetriveTicketRequest = retriveTicketRepoQuery;
        }
    }
}
