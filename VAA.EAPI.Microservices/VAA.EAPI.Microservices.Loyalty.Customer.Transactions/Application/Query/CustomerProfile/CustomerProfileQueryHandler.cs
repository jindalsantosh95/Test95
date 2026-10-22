using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile
{
    public class CustomerProfileQueryHandler : TransactionBaseQueryHandler<CustomerProfileDocResponse>, IRequestHandler<CustomerProfileQuery, Result<Response<CustomerProfileDocResponse>>>
    {
        public CustomerProfileQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<CustomerProfileDocResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<CustomerProfileDocResponse>>> Handle(CustomerProfileQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}