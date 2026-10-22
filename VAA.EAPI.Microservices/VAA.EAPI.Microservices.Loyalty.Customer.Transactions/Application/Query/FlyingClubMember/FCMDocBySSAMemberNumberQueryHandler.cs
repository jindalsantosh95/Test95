using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember
{

    public class FCMDocBySSAMemberNumberQueryHandler : TransactionBaseQueryHandler<FlyingClubMemberResponse>, IRequestHandler<FCMDocBySSAMemberNumberQuery, Result<Response<FlyingClubMemberResponse>>>
    {
        public FCMDocBySSAMemberNumberQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<FlyingClubMemberResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<FlyingClubMemberResponse>>> Handle(FCMDocBySSAMemberNumberQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new FCMDocBySSANumberRepoQuery(((FCMDocBySSAMemberNumberQuery)request).SSAMemberNumber)
            {
                DocType = request.DocType
            };
        }
    }
}
