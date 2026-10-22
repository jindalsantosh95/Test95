using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class IssueTicketRepoCommand : IRequest<Result<Response<OrderDocument>>>
    {
        public IssueTicketRequest IssueTicketRequest { get; set; }
    }
}
