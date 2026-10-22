using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory
{
    public class AddressHistoryQueryHandler : IRequestHandler<AddressHistoryQuery, IEnumerable<Infrastructure.Entities.AddressHistory>>
    {
        private readonly IMediator _mediator;

        public AddressHistoryQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<Infrastructure.Entities.AddressHistory>> Handle(AddressHistoryQuery request, CancellationToken cancellationToken)
        {
            var v = new AddressHistoryValidator().Validate(request);
            if (!v.IsValid)
                throw new ValidationException(v.Errors);

            return await _mediator.Send(new AddressHistoryRepoQuery(request), cancellationToken);

        }
    }
}

