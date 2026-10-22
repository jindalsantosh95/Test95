using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Preference;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class PreferenceAuditHistoryQueryHandler : IRequestHandler<PreferenceAuditHistoryQuery, IEnumerable<PreferenceAudit>>
    {
        private readonly IMediator _mediator;

        public PreferenceAuditHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<PreferenceAudit>> Handle(PreferenceAuditHistoryQuery request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }

            return await _mediator.Send(new PreferenceAuditHistoryRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}