using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.IssueTicket
{
    public class IssueTicketCommand : IssueTicketRequest, IRequest<Result<Response<IssueTicketResponse>>>
    {

        public IssueTicketRequest IssueTicketRequest { get; set; }

        public IssueTicketCommand(IssueTicketRequest orderCreateRequest)
        {
            IssueTicketRequest = orderCreateRequest;
        }
    }
}
