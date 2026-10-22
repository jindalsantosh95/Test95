using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.AmexCenturion;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class AmexCenturionAuditHistoryQueryHandler : IRequestHandler<AmexCenturionAuditHistoryQuery, IEnumerable<CardLinkingAudit>>
    {
        private readonly IMediator _mediator;

        public AmexCenturionAuditHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<CardLinkingAudit>> Handle(AmexCenturionAuditHistoryQuery request, CancellationToken cancellationToken)
        {
            if (request == null || request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }

            return await _mediator.Send(new AmexCenturionAuditHistoryRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}

