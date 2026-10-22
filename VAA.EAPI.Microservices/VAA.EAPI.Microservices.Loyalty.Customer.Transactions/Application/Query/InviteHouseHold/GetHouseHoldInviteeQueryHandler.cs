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
    public class GetHouseHoldInviteeQueryHandler : TransactionBaseQueryHandler<InviteHouseHoldResponse>, IRequestHandler<GetHouseHoldInviteeQuery, Result<Response<InviteHouseHoldResponse>>>
    {
        public GetHouseHoldInviteeQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<InviteHouseHoldResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<InviteHouseHoldResponse>>> Handle(GetHouseHoldInviteeQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new InviterHouseHoldRepoQuery(((GetHouseHoldInviteeQuery)request).InviteeMemberNumber, Constants.Application.InviteHouseHoldDbQueryforInvitee, Constants.Application.InviteeMemberNumber)
            {
                DocType = request.DocType
            };
        }
    }
}
