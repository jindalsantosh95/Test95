using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Activity
{
    public class ActivitiesQueryHandler : TransactionBaseQueryHandler<ActivitiesResponse>, IRequestHandler<ActivitiesQuery, Result<Response<ActivitiesResponse>>>
    {
        public ActivitiesQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<ActivitiesResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<ActivitiesResponse>>> Handle(ActivitiesQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
