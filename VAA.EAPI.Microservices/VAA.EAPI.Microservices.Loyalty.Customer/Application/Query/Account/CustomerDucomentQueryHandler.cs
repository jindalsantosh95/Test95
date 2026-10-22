using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account
{
    public class CustomerDucomentQueryHandler : IRequestHandler<CustomerDucomentQuery, Result<Response<Customers>>>
    {
        private readonly IMediator _mediator;

        public CustomerDucomentQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<Customers>>> Handle(CustomerDucomentQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.MemberNumber))
                throw new ArgumentNullException(nameof(request.MemberNumber));

            var customerSearchResponse = await _mediator.Send(new CustomerSearchDocRepoQuery(request.MemberNumber), cancellationToken);

            return await _mediator.Send(new CustomerDocumentRepoQuery(customerSearchResponse.CustomerId), cancellationToken);
        }
    }
}
