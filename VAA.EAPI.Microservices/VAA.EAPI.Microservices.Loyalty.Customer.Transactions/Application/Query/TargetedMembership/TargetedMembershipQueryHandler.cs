using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership
{
    public class TargetedMembershipQuery : TransactionBase, IRequest<Result<Response<TargetedMembershipResponse>>>
    { }

    public class TargetedMembershipQueryHandler : TransactionBaseQueryHandler<TargetedMembershipResponse>, IRequestHandler<TargetedMembershipQuery, Result<Response<TargetedMembershipResponse>>>
    {
        public TargetedMembershipQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<TargetedMembershipResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<TargetedMembershipResponse>>> Handle(TargetedMembershipQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
