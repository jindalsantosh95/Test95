using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class SupplementarySilverAuditHistoryQueryHandler : IRequestHandler<SupplementarySilverAuditHistoryQuery, IEnumerable<LoyaltyGenericAudit>>
    {
        private readonly IMediator _mediator;

        public SupplementarySilverAuditHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<LoyaltyGenericAudit>> Handle(SupplementarySilverAuditHistoryQuery request, CancellationToken cancellationToken)
        {
            if (request == null ||
                request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }
            return await _mediator.Send((SupplementarySilverAuditHistoryRepoQuery)request.CustomerId, cancellationToken);
        }
    }
}