using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query
{
    public class FetchCustomerDocumentQueryHandler : IRequestHandler<FetchCustomerDocumentQuery, Result<Response<Customers>>>
    {
        private readonly IMediator _mediator;

        public FetchCustomerDocumentQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<Response<Customers>>> Handle(FetchCustomerDocumentQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new CustomerDocumentRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}
