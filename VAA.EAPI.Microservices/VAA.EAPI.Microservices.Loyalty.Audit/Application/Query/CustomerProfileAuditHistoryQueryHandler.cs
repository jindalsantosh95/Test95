using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class CustomerProfileAuditHistoryQueryHandler : IRequestHandler<CustomerProfileAuditHistoryQuery, IEnumerable<CustomerProfileAudit>>
    {
        private readonly IMediator _mediator;

        public CustomerProfileAuditHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<CustomerProfileAudit>> Handle(CustomerProfileAuditHistoryQuery request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }

            return await _mediator.Send(new CustomerProfileAuditHistoryRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}