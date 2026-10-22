using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner
{
    public class BannerQueryHandler : TransactionBaseQueryHandler<BannerResponse>, IRequestHandler<BannerQuery, Result<Response<BannerResponse>>>
    {
        public BannerQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<BannerResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<BannerResponse>>> Handle(BannerQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new BannerRepoQuery(((BannerQuery)request).CustomerId, Constants.Application.BannerDetailsDbQuery)
            {
                DocType = Constants.Application.BannerDocType
            };
        }
    }
}
