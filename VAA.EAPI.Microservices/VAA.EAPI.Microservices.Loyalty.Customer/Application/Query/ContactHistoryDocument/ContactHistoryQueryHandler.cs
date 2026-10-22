using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.ContactHistoryDocument
{
    public class ContactHistoryQueryHandler : IRequestHandler<ContactHistoryQuery, IEnumerable<ContactHistory>>
    {
        private readonly IMediator _mediator;

        public ContactHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<ContactHistory>> Handle(ContactHistoryQuery request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                request.CustomerId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(request.CustomerId));
            }

            return await _mediator.Send(new ContactHistoryRepoQuery(request.CustomerId), cancellationToken);
        }
    }
}