using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.RetrieveTicket
{
    public class RetriveTicketQuery : IRequest<Result<Response<TicketData>>>
    {

        public RetriveTicketRequest RetriveTicketRequest { get; set; }

        public RetriveTicketQuery(RetriveTicketRequest retriveTicketRequest)
        {
            RetriveTicketRequest = retriveTicketRequest;
        }
    }
}

