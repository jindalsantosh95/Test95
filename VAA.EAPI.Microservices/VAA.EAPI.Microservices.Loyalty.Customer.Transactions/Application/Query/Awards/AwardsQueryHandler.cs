using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards
{
    public class AwardsQueryHandler : TransactionBaseQueryHandler<AwardResponse>, IRequestHandler<AwardsQuery, Result<Response<AwardResponse>>>
    {
        public AwardsQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<AwardResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardResponse>>> Handle(AwardsQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
