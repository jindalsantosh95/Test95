using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Application.Commands
{
    public class IssueTicketCommand : IRequest<Result<Response<IssueTicketResponse>>>
    {
        public IssueTicketRequest IssueTicketRequest { get; set; }

        public static explicit operator IssueTicketCommand(IssueTicketRequest issueTicketRequest) => new IssueTicketCommand { IssueTicketRequest = issueTicketRequest };
    }
}