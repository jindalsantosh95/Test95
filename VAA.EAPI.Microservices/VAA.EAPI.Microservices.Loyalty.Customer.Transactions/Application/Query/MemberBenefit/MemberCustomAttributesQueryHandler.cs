using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit
{
    public class MemberCustomAttributesQueryHandler : TransactionBaseQueryHandler<MemberCustomAttributesResponse>, IRequestHandler<MemberCustomAttributesQuery, Result<Response<MemberCustomAttributesResponse>>>, IMemberCustomAttributesQueryHandler
    {
        public MemberCustomAttributesQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<MemberCustomAttributesResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<MemberCustomAttributesResponse>>> Handle(MemberCustomAttributesQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
