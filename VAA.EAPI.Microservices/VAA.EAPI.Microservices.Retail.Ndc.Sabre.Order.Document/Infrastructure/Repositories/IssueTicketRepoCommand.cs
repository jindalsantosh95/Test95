using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Repositories
{
    public class IssueTicketRepoCommand : IRequest<Result<Response<IssueTicketResponse>>>
    {
        public IssueTicketRequest IssueTicketRequest { get; set; }
    }
}
