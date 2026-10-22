using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.InviteHouseHold
{
    public class GetHouseHoldInviterQueryHandler : TransactionBaseQueryHandler<InviteHouseHoldResponse>, IRequestHandler<GetHouseHoldInviterQuery, Result<Response<InviteHouseHoldResponse>>>
    {
        public GetHouseHoldInviterQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<InviteHouseHoldResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<InviteHouseHoldResponse>>> Handle(GetHouseHoldInviterQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new InviterHouseHoldRepoQuery(((GetHouseHoldInviterQuery)request).InviterMemberNumber, Constants.Application.InviterHouseHoldDbQuery, Constants.Application.InviterMemberNumber)
            {
                DocType = request.DocType
            };
        }
    }
}
