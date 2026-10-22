using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MembershipChange
{
    public class MembershipChangeQueryHandler : TransactionBaseQueryHandler<MembershipChangeResponse>, IRequestHandler<MembershipChangeQuery, Result<Response<MembershipChangeResponse>>>
    {
        public MembershipChangeQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<MembershipChangeResponse>> logger) : base(mediator, logger)
        { }
        public async Task<Result<Response<MembershipChangeResponse>>> Handle(MembershipChangeQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}

