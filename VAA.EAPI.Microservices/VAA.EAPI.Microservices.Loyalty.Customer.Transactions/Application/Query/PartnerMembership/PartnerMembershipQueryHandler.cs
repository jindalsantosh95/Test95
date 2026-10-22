using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership
{
    public class PartnerMembershipQueryHandler : TransactionBaseQueryHandler<PartnerMembershipResponse>, IRequestHandler<PartnerMembershipQuery, Result<Response<PartnerMembershipResponse>>>
    {
        public PartnerMembershipQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<PartnerMembershipResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<PartnerMembershipResponse>>> Handle(PartnerMembershipQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
