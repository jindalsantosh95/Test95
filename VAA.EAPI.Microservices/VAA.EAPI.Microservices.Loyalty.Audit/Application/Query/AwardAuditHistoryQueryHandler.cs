using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Award;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class AwardAuditHistoryQueryHandler : IRequestHandler<AwardAuditHistoryQuery, IEnumerable<AwardAudit>>
    {
        private readonly IMediator _mediator;

        public AwardAuditHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<AwardAudit>> Handle(AwardAuditHistoryQuery request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }

            return await _mediator.Send(new AwardAuditHistoryRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}