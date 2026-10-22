using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember
{
    public class FlyingClubMemberQueryHandler : TransactionBaseQueryHandler<FlyingClubMemberResponse>, IRequestHandler<FlyingClubMemberQuery, Result<Response<FlyingClubMemberResponse>>>
    {
        public FlyingClubMemberQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<FlyingClubMemberResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<FlyingClubMemberResponse>>> Handle(FlyingClubMemberQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
